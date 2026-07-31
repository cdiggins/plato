---
id: stdlib-377
title: Move animation tracks + skeletal animation to stdlib/future; exclude future from codegen and lint by default
type: debt
status: in-progress
priority: p2
effort: M
risk: low
area: stdlib
sprint: 
created: 2026-07-31
closed:
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

- [ ] Animation-track and skeletal-animation declaration files live under `stdlib/future`
- [ ] `TimeVarying` concept + bodies remain in `graphics` and no shipping-tier file references a
      `future` declaration
- [ ] `lint` and C# codegen skip `future` by default and include it behind an explicit flag
- [ ] `future` still parses and type-checks (all four tiers, 0 diagnostics)
- [ ] Lint ratchet ceiling lowered to the newly measured value in the same commit
- [ ] `stdlib/README.md`, `stdlib/LIBRARIES.md` and `AGENTS.md` describe the new default
