---
lesson: voxel-grid-sampling
title: Voxel Grids and Sampling Continuous Fields
domain: Meshes & spatial structures
v3-files: [33-pointclouds-voxels.plato, 29-sampling-grids.plato]
audience: Comfortable with 3D arrays and the idea of a function of position; no graphics pipeline experience required
status: draft-v1
---

# Voxel Grids and Sampling Continuous Fields

A smoke cloud, a medical scan, and a Minecraft world disagree about almost
everything — except this: somewhere you store a value per little cube of space.
That lattice is a **voxel grid**. The hard part is not the cubes; it is the
contract that maps world positions to cell indices, and the rule that
reconstructs a continuous field when you ask for a value *between* cell centres.

Get the contract wrong and every downstream tool — raymarching, meshing,
physics — silently shifts by half a cell.

## The idea

### Cells vs nodes

Two common lattices:

1. **Cell-centred (voxel) storage.** Value $V_{i,j,k}$ lives in the half-open
   cube covering
   $[o + (i,j,k)s,\; o + (i+1,j+1,k+1)s)$ where $o$ is the origin of cell
   $(0,0,0)$ and $s$ is the cell edge length. Occupancy, Minecraft blocks,
   many volume textures.
2. **Node-centred (sampled field) storage.** Values sit on lattice *points*
   (the corners of cells). Between nodes you interpolate. Heightmaps,
   simulation grids, FEM-style fields.

```
  cell-centred                 node-centred
  +---+---+---+                *---*---*---*
  | 0 | 1 | 2 |                |   |   |   |
  +---+---+---+                *---*---*---*
  | 3 | 4 | 5 |                |   |   |   |
  +---+---+---+                *---*---*---*
  origin at min corner         values at corners;
  of cell (0,0)                cell count = node count - 1
```

Mixing the two without converting is a classic off-by-half bug: a density that
was meant for cell centres gets trilinearly blended as if it lived on nodes.

### World ↔ index

For cell-centred voxels with origin $o$ and size $s$:

$$
i = \left\lfloor\frac{x - o_x}{s}\right\rfloor
\quad\text{(similarly }j,k\text{)}
$$

Points exactly on a positive face belong to the next cell (half-open intervals).
Out-of-range indices need an explicit **boundary** policy: clamp, wrap, mirror,
or read as empty/zero.

### Reconstruction

To treat a node lattice as a continuous `ScalarField3D`, pick an
**interpolation scheme**:

| Scheme | Feel |
|---|---|
| Nearest | blocky; preserves exact stored samples |
| Trilinear | standard smooth volume lookup |
| Tricubic / Catmull-Rom | smoother; can overshoot |

Resampling copies one lattice onto another: same continuous idea, new spacing.

### Sparse vs dense

Most of space is empty. Dense `Array3D` wastes memory; sparse stores only
occupied integer coordinates, or two-level **bricks** (dense blocks hung on a
coarse sparse index). The world↔index math stays the same; only storage
changes.

## In Plato

v3 makes the cell-centred contract explicit on every dense voxel type, and
puts node-centred fields in the sampling file.

From `33-pointclouds-voxels.plato`:

```plato
// Origin is the world position of the minimum corner of cell (0, 0, 0),
// CellSize is the world-space edge length of one cubic cell...
// Cell (i, j, k) spans the half-open world box
// [Origin + (i,j,k)*CellSize, Origin + (i+1,j+1,k+1)*CellSize).

type VoxelGrid3D<T>
{
    Origin: Point3D;
    CellSize: Number;
    Values: Array3D<T>;
}

type OccupancyGrid3D
    implements Value
{
    Origin: Point3D;
    CellSize: Number;
    Occupied: Array3D<Boolean>;
}

type DensityGrid3D
    implements Value
{
    Origin: Point3D;
    CellSize: Number;
    Values: Array3D<Number>;
}

type LevelSetGrid3D
    implements Value
{
    Origin: Point3D;
    CellSize: Number;
    Distances: Array3D<Number>;
    NarrowBandWidth: Number;
    BackgroundDistance: Number;
}

type SparseVoxelGrid3D<T>
{
    Origin: Point3D;
    CellSize: Number;
    Coordinates: Array<IntegerVector3>;
    Values: Array<T>;
}

concept Voxelizable3D
{
    ToOccupancyGrid(x: Self, cellSize: Number): OccupancyGrid3D;
}
```

From `29-sampling-grids.plato`:

```plato
type RegularGrid3D
    implements Value
{
    Bounds: Bounds3D;
    CellCounts: IntegerVector3;
}

// Field values stored one per grid node ... extent equals cell count + 1
type SampledScalarGrid3D
    implements Value, ScalarField3D
{
    Grid: RegularGrid3D;
    Values: Array3D<Number>;
}

type InterpolationScheme
    = Nearest
    | Linear
    | Bilinear
    | Bicubic
    | Trilinear
    | Tricubic
    | CatmullRom
    | CubicBSpline;

type GridBoundary = ClampToEdge | Wrap | Mirror | ZeroOutside;

type GridSamplingScheme
    implements Value
{
    Interpolation: InterpolationScheme;
    Boundary: GridBoundary;
}

type GridResampling3D
    implements Value
{
    Source: RegularGrid3D;
    Target: RegularGrid3D;
    Scheme: GridSamplingScheme;
}
```

Usage-shaped sketches:

```plato
occ = OccupancyGrid3D {
    Origin: Point3D { X: 0.0; Y: 0.0; Z: 0.0 };
    CellSize: 0.25;
    Occupied: occupiedCells;   // Array3D<Boolean>
}

// Continuous density on nodes (note: SampledScalarGrid3D, not DensityGrid3D)
field = SampledScalarGrid3D {
    Grid: RegularGrid3D {
        Bounds: Bounds3D {
            Min: Point3D { X: 0.0; Y: 0.0; Z: 0.0 };
            Max: Point3D { X: 10.0; Y: 10.0; Z: 10.0 };
        };
        CellCounts: IntegerVector3 { X: 32; Y: 32; Z: 32 };
    };
    Values: nodeSamples;       // dimensions 33×33×33
}

scheme = GridSamplingScheme {
    Interpolation: Trilinear;
    Boundary: ClampToEdge;
}

resample = GridResampling3D {
    Source: field.Grid;
    Target: finerGrid;
    Scheme: scheme;
}

// Rasterize any Voxelizable3D solid into cells of size 0.1
voxels = shape.ToOccupancyGrid(0.1);
```

`DensityGrid3D` is cell-centred unitless samples; `SampledScalarGrid3D` is a
`ScalarField3D` with documented trilinear evaluation between nodes. Same noun
“grid,” different sampling semantics — choose deliberately.

## Pitfalls / fine print

- **Half-open cells.** A point on the shared face of cells $(i)$ and $(i+1)$
  belongs to $(i+1)$. Dual-write bugs appear if one system uses closed intervals.
- **CellSize is `Number`, Bounds use points.** Voxel files do not use `Length`;
  keep a single world unit consistent with the rest of the scene.
- **Cubic cells only** on the voxel types (`CellSize` is one scalar). Anisotropic
  medical voxels need a different representation (or accept resampling onto
  cubes first).
- **Sparse coordinate order.** `SparseVoxelGrid3D` requires lexicographic
  $Z,Y,X$ sorted unique coordinates — binary search assumes that invariant.
- **Level sets.** `LevelSetGrid3D` stores distances at cell centres (same
  Origin/CellSize contract) but semantically wants smooth interpolation for
  zero-crossing extraction; nearest-neighbour meshing looks terraced.
- **`InterpolationScheme` includes `Bilinear` and `Trilinear`.** Using bilinear
  on a 3D grid is a type-level footgun waiting for a library body to reject it.

## Try it

1. Origin $(0,0,0)$, cell size $1$. Which cell contains world point
   $(2.0, 0.5, 0.5)$? Which contains $(2.0, 0.0, 0.0)$ on a face?
2. A `RegularGrid3D` has `CellCounts = (4,4,4)`. How many values must
   `SampledScalarGrid3D.Values` hold?
3. You have a `DensityGrid3D` (cell-centred) and want a smooth
   `ScalarField3D`. What goes wrong if you wrap it as `SampledScalarGrid3D`
   without converting?

<details>
<summary>Answers</summary>

1. $(2,0,0)$ for the interior point. The face point $(2,0,0)$ is the minimum
   corner of cell $(2,0,0)$ (still that cell under half-open rules), not cell
   $(1,*,*)$.
2. Nodes per axis $= 4+1 = 5$, so $5^3 = 125$ samples.
3. You shift the field by half a cell and invent an extra “layer” of nodes that
   never existed. Reconstruct from cell centres (offset the lattice by
   $s/2$) or resample onto an explicit node grid first.

</details>

## Library recommendations

- **missing-function** — `33-pointclouds-voxels.plato`: no
  `WorldToCell(grid, point): IntegerVector3` /
  `CellBounds(grid, cell): Bounds3D` helpers, even though the file banner
  defines the mapping in prose. Every consumer re-implements floor division
  and half-open edge cases.
- **missing-function** — `29-sampling-grids.plato` / `33-pointclouds-voxels.plato`:
  no conversion between cell-centred `DensityGrid3D` and node-centred
  `SampledScalarGrid3D` (offset by half `CellSize`, or box-filter resample).
  The sampling lesson’s central pitfall has no typed operation.
- **wrong-shape** — `29-sampling-grids.plato`: `InterpolationScheme` mixes 2D
  and 3D constructors in one sum (`Bilinear` beside `Trilinear`). Prefer
  separate 2D/3D scheme types so `GridSamplingScheme` for 3D cannot name
  bilinear.
- **missing-type** — `33-pointclouds-voxels.plato`: only isotropic `CellSize`.
  Medical volumes need `CellSize: Vector3D` (or `Number3`); teaching anisotropic
  sampling currently requires a disclaimer that v3 cannot represent it.
- **doc-comment** — `33-pointclouds-voxels.plato`: `DensityGrid3D` should
  cross-reference `SampledScalarGrid3D` and state “cell-centred, not a
  `ScalarField3D` by itself” so authors do not assume `Eval` exists on it.
