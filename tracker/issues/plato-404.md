---
id: plato-404
title: plato_simplify SIM001 rewrites tuples outside return position, breaking the type check
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: []
---

## Symptom

`plato_simplify` (the SIM001 rule: "the constructor name repeats the declared
return type, so the tuple alone says it") strips the type name from calls that
are *not* in return position. Applying all 161 SIM001 edits over the shipping
tiers left six `CHK101` type errors in three functions:

- `stdlib/foundation/axes.library.plato` — `Vector3D(self: SignedAxis3D)`, a
  `match` whose other arms are `Vector3D.UnitX` / `UnitY` / `UnitZ`:
  `Cannot unify 'Vector3D' with 'Tuple3<$G13,$G14,$G15>'` (x3, one per rewritten arm).
- `stdlib/foundation/color.library.plato` — `ColorAtParameter`, the true branch
  of a conditional: `Cannot unify 'Tuple4<...>' with 'Color'`.
- `stdlib/foundation/matrices-ops.library.plato` — `CanonicalAxis`, a nested
  conditional: `Cannot unify 'Vector3D' with 'Tuple3<...>'`.

In each case the sibling branch has the named type, so the branches no longer
unify. The three sites were restored by hand in `03c48ab`.

## Fix

SIM001 should only fire when the constructor call IS the function's result
expression: the whole body of an expression-bodied function, or the operand of
a `return`. A tuple in a `match` arm, a conditional branch, an argument
position or a `var` initializer has no declared type to fall back on.

Cheaper interim guard: have `apply` run the type gate afterwards and report
(or revert) the edits that introduced diagnostics, so the tool cannot leave the
corpus red.

## Done means

- [ ] SIM001 does not fire outside result position
- [ ] The three sites above stay simplified-or-untouched with `plato_check gates: "types"` clean
