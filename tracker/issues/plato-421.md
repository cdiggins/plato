---
id: plato-421
title: 3D lattice structures: unit cells, tilings, and lattice operators
type: feature
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

Plato has no vocabulary for periodic lattice structures — the strut-and-node
scaffolds used in additive manufacturing, metamaterials and infill. The pieces are
all present separately (`Grid3D`, `IntegerVector3`, `Line3D`, `Bounds3D`,
`PolygonMesh3D`, the implicit-SDF collection) and nothing names a unit cell or a
lattice operator over one.

Scope: **`stdlib/geometry`** (shipping tier — must pass lint strict, the checker
ratchet and index freshness). New `lattices.types.plato` +
`lattices.library.plato`, plus `lattices.concepts.plato` if an interface earns
its place.

Subject matter, roughly:

- **Strut lattices**: a unit cell as a set of nodes plus a connectivity list in
  normalized cell coordinates; the standard cells (simple/body-centred/face-centred
  cubic, octet truss, Kelvin/tetrakaidecahedron, diamond, re-entrant auxetic).
- **Tiling an instance**: a cell repeated over an `IntegerVector3` count inside a
  `Bounds3D`, producing struts as `Array<Line3D>` with the shared nodes welded, plus
  the derived readings (relative density, strut length, node valence, cell count).
- **Operators over a lattice**: uniform and graded scaling of strut radius or cell
  size from a field (`IScalarField3D`), trimming to a bounding solid or an SDF,
  conforming a lattice to a `Bounds3D` versus a deformation, and the dual lattice.
- **Triply-periodic minimal surfaces** as the implicit counterpart — gyroid,
  Schwarz P, Schwarz D, Neovius, primitive — as `IScalarField3D` /
  `ISignedDistanceField3D` implementations so the existing marching-cubes and SDF
  paths consume them with no new plumbing.

Reuse what exists. Struts become geometry through the existing `Line3D` /
`PolygonMesh3D` vocabulary; TPMS fields go through `implicit-sdf` and
`voxels.library.plato`'s marching cubes rather than a second extraction path.

## Design decisions

_(fill in — say what convention you chose and what you rejected)_

## Done means

- [ ] A unit-cell type and the named standard cells, in normalized cell coordinates
- [ ] A lattice instance tiles a cell over a count inside a bounds and yields welded struts
- [ ] Graded / field-driven and trimmed variants, without a second tiling path
- [ ] The TPMS family implements the existing scalar-field and SDF interfaces
- [ ] Derived readings: relative density, strut count, total strut length, node valence
- [ ] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green (lint strict + checker ratchet)
- [ ] Design decisions recorded above
