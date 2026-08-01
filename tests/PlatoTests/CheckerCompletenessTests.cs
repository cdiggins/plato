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
        // Measured 2026-07-29 after the function-typed-FIELD-invocation checker fix (Solver.cs,
        // TryResolveFieldInvocation): a UFCS call whose "callee" names a field of function type
        // (`self.Function(point)` -> the normalized call `Function(self, point)`) now types by INVOKING
        // the accessor's `Function{N}` result against the trailing arguments, rather than failing CHK201.
        // Strict fallback (only when no ordinary overload matched), so no winner changed; records nothing
        // in ResolvedCalls, so the emitted `self.Function(point)` delegate invocation is byte-identical.
        // Cleared the ScalarFields3D Function-field `Eval`, taking 26 -> 25 / 859. Remaining — CHK101
        // (cannot-unify, 18) + CHK201 (no-match, 7): the tuple->GENERIC-INTERFACE returns the checker
        // cannot soundly ground to a concrete implementer (Tuple2<$T,$T> vs IInterval<$T> / IBounds<$T,$D>
        // — a library redesign, not a checker rule), plus library repairs (Meshes.Lines/Transform,
        // Transforms.Quaternion, Vectors.Column/Dot, Curves2D/3D Bezier, Barycentric). The count drifts
        // with library growth, so the baseline is a ceiling to lower.
        //
        // RE-PINNED 25 -> 26 on 2026-08-01 (plato-382 phase D). Nothing in this corpus changed and no
        // body got worse: CHK205 — a call on a bounded type parameter that no declared bound supplies —
        // was promoted from warning to ERROR, and `IInterval.Size` was the one legacy function whose
        // only diagnostic was that warning. (`IInterval.MoveTo` and `IInterval.Recenter` carry the same
        // warning but were already failing CHK101, so they did not move the count.) All three
        // are the SAME under-promise: `interface IInterval<T> where T: IVectorLike`
        // (legacy/stdlib-legacy/core.interfaces.plato) whose bodies Add/Subtract two bare `T`, which
        // IVectorLike does not supply. It cannot be fixed by adding `T: IAdditive`: `IInterval<Point2D>`
        // (Line2D) would then be CHK309, because Point2D subtracts to a Vector2 rather than to itself —
        // it is the same IInterval/IBounds redesign already named above, not a new defect. LOWER this
        // when that redesign lands; do not raise it again.
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
