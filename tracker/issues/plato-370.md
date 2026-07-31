---
id: plato-370
title: Implement --scalar=double via TirScalarLowerer map + double intrinsics
type: feature
status: idea
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-31
closed:
links: [plato-023, docs/plato-tir-scalar-lowering-plan-2026-07-12.md, writers/Plato.CSharpWriter/TirScalarLowerer.cs, docs/plato-execution-plan-2026-07-09.md]
---

Split from [plato-023](plato-023.md): plan step **S4** only. Float scalar lowering already ships.

## Issue

`--scalar=float` erasure runs as `TirScalarLowerer` with `FloatMap`. The plan’s S4 goal —
`--scalar=double` as a second map plus double intrinsics and cross-precision conformance —
was never landed. The lowerer is already parameterized by `scalarMap`; only float is wired.

## Impact

BIM / large-coordinate / robust-predicate workloads want double without a second writer.
Until this exists, double is a parallel handwritten stack or out of reach.

## Affected code

- `writers/Plato.CSharpWriter/TirScalarLowerer.cs` — `FloatMap` only; docs mention double map.
- `writers/Plato.CSharpWriter/CSharpWriter.cs` — `--scalar=` flag plumbing.
- Intrinsics: need a double counterpart to the float V2 surface (see execution plan D.2).
- Plan: `docs/plato-tir-scalar-lowering-plan-2026-07-12.md` S4; `docs/plato-execution-plan-2026-07-09.md` D.2.

## Assumptions / open questions

- Namespace / assembly for double generated geometry (`Ara3D.Geometry.Double` vs flag-only).
- Whether double intrinsics live beside V2 or as a separate package.
- How large a float/double differential conformance set is enough.

## Priority

p2 — approved useful goal, not blocking forward-stdlib compile or the float shipping path.

## Dependencies

- Blocked by: double intrinsics + any runtime surface decisions.
- Blocks: large-coordinate geometry tests that need double emit.
- Touches: C# writer, intrinsics, generated projects — keep clear of plato-308 body work.

## Approaches

1. Add `DoubleMap` + wire `--scalar=double`; emit against double intrinsics; small differential tests.
2. Full parallel generated project + conformance suite (heavier, matches older roadmap wording).

## Simplest implementation

(1): map + intrinsics + one generated recipe that compiles, plus a seeded float/double
differential. Defer a second full golden tree until something consumes it.

## Done means

- [ ] `--scalar=double` selects a `DoubleMap` through `TirScalarLowerer` (no second lowerer).
- [ ] Double intrinsics exist for the scalar surface the map targets.
- [ ] A double emit recipe compiles on net8.0 default LangVersion.
- [ ] Seeded float/double differential tests exist and pass for an agreed kernel set.
