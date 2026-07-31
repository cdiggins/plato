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
        // Measured 2026-07-31 at 32891dd: 159, Error 0. (Was 229 at 52b3f8c hours earlier;
        // plato-321's obligation burn-down took 70 off it.)
        //
        //   LINT001 - a type implements a concept but an obligation has no implementation; the
        //             generated member throws NotImplementedException. Concentrated in the matrix
        //             family (Matrix2x2 x10, MatrixN / Matrix4x3 / Matrix3x3 x9 each) plus
        //             ColorXYZ x11 and HalfEdgeMesh x8. Same gap the writer reports as a degraded
        //             body, seen from the declaration side.
        //   LINT013 - a concept with no concrete implementer that library bodies nonetheless
        //             dispatch on, so that derived surface is unreachable (MetricSpace, Sliceable,
        //             Concatenable, ...). Burn-down: plato-277.
        //
        // A ceiling to LOWER, never to raise. Lower it in the same commit that earns it.
        private const int MaxLintRatchet = 159;

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
