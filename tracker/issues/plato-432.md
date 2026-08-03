---
id: plato-432
title: Marching cubes evaluates the field eight times per cell: every cell re-reads its own corners
type: debt
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`MarchingCubesLattice` in `stdlib/geometry/voxels.library.plato` evaluates the
sampled field **`8 x (n-1)^3` times** for an `n`-node lattice, where `n^3` would
do: every cell reads its own eight corners, and each interior corner is shared by
eight cells, so almost every sample is computed eight times.

This was a deliberate, documented choice — plato-413 records it: *"Corner samples
are re-read per cut edge rather than cached per cube. Cut cubes are the surface
and grow as the square of resolution while the total grows as the cube, so the
common case is an uncut cube paying eight reads and allocating nothing; a
per-cube cache would allocate on every cube instead."*

That reasoning holds when a sample is a cheap array read, which is the case for
`DensityGrid3D` and `LevelSetGrid3D`. **It does not hold when the field is
procedural**, because then a "read" is a full field evaluation.

## The measurement

From the lattices demo (plato-421), extracting an octet-truss `StrutSdf3D` of 240
struts over the unit cube. `StrutSdf3D.Eval` is a union of capsules and is linear
in strut count with no acceleration structure, so each sample is genuinely
expensive:

| Nodes per axis | Direct `MarchingCubes` |
|---|---|
| 12 | 1.9 s |
| 20 | 9.9 s |
| 24 | **15.5 s** |

Unbudgeted, one build at the top of both sliders took **73 seconds** — a page
freeze a user can reach by dragging.

Sampling the field once onto `n^3` nodes and going through
`SampledSdf3D.MarchingCubes` produces **identical output for about an eighth of
the work**. That is what the demo does by default, with the direct member left as
a toggle so the difference is visible.

## Why this is worth fixing rather than documenting

The eightfold factor is invisible at the call site. A caller writes
`sdf.MarchingCubes(bounds, nodeCounts)`, which is the obvious spelling and the one
every doc comment shows, and gets eight times the work — while the
sample-then-march route, which is strictly better for procedural fields, requires
knowing to reach for `SampledSdf3D`. The library should not make the naive
spelling the slow one.

## Fix approaches

1. **Route the procedural entry points through a sampling pass.** The
   `IScalarField3D` / `ISignedDistanceField3D` overloads sample onto an
   `n^3` grid and then call the grid path. Costs one grid allocation, saves
   roughly 7/8 of the evaluations. This is what the demo does by hand and is
   almost certainly the right answer.
2. **Keep the re-read for grid inputs.** plato-413's argument is sound where a
   sample is an array read; do not regress that path chasing a uniform rule.
3. Not recommended: a per-cube corner cache. plato-413 already rejected it, and
   the objection (allocating on every cube, including the majority that are
   uncut) still stands.

## Done means

- [ ] Marching a procedural field costs about one evaluation per node, not eight
- [ ] The grid-input path is unchanged
- [ ] The doc comment says what a caller pays, for each input kind
