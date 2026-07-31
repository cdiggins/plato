---

Build a coherent, declaration-only concept system for every domain represented in
`submodules/Plato/plato-src-v2`. Concepts are the reusable abstraction boundary for
future Plato libraries: generic functions should target capabilities rather than
individual records wherever the operation is genuinely shared.

## Design

- Restore foundational semantics lost from the old source, including affine
  differences, intervals, containment, component access, and typed dimensionality.
- Prefer small capability concepts with useful function signatures over classification
  markers or deep nominal hierarchies.
- Layer relationships from value/algebra/collections through geometry and fields, then
  animation, imaging, rendering, physics, engineering, statistics, optimization,
  signals, uncertainty, and scientific data.
- Assign concepts to concrete types throughout the catalog so the lattice is exercised
  rather than merely declared.
- Keep this phase declaration-only: implementations, laws, and libraries are follow-up
  vertical slices.

## Case against

A concept for every noun would create ceremony without reuse, while a dense inheritance
graph can produce ambiguous function groups and make monomorphization difficult. Many
domain records are metadata rather than algorithmic values and should not be forced
into capabilities they cannot satisfy. Plato also lacks sum types and associated types,
so some ideal relationships cannot yet be represented directly.

**Verdict: pursue a capability-driven lattice.** Add a concept only when it expresses
shared semantics or enables reusable generic functions; use inheritance only for true
substitutability, and leave deliberately passive metadata records as plain values.

## Done means

- [ ] Every domain in `plato-src-v2` has an explicit, composable concept vocabulary.
- [ ] Foundational semantics removed from the old source have principled successors.
- [ ] Concrete types are assigned all applicable concepts without known false claims.
- [ ] Concept relationships and function signatures support future generic libraries.
- [ ] The complete declaration-only source parses and resolves with zero errors.
id: plato-229
title: Complete the Plato v2 concept lattice
type: feature
status: in-progress
priority: p1
effort: L
risk: high
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---
