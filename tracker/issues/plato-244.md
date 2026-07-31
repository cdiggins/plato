---
id: plato-244
title: Rational number type (exact p/q arithmetic)
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

User idea, captured 2026-07-27.

Exact rational arithmetic as `p/q` with integer numerator/denominator, auto-reduced. Useful for
exact predicates in geometry, unit conversions, and reproducible results without floating-point
drift. Pairs with [[plato-246]] for unbounded numerators/denominators. Distinct from the unit-interval
[[plato-243]].

**Note 2026-07-27 (found while researching [[plato-252]]):** `plato-src-v3/05-numbers.plato` already
declares `type Rational { Numerator: Integer; Denominator: Integer }` (and `Complex`). Scope this
against V3 first — likely the real work is arithmetic + reduction + laws, not the type itself.
