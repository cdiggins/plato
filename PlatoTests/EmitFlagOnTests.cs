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
    /// NORMALIZATION: the writer uses two PROCESS-GLOBAL monotonic counters for synthesized names —
    /// <c>_var{N}</c> (lambda-capture temporaries) and <c>Core_{N}</c> (library class ids) — that do
    /// NOT reset between <c>WriteAll</c> runs, so any two runs in one process differ on those names
    /// alone (a fresh-process CLI run has no such offset — <c>--use-tir</c> vs the golden is
    /// 164/164). Both flag-off and flag-on emit these names identically MODULO the counter value, so
    /// the comparison canonicalizes the counters before diffing, exactly as regen strips the
    /// volatile timestamp header. This is pre-existing writer nondeterminism, unrelated to the flag.
    /// </summary>
    [TestFixture]
    public static class EmitFlagOnTests
    {
        // Lambda-capture temporaries "_var{N}" and any synthesized "{LibraryName}_{N}" class id
        // (Core_7, Vectors_24, ...) — both driven by process-global counters. The suffix regex
        // keeps the prefix (so a genuine name difference is still caught) and only canonicalizes
        // the volatile "_<digits>" tail that a letter precedes.
        private static readonly Regex VarCounter = new Regex(@"_var\d+", RegexOptions.Compiled);
        private static readonly Regex NameIdSuffix = new Regex(@"(?<=[A-Za-z])_\d+", RegexOptions.Compiled);

        // Drop the volatile "// Created on" header and canonicalize the process-global name
        // counters, so only real content differences remain.
        private static string Canonical(string s)
        {
            s = string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));
            s = VarCounter.Replace(s, "_var#");
            s = NameIdSuffix.Replace(s, "_#");
            return s;
        }

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
