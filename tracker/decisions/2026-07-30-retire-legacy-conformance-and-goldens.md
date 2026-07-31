---
date: 2026-07-30
title: Retire the legacy conformance suite and the golden diff-gates
status: accepted
superseded-by:
links: [../issues/plato-308.md, ../issues/plato-363.md, ../issues/plato-362.md]
---

## Context

Two aging safety nets had become process tax. (1) The golden diff-gate: committed copies of the
compiler's generated C# (`Generated/Plato.Generated.Unoptimized` + `Optimized`), byte-compared by
`tools/regen-generated.ps1`, with a "refresh in the same commit" rule that kept being violated
(plato-363 found the gate red and the legacy suite unbuildable from a prior commit's unrefreshed
writer changes). (2) The legacy conformance suite (`conformance/Ara3D.SDK.ConformanceTests`),
which ran ~205 algebraic laws against the OLD standard library (`stdlib-legacy`) — a library that
is no longer the direction of travel; the forward stdlib (`stdlib/`) is.

What actually ships (Ara 3D Studio) consumes the FROZEN V1 artifacts in ara3d-sdk, protected by
the cheap `tools/check-frozen-v1.ps1` checksum tripwire — independent of both retired nets.

## Decision

- Delete the golden diff-gate: `tools/regen-generated.ps1` removed, gate removed from
  `check-all.ps1`. The `Generated/` projects remain as ordinary buildable cached output (the
  optimizer-smoke Bench references them); staleness is acceptable, hand-editing still is not.
- Delete the legacy conformance suite: `conformance/Ara3D.SDK.ConformanceTests` and
  `tools/regen-conformance.ps1` removed; gate removed from `check-all.ps1`.
- Keep `check-frozen-v1.ps1` (protects shipping artifacts; one checksum, zero maintenance).
- Keep `stdlib-legacy/` sources and their lint gate (source hygiene, and the frozen V1's origin).
- plato-363 closed as no-longer-relevant (both broken things it reported are now deleted).
- plato-308 (make the FORWARD conformance suite compile and run) is promoted to p1: it is the
  replacement for all retired coverage.

## Rationale

The golden discipline caught nothing the build itself didn't (both regenerated projects compiled
clean even while "differing"), and its same-commit-refresh rule was routinely missed, producing
red gates that cost investigation time. The legacy suite tested yesterday's library; every hour
spent keeping it green is an hour not spent making the forward suite runnable.

## Alternatives rejected

- Fix plato-363 and keep both nets — keeps paying the tax on a library being replaced.
- Keep the legacy suite until the forward suite is green — honest-looking, but the suite was
  already broken and repairing it buys coverage of the wrong library.

## Consequences

- Until plato-308 lands, NO test executes generated Plato code. Interim coverage: forward
  Stage-1 type-check + checker ratchet, lint gates, PlatoTests, GeometryTests, frozen-V1
  tripwire, and the forward build-error count trend.
- Historical docs still reference the retired scripts; they are history, not instructions.
