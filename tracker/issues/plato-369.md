---
id: plato-369
title: Burn down stdlib type-checker diagnostics to zero
type: debt
status: ready
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-31
closed:
links: [plato-023, docs/plato-tir-scalar-lowering-plan-2026-07-12.md, tests/PlatoTests/CheckerCompletenessTests.cs, tests/PlatoTests/CheckerDiagnosticsSummaryTests.cs]
---

Split from [plato-023](plato-023.md): Mission 1 only. Scalar lowering (Mission 2) already shipped.

## Issue

The shipping stdlib still has type-checker error diagnostics on a minority of functions.
`CheckerCompletenessTests` pins a ceiling of **25 / ~859** (measured 2026-07-29). The goal
of Mission 1 in the scalar-lowering plan was **0**.

## Impact

Residual CHK101 / CHK201 noise slows library edits, hides real regressions in the ratchet,
and leaves some monomorphized library bodies less precisely typed than they could be. It no
longer blocks scalar lowering (that path shipped), but it is still the compiler trust endgame.

## Affected code

- `tests/PlatoTests/CheckerCompletenessTests.cs` — ratchet ceiling `MaxFunctionsWithDiagnostics = 25`.
- `tests/PlatoTests/CheckerDiagnosticsSummaryTests.cs` — full per-function worklist printer.
- `Plato.Compiler/Checking/` — Solver / TypeChecker / Elaborator / Monomorphizer.
- Plan inventory: `docs/plato-tir-scalar-lowering-plan-2026-07-12.md` (Mission 1 / M1.3).

## Cause / analysis

As of the 2026-07-29 ratchet note, the residue is roughly:

- **CHK101** (~18) — cannot-unify, dominated by tuple→generic-interface returns the checker
  cannot soundly ground (`Tuple2<$T,$T>` vs `IInterval<$T>` / `IBounds<$T,$D>`). Often a
  **library redesign**, not a new checker rule.
- **CHK201** (~7) — no-match library repairs (`Meshes.Lines`/`Transform`, `Transforms.Quaternion`,
  `Vectors.Column`/`Dot`, Curves Bezier, Barycentric, etc.).

Ambiguity (CHK203) was already driven to zero under plato-023.

## Priority

p1 — remaining half of the old plato-023 endgame; cheap to regress against via the ratchet;
compounds as the forward stdlib grows.

## Dependencies

- Blocked by: nothing hard; some CHK101s may need stdlib signature changes.
- Blocks: cleaner typing for future lowerings; quieter agent loops on stdlib edits.
- Touches: checker + shipping `stdlib-legacy` (the ratchet corpus) and any shared Solver code
  the forward stdlib also uses.

## Fix approaches

1. **Library repairs first** for the CHK201 no-match list — smallest, local.
2. **Redesign tuple→interface returns** (concrete return types or helper interfaces) for the
   CHK101 cluster — more durable than teaching the checker to invent implementers.
3. **Checker rules only where the type is soundly inferable** (e.g. bare receiver-member refs)
   — avoid papering over ungroundable interface returns.

## Bedrock

Strengthen the invariant that every emitted/monomorphized body the writers consume carries
concrete, verifiable types (`TirTypeVerifier` stays at 0 hard). Prefer fixing declarations so
the Solver does not have to invent an implementer. **Verdict: simplest-along-the-grain** —
do not add a “pick any implementer” coercion that would make Mission-2-style type-directed
passes lie.

## Done means

- [ ] `CheckerCompletenessTests` ceiling is **0** (and the test still passes).
- [ ] `CheckerDiagnosticsSummaryTests` prints 0 functions with error diagnostics on the
      shipping stdlib corpus.
- [ ] `TirTypeVerifier` remains 0 hard violations.
- [ ] No new golden/output regressions from checker-only changes (or intentional rebaseline
      documented in the landing commit).

## Simplest fix

Burn the CHK201 library repairs one function at a time, lowering the ratchet each time; park
the CHK101 tuple→interface cluster as an explicit sub-decision if a checker change would be
unsound. Pros: measurable. Cons: some items need API shape changes, not just Solver tweaks.
