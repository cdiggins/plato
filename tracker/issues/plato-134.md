---
id: plato-134
title: Plato kernel libraries: motion, effects, style, layout
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-21
closed:
links: [docs/design/plato-kernel-libraries-sketch-2026-07-21.md, tracker/issues/plato-076.md, tracker/issues/plato-077.md, tracker/issues/plato-078.md, tracker/issues/studio-074.md, submodules/Plato/plato-src/curves.plato, submodules/Plato/plato-src/colors.plato, submodules/Plato/plato-src/core.interfaces.plato, submodules/Plato/plato-src/interval.plato, submodules/Plato/plato-src/bounds.plato, submodules/gratify/src/gratify/core]
---

## Idea

A family of small, pure Plato libraries — motion (springs, easing, timelines),
effects (particles, procedural juice), style (color arithmetic, palettes,
gradients), layout (rect/point packing) — defined over time, unit intervals, and
interpolatable values rather than over widgets. Authored once, emitted to both
TypeScript and C#; Gratify (TS today, C# via studio-074) becomes one consumer
among several (Studio heat-maps, camera moves, gizmo/L2 animation). This is the
concrete content plan for the plato-076 "kernel" — carved to be useful and
reusable WITHOUT being Gratify-specific. Full type/interface sketches in
[docs/design/plato-kernel-libraries-sketch-2026-07-21.md].

Boundary rule: Plato takes anything expressible as "values in, values out, no
names as strings." Channels-by-name, parts/facets, Element trees, event routing
stay host-side.

## Assumptions

- Stdlib interfaces (`IInterpolatable`, `IScalarArithmetic`, `IAdditive`) let
  scalar cores lift to Vector2/3, Color, Angle — the whole 2D/3D generality
  story rides on this existing seam.
- A palette is a fixed-field record, so themes/cross-fades are Plato-expressible
  (Gratify's Tokens problem was the stored `mix` function + string lookup, not
  the palette).
- Generic `Spring<T>` and function-valued `Keyframe.Ease` survive the
  TIR/monomorphizer pipeline (same spike plato-076 calls for; fields.plato is
  the only precedent).
- TS writer revival (plato-078) happens — it gates all TS-side payoff; C#-only
  still serves studio-074.

## Design decisions

- **Alignment as Number vs enum** — encode align/justify as 0..1 Number
  (0=start, 0.5=center, 1=end): dodges the sum-type gap (plato-077), strictly
  richer than the TS union. Recommended.
- **Spring genericity** — `Spring<T>` generic vs Number-only core with
  per-component lifting hostside. Generic is nicer; spike decides.
- **Where OKLab/hash live** — grow colors.plato / new random.plato vs bundling
  into the new libraries. Stdlib placement serves non-UI consumers.
- **File layout** — motion/effects/style/layout as new plato-src files vs
  folding into curves/colors/bounds; interacts with plato-src content-leads
  process.

## Related

- [plato-076](plato-076.md) — parent: kernel-vs-framework split; this issue is
  the kernel's content plan, superset of its "springs + easing" spike.
- [plato-077](plato-077.md) — sum types; NOT needed here (the align-as-Number
  and ease-as-function-value moves dodge it) — that independence is a feature.
- [plato-078](plato-078.md) — TS writer revival; prerequisite for TS payoff.
- [studio-074](studio-074.md) — C# Gratify port; would start with its math
  layer already written.
- [submodules/gratify/src/gratify/core] — the TS code this would replace
  (spring, curve, vec, rect, color) plus containers.ts `packRows`, style.ts
  recipe formulas, theme.ts fade.

## Case against

- plato-076 already covers the strategic ground; this could be scope inflation
  of an unproven idea — the motion spike should succeed BEFORE a four-library
  plan earns effort. Cost of doing nothing: zero until that spike runs.
- Two-writer maintenance lands on Plato CI for libraries whose TS/C# hand
  ports total maybe 400 lines — the dedup payoff is modest in raw lines; the
  real payoff (one reviewed source, N consumers) only materializes if Studio
  actually consumes them beyond Gratify.
- OKLab + noise are new content, not transcription — small research scope.

Verdict: **park** — right content plan, wrong moment to execute; run the
plato-076 motion spike first, then promote this with evidence.

## Approaches

Short term:
1. Fold into the plato-076 spike: `motion.plato` (Spring/Approach/Decay/easing)
   emitted to C# + TS, swapped under Gratify's channel stepper. Smallest slice,
   proves the pipeline.
2. `style.plato` second — Palette + Gradients + Emphasis; immediately consumable
   by Studio heat-maps even if Gratify integration waits.
3. Layout packing third (`PackRows`/`PackAxis`/`RectOps`) — pure, but its
   payoff depends on Gratify container rewiring.

Long term: the four libraries become the shared kernel under TS Gratify and the
C#/PeacockV2 port; Plato gains its first non-geometry stdlib wing and a real
second TS-writer customer.

Adjacent ideas worth their own issue:
- random.plato — deterministic hash/noise stdlib (biggest true gap; serves
  procedurals.plato too).
- OKLab color space in colors.plato.

## Simplest possible implementation

`motion.plato` alone (≈60 lines: Spring, Step, Approach, Decay, SmoothStep,
InOutCubic), emitted to both targets, wired under Gratify's channel stepper —
identical to plato-076's Approach 1, now with the follow-on map drawn.
- Get: pipeline proof (TS writer, delegate fields, non-geometry ergonomics);
  the most-copied math deduped; go/no-go evidence for the other three
  libraries.
- Give up / risk: proves nothing about palettes-as-records or packing; if the
  TS writer needs surgery, compiler work precedes any UI value (inherited from
  plato-076).
