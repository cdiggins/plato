using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Well-formedness checker for sum types and <c>match</c> expressions (wave-2, plato-232).
    /// Runs at the SYMBOL level — before the elaborator lowers a match into a tag-conditional
    /// chain — so every diagnostic carries a source location (via <see cref="Compilation.GetAstNode"/>
    /// on the offending symbol) and names the sum type plus the offending/missing cases, per the
    /// design doc catalog (docs/design/plato-sum-types-design-2026-07-27.md §6).
    ///
    /// Diagnostics (CHK3xx):
    ///   CHK300 non-exhaustive match (missing case[s])
    ///   CHK301 unknown case in a match arm
    ///   CHK302 duplicate match arm for one case
    ///   CHK303 binder count != case field count
    ///   CHK304 match subject is not a sum type
    ///   CHK305 sum declaration repeats a case name
    ///   CHK306 generic sum type (only when generics are restricted — see GenericsRestricted)
    /// </summary>
    public class SumTypeChecker
    {
        public Compilation Compilation { get; }

        /// <summary>When true (the v1 default), a generic sum type is rejected with CHK306. The
        /// generic sum DECLARATION resolves, but a generic library CONSUMER cannot: a free type
        /// variable in a library signature must be spelled <c>$T</c> (the stdlib convention), and
        /// bare <c>T</c> — as option.plato writes it — does not resolve. Auto-generalizing bare
        /// library type parameters is a separate front-end change, so v1 ships monomorphic sums
        /// only (design doc §3.5 fallback). Set false once that front-end gap is closed.</summary>
        public bool GenericsRestricted { get; }

        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        public SumTypeChecker(Compilation compilation, bool genericsRestricted = true)
        {
            Compilation = compilation;
            GenericsRestricted = genericsRestricted;
        }

        public IReadOnlyList<CheckDiagnostic> Check()
        {
            foreach (var td in Compilation.AllTypeAndLibraryDefinitions)
                if (td != null && td.IsSum)
                    CheckDeclaration(td);

            foreach (var f in Compilation.FunctionDefinitions ?? Enumerable.Empty<FunctionDef>())
                if (f.Body != null)
                    foreach (var m in f.Body.GetSymbolTree().OfType<MatchExpression>())
                        CheckMatch(m);

            return Diagnostics;
        }

        // --- declaration checks --------------------------------------------------

        private void CheckDeclaration(TypeDef sum)
        {
            // CHK305: a case name declared more than once.
            var seen = new HashSet<string>();
            foreach (var c in sum.Cases)
                if (!seen.Add(c.Name))
                    Error("CHK305",
                        $"sum type '{sum.Name}' declares case '{c.Name}' more than once", sum);

            // CHK306: a generic sum, only when the generics stance is restricted.
            if (GenericsRestricted && sum.TypeParameters.Count > 0)
            {
                var ps = string.Join(", ", sum.TypeParameters.Select(p => p.Name));
                Error("CHK306",
                    $"generic sum type '{sum.Name}<{ps}>' is not supported in v1; declare a monomorphic "
                    + "sum or await the generics follow-up", sum);
            }
        }

        // --- match checks --------------------------------------------------------

        private void CheckMatch(MatchExpression m)
        {
            var sum = m.SumType;

            // CHK304: the subject is not a sum type. Product types, primitives, interfaces, arrays.
            if (sum == null || !sum.IsSum)
            {
                Error("CHK304",
                    $"cannot match on value of type '{sum?.Name ?? "?"}': match requires a sum type", m);
                return;
            }

            var caseByName = sum.Cases.ToDictionary(c => c.Name);
            var armCounts = new Dictionary<string, int>();

            foreach (var arm in m.Arms)
            {
                if (!caseByName.TryGetValue(arm.CaseName, out var caseDef))
                {
                    // CHK301: an arm names a case the sum type lacks.
                    var cases = string.Join(", ", sum.Cases.Select(c => c.Name));
                    Error("CHK301",
                        $"unknown case '{arm.CaseName}' in match on sum type '{sum.Name}' (cases: {cases})", m);
                    continue;
                }

                armCounts[arm.CaseName] = armCounts.TryGetValue(arm.CaseName, out var n) ? n + 1 : 1;

                // CHK303: the pattern's binder count must equal the case's field count.
                if (arm.Binders.Count != caseDef.Fields.Count)
                {
                    var fields = string.Join(", ", caseDef.Fields.Select(f => f.Name));
                    Error("CHK303",
                        $"case '{caseDef.Name}' of sum type '{sum.Name}' binds {caseDef.Fields.Count} "
                        + $"field(s) ({fields}) but the pattern supplies {arm.Binders.Count}", m);
                }
            }

            // CHK302: a case matched more than once.
            foreach (var kv in armCounts)
                if (kv.Value > 1)
                    Error("CHK302",
                        $"duplicate match arm for case '{kv.Key}' of sum type '{sum.Name}'", m);

            // CHK300: every case must appear; report the missing ones (declaration order).
            var missing = sum.Cases.Where(c => !armCounts.ContainsKey(c.Name)).Select(c => c.Name).ToList();
            if (missing.Count > 0)
            {
                var names = string.Join(", ", missing.Select(n => $"'{n}'"));
                Error("CHK300",
                    $"non-exhaustive match on sum type '{sum.Name}': missing case(s) {names} — "
                    + "add an arm for each (v1 has no default arm)", m);
            }
        }

        private void Error(string code, string message, Symbol origin)
            => Diagnostics.Add(new CheckDiagnostic(DiagnosticSeverity.Error, code, message, origin));
    }
}
