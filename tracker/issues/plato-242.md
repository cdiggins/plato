---
id: plato-242
title: Normal vector type (plus Unit Vector and Axis types)
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

Distinguish, in the type system, a general vector from:
- **UnitVector** — normalized, magnitude 1 by construction.
- **Normal** — a unit vector that transforms as a covector (inverse-transpose under a transform).
- **Axis** — a direction with no preferred sign (equivalent up to negation).

Payoff: normalization is guaranteed at the type level (no redundant `.Normalize()`), and
transform rules stop being a source of silent bugs. Relates to [[plato-241]].
