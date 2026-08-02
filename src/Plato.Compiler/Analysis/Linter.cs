using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Parakeet;

namespace Ara3D.Geometry.Compiler.Analysis
{
    /// <summary>
    /// How much a finding matters. Only <see cref="LintSeverity.Error"/> gates `lint --strict`:
    /// a finding is an Error when the declaration it names is unambiguously wrong (silently
    /// dropped, silently ambiguous, or a rule violation), a Warning when it names an incomplete
    /// but legal declaration, and Info when it only reports on the shape of the vocabulary.
    /// </summary>
    public enum LintSeverity
    {
        Info,
        Warning,
        Error,
    }

    /// <summary>
    /// A single lint finding. Formats as "file(line): LINT###: message" - the severity is
    /// deliberately NOT part of ToString(), so downstream parsers of the line format keep working.
    /// </summary>
    public class LintFinding
    {
        public string File { get; }
        public int Line { get; } // 1-based; 0 = unknown
        public string Code { get; }
        public LintSeverity Severity { get; }
        public string Message { get; }

        public LintFinding(string file, int line, string code, LintSeverity severity, string message)
        {
            File = file;
            Line = line;
            Code = code;
            Severity = severity;
            Message = message;
        }

        public override string ToString()
            => $"{File}({Line}): {Code}: {Message}";
    }

    /// <summary>
    /// A post-compilation lint pass (docs/plato-roadmap.md P1.6, plato-library-review.md section 2.1).
    /// Runs structural checks over the symbol tables of a completed Compilation:
    ///   LINT001 - interface obligations with no implementation (generated code throws NotImplementedException)
    ///   LINT002 - 'where' clauses constraining undeclared type variables
    ///   LINT003 - declared-but-unused type fields
    ///   LINT004 - duplicate function signatures within a library
    ///   LINT005 - generic type variables used only once, or used in a return type but not inferable from parameters
    ///   LINT006 - unique (affine) builder type used as a field type (builders may not be stored in fields)
    ///   LINT007 - unique (affine) builder type used as a generic type argument (no containers of builders)
    ///   LINT008 - interface with no concrete implementer
    ///   LINT009 - interface that nothing in the compilation mentions at all
    ///   LINT010 - concrete type that implements no interface and that no function or field mentions
    ///   LINT011 - redundant 'implements' clause (already implied by another implemented interface)
    ///   LINT012 - obligation and implementation disagree on the '_' receiver marker (static vs instance)
    ///   LINT013 - interface with no implementer that library bodies nonetheless dispatch on (unreachable derived surface)
    ///   LINT014 - a name that is BOTH a struct field and a no-arg library function on an unrelated receiver
    ///             (the C# writer's uniform rendering rule decides property-vs-method call syntax by NAME)
    ///   LINT015 - a type declaration with fields whose NAME is a writer primitive; the writer emits the
    ///             runtime type for that name and silently discards the declared shape
    ///   LINT016 - redundant 'inherits' clause on an interface (already implied by another inherited interface)
    /// The pass is read-only: it never mutates the compilation and has no effect on code generation.
    /// </summary>
    public class Linter
    {
        public Compilation Compilation { get; }
        public List<LintFinding> Findings { get; } = new List<LintFinding>();

        public Linter(Compilation compilation)
        {
            Compilation = compilation;
            CheckUnimplementedInterfaceObligations();
            CheckUndeclaredConstraintVariables();
            CheckUnusedFields();
            CheckDuplicateLibrarySignatures();
            CheckGenericTypeVariableUsage();
            CheckUniqueTypeStructuralBans();
            CheckVocabularyReachability();
            CheckRedundantImplements();
            CheckRedundantInherits();
            CheckReceiverMarkerAgreement();
            CheckFieldVersusNoArgFunctionNames();
            CheckPrimitiveNameShadowing();
        }

        public IEnumerable<LintFinding> SortedFindings
            => Findings.OrderBy(f => f.File).ThenBy(f => f.Line).ThenBy(f => f.Code);

        public int ErrorCount
            => Findings.Count(f => f.Severity == LintSeverity.Error);

        public int WarningCount
            => Findings.Count(f => f.Severity == LintSeverity.Warning);

        public int InfoCount
            => Findings.Count(f => f.Severity == LintSeverity.Info);

        /// <summary>
        /// Findings that matter for burn-down / ratchet tracking: Errors and Warnings.
        /// Info findings describe vocabulary shape and are excluded.
        /// </summary>
        public int RatchetCount
            => ErrorCount + WarningCount;

        private readonly HashSet<string> _seen = new HashSet<string>();

        public void Add(ILocation location, string code, LintSeverity severity, string message)
        {
            var range = location?.GetRange();
            var file = range?.FilePath.ToString() ?? "<unknown>";
            var line = range != null ? range.BeginLineIndex + 1 : 0;
            if (_seen.Add($"{file}|{line}|{code}|{message}"))
                Findings.Add(new LintFinding(file, line, code, severity, message));
        }

        public void Add(Symbol symbol, string code, LintSeverity severity, string message)
            => Add(GetLocation(symbol), code, severity, message);

        public ILocation GetLocation(Symbol symbol)
        {
            if (symbol == null)
                return null;
            var node = Compilation.GetAstNode(symbol);
            if (node != null)
                return node;
            // Library/interface functions are not mapped directly; their owning MethodDef is.
            if (symbol is FunctionDef fd && fd.OwnerType != null)
            {
                var method = fd.OwnerType.Methods.FirstOrDefault(m => m.Function == fd);
                if (method != null)
                    return Compilation.GetAstNode(method);
            }
            return null;
        }

        // -------------------------------------------------------------------
        // LINT001: For each concrete type T implementing interface I, an I member
        // with no matching library function/field. This mirrors exactly what the
        // C# writer emits as `throw new NotImplementedException()` (a declared
        // interface function whose chosen implementation has no body), applying
        // the same skips the writer applies (CSharpConcreteTypeWriter.SkipFunction
        // and the At/Count special cases which are generated from the fields).
        // -------------------------------------------------------------------

        // Kept in sync with Plato.CSharpWriter CSharpWriter.IgnoredFunctions:
        // members the writer implements by other means (or never emits), so an
        // "unimplemented" entry for them does not produce a throwing member.
        public static readonly HashSet<string> MembersImplementedByWriter = new HashSet<string>
        {
            "FieldNames", "FieldValues", "TypeName",
            "Equals", "NotEquals", "GetHashCode", "ToString", "GetType",
            "Components", "CreateFromComponents", "CreateFromComponent", "NumComponents",
            // "Range" and "MakeArray2D" left 2026-08-01 with their CSharpWriter.IgnoredFunctions
            // entries: both have Plato bodies in the forward stdlib now.
            "MapRange",
            // Synthesized by the writer (CSharpTypeWriter.GenerateFunc) from the type's shape:
            // from the FIELDS for a fixed-arity type (Point3D: X/Y/Z), or by delegating to the
            // single field that is itself indexable for a runtime-arity type (VectorN: Components).
            // A type whose shape fits neither must declare an explicit At/Count library body — that
            // lands in ImplementedFunctions and never reaches the synthesis (e.g. RegularPolygon).
            "At", "Count",
            // The serialization surface every generated struct carries (CSharpSerializationWriter):
            // JSON in and out, plus the System.IFormattable / ISpanFormattable / IParsable /
            // ISpanParsable members that discharge those interfaces.
            "ToJson", "FromJson", "AppendJson", "TryFormat", "Parse", "TryParse",
        };

        public void CheckUnimplementedInterfaceObligations()
        {
            foreach (var ct in Compilation.ConcreteTypes)
            {
                var fieldNames = ct.TypeDef.Fields.Select(f => f.Name).ToHashSet();
                foreach (var fi in ct.UnimplementedFunctions)
                {
                    // A declared interface function with a default body is emitted
                    // as that body; only body-less declarations become throws.
                    if (fi.Implementation.Body != null)
                        continue;
                    if (MembersImplementedByWriter.Contains(fi.Name))
                        continue;
                    if (fieldNames.Contains(fi.Name))
                        continue; // fulfilled by a field
                    if (fi.Name == ct.Name)
                        continue; // cast-to-self

                    var declaringInterface = fi.Implementation.OwnerType?.Name ?? fi.InterfaceName;
                    Add(GetLocation(ct.TypeDef) ?? GetLocation(fi.Implementation), "LINT001", LintSeverity.Warning,
                        $"type '{ct.Name}' implements '{declaringInterface}' but no implementation was found for " +
                        $"'{fi.SignatureId}'; the generated member will throw NotImplementedException");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT012: an obligation and the implementation that discharges it disagree
        // on the '_' receiver marker.
        //
        // Naming a function's first parameter '_' is how Plato says "I need the
        // receiver's TYPE to dispatch, but not its value" — the constructor-shaped
        // idiom (Zero(_: Color), Pi(_: Number), FromAmount(_: Self, x)). It is not
        // decoration: the C# writer keys off it directly
        // (CSharpFunctionInfo.IsStatic => ParameterNames[0] == "_") and emits a
        // static member for '_' and an instance member otherwise.
        //
        // An interface declaring Zero(x: Self) with an implementation written
        // Zero(_: Color) is LEGAL since the type-token-members increment: the C#
        // writer emits the pair `public static Color Zero()` + explicit interface
        // implementation `Color Additive<Color>.Zero() => Zero();`, so the static
        // fill satisfies the instance obligation and the member is reachable both
        // as Color.Zero() and off a value. That direction is no longer flagged.
        //
        // The REVERSE disagreement is still always a bug: when the obligation
        // marks the receiver '_' (type-level; `static abstract` in C# under
        // --static-abstract) an instance implementation cannot satisfy it, and
        // some implementors genuinely need the receiver (VectorN-style types read
        // their arity from it), which means the obligation itself is wrong.
        //
        // Nothing else in the language looks at a parameter NAME, so this was not
        // caught until C# compilation, more than a thousand generated files
        // downstream. This rule is the gate: it puts the disagreement at the
        // implementation, at lint time, in any backend. See plato-312 and the
        // 2026-07-29 static-interface-members ADR.
        // -------------------------------------------------------------------

        /// <summary>Plato's marker for "receiver type matters, receiver value does not".</summary>
        public static bool HasIgnoredReceiver(FunctionInstance f)
            => f.ParameterNames.Count > 0 && f.ParameterNames[0] == "_";

        public void CheckReceiverMarkerAgreement()
        {
            foreach (var ct in Compilation.ConcreteTypes)
            {
                foreach (var declared in ct.DeclaredFunctions)
                {
                    if (MembersImplementedByWriter.Contains(declared.Name))
                        continue; // synthesized by the writer; no authored receiver to compare
                    // Same pairing the writer and LINT001 use (ConcreteType.ImplementationFor).
                    var impl = ct.ImplementationFor(declared);
                    if (impl == null)
                        continue; // unimplemented is LINT001's business, not this rule's
                    if (declared.ParameterNames.Count == 0 || impl.ParameterNames.Count == 0)
                        continue;

                    var obligationIsTypeLevel = HasIgnoredReceiver(declared);
                    if (obligationIsTypeLevel == HasIgnoredReceiver(impl))
                        continue;

                    // Instance obligation discharged by a '_' fill: legal — the writer emits the
                    // static + an explicit interface implementation forwarding to it (both the
                    // Type.Member and value.Member surfaces work). Only the reverse is a defect.
                    if (!obligationIsTypeLevel)
                        continue;

                    var owner = declared.Implementation.OwnerType?.Name ?? declared.InterfaceName;

                    // Anchored at the IMPLEMENTATION: that is the edit site, and it collapses one
                    // finding per concrete type down to one per offending body.
                    Add(GetLocation(impl.Implementation) ?? GetLocation(ct.TypeDef), "LINT012", LintSeverity.Warning,
                        $"'{impl.SignatureId}': the '{owner}' obligation marks the receiver '_' (type-level, " +
                        $"emitted static) but this implementation names it '{impl.ParameterNames[0]}' (emitted " +
                        $"as an instance member, which cannot satisfy it). Name the receiver in the obligation " +
                        $"if any implementor needs the value, or mark this implementation's receiver '_'");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT002: 'where' clauses referencing type variables that are not
        // declared type parameters of the interface (dead constraints; the
        // symbol resolver silently drops them).
        // -------------------------------------------------------------------
        public void CheckUndeclaredConstraintVariables()
        {
            foreach (var td in Compilation.TypeDeclarations)
            {
                var typeParameters = td.TypeParameters.Select(tp => tp.Name.Text).ToHashSet();
                foreach (var c in td.Constraints)
                {
                    if (!typeParameters.Contains(c.Name.Text))
                    {
                        var declaredList = typeParameters.Count == 0 ? "none" : string.Join(", ", typeParameters);
                        Add(c, "LINT002", LintSeverity.Error,
                            $"'where' clause of '{td.Name.Text}' constrains '{c.Name.Text}' which is not a declared " +
                            $"type parameter (declared: {declaredList}); the constraint is silently ignored");
                    }
                }

                // The same rule for a FUNCTION's own `where` clause (plato-393): its target must be
                // a type variable the signature actually mentions, or the enclosing declaration's
                // parameter. A bound on a name that appears nowhere constrains nothing, and the
                // symbol layer would drop it in silence — exactly the trap this rule exists for.
                foreach (var md in td.Members.OfType<AstMethodDeclaration>())
                {
                    if (md.Constraints.Count == 0)
                        continue;
                    var inScope = new HashSet<string>(typeParameters);
                    foreach (var t in md.Parameters.Select(p => p.Type).Append(md.Type))
                        CollectTypeVariableNames(t, inScope);
                    foreach (var c in md.Constraints)
                    {
                        if (inScope.Contains(c.Name.Text))
                            continue;
                        var inScopeList = inScope.Count == 0 ? "none" : string.Join(", ", inScope);
                        Add(c, "LINT002", LintSeverity.Error,
                            $"'where' clause of '{td.Name.Text}.{md.Name.Text}' constrains '{c.Name.Text}' which its " +
                            $"signature does not mention (in scope: {inScopeList}); the constraint is silently ignored");
                    }
                }
            }
        }

        /// <summary>Every name a type expression writes, so a function's `where` target can be
        /// checked against the signature that is supposed to introduce it.</summary>
        private static void CollectTypeVariableNames(AstTypeNode t, HashSet<string> into)
        {
            if (t == null)
                return;
            into.Add(t.Name.Text);
            foreach (var a in t.TypeArguments)
                CollectTypeVariableNames(a, into);
        }

        // -------------------------------------------------------------------
        // LINT003: Declared-but-unused type fields. A field F of concrete type T
        // is considered used if:
        //   * any function that can see a T (a library function with a parameter
        //     of type T or of an interface T implements, or a method of an
        //     interface T implements) references the name F in its body, or
        //   * F fulfills a declared member of an interface T implements
        //     (field-backed interface implementation).
        // Name-based within a type-directed candidate set: precise enough to
        // catch real bugs (Lissajous.A, LookAt3D.Origin) without a full type
        // checker; may rarely under-report if an unrelated same-named field is
        // read by a candidate function.
        // -------------------------------------------------------------------
        public void CheckUnusedFields()
        {
            // Precompute referenced-name sets per function.
            var referencedNames = new Dictionary<FunctionDef, HashSet<string>>();

            HashSet<string> GetReferencedNames(FunctionDef fd)
            {
                if (referencedNames.TryGetValue(fd, out var r))
                    return r;
                r = fd.Body == null
                    ? new HashSet<string>()
                    : fd.Body.GetSymbolTree()
                        .OfType<RefSymbol>()
                        .Where(s => !(s is ParameterOrVariableRefSymbol))
                        .Select(s => s.Def?.Name ?? (s as KeywordRefSymbol)?.Name)
                        .Where(n => n != null)
                        .ToHashSet();
                referencedNames.Add(fd, r);
                return r;
            }

            var libraryFunctions = Compilation.LibraryDefinitionsByName.Values
                .SelectMany(l => l.Functions)
                .Where(f => f.Body != null)
                .ToList();

            foreach (var td in Compilation.TypeDefinitions)
            {
                if (td.Kind != TypeKind.ConcreteType || td.Fields.Count == 0)
                    continue;

                var implementedConcepts = td.GetAllImplementedConcepts()
                    .Where(c => c?.Def != null)
                    .ToList();

                // Interface members a field can fulfill by name.
                var interfaceMemberNames = implementedConcepts
                    .SelectMany(c => c.Def.Methods.Select(m => m.Name))
                    .ToHashSet();

                // Functions that can see a value of type td:
                //  * library functions with a parameter of type td or an interface td implements
                //  * methods (with default bodies) of interfaces td implements
                //  * compiler-generated functions of td itself
                var candidates = libraryFunctions
                    .Where(f => f.Parameters.Any(p =>
                        p.Type?.Def != null &&
                        (p.Type.Def == td ||
                         (p.Type.Def.IsInterface() && td.Implements(p.Type)))))
                    .Concat(implementedConcepts.SelectMany(c => c.Def.Functions).Where(f => f.Body != null))
                    .Concat(td.CompilerGeneratedFunctions.Where(f => f.Body != null));

                var used = new HashSet<string>();
                foreach (var f in candidates)
                    used.UnionWith(GetReferencedNames(f));

                foreach (var field in td.Fields)
                {
                    if (used.Contains(field.Name))
                        continue;
                    if (interfaceMemberNames.Contains(field.Name))
                        continue;
                    Add(field, "LINT003", LintSeverity.Info,
                        $"field '{td.Name}.{field.Name}' is never read by any library function, " +
                        $"interface implementation, or generated member");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT004: Duplicate function signatures within a library (same name and
        // parameter types; return type ignored since overloading on return type
        // alone is also ambiguous).
        // -------------------------------------------------------------------
        public void CheckDuplicateLibrarySignatures()
        {
            foreach (var lib in Compilation.LibraryDefinitionsByName.Values)
            {
                var groups = lib.Functions
                    .GroupBy(f => $"{f.Name}({string.Join(",", f.Parameters.Select(p => p.Type?.ToString() ?? "?"))})")
                    .Where(g => g.Count() > 1);

                foreach (var g in groups)
                {
                    // Report every occurrence after the first.
                    foreach (var f in g.Skip(1))
                    {
                        Add(f, "LINT004", LintSeverity.Error,
                            $"library '{lib.Name}' declares {g.Count()} functions with signature '{g.Key}'; " +
                            $"duplicate definitions are ambiguous (one silently wins)");
                    }
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT005: Generic type-variable consistency in library and interface
        // function signatures:
        //   (a) a type variable used in the return type but in no parameter can
        //       never be inferred;
        //   (b) a type variable that occurs exactly once in the signature, inside
        //       a function-typed parameter (Function0..N), is a lambda result
        //       that the rest of the signature ignores - almost always a
        //       declared/intended return type mismatch (e.g. intrinsics.plato
        //       WithNext takes Function2<$T,$T,$TR> but returns IArray<$T>,
        //       orphaning $TR).
        // A single-use type variable in an ordinary (non-function) parameter slot
        // (e.g. Contains(b: IBounds<$T,$D>, v: $T)) is structural and not flagged.
        // -------------------------------------------------------------------
        public void CheckGenericTypeVariableUsage()
        {
            foreach (var lib in Compilation.LibraryDefinitionsByName.Values)
            {
                foreach (var f in lib.Functions)
                {
                    var paramCounts = new Dictionary<string, int>();
                    var inFunctionType = new HashSet<string>();
                    foreach (var p in f.Parameters)
                        CountTypeVariables(p.Type, paramCounts, false, inFunctionType);

                    var returnCounts = new Dictionary<string, int>();
                    CountTypeVariables(f.ReturnType, returnCounts, false, null);

                    foreach (var tv in returnCounts.Keys.Where(tv => !paramCounts.ContainsKey(tv)))
                    {
                        Add(f, "LINT005", LintSeverity.Error,
                            $"function '{f.Name}' in library '{lib.Name}' uses type variable '{tv}' in its return " +
                            $"type '{f.ReturnType}' but in no parameter; it can never be inferred at a call site");
                    }

                    foreach (var tv in paramCounts.Keys)
                    {
                        var total = paramCounts[tv] + (returnCounts.TryGetValue(tv, out var rc) ? rc : 0);
                        if (total == 1 && inFunctionType.Contains(tv))
                        {
                            // Heuristic ("likely inconsistent"), so a Warning rather than an Error.
                            Add(f, "LINT005", LintSeverity.Warning,
                                $"function '{f.Name}' in library '{lib.Name}' takes a function argument producing " +
                                $"'{tv}' but uses '{tv}' nowhere else; the declared return type '{f.ReturnType}' is " +
                                $"likely inconsistent with the type variables used");
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT006/LINT007: the two cheap structural bans of the affine-type
        // conventions (roadmap Phase 6, docs/affine-types.md). Unique builder
        // types (unique type List<T> / Buffer<T>) may not be stored in fields
        // (LINT006) and may not appear as a generic type argument of another
        // type (LINT007: no IArray<List<T>>, no List<List<T>>). These are the
        // structurally detectable subset of the affine rules; the occurrence-
        // counting pass rides the future type-checker workstream.
        // -------------------------------------------------------------------
        public void CheckUniqueTypeStructuralBans()
        {
            bool IsUnique(TypeExpression te)
                => te?.Def != null && te.Def.IsUnique;

            bool ContainsUniqueArg(TypeExpression te)
                => te != null && te.TypeArgs.Any(a => IsUnique(a) || ContainsUniqueArg(a));

            foreach (var td in Compilation.TypeDefinitions)
            {
                foreach (var field in td.Fields)
                {
                    if (IsUnique(field.Type))
                        Add(field, "LINT006", LintSeverity.Error,
                            $"field '{td.Name}.{field.Name}' has unique type '{field.Type}'; " +
                            $"builders may not be stored in fields (affine rule, docs/affine-types.md)");
                    else if (ContainsUniqueArg(field.Type))
                        Add(field, "LINT007", LintSeverity.Error,
                            $"field '{td.Name}.{field.Name}' uses a unique type as a generic type argument " +
                            $"in '{field.Type}'; containers of builders are banned (affine rule, docs/affine-types.md)");
                }
            }

            foreach (var lib in Compilation.LibraryDefinitionsByName.Values)
            {
                foreach (var f in lib.Functions)
                {
                    foreach (var p in f.Parameters)
                        if (ContainsUniqueArg(p.Type))
                            Add(f, "LINT007", LintSeverity.Error,
                                $"parameter '{p.Name}' of function '{f.Name}' in library '{lib.Name}' uses a unique " +
                                $"type as a generic type argument in '{p.Type}'; containers of builders are banned " +
                                $"(affine rule, docs/affine-types.md)");
                    if (ContainsUniqueArg(f.ReturnType))
                        Add(f, "LINT007", LintSeverity.Error,
                            $"function '{f.Name}' in library '{lib.Name}' uses a unique type as a generic type " +
                            $"argument in its return type '{f.ReturnType}'; containers of builders are banned " +
                            $"(affine rule, docs/affine-types.md)");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT008/LINT009/LINT010: reachability of the vocabulary. In an interface-
        // oriented library a declaration earns its place by being implemented or
        // used; these three rules report the declarations that are neither.
        //   LINT008 - an interface no concrete type implements. Either dead
        //             vocabulary or a forgotten 'implements' line. Implementer
        //             lookup is transitive over interface inheritance, so an
        //             abstract intermediate interface counts as implemented as
        //             soon as any descendant interface has an implementer.
        //   LINT009 - an interface that nothing mentions at all: not implemented,
        //             not inherited, never a parameter/return type, never a
        //             'where' bound. Strictly stronger than LINT008, and
        //             reported instead of it.
        //   LINT010 - a concrete type that implements no interface AND that no
        //             other declaration mentions (no function signature, no
        //             field type). An unfinished declaration.
        // All three are Info: on a declarations-first folder they describe the
        // shape of the vocabulary rather than a defect.
        // -------------------------------------------------------------------
        // The number of library functions that dispatch on an interface: functions whose
        // FIRST parameter is the interface, which is the shape every derived body takes
        // (LIBRARIES.md rule 2). Counting only that position keeps the rule about
        // unreachable derived surface rather than about incidental mentions.
        private int DispatchedLibraryBodyCount(TypeDef iface)
            => Compilation.LibraryDefinitionsByName.Values
                .SelectMany(lib => lib.Functions)
                .Count(f => f.Parameters.Count > 0 && f.Parameters[0].Type?.Def == iface);

        public void CheckVocabularyReachability()
        {
            var mentioned = new HashSet<TypeDef>();     // any use outside the owning declaration
            var inherited = new HashSet<TypeDef>();     // target of an implements/inherits clause

            void Mention(TypeExpression te, TypeDef owner)
            {
                if (te?.Def == null)
                    return;
                if (te.Def != owner)
                    mentioned.Add(te.Def);
                foreach (var ta in te.TypeArgs)
                    Mention(ta, owner);
            }

            foreach (var f in Compilation.FunctionDefinitions)
                foreach (var te in f.ParametersAndReturnType)
                    Mention(te, f.OwnerType);

            foreach (var td in Compilation.AllTypeAndLibraryDefinitions)
            {
                foreach (var te in td.Implements.Concat(td.Inherits))
                {
                    Mention(te, td);
                    if (te?.Def != null)
                        inherited.Add(te.Def);
                }
            }

            // 'where' bounds now reach the symbols too (plato-382: TypeParameterDef.Constraints is
            // populated, and Checking/TypeConstraintChecker verifies them), but they are still read
            // off the AST here: reachability is a SYNTACTIC question — a bound written in the source
            // makes the interface reachable whether or not it resolved.
            var boundNames = Compilation.TypeDeclarations
                .SelectMany(d => d.Constraints
                    .Concat(d.Members.OfType<AstMethodDeclaration>().SelectMany(m => m.Constraints)))
                .Select(c => c.Constraint?.Name?.Text)
                .Where(n => n != null)
                .ToHashSet();

            foreach (var c in Compilation.GetConcepts())
            {
                var used = mentioned.Contains(c) || boundNames.Contains(c.Name);
                var implementers = Compilation.GetImplementers(c.ToTypeExpression()).ToList();
                if (implementers.Count > 0)
                    continue;
                if (!used)
                    Add(c, "LINT009", LintSeverity.Info,
                        $"interface '{c.Name}' is unreachable: no concrete type implements it, no interface " +
                        $"inherits it, and it appears in no parameter type, return type or 'where' bound");
                else
                {
                    // LINT013 takes precedence over LINT008: an implementer-less interface that
                    // library bodies dispatch on is not "vocabulary declared ahead of use", it
                    // is derived API no caller can ever reach.
                    var bodies = DispatchedLibraryBodyCount(c);
                    if (bodies > 0)
                        Add(c, "LINT013", LintSeverity.Warning,
                            $"interface '{c.Name}' has no concrete implementer, yet {bodies} library " +
                            $"function(s) dispatch on it; that derived surface is unreachable. " +
                            $"Implement the interface, or delete it together with those bodies");
                    else
                        Add(c, "LINT008", LintSeverity.Info,
                            $"interface '{c.Name}' has no concrete implementer" +
                            (inherited.Contains(c) ? " (directly or through a descendant interface)" : "") +
                            "; it is either dead vocabulary or a missing 'implements' clause");
                }
            }

            foreach (var td in Compilation.GetConcreteTypes())
            {
                if (td.IsPrimitive() || td.IsUnique)
                    continue; // intrinsic; backed by the runtime, not by declarations
                if (td.Implements.Count > 0 || mentioned.Contains(td))
                    continue;
                Add(td, "LINT010", LintSeverity.Info,
                    $"concrete type '{td.Name}' implements no interface and is mentioned by no function " +
                    $"signature or field; it participates in nothing");
            }
        }

        // -------------------------------------------------------------------
        // LINT011: an 'implements' clause already implied by another one on the
        // same type (Sphere implements ParametricSurface and Geometry3D, where
        // ParametricSurface already reaches Geometry3D). Legal, and sometimes
        // written deliberately to restate the top-level category - hence Info -
        // but it hides the real interface lattice and goes stale when the lattice
        // is refactored.
        // -------------------------------------------------------------------
        public void CheckRedundantImplements()
        {
            foreach (var td in Compilation.GetConcreteTypes())
            {
                if (td.Implements.Count < 2)
                    continue;
                foreach (var te in td.Implements)
                {
                    if (te?.Def == null)
                        continue;
                    var implier = td.Implements.FirstOrDefault(other =>
                        other?.Def != null && other.Def != te.Def &&
                        other.Def.GetAllImplementedConcepts().Any(i => i.Def == te.Def));
                    if (implier == null)
                        continue;
                    Add(GetLocation(te) ?? GetLocation(td), "LINT011", LintSeverity.Info,
                        $"type '{td.Name}' implements '{te.Def.Name}' redundantly; " +
                        $"'{implier.Def.Name}' already implies it");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT016: same predicate as LINT011, for interface-to-interface 'inherits'
        // (Orderable inherits Equatable and Comparable, where Comparable already
        // reaches Equatable). Shared detection lives in ConceptHierarchy.
        // -------------------------------------------------------------------
        public void CheckRedundantInherits()
        {
            foreach (var r in ConceptHierarchy.RedundantInherits(Compilation))
            {
                Add(GetLocation(r.Parent) ?? GetLocation(r.Interface), "LINT016", LintSeverity.Info,
                    $"interface '{r.Interface.Name}' inherits '{r.Parent.Def.Name}' redundantly; " +
                    $"'{r.ImpliedBy.Def.Name}' already implies it");
            }
        }

        // -------------------------------------------------------------------
        // LINT014: a name that is simultaneously a FIELD of some concrete type and a
        // NO-ARG LIBRARY FUNCTION on an unrelated receiver.
        //
        // The C# writer decides no-arg call-site syntax by the struct surface: a no-arg
        // member renders with property/field syntax iff its name is on the surface of the
        // RECEIVER'S OWN type (CSharpWriter.IsStructSurfaceProperty). That rendering rule
        // used to be global and keyed by NAME only, so a field on one struct decided how a
        // same-named member rendered on EVERY receiver — including receivers with no such
        // field, whose member is a classic extension METHOD needing "()". Two measured
        // instances in the forward stdlib: the field `Histogram.Range` stole the parentheses
        // from `ArrayExtensions.Range(this int)` (915 x CS0119), and a field named `Amount`
        // stole them from `Amount(x: Angle)` and its 60 sibling quantity projections
        // (110 x CS0030). Both are fixed: plato-323 cluster 1 made the rule receiver-aware
        // for erased scalar receivers, and item 2 generalized it to every receiver.
        //
        // So the RENDERING hazard is gone. What the collision still costs is MEMBER
        // PLACEMENT, which cannot be decided per receiver: a C# instance member silently
        // hides a same-name extension method, so a name that is a field anywhere is demoted
        // out of the library extension class and back into the struct on EVERY type
        // (ExtensionStylePlan.DemoteMovedNames). That is a real, if quieter, consequence —
        // it inflates every generated struct and couples unrelated types through a name —
        // and it is still invisible in the Plato source, which is why the lint stays.
        //
        // The finding count therefore no longer pre-measures a withheld writer fix (that
        // fix landed); it measures how much of the vocabulary is coupled by name collision.
        //
        // The predicate deliberately excludes the two shapes that are DESIGN, not defect:
        //   * same-type (or interface-satisfying) field forwarding. A field named N on T is
        //     exactly how T discharges an obligation N declared by an interface T implements,
        //     and a library function N over that interface is the derived surface reading it.
        //     Property syntax there is correct on both sides. Only a receiver that is
        //     NEITHER a field owner NOR an interface a field owner satisfies is ambiguous
        //     (ConceptGrounding.GroundsSelf is the same test the writer/checker use).
        //   * a '_' receiver. That is Plato's type-level idiom and the writer emits a
        //     STATIC member for it (CSharpFunctionInfo.IsStatic, LINT012), which is never
        //     rendered with instance property syntax.
        //
        // Info: the rule reports a naming collision in the shape of the vocabulary, both
        // sides of which are legal Plato, and it fires on the existing corpus — so it must
        // not gate `lint --strict` or enter the ratchet (the LINT003 precedent).
        // -------------------------------------------------------------------

        /// <summary>
        /// A library function in the no-arg MEMBER form: exactly one parameter (the receiver),
        /// which is a real named type rather than a type variable, and which is not the '_'
        /// type-level marker (that emits as a static, so it has no property-syntax hazard).
        /// </summary>
        public static bool IsNoArgMemberForm(FunctionDef f)
            => f != null
               && f.Parameters.Count == 1
               && f.Parameters[0].Name != "_"
               && f.Parameters[0].Type?.Def != null
               && !f.Parameters[0].Type.IsTypeVariable
               && f.Parameters[0].Type.Def.Kind != TypeKind.SelfType;

        public void CheckFieldVersusNoArgFunctionNames()
        {
            // name -> the concrete types declaring a field of that name.
            var fieldOwners = new Dictionary<string, List<TypeDef>>();
            foreach (var td in Compilation.TypeDefinitions)
            {
                if (td.Kind != TypeKind.ConcreteType || td.IsUnique)
                    continue; // only a generated struct contributes to the struct surface
                foreach (var field in td.Fields)
                {
                    if (!fieldOwners.TryGetValue(field.Name, out var owners))
                        fieldOwners.Add(field.Name, owners = new List<TypeDef>());
                    if (!owners.Contains(td))
                        owners.Add(td);
                }
            }

            foreach (var lib in Compilation.LibraryDefinitionsByName.Values)
            {
                foreach (var f in lib.Functions)
                {
                    if (!IsNoArgMemberForm(f))
                        continue;
                    if (!fieldOwners.TryGetValue(f.Name, out var owners))
                        continue;

                    var receiver = f.Parameters[0].Type.Def;
                    // Field forwarding (the receiver owns the field, or the field owner
                    // satisfies the receiver interface) is the intended design, not a collision.
                    var unrelated = owners
                        .Where(o => o != receiver && !ConceptGrounding.GroundsSelf(o, receiver))
                        .ToList();
                    if (unrelated.Count == 0)
                        continue;

                    var shown = string.Join(", ", unrelated.Take(3).Select(o => $"'{o.Name}.{f.Name}'"));
                    var more = unrelated.Count > 3 ? $" (+{unrelated.Count - 3} more)" : "";
                    Add(f, "LINT014", LintSeverity.Info,
                        $"'{f.Name}' is both a struct field ({shown}{more}) and the no-arg function " +
                        $"'{f.Name}({receiver.Name})' in library '{lib.Name}'; the C# writer decides " +
                        $"property-vs-method call syntax by NAME, so the field forces property syntax " +
                        $"onto this function's call sites even though '{receiver.Name}' has no such field " +
                        $"(plato-323: rename one side, or land the per-receiver rendering rule)");
                }
            }
        }

        // -------------------------------------------------------------------
        // LINT015: a type declaration whose NAME is a writer primitive, and which
        // declares fields.
        //
        // Primitiveness is decided by a NAME LIST in the backend
        // (WriterPrimitiveNames.All, mirrored as CSharpWriter.PrimitiveTypes),
        // invisible in the language. When a name is on that list the writer emits the
        // runtime type it maps to and never emits the declared fields
        // (CSharpConcreteTypeWriter guards its "// Fields" block on `!IsPrimitive`).
        // So `type Plane { Normal: Direction3D; Distance: Number }` reads as an
        // ordinary declaration but compiles to System.Numerics.Plane, whose real shape
        // is (Normal: Vector3, D: float). Nothing anywhere reported the mismatch: the
        // declaration is the checker's authority and the runtime is the writer's, and
        // the two were never compared. One un-checked refactor of a declaration is
        // silent wrong bits at runtime (plato-365).
        //
        // FIELDLESS declarations of a primitive name are NOT flagged: that is the
        // sanctioned idiom by which the stdlib states which interfaces an intrinsic
        // satisfies (`type Number implements Real { }`, stdlib/foundation/
        // primitives.types.plato). Nothing is discarded, because nothing was declared.
        // The rule is about a declared SHAPE that the writer throws away.
        //
        // Home: the linter rather than the type checker. It is a whole-program scan of
        // declarations against a backend fact, with no expression- or inference-level
        // reasoning — the exact shape of LINT002/LINT006/LINT014 — and it must run for
        // every backend, not only the ones with a type-checked path.
        //
        // Severity: Error, except for the names already shadowing when the rule landed
        // (WriterPrimitiveNames.KnownShadowedByStdlib), which report as Warnings while
        // plato-365 retires them. The point of the rule is prevention: any name added
        // to either side from now on is an immediate `lint --strict` failure.
        // -------------------------------------------------------------------
        public void CheckPrimitiveNameShadowing()
        {
            foreach (var td in Compilation.TypeDefinitions)
            {
                if (td.Kind != TypeKind.ConcreteType && td.Kind != TypeKind.Primitive)
                    continue;
                if (td.Fields.Count == 0)
                    continue; // declaring an intrinsic's interface surface; nothing is discarded
                if (!WriterPrimitiveNames.All.Contains(td.Name))
                    continue;

                var known = WriterPrimitiveNames.KnownShadowedByStdlib.Contains(td.Name);
                var fields = string.Join(", ", td.Fields.Select(f => $"{f.Name}: {f.Type}"));
                Add(td, "LINT015", known ? LintSeverity.Warning : LintSeverity.Error,
                    $"type '{td.Name}' declares fields ({fields}) but '{td.Name}' is a writer primitive " +
                    $"(WriterPrimitiveNames.All): the backend emits the runtime type for that name and " +
                    $"discards this shape, so the declaration is not the authority it appears to be. " +
                    $"Rename the type, or remove '{td.Name}' from the primitive name list so the " +
                    $"declaration generates an ordinary struct (plato-365)" +
                    (known
                        ? " — reported as a Warning only because this shadowing predates the rule; " +
                          "it is scheduled for removal in plato-365"
                        : ""));
            }
        }

        public static bool IsFunctionTypeName(string name)
            => name != null && name.StartsWith("Function") && name.Length > 8 && char.IsDigit(name[8]);

        public static void CountTypeVariables(
            TypeExpression te, Dictionary<string, int> counts, bool insideFunctionType, HashSet<string> inFunctionType)
        {
            if (te == null)
                return;
            if (te.IsTypeVariable)
            {
                counts[te.Name] = counts.TryGetValue(te.Name, out var c) ? c + 1 : 1;
                if (insideFunctionType)
                    inFunctionType?.Add(te.Name);
            }
            var inner = insideFunctionType || IsFunctionTypeName(te.Name);
            foreach (var ta in te.TypeArgs)
                CountTypeVariables(ta, counts, inner, inFunctionType);
        }
    }
}
