// Compares generated short-circuit expressions across emission settings.
using System.Linq;
using System.Text.RegularExpressions;

namespace PlatoTests
{
    /// <summary>
    /// The one intended difference between the legacy body writers (UseTir off) and the TIR body
    /// writers of the Rust and TypeScript backends: Boolean <c>&amp;&amp;</c> and <c>||</c>
    /// short-circuit (TKT-442, docs/SEMANTICS.md section 6). The legacy writer has no types, so it
    /// still writes the eager call <c>x.And(y)</c>; the TIR writer writes <c>(x &amp;&amp; y)</c>.
    /// </summary>
    public static class ShortCircuitEmitDifference
    {
        /// <summary>Whether two emitted files are equal line for line, a line pair that differs only
        /// in the spelling of And and Or counting as equal.</summary>
        public static bool Matches(string legacy, string tir)
        {
            var a = legacy.Split('\n');
            var b = tir.Split('\n');
            return a.Length == b.Length && a.Zip(b).All(p => LinesMatch(p.First, p.Second));
        }

        /// <summary>Equal lines, or a legacy line with an <c>.And(</c> or <c>.Or(</c> call that reads
        /// the same as the TIR line once the operator spellings and parentheses are removed.</summary>
        public static bool LinesMatch(string legacy, string tir)
            => legacy == tir
               || ((legacy.Contains(".And(") || legacy.Contains(".Or(")) && Neutral(legacy) == Neutral(tir));

        private static string Neutral(string line)
            => Regex.Replace(line, @"\.And\b|\.Or\b|&&|\|\||[()\s]", "");
    }
}
