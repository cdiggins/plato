using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The LINT ratchet for the forward stdlib — the sibling of
    /// <see cref="ForwardStdLibCheckerTests.ForwardStdLibDiagnosticCountDoesNotRegress"/>, which
    /// ratchets TYPE-CHECKER diagnostics. The two counters are unrelated and both are needed.
    ///
    /// Why a test and not the gate script: `Plato.CLI lint --strict` fails on ERRORS only. It
    /// prints the ratchet count and moves on, so a change that adds twenty LINT001 warnings — an
    /// obligation left unimplemented, which becomes a NotImplementedException in generated C# —
    /// passes every gate silently. Nothing enforced the printed number until this file.
    ///
    /// Info findings are deliberately excluded (the linter's own RatchetCount does the same):
    /// LINT003 alone is ~1.5k unused-field notes over vocabulary declared ahead of its bodies, so
    /// including them would make the ceiling a number nobody could move.
    ///
    /// SCOPE (stdlib-377): the ratchet lints the SHIPPING tiers only — foundation, geometry,
    /// graphics. `stdlib/future` is aspirational vocabulary that is not converted to C# and not
    /// linted; it must still parse and type-check, which
    /// <see cref="ForwardStdLibCheckerTests"/> enforces over all four tiers.
    /// <see cref="SummarizeForwardStdLibLintIncludingFuture"/> is the opt-in view that includes it.
    /// </summary>
    [TestFixture]
    public static class ForwardStdLibLintTests
    {
        // Measured 2026-08-01 over foundation+geometry+graphics: 32, Error 0. (Was 33 until
        // plato-382 phase D; 39 for all four tiers at 33; 38/44 before plato-378, 229 at 52b3f8c
        // on 2026-07-31, 159 mid-day, then plato-321 closed the obligation burn-down.)
        //
        //   LINT001 - a type implements a concept but an obligation has no implementation; the
        //             generated member throws NotImplementedException. Now ZERO across the
        //             shipping tiers. The last one was `Tween<T>`'s Sample, discharged by
        //             plato-382: `type Tween<T> where T: Interpolatable` makes the Lerp of From
        //             and To well-typed, so `library MotionGraphics` can carry a real body. (The
        //             two remaining LINT001 findings in the ALL-TIERS view are the `future`
        //             AnimationTrack/TangentTrack, which are out of this ratchet's scope.)
        //   LINT013 - a concept with no concrete implementer that library bodies nonetheless
        //             dispatch on, so that derived surface is unreachable (Sliceable,
        //             Concatenable, ...). All 32 of the 32. Burn-down: plato-277 / plato-325.
        //
        // A ceiling to LOWER, never to raise. Lower it in the same commit that earns it.
        private const int MaxLintRatchet = 32;

        // The Linter runs every rule from its constructor.
        private static Linter LintForwardStdLib()
            => new Linter(CheckerTestSupport.CompileForwardStdLibShippingTiers());

        private static IEnumerable<string> ByCode(Linter linter)
            => linter.Findings
                .GroupBy(f => f.Code)
                .OrderByDescending(g => g.Count())
                .Select(g => $"  {g.Key} x{g.Count()} ({g.First().Severity})");

        [Test]
        public static void ForwardStdLibLintRatchetDoesNotRegress()
        {
            var linter = LintForwardStdLib();
            foreach (var line in ByCode(linter))
                TestContext.WriteLine(line);
            TestContext.WriteLine(
                $"ratchet: {linter.RatchetCount} (Error {linter.ErrorCount} + Warning {linter.WarningCount}); "
                + $"Info {linter.InfoCount} excluded; ceiling {MaxLintRatchet}");

            Assert.IsEmpty(
                linter.Findings.Where(f => f.Severity == LintSeverity.Error).Select(f => f.ToString()),
                "Forward stdlib has LINT errors; `lint --strict` is red.");
            Assert.LessOrEqual(linter.RatchetCount, MaxLintRatchet,
                "Forward stdlib lint ratchet regressed. Fix the finding, or lower nothing and "
                + "explain here why the ceiling had to move. See SummarizeForwardStdLibLint for "
                + "the worklist.");
        }

        /// <summary>Not an assertion — the worklist. Run it to see what the ratchet is made of.</summary>
        [Test]
        public static void SummarizeForwardStdLibLint()
            => Summarize(LintForwardStdLib());

        /// <summary>The opt-in view: lint including `stdlib/future`, which the ratchet above does
        /// not gate on. Reporting only — a finding here is information about vocabulary nobody is
        /// shipping yet, not a regression.</summary>
        [Test]
        public static void SummarizeForwardStdLibLintIncludingFuture()
            => Summarize(new Linter(CheckerTestSupport.CompileForwardStdLib()));

        private static void Summarize(Linter linter)
        {
            foreach (var line in ByCode(linter))
                TestContext.WriteLine(line);

            TestContext.WriteLine("");
            TestContext.WriteLine("Ratchet findings by owning type/concept (most first):");
            var owners = linter.Findings
                .Where(f => f.Severity != LintSeverity.Info)
                .GroupBy(f => f.File)
                .OrderByDescending(g => g.Count());
            foreach (var g in owners)
                TestContext.WriteLine($"  {g.Key} x{g.Count()}");
        }
    }
}
