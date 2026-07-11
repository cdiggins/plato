using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Scalar-erasure (<c>--scalar=float</c>) counterpart of <see cref="EmitFlagOnTests"/>:
    /// generates the WHOLE scalar-erased library twice — once with <see cref="CSharpWriter.UseTir"/>
    /// OFF (the legacy symbol-graph body writer, which the V2 conformance suite + golden gate) and once
    /// with it ON (bodies routed through <see cref="TirCSharpBodyWriter"/> with the extension
    /// re-qualification context) — and asserts the two are byte-identical file-for-file. This is the
    /// flip criterion for retiring the legacy writer's scalar-erased heuristics.
    /// </summary>
    [TestFixture]
    public static class ScalarEmitFlagOnTests
    {
        private static string Canonical(string s)
            => string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));

        [Test]
        public static void UseTirOnReproducesTheFullScalarErasedLibraryByteForByte()
        {
            var flagOff = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-off")
                { ExtensionStyle = true, ScalarErase = true, UseTir = false };
            flagOff.WriteAll("float");

            var flagOn = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-on")
                { ExtensionStyle = true, ScalarErase = true, UseTir = true };
            flagOn.WriteAll("float");

            var totalEligible = flagOn.TirBodiesEmitted + flagOn.TirFallbackBodies;
            TestContext.WriteLine("=== UseTir flag-ON full-library measurement (--scalar=float extension style) ===");
            TestContext.WriteLine($"bodies emitted from the TIR   : {flagOn.TirBodiesEmitted}");
            TestContext.WriteLine($"  fell back to legacy writer  : {flagOn.TirFallbackBodies}");
            TestContext.WriteLine($"  total eligible bodies       : {totalEligible}"
                + (totalEligible > 0 ? $"  ({100.0 * flagOn.TirBodiesEmitted / totalEligible:F1} % from TIR)" : ""));

            Assert.Greater(flagOn.TirBodiesEmitted, 0,
                "the TIR path never fired — the flag-on library would be trivially all-fallback");

            Assert.AreEqual(flagOff.Files.Count, flagOn.Files.Count, "flag-on generated a different set of files");

            var differing = new List<string>();
            foreach (var kv in flagOff.Files)
            {
                Assert.IsTrue(flagOn.Files.ContainsKey(kv.Key), $"flag-on output is missing file {kv.Key}");
                if (Canonical(kv.Value.ToString()) != Canonical(flagOn.Files[kv.Key].ToString()))
                    differing.Add(kv.Key);
            }

            var identical = flagOff.Files.Count - differing.Count;
            TestContext.WriteLine($"flag-on vs flag-off files byte-identical      : {identical} / {flagOff.Files.Count}");
            foreach (var d in differing)
            {
                TestContext.WriteLine($"  DIFFERS: {d}");
                // First differing line, for fast diagnosis.
                var a = Canonical(flagOff.Files[d].ToString()).Split('\n');
                var b = Canonical(flagOn.Files[d].ToString()).Split('\n');
                for (var i = 0; i < a.Length && i < b.Length; i++)
                    if (a[i] != b[i])
                    {
                        TestContext.WriteLine($"    line {i + 1}:");
                        TestContext.WriteLine($"      off: {a[i]}");
                        TestContext.WriteLine($"      on : {b[i]}");
                        break;
                    }
            }

            Assert.AreEqual(0, flagOn.TirFallbackBodies,
                "an scalar-erased body fell back to the legacy writer");

            Assert.IsEmpty(differing,
                "UseTir=true changed scalar-erased output vs UseTir=false");
        }
    }
}
