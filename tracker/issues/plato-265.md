---
id: plato-265
title: HTML Plato library browser via Fable spike
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-230, submodules/Plato/plato-src-v3, docs/plato-v3-vocabulary-report.md]
---

## Idea

Ship a browsable HTML catalog of the Plato standard library (types, concepts, docs, cross-links). Approach: use a Fable agent to author a **small spike** (a couple of representative pages), verify the look/IA feels right, then extend generation across the whole library — do not boil the ocean first.

## Assumptions

- Forward vocabulary (`plato-src-v3` / eventual `stdlib`) is the primary browse target; shipping `plato-src` optional or secondary.
- Static HTML (or lightly interactive) is enough for v1; no need for a live Plato runtime in-browser yet ([plato-257](plato-257.md) V2 territory).
- Navigation/index or a lint/AST dump can supply the symbol graph for generation.

## Design decisions

- **Spike corpus** — hand-pick 2–3 domains (e.g. vectors + transforms) vs auto-generate two random files.
- **Generator host** — one-off Fable pages checked in vs scripted emitter from Plato sources once style is approved.
- **IA** — flat type list vs concept lattice vs domain chapters matching numbered `plato-src-v3` files.
- **Hosting** — local `file://` / docs folder vs GitHub Pages.

## Related

- [plato-230](plato-230.md) / [docs/plato-v3-vocabulary-report.md](../../docs/plato-v3-vocabulary-report.md) — vocabulary inventory to browse.
- [plato-257](plato-257.md) — lessons corpus; browser could link out to lessons later.
- [plato-264](plato-264.md) / [plato-266](plato-266.md) / [plato-267](plato-267.md) — hover, external links, and images all feed or consume this site.
- [studio-128](studio-128.md) — Studio HTML guide (different product; reuse static-site habits only).

## Approaches

Short term: Fable produces 2–3 polished pages for one domain; human reviews; then a generator clones the template over all types.
Long term: full site with search, concept graph, images, Wikipedia/MathWorld links, lesson deep-links.
Adjacent: emit the same pages from CI on stdlib change.

## Case against

- **Markdown may be enough.** Lessons + vocabulary reports already teach; a site is another surface to maintain.
- **Spike-then-scale fails** if the hard problem is data (doc quality, cross-links), not page chrome — pretty empty pages don't validate the product.
- **Agent-authored HTML** can be inconsistent; style may need a human design pass before scaling.

**Verdict: pursue** with a hard spike gate: only extend after 2–3 pages are judged useful for looking up a type. Drop if the spike shows the bottleneck is missing docs, not missing HTML.

## Bedrock

Strengthens the **stdlib-as-readable-API** boundary: one generated consumer of declaration + doc comments, forcing comments and naming to be publishable. **Verdict: simplest-along-the-grain** — spike pages first; must NOT build a full search SPA, live evaluator, or dual old/new library site in the spike.

## Done means

- [ ] 2–3 spike pages reviewed and accepted (or rejected with written reasons)
- [ ] If accepted: generator covers full forward stdlib with same template
- [ ] Entry point documented (how to open/build the site)

## Simplest possible implementation

Hand-generate (via Fable) pages for `Vector3` / `Matrix4x4` / one concept file; check into `docs/` or `submodules/Plato/docs/api/`; decide go/no-go before automation.

Pros: validates IA cheaply; no toolchain risk  
Cons: throwaway if rejected; manual pages drift immediately
