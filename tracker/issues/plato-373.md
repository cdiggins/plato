---
id: plato-373
title: Ellipse.Perimeter (Ramanujan II) is 4e-4 off for flattened ellipses
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-31
closed:
links: []
---

## Issue

`Perimeter(e: Ellipse)` in `stdlib/geometry/planar-ellipses.library.plato` is Ramanujan's
second approximation. Its doc comment claimed the worst case was `3.99969*a` at `b = 0`
("about 7.5e-5") and "for any ellipse with b/a >= 0.1 the relative error is below 1e-8".
Both numbers were wrong. Measured 2026-07-31 against a converged (2e6-panel Simpson,
double-precision) arc-length integral:

| b/a   | relative error |
|-------|----------------|
| 1     | 1e-16          |
| 0.5   | -4.6e-10       |
| 0.4   | -6.3e-9        |
| 0.3   | -7.4e-8        |
| 0.1   | -1.2e-5        |
| 0.01  | -2.4e-4        |
| 0.001 | -3.8e-4        |
| 0     | -4.0e-4 (the limit is (14/11)*pi*a = 3.99838*a, not 3.99969*a) |

The doc comment was corrected in place (commit landing with plato-308's law work), so the
library no longer *lies* about its accuracy. The underlying inaccuracy is untouched.

Consequence beyond documentation: `Law_SuperEllipseExponentTwoPerimeterIsEllipse`
(`tests/stdlib-tests/special-numerics.laws.plato`) compares the superellipse's Simpson
integral at Exponent 2 — which is accurate to float32 precision, ~4e-7 relative — against
this approximation. The law's tolerance is therefore pinned at 1e-3 relative by the ellipse
side alone. Every 1e-6-class statement about ellipse perimeters is blocked on this body.

## Fix approaches

1. Cheapest: keep Ramanujan II for `b/a >= 0.4` and switch to the exact `4*a*E(e)` complete
   elliptic integral of the second kind via the arithmetic-geometric mean (a handful of AGM
   iterations, no series truncation) below it. AGM converges quadratically and is already the
   standard route.
2. Uniform: replace the body with the AGM/Carlson `RG` form everywhere and drop the
   approximation entirely. Costs a loop where there is now a closed form; check whether the
   GLSL/C++ writers tolerate that before choosing it.
3. Do nothing but keep the corrected comment, and let callers who need accuracy integrate.

Whichever is chosen, tighten the law's 1e-3 tolerance back toward 1e-6 in the same change and
say so in the law comment (it names this issue).

## Done means

- [ ] `Ellipse.Perimeter` relative error is below 1e-8 for every `b/a` in (0, 1].
- [ ] The doc comment states the new measured bound, not an estimate.
- [ ] `Law_SuperEllipseExponentTwoPerimeterIsEllipse` is back to a 1e-6-class tolerance and
      still passes over the full semi-axis regime.
