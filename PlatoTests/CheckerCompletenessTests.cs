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
        // Measured 2026-07-12 after F2 coercion (value→single-field-struct + per-element tuple
        // coercion) AND the F1 all-variable-return tie fix (concept-method Self-var ties commit
        // instead of erroring): 39 / 859 (down from 70). Remaining: F1 concrete-return ties
        // (Subtract/Add/Lerp Vector2|Vector3|… on concept-constrained receivers, CHK203 ~14), F2a
        // tuple→generic-interface returns, and library repairs. The count drifts with library growth,
        // so the baseline is a ceiling to lower, not a fixed target.
        private const int MaxFunctionsWithDiagnostics = 39;

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
