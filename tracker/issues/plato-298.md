---
id: plato-298
title: Represent polygon meshes with and without holes; refine manifold interfaces
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/stdlib/meshes.plato, submodules/Plato/stdlib/topology-classification.plato, submodules/Plato/stdlib/geometry.concepts.plato, submodules/Plato/stdlib/topology-half-edges.plato, submodules/Plato/earcut/earcut.plato, docs/geometry3sharp-port-candidates.md, docs/ara3d-modifiers-generators-backlog-2026-07-16.md, plato-297, ara3d-056, plato-273]
---

## Idea

Make sure Plato can **properly represent polygon meshes**, including the split between faces **without holes** and faces **with holes**, and decide which **topology interfaces** (especially manifoldness) belong as type markers vs computed classification. Forward stdlib already has CSR `PolygonMesh3D` (simple face rings, no hole rings), `TriangleMesh3D` / `QuadMesh3D`, marker interface `Manifold`, and enum `Manifoldness` on `TopologySummary`. 2D already has `PolygonWithHoles` in Earcut. Gap: 3D polygon meshes cannot express a face with inner rings; there is no clear story for when a mesh *is* a manifold vs when manifoldness is only a runtime audit; hole-aware half-edge (`BordersHole`) assumes hole representation that the mesh type does not yet carry.

## Assumptions

- Arbitrary-arity faces matter for polyhedra ([plato-297](plato-297.md)), CAD/BREP tessellation, and quad-dominant cages — triangle-only is not enough.
- Holes appear in at least two places: (a) **planar face holes** (outer + inner rings on one face), (b) **surface boundary loops** (mesh holes). These must not be conflated in the type system.
- Earcut's `PolygonWithHoles` is the 2D precedent for (a); half-edge `BordersHole` / boundary loops cover (b).
- Marker interfaces (`Manifold`, `ClosedShape`, `Orientable`) are promises; `Manifoldness` on `TopologySummary` is a measurement — both can coexist if the rules are explicit.

## Design decisions

- **One type vs two** — keep a single `PolygonMesh3D` that optionally carries hole rings vs split `SimplePolygonMesh3D` (no holes) and `PolygonMeshWithHoles3D` (or face-local `FaceWithHoles`). Two types make illegal states unrepresentable for Conway/dual; one type is simpler for importers. Leaning two categories (user request) with a shared interface `PolygonalMesh3D`.
- **Hole encoding** — CSR with ring breaks (offsets + `RingKind` outer/hole) vs array-of-faces where each face is `PolygonWithHoles3D` (3D-embedded rings). Face-as-value matches Earcut and triangulation per face; flat CSR matches GPU upload. Prefer face records for library clarity; CSR as a derived packing view.
- **Manifold as interface vs enum** — strengthen `Manifold` / add `ManifoldWithBoundary` markers for APIs that require them (Conway, dual, volume) vs only populate `TopologySummary.Manifoldness` after analysis. Prefer markers for *static* seeds (Platonic meshes) and computed enum for imported soups.
- **New interfaces worth adding** — candidates: `ManifoldWithBoundary`, `Watertight` / `ClosedSurface` (stronger than `ClosedShape`), `OrientableSurface`, `PolygonalFace` / `FaceWithHoles`, maybe `PureSimplicial` for triangle meshes. Avoid a god-interface; compose markers.
- **Non-manifold policy** — represent non-manifold meshes as first-class (importers) with analysis reporting `NonManifold`, vs refuse construction. Must allow soup → repair pipelines (see modifiers backlog).

## Related

- [meshes.plato](../../submodules/Plato/stdlib/meshes.plato) — current `PolygonMesh3D` CSR without holes.
- [topology-classification.plato](../../submodules/Plato/stdlib/topology-classification.plato) — `Manifoldness`, `TopologySummary`.
- [geometry.concepts.plato](../../submodules/Plato/stdlib/geometry.concepts.plato) — marker `Manifold`.
- [topology-half-edges.plato](../../submodules/Plato/stdlib/topology-half-edges.plato) / meshes-topology — hole-bordering half-edges already named.
- [earcut.plato](../../submodules/Plato/earcut/earcut.plato) — 2D `PolygonWithHoles` + triangulation.
- [docs/geometry3sharp-port-candidates.md](../../docs/geometry3sharp-port-candidates.md) — immutable topology; state manifold preconditions in types.
- [docs/ara3d-modifiers-generators-backlog-2026-07-16.md](../../docs/ara3d-modifiers-generators-backlog-2026-07-16.md) — manifoldness diagnostics as mesh-analysis foundation.
- [plato-297](plato-297.md) — polyhedra need hole-free manifold polygon meshes as operator domain.
- [ara3d-056](ara3d-056.md) / [plato-273](plato-273.md) — solid/BREP/mesh capability and stdlib migration context.

## Approaches

Short term: (1) document the two hole meanings; (2) add face-with-holes type (or extend polygon mesh) + per-face triangulate via Earcut; (3) add `ManifoldWithBoundary` marker and laws that `TopologySummary.Manifoldness` agrees for known seeds.

Long term: half-edge build from both simple and holed polygon meshes; watertight/orientable markers; repair ops that upgrade `NonManifold` → manifold; BREP face loops map cleanly onto face-with-holes.

Adjacent ideas: typed boundary-loop / edge-span results (G3 port doc); mesh-repair interface family; CSR packing view as separate type.

## Case against

- **Premature split.** Most Studio pipelines are triangle meshes; polygon+holes may wait until BREP/CAD import is real.
- **Two types tax every algorithm.** Dual implementations or constant upcast to the holed form — complexity for rare faces-with-holes.
- **Markers lie.** Declaring `Manifold` on a type that can be deformed into non-manifold geometry recreates the stale-flag problem ara3d-056 warned about — prefer computed classification only.
- **Earcut already handles 2D holes.** Lifting holes into 3D mesh types before Earcut is in stdlib ([plato-273](plato-273.md)) duplicates ring logic.

**Verdict: pursue** a clear two-category model (simple faces vs faces-with-holes) plus an explicit rule: **markers only on closed constructor paths / validated wrappers; soups carry computed `Manifoldness`**. Park watertight-as-marker until volume/contains APIs need it. Do not drop hole support — half-edge comments already assume it.

## Bedrock

Strengthens the **mesh representation seam** in `meshes.plato` + topology markers: illegal hole/manifold states become harder to represent silently, so Conway/dual ([plato-297](plato-297.md)), Earcut, and half-edge navigation share one contract instead of three implied ones. **Verdict: simplest-along-the-grain** — introduce face-with-holes (or documented extension of `PolygonMesh3D`) + `ManifoldWithBoundary` marker + written invariant linking markers to `Manifoldness`; must NOT rewrite `TriangleMesh3D` storage or port full DMesh3 mutation in the same change.

## Done means

- [ ] Written decision: one vs two polygon-mesh types; how face holes vs boundary loops differ
- [ ] Representation exists for a polygon face with outer + inner rings in 3D, triangulable (Earcut or equivalent)
- [ ] Simple (no-hole) polygon meshes remain the easy path for Platonic/Conway seeds
- [ ] Interface story documented: which of `Manifold`, `ManifoldWithBoundary`, `Orientable`, `ClosedShape`/`Watertight` are markers vs computed; `TopologySummary.Manifoldness` stays the audit enum
- [ ] At least one law or test: known manifold seed reports `Manifold` / `ManifoldWithBoundary` consistently with markers

## Simplest possible implementation

Add `PolygonFace3D { Outer: Array<VertexIndex>; Holes: Array<Array<VertexIndex>> }` (or reuse a 3D `PolygonWithHoles` of indices), keep today's CSR `PolygonMesh3D` as the simple no-hole form (or build it from faces with empty holes), wire per-face triangulation through Earcut; add `ManifoldWithBoundary` marker interface; document when markers may be claimed.

Pros: matches user split; reuses Earcut; unblocks polyhedra operators on the simple path.
Cons: two face encodings until CSR packing is unified; marker discipline must be enforced by convention/laws, not the type checker alone.
