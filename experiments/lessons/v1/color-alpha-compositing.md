---
lesson: color-alpha-compositing
title: Color, Alpha, and Compositing
domain: Color & imaging
v3-files: [14-color.plato, 46-image-processing.plato]
audience: Basic graphics or UI experience; RGB familiarity
status: draft-v1
---

# Color, Alpha, and Compositing

Layer a translucent red circle over a blue background. What color is each
pixel? "Mix the RGB channels" is incomplete — you also need to know how much
of the background shows through, whether the red was stored premultiplied,
and whether "mix" means Normal, Multiply, or Screen. Alpha compositing is
the algebra of coverage; blend modes are the algebra of channel combination.
They are related but not the same knob.

## The idea

A pixel color in linear light is often written $(R, G, B, A)$ with $A \in
[0,1]$ describing **coverage** (or opacity): how much of the pixel the
foreground occupies.

**Straight (unassociated) alpha** stores color channels of the opaque source
and a separate coverage:

$$
C_{\mathrm{out}} = \alpha_s C_s + (1 - \alpha_s)\, C_d
$$

for the classic "source over destination" composite of opaque-looking colors
$C_s, C_d$ (ignoring destination alpha for a moment).

**Premultiplied (associated) alpha** stores $(\alpha R, \alpha G, \alpha B,
\alpha)$. Coverage is already baked into the channels. Compositing becomes
linear and filters (blur, mipmaps) behave correctly at edges — transparent
pixels contribute zero light instead of leaking arbitrary RGB.

Porter and Duff named the coverage operators: SourceOver, DestinationIn,
Xor, and friends. Each describes which parts of the source and destination
coverage footprints survive. Separately, **blend modes** (Multiply, Screen,
Overlay, …) describe how source and destination *colors* combine in the
overlap region before coverage is applied — the Photoshop/CSS vocabulary.

```
  Source ●████████░░░░     alpha = coverage of foreground
  Dest   ░░████████████
  Over   ●████▓▓▓▓████     overlap uses blend; outside uses one layer
```

Rough mental model:

1. **BlendMode** — how colors interact where both layers have coverage
2. **PorterDuff** — how the coverage masks themselves combine
3. **Color.A** — the per-pixel opacity participating in that math

## In Plato

Core working color is linear RGBA in `14-color.plato`:

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

`Color` implements `Numerical`, so component-wise `Add`, `Multiply` by a
scalar, and `Lerp` are available — useful building blocks, but not a full
compositor by themselves.

Storage/interop often uses 8-bit sRGB-ish values:

```plato
type Color8
    implements Value, Hashable
{
    R: Integer;
    G: Integer;
    B: Integer;
    A: Integer;
}
```

Compositing vocabulary sits in `46-image-processing.plato` as sum types —
parameter records for a later processing library:

```plato
// How a source color combines with a destination color when layered
// (Photoshop/CSS blend modes).
type BlendMode
    = Normal
    | Multiply
    | Screen
    | Overlay
    | Darken
    | Lighten
    | ColorDodge
    | ColorBurn
    | HardLight
    | SoftLight
    | Difference
    | Exclusion
    | Hue
    | Saturation
    | Color
    | Luminosity
    | Add
    | Subtract
    | Divide;

// A Porter-Duff alpha-compositing operator: how source and destination
// coverage combine.
type PorterDuff
    = Clear
    | Source
    | Destination
    | SourceOver
    | DestinationOver
    | SourceIn
    | DestinationIn
    | SourceOut
    | DestinationOut
    | SourceAtop
    | DestinationAtop
    | Xor
    | Plus;
```

Illustrative usage shape (functions not yet declared — the gap is the point):

```plato
let src = Color { R: 1.0, G: 0.0, B: 0.0, A: 0.5 };
let dst = Color { R: 0.0, G: 0.0, B: 1.0, A: 1.0 };

let mode = BlendMode.Normal;
let op = PorterDuff.SourceOver;

// Desired API shape — not declared in v3 declarations yet:
// let out = Composite(src, dst, mode, op);
```

Until compositing functions land, you can still express the *coverage* part
of SourceOver for opaque destinations with channel math on linear
`Color`:

```plato
// Straight-alpha SourceOver onto an opaque destination (dst.A == 1):
// out.RGB = src.A * src.RGB + (1 - src.A) * dst.RGB
let t = src.A;
let outR = t * src.R + (1.0 - t) * dst.R;
let outG = t * src.G + (1.0 - t) * dst.G;
let outB = t * src.B + (1.0 - t) * dst.B;
let out = Color { R: outR, G: outG, B: outB, A: 1.0 };
```

`Lerp` on `Color` is component-wise including alpha — fine for gradients,
dangerous if you treat it as SourceOver (it is not coverage math).

## Pitfalls / fine print

**Linear vs gamma.** `Color` is linear-light. Blending in sRGB-encoded
`Color8` space darkens midtones incorrectly. Convert to linear `Color`,
composite, convert back for storage.

**Premultiplied vs straight.** Filtering premultiplied data is correct;
filtering straight alpha fringes. Mixing the two conventions without
conversion produces black or colored halos.

**BlendMode.Normal is not a no-op with alpha.** Normal still respects
coverage. Multiply darkens; Screen lightens; Difference is for effects, not
UI overlays.

**Destination alpha.** Full Porter–Duff tracks both $\alpha_s$ and
$\alpha_d$. The "opaque backdrop" shortcut fails for stacking multiple
partially transparent layers — you need the full $\alpha_{\mathrm{out}}$
formula.

**HDR and A.** Wide-gamut / HDR values may have channel values outside
$[0,1]$ while $A$ remains a coverage in $[0,1]$. Clamping RGB for display
is separate from clamping alpha.

**Hue/Saturation/Color/Luminosity modes.** These operate in a different
decomposition than RGB arithmetic; implementing them as naive channel
formulas will not match Photoshop/CSS.

## Try it

1. Source $(1,0,0)$ with $A=0.5$ over opaque blue $(0,0,1)$. What is the
   straight-alpha SourceOver RGB result?
2. Same source stored premultiplied. What are the stored RGBA values before
   compositing?
3. Why is `Lerp(dst, src, src.A)` not always the same as SourceOver when
   both layers have alpha less than 1?

<details>
<summary>Answers</summary>

1. $(0.5,\; 0,\; 0.5)$ — half red, half blue.
2. $(0.5,\; 0,\; 0,\; 0.5)$ — RGB scaled by alpha.
3. `Lerp` blends channels independently with a single $t$; SourceOver with
   two alphas uses coverage algebra for both color and output alpha. They
   coincide only in special cases (e.g. opaque destination).

</details>

## Library recommendations

- **missing-function** — `46-image-processing.plato`: `BlendMode` and
  `PorterDuff` are declared as data, but no `Composite(src: Color, dst:
  Color, mode: BlendMode, op: PorterDuff): Color` (or Bitmap-level) function
  exists. The lesson can name the operators but cannot show a real call.

- **missing-type** — `14-color.plato`: no `PremultipliedColor` (or a tag on
  `Color`) distinguishing associated vs straight alpha. Without it,
  compositing APIs cannot make the dangerous conversion a typed boundary.

- **doc-comment** — `14-color.plato`: `Color.A` should state whether the
  canonical `Color` is straight or premultiplied. Computation types need a
  single documented convention; silence guarantees mismatched callers.

- **missing-function** — `14-color.plato`: `Lerp` on `Color` should be
  documented as component-wise numerical interpolation, explicitly *not*
  alpha compositing, to prevent the most common misuse when teaching layers.
