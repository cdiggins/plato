---
id: plato-258
title: PGA meet/join incidence library — collapse the intersect-per-type-pair table
type: idea
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

Proposed 2026-07-28 (agent idea; user asked for it to be captured after a discussion of geometric
algebra). Untriaged.

## The problem it solves

Incidence and intersection currently need one function per type pair. `plato-src-v3/16-lines.plato`
alone declares `Line2D`, `Line3D`, `Ray2D`, `Ray3D`, `LineSegment2D/3D/4D`, `LineEquation2D`,
`Plane`, `HalfSpace`, `HalfPlane2D`, `Slab2D`, `Slab3D` — and every meaningful pair wants its own
intersect routine, each with its own parallel/degenerate branch and its own epsilon. That table
grows quadratically and every cell is a place for an inconsistent answer to hide.

Projective geometric algebra (PGA) replaces the table with two operators:

- **meet** — intersection. plane-meet-plane = line, line-meet-plane = point, three planes = point.
- **join** — spanning. point-join-point = line, point-join-line = plane.

Same operator regardless of operand types, and degenerate cases produce meaningful elements
instead of division by zero: two parallel planes meet in a line at infinity, which is a real
element of the algebra that downstream code can test for rather than a NaN to guard against.

## Why this is the right slice of GA to take

Explicitly **orthogonal to the transform representation**. It does not ask `Transform3D` to stop
being TRS/matrix-canonical, does not ask rotation to stop being quaternion-canonical (see
[[plato-260]]), and does not require adopting rotors anywhere. It is an additive query library.
That makes it the cheapest way to find out whether GA earns its keep in this codebase.

## Sketch

- 3D PGA is the Clifford algebra over basis e0,e1,e2,e3 where e0*e0 = 0 — that degenerate basis
  vector is what buys translations and elements at infinity. Elements needed: planes (grade 1),
  lines (grade 2), points (grade 3).
- Provide conversions at the boundary in both directions, so the existing `Plane` / `Line3D` /
  `Point3D` types keep their current shape and callers opt in.
- Start with the handful of intersections that already have hand-written implementations, and
  check the PGA versions against them on random and degenerate inputs (seeded `ValueGen`).

## Open questions

- **Naming.** "meet"/"join" vs `Intersect`/`Span`. The wedge symbols are not available as
  operators; pick names that read well in Plato's method-call style.
- **Normalization and weights.** PGA elements carry a weight, and a point with zero weight is at
  infinity. Decide whether the public API normalizes eagerly or exposes the weight.
- **Cost.** A general multivector product touches many components; the useful products are sparse
  and want specialized implementations. Measure before claiming parity with hand-written routines.
- **Backends.** Check what the GLSL/C++ writers make of the resulting code shape.
- Relationship to robust predicates ([[plato-255]]): meet/join gives cleaner *formulas*, not exact
  *signs*. The two are complementary — do not expect this to fix floating-point degeneracy.
