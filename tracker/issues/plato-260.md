---
id: plato-260
title: Settle the Rotor3D-vs-Quaternion story (one canonical rotation algebra)
type: problem
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: []
---

Proposed 2026-07-28 (agent idea; user asked for it to be captured). Untriaged.
Type is **problem** — this wants a decision recorded, not a feature built.

## The situation

`plato-src-v3/10-rotations.plato` declares both:

- `Quaternion { X, Y, Z, W }` — and the file header already states it is canonical for 3D
  composition and interpolation, with the other representations serving "authoring, interop, and
  geometric-algebra styles".
- `Rotor3D { Scalar: Number; Bivector: Bivector3D }` — plus `Rotor2D`, `Bivector2D`, `Bivector3D`.

`Rotor3D` and `Quaternion` are the same algebra: four numbers, identical composition rule, with
rotor components named by the plane they span (`YZ`, `ZX`, `XY`) instead of by `i`, `j`, `k`.
A grep finds `Rotor` only in type declarations (`10-rotations.plato`, `69-higher-dimensions.plato`)
— there is no operation library behind these types today.

## Why it needs settling

Right now the types are inert, so nothing is broken. The failure mode is drift: someone eventually
writes rotor operations, and the library ends up carrying two parallel rotation algebras that are
mathematically identical, each needing its own laws, conformance entries, optimizer coverage, and
conversions to everything else. Doubled surface, zero new capability.

## The decision to make

Recommended (discussed with the user 2026-07-28): **Quaternion stays the single canonical rotation
algebra. `Rotor3D`/`Rotor2D` remain an interop and teaching view — conversions to and from
Quaternion, and no independent operation library.** A rotor buys nothing as a stored
representation; the genuine geometric-algebra wins lie elsewhere and are tracked separately as
[[plato-258]] (meet/join incidence) and [[plato-259]] (motors for rigid-motion interpolation).

Alternatives worth naming before this is closed:

1. **Delete the rotor types.** Cleanest, but discards a legitimate authoring style and pre-empts
   [[plato-259]], which wants bivectors anyway.
2. **Flip canonical to `Rotor3D`.** Better-motivated component names and cleaner sign conventions,
   but every consumer, file format, and engine speaks quaternion — migration cost, no capability
   gain.
3. **Recommended: keep both, one canonical, conversions only.**

`Bivector3D` should be kept regardless — it is the honest replacement for the cross-product
pseudovector, and [[plato-259]] needs it.

## Done means

- An ADR in `tracker/decisions/` recording the choice and the alternatives rejected.
- The doc comments in `10-rotations.plato` state the rule explicitly, so the next person does not
  have to re-derive it.
