---
id: compiler-487
title: Evaluate concrete storage and callback specialization
type: feature
status: idea
priority: p3
effort: L
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/concrete-storage.md]
---

Reviewed experiment: preserve logical IReadOnlyList locals and public ABI, use exact backing arrays for proven internal loop reads only. CR-09 requires an eligibility census and reachable fixture, such as separately lowered Zip sources, before implementation. Callback work depends on the later shared clone registry, not just statement inlining; start noncapturing. Struct functors and spans remain deferred.

## Done means

- [ ] Find eligible surviving producer/consumer shapes without relying on deferred alias propagation.
- [ ] Preserve inference, overload identity, logical interfaces and physical element types in compiled regression tests.
- [ ] Measure JIT/code benefit independently; add callbacks only through shared specialization with an established capture contract.
