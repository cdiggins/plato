---
lesson: texture-filtering-modes
title: Texture Filtering Modes
domain: Color & imaging
v3-files: [11-points.plato, 14-color.plato, 47-texturing.plato]
audience: Basic 3D graphics familiarity and general programming background
status: draft-v1
---

# Texture Filtering Modes

A texture is a grid of texels. A shader asks for a color at a continuous UV coordinate
that almost never lands on a texel center. **Filtering** decides how neighboring
texels blend — and, when the surface is far away, which **mip** level of a prefiltered
pyramid to read. Choose poorly and you get sparkling aliasing, blurry smears, or
both at once.

`Nearest` is honest and ugly. `Linear` is the everyday soft sample. Mip combinations
and anisotropy are how real-time renderers keep detail stable under minification.

## The idea

**Magnification** — UV footprint smaller than a texel (camera close). You stretch the
image. Options:

- **Nearest:** snap to the closest texel. Blocky, sharp, stable.
- **Linear:** bilinear blend of the $2\times2$ neighborhood. Smooth, slightly soft.

**Minification** — UV footprint larger than a texel (surface far or steeply angled).
Many texels map into one pixel. Sampling one texel aliases; you need a low-pass.

**Mipmaps** are precomputed downsized copies (levels $0,1,2,\ldots$), each roughly
half-resolution. Choosing a level matches filter width to pixel footprint:

```
 level 0  #### #### #### ####
 level 1  ##   ##   ##   ##
 level 2  #     #     #
```

**Mip filters** combine in-level filtering with across-level selection:

| Mode | Within level | Across mips |
|------|--------------|-------------|
| NearestMipNearest | nearest | pick one mip |
| LinearMipNearest | bilinear | pick one mip |
| NearestMipLinear | nearest | blend two mips |
| LinearMipLinear | bilinear | blend two mips (trilinear) |

**Anisotropic** filtering acknowledges that a pixel's footprint in UV space is often
an elongated ellipse (floors viewed at a grazing angle), not a circle. Extra samples
along the long axis preserve sharpness along edges that trilinear alone blurs.

**Wrap modes** are separate from filtering: they answer what happens when $u$ or $v$
leaves $[0,1]$ — repeat, clamp to edge, mirror, or clamp to a border color.

## In Plato

`47-texturing.plato` splits wrap, filter, and the full sampler state.

```plato
type WrapMode
    = Repeat
    | ClampToEdge
    | MirroredRepeat
    | ClampToBorder
    | MirrorClampToEdge;

type FilterMode
    = Nearest
    | Linear
    | NearestMipNearest
    | LinearMipNearest
    | NearestMipLinear
    | LinearMipLinear
    | Anisotropic;

type TextureSampler
    implements Value
{
    WrapU: WrapMode;
    WrapV: WrapMode;
    WrapW: WrapMode;
    Filter: FilterMode;
    Anisotropy: Number;     // max ratio; 1 disables
    MipBias: Number;
    BorderColor: Color;
}
```

Doc note: `LinearMipLinear` is trilinear. `Anisotropy` on the sampler is the maximum
anisotropy ratio; `1` disables.

A 2D texture descriptor carries size, format, and mip count (`0` requests a full
chain down to $1\times1$). Pixel data lives in image types, not in the descriptor.

```plato
type Texture2D
{
    Name: String;
    Size: IntegerSize2D;
    Format: PixelFormat;
    MipLevels: Integer;
}
```

Bindings wire descriptor + sampler + UV transform for materials:

```plato
type UvTransform
{
    Offset: Vector2D;
    Rotation: Angle;
    Tiling: Number2;
}

type TextureBinding
{
    Texture: Texture2D;
    Sampler: TextureSampler;
    Transform: UvTransform;
    UvSet: Integer;
    Channels: TextureChannelMask;
}
```

UV coordinates themselves are `UvCoordinate` from `11-points.plato`:

```plato
type UvCoordinate
{
    U: Number;
    V: Number;
}
```

Usage-shaped sampler presets (illustrative):

```plato
// Crisp UI / pixel art
let nearest = TextureSampler(
    ClampToEdge, ClampToEdge, ClampToEdge,
    Nearest, 1, 0, border);

// Standard material, trilinear
let tri = TextureSampler(
    Repeat, Repeat, Repeat,
    LinearMipLinear, 1, 0, border);

// Ground plane: anisotropic, mild mip bias toward sharpness
let ground = TextureSampler(
    Repeat, Repeat, ClampToEdge,
    Anisotropic, 8, -0.5, border);
```

Procedural textures (`ProceduralTexture`, `CheckerTexture`, …) evaluate `ColorAt` at
a UV continuously — they still need the same wrap/filter story when you rasterize
them into a `Texture2D`.

## Pitfalls / fine print

**FilterMode.Anisotropic vs Anisotropy field.** Selecting `Anisotropic` without
`Anisotropy > 1` is a no-op on many APIs. Conversely, some drivers ignore anisotropy
unless the min filter is a mip linear mode. Keep mode and ratio consistent.

**MipBias direction.** Positive bias tends to pick blurrier (higher) mips; negative
bias sharpens and can reintroduce aliasing. Doc comment says "mip level-of-detail
bias" without signing the convention — match your backend.

**ClampToBorder without caring about BorderColor.** If wrap is not `ClampToBorder`,
`BorderColor` is unused. If it is, transparent black vs opaque black changes silhouettes.

**Seams with Repeat + linear.** Bilinear at the $0/1$ boundary blends first and last
texels — desired for tiles, disastrous for atlas subrects (use clamp or pad gutters).

**MipLevels = 0.** Means "build full chain," not "zero mips." Off-by-one in exporters
is common.

**Nearest is not "no filter" for minification.** Without mips, nearest still aliases
under minification; it only avoids blending. Pair pixel-art styles with constrained
camera scales or explicit integer scales.

## Try it

1. Which `FilterMode` is classic trilinear filtering?
2. UV just outside $[0,1]$ with `ClampToEdge` vs `Repeat`: what texel neighborhood
   do you read?
3. Why does a floor texture often want anisotropy more than a wall billboard?

<details>
<summary>Answers</summary>

1. `LinearMipLinear`.
2. Clamp: UVs snap to the edge texel strip; blends stay within the border row/column.
   Repeat: UVs wrap modulo 1; blends can cross the wrap seam.
3. Grazing view stretches the pixel footprint into a long UV ellipse; anisotropic
   filtering samples along that ellipse. A facing billboard has a rounder footprint,
   so trilinear often suffices.

</details>

## Library recommendations

- **doc-comment** — `47-texturing.plato`: `FilterMode.Anisotropic` should state required
  interaction with `TextureSampler.Anisotropy` (minimum ratio, and whether underlying
  min/mag is implied to be linear-mip-linear). The lesson cannot specify a portable
  contract from the declarations alone.

- **naming** — `47-texturing.plato`: `Nearest` / `Linear` omit "mip" and double as mag
  filters, while other cases encode both. Consider documenting that bare `Nearest`/
  `Linear` mean "no mip selection" (level 0 only) vs "undefined mip," which backends
  treat differently.

- **missing-function** — `47-texturing.plato`: no `Sample(binding, uv: UvCoordinate): Color`
  on `TextureBinding` / `ProceduralTexture` parallelism beyond `ColorAt`. Teaching
  filtering needs a single sampling entry point that *uses* `TextureSampler`.

- **wrong-shape** — `47-texturing.plato`: consider splitting mag filter, min filter, and
  mip mode (as GPU APIs do) instead of one flat `FilterMode` sum — anisotropy and
  mip bias sit awkwardly beside a single enum that already tried to encode combinations.
