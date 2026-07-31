---
id: plato-243
title: Fraction type (unit interval 0..1)
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

A `Fraction` type constrained to the closed unit interval [0, 1] — the natural type for
interpolation parameters, barycentric weights, probabilities, colour channels, and percentages.
Open questions: clamp vs. assert on out-of-range construction; whether arithmetic returns
`Fraction` (saturating) or a plain `Number`. Distinct from [[plato-244]].

**Note 2026-07-27 (found while researching [[plato-252]]):** `plato-src-v3/05-numbers.plato` already
declares `Proportion` ("a unitless scalar expected to lie in [0, 1]"), plus `Percent` and
`Probability`. Check what V3 already gives you before building anything — this may reduce to
"enforce the constraint / add the laws" rather than a new type.
