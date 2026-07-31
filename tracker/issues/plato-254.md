---
id: plato-254
title: Interval arithmetic (guaranteed-enclosure numeric type)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---

Proposed 2026-07-27 (agent idea, accepted by user for capture). Untriaged.

## Not the same as the existing interval types

`plato-src/interval.plato` and `plato-src-v3/12-intervals-bounds.plato` model *ranges as
containers* — `IntervalLike<T>` with Start/End, `NumberInterval`, bounds. This idea is the other
meaning: **interval arithmetic**, where a value is a guaranteed enclosure `[lo, hi]` and every
operation is outward-rounded so the true result is provably inside the result interval. Naming will
need care given the collision.

## Why

- **Provable bounds instead of guesses.** Ray/SDF marching, root isolation, curve-surface
  intersection, and collision culling all currently rely on tuned epsilons and Lipschitz guesses.
  Interval (or affine) arithmetic replaces those with a certificate: if the interval evaluation of
  f over a box excludes zero, there is no root in that box, full stop.
- **It composes with existing work.** Reuses the numeric concept lattice; pairs with robust
  predicates ([[plato-255]]) and exact rationals ([[plato-244]]); an interval is also the natural
  container for a tolerance policy.

## Design notes

- `type Interval { Lo: Number; Hi: Number }` implementing the arithmetic concepts, with correct
  handling of division by an interval containing zero (empty / split / infinite — pick a policy).
- **Rounding is the hard part.** A true enclosure needs directed rounding (round-down for `Lo`,
  round-up for `Hi`). .NET has no portable rounding-mode control, so the practical approach is to
  nudge by one ULP (`Math.BitIncrement`/`BitDecrement`) after each op. Cost and correctness of that
  choice should be measured, and per-backend behavior (GLSL/C++) checked — this may be C#-only at
  first.
- Consider **affine arithmetic** as a follow-on: tracks correlation between terms, so it avoids the
  dependency problem where `x - x` widens instead of collapsing to zero.
- Validate with the seeded `ValueGen` harness: for random inputs, assert the scalar result is
  inside the interval result (the containment law is the whole specification).
