---
id: compiler-486
title: Specialize constant arguments without changing calls
type: feature
status: idea
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/specialization.md]
---

Reviewed sequence: straight-line statement inlining, narrow Integer/Boolean propagation, then only justified clone sharing, common-subexpression elimination and invariant motion. CR-05/08/10 constrain identity, eager actuals, result conversions and shared callback specialization. Resolve or exclude plato-374 cases. Compare with existing --inline and JIT behavior.

## Done means

- [ ] Define exact call identity, type/conversion preservation and cumulative expansion limits.
- [ ] Verify eager actuals, cloned bindings and conservative folding/refusal on actual generated C#.
- [ ] Measure incremental benefit over the same recipe with existing --inline before claiming allocation or throughput gains.
