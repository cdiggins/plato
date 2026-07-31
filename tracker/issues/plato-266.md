---
id: plato-266
title: Inline external links in Plato stdlib docs
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-257, submodules/Plato/plato-src, submodules/Plato/plato-src-v3]
---

## Idea

Plato stdlib declarations should carry inline links to external authorities — candidates include MathWorld, Wikipedia, hosted Plato docs/library browser, and lesson pages — so a reader (human or agent) can jump from a type to the math or pedagogy behind it. Historical `plato-src` curves already cite Wikipedia in places; systematize and extend.

## Assumptions

- Doc comments (or a structured attribute/annotation) can hold URLs without breaking the compiler.
- External sites are stable enough for curated links (or we accept occasional rot + a link-check job).
- Lessons ([plato-257](plato-257.md)) and an HTML browser ([plato-265](plato-265.md)) will exist as optional targets, not blockers.

## Design decisions

- **Authority preference** — MathWorld vs Wikipedia vs both (MathWorld for formulas, Wikipedia for intuition).
- **Encoding** — free-text markdown URLs in comments vs structured `see:` / `@link` fields parsers understand.
- **Coverage policy** — every concrete geometric type vs only classical named curves/surfaces vs opt-in.
- **Lesson links** — relative paths into `docs/lessons/` vs published site URLs.

## Related

- [plato-257](plato-257.md) — lesson corpus as pedagogy target.
- [plato-265](plato-265.md) — HTML browser should render these links.
- [plato-264](plato-264.md) — hover can surface the same URLs.
- `plato-src/curves.plato` — existing Wikipedia-citation habit to generalize.

## Approaches

Short term: convention in doc comments (`Wikipedia: …`, `MathWorld: …`) on a high-value file; lint optional.
Long term: structured metadata + link checker + browser/hover consumers.
Adjacent: cite primary papers/DOIs for less-classical types.

## Case against

- **Link rot.** Wikipedia/MathWorld URLs and titles change; unchecked links become noise.
- **Licensing/branding.** Heavy reliance on third-party pages may look unfinished vs first-party docs.
- **Comment clutter.** URLs dwarf the mathematical content in short declarations.
- **Lessons may supersede.** If first-party lessons cover the type, external links are secondary.

**Verdict: pursue** as a light convention first (Wikipedia and/or MathWorld on classical named types), structured fields only once a consumer (browser/hover) needs them. Park blanket coverage until the browser spike lands.

## Bedrock

Strengthens **stdlib declarations as the single doc authority** that other surfaces (hover, HTML, lessons) project — links live next to the type, not in a separate wiki. **Verdict: simplest-along-the-grain** — comment URL convention; must NOT build a custom link DSL or scraper before a consumer exists.

## Done means

- [ ] Written convention for where/how links appear in `.plato` docs
- [ ] Pilot set of types linked (one domain) following the convention
- [ ] At least one consumer path documented (browser, hover, or lessons)

## Simplest possible implementation

Add Wikipedia (and optionally MathWorld) URLs to doc comments on curves/surfaces in the forward stdlib; document the pattern in the agent guide.

Pros: zero tooling; immediate value for readers  
Cons: manual; no validation; easy to forget on new types
