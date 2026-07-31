---
id: plato-255
title: Robust geometric predicates (exact/filtered orientation, incircle, intersection)
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

## Why

Floating-point sign errors in geometric predicates do not produce slightly-wrong answers; they
produce inconsistent topology — CSG that leaks, triangulations that fail to close, hulls that are
not convex. The failures are input-dependent and miserable to debug after the fact. `csg/` and
`earcut/` in this repo both quietly depend on getting these signs right.

## Scope

The classic predicate set, each returning an exact sign:

- `Orient2D(a, b, c)` — is c left of, right of, or on line ab.
- `Orient3D(a, b, c, d)` — which side of plane abc is d.
- `InCircle` / `InSphere` — Delaunay and circumsphere tests.
- Segment/segment and segment/triangle intersection classification built on the above.

## Approach

Shewchuk's staged, adaptive scheme is the standard answer and is the right target:

1. Compute the cheap floating-point determinant plus a rigorous error bound.
2. If the magnitude exceeds the bound, the sign is certified — return immediately. This is the
   overwhelmingly common path, so typical cost stays near a plain float evaluation.
3. Otherwise escalate to exact expansion arithmetic (or to [[plato-244]]/[[plato-246]] exact
   rationals) until the sign is certain.

Interval arithmetic ([[plato-254]]) is the natural implementation of step 1-2: evaluate the
determinant over intervals and escalate only when the result interval straddles zero.

## Open questions

- **Purity vs. the fast path.** The adaptive escalation is a data-dependent branch, which is fine
  in Plato, but the exact stage wants expansions (arrays of floats) — check what that costs a pure
  value-type library, and what GLSL/C++ backends can do (a GPU backend likely gets the filtered
  stage only).
- **Degeneracy policy.** Exact zero is a legitimate answer (collinear/coplanar). Downstream
  algorithms need a consistent tie-breaking rule — symbolic perturbation (simulation of simplicity)
  is the usual choice, and it is a separate decision worth writing down.
- **Validation.** Seeded `ValueGen` plus deliberately degenerate constructions (collinear,
  cocircular, near-miss at 1 ULP), checked against exact rational evaluation as the oracle.
