---
id: compiler-511
title: Explore reuse of neighboring grid samples
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/grid-reuse.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [ ] Establish a lowered nested schedule, resolved callback alias identity and totality contract before proposing production grid caching.

## Production continuation â€” 2026-10-25

The committed secondary census shows only the outer rectangular MapRange is lowered; the inner range remains a callback runtime call and sampling uses separate capture aliases. Production automatic caching is deferred because nested scheduling, alias identity and callback totality are unproved.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The remaining checkbox is the next execution-plan gate; this item is not closed by fixture evidence alone.

## Geometry qualification

The frozen current compiler lowers both rectangular MapRange levels, removing the earlier structural barrier. Raw callback identity and a narrow polynomial totality check pass. Capture aliases, coordinate conversions, domain equality and complete call-site totality remain unproved, so production caching and timing remain deferred. The remaining criterion stays open. See [geometry evidence](../../docs/reports/optimizer-geometry-grid-reuse-2026-10-25.md).
