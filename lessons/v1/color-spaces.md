---
lesson: color-spaces
title: Color Spaces
domain: Color & imaging
v3-files: [14-color.plato, 44-color-spaces.plato]
audience: General programming background; no color-science degree required.
status: draft-v1
---

# Color Spaces

"Pick a nice blue, then blend halfway to yellow." In RGB that midpoint is a muddy grayish
swamp — not the vibrant green your eye expects on a hue wheel. RGB is an excellent
*encoding* for lights and displays. It is a poor *thinking* space for choosing, harmonizing,
and interpolating colors. **Color spaces** are alternative coordinates for the same physical
stimulus, each making some job easy and another hard.

## The idea

A color is not "three numbers" in the abstract. It is a point that can be written in many
bases:

| Space | Good for | Bad for |
|-------|----------|---------|
| Linear RGB (`Color`) | lighting math, compositing | picking, perceptual blends |
| sRGB encoded | storage, PNG/CSS interchange | arithmetic (gamma!) |
| HSV / HSL | UI color pickers | perceptual uniformity |
| CIELAB / OkLab | perceptual difference, blends | direct display output |
| CMYK | ink printing | additive light |

### Hue is an angle

HSV and HSL reparameterize RGB so that **hue** walks a circle, **saturation** is purity,
and **value** or **lightness** is a brightness-like axis. Hue as an angle matches how
people talk ("shift 30° toward green") and how harmony rules work (complementary = +180°).

```
        green
          |
   yellow | cyan
          |
  red ----+---- blue
          |
   magenta| 
          |
        (hue wheel)
```

HSL's lightness puts pure white and pure black at the poles; HSV's value puts pure white
only at full saturation collapse — different UI conventions, same hue circle.

### Device-independent hubs

Displays disagree about what "red" means. **CIE XYZ** is the standard tristimulus hub:
convert space A → XYZ → space B. **Chromaticity** $(x,y)$ strips luminance so you can
talk about primaries and white points on the famous horseshoe diagram. **CIELAB** and
**OkLab** attempt perceptual uniformity: equal Euclidean steps ≈ equal perceived
difference. OkLab is generally preferred today for gradients and blending because its
hue is more linear.

### Linear vs encoded

`Color` in Plato is **linear-light** RGBA. Most 8-bit images and CSS hex colors are
**sRGB-encoded**. Adding two sRGB bytes as if they were linear light darkens midtones —
the single most common color bug. Decode to linear for math; encode for display or
storage.

## In Plato

### Core types (`14-color.plato`)

```
type Color     { R, G, B, A: Number; }          // linear, implements Numerical
type Color8    { R, G, B, A: Integer; }         // 0..255 storage, typically sRGB
type ColorHSV  { Hue: Angle; Saturation, Value: Number; }
type ColorHSL  { Hue: Angle; Saturation, Lightness: Number; }
type ColorStop { Position: Number; Color: Color; }
type ColorGradient { Stops: Array<ColorStop>; }
```

Because `Color` implements `Numerical`, it supports `Lerp` component-wise — correct in
linear light, wrong if you meant a perceptual mid-hue.

```
mid = Lerp(Color(1, 0, 0, 1), Color(0, 1, 0, 1), 0.5)   // dull yellow-green in RGB
```

For picker-style edits, convert through HSV/HSL (conversion functions are a later library
pass; the *types* already name the endpoints):

```
hsv = ColorHSV(hue, sat, value)
hsl = ColorHSL(hue, sat, lightness)
```

### Science and named spaces (`44-color-spaces.plato`)

Chromaticity and white points:

```
type Chromaticity { X: Number; Y: Number; }
type WhitePoint { Chromaticity: Chromaticity; Luminance: Number; }
type StandardIlluminant = A | B | C | D50 | D55 | D60 | D65 | D75 | E | F2 | F7 | F11;
```

RGB space description:

```
type TransferFunction = Linear | SRGB | Gamma22 | … | PQ | HLG;
type RgbPrimaries { Red, Green, Blue, White: Chromaticity; }
type RgbColorSpace { Name: String; Primaries: RgbPrimaries; Transfer: TransferFunction; }
type NamedColorSpace = SRGB | LinearSRGB | DisplayP3 | AdobeRGB | Rec2020 | ACEScg | …;
```

Device-independent and cylindrical perceptual forms:

```
type ColorXYZ { X, Y, Z: Number; }       // Y = luminance
type ColorXyY { Chromaticity: Chromaticity; Luminance: Number; }
type ColorLab { L, A, B: Number; }
type ColorLCh { L, Chroma: Number; Hue: Angle; }
type ColorOkLab { L, A, B: Number; }
type ColorOkLCh { L, Chroma: Number; Hue: Angle; }
```

Encoded / process cousins: `ColorSRGB`, `ColorYUV`, `ColorYCbCr`, `ColorCMYK`,
`ColorHWB`, plus `SpectralColor` for measured spectra.

Harmony and difference:

```
type ColorHarmony = Complementary | Analogous | Triadic | … | Monochromatic;
type ColorDifference = CIE76 | CIE94 | CIEDE2000 | CMC | EuclideanOkLab;
type Palette { Colors: Array<Color>; }
```

Conversion *recipes* (not yet executable functions) are data:

```
type ColorSpaceConversion
{
    Source: NamedColorSpace;
    Target: NamedColorSpace;
    Adaptation: ChromaticAdaptation;
    Intent: RenderingIntent;
}
```

## Pitfalls / fine print

**Do not lerp HSV blindly either.** Hue is circular: lerping $10°$ toward $350°$ the long
way goes through green. Use shortest-arc angle interpolation on `Hue`.

**HSL lightness ≠ luminance.** HSL's $L$ is a rough UI knob, not CIE $Y$ or Lab $L^*$.
Accessibility contrast needs a luminance-aware metric.

**Wide gamut / HDR.** Linear `Color` components may exceed $[0,1]$. Clamping too early
destroys specular highlights and Display-P3 content.

**White point matters.** Lab/OkLab coordinates are relative to a reference white.
Mixing Lab numbers computed under D65 with D50 data without chromatic adaptation is a
silent error. `ChromaticAdaptation` (`Bradford`, `CAT16`, …) names the fix.

**Color8 is not linear.** Treat `Color8` as storage. Convert via an sRGB transfer before
arithmetic.

**Complementary in RGB ≠ complementary in hue.** RGB `(1,0,0)` and `(0,1,1)` are
channel complements; perceptual complements live on a hue wheel in a perceptual space.

## Try it

<details>
<summary>Exercise 1 — Which type for math?</summary>

You need to average two lit material samples for a blur. Which type?

**Answer.** `Color` (linear). Averaging `Color8` or `ColorSRGB` bytes is wrong.
</details>

<details>
<summary>Exercise 2 — Picker coordinates</summary>

A design tool exposes a hue wheel and a vertical "brightness" slider that goes to white
at the top even at full saturation. Is that HSV or HSL?

**Answer.** HSV — value at max is white/bright pure colors; HSL puts white only at
lightness 1 with saturation collapsing.
</details>

<details>
<summary>Exercise 3 — Perceptual midpoint</summary>

You want a gradient from deep blue to bright yellow that stays vivid in the middle.
Prefer `Lerp` in `Color` or in `ColorOkLab`?

**Answer.** `ColorOkLab` (or `ColorOkLCh` with careful hue arcs) — linear RGB lerp
desaturates through grayish midtones.
</details>

## Library recommendations

- **missing-function** — `14-color.plato` / `44-color-spaces.plato`: conversions are
  explicitly deferred, but teaching needs at least declared signatures such as
  `ToLinear(ColorSRGB): Color`, `ToSRGB(Color): ColorSRGB`, `ToOkLab(Color): ColorOkLab`,
  and `ToHSV(Color): ColorHSV`. Types without maps strand every consumer.

- **missing-function** — `44-color-spaces.plato`: `ColorDifference` names formulas but
  there is no `DeltaE(a, b, formula)` declaration. Palette tooling and tests need it.

- **wrong-shape** — hue types split across files: `ColorHSV`/`ColorHSL` in `14-color.plato`,
  `ColorLCh`/`ColorOkLCh`/`ColorHWB` in `44-color-spaces.plato`. Either document 14 as
  "UI companions to Color" or colocate all cylindrical models so discoverability matches
  the conceptual family.

- **doc-comment** — `14-color.plato`: `Color` should state up front "do not construct from
  sRGB hex/bytes without decoding" and point at `ColorSRGB` / `Color8`. The linear
  invariant is necessary but not sufficient pedagogy for the #1 misuse.
