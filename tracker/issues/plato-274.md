---
id: plato-274
title: Typed SVG specification library in Plato
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [labs-legacy/Ara3D.SVG.Creator, studio-168, plato-267, submodules/Plato/stdlib/vector-styling.plato, submodules/Plato/stdlib/paths.plato, submodules/Plato/stdlib/scene2d.plato]
---

## Idea

Build a Plato library that is a **typed model of the SVG specification** (elements, attributes, path data, transforms, paint/stroke) — not a thin string emitter. Illegal SVG structures should be hard or obvious to express; path commands, lengths, and presentation attributes become Plato types/interfaces. Goal: author, transform, and serialize SVG from portable pure code (and eventually feed Studio’s SVG/vector flowable path).

## Assumptions

- SVG 1.1 / SVG 2 subset is enough for geometry/docs/UI; full W3C surface is neither needed nor desirable at first.
- Existing stdlib pieces (`paths.plato`, `vector-styling-stroke.plato` / `vector-styling-paint.plato`, `scene2d.plato`) are building blocks, not an SVG DOM.
- Prior art in-repo: `labs-legacy/Ara3D.SVG.Creator` (path builders wrapping the C# `Svg` package) and older SvgEditor labs — useful for command inventory, not the typed core.
- Serialization to XML/SVG text is a separate thin layer over the typed model (like JSON codecs over ASTs).

## Design decisions

- **Coverage** — path + basic shapes + groups + style first vs attempt full SVG DOM. Prefer a **geometry-useful subset** (path, rect, circle, ellipse, line, polyline/polygon, g, transform, fill/stroke).
- **Path data** — algebraic sum type of commands (`Move`, `Line`, `Cubic`, `Quad`, `Arc`, `Close`) vs string DSL. Prefer closed ADT + flatten-to-string.
- **Units / lengths** — `Number` only vs typed `SvgLength` (px/em/%). Start with user-unit `Number`; add units when a consumer needs them.
- **Relationship to Scene2D** — SVG library *is* Scene2D, *projects from* Scene2D, or stands alone. Prefer standalone SVG types with a projection from/to Scene2D/paths.
- **Spec fidelity** — mirror SVG element names 1:1 for learnability vs Plato-idiomatic names (`SvgPath` vs `Path2D`). Prefer SVG-familiar names with Plato casing.

## Related

- [labs-legacy/Ara3D.SVG.Creator](../../labs-legacy/Ara3D.SVG.Creator) — imperative path builder over Svg.NET.
- [studio-168](studio-168.md) — SVG/vector paths as a flowable Studio type.
- [plato-267](plato-267.md) — type images (often SVG assets); consumer of emit, not the typed model.
- [vector-styling-stroke.plato](../../submodules/Plato/stdlib/vector-styling-stroke.plato) / [vector-styling-paint.plato](../../submodules/Plato/stdlib/vector-styling-paint.plato) — stroke/fill vocabulary overlap.
- [paths.plato](../../submodules/Plato/stdlib/paths.plato) — abstract paths; SVG path data should map here.
- [scene2d.plato](../../submodules/Plato/stdlib/scene2d.plato) — 2D scene graph; possible projection target/source.

## Approaches

Short term: declare `SvgPathCommand` ADT + `SvgPath` / basic shapes + `ToSvgString` for a path; round-trip a handful of golden SVG snippets.

Long term: groups, transforms, styles, text, clipPaths; Studio import (SVG → profiles → extrude); doc/lesson figure generation from typed scenes.

Adjacent: SVG parser (string → typed); CSS presentation attributes vs style element; SVG fonts/icons pack.

## Case against

- **Spec gravity well.** “Typed SVG spec” invites endless attribute coverage; most Studio value is path + style.
- **Duplicate Scene2D.** Two 2D scene models (Scene2D and SVG DOM) will drift unless one is clearly a serialization view.
- **Host libraries exist.** Svg.NET / browser DOM already parse and emit; a Plato model only pays off if algorithms run *on* the typed form across backends.
- **Percentages and CSS.** Real SVG is tangled with CSS/cascade; a pure subset may be “not really SVG.”

**Verdict: pursue** as a **geometry-useful typed subset** (path commands + shapes + group/transform + paint), explicitly *not* a full SVG 2 implementation. Park parser and CSS until emitters prove useful. Drop “complete spec mirror” as a goal.

## Bedrock

Strengthens the **vector interchange seam**: one algebraic SVG/path model in stdlib that Studio (studio-168), docs/figures (plato-267), and codegen backends can share — instead of string pasting or C#-only Svg.NET wrappers. **Verdict: simplest-along-the-grain** — path ADT + basic shapes + string emit; must NOT build a full DOM/CSS engine or replace Scene2D in the same pass.

## Done means

- [ ] Plato types for path commands and a small shape/group set under `stdlib` (e.g. `svg.plato`)
- [ ] `ToSvg` / string serialization for paths and a compound document (group of shapes)
- [ ] At least one golden fixture (hand SVG ↔ typed → SVG) in tests or conformance
- [ ] Doc note: subset boundary (what is in / explicitly out) and relation to `paths` / `scene2d` / `vector-styling`

## Simplest possible implementation

One `svg.plato` with path-command sum type, `SvgPath`, `SvgCircle`/`SvgRect`, and pure functions that emit path `d` strings and a minimal `<svg>…</svg>` document.

Pros: validates typing payoff quickly; feeds Studio SVG flow and lesson figures.
Cons: incomplete vs real SVG; risk of parallel Scene2D without a projection story.
