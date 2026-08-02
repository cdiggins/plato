---
lesson: noise
title: Procedural Noise
domain: Fields, implicits & noise
v3-files: [28-noise.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Procedural Noise

"Random" textures that look natural are not coin flips. A heightmap for hills must change
smoothly from sample to sample; a marble shader must look the same every frame; a particle
swirl must never invent sources or sinks out of nowhere. Procedural noise is engineered
randomness: deterministic functions of position (and a seed) that are coherent enough to
shade, sculpt, and animate with, yet irregular enough to break repetition. Plato's v3
noise vocabulary is a catalog of those engineered fields — value, gradient, cellular,
spectral, fractal, and divergence-free — all implementing ordinary scalar or vector field
interfaces so they plug into the same evaluation pipeline as any other field.

## The idea

True white noise assigns independent random values to every point. Adjacent samples are
uncorrelated, so the result looks like television static — useful for dithering, useless
for terrain. **Coherent noise** correlates nearby samples.

**Value noise** plants random scalars on an integer lattice and interpolates between them.
Cheap and continuous, but the lattice orientation often shows as faint axis-aligned
artifacts.

**Gradient (Perlin) noise** plants random *directions* (gradients) on the lattice. At a
query point you take the dot product of each corner gradient with the offset to that
corner, then interpolate the dots. The field is zero at lattice points and band-limited —
visually smoother than value noise for the same frequency.

**Simplex noise** uses a triangular (2D) or tetrahedral (3D) lattice instead of a square
grid. Fewer directional artifacts, and the cost scales better as dimension grows.

**Worley (cellular) noise** scatters feature points (typically one per cell, with
optional jitter) and reports distances to the nearest, second-nearest, and so on. Patterns
look cracked, crystalline, or biological depending on which feature combination you keep
($F_1$, $F_2 - F_1$, …).

**Gabor noise** convolves sparse oriented kernels, giving direct control of the spectrum:
stripe direction, bandwidth, anisotropy.

One octave of any basis is a single frequency band. Natural phenomena are multi-scale.
**Fractal Brownian motion (fBM)** sums octaves: each layer multiplies frequency by
**lacunarity** (often 2) and amplitude by **gain** (often 0.5):

$$
n(p) = \sum_{i=0}^{N-1} g^i \, B\!\big(f \cdot \ell^i \cdot p\big)
$$

**Turbulence** sums absolute values of each octave (billowy creases). **Ridged** noise
inverts absolutes and offsets them (mountain ridges, lightning). **Domain warping**
displaces the sample point by another noise field before evaluating the basis — the
classic path to marble and fluid-like swirls.

**Curl noise** builds a *vector* field that is divergence-free: particles advected by it
neither clump nor explode. In 2D that is a 90° rotation of a scalar potential's gradient;
in 3D it is the curl of a vector potential.

## In Plato

Every noise type in `28-noise.plato` is a deterministic `Value` implementing
`ScalarField2D` / `ScalarField3D` (or the vector-field variants for curl). Evaluation goes
through `Procedural.Eval`. Scalar bases nominally range over $[-1, 1]$ unless noted;
white noise is $[0, 1]$; Worley is non-negative.

```plato
type NoiseBasis = White | Value | Perlin | Simplex | Worley | Gabor;

type PerlinNoise3D
    implements Value, ScalarField3D
{
    Seed: Integer;
    Frequency: Number;
}

type WorleyNoise3D
    implements Value, ScalarField3D
{
    Seed: Integer;
    Frequency: Number;
    Jitter: Number;              // 0 = cell centers, 1 = fully random in cell
    Distance: WorleyDistance;    // Euclidean | Manhattan | Chebyshev | Minkowski
    Feature: WorleyFeature;      // F1 | F2 | F2MinusF1 | F1PlusF2
}
```

Fractal wrappers pick a basis by enum rather than embedding another noise record:

```plato
type FbmNoise3D
    implements Value, ScalarField3D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    Octaves: Integer;
    Lacunarity: Number;
    Gain: Number;
}

type TurbulenceNoise3D { /* same fields as Fbm */ }
type RidgedNoise3D
{
    /* Fbm fields plus */
    Offset: Number;   // typically 1
}
```

Domain warp and curl:

```plato
type DomainWarpNoise3D
    implements Value, ScalarField3D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    WarpStrength: Number;
    WarpFrequency: Number;
    Iterations: Integer;
}

type CurlNoise3D
    implements Value, VectorField3D
{
    Seed: Integer;
    Frequency: Number;
}
```

Usage-shaped snippets:

```plato
hills = FbmNoise3D(Perlin, seed, 0.02, 5, 2.0, 0.5)
height = Eval(hills, Point3D(x, 0, z))

cells = WorleyNoise2D(seed, 4.0, 0.8, Euclidean, F2MinusF1)
cracks = Eval(cells, uv)   // bright ridges along cell borders

flow = CurlNoise3D(seed, 1.5)
v = Eval(flow, p)          // Vector3D, divergence-free advection
```

Gabor exposes spectral knobs directly:

```plato
type GaborNoise2D
    implements Value, ScalarField2D
{
    Seed: Integer;
    Frequency: Number;
    Orientation: Angle;
    Bandwidth: Number;
    Anisotropy: Number;   // 0 isotropic … 1 fully oriented
}
```

## Pitfalls / fine print

**Seed is part of the value.** Same parameters and seed ⇒ same field forever. Changing
seed is a different texture, not "more random over time." Animate with domain offset or
a time-varying field interface, not by reseeding every frame.

**Frequency vs world size.** Frequency scales the input domain. Doubling frequency halves
feature size. Mixing a noise designed in unit space with a 1000-unit scene without
scaling frequency produces either mush or spaghetti.

**Octave explosion.** Each octave multiplies cost. Past ~6–8 octaves the high frequencies
are often lost to screen sampling anyway (aliasing). Prefer fewer octaves plus a detail
map over blindly cranking `Octaves`.

**Basis enum vs parameterized bases.** `FbmNoise3D.Basis` is a `NoiseBasis` tag. Worley
and Gabor need extra parameters (`Jitter`, `Feature`, `Orientation`, …) that the fractal
wrapper does not carry. Using `Worley` or `Gabor` as an fBM basis implies library defaults
— or a hole in the API (see recommendations).

**Range assumptions.** Shaders that expect $[0,1]$ will break on Perlin's $[-1,1]$. Remap
explicitly: $(n+1)/2$. Turbulence and Worley are already non-negative but not normalized
to a fixed max.

**Lattice artifacts.** Value noise and axis-aligned Perlin can show grid bias. If a
texture "looks square," try Simplex, rotate the domain, or domain-warp lightly.

**Curl is not a scalar.** `CurlNoise2D` / `CurlNoise3D` implement vector fields. You
cannot feed them to an API that wants `ScalarField3D` without taking a component or
magnitude (and magnitude is *not* divergence-free).

**Domain warp iterations.** Each iteration displaces by warp noise again. Cost multiplies;
strong `WarpStrength` folds space so hard that the basis frequency no longer means what
you think.

## Try it

1. One octave of Perlin at frequency $f$ has features of characteristic size roughly
   $1/f$. What characteristic size does octave $i$ of fBM have if lacunarity is 2?
2. Why is white noise a poor heightmap even though it is "more random"?
3. You want cell borders for a cracked-mud material. Which `WorleyFeature` is the usual
   choice, and why?

<details>
<summary>Answers</summary>

1. About $1/(f \cdot 2^i)$.
2. Adjacent samples are uncorrelated, so the surface has no slope continuity — pure
   spikes. Coherent noise exists precisely to supply spatial correlation.
3. `F2MinusF1`: distance to the second-nearest feature minus distance to the nearest
   peaks along the Voronoi edges (cell borders) and is low inside cells.

</details>

## Library recommendations

- **wrong-shape** — `28-noise.plato`: `FbmNoise2D` / `FbmNoise3D` (and turbulence/ridged/
  warp) select `Basis: NoiseBasis`, but `Worley` and `Gabor` require parameters that the
  fractal types do not store. Either document mandatory defaults for those bases, or
  replace the enum with a sum that can carry Worley/Gabor payloads (or nest a basis
  noise value). Teaching fBM+Worley currently forces a silent assumption.

- **missing-function** — `28-noise.plato`: there is no declared `NoiseGradientAt` /
  analytic derivative companion for `PerlinNoise*` / `SimplexNoise*`, even though
  `DifferentiableScalarField3D.GradientAt` exists in `26-fields.plato`. Noise-as-terrain
  needs slopes; without a declared gradient story, implementors invent incompatible
  numeric schemes.

- **doc-comment** — `28-noise.plato`: file banner says scalar noises nominally range over
  $[-1, 1]$, but `WhiteNoise*` is $[0,1]$ and `Worley*` is non-negative with no stated
  upper bound. Per-type range lines in the doc comments would prevent the classic remap
  bugs the lesson has to warn about.

- **missing-type** — `28-noise.plato`: `CurlNoise3D` has only `Seed` and `Frequency` —
  no octave/lacunarity controls and no choice of potential basis. Multi-scale curl
  (common for smoke) has to be layered by the caller with no vocabulary support.
