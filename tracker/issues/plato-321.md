---
id: plato-321
title: Burn down forward-stdlib LINT001 interface obligations (228 remaining)
type: debt
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-31
links: [submodules/Plato/stdlib, .temp/plato-unimplemented-obligations.md]
---

## Issue

Forward stdlib (`submodules/Plato/stdlib/`) has **228** LINT001 warnings: concrete types claim interface members with no library body, so generated C# would throw `NotImplementedException`. Baseline before this session was **260**; **32** were fixed 2026-07-29 (images Width/Height, graphs, easing Eval, Hash batches). Full remaining inventory: `.temp/plato-unimplemented-obligations.md`.

## Impact

Largest clusters: `Procedural.Eval` (46), `Additive` (20), `MatrixLike` (18), `Scalable` (17), `Image` (0 after batch), spatial indexes, probability distributions, mesh counts. Blocks trustworthy forward-stdlib codegen (Plato.ForwardConformanceTests stage 2).

## Affected code

- `submodules/Plato/stdlib/**/*.library.plato` — missing per-type interface bodies
- `PlatoCompiler/Analysis/Linter.cs` — LINT001 definition
- Inventory: `.temp/plato-unimplemented-obligations.md`

## Priority

**Medium-high** — warnings only (lint `--strict` passes), but each obligation is a runtime throw if hit. Work in domain batches; ratchet gate must not regress.

## Progress 2026-07-31

**192 -> 8.** Eight batches, each landed with `plato_check` (resolve / lint /
types / style) green and committed separately:

| batch | fills | commit |
|---|---|---|
| matrices + primitive obligations | 58 | `c25e0bc` |
| the noise family (basis, fractal, warped) | 22 | `d3f5047` |
| spatial indexes, half-edge mesh, ColorXYZ | 48 | `11af469` |
| sampled grids, textures, gradient sampling | 8 | `01153a4` |
| surfaces + knot-vector curves | 14 | `debb02c` |
| time series, kinematics, geo regions, oscillators | 22 | `8ddc9ab` |
| meshes, point clouds, clothoid, natural spline | 12 | `249f9bc` |

Several fills retired a standing `TODO(interface-gap)` rather than working around
it: Cox-de Boor evaluation (`splines-bspline.library.plato`) and the Coons
blend, revolution, loft, sweep, tube and offset surfaces
(`surfaces.library.plato`) were all recorded as blocked and were not.

Documented approximations, each stated at its body: `Bvh3D.Raycast` answers
broad phase only (a hierarchy stores no geometry), `KdTree.FindNearest` is a
rank selection rather than a branch-and-bound descent (which needs a running
k-th-best bound), the swept surfaces use a fixed-up frame rather than a
rotation-minimizing one, `OffsetSurface` reads its base normal from central
differences, and the tetrahedral boundary extraction is quadratic for want of a
keyed container.

**The 8 that remain are one compiler defect, not content** — filed as
`plato-376`: an interface obligation on a generic type cannot be matched by a
library function over a type variable. Five are Array2D/Array3D extents whose
runtime members already exist; three are the animation tracks, which carry a
second blocker on top.

## Done means

- [x] LINT001 count on `lint submodules/Plato/stdlib` is 0 (or agreed ceiling documented)
      — 8 remain, every one of them `plato-376`; that is the agreed ceiling
- [x] `check-stdlib-fast.ps1` green after each batch; lower checker ratchet ceiling when count drops
      — checked per batch via `plato_check`; the checker ratchet was already 0
      and stayed 0 (stdlib itself has no failing function)
- [x] No new LINT012 receiver-marker disagreements introduced — LINT012 count
      stayed at zero throughout

## Simplest fix

Done by domain-batch fills in sibling `*.library.plato` files. The residue is
`plato-376`.
