---
lesson: images-as-functions
title: Images as Functions
domain: Color & imaging
v3-files: [45-images.plato, 46-image-processing.plato]
audience: Comfortable with 2D arrays and the idea of sampling a continuous signal.
status: draft-v1
---

# Images as Functions

An image looks like a grid of pixels. Computationally it is often better to treat it as
a **function of position**: given a continuous coordinate, return a color (or a scalar,
or a normal). Discrete storage is then a *sampled* representation of that function, and
resizing, warping, and filtering become reconstruction problems — not just loops over
integer indices.

That shift explains why blur, resize, and blend APIs talk about filters and modes instead
of "the byte at $(i,j)$."

## The idea

Write an ideal image as

$$
I: \Omega \subset \mathbb{R}^2 \to C
$$

where $C$ might be linear RGBA, a scalar mask, or a direction. A **bitmap** stores
samples $I(x_i, y_j)$ on a lattice. Asking for $I(3.4, 7.1)$ requires a
**reconstruction filter**: nearest, bilinear, bicubic, Lanczos, …

```
continuous I(x,y)          stored samples
    ████████                  +--+--+--+
    ████████                  |  |  |  |
    ████████                  +--+--+--+
         ↑ sample & store           ↑ reconstruct
```

### Why the function view matters

- **Resize** = re-evaluate $I$ on a new lattice with a chosen filter.
- **Warp** = $I(T^{-1}(p))$ for a spatial transform $T$.
- **Convolve** = integrate $I$ against a kernel — still a function operation.
- **Compositing** = algebra on colors at the same continuous position, then store.

Pixels remain the currency of memory. Functions are the currency of *meaning*.

### Storage vs working form

Display-referred 8-bit sRGB grids (`Bitmap`) are for interop and packing. Filtering and
compositing want linear-light floats (`FloatImage`). Confusing the two reintroduces the
gamma bug inside every blur.

## In Plato

### The `Image` interface (`45-images.plato`)

```
interface Image
{
    Size(x: Self): IntegerSize2D;
    Width(x: Self): Integer;
    Height(x: Self): Integer;
}
```

Deliberately **no** pixel accessor on the interface — concrete types disagree on element
type (`Color8`, `Color`, `Boolean`, palette indices, …). Dimensions are the shared face.

### Concrete rasters

```
type Bitmap       { Size: IntegerSize2D; Pixels: Array<Color8>; }     // sRGB-encoded
type FloatImage   { Size: IntegerSize2D; Pixels: Array<Color>; }      // linear working
type GrayscaleImage { Size: IntegerSize2D; Values: Array<Number>; }
type BinaryImage  { Size: IntegerSize2D; Values: Array<Boolean>; }
type IndexedImage { Size: IntegerSize2D; Indices: Array<PaletteIndex>; Palette: Palette; }
type DepthImage   { Size: IntegerSize2D; Depths: Array<Number>; Near, Far: Number; }
type NormalImage  { Size: IntegerSize2D; Normals: Array<Vector3D>; }
```

Convention: row-major, top-left origin unless noted; `Pixels.Count = Width * Height`.
`ImageOrigin = TopLeft | BottomLeft` names the codec vs GL tension.

Usage-shaped indexing (illustrative; element access is on concrete types):

```
bmp: Bitmap = …
ij = row * bmp.Size.Width + col
c8 = bmp.Pixels[ij]
```

Higher-order containers: `MipChain`, `TiledImage`, `AnimatedImage`, `VolumeImage`,
`CubemapImage`, plus `EncodedImage` for compressed file bytes tagged by `ImageCodec`.

### Processing as data (`46-image-processing.plato`)

Operations are **parameter records** consumed by future processing functions — pure
descriptions of what to do.

Convolution and blur:

```
type ConvolutionKernel { Width, Height: Integer; Weights: Array<Number>; Scale, Bias: Number; }
type SeparableKernel { HorizontalTaps, VerticalTaps: Array<Number>; }
type GaussianBlurParameters { Radius: Integer; Sigma: Number; }
```

Resampling — the heart of "image as function":

```
type ResampleFilter = Nearest | Bilinear | Bicubic | Lanczos2 | Lanczos3 | Mitchell | CatmullRom | Box;

type ResizeParameters
{
    TargetSize: IntegerSize2D;
    Filter: ResampleFilter;
    PreserveAspect: Boolean;
}
```

Compositing algebra:

```
type BlendMode = Normal | Multiply | Screen | Overlay | … | Luminosity | Add | Subtract | Divide;
type PorterDuff = Clear | SourceOver | DestinationOver | … | Xor | Plus;
```

`BlendMode` answers "how do the *colors* combine?"; `PorterDuff` answers "how do the
*coverages* combine?" — orthogonal axes often collapsed into one enum in ad-hoc APIs.

Tonal tools (`LevelsAdjustment`, `CurvesAdjustment`, `HueSaturationAdjustment`,
`ExposureAdjustment`, `ColorLookupTable`, …) and structure tools (`EdgeDetection`,
`MorphologyParameters`, `Threshold`) round out the toolbox. Analysis uses
`ImageHistogram` over a `ColorChannel`.

## Pitfalls / fine print

**No `Eval` on `Image` yet.** The pedagogical slogan "image as function" is not reflected
as `Procedural<UvCoordinate, Color>` on the interface. Sampling at continuous UVs is a
library gap — resize filters imply it, but nothing is declared.

**Bitmap vs FloatImage.** Blurring a `Bitmap` in gamma space darkens halos. Convert to
`FloatImage` (linear `Color`) first.

**Half-open regions.** `ImageRegion` uses `IntegerBounds2D` that contain `Min` and
exclude `Max` — classic half-open discipline; off-by-one bugs love the inclusive max.

**Normal maps.** `NormalImage` stores unit directions in $[-1,1]$, not remapped colors.
Do not save them as `Bitmap` without an encoding convention.

**Separable ≠ arbitrary.** `SeparableKernel` is cheaper but cannot express every 2D
kernel (motion blur at 30° needs a dense kernel or a rotated separable pass).

**Premultiplied alpha.** Blend math depends on whether RGB is premultiplied by A. v3
blend enums do not encode premultiplication state — document pipeline assumptions
explicitly.

## Try it

<details>
<summary>Exercise 1 — Which storage?</summary>

You are compositing transparent HDR layers for a renderer. `Bitmap` or `FloatImage`?

**Answer.** `FloatImage` — linear unbounded `Color` samples.
</details>

<details>
<summary>Exercise 2 — Filter choice</summary>

Magnifying pixel art 4× without blur — which `ResampleFilter`?

**Answer.** `Nearest` — preserves hard texel edges.
</details>

<details>
<summary>Exercise 3 — Blend vs Porter-Duff</summary>

You need "draw source only where destination already has coverage." Is that primarily a
`BlendMode` or a `PorterDuff` choice?

**Answer.** `PorterDuff` — e.g. `SourceIn`. Blend modes assume coverage policy separately.
</details>

## Library recommendations

- **missing-function** — `45-images.plato`: no
  `Sample(image, uv: UvCoordinate, filter: ResampleFilter): Color` (or per-concrete-type
  overloads). The function view is unteachable as an API until sampling exists; resize
  parameters hint at it but do not provide it.

- **missing-interface** — `45-images.plato`: consider
  `interface SampledImage inherits Image, Procedural<UvCoordinate, Color>` for linear
  working images, so `Eval`/`Sample` is discoverable the same way easings expose `Eval`.

- **doc-comment** — `45-images.plato`: `Bitmap` says "sRGB-encoded" and `FloatImage`
  says "linear-light"; add an explicit warning that convolution on `Bitmap` is almost
  always wrong. Processing docs in `46` assume linear inputs without saying so on each
  record.

- **missing-type** — `46-image-processing.plato`: compositing needs a
  `CompositeOp { Blend: BlendMode; Coverage: PorterDuff; }` (plus optional
  premultiplication flag). Orthogonal enums without a pairing type invite incomplete
  parameters in every call site.
