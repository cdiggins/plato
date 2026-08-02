---
id: plato-340
title: Re-parent color types under a shared Color interface
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-139, plato-283, plato-284, submodules/Plato/stdlib/color.plato, submodules/Plato/stdlib/color-spaces-models.plato, submodules/Plato/stdlib/color-spaces-video.plato, submodules/Plato/stdlib/points.concepts.plato, submodules/Plato/stdlib/algebra-numeric.concepts.plato, tracker/decisions/2026-07-29-static-interface-members.md]
---

## Idea
Today Plato's color representations mostly sit under bare `Value` (or under `Numerical`, which already inherits `Value`). There is no shared interface that says "this is a color." The idea is to re-parent them under something more specific — candidates floated were Measure-like, Coordinate-like, or a dedicated Color family — and that **all color representations should share a common Color identity** so they are uniformly treated as colors.

Verified inventory (stdlib): `Color` and `ColorXYZ` implement `Numerical`; `Color8` implements `Value, Hashable`; `ColorHSV`, `ColorHSL`, `ColorSRGB`, `ColorYUV`, `ColorYCbCr`, `ColorCMYK`, `ColorHWB`, `ColorXyY`, `ColorLab`, `ColorLCh`, `ColorLuv`, `ColorOkLab`, `ColorOkLCh` implement `Value` only. There is **no** `Measure` interface in the stdlib. There **is** a `Coordinate` interface (`points.concepts.plato`), but it is spatial (`Value + Equatable + Interpolatable`) and used by points/UV/geo types — not a natural parent for colors. A concrete type already named `Color` (linear float RGBA) occupies the obvious interface name.

## Assumptions
- Callers and libraries want to write generic code over "any color representation" (convert, mix, sample fields) without listing every `Color*` type.
- Bare `Value` is too weak a taxonomy signal; colors are not just another record.
- Putting colors under spatial `Coordinate` would be a category error (position vs appearance).
- Inventing a full `Measure` hierarchy just to house colors is out of scope unless Measure already has independent demand.
- Renaming the existing `Color` type is expensive (materials, lights, meshes, codegen, ADRs around `Color.Zero`).

## Design decisions
- **Parent interface** — dedicated Color-family interface vs stretch `Coordinate` vs invent `Measure`. Prefer dedicated; Coordinate is spatial; Measure does not exist.
- **Interface name vs type `Color`** — cannot both be `Color`. Options: rename the RGBA type (`Rgba` / `LinearRgba` / `ColorRgba`) and take `interface Color`; or keep type `Color` and name the interface `ColorModel` / `ColorSpace` / `ColorRepresentation`.
- **What the interface obligates** — marker-only (`inherits Value`) vs require conversion-to-canonical (`ToColor` / `ToLinearRgba`) vs require channel/interpolability obligations. Marker is cheapest; conversion obligation is the first useful generic surface.
- **Who implements it** — only alternate spaces (`ColorHSV`…), or also canonical `Color` / `Color8` / gradients/stops? Gradients and stops are containers, not colors.
- **Relationship to `Numerical`** — `Color`/`ColorXYZ` stay `Numerical` and *also* implement the Color interface, or Color interface sits between Value and Numerical for those types. Prefer additive `implements Numerical, ColorModel` over demoting Numerical.

## Related
- [plato-139](plato-139.md) — OKLab + perceptual mix; same color domain, assumes type surface, not interface taxonomy.
- [plato-283](plato-283.md) — color constants folder; orthogonal layout, not interface parentage.
- [plato-284](plato-284.md) — OpaqueColor8 / ColorWithAlpha8; new color types would need the same parent once chosen.
- [plato-334](plato-334.md) — parallel "many types, only `implements Value`" taxonomy debt in query/solve results (pattern, different domain).
- [stdlib/color.plato](../../submodules/Plato/stdlib/color.plato) — `Color`, `Color8`, `ColorHSV`, `ColorHSL`.
- [stdlib/color-spaces-models.plato](../../submodules/Plato/stdlib/color-spaces-models.plato) — XYZ/Lab/OkLab family.
- [stdlib/color-spaces-video.plato](../../submodules/Plato/stdlib/color-spaces-video.plato) — SRGB/YUV/CMYK/HWB family.
- [stdlib/points.concepts.plato](../../submodules/Plato/stdlib/points.concepts.plato) — existing `Coordinate` (spatial; do not overload).
- [stdlib/algebra-numeric.concepts.plato](../../submodules/Plato/stdlib/algebra-numeric.concepts.plato) — `Numerical` inherits `Value`.
- [tracker/decisions/2026-07-29-static-interface-members.md](../decisions/2026-07-29-static-interface-members.md) — `Color.Zero` / Numerical obligations; any rename of type `Color` collides with this ADR surface.

## Approaches
Short term: add a thin marker interface (provisional name `ColorModel`) in a color interfaces file; have every alternate-space `Color*` type `implements ColorModel` (keep existing `Value`/`Numerical`); leave type `Color` named as today and also implement the interface.
Long term: settle naming (interface vs RGBA type), add a conversion obligation to the canonical linear form, and let imaging/materials APIs take `ColorModel` where any representation is fine.
Adjacent ideas worth their own issue:
- Whether a general `Measure` interface belongs in the numeric hierarchy (independent of color).
- Whether `ColorStop` / `ColorGradient` should implement a separate `ColorRamp` interface.

## Bedrock
The seam is the missing interface between bare `Value` and the many `Color*` records in `stdlib/color*.plato` — the same pattern as `Coordinate` for points. A shared Color-family interface makes generic conversion/mix APIs and future types (plato-284) land on one implements line instead of ad-hoc lists. Verdict: **simplest-along-the-grain**. Simple version must NOT rename type `Color`, must NOT hang colors off spatial `Coordinate`, and must NOT invent a Measure hierarchy in the same change.

## Done means
- [ ] A Color-family interface exists and is documented as the parent for color representations
- [ ] All alternate-space color types (`ColorHSV`, `ColorHSL`, Lab/OkLab/YUV/CMYK/… families) implement that interface
- [ ] Canonical `Color` (and ideally `Color8`) implement it without losing `Numerical` / `Hashable` where they have them today
- [ ] No color type is re-parented under spatial `Coordinate`
- [ ] Fast stdlib gate still passes after the implements changes

## Simplest possible implementation
Add `interface ColorModel inherits Value { }` (name TBD) next to the color types; change each color-representation type's implements line to include it; skip containers (`ColorStop`, `ColorGradient`, `ColorSpaceConversion`).

Pros:
- Immediate taxonomy signal; unlocks generic signatures later
- Tiny diff; no rename blast radius
- Matches how `Coordinate` marks spatial values

Cons:
- Marker-only until conversion obligations exist
- Interface name still conflicts with vernacular "Color" until an ADR picks one
- Does not by itself fix inconsistent Numerical vs Value among color types
