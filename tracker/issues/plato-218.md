---
id: plato-218
title: Plato glyph library: fonts as first-class contour geometry
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-25
closed:
links: [tracker/issues/plato-028.md, tracker/issues/studio-155.md, tracker/issues/studio-205.md]
---

## Idea
Make glyphs first-class Plato geometry: a `Glyph` is a list of closed contours, a contour a list
of quadratic Bezier segments — exactly what TTF outlines are. Host-side C# parses font files and
lowers them into Plato glyph values; everything downstream is pure Plato geometry that composes
with the existing stdlib: flatten → polygon → earcut → mesh (a Studio "3D Text" generator),
extrude/offset/stroke, distance-to-contour → SDF text (pairs with the GLSL backend), and the
msdfgen core (edge coloring + per-channel signed distance, a pure `contours → texel grid`
function). One data type unifies 2D UI text, 3D text solids, and shader-rendered text.
Prompted by the Peacock workbench text-rendering work: WPF DrawText was the perf bottleneck and
the planned Silk.NET/GL backend has no glyph path (studio-155 explicitly skips DrawText).

## Assumptions
- Font FILE parsing (TTF/OTF tables, cmap, composite glyphs, hinting) stays host-side C# using an
  existing parser (Typography / SixLabors.Fonts / stb port) — I/O at the edge, geometry inside.
- Layout scope is deliberately Latin-ish: advance widths + kerning pairs. No shaping, ligatures,
  bidi (HarfBuzz territory — out of scope permanently, not "later").
- Small crisp UI text (~11px, hinted) is NOT the target; a bitmap-atlas library (FontStashSharp)
  serves that better for years. Plato text wins where geometry wins: large, zoomed, rotated,
  extruded, shader-evaluated.
- Earcut port exists and works (plato-028 tracks its exposed gaps); mesh extrusion exists.

## Design decisions
- Glyph curve representation — quadratic-only (native TTF) vs unified quad/cubic segments (OTF/CFF
  needs cubics). Quad-only is simpler and covers TTF; cubics widen font support but complicate
  every distance function.
- Where MSDF generation runs — pure Plato on CPU vs Plato-emitted GLSL. CPU first (testable,
  deterministic); GLSL later as an optimizer/backend showcase.
- Distance function API — per-segment nearest-point solve (robust cubic root, degenerate cases)
  exposed as stdlib, vs internal-only. Exposing it benefits curves generally (plane cuts, hit
  testing) — prefer stdlib.
- Atlas packing — Plato pure function vs host bookkeeping. Rect/skyline packing is a clean pure
  algorithm; texture lifetime stays host-side regardless.

## Related
- [plato-028] — earcut consumer-driven gaps; glyph triangulation is a new demanding consumer.
- [studio-155] — Peacock GL overlay pass has no glyph rasterizer (DrawText skipped); this supplies
  the long-term text path for GL Peacock backends.
- [studio-205] — tool icons want scalable glyphs; an MSDF glyph path would serve icon rendering.

## Approaches
Short term: (1) host importer lowering TTF glyphs to contour values; (2) flatten → earcut →
extrude = Studio "Text" 3D generator (days of work, all building blocks exist); (3) robust
nearest-point-on-quadratic-Bezier as a stdlib function.
Long term: MSDF core port (faithful msdfgen algorithm — corner-preservation heuristics are
fiddly; do not reinvent), Plato-emitted GLSL glyph shading, text-on-path via existing deform
modifiers (bend/twist text along a curve).
Adjacent ideas worth their own issue: Studio "Text" generator as a shipped tool; FontStashSharp
adapter for a Silk.NET Peacock host (near-term UI text, independent of Plato).

## Bedrock
The seam is "font files end at the importer": host C# owns bytes-to-contours, Plato owns
everything after. That is the same importer boundary the other geometry sources use, and it keeps
shaping/i18n permanently outside the geometry library. The stdlib gains distance-to-Bezier and
contour ops that plane cuts, curve hit-testing, and offsetting already want — text is just the
first paying customer. Verdict: **simplest-along-the-grain**. The simple version must NOT parse
font binaries inside Plato and must NOT bake layout decisions (kerning, line breaking) into glyph
geometry types.

## Done means
- [ ] `Glyph`/`Contour` types in the Plato stdlib with flatten + triangulate + extrude
- [ ] Host importer converts a TTF glyph to contours (verified against a known glyph's point data)
- [ ] Studio generator produces a 3D text mesh from a string
- [ ] Nearest-point-on-quadratic-Bezier stdlib function with degenerate-case tests

## Simplest possible implementation
Host importer (existing parser) → contour records → flatten at fixed tolerance → existing earcut
→ existing extrude → Studio generator script. No SDF, no MSDF, no atlas, no kerning beyond
advance widths.
Pros:
- 3D text in Studio within days; exercises earcut on hostile real-world input (plato-028)
- Establishes the importer seam and stdlib types without committing to any rendering strategy
Cons:
- No crisp 2D text path yet (UI text still needs FontStashSharp host-side)
- Fixed flatten tolerance bakes resolution in; SDF/MSDF quality work all still ahead
