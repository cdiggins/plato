---
id: plato-431
title: ToInteger truncates where CONVENTIONS.md says it rounds, and it changes derived counts
type: bug
status: ready
priority: p2
effort: S
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`stdlib/CONVENTIONS.md` states that `ToInteger` **rounds**. The TypeScript writer
emits it as `Math.trunc`. Nobody noticed until a body divided and then converted,
because for the common `.Floor.ToInteger` / `.Ceiling.ToInteger` spellings the
mode is a no-op.

Found by the lattices demo (plato-421) driving `UniformLattice`, and independently
flagged by the demo prelude agent as the one place it declined to change behaviour
library-wide under six concurrent sessions.

## The concrete failure

`CellDivisions(extent, size) = (extent / size).ToInteger.Max(1)` decides how many
cells a lattice gets on each axis. Truncation gives a non-longest axis one cell
fewer than the nearest-cubic answer whenever the quotient's fractional part is at
or above one half:

- A 3 x 1 x 2 box at 5 divisions targets a cell size of 0.6. The y quotient is
  1.667, so y comes out **1** rather than 2 — cell size (0.6, 1.0, 0.667), aspect
  **1.67**. Rounding gives counts (5, 2, 3) and aspect **1.2**.
- Same shape of error on a 1.2 x 3 x 1.2 box at 4 divisions.

The lattice is not wrong — it tiles correctly at whatever counts it gets — but
`UniformLattice`'s documented promise is cells "as near cubic as whole counts
allow", and truncation does not deliver that.

## Which side is wrong

Unresolved, and that is the decision this issue needs before anyone edits code.
Two defensible answers:

1. **The writer is wrong.** `CONVENTIONS.md` is normative and says rounds; the
   C# and TypeScript backends should agree with it, and today they may not agree
   with each other either — worth checking the C# emission before deciding.
2. **The convention is wrong.** Truncation toward zero is what most languages
   give and what a reader of `(a / b).ToInteger` on integers expects. If that is
   the intent, `CONVENTIONS.md` should say truncates, and every body relying on
   rounding must be found and respelled.

Either way the fix is cheap; picking the wrong side quietly is what is expensive,
because the failure mode is an off-by-one in a derived count rather than an error.

## Simplest fix

Decide the semantics, then make the two backends and `CONVENTIONS.md` agree.
Whichever way it goes, `CellDivisions` should spell its intent explicitly
(`.Round.ToInteger` or `.Truncate.ToInteger`) rather than relying on the default,
since that call site is the one with a documented promise about its output.

## Done means

- [ ] The intended semantics of `ToInteger` is decided and recorded
- [ ] `CONVENTIONS.md`, the C# writer and the TypeScript writer agree with it
- [ ] `CellDivisions` spells its rounding intent explicitly
- [ ] Bodies relying on the other mode are found and respelled
