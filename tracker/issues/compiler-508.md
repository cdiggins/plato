---
id: compiler-508
title: Explore array length and index range analysis
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/array-facts.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [x] Implement and test the planned read-only producer/alias fact adapter on representative callers before any bounds or length rewrite.

## Production continuation â€” 2026-10-25

The committed secondary census identifies completed loop results, TirTempRef and local aliases in actual Plato TIR. It demonstrates why owned length facts need resolved producer/alias provenance and why foreign collection facts must be refused. No bounds or count rewrite was added.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The geometry adapter qualification below completes this bounded analysis exploration.

## Geometry adapter qualification — 2026-10-25

The private read-only adapter recognizes completed canonical producers and local aliases on an actual geometry sample-row kernel. Focused identity, capture, escape, assignment, coercion and scope refusals pass; emitted C# is unchanged. It connects Count to the producer saved normalized length without dropping materialization. Independent review accepts the planned adapter milestone. Keep this analysis private until a production consumer demonstrates benefit; no runtime speedup or bounds elimination is claimed.

Evidence: [geometry report](../../docs/reports/optimizer-geometry-array-facts-2026-10-25.md).
