---
id: compiler-509
title: Explore exact allocation for fixed-output FlatMap
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/output-sizing.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [ ] Qualify a representative geometry FlatMap caller against the growing-list baseline before broadening eligibility or changing defaults.

## Production continuation â€” 2026-10-25

Actual TIR fixtures, retained intrinsic provenance and a guarded exact-allocation emitter are implemented in d81e6065. All 17 focused cases passed, including callback/read order, faults, mutation and fallback. Actual generated wrapper allocation fell 22.29% for the measured literal-width fixture. The default-off option is production code; geometry-wide benefit and broader callback shapes are not qualified.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The remaining checkbox is the next execution-plan gate; this item is not closed by fixture evidence alone.
