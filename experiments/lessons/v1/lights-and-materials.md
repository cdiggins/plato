---
lesson: lights-and-materials
title: Lights and Materials
domain: Rendering
v3-files: [49-lights.plato, 50-materials.plato]
audience: Basic 3D graphics familiarity; no prior PBR coursework assumed.
status: draft-v1
---

# Lights and Materials

Shading is a conversation between **what the light is doing** and **how the surface
responds**. A bright white spotlight on black velvet looks nothing like the same spotlight
on brushed steel. Physically based rendering (PBR) gives both sides a shared, energy-aware
vocabulary: lights carry measurable photometric quantities; materials carry albedo,
roughness, and metalness instead of a pile of ad-hoc RGB knobs.

## The idea

### Energy conservation in one picture

A surface cannot reflect more light than it receives (ignoring emission). As **roughness**
rises, the specular highlight spreads and its peak drops so the *integrated* reflected
energy stays plausible. As **metalness** rises, dielectric diffuse albedo gives way to
tinted specular reflectance — metals have almost no diffuse lobe.

```
roughness low          roughness high
    |* |                   | *** |
    |* |                   |*****|
    |* |                   | *** |
   sharp bright           wide dim peak
```

### Metallic-roughness parameters

The glTF-style **metallic-roughness** model (Plato’s core `Material`) uses:

| Parameter | Meaning |
|-----------|---------|
| BaseColor | Albedo for dielectrics; specular tint for metals (linear RGB) |
| Metallic | $0$ = dielectric, $1$ = metal |
| Roughness | $0$ = mirror, $1$ = fully diffuse microsurface |
| Emissive | Self-glow (plus a strength multiplier) |
| Occlusion | Ambient shadowing factor (usually textured) |

**Dielectric** (plastic, wood, concrete): soft diffuse color from `BaseColor`, specular
mostly white and Fresnel-driven. **Metal**: little diffuse, specular colored by
`BaseColor`.

### Light families and units

Different lights want different photometric units:

| Light | Natural unit | Intuition |
|-------|--------------|-----------|
| Directional / ambient | Illuminance (lux) | Arrival on a facing surface |
| Point / spot | Luminous flux (lumens) | Total emitted power |
| Area / disk / sphere / tube | Luminance (nits) | Glow of an extended surface |
| Environment / sky | Dimensionless intensity | Scale on stored/computed radiance |

Using one RGB "intensity" for all of these is how scenes become impossible to relight
when you change exposure.

## In Plato

### Analytic lights (`49-lights.plato`)

```
concept LightSource
{
    CastsShadows(x: Self): Boolean;
}

type Attenuation = None | Linear | Quadratic | InverseSquare | Smooth;
```

Directional (sun):

```
type DirectionalLight
{
    Direction: Direction3D;       // travel direction, toward the scene
    Color: Color;
    Illuminance: Illuminance;
    AngularDiameter: Angle;       // soft-shadow penumbra; sun ≈ 0.53°
    CastsShadows: Boolean;
}
```

Point and spot (flux in the cone for spots):

```
type PointLight
{
    Position: Point3D;
    Color: Color;
    Flux: LuminousFlux;
    Range: Number;                // 0 = unlimited
    Falloff: Attenuation;
    CastsShadows: Boolean;
}

type SpotLight
{
    Position: Point3D;
    Direction: Direction3D;
    Color: Color;
    Flux: LuminousFlux;
    InnerAngle: Angle;            // half-angles; Inner ≤ Outer
    OuterAngle: Angle;
    Range: Number;
    Falloff: Attenuation;
    CastsShadows: Boolean;
}
```

Shaped emitters carry `Luminance`: `AreaLight`, `DiskLight`, `SphereLight`, `TubeLight`.

Environment and fill:

```
type EnvironmentLight { Cube: TextureCube; Rotation: Angle; Intensity: Number; Tint: Color; }
type ProceduralSky { SunDirection: Direction3D; Turbidity: Number; GroundAlbedo: Color; Intensity: Number; }
type AmbientLight { Color: Color; Illuminance: Illuminance; }
type LightProbe { Position: Point3D; InfluenceRadius: Number; Intensity: Number; }
```

Shadows and extras: `ShadowSettings`, `LightLayerMask`, `IesProfileReference`,
`VolumetricLightSettings`.

### Core material (`50-materials.plato`)

```
type AlphaMode = Opaque | Mask(Cutoff: Proportion) | Blend;

type Material
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

Textures ride beside scalars:

```
type MaterialTextureSet
{
    BaseColor: TextureBinding;
    MetallicRoughness: TextureBinding;   // glTF: roughness=G, metallic=B
    Normal: TextureBinding;
    Occlusion: TextureBinding;
    Emissive: TextureBinding;
}

type TexturedMaterial { Base: Material; Textures: MaterialTextureSet; }
```

Usage-shaped plastic vs metal:

```
plastic = Material(
    BaseColor: Color(0.8, 0.1, 0.1, 1),
    Metallic: Proportion(Value: 0),
    Roughness: Proportion(Value: 0.4),
    …)

steel = Material(
    BaseColor: Color(0.9, 0.9, 0.95, 1),
    Metallic: Proportion(Value: 1),
    Roughness: Proportion(Value: 0.25),
    …)
```

### Layers and alternatives

`LayeredMaterial` stacks optional glTF-shaped layers: `ClearCoatLayer`, `SheenLayer`,
`SpecularLayer`, `TransmissionLayer`, `IridescenceLayer`, `AnisotropyLayer`,
`SubsurfaceParameters`. Each layer documents a disable convention (zero strength or black
color).

Also available: `SpecularGlossinessMaterial` (legacy import), `UnlitMaterial`,
`ToonMaterial`, `MatcapMaterial`, `WireframeMaterial`. The reflectance model enum

```
type Brdf = Lambert | OrenNayar | GgxMetallicRoughness | Disney | Phong | BlinnPhong;
```

names what a renderer might evaluate — but core `Material` does not carry a `Brdf`
field; metallic-roughness + GGX is the implied default for `Material`.

## Pitfalls / fine print

**Linear color only.** `BaseColor` and light `Color` tints are linear-light. Authoring in
sRGB hex without decoding dirties every BRDF.

**Roughness ≠ Phong shininess.** They are not reciprocals of each other under a simple
map. Use a documented conversion if you import specular-glossiness assets
(`SpecularGlossinessMaterial`).

**Metalness binary in authorship.** Intermediate metallic values are allowed and used for
transitions, but most real materials sit near 0 or 1. Muddy mid-metallic often means a
wrong texture packing.

**Attenuation hacks.** `Linear` / `Quadratic` falloffs are non-physical conveniences.
`InverseSquare` with a smooth range window (`Smooth`) is the physically motivated choice
for point/spot.

**Ambient is a lie.** `AmbientLight` is a crude fill with no direction. Prefer image-based
`EnvironmentLight` once you care about look.

**Double-sided lighting.** `DoubleSided: true` flips normals for back faces; transparent
leaves and thin surfaces need it, solid volumes usually do not.

**Shadow bias.** `ShadowSettings.Bias` / `NormalBias` fight acne vs peter-panning — tune
per light, do not copy one universal bias.

## Try it

<details>
<summary>Exercise 1 — Pick units</summary>

You are modeling the sun. Which light type and photometric field?

**Answer.** `DirectionalLight` with `Illuminance` (and a small `AngularDiameter` for soft
shadows).
</details>

<details>
<summary>Exercise 2 — Dielectric highlight color</summary>

For `Metallic: 0`, should the specular highlight normally be tinted by `BaseColor` or
stay close to white (Fresnel aside)?

**Answer.** Close to white — dielectrics do not tint specular with albedo the way metals
do.
</details>

<details>
<summary>Exercise 3 — Disable clear coat</summary>

In `ClearCoatLayer`, which field disables the layer?

**Answer.** `Intensity: 0` (per the layer’s documented convention).
</details>

## Library recommendations

- **missing-function** — `49-lights.plato` / `50-materials.plato`: no declared shading
  entry point such as `Shade(material, lights, geo, view): Color` or even
  `Irradiance(light, position, normal)`. Photometric fields cannot be taught to
  completion without an evaluation API that consumes them.

- **wrong-shape** — `50-materials.plato`: `Brdf` exists as a free enum but `Material`
  does not select one. Either attach `Brdf` to `Material` / `TexturedMaterial` or
  document that `GgxMetallicRoughness` is mandatory for `Material` and move other BRDFs
  to alternate material types only.

- **doc-comment** — `49-lights.plato`: `PointLight.Range` of zero means unlimited, while
  many engines use zero as "disabled." Bold that sentinel in the field comment; it is an
  easy interoperability footgun.

- **missing-type** — `49-lights.plato`: a sum `type Light = Directional(DirectionalLight) |
  Point(PointLight) | …` (or a scene list concept) would let examples and scene graphs
  refer to "a light" without inventing host-side unions. Right now only the
  `LightSource` concept (shadows only) unifies them.
