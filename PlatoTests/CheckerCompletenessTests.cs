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
        // Measured 2026-07-29 after the tuple-coercion checker fixes (Solver.cs): the return-position
        // coercion no longer prematurely binds an unresolved tuple result var to the declared struct,
        // and MatchArg now accepts a same-shape tuple literal for a struct PARAMETER (the Scale((x,y,z))
        // family). Both mirror generated-C# tuple-constructor implicit operators, so no checker rule was
        // weakened and no overload winner changed. 32 -> 26 / 859 remain — CHK101 (cannot-unify, 18) +
        // CHK201 (no-match, 8): the tuple->GENERIC-INTERFACE returns the checker cannot soundly ground
        // to a concrete implementer (Tuple2<$T,$T> vs IInterval<$T> / IBounds<$T,$D> — a library
        // redesign, not a checker rule), plus library repairs (Meshes.Lines/Transform, Transforms.
        // Quaternion, Vectors.Column/Dot, Curves2D/3D Bezier, Barycentric, ScalarFields3D Function-field
        // invocation). The count drifts with library growth, so the baseline is a ceiling to lower.
        private const int MaxFunctionsWithDiagnostics = 26;

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
