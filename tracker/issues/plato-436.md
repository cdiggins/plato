---
id: plato-436
title: TypeScript writer: Arr is a lazy view with no memoization, so library members are super-linear in their own iteration counts
type: bug
status: ready
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`Arr` in the emitted TypeScript preamble is a **lazy view with no memoization**:
`Map`, `MapRange` and `Zip` return an object that recomputes its function on
every `At(i)`. Nothing caches. So a library member written as a fold — the
natural spelling in a pure language, and the one the Plato sources use
everywhere — costs polynomially or exponentially more than its own arithmetic,
depending on how many elements of the previous layer each step reads.

This is the single most consequential defect the six demo pages found. **It has
now been hit five times independently**, in five different libraries, by five
agents who did not know about each other's findings:

| Where | Measured |
|---|---|
| `RelaxedPoints2D` (sampling, plato-430) | 1092 ms against 83 ms for the same answer, driving the loop externally |
| `SolveConstrained` (finite elements) | 60-DOF bar at iteration caps 10/20/40/80: 71 / 336 / 2437 / 6439 ms — cubic in the iteration count. A 1224-DOF cantilever did not finish in ten minutes |
| `StepBallScene` (rigid bodies, plato-434) | reading one `Center`: 0.87 ms at frame 0, 63.5 at 15, 1984 at 20, **75 003 at frame 25** |
| `LaplacianSmoothed` (remeshing) | 32 triangles, iterations 3/4/5/6: 272 / 1187 / **11 441** / **50 149** ms. The same ten iterations issued one at a time with the result materialized cost **93 ms total** |
| `StepMassSpring` (cloth) | a 10x10 sheet at two substeps did not complete 120 frames in ten minutes; **9 ms/frame** with one `MakeArray` per frame |

`IsotropicRemeshPass` is the same hazard one level up — it chains four passes
with no materialization between them: **21 s on 18 triangles, 279 s on 32**,
against **190 ms on 32** for the same four members called in the same order with
the result read into flat arrays between them.

## Why this is worse than a performance note

**It is not a slow path, it is a hang, and it is silent.** No throw, no NaN, no
warning — the page simply stops. Nothing in the type system, the linter or any
gate can see it, and the C# target has no such chain, so it is invisible to
everything except running the generated TypeScript.

It also changes what the libraries' own cost documentation means. `TopologyOf`
is documented as quadratic in the corner count; in the emitted TypeScript it is
effectively **cubic**, because its `rank` array is a prefix count over `naming`,
which reads `twins`, which recomputes `TwinCorner`'s linear scan on every read.
Materializing `CornerEdges` costs 31 ms at 32 triangles, 106 at 50 and **2279 at
128**. The remeshing page's real interactive ceiling is therefore about **130
triangles**, against the ~1000 the library's stated complexity implies.

## Fix approaches

1. **Memoize `Arr`.** A view caches each element on first read. Fixes every case
   above at once, and costs memory proportional to what is actually read. Almost
   certainly the right answer.
2. **Emit eager arrays** for members whose result is read more than once. Needs
   the writer to know which those are, which it does not.
3. **Materialize at fold boundaries in the writer** — wherever a `Reduce`
   accumulator is itself an array, force it. Narrower than (1) and catches the
   simulation cases, which are the worst ones.

The demo prelude works around it with an `eager` helper applied case by case
(`ReplacedAt`, the cloth `With*` rebuilds, the finite-element `System*` vector
operations, `StepParticles`), and every demo page carries its own materialization
helper. That is five workarounds for one defect, which is the argument for
fixing it once.

## Done means

- [x] An `Arr` element is computed at most once per view
- [x] The five cases above run in time proportional to their arithmetic
- [x] The demo prelude's `eager` workarounds and the pages' materialization
      helpers can be deleted, and are
