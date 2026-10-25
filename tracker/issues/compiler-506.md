---
id: compiler-506
title: Explore fusion of multiple array consumers
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/multi-consumer.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [ ] Qualify a representative geometry caller and record measured benefit before widening fusion eligibility or changing defaults.

## Production continuation â€” 2026-10-25

Actual TIR fixtures, canonical identity checks, marker/emitter integration and compiled off/on execution are implemented in d81e6065; f0f63d81 restores the independently reviewed traversal after a shared-file race. The default-off production slice preserves producer evaluation and per-fold arithmetic order. All 13 focused fusion tests passed; the combined non-diagnostic suite passed 393 tests. Generated fixture timing shows only a modest noisy gain, not geometry qualification.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The remaining checkbox is the next execution-plan gate; this item is not closed by fixture evidence alone.
