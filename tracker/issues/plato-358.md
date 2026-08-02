---
id: plato-358
title: Reclassify TetrahedronCell and peers as Face-like cells
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-350, stdlib/meshes-volumetric.plato, stdlib/meshes.concepts.plato]
---

## Idea
`TetrahedronCell` (and similar volumetric cell types) are named/treated like cells but structurally match faces: fixed vertex-index corners, Hashable, natural Indexable — i.e. they are Face-like elements of a volumetric mesh, not a separate taxonomic kingdom. `TetrahedronCell` today: `implements Value, Hashable` with A,B,C,D: VertexIndex (`meshes-volumetric.plato`); `Face` interface inherits `Value, Hashable, Indexable<VertexIndex>`.

## Assumptions
- Volumetric meshes need cell types parallel to TriangleFace/QuadFace.
- Calling them "Cell" is fine if they implement Face (or a Cell interface inheriting Face).
- plato-324 unified surface faces; volumetric side was left behind.

## Design decisions
- **Face vs Cell interface** — Cell inherits Face vs separate Cell with same obligations vs rename to TetrahedronFace (misleading in 3D FEM).
- **Orientation / volume sign** — cell-specific ops beyond Face.
- **Coverage** — HexahedronCell, WedgeCell, PyramidCell same treatment.

## Related
- `stdlib/meshes-volumetric.plato` — TetrahedronCell.
- `stdlib/meshes.concepts.plato` — Face.
- DONE plato-324 — Face unification for triangle/quad.
- [plato-350](plato-350.md) — Indexable boilerplate for corner fields.

## Approaches
Short term: `TetrahedronCell implements Face` (+ Indexable synthesis or manual At/Count); library helpers Edges/Faces-of-cell.
Long term: Cell interface for volume; shared mesh incidence for tet meshes.
Adjacent: rename discussion only if Face implement is confusing.

## Bedrock
Extends the **Face obligation seam** from plato-324 into volumetric elements. Verdict: **simplest-along-the-grain**. Must NOT rename away "Cell" if FEM vocabulary needs it — implement Face (or Cell⊃Face).

## Done means
- [ ] TetrahedronCell participates in Face (or documented Cell⊃Face)
- [ ] Count/At/Vertices work
- [ ] Peer cells listed or explicitly out of scope

## Simplest possible implementation
Add implements Face + At/Count like QuadFace; keep name TetrahedronCell.
- Pros: taxonomy fix; tiny.
- Cons: Face-of-volume naming awkward in prose.

## Case against
- Face implies 2D facet; cells are 3D — interface abuse.
- Volumetric incidence differs; forcing Face helpers may be wrong.
- Verdict: **pursue** shared Indexable/Hashable obligations; **park** literal `implements Face` if naming confuses — prefer `interface Cell inherits Face` or parallel interface.
