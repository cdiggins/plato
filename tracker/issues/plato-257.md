---
id: plato-257
title: Plato geometry/math lessons (V1 markdown corpus)
type: idea
status: done
priority: p2
effort: L
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [plato-078, docs/discussions/plato-geometry-july-7-2026-12h44.md, submodules/Plato/demos/typescript/geometry-samples, submodules/Plato/plato-src/curves.plato, submodules/Plato/plato-src-v3, submodules/Plato/docs/plato-257-lessons-v1-plan.md, ara3d-040, studio-041]
---

## Idea

Ship a large collection of small, self-contained geometry and mathematics lessons for Plato. V1 is **markdown only**: each lesson teaches one real idea using `plato-src-v3` vocabulary as notation, stands entirely alone (no index, no cross-lesson refs), and ends with specific library recommendations surfaced by writing. This stress-tests the v3 declaration surface and captures vocabulary gaps before implementation bodies land.

A later phase (still gated on [plato-078](plato-078.md)) rewrites survivors into an interactive online textbook with TypeScript emission, live figures, and curriculum structure — the original vision below.

## Assumptions

- `plato-src-v3` declarations + doc comments are sufficient notation for teaching semantics without runnable code.
- Lessons authored against v3 surface the highest-value vocabulary feedback before bodies are implemented.
- Self-contained markdown lessons can ship in parallel (one file per slug) without site infrastructure.
- Content cost is tractable when scoped to ~60 small lessons (~150–400 lines each) rather than a full interactive curriculum.

## Design decisions

- **V1 output** — one file per lesson at `submodules/Plato/lessons/v1/<slug>.md`; no index, no README, no shared assets. Catalog lives in [plato-257-lessons-v1-plan.md](../../submodules/Plato/docs/plato-257-lessons-v1-plan.md).
- **V1 format** — YAML front matter + hook / idea / In Plato / pitfalls / optional Try it + required `## Library recommendations`. See plan for normative rules.
- **v3 names only** — every Plato identifier must exist in `plato-src-v3`; gaps go in recommendations, not invented silently.
- **Orchestration** — waves of 5–8 parallel author agents; after each wave, skim for rule violations, harvest recommendations into `docs/plato-257-lessons-v1-recommendations.md`, commit the wave.
- **Do not edit v3** — recommendations are recorded and triaged separately.

## Related

- [plato-257-lessons-v1-plan.md](../../submodules/Plato/docs/plato-257-lessons-v1-plan.md) — V1 catalog, format, waves, acceptance.
- [plato-src-v3](../../submodules/Plato/plato-src-v3/) — normative vocabulary for lesson notation.
- [plato-078](plato-078.md) — gates V2 interactive TypeScript textbook, not V1 markdown.
- [docs/discussions/plato-geometry-july-7-2026-12h44.md](../../docs/discussions/plato-geometry-july-7-2026-12h44.md) — browser geometry demo; relevant to V2 packaging.
- [submodules/Plato/demos/typescript/geometry-samples](../../submodules/Plato/demos/typescript/geometry-samples) — V2 host scaffold.
- [ara3d-040](ara3d-040.md) / [studio-041](studio-041.md) — Studio teaching demos; complementary.

## Approaches

**V1 (now):** Execute the plan catalog — ~60 standalone markdown lessons in waves, recommendations aggregation, no site.

**V2 (later, plato-078):** Structured curriculum with index, cross-links, MDX/site shell, Plato-emitted TypeScript interactive figures.

Adjacent ideas worth their own issue:
- Literate Plato → HTML emitter.
- Publishable npm package for Plato geometry TS.

## Bedrock

V1 dogfoods `plato-src-v3` as curriculum source *before* bodies exist — the cheapest stress test of declared vocabulary. Recommendations become actionable tracker input for v3 refinement. **Verdict: simplest-along-the-grain** — V1 must NOT invent a site, TS emission, or edit v3; it writes lessons and records gaps.

## Done means

- [x] **≥100** standalone markdown lesson files under `submodules/Plato/lessons/v1/*.md` (plan catalog first, then additional slugs on undeclared-but-teachable v3 angles)
- [x] `submodules/Plato/lessons/v1/` exists with one standalone markdown file per catalog slug in [plato-257-lessons-v1-plan.md](../../submodules/Plato/docs/plato-257-lessons-v1-plan.md) (~60 slugs across all domain groups)
- [x] Every lesson file follows the V1 format: complete YAML front matter (`lesson`, `title`, `domain`, `v3-files`, `audience`, `status: draft-v1`); body with hook, idea, "In Plato", pitfalls; required `## Library recommendations` with ≥1 specific file-and-declaration item (or explicit "vocabulary fully covered this lesson")
- [x] Hard rules satisfied per lesson: no cross-lesson/chapter/index references; Plato identifiers spot-checked against `plato-src-v3`; no edits to v3 or other repo files during authoring
- [x] Wave 1 catalog slugs landed and committed: `points-vs-vectors`, `quaternions-without-tears`, `signed-distance-fields`, `bezier-curves`, `indexed-meshes`, `linear-vs-gamma`, `floating-point-tolerance`
- [x] Remaining catalog slugs landed in subsequent waves with one commit per wave (`feat(plato-257): lessons v1 wave N — <slugs>`)
- [x] `submodules/Plato/docs/plato-257-lessons-v1-recommendations.md` aggregates and de-duplicates all `## Library recommendations` sections; high-signal items filed as separate tracker issues where warranted
- [x] Plan acceptance checklist spot-checked on a sample from each domain group (standalone, teaches one idea, 150–400 lines, identifiers resolve)

## Simplest possible implementation

Create `lessons/v1/`, write wave 1 (7 slugs spanning domains), harvest recommendations, commit. Repeat waves until catalog complete. No tooling beyond markdown files and the existing plan catalog.

Pros:
- Parallel-safe (one writer per file)
- No dependency on plato-078 or TS writer
- Direct vocabulary feedback loop into v3 planning

Cons:
- No interactivity or runnable examples in V1
- ~60 lessons is substantial authoring volume
- Recommendations triage is manual orchestrator work

## Case against

- **Content cost dominates.** ~60 dense lessons is weeks of careful writing even with parallel agents; quality may vary across authors.
- **Markdown-only may feel underwhelming** compared to Desmos/GeoGebra until V2 lands — but V1's job is vocabulary stress test, not public launch.
- **Risk of invented identifiers** if agents skip v3 verification — mitigated by spot-checks and MCP/search per wave.
- **V2 still blocked on plato-078** for the interactive textbook vision.

**Verdict: pursue (V1 scope).** V1 is unblocked: markdown lessons against declared v3 need no TS writer, no site, no plato-078. Park the interactive TypeScript textbook until plato-078 is at least `ready`/`in-progress` with a green sample suite; execute V1 now per the plan.

## Later / full vision (V2, gated on plato-078)

Interactive online textbook for classical geometry whose definitions, formulas, and runnable examples are authored in Plato and executed in the browser via TypeScript emission. Plato source doubles as the mathematical specification; the textbook layers prose, diagrams, and live canvases on top.

V2 Done means (not V1):
- At least one published chapter with ≥3 interactive figures driven by Plato-emitted TypeScript
- Each figure links to (or embeds) the corresponding Plato definition
- CI (or documented regen script) rebuilds the TS used by the site from Plato source
- Landing page states scope and how to run locally

Simplest V2 spike: static site wrapping `demos/typescript/geometry-samples` — one Curves chapter, three canvases, deploy to GitHub Pages.
