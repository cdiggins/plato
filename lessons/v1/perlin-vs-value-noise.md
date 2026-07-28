---
lesson: perlin-vs-value-noise
title: Perlin Noise vs Value Noise
domain: Fields, implicits & noise
v3-files: [28-noise.plato]
audience: Comfortable with interpolating values on a grid; no signal-processing background assumed
status: draft-v1
---

# Perlin Noise vs Value Noise

You want randomness that does not look like static: hills that connect smoothly,
clouds without speckles, a wood grain that varies but never pops a hard edge.
**Value noise** and **Perlin (gradient) noise** both deliver coherent fields
from a lattice of random data — but they store different random things, and
your eye can tell.

## The idea

### Shared skeleton

Both families:

1. Lay an integer lattice over the domain (scaled by a **frequency**).
2. Attach random data to each lattice point (from a deterministic **seed**).
3. For a query point, find the surrounding cell and **smoothly interpolate**
   the lattice contributions.

Same frequency → similar feature size. Same seed → reproducible field. Output
is typically scaled into roughly $[-1,1]$ for signed noises (white noise in
v3 is the odd one out, ranging $[0,1]$).

```
  lattice values (value noise)          lattice gradients (Perlin)
  * 0.2 ---- * -0.7                     * → ---- * ↓
  |          |                          |        |
  |    x     |                          |   x    |
  |          |                          |        |
  * 0.5 ---- *  0.1                     * ↗ ---- * →
```

### Value noise

Each lattice point stores a **scalar**. Inside a cell you interpolate those
scalars (often with a smoothstep / Hermite fade, not a raw linear blend).

- Cheap to evaluate.
- Tends to show the **lattice orientation**: blotchy axis-aligned artifacts,
  especially when you stretch frequency differently on axes.
- Local maxima often sit on lattice points (where the stored values peak).

### Perlin gradient noise

Each lattice point stores a **random unit gradient** (a direction). The
contribution of a corner is the **dot product** of that gradient with the
offset vector from the corner to the query point. Interpolate those scalar
contributions with a fade curve.

Ken Perlin’s insight: dots with offsets are zero when you sit *on* a lattice
point (offset $=0$), so the noise is **zero at every lattice node**. Between
nodes it rises and falls with a characteristic band-limited look. Visually
smoother, fewer obvious grid artifacts than value noise at the same lattice
density.

### Side-by-side intuition

| | Value | Perlin |
|---|---|---|
| Lattice data | scalar | gradient |
| At lattice points | random value | $0$ |
| Look | blotchy, can show grid | swirly, band-limited |
| Cost | lower | slightly higher (dots) |
| Typical use | cheap hash fields, Minecraft-ish | terrain base, textures |

Neither is “true Gaussian random.” Both are **structured**: periodicity of
the lattice (or of a permutation table) can appear if you zoom out far enough
or tile carelessly.

### Simplex (briefly)

**Simplex** noise replaces the square/cubic lattice with triangles/tetrahedra,
cutting directional artifacts and improving higher-dimensional cost. It is the
usual upgrade path when Perlin’s square look still bothers you — same job
description (gradient noise), different lattice.

## In Plato

Basis kinds are an explicit sum; each concrete noise is a `ScalarField2D/3D`.

From `28-noise.plato`:

```plato
type NoiseBasis = White | Value | Perlin | Simplex | Worley | Gabor;

// Planar value noise: random values on an integer lattice, smoothly
// interpolated between lattice points. Cheap, but shows lattice artifacts.
type ValueNoise2D
    implements Value, ScalarField2D
{
    Seed: Integer;
    Frequency: Number;
}

type ValueNoise3D
    implements Value, ScalarField3D
{
    Seed: Integer;
    Frequency: Number;
}

// Planar Perlin gradient noise: random gradients on an integer lattice with
// smooth interpolation. Zero at lattice points; band-limited and visually
// smooth.
type PerlinNoise2D
    implements Value, ScalarField2D
{
    Seed: Integer;
    Frequency: Number;
}

type PerlinNoise3D
    implements Value, ScalarField3D
{
    Seed: Integer;
    Frequency: Number;
}

type SimplexNoise2D
    implements Value, ScalarField2D
{
    Seed: Integer;
    Frequency: Number;
}

type WhiteNoise2D
    implements Value, ScalarField2D
{
    Seed: Integer;
}
```

Usage-shaped sketches:

```plato
hills = PerlinNoise2D {
    Seed: 42;
    Frequency: 0.05;    // smaller → larger features
};

blobs = ValueNoise2D {
    Seed: 42;
    Frequency: 0.05;    // same frequency, different character
};

// Sample as a scalar field (Procedural Eval from Field → Procedural)
h = hills.Eval(Point2D { X: 10.0; Y: 3.0 });
v = blobs.Eval(Point2D { X: 10.0; Y: 3.0 });

// White noise: no Frequency — uncorrelated per point
static = WhiteNoise2D { Seed: 7 };

// Pick a basis by tag when a fractal wrapper needs one (see Fbm types)
basis = Perlin;   // NoiseBasis sum case
```

`ScalarField2D` inherits `Field<Point2D, Number>` which inherits
`Procedural<Point2D, Number>`, so evaluation is `Eval(field, point)`.
Frequency scales the input domain: doubling frequency halves feature size.

v3 does **not** expose the fade curve, gradient table, or periodic wrap size
as fields — those are implementation choices behind the type. Lessons can
contrast Value vs Perlin using only `Seed` and `Frequency`.

## Pitfalls / fine print

- **Calling everything “Perlin.”** Internet code samples often say Perlin and
  ship value noise. Believe the lattice data, not the label.
- **Frequency $0$.** Degenerate; every point maps to the same lattice cell
  neighbourhood. Keep frequency positive.
- **Anisotropic stretch.** Scaling $x$ and $y$ differently before Eval reveals
  value-noise lattice artifacts faster than Perlin — a useful A/B test.
- **Range.** Signed noises are *nominally* $[-1,1]$; implementations sometimes
  slightly exceed that. Do not assume hard clamps unless you clamp yourself.
- **White vs Value.** White has no spatial coherence and no `Frequency`. Using
  it as a “cheap Perlin” produces sparkle, not hills.
- **Seed portability.** Same seed must reproduce across backends once library
  bodies exist; until then, treat seeds as logical, not bit-identical across
  hosts.

## Try it

1. At a lattice point after frequency scaling, what does Perlin return (ideally)?
   What about value noise?
2. You double `Frequency` on `PerlinNoise2D`. Do features get larger or smaller?
3. Why might `ValueNoise2D` look more “tiling square” than `PerlinNoise2D` at
   the same frequency?

<details>
<summary>Answers</summary>

1. Perlin ≈ $0$ (all offset dots vanish). Value noise ≈ the random scalar
   stored at that lattice node (generally nonzero).
2. Smaller — higher frequency packs more lattice cells into the same world
   distance.
3. Value noise interpolates scalars whose extrema often align with the square
   grid; gradient noise’s zero-at-nodes plus directional dots break up that
   axis-aligned blotch pattern.

</details>

## Library recommendations

- **doc-comment** — `28-noise.plato`: `ValueNoise2D/3D` and `PerlinNoise2D/3D`
  should state the intended output range and whether values may slightly
  exceed $[-1,1]$. Compare lessons always hit this ambiguity.
- **missing-function** — `28-noise.plato`: no
  `Fade(t: Number): Number` / documented quintic fade as part of the public
  contract. Teaching Perlin without a named fade makes “smooth interpolation”
  hand-wavy relative to the rest of v3’s explicitness.
- **pedagogy** — `28-noise.plato`: `NoiseBasis` lists `Value` and `Perlin` as
  peers (good) but nothing in the type system stops someone from assuming they
  are interchangeable in an `FbmNoise2D`. A short banner comment that “basis
  choice changes visual family, not just cost” would match what this lesson
  teaches.
- **missing-type** — `28-noise.plato`: no parameters for lattice period /
  wrapping (`Period: IntegerVector2`). Tiling textures need seamless noise;
  authors currently fake it with domain tricks outside the type.
- **naming** — `28-noise.plato`: consider documenting “gradient noise” as a
  synonym in the Perlin doc comment. Learners searching for gradient noise
  otherwise miss the type they want and reinvent value noise under a new name.
