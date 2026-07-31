---
id: plato-226
title: Explore Flow augmentation type in Plato + presheaf-style channel transport maps
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-25
closed:
links: [studio-220, ara3d-sdk/src/Ara3D.Flow/FlowObject.cs, labs/platoflow, plato-134]
---

## Idea
Two connected explorations spun off studio-220 (Flow<T> lens for the Studio pipeline):

1. **Flow as a Plato-expressible type.** PlatoFlow (visual language, labs/platoflow) maps to/from Plato; `Flow<T>` = `T × A` (content plus channel data) is the currency at graph edges. Question: does Plato need a special construct to "augment arbitrary X with arbitrary data", or is `Flow<T>` definable as an ordinary library type (product + functorial map) given generics + concepts? Working position from studio-220 discussion (2026-07-25): library type; special-casing buys only auto-lifting (implicit `T` to `Flow<T>`), which the PlatoFlow assembler can do by inserting lift/extract nodes at graph edges during assembly — keeps the language core small. Revisit only if row polymorphism / extensible records enter Plato anyway; the general PL answers here are extensible records or graded comonads (coeffects).

2. **Presheaf-style channel transport maps.** Categorically, `Flow` is the Writer monad over the channel-list monoid (unit = lift with empty channels, join = channel append) and a product comonad (extract = Content). The deeper structure: attribute domains (vertex/face/edge/corner/instance) form a category of index sets + reindexing maps induced by modifiers; an attribute is data over an index set and should transport contravariantly along the reindexing map — a presheaf. Today's `FlowAttribute.AttributeDomainMask` drop rule is the admission that transport maps aren't tracked: can't transport, so drop. If modifiers optionally emitted index correspondences (old-vertex to new-vertex map), channels would transport automatically instead of being dropped — the principled fix for stale attributes, and it upgrades `map` from lax-functorial (worst-case drops) to honest.

Capture-only per studio-220 spin-off; elaborate before promotion.