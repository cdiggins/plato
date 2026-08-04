---
id: plato-440
title: "Sum-typed parameters have no TypeScript surface: LaplacianSmoothed, TaubinSmoothed and the Decimated family are uncallable"
type: bug
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439]
---

## Problem

`CHK320` — "sum types are C#-only in v1" — makes the TypeScript writer skip the sum
type itself, but it does **not** skip the functions that take one as a parameter.
Those are emitted with a parameter whose type name is undefined in the output. The
generated file carries `@ts-nocheck`, so this compiles and only fails at the call
site, where a consumer has no value to pass.

Found from the consumer seat while rebuilding `demos/typescript/geometry-samples` on
the stdlib (plato-439). `TriangleMesh3D.LaplacianSmoothed(weighting, strength,
iterations)` is the shortest path to smoothing a mesh and it cannot be called at all;
the sample composes `UniformLaplacianField` plus an explicit step instead.

The number of sum types skipped in one generated file is the `CHK320` comment count in
`demos/typescript/geometry-samples/src/plato/plato.g.ts`. `LaplacianWeighting`,
`CsgOperation`, `Axis3D`, `Containment`, `SubdivisionScheme` and `NoiseBasis` are the
ones blocking the most reachable API — CSG, the axis helpers, spatial-query
containment results, subdivision surfaces and the whole `FbmNoise2D` / `Turbulence`
family are unreachable from a non-C# backend for this reason.

## Options

1. **Emit a non-generic sum as a tagged union.** Each case becomes a class carrying
   the `IsCase()` predicates the bodies already call
   (`weighting.IsUniformWeights()`), and the sum becomes their union. Bodies lower to
   conditionals with no new TIR node
   (`docs/design/plato-sum-types-design-2026-07-27.md`), so nothing else changes.
2. **Emit a case enum plus predicate helpers** — smaller surface, same call sites.
3. **Skip functions whose parameter type was skipped**, and say so in a comment.
   Honest, but strictly worse for the consumer: the API silently shrinks.

Option 1 is closest to what the C# side means, and the only one that leaves generated
TypeScript able to express what the library expresses.

## Done means

- [ ] A non-generic sum type reachable from a shipping tier is callable from generated
      TypeScript, `LaplacianSmoothed` included.
- [ ] The geometry-samples half-edge sample calls `LaplacianSmoothed` instead of
      composing the step by hand.
- [ ] Writer behaviour covered by a test in `writers/Plato.TypeScriptWriter` or
      `tests/`.
