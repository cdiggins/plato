using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Reporting + gate fixture for <see cref="TirTypeVerifier"/> — the mechanical definition of
    /// "the emitter can trust the TIR" that the scalar-lowering endgame
    /// (docs/plato-tir-scalar-lowering-plan) depends on. Prints the per-rule census of type
    /// inconsistencies over every emitted body so the residue is a measured worklist, and pins the
    /// hard-violation count against a baseline that ratchets DOWN as the checker is completed (never
    /// up — a regression fails here). When it reaches zero, Mission 2 (TirScalarLowerer) is unblocked.
    /// </summary>
    [TestFixture]
    public static class TirTypeVerifierTests
    {
        // Ratchet baseline. TIGHTEN as checker completeness lands; never loosen. Measured
        // 2026-07-12 after the all-variable-return tie fix (Quad3D.Normal's residual var, via the
        // now-resolved Average→Lerp, is gone): ZERO hard violations — R1 null-type, R2 unresolved,
        // R4 arg/param mismatch, R5 coerce-inconsistency and R6 residual-type-var are all empty.
        // The emitted TIR is fully type-consistent; Mission 2 (TirScalarLowerer) is unblocked on this
        // axis. R3 (syntactic null-callee calls) is tracked but NOT gated here — the handwritten-
        // intrinsic census (14 names: Add/Subtract/Scale/…), the same worklist as the checker's
        // remaining CHK201/203, shrinking as those close.
        private const int MaxHardViolationsExcludingSyntactic = 0;

        [Test]
        public static void VerifyEmittedTirIsTypeConsistent()
        {
            var verifier = new TirTypeVerifier(CheckerTestSupport.CompileStdLib());
            var violations = verifier.Verify();

            var byRule = violations.GroupBy(v => v.Rule).OrderByDescending(g => g.Count());
            TestContext.WriteLine($"total violations: {violations.Count}");
            foreach (var g in byRule)
                TestContext.WriteLine($"  {g.Key}: {g.Count()}");

            TestContext.WriteLine($"informational: Self-residue bodies {verifier.SelfResidueBodies}, "
                + $"type-parameter-residue bodies {verifier.TypeParameterResidueBodies}");
            TestContext.WriteLine($"distinct syntactic-call (null-callee) names: {verifier.SyntacticCallNames.Count}");
            foreach (var n in verifier.SyntacticCallNames.OrderBy(x => x))
                TestContext.WriteLine($"    {n}");

            // A sample of each hard rule's offenders (deduped), so the worklist is legible.
            foreach (var g in byRule.Where(g => g.Key != TirTypeVerifier.TirVerifyRule.R3_SyntacticCall))
            {
                TestContext.WriteLine($"-- {g.Key} sample --");
                foreach (var v in g.Select(v => $"{v.Scope}: {v.Detail}").Distinct().Take(25))
                    TestContext.WriteLine($"    {v}");
            }

            var hard = violations.Count(v => v.Rule != TirTypeVerifier.TirVerifyRule.R3_SyntacticCall);
            TestContext.WriteLine($"hard violations (excl. syntactic): {hard}");

            Assert.LessOrEqual(hard, MaxHardViolationsExcludingSyntactic,
                "TIR type-consistency regressed; the verifier found more hard violations than the ratchet baseline.");
        }
    }
}
