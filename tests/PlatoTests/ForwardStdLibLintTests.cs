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
    /// LINT003 alone is ~2.2k unused-field notes over vocabulary declared ahead of its bodies, so
    /// including them would make the ceiling a number nobody could move.
    /// </summary>
    [TestFixture]
    public static class ForwardStdLibLintTests
    {
        // Measured 2026-07-31: 44, Error 0. (229 at 52b3f8c that morning, 159 mid-day; plato-321
        // closed the obligation burn-down and took the rest.)
        //
        //   LINT001 - a type implements a concept but an obligation has no implementation; the
        //             generated member throws NotImplementedException. Down to 8, and all 8 are
        //             one compiler defect rather than missing content: an obligation keyed by a
        //             generic type's OWN parameter cannot be matched by a library function over a
        //             type variable, so Array2D/Array3D's extents and the three animation tracks
        //             are unimplementable from the library side. See plato-376.
        //   LINT013 - a concept with no concrete implementer that library bodies nonetheless
        //             dispatch on, so that derived surface is unreachable (Sliceable,
        //             Concatenable, ...). 36 of the 44. Burn-down: plato-277 / plato-325.
        //
        // A ceiling to LOWER, never to raise. Lower it in the same commit that earns it.
        private const int MaxLintRatchet = 44;

        // The Linter runs every rule from its constructor.
        private static Linter LintForwardStdLib()
            => new Linter(CheckerTestSupport.CompileForwardStdLib());

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
        {
            var linter = LintForwardStdLib();
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
