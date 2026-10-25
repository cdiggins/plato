---
id: compiler-510
title: Explore loop splitting on invariant conditions
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/loop-unswitch.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [x] Validate the capture-aware immutable-Boolean analysis on a representative workload and measure JIT benefit before production unswitching.

## Production continuation â€” 2026-10-25

The committed secondary census finds an invariant Boolean parameter in an actual MapRange callback and shows capture rewriting introduces a distinct local binding. Variant comparisons and faulting delegate conditions are refused. Production loop duplication is deferred pending capture-aware proof and measured benefit.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The remaining checkbox is the next execution-plan gate; this item is not closed by fixture evidence alone.

## Geometry qualification

Actual TIR proves the exact immutable Boolean binding through an identity coercion. A private emitted-loop split preserves count conversion, allocation and both branch bodies; focused generated-code checks pass. The bounded timing comparison found no consistent speed benefit, equal allocation and larger emitted source. This completes the exploration with a negative promotion decision: no production unswitch pass or flag. See [geometry evidence](../../docs/reports/optimizer-geometry-loop-unswitch-2026-10-25.md).
