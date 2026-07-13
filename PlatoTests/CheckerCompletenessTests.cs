using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The checker-completeness RATCHET (scalar-lowering plan, Mission 1.4). Asserts the number of
    /// stdlib functions carrying error diagnostics never rises above the pinned baseline; TIGHTEN it
    /// (never loosen) as coercion/dispatch gaps are closed, until it reaches 0. The full per-function
    /// worklist is printed by <see cref="CheckerDiagnosticsSummaryTests"/>; this is the gate.
    /// </summary>
    [TestFixture]
    public static class CheckerCompletenessTests
    {
        // Measured 2026-07-12 after the ambiguity work: F2 coercion + the F1 tie fixes
        // (all-variable-return ties commit; bounded-polymorphic ties on unbound constrained type
        // variables defer to monomorphization; C#-style specificity tie-break picks the most-derived
        // overload, IArray2D over IArray). **CHK203 ambiguity errors are now ZERO** (was 25). 32 / 859
        // remain — all CHK101 (cannot-unify, 19) + CHK201 (no-match, 13): F2a tuple→generic-interface
        // returns and library repairs, not ambiguities. The count drifts with library growth, so the
        // baseline is a ceiling to lower, not a fixed target.
        private const int MaxFunctionsWithDiagnostics = 32;

        [Test]
        public static void StdLibDiagnosticCountDoesNotRegress()
        {
            var results = new TypeChecker(CheckerTestSupport.CompileStdLib()).CheckAll();
            var failing = results.Count(r => !r.Succeeded);
            TestContext.WriteLine($"functions with diagnostics: {failing} / {results.Count} (ceiling {MaxFunctionsWithDiagnostics})");
            Assert.LessOrEqual(failing, MaxFunctionsWithDiagnostics,
                "Checker completeness regressed: more stdlib functions carry diagnostics than the ratchet baseline. "
                + "See CheckerDiagnosticsSummaryTests for the worklist.");
        }
    }
}
