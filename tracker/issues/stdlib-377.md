---
id: stdlib-377
title: Move animation tracks + skeletal animation to stdlib/future; exclude future from codegen and lint by default
type: debt
status: done
priority: p2
effort: M
risk: low
area: stdlib
sprint: 
created: 2026-07-31
closed: 2026-07-31
links: [plato-376]
---

## Context

The keyframe/track and skeletal-animation vocabulary in `stdlib/graphics` is aspirational: it
has no bodies, no runtime counterpart, and three of the eight remaining LINT001 findings
(`plato-376`) come from `AnimationTrack`/`TangentTrack`/`NamedTrack` obligations that cannot be
discharged from the library side at all. It belongs in `stdlib/future`, the tier reserved for
vocabulary that is declared but not yet shipped.

Once it is there, `future` as a whole should stop being converted to C# and stop being linted:
both gates measure content that nobody is trying to make shippable yet, and the noise hides real
regressions in the three shipping tiers. It must still PARSE and TYPE-CHECK — that is the
property that keeps the aspirational vocabulary honest.

## Decisions

- `TimeVarying<TValue>` stays in `graphics` (as `time-varying.concepts.plato` /
  `time-varying.library.plato`): `Tween` and `Oscillator` in `motion-graphics*.types.plato`
  implement it, and nothing in a shipping tier may reach into `future`.
- Default tier list for lint and codegen is `foundation, geometry, graphics`. `future` is added
  back by an explicit opt-in flag (`-IncludeFuture` in the PowerShell gates,
  `--include-future` in `tools/record-gates.py`).
- Type-checking keeps enumerating all four tiers, recursively: `ForwardStdLibParsesAndCompiles`
  and `ForwardStdLibDiagnosticCountDoesNotRegress` are unchanged.

## Done means

- [x] Animation-track and skeletal-animation declaration files live under `stdlib/future`
- [x] `TimeVarying` interface + bodies remain in `graphics` and no shipping-tier file references a
      `future` declaration
- [x] `lint` and C# codegen skip `future` by default and include it behind an explicit flag
- [x] `future` still parses and type-checks (all four tiers, 0 diagnostics)
- [x] Lint ratchet ceiling lowered to the newly measured value in the same commit (44 -> 38)
- [x] `stdlib/README.md`, `stdlib/LIBRARIES.md` and `AGENTS.md` describe the new default

## Result

Measured 2026-07-31 (`python tools/record-gates.py --full --dry-run`):

| gate | result |
|---|---|
| Plato.CLI build (Release) | PASS |
| lint --strict (three shipping tiers) | PASS — 0 error / 38 warning / 1486 info, ratchet 38 |
| PlatoTests (both ratchets) | PASS — 197 passed / 0 failed |
| forward-stdlib codegen (full recipe) | PASS — 1322 .g.cs, 31 degraded bodies |

Re-measured later the same evening, after the `Generated/` cleanup below: lint ratchet 33,
PlatoTests 200/200, codegen PASS at 1052 .g.cs (the drop is plato-378's intrinsics pruning
landing in the same tree, not this change), and the stale-file CS0246 cluster is gone.
`forward conformance (build + law runner)` is RED for an unrelated in-flight reason — 12 x
CS0102 in `src/Plato.Intrinsics/Integer.cs` and `Number.cs`, where `Zero`/`One` are now defined
both by hand and by generated constants. That belongs to plato-378's primitives split.

`TimeRemap.Track: AnimationTrack<Number>` became `TimeRemap.Curve: TimeVarying<Number>`: it was
the one shipping-tier reference into the moved vocabulary, and the interface is what it always
meant. The existential is object-safe, so `ForwardStdLibHasNoViewlessExistentialReferences`
(CHK308) stays green.

Two defects surfaced and were fixed in passing, since both blocked verifying this change:

- `tools/check-stdlib-fast.ps1` and `tools/stage-stdlib.ps1` still used pre-reorg project paths
  (`Plato.CLI\...` rather than `src\Plato.CLI\...`), so the inner-loop gate could not run at all.
  This is the Plato-side twin of `plato-372`.
- `tools/record-gates.py` never emptied the conformance `Generated/` folder, so `.g.cs` files
  from a previous, wider generation kept compiling into the suite. Dropping a tier made that
  visible as CS0246 on `GeoRegion<>` / `TimeSampled<>`; the same trap would fire on any rename.
