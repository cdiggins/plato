---
id: plato-076
title: Port Gratify to Plato (feasibility: kernel vs framework)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-19
closed:
links: [submodules/gratify, submodules/gratify/src/gratify/part.ts, submodules/Plato/plato-src/fields.plato, submodules/Plato/docs/plato-overview.md, submodules/Plato/docs/affine-types.md, submodules/Plato/Plato.TypeScriptWriter, tracker/issues/studio-074.md, docs/peacockv2-guide.md]
---

## Idea

Port Gratify — the TypeScript UI framework in `submodules/gratify` (~2.3K lines
core + `core/` math) — to Plato. Interpretation: this is a *feasibility* idea,
and the feasibility analysis splits Gratify into two halves that fare opposite
ways. The **pure kernel** (core/ vec/rect/color/curve/spring math, layout
pack/measure/arrange functions, easing, channel/spring step functions, style
resolution as `tokens × channels → values`) is squarely Plato's sweet spot —
pure functions over small value types. The **framework shell** (typestate
builder, retained heterogeneous scene tree, reconciler, intent routing, canvas
Painter, event loop) is built from exactly the constructs Plato is *defined by
excluding*. A whole-framework port is a category error; a kernel port is the
founding Plato pitch (one source → C# and TS) and would let the studio-074
C# port and TS Gratify share one kernel.

## What Plato already has (verified in source)

- **Function-valued fields** — `plato-src/fields.plato` stores
  `Function1<Vector3, Number>` in a type field (`ScalarField3D.Function`);
  `Function0..N` are types in `primitives.plato:74`. So facet-records
  (a part spec as a record of functions) are expressible in principle.
- **Lambdas as arguments** — `array.plato`, `core.library.plato` pass
  `(a, b) => …` freely; `geometry.interfaces.plato` `Deform` takes
  `Function1<Point3D, Point3D>`.
- **String type** — `primitives.plato:25` (keys, part names) — exists, though
  the library has near-zero string operations.
- **Affine builder types** — `docs/affine-types.md`: `unique type List<T>/Buffer<T>`,
  runtime-checked mutate-then-freeze. A seed of a controlled-mutation story.
- **TypeScript writer** — `Plato.TypeScriptWriter/` exists with a TIR body
  writer (~320 lines), currently declared out of scope in CLAUDE.md.

## Clearly missing from Plato (would need adding)

1. **Sum types + pattern matching** — Gratify's intent/`Intentish` vocabulary,
   interactor kinds, and reducers are discriminated unions. Plato has none
   (already called the weakest part of the type story in plato-overview.md).
   This is the top *addable* gap and would pay off beyond UI (Option/Result
   for `Invert`/ray-miss partiality).
2. **String-keyed maps** — `Channels = Record<string, number>`, `Tokens`,
   theme extension tables. No dictionary type in Plato; kernel workaround is
   index-based channel arrays with hostside name interning.
3. **General mutable/reference state** — retained scene nodes mutated per
   frame, `local` state, memoized measure cache. Affine List/Buffer is
   append-and-freeze only. Springs/channels dodge this (pure
   `Step(s, dt): Spring`), the retained tree does not.
4. **Heterogeneous collections / existentials** — `Element[]` holds parts with
   erased prop types (`PartDef<unknown, unknown>`); children of one node have
   different `P`. Monomorphized zero-dispatch structs *cannot* express this —
   it is anti-Plato by design, not an addable feature. The scene tree must
   stay in the host language.
5. **Type-level ergonomics** — the typestate builder (capability unions,
   `Exclude`/`Pick`), `infer`-conditional types, mapped `Defaulted<P, D>`.
   Unportable to Plato (and to C# — studio-074 says the same). Any port keeps
   the semantics, loses the guided-chain DX.
6. **Host-event/effect story** — pointer/keyboard events, rAF loop, canvas.
   The intrinsics FFI pattern (declaration-only library, host bodies) fits a
   `Painter` interface cleanly, but *inbound* events into a pure language have
   no precedent in Plato; host shell owns the loop and calls pure Plato
   functions.

## Riskiest / hardest part

**Scope creep from kernel to framework.** The single biggest risk is reading
"port Gratify" as "the whole framework": the reconciler + heterogeneous
retained tree + event loop sit on mutation, existentials, and dynamic dispatch
— the exact constructs whose *absence* makes Plato's multi-target promise
credible. Chasing them would damage the language to serve one library.
Hardest *legitimate* technical part: the layout engine's `MeasureCtx`
(memoized child-measure callbacks — a function-valued interface with caching
semantics) and delegate-heavy code meeting the monomorphizer/beta-reduction
pipeline, where delegate inlining is foundation-shipped but
application-deferred (property/method duality blocker, see
beta-reduction memory/plan doc).

## Assumptions

- Plato's TS writer can be revived to production quality for the kernel subset
  (it is currently explicitly out of scope).
- Delegate-typed fields (`Function1<>` in types) survive the TIR/monomorphize
  pipeline for nontrivial cases — fields.plato is the only stdlib precedent;
  needs a spike to confirm it isn't a special case.
- Gratify stays the reference implementation; Plato kernel is extracted
  beneath it, not a rewrite of it.
- studio-074 (C# port) proceeds — the shared-kernel play is what makes this
  idea more than an exercise.

## Design decisions

- **Kernel-only vs full port** — kernel-only (recommended): pure math + layout
  + springs + style resolution in Plato, scene/reconciler/events stay
  TS/C#-hostside. Full port requires sum types, maps, mutation, existentials —
  three language features and one impossibility.
- **Which gaps to close in Plato** — sum types + pattern matching only
  (general-purpose payoff) vs also string-maps (UI-serving, weaker general
  case) vs none (encode intents as tagged Integer + fields hostside).
- **TS writer revival** — prerequisite or deferred? Kernel-in-Plato emitting
  only C# still serves studio-074/PeacockV2; emitting TS too is what pays
  Gratify back.
- **Where the kernel lives** — new `gratify.plato` library in plato-src vs a
  separate library folder; interacts with the plato-src content-leads process.

## Related

- [studio-074](studio-074.md) — sibling: Gratify→C# port. This idea is the
  "one source, both targets" alternative/complement: a Plato kernel would feed
  studio-074's C# side and Gratify's TS side from one file.
- [docs/peacockv2-guide.md] — the C# framework design both ideas serve.
- [submodules/Plato/plato-src/fields.plato] — existence proof for
  function-valued fields.
- [submodules/Plato/docs/plato-overview.md] — §"Bad at" already names the
  sum-type/partiality gap this idea would force.
- [gratify-055](gratify-055.md) — open Gratify work (local state/dropdowns);
  local-state semantics are hostside under the kernel split, so no conflict.

## Approaches

Short term:
1. **Spike: springs + easing in Plato** — port `core/spring.ts` + `curve.ts`
   (~100 lines, zero dependencies, pure step functions) to a `motion.plato`
   library; emit C# and TS; swap into Gratify behind its existing API. Proves
   the TS writer + delegate-field pipeline on the smallest real slice.
2. **Layout kernel** — port the `pack` algebra (sizes × avail → offsets) as
   pure functions; hostside keeps MeasureCtx memoization.
3. **Sum-types RFC for Plato** — write the language addition Gratify would
   need (also serves Option/Result); decide independently of this port.

Long term: `gratify.plato` as the shared kernel under both the TS framework
and the C#/PeacockV2 port — one reviewed source for math, layout, motion, and
style resolution; framework shells per host. Plato gains sum types and a
revived TS target with a real second consumer.

Adjacent ideas worth their own issue (all filed 2026-07-19):
- [plato-077](plato-077.md) — sum types + pattern matching in Plato (RFC first).
- [plato-078](plato-078.md) — revive/productionize the TypeScript writer.
- [plato-079](plato-079.md) — Option/Result partiality cleanup of stdlib
  (blocked on plato-077).

## Simplest possible implementation

The spike from Approach 1: one `motion.plato` file (springs, exponential
approach, decay/impulse, easing curves) compiled to both targets, wired into
Gratify's channel stepper and (when studio-074 starts) the C# port.
- Get: hard evidence on the three real risks (TS writer health, delegate
  fields through TIR, ergonomics of Plato for non-geometry math); immediate
  dedup of the most-copied code between the two ports; no Gratify API change.
- Give up / risk: proves nothing about layout callbacks or style records (the
  hard middle); two-writer maintenance burden lands on Plato CI; if the TS
  writer needs deep surgery the spike balloons into compiler work before any
  UI value appears.

## Note — 2026-07-21: surface-idiom evaluation + content-library tiering

Evaluated (against Gratify as-built post-0.1.0) whether Gratify should adopt
C#/Plato-portable surface idioms now. Answer: no — the kernel/shell split
above stands; what doesn't port in the shell is TS type veneer or host-side by
nature, and restyling it buys the Plato route nothing. Full findings in the
studio-074 note (same date). Two Gratify hygiene changes recommended there
serve this idea directly: (1) pure layout math into core/ with a purity rule —
makes kernel extraction mechanical translation; (2) de-globalize theme state.

**Content libraries in Plato above the kernel** (controls/effects/interactions
/adornments authored once, emitted to both hosts, each host running its own
framework shell) — feasibility tiers per facet:

- **Style recipes + style facets** — best fit: pure `tokens × channels →
  record`. Needs a Channels accessor story (opaque host type + `Get(name)`
  intrinsic or generated accessors) and free-function `mix` (recipes currently
  call the function-valued `Tokens.mix` field).
- **Layout pack functions + motion (channel specs, easing, springs)** — pure;
  ready modulo the spike risks above.
- **Render facets** — NOT portable as-is: imperative void calls on Painter
  (`push`/`alpha`/`glow(callback)`) clash with Plato purity. Portable form =
  render-as-data (facet returns a draw-command list; host rasterizes). This is
  the one genuinely valuable portability-motivated Gratify reshape found — but
  a real API change, decide on its own merits.
- **Body/adorn (structure)** — plausible via opaque `Element` handle type +
  declared per-part FFI constructors (heterogeneity erased behind the handle,
  so the anti-Plato existential problem is dodged — Plato constructs trees, the
  host reconciles them). Unproven; needs its own spike.
- **Reduce/intents/interactors (behavior)** — blocked on sum types
  (plato-077) for tolerable authoring; tagged-Integer encoding possible but
  ugly. Interactor values could be built via FFI constructors
  (function-valued fields have the fields.plato precedent).

Practical shape: Plato authors facet *functions*; each host binds them into a
part with a few lines of builder glue (or generated bindings). "Write `Select`
once in Plato" needs render-as-data + the structure spike + plato-077; a
style/motion/layout/effects library needs none of those.
