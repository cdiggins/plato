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

_(fill in — the immutable-remeshing representation, and what you rejected)_

## Done means

- [ ] Edge split / collapse / flip as pure mesh-to-mesh operations
- [ ] Loop and Catmull-Clark subdivision, with `SubdivisionScheme` no longer empty
- [ ] Quadric-error decimation to a target triangle count or error bound
- [ ] Isotropic remeshing to a target edge length
- [ ] Laplacian, cotangent and Taubin smoothing
- [ ] Vertex welding, so unwelded triangle soup becomes a mesh
- [ ] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green
- [ ] Design decisions recorded above
