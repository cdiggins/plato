---
lesson: sampling-and-grids
title: Sampling and Grids
domain: Fields, implicits & noise
v3-files: [29-sampling-grids.plato, 45-images.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Sampling and Grids

A continuous field knows a value at every real-valued point. Computers store finitely many
samples. The moment you bake a heightmap, a simulation density, or a photograph, you face
the same two questions: *where do the samples live in space?* and *how do you reconstruct
values between them?* Regular grids answer the first with lattice geometry; interpolation
schemes and images answer the second. Plato keeps the lattice metadata beside the arrays
so a raw `Array2D` never pretends to know its world placement.

## The idea

A **regular grid** partitions a bounding region into axis-aligned cells of equal size.
In 2D, if the bounds are a rectangle and you ask for $m \times n$ cells, you get
$(m+1) \times (n+1)$ **nodes** (lattice corners). Cell $(0,0)$ touches the minimum corner
of the bounds.

```
  Bounds min                         Bounds max
       +-----+-----+-----+-----+
       |     |     |     |     |
       +-----+-----+-----+-----+   ← nodes at every +
       |     |     |     |     |
       +-----+-----+-----+-----+
         cell (0,0) →
```

Sampled fields store one value **per node**. Evaluating at an arbitrary point means:

1. Map the world point into continuous grid coordinates $(u, v)$.
2. Find the enclosing cell.
3. **Interpolate** from the corner samples.

**Nearest** picks the closest node (blocky). **Bilinear** (2D) / **trilinear** (3D)
weights the $2^n$ corners by fractional position — the workhorse. Higher-order schemes
(bicubic, Catmull-Rom, B-spline) use a larger neighborhood for smoother results and
different overshoot behavior.

Outside the bounds you must pick a **boundary policy**: clamp to the edge value, wrap
(tile), mirror, or read zero.

Images are the special case where the "field" is color (or grayscale, depth, …) and the
domain is pixel index space rather than an arbitrary world `Bounds2D`. A `GrayscaleImage`
is still a sampled scalar field in spirit — Plato just gives it a dedicated raster type
with codec-friendly layout conventions.

Scattering patterns (Poisson disk, Halton, blue noise, …) answer a different sampling
problem: place *points* in a region for integration, stippling, or meshing — not storing
a dense lattice.

## In Plato

Grids and sampled fields (`29-sampling-grids.plato`):

```plato
type RegularGrid2D
    implements Value
{
    Bounds: Bounds2D;
    CellCounts: IntegerVector2;
}

type SampledScalarGrid2D
    implements Value, ScalarField2D
{
    Grid: RegularGrid2D;
    Values: Array2D<Number>;
}

type SampledVectorGrid3D
    implements Value, VectorField3D
{
    Grid: RegularGrid3D;
    Values: Array3D<Vector3D>;
}

type SampledColorGrid2D
    implements Value, ColorField2D
{
    Grid: RegularGrid2D;
    Values: Array2D<Color>;
}
```

`SampledScalarGrid2D` implements `ScalarField2D`, so `Eval` reconstructs by bilinear
interpolation (per the type's doc comment). Cell helpers carry both index and world box:

```plato
type GridCell2D
{
    CellIndex: IntegerVector2;
    Bounds: Bounds2D;
}
```

Reconstruction policy when you need more than the default:

```plato
type InterpolationScheme
    = Nearest | Linear | Bilinear | Bicubic
    | Trilinear | Tricubic | CatmullRom | CubicBSpline;

type GridBoundary = ClampToEdge | Wrap | Mirror | ZeroOutside;

type GridSamplingScheme
{
    Interpolation: InterpolationScheme;
    Boundary: GridBoundary;
}

type GridResampling2D
{
    Source: RegularGrid2D;
    Target: RegularGrid2D;
    Scheme: GridSamplingScheme;
}
```

Sample patterns for point sets:

```plato
type PoissonDiskPattern2D
{
    Region: Bounds2D;
    MinimumDistance: Number;
    MaxAttempts: Integer;
    Seed: Integer;
}

type HaltonPattern2D
{
    Region: Bounds2D;
    Count: Integer;
    BaseX: Integer;
    BaseY: Integer;   // typically 2 and 3, coprime
}
```

Images (`45-images.plato`) — rasters with explicit size, separate from world grids:

```plato
interface Image
{
    Size(x: Self): IntegerSize2D;
    Width(x: Self): Integer;
    Height(x: Self): Integer;
}

type Bitmap
    implements Image
{
    Size: IntegerSize2D;
    Pixels: Array<Color8>;
}

type FloatImage
    implements Image
{
    Size: IntegerSize2D;
    Pixels: Array<Color>;
}

type GrayscaleImage
    implements Image
{
    Size: IntegerSize2D;
    Values: Array<Number>;
}
```

Usage-shaped snippets:

```plato
grid = RegularGrid2D(bounds, IntegerVector2(256, 256))
height = SampledScalarGrid2D(grid, samples)   // samples: 257×257 nodes
h = Eval(height, Point2D(x, y))               // bilinear in Bounds

scheme = GridSamplingScheme(Bicubic, ClampToEdge)
resample = GridResampling2D(hiRes, loRes, scheme)

img = GrayscaleImage(IntegerSize2D(512, 512), pixels)
// pixel (i, j) ↔ discrete sample; world placement is a separate transform
```

`SampledCurve2D` / `SampledSurfaceGrid` apply the same idea to parametric geometry:
store parallel parameter and position arrays, interpolate between samples.

## Pitfalls / fine print

**Cells vs nodes.** `CellCounts` is the number of cells. Node counts are one greater per
axis. An `Array2D` whose dimensions equal `CellCounts` is off-by-one for node-sampled
fields — a perennial bug.

**Row-major and origin.** Image pixels are row-major from the top-left unless stated
otherwise (`ImageOrigin` distinguishes `TopLeft` vs `BottomLeft` for interop). World
grids put cell $(0,0)$ at the **minimum** corner of `Bounds`. Mixing image $y$-down with
world $y$-up flips textures.

**Bilinear on non-scalars.** `SampledVectorGrid2D` interpolates **per component**. That is
usually right for velocities; for directions you may need renormalization after lerp
(directions live in `DirectionField` interfaces elsewhere — a sampled direction grid is not
declared in file 29).

**Boundary default.** Doc comments say evaluation outside bounds clamps to the edge
unless a resampling descriptor says otherwise. Wrapping textures must pass `Wrap`
explicitly via `GridSamplingScheme`.

**Color space.** `SampledColorGrid2D` stores `Color` (linear). `Bitmap` stores `Color8`
(sRGB-encoded). Filtering in sRGB is a classic wrong look — upsample/filter in linear
(`FloatImage`) when quality matters.

**Low-discrepancy vs Poisson.** Halton/Hammersley/Sobol reduce integration variance;
Poisson disk enforces a minimum distance for even *visual* distribution. They solve
different problems; swapping them for "nice samples" is a category error.

**Hammersley needs fixed count.** Unlike Halton, Hammersley embeds $i/N$ and is not
progressively refinable by simply taking a longer prefix with a new $N$.

## Try it

1. A `RegularGrid2D` has `CellCounts = (2, 1)`. How many nodes does `Values` need for a
   `SampledScalarGrid2D`?
2. At the exact center of a cell whose four corner values are $0, 0, 1, 1$ along one
   diagonal pair of opposites being 0 and the other 1 — what does bilinear give?
3. Why is `Eval` on `SampledScalarGrid2D` enough for many apps, yet `GridSamplingScheme`
   still exists?

<details>
<summary>Answers</summary>

1. Nodes are $(2+1) \times (1+1) = 3 \times 2 = 6$.
2. Bilinear at the cell center averages the four corners: $(0+0+1+1)/4 = 0.5$.
3. The type's default Eval commits to bilinear + clamp-style behavior. Resampling between
   differently shaped grids, bicubic reconstruction, or wrap/mirror boundaries need an
   explicit `GridSamplingScheme` / `GridResampling2D`.

</details>

## Library recommendations

- **missing-function** — `29-sampling-grids.plato`: `RegularGrid2D` / `RegularGrid3D`
  declare layout but no helpers such as `NodeCounts`, `CellSize`, `WorldToGrid`, or
  `CellAt(point)`. Every consumer re-derives the off-by-one node rule; teaching it in
  prose is a sign the API should own those operations.

- **missing-type** — `29-sampling-grids.plato`: there is `SampledColorGrid2D` but no
  `SampledColorGrid3D`, and no sampled `DirectionField` grid. Volume color and sampled
  orientation fields are common; file 33's `VoxelColorGrid3D` is a different
  parameterization (Origin/CellSize vs Bounds/CellCounts), which fractures the mental
  model.

- **naming** — `29-sampling-grids.plato` vs `45-images.plato`: `SampledColorGrid2D` and
  `FloatImage` / `GrayscaleImage` both store dense 2D samples with different metadata
  (`RegularGrid2D` vs `IntegerSize2D`). A doc-comment bridge ("image = grid in pixel
  index space") or a conversion interface would reduce the dual vocabulary the lesson must
  explain.

- **doc-comment** — `45-images.plato`: `Image` deliberately omits pixel accessors, which
  is fine, but nothing states how `GrayscaleImage` relates to `ScalarField2D`. Declaring
  an adapter or noting that images are not `Procedural` over `Point2D` would clarify why
  `Eval` works on grids but not on `Bitmap`.
