---
id: compiler-499
title: Repair inlining emission failures on plato-brep
type: bug
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
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

- [ ] Reproduce each diagnostic family with a focused regression and repair its cause.
- [ ] Full B-rep generation with `--inline --loops --fuse` compiles without file fallback.
- [ ] Mesh digests, face diagnostics, and contract checks match the non-inlined baseline.
