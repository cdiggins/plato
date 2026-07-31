using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.RustWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Rust counterpart of <see cref="EmitFlagOnTests"/>: generates the whole Rust library
    /// twice — UseTir off (legacy symbol-graph body writer) and on (TirRustBodyWriter) —
    /// and asserts byte-identical output. The flip criterion for the Rust writer.
    /// </summary>
    [TestFixture]
    public static class RustEmitFlagOnTests
    {
        private static string Canonical(string s)
            => string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));

        [Test]
        public static void UseTirOnReproducesTheFullRustLibraryByteForByte()
        {
            var flagOff = new RustWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-off") { UseTir = false };
            flagOff.WriteAll();

            var flagOn = new RustWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-on") { UseTir = true };
            flagOn.WriteAll();

            var totalEligible = flagOn.TirBodiesEmitted + flagOn.TirFallbackBodies;
            TestContext.WriteLine("=== UseTir flag-ON full-library measurement (Rust) ===");
            TestContext.WriteLine($"bodies emitted from the TIR   : {flagOn.TirBodiesEmitted}");
            TestContext.WriteLine($"  fell back to legacy writer  : {flagOn.TirFallbackBodies}");
            TestContext.WriteLine($"  total eligible bodies       : {totalEligible}"
                + (totalEligible > 0 ? $"  ({100.0 * flagOn.TirBodiesEmitted / totalEligible:F1} % from TIR)" : ""));

            Assert.Greater(flagOn.TirBodiesEmitted, 0, "the TIR path never fired");
            Assert.AreEqual(flagOff.Files.Count, flagOn.Files.Count, "flag-on generated a different set of files");

            var differing = new List<string>();
            foreach (var kv in flagOff.Files)
            {
                Assert.IsTrue(flagOn.Files.ContainsKey(kv.Key), $"flag-on output is missing file {kv.Key}");
                if (Canonical(kv.Value.ToString()) != Canonical(flagOn.Files[kv.Key].ToString()))
                    differing.Add(kv.Key);
            }

            TestContext.WriteLine($"flag-on vs flag-off files byte-identical      : {flagOff.Files.Count - differing.Count} / {flagOff.Files.Count}");
            foreach (var d in differing)
            {
                TestContext.WriteLine($"  DIFFERS: {d}");
                var a = Canonical(flagOff.Files[d].ToString()).Split('\n');
                var b = Canonical(flagOn.Files[d].ToString()).Split('\n');
                var shown = 0;
                for (var i = 0; i < a.Length && i < b.Length && shown < 10; i++)
                    if (a[i] != b[i])
                    {
                        TestContext.WriteLine($"    line {i + 1}:");
                        TestContext.WriteLine($"      off: {a[i]}");
                        TestContext.WriteLine($"      on : {b[i]}");
                        shown++;
                    }
            }

            Assert.AreEqual(0, flagOn.TirFallbackBodies, "a Rust body fell back to the legacy writer");
            Assert.IsEmpty(differing, "UseTir=true changed Rust output vs UseTir=false");
        }
    }
}
