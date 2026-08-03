---
id: plato-413
title: Marching cubes: isosurface extraction from dense scalar grids and sampled SDFs
type: idea
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: [plato-409, plato-315]
---

## What and why

`stdlib/geometry` declares the volumetric input types (`DensityGrid3D`,
`LevelSetGrid3D`, `SampledSdf3D`, `voxels.types.plato`) and the unwelded output type
(`TriangleArray3D`, `meshes.types.plato`, whose doc comment already names marching
cubes as a producer), but nothing joined them: no path existed from a scalar volume
or an SDF to geometry. plato-409 named mesh extraction as the second major consumer
of the SDF collection after ray marching and deferred it to its own issue; this is
that issue.

Port source: `ara3d-sdk/src/Ara3D.Geometry/MeshAlgorithms/MarchingCubes.cs` in the
studio checkout (and its older twin under `submodules/Plato.Geometry/WIP/`), which is
in turn Paul Bourke's table (http://paulbourke.net/geometry/polygonise/).

## Design decisions

- **New file `stdlib/geometry/voxels.library.plato`** (`library Voxels`), the sibling
  bodies file for `voxels.types.plato` per LIBRARIES.md ground rule 1. No declaration
  file was touched, so no new types and no `types-and-concepts.txt` churn.
- **Sign convention: the extracted region is the AT-OR-ABOVE side of the iso level**,
  and configuration bit `c` is set when corner `c` is BELOW it. That is Bourke's
  convention, and it is what makes his table wind counter-clockwise seen from outside
  that region (CONVENTIONS.md — winding). The distance-valued inputs are negative
  inside, so their entry points march the negated value at iso level zero.
  **The C# original had this inverted** — it flags corners at or above the threshold
  and then papers over the resulting inward normals by emitting each triangle twice,
  once in each winding, which also doubles the triangle count. The port does not
  reproduce that.
- **Corner and edge numbering is arithmetic, not a lookup table.** An array literal
  lowers to an allocating `MakeArray` call, so a corner-offset table would allocate
  once per cube. The case table is unavoidable data and is therefore read exactly once
  per extraction (in `MarchingCubesLattice`) and passed down.
- **The case table is a constant dispatched on `TriangleArray3D`**, the type the
  extraction produces, read as `TriangleArray3D.MarchingCubesCaseTable`. The tree's
  constant idiom wants the result type as the ignored receiver, but the result type
  here is the generic `Array<Integer>`; the algorithm's output type is the nearest
  non-generic stand-in. Bourke's separate 256-entry edge-mask table was dropped: its
  only use is the empty-configuration early-out, which the first entry of a case row
  already reports.
- **Corner samples are re-read per cut edge rather than cached per cube.** Cut cubes
  are the surface and grow as the square of resolution while the total grows as the
  cube, so the common case is an uncut cube paying eight reads and allocating nothing;
  a per-cube cache would allocate on every cube instead.
- **One kernel, five entry points.** `MarchingCubesLattice` is written against a
  value-sampling and a position-sampling function rather than a grid type, so the two
  dense grids, the sampled SDF, and a procedural field share it. The field entry points
  store no grid at all.

## Verification

Lint (strict), the checker ratchet and index freshness all pass — see the commit.
No gate that currently runs *executes* geometry bodies (`plato-308` keeps the forward
conformance law runner red), so the algorithm was verified out-of-band instead, against
the same table and the same index arithmetic the Plato source uses:

- For all 256 configurations, the set of edges the table names is exactly the set of
  edges whose two corners fall on opposite sides of the iso level, under the corner and
  edge formulas in `MarchingCubesCornerOffset*` / `MarchingCubesEdgeCorner*`, and every
  row's entry count is a multiple of three.
- For all 254 cut configurations, the summed right-hand normal of the emitted triangles
  points away from the at-or-above region (strictly, for the 238 configurations where
  the two regions' centroids are not symmetric; the remaining 16 are the diagonally
  ambiguous cases, where it is exactly perpendicular rather than inverted).

## Done means

- [x] A dense scalar grid, a level-set grid and a sampled SDF each extract to a
      `TriangleArray3D`
- [x] Any `IScalarField3D` / `ISignedDistanceField3D` extracts over a caller-given
      bounds and lattice resolution, without materializing a grid
- [x] Faces wind counter-clockwise seen from outside the extracted region, and the
      convention is stated where a reader of the bodies will find it
- [x] Lint strict, checker ratchet and index freshness green
- [ ] The bodies are executed by something — blocked on `plato-308` (the forward
      conformance law runner does not compile its generated code yet). Until then the
      correctness evidence is the out-of-band check above, not a run.

## Follow-ups worth their own issue

- Dual contouring / surface nets: sharper features from the same inputs, and the natural
  consumer of `GradientAt` now that plato-409 landed it.
- A welding pass (`TriangleArray3D` to `TriangleMesh3D` with shared vertices), which
  marching cubes output wants and which nothing in the tree provides.
