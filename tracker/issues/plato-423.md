---
id: plato-423
title: Remeshing: subdivision, decimation, isotropic remeshing and smoothing
type: feature
status: in-progress
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`SubdivisionScheme` is an **empty type** and `SubdivisionSurface` names it, a level
count and a control mesh — with nothing behind either. Nothing in the tree splits,
collapses or flips an edge; nothing decimates; nothing smooths a mesh. plato-420
lists this among the declared-but-unimplemented tiers. Remeshing is also the
missing consumer for the marching-cubes output (plato-413), which is unwelded and
badly shaped by construction.

Scope: **`stdlib/geometry`** (shipping tier — lint strict, checker ratchet, index
freshness). Likely `remeshing.types.plato` + `remeshing.library.plato`, plus bodies
in `meshes.library.plato` / `topology.library.plato` where the operation is really
a mesh primitive.

Subject matter, roughly:

- **The local operators**: edge split, edge collapse, edge flip, vertex split —
  written functionally (a new mesh, not a mutation), which is the interesting design
  problem in a pure language and the part to think hardest about.
- **Subdivision**: Loop (triangles), Catmull-Clark (quads/polygons), and the
  interpolating Butterfly, filling `SubdivisionScheme` in as a sum type (non-generic;
  see AGENTS.md) with the level count driving repeated application.
- **Decimation**: quadric error metrics (Garland-Heckbert), which needs the
  per-vertex quadric — note the tree already has a `Quadric` type carrying a
  `Matrix4x4`, so check whether it fits before adding another.
- **Isotropic remeshing**: the Botsch-Kobbelt loop — split long edges, collapse
  short ones, flip toward valence six, tangentially relax — over a target edge
  length. This is the headline result.
- **Smoothing**: Laplacian, cotangent-weighted Laplacian, Taubin lambda/mu (which
  does not shrink), and tangential relaxation.
- **Welding / merging coincident vertices**, which marching-cubes output needs and
  which nothing provides today.

Purity is the constraint that shapes all of this: an incremental remesher is
normally written as in-place mutation of a half-edge structure. Say in the issue how
you resolved that — batched passes over an immutable mesh, an index-remap
representation, or something else — because that decision is the reusable part.

## Design decisions

### The immutable-remeshing representation: batched passes over two reified intermediates

Every remeshing operation is a **whole-mesh rebuild driven by decisions taken against a
single, unchanging input mesh**. The mesh type never changes: a `TriangleMesh3D` goes in
and a `TriangleMesh3D` comes out, at every stage. Two derived intermediates carry the work,
both declared in `remeshing.types.plato`:

- **`TriangleMeshTopology`** — the recovered edge table (corner-to-edge, corner twins,
  canonical endpoint pairs, edge-to-naming-corner). It plays the role a half-edge structure
  plays in a mutable remesher, minus the part that makes a half-edge structure hard: it is
  *derived and read-only*, rebuilt per pass and thrown away, never updated in place. A pass
  that does not change connectivity — all the smoothing passes — builds it once and reuses it
  across every iteration, which is sound only because it is a value rather than a structure
  being edited.
- **`VertexRemap`** — a per-old-vertex target array plus the compacted position array. Every
  vertex-REMOVING operation (welding, edge collapse, quadric decimation) reduces to "decide
  which vertices become one, decide where the survivor lands, then `ApplyRemap`", which
  rewrites the face table and drops the faces the merge degenerated.

Operations that ADD vertices need no remap: they append the new points to the position array
in a fixed block order (old vertices, then one point per split edge) and rewrite the face
table by index arithmetic. That block layout is what lets Loop and Butterfly subdivision take
their *topology* from the generic `SplitEdges` with every edge masked and supply only a
different position array of the same shape — the schemes' entire geometric content is one
array, and the refinement pattern is written once.

The single-edge operators (`SplitEdge` / `CollapseEdge` / `FlipEdge`) are the batched
functions applied to a mask that selects one edge. The mutable-style API exists; it is a
special case of the batched one, not a separate code path.

**Batching needs conflict rules, and getting them right was the real work.** All decisions
are taken against one mesh, so two individually-legal edits can be jointly illegal:

- *Collapse* requires the **link condition** (enforced inside `CollapseEdges`, not left to
  callers — an edge that fails it has no correct collapse), and requires that no two accepted
  collapses have **adjacent** endpoints. Forbidding only a shared endpoint is not enough: two
  collapses one edge apart turn two distinct triangles into the same triangle, and the surface
  gets a doubled face. Fold-over is still not checked and is documented as such.
- *Flip* requires that the new diagonal is not already an edge, and claims **vertices**
  rather than faces. On an octahedron every equatorial edge sees the same two poles, so two
  face-disjoint flips would each draw the pole-to-pole diagonal.

### Rejected

- **A persistent half-edge structure with functional update.** `Array` has no structural
  sharing, so every "local" edit rebuilds the whole array anyway — the same cost as a rebuild,
  but with four parallel index arrays and their -1 invariants to maintain through every step.
- **A sequential fold of single-edge operations** (`edges.Reduce(mesh, (m, e) => m.CollapseEdge(e))`)
  as the primary form. It is expressible and pure, but it is one full mesh rebuild per edge
  and, worse, every edge index is invalidated by the preceding step — which is exactly the
  bookkeeping that makes a mutable remesher hard. Batched decision arrays sidestep index
  invalidation entirely.
- **A `unique` (affine) mutable builder holding a half-edge structure.** Legal in Plato, but it
  puts the algorithm's meaning in a traversal order.
- **A second quadric type.** The existing `Quadric` (`spatial-structures.types.plato`, a
  symmetric `Matrix4x4`) is exactly the Garland-Heckbert error quadric: the surface reading is
  "where the form is zero", the decimation reading is "the value of the form at a point". No
  new type; `QuadricError` / `QuadricMinimizer` / `Add` are added in `remeshing.library.plato`.
  The minimizer is found by replacing the quadric's last COLUMN with (0,0,0,1) and reading the
  inverse's translation row, which reuses `Matrix4x4.Invert` rather than hand-rolling a solver.
- **A `remeshing.concepts.plato`.** No interface earned its place: every operation is concrete
  over `TriangleMesh3D` / `PolygonMesh3D`, and an interface with no implementer is a LINT013.

### Corrections to the brief

`SubdivisionScheme` was **not** empty — it is `CatmullClark | Loop | DooSabin`
(`surfaces.types.plato`). What was missing was bodies. All three cases are implemented and
dispatched by `SubdividedOnce`; no declaration file was touched, so `types-and-concepts.txt`
gains only the three new declarations of `remeshing.types.plato`. Butterfly is implemented as
a standalone triangle-mesh function rather than a fourth sum case, to avoid churning a shared
declaration file for a scheme the type never named.

### Cost

Nothing in this vocabulary has a keyed container (LINT013 on `IMap`), so the edge table is
recovered by scanning corners against corners: `TopologyOf` is quadratic in the corner count
and dominates every pass built on it. That is the same trade `meshes-polygon.library.plato`
already documents. The per-function comments give the working ranges.

## Verification

Lint (strict) and the checker ratchet both pass — see the commit. No gate currently
*executes* geometry bodies (`plato-308` keeps the forward conformance law runner red), so the
algorithms were verified out-of-band, against a transcription of the same index arithmetic the
Plato source uses, over a tetrahedron, an octahedron, an open triangulated grid, and a
twice-subdivided octahedron:

- `TopologyOf` recovers V - E + F = 2 on the closed inputs and 1 on the disk; every corner's
  edge number lies in range, twin pairs agree on it, and no apex lies on its own edge.
- Uniform `SplitEdges` gives exactly 4F faces and V + E vertices, preserves the Euler
  characteristic, and leaves the mesh closed and consistently wound; all eight partial-split
  masks on a face leave the mesh closed and consistently wound.
- `FlipEdges` preserves face count and positions, leaves the mesh closed and oriented, and
  correctly refuses every flip on a tetrahedron (where all four vertices are already joined).
- `CollapseEdges` never leaves a degenerate face, and the link condition plus the
  adjacent-endpoint rule are each necessary: removing either one produces a doubled face or a
  pinched surface on the octahedron test.
- Welding 24 unwelded octahedron corners yields 6 vertices and 8 faces with Euler 2, and a
  chained tolerance group resolves to a single representative.
- The quadric minimizer of three orthogonal planes is their common point, the error there is
  zero, and it grows away from it.
- Four isotropic passes at half and at twice the starting edge length both stay closed,
  oriented and Euler-2 while the face count moves the expected way (128 to 512 refining,
  128 to 50 coarsening).
- Twenty quadric-decimation passes toward 32 faces stay closed, oriented and Euler-2, approach
  the target from above, and keep the vertices in the shell the input occupied.
- Catmull-Clark on a cube emits one quad per corner over a V+E+F numbering, and Doo-Sabin
  emits F+E+V faces; both results are closed and consistently oriented. The Doo-Sabin edge
  quad had to be reversed to get there, which this check is what caught.

## Done means

- [x] Edge split / collapse / flip as pure mesh-to-mesh operations
- [x] Loop and Catmull-Clark subdivision, with `SubdivisionScheme` no longer empty
      (it already was not empty; all three of its cases now have bodies, plus Butterfly)
- [x] Quadric-error decimation to a target triangle count or error bound
- [x] Isotropic remeshing to a target edge length
- [x] Laplacian, cotangent and Taubin smoothing (plus tangential relaxation)
- [x] Vertex welding, so unwelded triangle soup becomes a mesh
- [x] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green
- [x] Design decisions recorded above
- [x] The out-of-band check is durable rather than ephemeral —
      `tools/out-of-band-checks/remeshing.py`, with a README saying what it does
      and does not prove
- [x] `stdlib/types-and-concepts.txt` regenerated — three new types make it stale
- [ ] A browser demo drives it: `demos/webgl/remeshing.html` +
      `src/demos/remeshing.ts`, green under `npm run typecheck` and `npm run scenes`
- [ ] The bodies are executed by something — blocked on `plato-308`, exactly as `plato-413`
      is. Until then the correctness evidence is the out-of-band check above, not a run.
