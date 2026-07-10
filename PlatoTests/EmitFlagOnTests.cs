using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// De-risking measurement for the off-by-default <see cref="CSharpWriter.UseTir"/> flag
    /// (Elaborate → Monomorphize → Emit retarget). It generates the WHOLE default-style library
    /// twice — once with the flag OFF (the production writer, which <c>regen-plato.ps1</c> proves
    /// equals the checked-in golden) and once with the flag ON (member bodies routed through
    /// <see cref="TirCSharpBodyWriter"/> for every (function, type) with a fully-ground TIR, the
    /// current writer as the fallback) — and asserts the two are byte-identical file-for-file. This
    /// is the real flip criterion: the full 164-file library, not just the aligned-subset
    /// differential. It also reports the body-level split (how many bodies came from the TIR).
    ///
    /// The comparison is EXACT modulo the volatile "// Created on" timestamp header (the same
    /// line regen strips): the `_var{N}` lambda-capture counter resets per WriteAll, and the
    /// Symbol-id name leak (Core_7-style, via the retired "AMBIGUOUS FUNCTIONS" debug comments)
    /// is gone from generated output, so no name canonicalization is needed — a numbering drift
    /// between the two paths now FAILS this test.
    /// </summary>
    [TestFixture]
    public static class EmitFlagOnTests
    {
        // Drop the volatile "// Created on" header so only real content differences remain.
        private static string Canonical(string s)
            => string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));

        [Test]
        public static void UseTirOnReproducesTheFullDefaultLibraryByteForByte()
        {
            // Each writer gets its OWN freshly-parsed compilation (WriteAll is not idempotent over a
            // shared Compilation), so the only residual same-process nondeterminism is the two global
            // name counters canonicalized above. UseTir defaults to TRUE since increment 3, so the
            // legacy comparison writer pins it off explicitly.
            var flagOff = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-off") { UseTir = false };
            flagOff.WriteAll("float");

            var flagOn = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-on") { UseTir = true };
            flagOn.WriteAll("float");

            var totalEligible = flagOn.TirBodiesEmitted + flagOn.TirFallbackBodies;
            TestContext.WriteLine("=== UseTir flag-ON full-library measurement (default C# style) ===");
            TestContext.WriteLine($"member-instance bodies emitted from the TIR : {flagOn.TirBodiesEmitted}");
            TestContext.WriteLine($"  fell back to the current writer            : {flagOn.TirFallbackBodies}");
            TestContext.WriteLine($"  total eligible member bodies               : {totalEligible}"
                + (totalEligible > 0 ? $"  ({100.0 * flagOn.TirBodiesEmitted / totalEligible:F1} % from TIR)" : ""));

            // The measurement is only meaningful if the TIR path actually fired.
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
                TestContext.WriteLine($"  DIFFERS: {d}");

            // The whole point of the flag: OFF and ON produce byte-identical output (modulo the
            // pre-existing global counters). OFF equals the checked-in golden (regen-plato.ps1),
            // so ON does too.
            Assert.IsEmpty(differing,
                "UseTir=true changed generated output vs UseTir=false — the flag is not inert-equivalent on the full library");
        }
    }
}
