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
    ///   LINT008 - concept with no concrete implementer
    ///   LINT009 - concept that nothing in the compilation mentions at all
    ///   LINT010 - concrete type that implements no concept and that no function or field mentions
    ///   LINT011 - redundant 'implements' clause (already implied by another implemented concept)
    ///   LINT012 - obligation and implementation disagree on the '_' receiver marker (static vs instance)
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
            CheckReceiverMarkerAgreement();
        }

        public IEnumerable<LintFinding> SortedFindings
            => Findings.OrderBy(f => f.File).ThenBy(f => f.Line).ThenBy(f => f.Code);

        public int ErrorCount
            => Findings.Count(f => f.Severity == LintSeverity.Error);

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
            // Library/concept functions are not mapped directly; their owning MethodDef is.
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
            "Range", "MakeArray2D", "MapRange",
            // Synthesized by the writer (CSharpTypeWriter.GenerateFunc) from the type's shape:
            // from the FIELDS for a fixed-arity type (Point3D: X/Y/Z), or by delegating to the
            // single field that is itself indexable for a runtime-arity type (VectorN: Components).
            // A type whose shape fits neither must declare an explicit At/Count library body — that
            // lands in ImplementedFunctions and never reaches the synthesis (e.g. RegularPolygon).
            "At", "Count",
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
        // So when a concept declares Zero(x: Self) and the implementation writes
        // Zero(_: Color), the interface gets an instance member and the type gets a
        // static one — which cannot satisfy it (C# CS0736). The reverse disagreement
        // is just as real: concept Vector declared Broadcast(_: Self, x) while
        // VectorN.Broadcast uses its receiver for arity.
        //
        // Nothing else in the language looks at a parameter NAME, so neither
        // direction was caught until C# compilation, more than a thousand generated
        // files downstream. This rule is the gate: it puts the disagreement at the
        // implementation, at lint time, in any backend. See plato-312 and the
        // 2026-07-29 static-concept-members ADR.
        // -------------------------------------------------------------------

        /// <summary>Plato's marker for "receiver type matters, receiver value does not".</summary>
        public static bool HasIgnoredReceiver(FunctionInstance f)
            => f.ParameterNames.Count > 0 && f.ParameterNames[0] == "_";

        public void CheckReceiverMarkerAgreement()
        {
            foreach (var ct in Compilation.ConcreteTypes)
            {
                // Same pairing the writer and LINT001 use: an obligation is discharged by
                // the implementation sharing its (substituted) signature.
                var implBySignature = new Dictionary<string, FunctionInstance>();
                foreach (var impl in ct.ImplementedFunctions)
                    if (!implBySignature.ContainsKey(impl.SignatureId))
                        implBySignature.Add(impl.SignatureId, impl);

                foreach (var declared in ct.DeclaredFunctions)
                {
                    if (MembersImplementedByWriter.Contains(declared.Name))
                        continue; // synthesized by the writer; no authored receiver to compare
                    if (!implBySignature.TryGetValue(declared.SignatureId, out var impl))
                        continue; // unimplemented is LINT001's business, not this rule's
                    if (declared.ParameterNames.Count == 0 || impl.ParameterNames.Count == 0)
                        continue;

                    var obligationIsTypeLevel = HasIgnoredReceiver(declared);
                    if (obligationIsTypeLevel == HasIgnoredReceiver(impl))
                        continue;

                    var owner = declared.Implementation.OwnerType?.Name ?? declared.InterfaceName;
                    var advice = obligationIsTypeLevel
                        ? $"the '{owner}' obligation marks the receiver '_' (type-level, emitted static) but this " +
                          $"implementation names it '{impl.ParameterNames[0]}' (emitted as an instance member)"
                        : $"this implementation marks the receiver '_' (type-level, emitted static) but the " +
                          $"'{owner}' obligation names it '{declared.ParameterNames[0]}' (an instance member)";

                    // Anchored at the IMPLEMENTATION: that is the edit site, and it collapses one
                    // finding per concrete type down to one per offending body.
                    Add(GetLocation(impl.Implementation) ?? GetLocation(ct.TypeDef), "LINT012", LintSeverity.Warning,
                        $"'{impl.SignatureId}': {advice}. A static member cannot implement an instance " +
                        $"obligation (C# CS0736) and vice versa; use '_' on both when the operation is " +
                        $"type-level, or a name on both when any implementor needs the receiver");
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
                if (td.Constraints.Count == 0)
                    continue;
                var declared = td.TypeParameters.Select(tp => tp.Name.Text).ToHashSet();
                foreach (var c in td.Constraints)
                {
                    if (!declared.Contains(c.Name.Text))
                    {
                        var declaredList = declared.Count == 0 ? "none" : string.Join(", ", declared);
                        Add(c, "LINT002", LintSeverity.Error,
                            $"'where' clause of '{td.Name.Text}' constrains '{c.Name.Text}' which is not a declared " +
                            $"type parameter (declared: {declaredList}); the constraint is silently ignored");
                    }
                }
            }
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
                    Add(field, "LINT003", LintSeverity.Warning,
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
        // LINT005: Generic type-variable consistency in library and concept
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
        // LINT008/LINT009/LINT010: reachability of the vocabulary. In a concept-
        // oriented library a declaration earns its place by being implemented or
        // used; these three rules report the declarations that are neither.
        //   LINT008 - a concept no concrete type implements. Either dead
        //             vocabulary or a forgotten 'implements' line. Implementer
        //             lookup is transitive over concept inheritance, so an
        //             abstract intermediate concept counts as implemented as
        //             soon as any descendant concept has an implementer.
        //   LINT009 - a concept that nothing mentions at all: not implemented,
        //             not inherited, never a parameter/return type, never a
        //             'where' bound. Strictly stronger than LINT008, and
        //             reported instead of it.
        //   LINT010 - a concrete type that implements no concept AND that no
        //             other declaration mentions (no function signature, no
        //             field type). An unfinished declaration.
        // All three are Info: on a declarations-first folder they describe the
        // shape of the vocabulary rather than a defect.
        // -------------------------------------------------------------------
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

            // 'where' bounds are dropped by the resolver (see LINT002), so read them off the AST.
            var boundNames = Compilation.TypeDeclarations
                .SelectMany(d => d.Constraints)
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
                        $"concept '{c.Name}' is unreachable: no concrete type implements it, no concept " +
                        $"inherits it, and it appears in no parameter type, return type or 'where' bound");
                else
                    Add(c, "LINT008", LintSeverity.Info,
                        $"concept '{c.Name}' has no concrete implementer" +
                        (inherited.Contains(c) ? " (directly or through a descendant concept)" : "") +
                        "; it is either dead vocabulary or a missing 'implements' clause");
            }

            foreach (var td in Compilation.GetConcreteTypes())
            {
                if (td.IsPrimitive() || td.IsUnique)
                    continue; // intrinsic; backed by the runtime, not by declarations
                if (td.Implements.Count > 0 || mentioned.Contains(td))
                    continue;
                Add(td, "LINT010", LintSeverity.Info,
                    $"concrete type '{td.Name}' implements no concept and is mentioned by no function " +
                    $"signature or field; it participates in nothing");
            }
        }

        // -------------------------------------------------------------------
        // LINT011: an 'implements' clause already implied by another one on the
        // same type (Sphere implements ParametricSurface and Geometry3D, where
        // ParametricSurface already reaches Geometry3D). Legal, and sometimes
        // written deliberately to restate the top-level category - hence Info -
        // but it hides the real concept lattice and goes stale when the lattice
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
