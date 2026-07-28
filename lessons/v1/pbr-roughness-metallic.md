---
lesson: pbr-roughness-metallic
title: Metallic-Roughness PBR Materials
domain: Rendering & materials
v3-files: [50-materials.plato]
audience: Familiar with albedo and specular highlights; new to the metallic-roughness parameterization.
status: draft-v1
---

# Metallic-Roughness PBR Materials

Physically based rendering needs a compact way to say how a surface reflects light.
The industry default (and Plato's core `Material`) is the **metallic-roughness** model
aligned with glTF 2.0: a base color, a metalness blend, and a roughness that blurs the
specular lobe. Optional layers (clear coat, transmission, sheen, …) stack on top without
replacing that core.

This lesson stays on the two scalars that confuse newcomers most — `Metallic` and
`Roughness` — and how they interact with `BaseColor`.

## What the two knobs mean

Light reflection splits into **diffuse** (scattered into the hemisphere) and **specular**
(mirror-like lobe around the reflection direction). Metals and dielectrics behave
differently:

| | Dielectric (plastic, wood, skin) | Metal (iron, gold, copper) |
|---|----------------------------------|----------------------------|
| Diffuse | Colored by albedo | Essentially none |
| Specular | Weak, usually achromatic (~4% F0) | Strong, tinted by metal reflectance |
| `Metallic` | $0$ | $1$ |

**Roughness** controls microfacet spread:

| Roughness | Appearance |
|-----------|------------|
| Near $0$ | Sharp mirror / glossy |
| Mid | Soft highlights, blurred reflections |
| Near $1$ | Matte; specular energy spread wide |

```
  Roughness ≈ 0          Roughness ≈ 1
  light → \                 light → \
           \ mirror              ····· soft scatter
            \                       ·····
```

`Metallic` and `Roughness` are independent axes. Brushed metal is metallic with high
roughness; polished ceramic is dielectric with low roughness.

### BaseColor dual role

In the MR workflow, `BaseColor` means two different things depending on metalness:

- **Dielectric (`Metallic = 0`):** albedo — the diffuse color.
- **Metal (`Metallic = 1`):** specular reflectance color (gold is yellowish here, not in a
  separate specular map).

Intermediate metalness (0.2–0.8) is mostly for authored blends or dirty metals; real
materials tend to hug $0$ or $1$.

## In Plato

```plato
type Material
    implements Value
{
    BaseColor: Color;
    Metallic: Proportion;
    Roughness: Proportion;
    EmissiveColor: Color;
    EmissiveStrength: Number;
    NormalScale: Number;
    OcclusionStrength: Proportion;
    Alpha: AlphaSettings;
    DoubleSided: Boolean;
}
```

Colors are **linear-light**. `Metallic` and `Roughness` are `Proportion` values in
$[0,1]$. The reflectance model enum names the GGX MR lobe among others:

```plato
type Brdf = Lambert | OrenNayar | GgxMetallicRoughness | Disney | Phong | BlinnPhong;
```

`Material` itself assumes the metallic-roughness core; `Brdf` lets a renderer select an
evaluator when multiple models are supported.

### Textures — packed MR map

```plato
type MaterialTextureSet
    implements Value
{
    BaseColor: TextureBinding;
    MetallicRoughness: TextureBinding;
    Normal: TextureBinding;
    Occlusion: TextureBinding;
    Emissive: TextureBinding;
}

type TexturedMaterial
    implements Value
{
    Base: Material;
    Textures: MaterialTextureSet;
}
```

Per glTF convention (documented on the type): the metallic-roughness texture stores
**roughness in G** and **metallic in B**. Occlusion commonly lives in R of an occlusion
map. Unbound slots mean "use the scalar factors on `Material` alone."

```
var steel = Material(
    BaseColor: Color(0.56, 0.57, 0.58, 1.0),
    Metallic: 1.0,
    Roughness: 0.35,
    EmissiveColor: Color(0, 0, 0, 1),
    EmissiveStrength: 0.0,
    NormalScale: 1.0,
    OcclusionStrength: 1.0,
    Alpha: AlphaSettings(AlphaMode.Opaque, 1.0),
    DoubleSided: false);

var rubber = Material(
    BaseColor: Color(0.02, 0.02, 0.02, 1.0),
    Metallic: 0.0,
    Roughness: 0.9,
    ...);
```

### Alpha and sidedness

```plato
type AlphaMode
    = Opaque
    | Mask(Cutoff: Proportion)
    | Blend;

type AlphaSettings
    implements Value
{
    Mode: AlphaMode;
    Opacity: Proportion;
}
```

`Opacity` multiplies base-color alpha. `Mask` discards below cutoff (cutouts); `Blend`
composites. `DoubleSided` disables back-face culling for thin surfaces (leaves, paper).

### Layers beyond the core

When the base MR pair is not enough:

```plato
type LayeredMaterial
    implements Value
{
    Base: TexturedMaterial;
    ClearCoat: ClearCoatLayer;
    Sheen: SheenLayer;
    Specular: SpecularLayer;
    Transmission: TransmissionLayer;
    Iridescence: IridescenceLayer;
    Anisotropy: AnisotropyLayer;
    Subsurface: SubsurfaceParameters;
}
```

Each layer documents a disable convention (zero intensity, black color, …). A plain
surface is `LayeredMaterial` with every optional layer disabled — or simply `Material` /
`TexturedMaterial` without the wrapper.

Legacy import path:

```plato
type SpecularGlossinessMaterial
    implements Value
{
    Diffuse: Color;
    Specular: Color;
    Glossiness: Proportion;
    Alpha: AlphaSettings;
    DoubleSided: Boolean;
}
```

Prefer metallic-roughness for new content; glossiness is roughly "inverted roughness"
but the diffuse/specular split does not match MR's metalness story one-to-one — convert
carefully when ingesting old assets.

### Worked comparison

| Material | BaseColor | Metallic | Roughness |
|----------|-----------|----------|-----------|
| Glossy red plastic | saturated red | 0 | 0.15 |
| Matte red plastic | saturated red | 0 | 0.85 |
| Mirror chrome | pale gray | 1 | 0.05 |
| Brushed steel | mid gray | 1 | 0.45 |
| Gold nugget | yellow-orange | 1 | 0.3 |

Same red `BaseColor` with `Metallic = 1` is wrong for plastic — you would get red metal,
not red dielectric.

## Pitfalls and fine print

**sRGB albedo in linear fields.** Author colors in linear space as the file requires, or
convert on import. Double gamma makes plastics look chalky.

**Metalness in the middle.** Values around 0.5 rarely match real materials; they often
signal a bad conversion from specular-glossiness.

**Roughness vs glossiness.** High gloss = low roughness. Do not paste gloss maps into the
roughness channel without inverting.

**Packed texture swizzles.** Sampling metallic from the wrong channel of
`MetallicRoughness` silently swaps metal and rough — highlights look insane.

**Emissive is not lighting.** `EmissiveColor * EmissiveStrength` adds glow; it does not
replace `Metallic`/`Roughness` for reflected light.

## Try it

<details>
<summary>Exercise 1 — Identify the material</summary>

`Metallic = 0`, `Roughness = 0.1`, `BaseColor` bright blue. Plastic or metal? Glossy or
matte?

**Answer.** Dielectric (plastic-like), glossy.
</details>

<details>
<summary>Exercise 2 — Channel packing</summary>

In a glTF-style MR texture, which channel is roughness? Which is metallic?

**Answer.** Roughness in **G**, metallic in **B**.
</details>

<details>
<summary>Exercise 3 — Wrong metalness</summary>

An artist sets gold's yellow in `BaseColor` but leaves `Metallic = 0` and low roughness.
What goes wrong visually?

**Answer.** You get glossy yellow *plastic* (colored diffuse + white-ish specular), not
yellow-tinted metal specular. Set `Metallic = 1`.
</details>

## Library recommendations

- **doc-comment** — `Material`: spell out BaseColor's dielectric-vs-metal dual role on
  the type itself. The file banner mentions glTF MR; the field docs should repeat the
  dual meaning where authors look first.

- **missing-function** — no declared `Lerp`/`Mix` guidance for `Material` (metalness
  blends are nonlinear in appearance). A documented `BlendMaterials` or a warning comment
  would reduce naive component lerps in LOD transitions.

- **pedagogy** — `SpecularGlossinessMaterial` says "prefer Material" but does not point at
  a conversion sketch (gloss → rough = $1 - g$, metalness heuristics). A short conversion
  note would help importers.

- **missing-type** — `Brdf` is declared but `Material` does not carry a `Brdf` field; the
  link is implicit. Either add an optional model selector or document that
  `GgxMetallicRoughness` is always assumed for `Material`.
