---
lesson: point-clouds-voxels
title: Point Clouds and Voxels
domain: Meshes & spatial structures
v3-files: [33-pointclouds-voxels.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Point Clouds and Voxels

Not every 3D model is a skin of triangles. A LiDAR scanner emits a spray of points. A CT
scan emits a regular lattice of densities. A destruction sim may track occupancy cubes.
Meshes shine when you need an explicit surface; **point clouds** shine when you measured
samples; **voxels** shine when the interior matters as much as the boundary. Plato's v3
vocabulary gives each representation its own types — enriched clouds, dense and sparse
grids, level sets — plus concepts for converting *to* a cloud or an occupancy raster.

## The idea

### Point clouds

A bare cloud is an unstructured list of positions: no edges, no faces, no guaranteed
ordering.

```
  *    *      *
     *    *  *     ← samples only; surface is implied
  *      *    *
```

Enrichment adds parallel channels:

| Enrichment | Why |
|------------|-----|
| Normals | Orientation for reconstruction, splatting, lighting |
| Colors | Photogrammetry / textured display |
| Intensities | Raw sensor return strength |

Oriented clouds are the usual input to Poisson reconstruction and similar algorithms:
positions say *where*, normals say *which way is outside*.

### Voxels

A **voxel grid** assigns a value to every cell of a regular 3D lattice. Plato's dense
grids share one parameterization:

- `Origin` — world position of the minimum corner of cell $(0,0,0)$
- `CellSize` — edge length of one cubic cell
- Cell $(i,j,k)$ spans the half-open box
  $[\text{Origin} + (i,j,k)s,\; \text{Origin} + (i+1,j+1,k+1)s)$

```
  Origin
    o----o----o
    | c00| c10|
    o----o----o----→ +X
    | c01| c11|
    o----o----o
    |
    ↓ +Y  (+Z out of page)
```

Variants by payload:

- **Occupancy** — Boolean in/out (collision masks, voxelized solids)
- **Density** — scalar smoke, medical values, probabilities
- **Color** — voxel art, baked lighting
- **Level set** — narrow-band signed distances for surfaces inside a volume

Dense grids waste memory on empty space. **Sparse** forms store only occupied cells
(`SparseVoxelGrid3D`) or two-level **bricks** (`BrickMap3D`) — dense blocks hung on a
sparse index.

### When each wins

| Need | Prefer |
|------|--------|
| Scanned real-world surface samples | Point cloud |
| Editable/explicit topology, UVs, animation | Mesh |
| Volumetric fog, medical data, CSG by raster | Voxels / density |
| Smooth deformable surface with narrow band | Level set grid |
| Huge mostly-empty worlds | Sparse voxels / bricks |

## In Plato

Bare and enriched clouds (`33-pointclouds-voxels.plato`; bare `PointCloud3D` is also
declared among mesh file neighbors, and this file focuses on enrichment):

```plato
type OrientedPointCloud3D
    implements Value, PointCloudable3D
{
    Positions: Array<Point3D>;
    Normals: Array<Direction3D>;
}

type ColoredPointCloud3D
    implements Value, PointCloudable3D
{
    Positions: Array<Point3D>;
    Colors: Array<Color>;
}

type AttributedPointCloud3D
    implements Value, PointCloudable3D
{
    Positions: Array<Point3D>;
    Normals: Array<Direction3D>;
    Colors: Array<Color>;
    Intensities: Array<Number>;   // empty array = channel absent
}
```

Dense grids:

```plato
type VoxelGrid3D<T>
{
    Origin: Point3D;
    CellSize: Number;
    Values: Array3D<T>;
}

type OccupancyGrid3D
{
    Origin: Point3D;
    CellSize: Number;
    Occupied: Array3D<Boolean>;
}

type DensityGrid3D
{
    Origin: Point3D;
    CellSize: Number;
    Values: Array3D<Number>;
}

type LevelSetGrid3D
{
    Origin: Point3D;
    CellSize: Number;
    Distances: Array3D<Number>;
    NarrowBandWidth: Number;
    BackgroundDistance: Number;   // correct sign outside the band
}
```

Sparse forms:

```plato
type SparseVoxelGrid3D<T>
{
    Origin: Point3D;
    CellSize: Number;
    Coordinates: Array<IntegerVector3>;  // unique, sorted Z then Y then X
    Values: Array<T>;
}

type BrickMap3D
{
    Origin: Point3D;
    CellSize: Number;
    BrickSize: Integer;
    Bricks: Array<VoxelBrick3D>;
}
```

Conversion concepts:

```plato
concept PointCloudable3D
{
    ToPointCloud(x: Self): PointCloud3D;
}

concept Voxelizable3D
{
    ToOccupancyGrid(x: Self, cellSize: Number): OccupancyGrid3D;
}
```

Usage-shaped snippets:

```plato
cloud: OrientedPointCloud3D
p = cloud.Positions[i]
n = cloud.Normals[i]                 // Direction3D, parallel array

occ = ToOccupancyGrid(solid, 0.05)   // Voxelizable3D
inside = occ.Occupied[i, j, k]

ls: LevelSetGrid3D
d = ls.Distances[i, j, k]
// |d| <= NarrowBandWidth near surface; else ±BackgroundDistance
```

2D siblings (`OccupancyGrid2D`, `DensityGrid2D`) use the same Origin/CellSize pattern in
the plane — floor plans, walkability, height-as-density.

## Pitfalls / fine print

**Parallel array lengths.** For oriented/colored clouds, positions and attribute arrays
must match length. `AttributedPointCloud3D` allows *empty* channels to mean absent —
empty is not the same as "zeros for every point."

**Half-open cells.** A point on the shared face of two cells belongs to the higher-index
cell (or neither, depending on your classifier). Be consistent with the half-open
contract when voxelizing.

**Cubic cells only.** `CellSize` is a single `Number` — anisotropic voxels are not
representable without a different type. Stretching space before sampling is the workaround.

**Level set vs full SDF grid.** `LevelSetGrid3D` stores accurate distances only in a
narrow band. Far away you only know the sign via `BackgroundDistance`. Do not treat every
cell as an exact SDF sample.

**Sparse sort order.** `SparseVoxelGrid3D.Coordinates` are sorted lexicographically Z,
then Y, then X. Binary search depends on that order; inserting unsorted breaks lookups.

**Brick emptiness.** Regions with no brick in a `BrickMap3D` are empty — not zero density.
Missing brick ≠ brick of zeros unless you define it that way in your producer contract.

**Occupancy is conservative.** `Voxelizable3D.ToOccupancyGrid` is documented as
*conservative* rasterization: cells that touch the solid may mark occupied even if the
cell center is outside. Fine for collision pads; coarse for volume measurement.

**Normals are directions.** `OrientedPointCloud3D` uses `Direction3D` (unit). Sensor
estimators that produce unnormalized vectors must normalize (or reject) before packing.

## Try it

1. `Origin = (0,0,0)`, `CellSize = 2`. Which cell contains world point $(3.5, 0.5, 1.0)$?
2. Why store intensities as a separate optional channel instead of overloading `Color`
   alpha?
3. A solid fills a $1\times1\times1$ cube. You voxelize with `CellSize = 1`. Why might
   more than one cell be marked occupied under conservative rasterization?

<details>
<summary>Answers</summary>

1. Floor of $(3.5, 0.5, 1.0)/2 = (1, 0, 0)$ → cell $(1,0,0)$, spanning $[2,4)\times[0,2)\times[0,2)$.
2. Intensity is a sensor physical quantity, not a display color; many points have intensity
   without RGB. Keeping channels separate matches real scan pipelines and absent-data
   conventions.
3. The cube touches face-adjacent cells (and possibly more at edges/corners) even when
   cell centers lie outside the solid; conservative voxelization marks all touched cells.

</details>

## Library recommendations

- **missing-function** — `33-pointclouds-voxels.plato`: grids declare Origin/CellSize but
  no `WorldToCell`, `CellBounds`, or `SampleTrilinear` helpers. Every lesson example has
  to restate the half-open mapping; those operations belong on the types.

- **missing-concept** — `33-pointclouds-voxels.plato`: `LevelSetGrid3D` does not implement
  `SignedDistanceField3D` / `ScalarField3D`, so it cannot `Eval` like `SampledSdf3D`
  (file 27). Bridging level-set grids into the field vocabulary would unify sampling.

- **wrong-shape** — `33-pointclouds-voxels.plato` vs `29-sampling-grids.plato`: dense
  volumes use Origin+CellSize here but Bounds+CellCounts in sampled scalar grids. Two
  parameterizations for "regular 3D lattice" force converters and confuse teaching.
  Pick one canonical grid header or declare explicit conversions.

- **doc-comment** — `33-pointclouds-voxels.plato`: `AttributedPointCloud3D` says empty
  channel arrays mean absent, but does not say whether partially filled (length mismatch)
  is illegal. State the invariant: each non-empty channel length equals `Positions` length.
