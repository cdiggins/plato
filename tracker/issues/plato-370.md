---
id: plato-370
title: Double-precision scalars (approach must be re-designed — TirScalarLowerer is gone)
type: feature
status: idea
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-31
closed:
links: [plato-023, decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md, docs/archive/plato-execution-plan-2026-07-09.md]
---

Split from [plato-023](plato-023.md): plan step **S4** only.

> **Stale as written (2026-08-01).** This issue assumed `--scalar=double` would ride
> `TirScalarLowerer` as a second `scalarMap`. Scalar erasure and that pass were retired
> ([decision](../decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)),
> so there is no lowerer to parameterize and no `--scalar` flag to extend. The GOAL — double
> precision for BIM / large-coordinate / robust-predicate work — is still wanted; the approach
> has to be redesigned around wrapper scalars (e.g. `Number` backed by `double`, or a
> precision-parameterized wrapper). Effort raised from M to L accordingly. Do not start from
> the plan below without re-deriving it.

## Issue

Double precision is not available. The former route to it (an erasure map) no longer exists.

## Impact

BIM / large-coordinate / robust-predicate workloads want double without a second writer.
Until this exists, double is a parallel handwritten stack or out of reach.

## Affected code

- `writers/Plato.CSharpWriter/TirScalarLowerer.cs` — `FloatMap` only; docs mention double map.
- `writers/Plato.CSharpWriter/CSharpWriter.cs` — `--scalar=` flag plumbing.
- Intrinsics: need a double counterpart to the float V2 surface (see execution plan D.2).
- Plan: `docs/archive/plato-tir-scalar-lowering-plan-2026-07-12.md` S4; `docs/archive/plato-execution-plan-2026-07-09.md` D.2.

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
