---
id: plato-275
title: SVG parser for Plato (string to typed model)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-274, studio-168, labs-legacy/Ara3D.SVG.Creator, submodules/Plato/stdlib/paths.plato, submodules/Plato/stdlib/scene2d.plato]
---

## Idea

A pure Plato parser that turns SVG text (or at least path `d` strings and a geometry-useful element subset) into the typed SVG / path model from [plato-274](plato-274.md). Complements the emitter: round-trip fixtures, Studio import (SVG → profiles → extrude per [studio-168](studio-168.md)), and lesson/figure pipelines that start from existing SVG assets.

## Assumptions

- [plato-274](plato-274.md) (or an equivalent typed path/shape ADT) exists or lands first — parsing into strings only is not the point.
- A **geometry-useful subset** is enough: path data, basic shapes, groups, transforms, fill/stroke attributes. Full SVG + CSS cascade is out of scope for v1.
- Plato can express a recursive-descent or combinator parser over strings/arrays well enough for this subset (or path-`d` alone is the first milestone if full XML is painful).
- Host parsers (Svg.NET, browser DOM) remain available for C#; the Plato parser is for portable algorithms and multi-backend use.

## Design decisions

- **What to parse first** — path `d` only vs minimal XML document (`svg`/`g`/`path`/`rect`/…). Prefer `d` first (high value, no XML), then a thin element subset.
- **XML strategy** — hand-rolled tag/attr lexer in Plato vs assume a host supplies a pre-tokenized element tree and Plato only interprets geometry attrs. Prefer pure Plato for `d`; for XML, decide whether Plato owns markup or consumes a structural token stream.
- **Error model** — fail the whole document vs partial parse with skipped unknown elements. Prefer skip-unknown for import robustness; hard-fail on malformed path commands when strict mode is on.
- **Output target** — typed SVG ADT (plato-274) vs flatten straight to `Path2D` / `Scene2D`. Prefer typed SVG, then project to paths/scene.
- **Units / percentages** — resolve to user units at parse time vs keep `SvgLength`. Defer %/em until a consumer needs them; user units only in v1.

## Related

- [plato-274](plato-274.md) — typed SVG model + emitter; this issue is the inverse; listed there as adjacent.
- [studio-168](studio-168.md) — SVG/vector paths as Studio flowable (import → profile → extrude).
- [labs-legacy/Ara3D.SVG.Creator](../../labs-legacy/Ara3D.SVG.Creator) — imperative builders over Svg.NET (emit-side prior art, not a Plato parser).
- [paths.plato](../../submodules/Plato/stdlib/paths.plato) — likely projection target after parse.
- [scene2d.plato](../../submodules/Plato/stdlib/scene2d.plato) — alternate projection for grouped SVG.

## Approaches

Short term: parse SVG path `d` → command ADT; golden strings from MDN/fixtures; round-trip with plato-274 emitter where overlap exists.

Long term: minimal SVG XML subset → typed document; Studio import node; optional strict/lenient modes; project to Scene2D.

Adjacent: CSS presentation / `style=` attribute (park); SVG transform attribute grammar; icon pack loader on top of parser.

## Case against

- **Depends on typed model.** Without plato-274, this is a string→string transformer or a one-off ADT that will be rewritten.
- **XML in Plato is heavy.** Markup parsing may belong in the host; only path/`d` may justify a pure Plato grammar.
- **Existing host parsers.** Svg.NET already imports; a Plato parser only pays off if algorithms run on the result across backends.
- **Spec surface.** Real SVG files use CSS, `<use>`, symbols, percentages — a subset parser will “fail” on many assets unless skip-unknown is well designed.

**Verdict: pursue** after (or tightly with) plato-274’s path ADT; start with **`d` parsing only**. Park full XML until emitter + `d` round-trips prove the typed model. Drop “parse any SVG file” as a goal.

## Bedrock

Strengthens the **SVG interchange seam** started in plato-274: string ↔ typed model ↔ paths/scene, so Studio import and doc pipelines share one grammar instead of host-only Svg.NET. **Verdict: simplest-along-the-grain** — path-`d` parser into the typed command ADT; must NOT implement CSS, `<use>`, or a general XML stack in the same pass.

## Done means

- [ ] Plato function(s) parse a representative set of SVG path `d` strings into the typed command ADT (plato-274 or interim)
- [ ] Golden fixtures: at least N known paths parse (and ideally round-trip via emitter)
- [ ] Documented subset + failure/skip policy
- [ ] Optional stretch: minimal `<svg>`/`<path>`/`<g>` document parse into the typed model

## Simplest possible implementation

A pure function `ParsePathData(d: String): Array<SvgPathCommand>` (or `Result`) covering M/L/H/V/C/Q/A/Z (absolute + relative); tests from a small fixture list; no XML.

Pros: unlocks round-trip and Studio path import without markup complexity.
Cons: cannot ingest real multi-element SVGs until an XML (or host-preparse) layer exists.
