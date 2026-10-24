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

- [ ] Compiler inventory, TypeScript emission and intrinsic-location failures are corrected under their language contracts.
- [ ] Navigation assignment references and meaning-preserving simplification pass focused and corpus regressions.
- [ ] Navigation rebuild passes a meaningful latency check without an arbitrary budget increase.
- [ ] Full compiler/navigation checks and required mission gates are recorded, with repeated CLI performance comparison and independent review.

## Plan

See [the failure-fix plan](../../docs/reports/compiler-navigation-fixes-plan-2026-10-24.md). Individual work is tracked in compiler-472, compiler-475, compiler-477, compiler-478, plato-438 and stdlib-476.
