---
id: plato-415
title: "Expand the polygon library: triangulation, booleans, offset, predicates, queries"
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-08-02
closed:
links: [stdlib/geometry/polygons.types.plato, stdlib/geometry/polygons.library.plato, stdlib/geometry/planar.types.plato, stdlib/geometry/geometry.library.plato, stdlib/geometry/solids-csg.library.plato, experiments/earcut/earcut.plato, experiments/earcut/earcut-fast.plato, plato-273, plato-336, plato-028]
---

## Idea

`polygons.types.plato` declares a good vocabulary — `Polygon2D`, `ConvexPolygon2D`,
`PolygonWithHoles2D`, `PolygonSet2D`, `RegularStar2D`, the three polylines, `Polygon3D`,
`Prism3D`, `Pyramid3D` — and `polygons.library.plato` fills the measurement obligations
(area, perimeter, centroid, bounds, containment, nearest point, support, deform, volume,
surface area). What is missing is everything that *constructs* or *transforms* a polygon.
This issue is the umbrella for that gap: a catalogued backlog of polygon operations, each
line a candidate for its own issue when it graduates. It does not propose one change; it
proposes the map.

The gap is visible in the type list itself: `PolygonSet2D` is documented as "the result
shape of Boolean operations over polygons" and no Boolean operation exists. `ConvexHull2D`
exists as a type (`geometry.library.plato:214`) with no operation producing one from a
polygon. Every downstream consumer that needs triangles today has to leave the tree.

### The catalogue

1. **Triangulation** — LANDED in `stdlib/geometry/triangulation.library.plato` across every
   polygon type: `Polygon2D`, `ConvexPolygon2D`, `PolygonWithHoles2D` and `PolygonSet2D` as
   the `IMesh2D` obligation, `Polygon3D` as `IMesh3D`. Executable gate:
   `tools/regen-triangulation.ps1`. Still open: triangle strips, and a quality-driven mesher
   (Delaunay refinement) rather than ear clipping — plus the performance work in plato-417.
2. **2D Booleans** — `Union` / `Intersection` / `Difference` / `Xor` returning
   `PolygonSet2D`. The declared result type already commits to the shape.
3. **Offset / buffer** — `Offset(Polygon2D, distance, joinStyle)` with miter/round/bevel
   joins; inset for shells, outset for clearance; polyline stroking for `paths.types.plato`.
4. **Convex hull and decomposition** — `HullOf(Polygon2D): ConvexPolygon2D` (the existing
   `ConvexHull2D` type is the natural intermediate), `IsConvex(Polygon2D)`,
   Hertel–Mehlhorn convex partition.
5. **Predicates and repair** — LANDED in `polygons.library.plato`: `IsSimple`,
   `SelfIntersectionCount`, `Winding`, `EnsureCounterClockwise`, `RemoveDuplicateVertices`,
   `RemoveCollinearVertices`, `Canonical`, plus `IsSimple` / `HolesLieInside` / `Canonical`
   over `PolygonWithHoles2D`. `Polygon2D`'s "no edge crossings" is now checkable. Still open:
   the O(n^2) all-pairs test wants a sweep line, and repair cannot fix a crossing (it would
   change the region), so a self-intersection RESOLVER is separate work.
6. **Simplify and resample** — Douglas–Peucker, Visvalingam, `ResampleByArcLength`,
   Chaikin smoothing over the polylines.
7. **Distance and intersection queries** — `SignedDistance(Polygon2D, Point2D)` (negative
   inside; pairs with the SDF vocabulary landed in plato-409/plato-411), winding-number
   containment as a robust alternative to the even-odd rule the library fixes today, GJK
   over `ConvexPolygon2D` (`ISupport2D` is already implemented), polygon/segment/ray
   intersection.
8. **Further mass properties** — second moment of area, minimum-area bounding rectangle
   (rotating calipers, feeding `OrientedBox2D`), minimum enclosing circle, diameter,
   compactness.
9. **Sampling and parametrization** — `PointAtParameter(Polyline2D, t)`, `Tangent`,
   `SampleUniform`, area-weighted `RandomPointInside` (needs triangulation).
10. **More sweeps** — `Frustum3D`, `Loft(Polygon3D, Polygon3D)`, sweep along a
    `Polyline3D`, `Revolve(Polygon2D, Angle)`. `Prism3D` and `Pyramid3D` are the only two
    polygon-derived solids today.
11. **Lift and flatten** — `Lift(Polygon2D, Plane): Polygon3D` and the inverse, sharing the
    `PlaneTangent` / `PlaneCoordinates` basis already in `polygons.library.plato`.
12. **Star and regular-polygon bridges** — true Schläfli `{n/m}` star polygons (the current
    `RegularStar2D` is the two-radius form only), and `RegularPolygon → Polygon2D` /
    `Triangle2D → Polygon2D` conversions so the concrete planar shapes enter the general
    pipeline.
13. **Tiling and subdivision** — Delaunay triangulation and Voronoi cells over a point set,
    polygon grids.

## Assumptions

- The polygon vocabulary in `polygons.types.plato` is settled and these are bodies, not
  a redesign. Where a construction needs a type that does not exist (a join style, a
  Boolean operation tag), that declaration is its own small piece of work under
  `LIBRARIES.md` ground rule 6.
- Constructive polygon operations belong in the forward stdlib rather than a sidecar,
  per plato-273.
- The language is not the constraint it was assumed to be: `while`, `if` statements and
  assignment to a `var` local are all in the checked language, and the affine builders
  (`List<T>` / `Buffer<T>`) make genuinely imperative scratch storage available — see
  `docs/SEMANTICS.md` §3. A sweep-line or a mutated linked list can be written directly, so
  items 2, 3 and 13 are not forced into folds at an asymptotic penalty. Two real constraints
  replace the imagined one: a builder cannot be a function's FIRST parameter (compiler-416),
  and `&&` / `||` do not short-circuit (`docs/SEMANTICS.md` §6).
- `STYLE_GUIDE.md`'s ordering holds: the canonical body is correct/composable/functional
  first; a faster variant is separate later work, never a compromise of the canonical one.

## Design decisions

- **Index output vs value output for triangulation** — return `Array<Triangle2D>` (values,
  self-contained) vs index triples into the source point array (provenance, no duplication,
  meshable). plato-336 is exactly the complaint that the tree loses provenance; indices
  answer it, values are friendlier at the call site. Likely both, with the index form
  primary.
- **Robustness model** — exact/adaptive predicates vs plain floating-point with a documented
  tolerance. `CONVENTIONS.md` already fixes an epsilon convention; Booleans and
  self-intersection tests are where that convention gets stress-tested.
- **Even-odd vs winding-number containment** — the library fixes even-odd today and says so.
  Adding winding number means two containment answers coexisting for self-intersecting
  rings; either the second is a separately named function or the convention changes.
- **Where Boolean operations live** — a new `polygons-boolean.library.plato` sibling vs a
  section of `polygons.library.plato`. `solids-csg.library.plato` is the precedent for
  splitting a constructive algorithm out of the measurement library.
- **Whether `PolygonSet2D` needs an empty representation** — its declaration allows the
  empty set, but `Bounds(PolygonSet2D)` already carries a "at least one component"
  precondition. Booleans will produce empty results routinely.

## Related

- [plato-273](plato-273.md) — "Move geometry libraries into Plato stdlib (Earcut, CSG, BREP,
  Noise, Models)": the migration issue. This issue is the polygon-shaped half of its target
  state; the earcut item is shared between them.
- [plato-336](plato-336.md) — triangulate/hull results lose provenance back to input points,
  and no convex-hull *operation* exists. Directly constrains items 1 and 4.
- [plato-028](plato-028.md) — consumer-driven refactoring direction, names the Earcut gaps
  and query vocabulary. Informs the priority order here.
- `experiments/earcut/earcut.plato` and `earcut-fast.plato` — the two existing ear-clipping
  formulations (functional folds; affine buffer with a linked list). Source material for
  item 1, now superseded by `stdlib/geometry/triangulation.library.plato`.
- `stdlib/geometry/solids-csg.library.plato` — the 3D Boolean already in the tree
  (plane-fragment based). Precedent for both file layout and tolerance handling in item 2.

## Approaches

**Short term** — the three items that unblock the most:

1. ~~Triangulation, ported from the earcut experiment.~~ Landed across `Polygon2D`,
   `ConvexPolygon2D`, `PolygonWithHoles2D`, `PolygonSet2D` and `Polygon3D`, with an executable
   gate (`tools/regen-triangulation.ps1`). Items 9 and 13 are unblocked.
2. ~~Predicates and repair (item 5).~~ Landed. Every later item can now state
   "precondition: simple" and have callers able to honour it.
3. Convex hull + `IsConvex` (item 4). The type exists; a monotone-chain hull is a fold.

**Long term** — a complete constructive polygon kernel: Booleans, offsetting and Delaunay,
which together make the polygon vocabulary a working modelling surface rather than a
measurement surface, and give `paths.types.plato` and the mesh types a supply of geometry.

**Adjacent ideas worth their own issue:**

- Robust-predicate policy for the whole geometry tree (exact orientation/incircle vs epsilon).
- A `JoinStyle` / `EndCapStyle` vocabulary shared by offsetting, path stroking, and any
  future 3D shelling.
- Sweep-line as a reusable pattern: whether the language subset can host one at all, and
  what it costs if the answer is "only as an O(n²) fold".

## Bedrock

The seam this strengthens is the split `polygons.types.plato` already draws and
`polygons.library.plato` already honours: **declarations state invariants, kernels are
shared and named once, per-type bodies are thin.** The 2D ring kernels live in
`geometry.library.plato` and the space kernels in `polygons.library.plato`, and every body
is written against them rather than respelling a shoelace. Every item in this catalogue
should extend that kernel layer, not accumulate per-type special cases: triangulation
should produce one ring-level kernel that `Polygon2D`, `PolygonWithHoles2D`, `Polygon3D`
(via the existing projection basis) and the mesh types all call. Done that way, the
constructive half of the library costs one kernel per algorithm rather than one body per
type pair.

The invariant it makes cheaper to trust is `Polygon2D`'s "no edge crossings", currently
declared and unenforceable. Item 5 turns it from documentation into a checkable property,
which every later constructive item gets to assume.

Verdict: **simplest-along-the-grain**. The simple version must NOT introduce a
triangulation body that takes a concrete polygon type and returns concrete triangles with
the ring walk inlined — that is the shape that forecloses the kernel layer and forces a
rewrite when `PolygonWithHoles2D` and `Polygon3D` arrive. Index-level ring kernel first,
typed wrappers on top.

## Done means

This is an umbrella issue; it is closed when its catalogue has been dispersed, not when
the work is finished.

- [ ] Each of the 13 catalogue items is either filed as its own issue or explicitly
      declined with a reason recorded here.
- [ ] The three short-term items have issues with `Done means` sections of their own.
- [ ] Overlap with plato-273 and plato-336 is resolved: each shared item names one owner.

## Simplest possible implementation

Treat this file as the polygon backlog and disperse it lazily: file an issue for an item
only when someone is about to work on it, taking the catalogue entry as the starting draft.

- **What you get** — no up-front issue churn, no speculative design, and the catalogue
  stays in one readable place while the priorities are still unknown.
- **What you give up** — an umbrella issue is invisible to sprint planning and easy to let
  rot; items that never get picked up never get examined; and the design decisions above
  stay unresolved, so two people could start adjacent items with incompatible answers to
  the index-vs-value and robustness questions.
