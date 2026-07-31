---
lesson: linear-vs-gamma
title: Linear vs Gamma Color
domain: Color & imaging
v3-files: [14-color.plato, 44-color-spaces.plato]
audience: General programming background; some exposure to RGB hex colors helpful.
status: draft-v1
---

# Linear vs Gamma Color

You average pure black `(0,0,0)` and pure white `(1,1,1)` and expect middle gray. In a photo editor
the midpoint looks right. In a naive shader that averages the **encoded** channel values sitting in
an 8-bit PNG, the result is too dark — a muddy ~0.5 in storage space is not perceptually halfway,
and worse, it is not halfway in **light energy** either.

That bug has a name: doing math in **gamma-encoded** coordinates instead of **linear-light**
coordinates. It is the single most common color mistake in graphics code. Plato's type split exists
to make the mistake harder.

## The idea

### Light adds in linear space

Physical light energies add. If two lamps each contribute radiance $L$, together they contribute
$2L$. Doubling a linear RGB channel doubles photons (within the model's idealizations). Blending,
filtering, lighting integrals, and mipmap averages all want **linear** intensities.

### Displays and files use a transfer curve

Human vision and old CRTs are roughly nonlinear. sRGB storage does not keep linear voltages in the
bytes. A **transfer function** encodes linear light $L \in [0,1]$ into a stored code $c$, and
decodes back:

$$
c = EOTF^{-1}(L) \qquad L = EOTF(c)
$$

For sRGB the curve is piecewise (linear near zero, then a power-like segment). People often say
"gamma 2.2" as a rough description; real sRGB is the piecewise `TransferFunction.SRGB` curve, not
a pure power.

```
linear light L          encoded code c (what PNGs store)
0.0 ──────── 1.0        0.0 ──────── 1.0
     |                         /
     |                        /
     |            ≈          /
     |                      /
     +----------------------+
        "gamma encode" →
```

Halfway in code space ($c=0.5$) corresponds to much less than half the photons. Mid-gray *appearance*
is a third topic again (perceptual spaces like OkLab) — do not conflate "looks midway" with
"linear midway."

### The rule of thumb

1. **Decode** storage colors to linear light before arithmetic.
2. Do lighting, lerp, blur, and resize in linear light.
3. **Encode** for display or for writing ordinary 8-bit images.

Skip step 1 and dark seams appear in blends; mipmaps gray-shift; soft shadows look wrong.

## In Plato

### Canonical compute color is linear

From `14-color.plato`:

```plato
// A linear-light RGBA color. The canonical color type for computation;
// interpolation and arithmetic are component-wise.
type Color
    implements Numerical
{
    R: Number;
    G: Number;
    B: Number;
    A: Number;
}
```

`Color` implements `Numerical`, so `Lerp`, scalar multiply, and add are defined on linear channels.
That is intentional: the type is the "do math here" form.

```
a = Color(1, 0, 0, 1)          // linear red
b = Color(0, 0, 1, 1)          // linear blue
m = a.Lerp(b, 0.5)             // linear midpoint (purple energy mix)
```

### Storage and interchange are separate types

```plato
// An 8-bit-per-channel RGBA color with components in [0, 255]; storage and
// interop form, typically sRGB-encoded.
type Color8
    implements Value, Hashable
{
    R: Integer;
    G: Integer;
    B: Integer;
    A: Integer;
}
```

From `44-color-spaces.plato`:

```plato
// A gamma-encoded sRGB color with alpha, components nominally in [0, 1]: the
// interchange form of most 2D content. Decode to linear `Color` before
// arithmetic.
type ColorSRGB
    implements Value
{
    R: Number;
    G: Number;
    B: Number;
    A: Number;
}
```

Same three floats as `Color`, different meaning. The type name carries the encoding so APIs can
demand `Color` when they mean energy.

### Naming the curve and the space

```plato
type TransferFunction
    = Linear | SRGB | Gamma18 | Gamma22 | Gamma24 | Gamma26 | Rec709 | PQ | HLG;

type NamedColorSpace
    = SRGB
    | LinearSRGB
    | DisplayP3
    | AdobeRGB
    | ProPhotoRGB
    | Rec709
    | Rec2020
    | ACEScg
    | ACES2065;
```

`NamedColorSpace.SRGB` is the familiar encoded space; `LinearSRGB` is the same primaries with a
linear transfer — i.e. what you want after decoding sRGB for shading. HDR paths use `PQ` / `HLG`
instead of a simple gamma.

A full recipe:

```plato
type RgbColorSpace
{
    Name: String;
    Primaries: RgbPrimaries;
    Transfer: TransferFunction;
}
```

Primaries fix the gamut (which reds/greens/blues); transfer fixes encode/decode. Both matter when
converting through `ColorXYZ`.

### Authoring spaces vs light math

`ColorHSV` / `ColorHSL` (file 14) and perceptual types like `ColorOkLab` (file 44) are for picking
and perceptually uniform blends. They are not substitutes for linear RGB in lighting integrals.
A common correct pipeline:

```
bytes  --decode sRGB-->  Color (linear)
Color  --to OkLab-->     ColorOkLab     // optional perceptual lerp
ColorOkLab --back-->     Color
Color  --encode sRGB-->  ColorSRGB / Color8
```

## Pitfalls / fine print

**Averaging hex colors as integers.** `(#000000 + #FFFFFF) / 2` in byte space is not middle gray in
linear light. Decode, average `Color`, encode.

**Alpha and premultiplication.** Even in linear space, blending with transparency wants
premultiplied alpha. Encoding after premultiply, or mixing encoded colors with linear alpha, creates
halos.

**"Gamma 2.2" ≠ sRGB.** Close for rough work; wrong for exact round-trips. Prefer
`TransferFunction.SRGB` when the standard matters.

**Wide gamut and HDR.** `Color` components may exceed $[0,1]$ (doc comment on `Color`). Encoded
PQ/HLG values are not "just gamma." Do not run the sRGB EOTF on HDR buffers.

**UI theme colors.** Design tools often hand you sRGB. Convert once at the asset boundary into
`Color`, then keep linear inside the engine.

**Lerp in HSL is not lighting.** Hue lerp can be great for gradients and terrible for combining
lamp contributions. Match the space to the job.

## Try it

<details>
<summary>Exercise 1 — Which type?</summary>

You are writing a shader lighting term `albedo * irradiance`. Should `albedo` be `Color`,
`ColorSRGB`, or `Color8`?

**Answer.** `Color` (linear). Decode textures to linear before multiplying by irradiance.
</details>

<details>
<summary>Exercise 2 — Predict the bug</summary>

A thumbnailer averages 8-bit sRGB pixels with integer arithmetic, then writes an 8-bit PNG. Why do
downscaled images look too dark?

**Answer.** Averaging encoded values underweights bright contributions relative to linear light;
the stored midtones skew dark. Correct approach: decode → average in linear → encode.
</details>

<details>
<summary>Exercise 3 — Name the space</summary>

You need sRGB primaries but linear values for an offline path tracer. Which `NamedColorSpace` case
matches?

**Answer.** `LinearSRGB`.
</details>

## Library recommendations

- **missing-function** — `14-color.plato` / `44-color-spaces.plato`: no declared
  `ToLinear(ColorSRGB): Color`, `ToSRGB(Color): ColorSRGB`, or `Color8` ↔ `Color` conversions with
  an explicit `TransferFunction`. The doc comments order the pipeline; the vocabulary never names
  the functions the lesson must teach.

- **naming** — `14-color.plato`: `Color` is easy to misread as "any RGB." Renaming to `LinearColor`
  (keeping `Color` as a deprecated alias) or amplifying the doc banner to say "never store encoded
  sRGB in this type" would match how often this lesson's bug appears.

- **doc-comment** — `Color8`: "typically sRGB-encoded" is soft. State the default assumption
  (sRGB transfer + sRGB primaries unless tagged otherwise) or pair `Color8` with an explicit
  `RgbColorSpace` field — untagged bytes are how encode mistakes travel.

- **missing-type** — no `EncodedColor` wrapper tying `(ColorSRGB values × TransferFunction ×
  RgbPrimaries)`. `ColorSRGB` hard-codes sRGB; HDR and Display P3 encoded buffers need a parallel
  story or everyone invents one.

- **pedagogy** — `Color.Lerp` is component-wise on linear channels (good for light) but the type
  also tempts perceptual blends. A sibling `Lerp` on `ColorOkLab` called out in `Color`'s doc —
  "for perceptual midtones prefer ColorOkLab" — would steer authors without forbidding linear lerp.
