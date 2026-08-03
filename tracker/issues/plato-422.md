---
id: plato-422
title: Blue noise and low-discrepancy sampling: implement the declared point-pattern types
type: feature
status: in-progress
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

`stdlib/geometry/sampling.types.plato` **declares** the point-pattern types and
`sampling.library.plato` implements none of them: `BlueNoisePattern2D`,
`PoissonDiskPattern2D`, `JitteredGridPattern2D`, `StratifiedPattern2D`,
`HaltonPattern2D`, `SobolPattern2D` are fields with no generator. That is one of the
four gaps catalogued in plato-420 — this issue closes the sampling one.

Scope: **`stdlib/geometry`** (shipping tier — lint strict, checker ratchet, index
freshness). Prefer implementing the declared types over inventing new ones; add
types only where the vocabulary is genuinely missing (a 3D counterpart, a spectral
reading).

Subject matter, roughly:

- **Poisson-disk**: Bridson's dart-throwing over a background grid, seeded and
  deterministic. This is the workhorse the blue-noise type should route to unless
  there is a reason not to.
- **Best-candidate (Mitchell)** as the simpler, allocation-light alternative, and
  the relaxation (Lloyd) that turns any pattern into a more even one.
- **Low-discrepancy**: Halton (already declared with per-axis bases), Sobol, and the
  R2 / golden-ratio plastic-constant sequence, which is a handful of lines and is
  the one people actually reach for.
- **Stratified and jittered-grid**, which are the cheap baselines the others are
  judged against.
- **3D counterparts** where the type list stops at 2D, since the lattice, voxel and
  volume demos all want them.
- **Readings that let a demo argue the point**: nearest-neighbour distance
  distribution, discrepancy, and a radial spectrum, so "blue" is a measurement and
  not an adjective.

Determinism matters more than speed here: every type carries a `Seed`, and the same
seed must give the same points on every target.

## Design decisions

_(fill in — RNG choice and why, rejection strategy, what a `Seed` guarantees)_

## Done means

- [ ] Every declared `*Pattern2D` type in `sampling.types.plato` generates points
- [ ] Poisson-disk via a background grid, not O(n^2) rejection
- [ ] A low-discrepancy family including R2, with per-axis bases honoured
- [ ] 3D counterparts for the patterns the demos need
- [ ] At least one quality reading (nearest-neighbour distance, discrepancy or spectrum)
- [ ] Same seed, same points — stated where a reader of the bodies will find it
- [ ] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green
- [ ] Design decisions recorded above
