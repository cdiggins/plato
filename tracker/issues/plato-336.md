---
id: plato-336
title: Triangulate and hull results lose provenance back to input points
type: debt
status: idea
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-30
closed:
links: [submodules/Plato/stdlib/meshes.concepts.plato, submodules/Plato/stdlib/meshes-indexed.library.plato, submodules/Plato/stdlib/spatial-queries-proximity.plato]
---

## Issue
The mesh-producing interfaces return bare meshes with no mapping back to the input
they were computed from. `Meshable2D.Triangulate(x: Self): TriangleMesh2D`
(`meshes.concepts.plato:31`) and `Meshable3D.ToTriangleMesh(x: Self): TriangleMesh3D`
(`meshes.concepts.plato:22`) hand back vertices and indices; nothing says which input
point or which source edge each output vertex came from. Separately, there is no
convex-hull type or operation at all — "Delaunay" appears once, in a doc comment at
`meshes.plato:52`, and `ConvexHull` appears nowhere in `stdlib/`.

## Impact
Provenance is what makes a triangulation composable with the data that produced it.
Without it a caller cannot: carry per-input attributes (colors, weights, ids, UVs)
through a triangulation; map a picked triangle back to the polygon vertex the user
actually selected; or diff two triangulations of the same edited input. The workaround
is a position-based lookup after the fact — an epsilon comparison against the input
points — which is both slow and wrong at coincident or welded vertices, exactly the
cases triangulators create.

Frequency: every consumer that triangulates something it cares about the identity of.
Currently low, because `Triangulate` has one implementation
(`meshes-indexed.library.plato:72`, `TriangleMesh2D` returning itself) and no caller
needs provenance yet. It rises the moment polygon or point-cloud triangulation lands.

## Affected code
- `submodules/Plato/stdlib/meshes.concepts.plato:22` — `Meshable3D.ToTriangleMesh`,
  returns `TriangleMesh3D`, no index mapping.
- `submodules/Plato/stdlib/meshes.concepts.plato:31` — `Meshable2D.Triangulate`,
  returns `TriangleMesh2D`, no index mapping.
- `submodules/Plato/stdlib/meshes-indexed.library.plato:72` — the sole
  implementation; identity for `TriangleMesh2D`, so the loss is not yet observable.
- `submodules/Plato/stdlib/meshes.concepts.plato:36-42` —
  `TriangulatedGeometry3D` exposes `FaceAt`/`PositionAt` by typed index, so the
  *output* is well indexed; only the input correspondence is missing.
- No `ConvexHull`, `Delaunay`, or hull-shaped type anywhere in `stdlib/` — verified
  by grep. `meshes.plato:52` mentions Delaunay only as an example of what a
  `TriangleMesh2D` might hold.

## Cause / analysis
The interfaces were defined at their narrowest useful signature — "anything that can
produce a triangle mesh of itself" — which is the right shape for rendering and
measurement, the first consumers. Provenance was not needed then and adding it to
the interface would have complicated every implementer. The debt is that the narrow
signature is now the only signature, so a consumer who needs correspondence has no
supported path and will reach for position matching.

The hull absence is simply un-landed content rather than debt: nothing was written,
nothing decayed. It is folded in here because a hull has the identical provenance
requirement (which input points survived onto the hull) and should not be designed
without it — filing them separately would risk landing a hull that repeats the
mistake.

## Priority
Recommend **p3**. Nothing is broken and nothing is blocked today. But this is the
cheapest it will ever be to fix: one implementation exists, so changing or extending
the interface costs almost nothing now and costs proportionally more per implementer
later. Deferring past the next mesh content wave is the expensive choice. Safe to
defer in the short term; should be decided before polygon triangulation lands.

## Dependencies
- Blocked by: nothing.
- Blocks: nothing filed. Would sensibly precede any polygon-triangulation or
  hull content work.
- Related: plato-334 — a hull or triangulation *result* type is exactly the
  "algorithm outcome" shape that issue proposes to unify, and should be designed
  against those interfaces rather than as another one-off record. If plato-334 lands
  first, this becomes a straightforward application of it.
- Touches: `meshes.concepts.plato` is depended on broadly; changing an interface
  signature ripples to every implementer. Additive approaches avoid this.

## Fix approaches
1. **Additive sibling interface** — leave `Meshable2D`/`Meshable3D` alone, add a
   `TraceableMeshable2D` (or similar) whose operation returns mesh plus
   `SourceIndices: Array<VertexIndex>`. Non-breaking; implementers opt in. Costs a
   second interface and the question of which one callers should target.
2. **Widen the existing interfaces** — have `Triangulate` return a record carrying both
   mesh and correspondence. One clear path, no duplication; breaks the one existing
   implementation and every future one, and taxes implementers that genuinely have
   no meaningful correspondence (an implicit surface, for instance).
3. **Provenance as a mesh attribute** — reuse `MeshAttribute<T>` with a
   `PerVertex` domain to carry source indices, no interface change at all. Zero new
   vocabulary and it composes with the attribute machinery already in
   `mesh-attributes.plato`; but it makes provenance optional-and-unchecked, findable
   only by knowing the attribute's name — a stringly-typed contract.

## Bedrock
The invariant worth establishing: *an operation that renumbers things reports the
renumbering.* This is the same seam as plato-334 — the boundary between an algorithm
and its caller — and the same failure: the result type describes the output but not
the relationship to the input, so the caller reconstructs by guessing. Fixing it
makes attribute transfer, picking, and incremental re-triangulation cheap, and all
three are currently impossible without epsilon matching. Option 3 is tempting because
it costs nothing, but it encodes the contract in an attribute name rather than a
type, which is precisely the pattern CONVENTIONS.md and the sum-type work elsewhere
in Plato move away from.

Verdict: **simplest-along-the-grain** — take option 1, and the additive interface must
NOT be introduced without deciding what the plain `Meshable2D` means once it has a
traceable sibling. If both survive indefinitely with no guidance on which to
implement, this becomes two vocabularies for one idea, and option 2's single path
stops being reachable.

## Done means
- [ ] a triangulation result exposes, for each output vertex, the input it came from
      (or an explicit "generated, no source" representation — not a `-1` sentinel;
      see plato-079)
- [ ] the correspondence is expressed in the type system, not an attribute-name
      convention
- [ ] guidance recorded on which interface new implementers should target
- [ ] convex hull, if landed, uses the same correspondence vocabulary from the start
- [ ] ForwardStdLib test green

## Simplest fix
Option 1 for triangulation only, hull deferred: one additional interface and one
result record. Gain: unblocks attribute transfer and picking, breaks nothing, can
land in isolation. Give up: two mesh-producing interfaces coexist, so authors must be
told which to implement — that guidance is part of the work, not an optional extra.

## Prevention
- **Convention**: CONVENTIONS.md has no entry on renumbering. A line stating that
  operations which reindex must report the mapping would generalize past meshes to
  welding, decimation, and CSG — all of which have the same obligation and none of
  which currently state it.
- **Design review**: the hull half of this issue exists because a hull has not been
  written yet. The preventive step is that it should not be written without this
  question answered — worth noting in whatever content wave picks it up.
- Related existing items: plato-334 (shared result interfaces) is the natural home for
  the result-type half; plato-079 (sentinels/Option) governs how "no source vertex"
  should be spelled.

## Update 2026-08-04 (hull landed; the provenance complaint survives)

Two premises in this issue are now false, and one of the two is exactly the
outcome the Prevention section warned against.

- "No `ConvexHull`, `Delaunay`, or hull-shaped type anywhere in `stdlib/`" was
  already out of date before this update: `ConvexHull2D` and `ConvexHull3D` are
  declared types (`stdlib/geometry/geometry.types.plato:98,111`) with query
  bodies in `stdlib/geometry/geometry.library.plato`.
- "no convex-hull *operation* exists" is now false too. plato-442 landed
  `ConvexHull(Array<Point2D>): ConvexHull2D` — a monotone-chain builder —
  at `stdlib/geometry/geometry.library.plato:242`.

The file references above also predate the 2026-07-30 stdlib reorganisation:
`meshes.concepts.plato` and `meshes-indexed.library.plato` now live under
`stdlib/geometry/`, and there is no `stdlib/meshes.plato`.

The hull half of this issue is therefore answered, and answered the way the
Prevention section asked for: `ConvexHull2D` and `ConvexHull3D` each carry
`SourceIndices: Array<ItemIndex>`, documented as index-aligned with the hull's
own points, with `SourceOf` and `SourcePoints` reading it. The correspondence is
a field of the result type rather than an attribute-name convention — option 3
was not taken — so the `Done means` item "convex hull, if landed, uses the same
correspondence vocabulary from the start" is satisfied for the hull.

What survives is the triangulation half, unchanged: `Meshable3D.ToTriangleMesh`
and `Meshable2D.Triangulate` (`stdlib/geometry/meshes.concepts.plato:30,48`)
still return a bare mesh with no declared mapping, and no guidance exists on
which interface a new implementer should target. The hull now supplies the
precedent that half was missing — a `SourceIndices` array index-aligned with the
output, named in the type — so option 1 has a shape to copy rather than to
invent.

One nuance the issue could not have known: the landed ear-clipping bodies
(`stdlib/geometry/triangulation.library.plato:581-671`) happen to reuse the
input point array verbatim as the output vertex array, so for those specific
implementations the correspondence is the identity. That makes the loss
unobservable today for the same reason the original issue gave, not fixed; the
interface still permits an implementation that renumbers, and says nothing when
one does.

`Delaunay` genuinely does not exist anywhere in the tree (verified 2026-08-04 by
symbol search, zero hits), and remains deferred per the decision above.
