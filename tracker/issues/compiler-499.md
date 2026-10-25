---
id: compiler-499
title: Repair inlining emission failures on plato-brep
type: bug
status: done
priority: p1
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: []
---

## Problem

Enabling `--inline` alongside the shipping `--loops --fuse` recipe prevents the
plato-brep generated library from compiling. This blocks measuring and deploying
source inlining on the production mesher. The default inliner, without any
experimental budget changes, fails; larger budgets are not the cause.

Reproduction and measurements: compiler-498, `tools/experiments/brep-inlining.py`,
Plato 763034a67612fb1ada95891434cba8fc2ecf5707, plato-brep
99c8fdee6be8d3fc92f45401321d442e15375316. Baseline without `--inline` compiles.
The inline output passes the existing throwing-stub gate but has 1,862 distinct
C# diagnostics across 51 generated files. Working full log:
`.temp/csharp-build-logs/inline-brep-default-host.log`.

Representative diagnostics (the codes and messages are the durable evidence):

- CS1955, `BrepGeometryLaws.g.cs:306`: Non-invocable member 'NumberInterval.End' cannot be used like a method.
- CS0030, `BisectionCached.g.cs:446`: Cannot convert type 'Ara3D.StepBrep.Boolean' to 'Ara3D.StepBrep.Integer'.
- CS1061, `_RawDecodedModel.g.cs:441`: 'VertexIndex' does not contain a definition for 'Orientation'.
- CS1501, `Curves.g.cs:2671`: No overload for method 'Distance' takes 0 arguments.
- CS1503, `_UvRegion.g.cs:123`: Argument 3: cannot convert from 'target-typed conditional expression' to 'Ara3D.StepBrep.VertexIndex'.

This is an emitter/type-preservation repair of medium estimated scope, requiring
minimal regressions for each failure family and full B-rep compilation. Do not
raise diagnostic ceilings or treat partial experiment builds as a production fix.

## Done means

- [x] Reproduce each diagnostic family with a focused regression and repair its cause.
- [x] Full B-rep generation with `--inline --loops --fuse` compiles without file fallback.
- [x] Mesh digests, face diagnostics, and contract checks match the non-inlined baseline.


## Repair milestone

Constructor recovery and loop lowering now preserve resolved result types. The C#
emitter groups conditional array receivers and conditional typed-index cast operands.
The focused fixture reproduces the underlying field-call and conditional-precedence
failures: baseline passed before repair, both inline modes failed; all three now
compile and execute correctly. Nine focused emission/inliner tests pass. Full B-rep
compilation and meshing equivalence remain outstanding. Greenhouse evidence and prose
review are refreshed; the documentation check and six maintenance tests pass.

The requested rerun also measures component expansion combined with fusion and loops,
with pass-stage evidence to test which downstream transformations inlining enables.


## Full-build milestone

Pinned repair 8515ca59 builds all six configurations without file fallback: no/current/
aggressive source inlining, each with fusion+loops and with fusion+components+loops.
The same throwing-stub gate passes every configuration. Stage counters and a Roslyn
syntax utility record downstream transformations separately from expansion totals.
A common isolated host adds complete meshing elapsed time (compiler-502) and passes
an ODIN smoke digest/timer check. All common host/dependency DLLs are frozen across
variants. Full PlatoTests passes 364 cases; Studio integration retains only repo-473.
Meshing equivalence and repeated measurements across all variants remain outstanding.

## Completed measurements

All 54 primary samples across six policies and all 18 performance-core replication
samples match baseline digests, face/edge diagnostics, boundary counts and contract
tables. No generated files use fallback. Results and reproduction commands are in
`docs/reports/brep-full-inlining-2026-10-24.md`, with raw samples, hashes, selected
stage counts and refusal categories in the adjacent data JSON. Inlining enables
fusion and lambda removal, but larger expression-inlining budgets do not yield a
net speed benefit here. Statement-body and lambda restrictions remain in force;
this is not unrestricted inlining. Allocation attribution remains compiler-500,
the harness timing gap compiler-502, and forward component coverage compiler-503.
All C# validation described above passes apart from the existing external repo-473
integration failure. Native C++/CUDA checks were omitted at the user's request.
