// The current partitioner: consecutive definitions, a fixed line limit per unit.
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Keeps definitions intact and preserves their deterministic dependency order. A single oversized
    /// definition occupies its own unit; an empty program still emits one unit so hosts need no special
    /// empty-program convention.
    /// </summary>
    public sealed class LineLimitPartitioner : IUnitPartitioner
    {
        public int MaxFunctionLines { get; init; } = 20000;
        public int MaxJsonLines { get; init; } = 2000;

        public IEnumerable<string> Partition(IEnumerable<string> definitions, string head, string tail, UnitKind kind)
        {
            var maxLines = kind == UnitKind.Json ? MaxJsonLines : MaxFunctionLines;
            var sb = new StringBuilder(head);
            var lines = 0;
            foreach (var definition in definitions)
            {
                var def = definition.TrimEnd('\r', '\n');
                var defLines = def.Count(c => c == '\n') + 2;
                if (lines > 0 && lines + defLines > maxLines)
                {
                    yield return sb.Append(tail).ToString();
                    sb = new StringBuilder(head);
                    lines = 0;
                }
                sb.Append(def).Append("\n\n");
                lines += defLines;
            }
            yield return sb.Append(tail).ToString();
        }
    }

    public static class UnitLinePolicies
    {
        /// <summary>The policies with function units limited to <paramref name="lines"/> lines (--cpp-unit-lines=N).</summary>
        public static CppPolicies WithUnitLines(this CppPolicies p, int lines) => new CppPolicies
        {
            Parameters = p.Parameters, Reads = p.Reads, Lambdas = p.Lambdas, Sums = p.Sums, Strings = p.Strings,
            Units = new LineLimitPartitioner { MaxFunctionLines = lines },
            Attributes = p.Attributes, Loops = p.Loops,
        };
    }
}
