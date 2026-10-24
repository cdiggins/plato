---
id: compiler-484
title: Plan private storage for recursive numeric kernels
type: feature
status: idea
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/scratch-storage.md]
---

Reviewed staged opportunity: exact self tail-call lowering, scoped recurrence visibility, private alternating buffers, then dependence-proved in-place reuse. Tail conversion alone removes no arrays. CR-06/07 and sequencing investigation are prerequisites; stack allocation, pooling and repeated-array specialization remain deferred.

## Done means

- [ ] Specify and verify exact self-call, capture, state-update and resource-exhaustion boundaries before tail lowering.
- [ ] Prove ownership, logical bounds, overflow and borrowed-input preservation before private buffer reuse.
- [ ] Demonstrate allocation benefit and equal numeric results on representative recursive kernels before broader deployment.
