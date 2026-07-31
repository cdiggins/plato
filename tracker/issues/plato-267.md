---
id: plato-267
title: Generate images for Plato types
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-257, submodules/Plato/plato-src-v3]
---

## Idea

Generate illustrative images (and/or diagrams) for Plato types — e.g. a picture of a torus, a vector diagram, a color-space sketch — so docs, lessons, and a library browser can show what a type *is* without reading prose. Scope includes still images; optional later: animated or interactive figures.

## Assumptions

- Many Plato types are geometric or visual and benefit more from a picture than from another sentence.
- Generation can be offline/batch (asset pipeline), not live-in-editor.
- Forward stdlib + lessons ([plato-257](plato-257.md)) / browser ([plato-265](plato-265.md)) are the primary consumers.

## Design decisions

- **Source of truth** — hand-authored SVGs vs rendered from Plato/TS demos vs AI-generated rasters vs mixed.
- **Association** — filename convention beside the `.plato` file vs frontmatter/`@image` in doc comments vs separate manifest.
- **Fidelity bar** — schematic diagram vs photoreal vs exact plot from the type's formula.
- **Which types** — only concrete geometric shapes vs also abstract concepts (needs diagrams, not "photos").

## Related

- [plato-265](plato-265.md) — HTML browser is the natural gallery.
- [plato-257](plato-257.md) — lessons need figures; recommendations already note pedagogy gaps.
- [plato-266](plato-266.md) — external links complement, don't replace, first-party images.
- Historical geometry demos / TS samples under `submodules/Plato/demos/` as possible render oracles.

## Approaches

Short term: pick 10 flagship types; produce SVGs or renders; embed in spike browser pages.
Long term: CI regenerates plots from Plato formulas where possible; AI fills schematic gaps with review.
Adjacent: type "cards" for social/docs with image + one-line blurb.

## Case against

- **Maintenance.** Images drift from definitions (especially AI art that isn't formula-true).
- **Abstract types.** `Monoid` / `MetricSpace` don't have obvious pictures; forcing imagery adds kitsch.
- **Pipeline cost.** A render/codegen path is a project; hand assets don't scale to 1000+ types.
- **Lessons-first.** Figures may belong in lesson markdown, not beside every stdlib declaration.

**Verdict: pursue** for a curated geometric subset tied to the HTML browser spike; do not attempt full-library coverage. Prefer formula-true plots/SVGs over decorative AI where the type has a clear visual.

## Bedrock

Strengthens the **type → teaching artifact** mapping used by browser and lessons — assets keyed by type name. **Verdict: simplest-along-the-grain** — curated assets for spike types; must NOT build an automated full-corpus image factory until association + review rules exist.

## Done means

- [ ] Naming/association convention for type images
- [ ] Pilot gallery (≥5 types) usable from browser or lessons
- [ ] Policy written: which types get images and what "correct" means

## Simplest possible implementation

Hand-make or agent-make a few SVGs/PNGs for classic solids/curves; reference them from spike HTML pages.

Pros: validates usefulness fast  
Cons: doesn't scale; risk of one-off unmanaged assets
