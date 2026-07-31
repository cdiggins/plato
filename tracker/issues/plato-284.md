---
id: plato-284
title: Add OpaqueColor8 / ColorWithAlpha8 alongside Color8
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-139, plato-272, submodules/Plato/stdlib/color.plato, submodules/Plato/stdlib/color.library.plato]
---

## Idea
Today `Color8` is always RGBA (`R,G,B,A: Integer`). Named CSS/X11 constants are fully opaque (`A = 255`) but still construct a 4-channel value, and callers who only mean "byte RGB" keep repeating alpha. Introduce a clearer 8-bit vocabulary — e.g. `OpaqueColor8` / `Color8` (RGB only) and `ColorWithAlpha8` (RGBA) — so opaque storage and interop stop over-specifying alpha, and conversions make the alpha policy explicit.

## Assumptions
- Most named-color and texture-sample interop paths are opaque or treat alpha separately.
- `Color` (linear float RGBA) remains the computation canonical; 8-bit types stay storage/interop.
- Renaming or splitting `Color8` is a vocabulary break for any body/codegen already emitting `Color8`.

## Design decisions
- **Naming** — `Color8` = opaque RGB vs keep `Color8` = RGBA and add `Rgb8`/`Rgba8`. Prefer industry-familiar `Rgb8`/`Rgba8`, or `OpaqueColor8`/`ColorWithAlpha8` if emphasizing opacity semantics over channel count.
- **Which is canonical for constants** — opaque type (matches CSS table) vs keep RGBA with A=255. Prefer opaque as the constant result type; lift to alpha form at the boundary.
- **Conversion** — `WithAlpha(c, a)`, `Opaque(c)` / drop-alpha policy (premultiply? discard? require A==255?).
- **Does `Color8` stay as alias** — deprecate-as-alias to RGBA form for one release vs hard rename.

## Related
- [plato-272](plato-272.md) — color constants follow-up; result type of the table.
- [plato-283](plato-283.md) — where the constant table lives.
- [plato-139](plato-139.md) — OKLab / perceptual mix (float `Color` path; orthogonal but same color domain).
- [stdlib/color.plato](../../submodules/Plato/stdlib/color.plato) — current `Color` / `Color8` declarations.
- [stdlib/color.library.plato](../../submodules/Plato/stdlib/color.library.plato) — 141 constants all `A = 255` on `Color8`.

## Approaches
Short term: add `Rgb8` (or `OpaqueColor8`) with R,G,B; add conversions to/from current `Color8`; retarget named constants to the opaque type; leave `Color8` as RGBA.
Long term: rename to `Rgb8`/`Rgba8` everywhere; codegen/intrinsics map to backend packed formats; imaging APIs take the precise width.
Adjacent: plato-283 (constants file); byte formats in `images` / `texturing`.

## Bedrock
Strengthens the **type-affordance seam** for 8-bit color: illegal "forgot alpha" and "alpha was meaningless 255" states become representable separately. **Verdict: simplest-along-the-grain** — must NOT change linear `Color` or invent a generic `Optional` alpha; just two concrete 8-bit records + conversions.

## Done means
- [ ] Opaque 8-bit RGB type and RGBA 8-bit type both declared in `color.plato` (names TBD)
- [ ] Conversions between them (and to/from linear `Color` policy) are documented
- [ ] Named-color constants use the opaque type (or documented decision to keep RGBA)
- [ ] `lint stdlib` still 0 parse / 0 symbol errors

## Simplest possible implementation
Add `type OpaqueColor8 { R,G,B: Integer }` beside existing `Color8`; add `WithAlpha` / `WithoutAlpha`; move named constants to return `OpaqueColor8`.
- Pros: no rename of `Color8`; constants match their true opacity; small diff.
- Cons: three names in play (`Color`, `Color8`, `OpaqueColor8`); eventual `Rgb8`/`Rgba8` rename still likely.

## Case against
- **Alpha is cheap.** Four integers vs three is not a real cost; repeating `255` is noisy but clear.
- **Proliferation.** Extra color types multiply conversion tables, GLSL/C# mappings, and teaching surface.
- **CSS table is conventionally RGBA** in many APIs (HTML canvas, CSS Color Module) — opaque-only may surprise.
- Verdict: **pursue** if constant/API authorship pain is real; otherwise **park** until a concrete consumer (images/interop) demands packed RGB vs RGBA distinction. Prefer `Rgb8`/`Rgba8` names if promoted.
