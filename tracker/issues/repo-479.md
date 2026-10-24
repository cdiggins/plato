---
id: repo-479
title: Resolve compiler and navigation baseline failures with performance validation
type: bug
status: in-progress
priority: p2
effort: L
risk: med
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

Resolve the seven compiler/navigation failures reproduced during repo-468. Use parallel Sol ownership tracks, separate reversible work commits and explicit integration/performance checks. Existing report edits and another mission's root scratch notes remain untouched.

## Done means

- [x] Compiler inventory, TypeScript emission and intrinsic-location failures are corrected under their language contracts.
- [x] Navigation assignment references and meaning-preserving simplification pass focused and corpus regressions.
- [x] Navigation rebuild passes a meaningful latency check without an arbitrary budget increase.
- [x] Full compiler/navigation checks and required mission gates are recorded, with repeated CLI performance comparison and independent review.

## Plan

See [the failure-fix plan](../../docs/reports/compiler-navigation-fixes-plan-2026-10-24.md). Individual work is tracked in compiler-472, compiler-475, compiler-477, compiler-478, plato-438 and stdlib-476.

## Verified outcome

[Results and reversible commits](../../docs/reports/compiler-navigation-fixes-results-2026-10-24.md) and [machine-readable evidence](../../docs/reports/compiler-navigation-fixes-evidence-2026-10-24.json) record the completed work. The full compiler suite passed 330 tests, navigation passed 73, and interpreter execution passed 35 selected cases. Original assemblies reproduce the new assignment/tuple regressions. Default and optimized foundation emission remain byte-equivalent after timestamp/line-ending normalization; an elevated optimized timing did not reproduce in five additional alternating pairs, so no speedup is claimed.

The required Studio battery ran once: both library lints, the SDK build and geometry tests pass. Its pre-existing frozen-artifact failure remains external under repo-473, with the same changed `PlatoList.cs`, `PlatoBuffer.cs` and `Number.cs` files. The local compiler/navigation acceptance is complete; cross-repository green status is not claimed. No outside artifacts, manifest baselines, ratchets or exclusions were changed.
