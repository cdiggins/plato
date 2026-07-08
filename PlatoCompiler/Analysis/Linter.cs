using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Parakeet;

namespace Ara3D.Geometry.Compiler.Analysis
{
    /// <summary>
    /// A single lint finding. Formats as "file(line): LINT###: message".
    /// </summary>
    public class LintFinding
    {
        public string File { get; }
        public int Line { get; } // 1-based; 0 = unknown
        public string Code { get; }
        public string Message { get; }

        public LintFinding(string file, int line, string code, string message)
        {
            File = file;
            Line = line;
            Code = code;
            Message = message;
        }

        public override string ToString()
            => $"{File}({Line}): {Code}: {Message}";
    }

    /// <summary>
    /// A post-compilation lint pass (docs/plato-roadmap.md P1.6, plato-library-review.md section 2.1).
    /// Runs five structural checks over the symbol tables of a completed Compilation:
    ///   LINT001 - interface obligations with no implementation (generated code throws NotImplementedException)
    ///   LINT002 - 'where' clauses constraining undeclared type variables
    ///   LINT003 - declared-but-unused type fields
    ///   LINT004 - duplicate function signatures within a library
    ///   LINT005 - generic type variables used only once, or used in a return type but not inferable from parameters
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
        }

        public IEnumerable<LintFinding> SortedFindings
            => Findings.OrderBy(f => f.File).ThenBy(f => f.Line).ThenBy(f => f.Code);

        private readonly HashSet<string> _seen = new HashSet<string>();

        public void Add(ILocation location, string code, string message)
        {
            var range = location?.GetRange();
            var file = range?.FilePath.ToString() ?? "<unknown>";
            var line = range != null ? range.BeginLineIndex + 1 : 0;
            if (_seen.Add($"{file}|{line}|{code}|{message}"))
                Findings.Add(new LintFinding(file, line, code, message));
        }

        public void Add(Symbol symbol, string code, string message)
            => Add(GetLocation(symbol), code, message);

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
            "At", "Count", // generated from the type's fields
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
                    Add(GetLocation(ct.TypeDef) ?? GetLocation(fi.Implementation), "LINT001",
                        $"type '{ct.Name}' implements '{declaringInterface}' but no implementation was found for " +
                        $"'{fi.SignatureId}'; the generated member will throw NotImplementedException");
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
                        Add(c, "LINT002",
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
                    Add(field, "LINT003",
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
                        Add(f, "LINT004",
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
                        Add(f, "LINT005",
                            $"function '{f.Name}' in library '{lib.Name}' uses type variable '{tv}' in its return " +
                            $"type '{f.ReturnType}' but in no parameter; it can never be inferred at a call site");
                    }

                    foreach (var tv in paramCounts.Keys)
                    {
                        var total = paramCounts[tv] + (returnCounts.TryGetValue(tv, out var rc) ? rc : 0);
                        if (total == 1 && inFunctionType.Contains(tv))
                        {
                            Add(f, "LINT005",
                                $"function '{f.Name}' in library '{lib.Name}' takes a function argument producing " +
                                $"'{tv}' but uses '{tv}' nowhere else; the declared return type '{f.ReturnType}' is " +
                                $"likely inconsistent with the type variables used");
                        }
                    }
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
