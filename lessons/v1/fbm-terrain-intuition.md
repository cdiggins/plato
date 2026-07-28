---
lesson: fbm-terrain-intuition
title: Fractal Brownian Motion for Terrain
domain: Fields, implicits & noise
v3-files: [28-noise.plato, 26-fields.plato]
audience: Already knows what a coherent noise field is (smooth pseudo-random heights); high-school exponents help
status: draft-v1
---

# Fractal Brownian Motion for Terrain

One octave of Perlin noise looks like gentle rolling hills — or like mashed
potatoes, depending on the frequency. Real landscapes mix **broad valleys**
with **fine ridges**. **Fractal Brownian motion (fBM)** builds that mix by
stacking several noise layers: each layer is finer and weaker than the last.
The same recipe makes clouds, wood, and marble; terrain is the intuition pump
because you can see amplitude as height.

## The idea

### Octaves, lacunarity, gain

Start with a basis noise $N(p)$ (often Perlin or Simplex). Define

$$
\mathrm{fBM}(p)
  = \sum_{i=0}^{O-1} G^{\,i}\;
    N\!\bigl(F\cdot L^{\,i}\cdot p\bigr)
$$

- $O$ — **octaves**: how many layers  
- $F$ — base **frequency**: size of the largest features  
- $L$ — **lacunarity**: frequency multiplier per octave (typically $2$)  
- $G$ — **gain** (persistence): amplitude multiplier per octave (typically $0.5$)

```
  octave 0: ~~~~           large, strong
  octave 1:  /\/\/         half amplitude, double frequency
  octave 2:  ^^^^^         quieter detail
  sum:      rough hills with fine crinkle
```

With $L=2$ and $G=0.5$, each octave adds detail at twice the spatial
frequency and half the height — a discrete nod at a $1/f$ power spectrum,
which is why the result feels “natural.”

### Why mountains need more than fBM

Plain fBM produces **rounded** hummocks: valleys and peaks are statistically
similar (just sign-flipped). Real eroded mountains have sharp ridges and
broader valleys. Two common variants:

- **Turbulence** — sum $|N|$ per octave → billowy, creasey, cloud-like  
- **Ridged** multifractal — invert absolute noise (offset by a constant) so
  sharp bright ridges dominate  

Domain warping (displace $p$ by another noise before sampling) adds
coastline wiggles without changing the octave story.

### From field to heightmap

Treat planar fBM as a `ScalarField2D`: height at ground point $(x,y)$. Sample
it onto a regular lattice for rendering, or keep it procedural and evaluate
per pixel. Rescale/bias the nominal $[-1,1]$-ish sum into metres of elevation
for a world.

Amplitude of the sum is roughly geometric:

$$
A \approx \sum_{i=0}^{O-1} G^i = \frac{1-G^O}{1-G}\quad (G\neq 1)
$$

So six octaves at $G=0.5$ reach almost the infinite-sum ceiling $2$ times the
base amplitude — adding a 10th octave barely moves large-scale elevation.

## In Plato

Fractal noises wrap a `NoiseBasis` and the octave knobs. Fields provide the
evaluation vocabulary and a way to compose scalar fields.

From `28-noise.plato`:

```plato
// Planar fractal Brownian motion: a sum of Octaves layers of the Basis noise
// with geometrically scaled frequency and amplitude. The workhorse for
// terrain, clouds, and natural textures.
type FbmNoise2D
    implements Value, ScalarField2D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    Octaves: Integer;
    Lacunarity: Number;
    Gain: Number;
}

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

type TurbulenceNoise2D
    implements Value, ScalarField2D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    Octaves: Integer;
    Lacunarity: Number;
    Gain: Number;
}

type RidgedNoise2D
    implements Value, ScalarField2D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    Octaves: Integer;
    Lacunarity: Number;
    Gain: Number;
    Offset: Number;
}

type DomainWarpNoise2D
    implements Value, ScalarField2D
{
    Basis: NoiseBasis;
    Seed: Integer;
    Frequency: Number;
    WarpStrength: Number;
    WarpFrequency: Number;
    Iterations: Integer;
}
```

From `26-fields.plato`:

```plato
concept ScalarField2D
    inherits Field<Point2D, Number>
{ }

concept DifferentiableScalarField2D
    inherits ScalarField2D
{
    GradientAt(x: Self, point: Point2D): Vector2D;
}

// Expression graphs combine scalar fields (Source, Constant, Add, Multiply, Remap, ...).
type ScalarFieldGraph2D
    implements Value
{
    Nodes: Array<ScalarFieldNode2D>;
    Root: FieldNodeIndex;
}
```

Usage-shaped sketches:

```plato
terrain = FbmNoise2D {
    Basis: Perlin;
    Seed: 1337;
    Frequency: 0.002;   // continent-scale wiggles in world units
    Octaves: 6;
    Lacunarity: 2.0;
    Gain: 0.5;
};

height = terrain.Eval(Point2D { X: 1200.0; Y: -400.0 });

mountains = RidgedNoise2D {
    Basis: Simplex;
    Seed: 1337;
    Frequency: 0.004;
    Octaves: 5;
    Lacunarity: 2.0;
    Gain: 0.5;
    Offset: 1.0;        // typical ridged offset
};

clouds = TurbulenceNoise2D {
    Basis: Perlin;
    Seed: 99;
    Frequency: 0.01;
    Octaves: 4;
    Lacunarity: 2.0;
    Gain: 0.5;
};

coasts = DomainWarpNoise2D {
    Basis: Perlin;
    Seed: 3;
    Frequency: 0.003;
    WarpStrength: 40.0;
    WarpFrequency: 0.01;
    Iterations: 2;
};

// Remap fBM from roughly [-1,1] into elevation metres via a field graph:
// (illustrative structure — wire Source indices to an external field list)
```

`FbmNoise2D` **is** a `ScalarField2D`, so it plugs into anything expecting a
planar scalar field — including, once library bodies exist, sampling onto
grids or feeding `ScalarFieldGraph2D` as a `Source`.

**Gap:** fBM types do not implement `DifferentiableScalarField2D` in the
declarations, even though terrain shading wants $\nabla h$ for slope. You
can finite-difference `Eval`, but the vocabulary does not promise
`GradientAt`.

`NoiseBasis.White` inside fBM is almost never what you want (uncorrelated
octaves → sparkly mess). The sum type still allows it.

## Pitfalls / fine print

- **Too many octaves.** Past the Nyquist limit of your sampling grid, extra
  octaves alias into sparkle. Match octaves to pixel footprint.
- **Gain ≥ 1.** Amplitudes explode; the “fine detail is quieter” story breaks.
- **Lacunarity ≤ 1.** Layers do not refine; you stack redundant low frequencies.
- **Ignoring range.** Raw fBM is not “metres.” Bias and scale explicitly
  (graph `Remap` / multiply by a constant field).
- **Ridged `Offset`.** Doc comment says typically $1$; leaving it at $0$
  collapses the ridge transform. It is not optional in spirit.
- **Seed vs basis.** Changing `Basis` from `Perlin` to `Value` keeps knobs
  identical but changes the mountain character entirely — same recipe,
  different flour.

## Try it

1. With $G=0.5$, $O=4$, what fraction of the infinite-octave amplitude sum
   do you already have?
2. You want sharper peaks on the same continent outline. Which type is the
   first reach — more octaves on `FbmNoise2D`, or `RidgedNoise2D`?
3. Why does adding octaves while rendering a $128\times 128$ height sample
   eventually look noisier instead of more detailed?

<details>
<summary>Answers</summary>

1. Partial sum $= 1 + 0.5 + 0.25 + 0.125 = 1.875$; ceiling $= 2$; so
   $1.875/2 = 93.75\%$.
2. `RidgedNoise2D` — it changes the ridge/valley asymmetry. Extra fBM octaves
   only add symmetric fine wrinkle.
3. Once an octave’s wavelength approaches the sample spacing, you cannot
   represent it; it aliases. Cap octaves using grid resolution / world
   frequency.

</details>

## Library recommendations

- **missing-concept** — `28-noise.plato`: `FbmNoise2D/3D` (and ridged /
  turbulence) should implement `DifferentiableScalarField2D/3D` or document
  that gradients are finite-difference only. Terrain lessons always need
  slope; `GradientAt` is the natural API already declared in `26-fields.plato`.
- **doc-comment** — `28-noise.plato`: `FbmNoise2D` should recommend default
  starting knobs (`Lacunarity: 2`, `Gain: 0.5`, `Octaves: 4..8`) and warn that
  `Basis: White` is legal but unsuitable. Teaching time is wasted rediscovering
  defaults.
- **missing-function** — `28-noise.plato`: no
  `AmplitudeBound(gain: Number, octaves: Integer): Number` helper for the
  geometric sum. Every author renormalizes fBM by hand to map into metres.
- **wrong-shape** — `28-noise.plato`: `DomainWarpNoise2D` omits `Octaves` /
  `Lacunarity` / `Gain` — it warps a single basis layer. Terrain coastlines
  usually want warped *fBM*. Either document composing warp ◦ fBM externally
  or add an `FbmDomainWarpNoise2D` record.
- **pedagogy** — `26-fields.plato`: `ScalarFieldGraph2D` is ideal for
  “fBM × scale + bias” elevation graphs, but wiring `Source(FieldIndex)` to
  noise values is only explained indirectly. A doc example naming noise as a
  typical external source would connect the two files the way this lesson must.
