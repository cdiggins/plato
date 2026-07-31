using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Optimizer stage 2 increment 1 (--optimize-arrays, TirArrayMaterializer): generates the
    /// default-style library with the flag off and on, and asserts the ONLY differences are
    /// Map → MapEager / MapRange → MapRangeEager call-site renames (and that some actually
    /// happened). Semantics are covered by the Opt conformance suite, which now runs with the
    /// flag; this test pins the transform's footprint.
    /// </summary>
    [TestFixture]
    public static class ArrayMaterializerTests
    {
        private static string Canonical(string s)
            => string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));

        [Test]
        public static void FlagOnlyRenamesMapCallSitesInMaterializationPositions()
        {
            var off = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-off");
            off.WriteAll("float");

            var on = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-on") { OptimizeArrays = true };
            on.WriteAll("float");

            Assert.AreEqual(off.Files.Count, on.Files.Count);

            var renames = 0;
            var unexpected = new List<string>();
            foreach (var kv in off.Files)
            {
                var a = Canonical(kv.Value.ToString()).Split('\n');
                var b = Canonical(on.Files[kv.Key].ToString()).Split('\n');
                Assert.AreEqual(a.Length, b.Length, $"{kv.Key}: line count changed");
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i] == b[i])
                        continue;
                    // The only permitted difference: eager renames on this line.
                    var undone = b[i].Replace("MapRangeEager(", "MapRange(").Replace("MapEager(", "Map(");
                    if (undone == a[i])
                        renames++;
                    else
                        unexpected.Add($"{kv.Key}:{i + 1}\n  off: {a[i]}\n  on : {b[i]}");
                }
            }

            TestContext.WriteLine($"lines with eager renames: {renames}");
            foreach (var u in unexpected.Take(10))
                TestContext.WriteLine("UNEXPECTED DIFF: " + u);

            Assert.IsEmpty(unexpected, "--optimize-arrays changed something other than Map/MapRange renames");
            Assert.Greater(renames, 0, "the materializer never fired on the stdlib");
        }
    }
}
