# Plato issues — recommended execution order

**Date:** 2026-07-28  
**Scope:** Open tracker items with `area: plato` (plus [studio-096](../tracker/issues/studio-096.md), which is Plato-area).  
**Basis:** Dependencies and ROI, not tracker priority alone. Parallel tracks noted where safe.

Related:

- Lessons library triage (gates + packets): [plato-257-lessons-v1-recommendations-numbered.md](../submodules/Plato/docs/plato-257-lessons-v1-recommendations-numbered.md) (reviewer §A / item 475)
- Older consumer-driven ladder: [plato-refactoring-recommendations-2026-07-12.md](plato-refactoring-recommendations-2026-07-12.md)
- Backlog index: [tracker/BACKLOG.md](../tracker/BACKLOG.md)

---

## Finish what’s already moving

| # | Id | Title | Why now |
|---|---|---|---|
| 1 | [plato-263](../tracker/issues/plato-263.md) | Rename stdlib-legacy / stdlib to clearer stdlib names | Mid-flight — finish first so later path/diff work doesn’t churn twice |
| 2 | [plato-240](../tracker/issues/plato-240.md) | vscode-plato: Go to Definition + Find All References | One verification box left; unblocks editor UX and [plato-264](../tracker/issues/plato-264.md) |
| 3 | [plato-256](../tracker/issues/plato-256.md) | Navigation find-refs cross-root same-name pollution | Dual-root bug; worse until rename settles, then fix |
| 4 | [plato-272](../tracker/issues/plato-272.md) | Add Constants library to stdlib and use it | Ready p1 — cheap foundation win on the forward stdlib |
| 5 | [plato-023](../tracker/issues/plato-023.md) | TIR scalar lowering + type-checker completion | Compiler endgame — keep going **in parallel** with library work |
| 6 | [plato-229](../tracker/issues/plato-229.md) | Complete the Plato v2 concept lattice | Retarget or close: v2 largely superseded by [plato-230](../tracker/issues/plato-230.md) (done); don’t keep two concept missions |

---

## Decision gates (before more vocabulary inventing)

| # | Id | Title | Why now |
|---|---|---|---|
| 7 | [plato-268](../tracker/issues/plato-268.md) | Pick canonical type-class keyword: interface vs concept vs trait | One keyword for docs/errors |
| 8 | [plato-271](../tracker/issues/plato-271.md) | Decide Plato numeric precision and fixed-size types | Policy before BigInt / Decimal / Float8 / double ports |
| 9 | [plato-260](../tracker/issues/plato-260.md) | Settle Rotor3D vs Quaternion (one canonical rotation algebra) | Before Motor/Pose work |
| 10 | [plato-269](../tracker/issues/plato-269.md) | Differential: stdlib vs stdlib-legacy | After [plato-263](../tracker/issues/plato-263.md) — drives port order |
| 11 | [plato-270](../tracker/issues/plato-270.md) | Differential: Ara3D.Geometry vs Plato stdlib | After/with [plato-269](../tracker/issues/plato-269.md) |

---

## Agent/editor + small fixes

| # | Id | Title | Why now |
|---|---|---|---|
| 12 | [studio-096](../tracker/issues/studio-096.md) | `var` reassigned at two loop-nesting depths fails symbol resolution | Small resolver bug; cheap confidence |
| 13 | [plato-253](../tracker/issues/plato-253.md) | Diagnostics quality pass: spans, snippets, did-you-mean | Pays rent on every later library edit |
| 14 | [plato-264](../tracker/issues/plato-264.md) | VS Code hover docs for Plato definitions | After [plato-240](../tracker/issues/plato-240.md) |
| 15 | [plato-266](../tracker/issues/plato-266.md) | Inline external links in Plato stdlib docs | Feeds hover/browser |

---

## Stdlib shape / partiality (before big ports)

| # | Id | Title | Why now |
|---|---|---|---|
| 16 | [plato-079](../tracker/issues/plato-079.md) | Option/Result partiality cleanup of Plato stdlib | Matches sum types already shipped ([plato-232](../tracker/issues/plato-232.md)) |
| 17 | [plato-233](../tracker/issues/plato-233.md) | v3 kind-pattern sweep: remaining ~100 Kind types → enums | Shape cleanup before more APIs |
| 18 | [plato-241](../tracker/issues/plato-241.md) | Rename Vector2/3/4 → Vector2D/3D/4D (etc.) | If still needed post-v3 |
| 19 | [plato-242](../tracker/issues/plato-242.md) | Normal vector type (plus Unit Vector and Axis) | Foundation geometric types |
| 20 | [plato-243](../tracker/issues/plato-243.md) | Fraction type (unit interval 0..1) | Foundation numeric type |
| 21 | [plato-138](../tracker/issues/plato-138.md) / [plato-139](../tracker/issues/plato-139.md) | random.plato / OKLab in colors.plato | Only where v3 still gaps — check inventory first |

---

## Geometry kernel ladder (consumer-driven)

| # | Id | Title | Why now |
|---|---|---|---|
| 22 | [plato-255](../tracker/issues/plato-255.md) | Robust geometric predicates | Query/kernel foundation |
| 23 | [plato-254](../tracker/issues/plato-254.md) | Interval arithmetic | Feeds predicates / queries |
| 24 | [plato-028](../tracker/issues/plato-028.md) | Consumer-driven refactoring (Earcut, query vocab, topology) | Direction for ports |
| 25 | [plato-029](../tracker/issues/plato-029.md) | Port Geometry3Sharp pure query kernels to Plato | After query vocabulary exists |
| 26 | [plato-015](../tracker/issues/plato-015.md) | Better-performing geometry algorithms from Plato | After query vocab exists |

---

## PGA → motion → physics

| # | Id | Title | Why now |
|---|---|---|---|
| 27 | [plato-258](../tracker/issues/plato-258.md) | PGA meet/join incidence library | Collapse intersect-per-type-pair table |
| 28 | [plato-259](../tracker/issues/plato-259.md) | Motor as canonical for Pose3D | Needs [plato-258](../tracker/issues/plato-258.md) + [plato-260](../tracker/issues/plato-260.md) |
| 29 | [plato-248](../tracker/issues/plato-248.md) | Ensure spatial data structures are efficient | Broad-phase substrate |
| 30 | [plato-250](../tracker/issues/plato-250.md) | Basic numerical integration techniques | Physics prerequisite |
| 31 | [plato-249](../tracker/issues/plato-249.md) | Physics simulation (rigid and soft body) | Needs [plato-248](../tracker/issues/plato-248.md) + [plato-250](../tracker/issues/plato-250.md) |

---

## Docs surfaces (after content is worth browsing)

| # | Id | Title | Why now |
|---|---|---|---|
| 32 | [plato-265](../tracker/issues/plato-265.md) | HTML Plato library browser via Fable spike | After docs/links exist |
| 33 | [plato-267](../tracker/issues/plato-267.md) | Generate images for Plato types | After browser/lessons need them |

---

## Language / backends / aspirational (park until above is healthy)

| # | Id | Title | Note |
|---|---|---|---|
| 34 | [plato-235](../tracker/issues/plato-235.md) | GLSL overload erasure picks winner by emission order | Backend correctness problem |
| 35 | [plato-231](../tracker/issues/plato-231.md) | Type-level naturals (const generics) | Language feature |
| 36 | [plato-252](../tracker/issues/plato-252.md) | Automatic differentiation for Plato | Large compiler feature |
| 37 | [plato-024](../tracker/issues/plato-024.md) / [plato-078](../tracker/issues/plato-078.md) | RustWriter / TypeScript writer | Backend productionize |
| 38 | [plato-076](../tracker/issues/plato-076.md) | Port Gratify to Plato (feasibility) | Spike / park |
| 39 | [plato-134](../tracker/issues/plato-134.md) / [plato-218](../tracker/issues/plato-218.md) / [plato-226](../tracker/issues/plato-226.md) | Kernel libs / glyphs / Flow augmentation | Domain expansion |
| 40 | [plato-244](../tracker/issues/plato-244.md) … [plato-247](../tracker/issues/plato-247.md) | Rational / Decimal / BigInt / Float8 | After [plato-271](../tracker/issues/plato-271.md) |
| 41 | [plato-251](../tracker/issues/plato-251.md) | Simple LLM demo written in Plato | Demo / park |
| — | [plato-077](../tracker/issues/plato-077.md) | Sum types + pattern matching (RFC) | Likely close as superseded by [plato-232](../tracker/issues/plato-232.md) |

---

## Parallelism

Safe concurrent tracks:

- [plato-023](../tracker/issues/plato-023.md) (compiler)
- [plato-263](../tracker/issues/plato-263.md) → [plato-256](../tracker/issues/plato-256.md) → [plato-240](../tracker/issues/plato-240.md) (rename / nav)
- [plato-272](../tracker/issues/plato-272.md) (Constants)
- ADR trio [plato-268](../tracker/issues/plato-268.md) / [plato-271](../tracker/issues/plato-271.md) / [plato-260](../tracker/issues/plato-260.md) (docs/decisions only)

Do **not** parallelize two agents on the same `.plato` domain files.

---

## Not yet tracker items (process gates)

From the [lessons recommendations reviewer pass](../submodules/Plato/docs/plato-257-lessons-v1-recommendations-numbered.md):

1. Optional-return style (no generic `Optional<T>` — fallback / result record / concrete sum)
2. One `CONVENTIONS.md` (handedness, winding, alpha, radians, sentinels, …)
3. One epsilon / `AlmostEqual` policy
4. One angle-periodicity kit (`Normalize`, `Wrap`, `LerpShortest`, …)

These should land (or be filed) before assigning the §B foundation library packets.
