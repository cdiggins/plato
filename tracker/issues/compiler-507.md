---
id: compiler-507
title: Explore elimination of temporary records
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/optimizer-exploration/record-elimination.md]
---

User-authorized optimizer exploration. Contract revision O1: dedicated agent, isolated experiment, critical review, execution plan, and a bounded first implementation. Shared compiler changes require supervisor ownership approval.

## Done means

- [x] Elaborate the opportunity against current source and existing proposals.
- [x] Run a reproducible experiment with correctness checks and record limitations.
- [x] Critically review legality, benefit, and implementation risks.
- [x] Write an ordered execution plan and execute its first bounded step.
- [x] Review and commit deliverables; distinguish prototype evidence from production integration.

- [x] Execute the actual Plato TIR fixture milestone and record the bounded production integration decision.
- [ ] Establish a representative surviving cost or downstream optimization benefit, then review a typed ordered-binding projection slice before production rewriting.

## Production continuation â€” 2026-10-25

The committed secondary census parses actual Plato and identifies property-call over constructor-call, using resolved field identity. The unused faulting constructor argument is retained. Production replacement is deferred because the earlier JIT microkernel found no saving and no representative surviving cost is established.

Evidence: [production report](../../docs/reports/optimizer-production-2026-10-25.md) and [secondary TIR census](../../docs/reports/optimizer-secondary-tir-2026-10-25.md). The remaining checkbox is the next execution-plan gate; this item is not closed by fixture evidence alone.

## Geometry qualification

A private actual-TIR projection rewrite preserves all constructor arguments in ordered typed bindings. Generated baseline and transformed geometry kernels pass bit-pattern, callback-order, first-fault and refusal checks. Critical review found an unused lambda-parameter naming collision; the focused regression now compiles and executes both variants with a fresh local name. Representative surviving cost remains unmeasured, so the production-benefit criterion stays open and no production pass is added. See [geometry evidence](../../docs/reports/optimizer-geometry-record-elimination-2026-10-25.md).
