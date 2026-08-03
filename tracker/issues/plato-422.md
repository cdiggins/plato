---
id: plato-422
title: Blue noise and low-discrepancy sampling: implement the declared point-pattern types
type: feature
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed: 2026-08-03
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

- **No second random source.** Every seeded generator draws from the integer
  hashing already in `noise.library.plato` — `LatticeHash` / `NoiseMix` /
  `HashToUnit` / `FeatureOffset2D` / `FeatureOffset3D`. No state, no stream, no
  new constants. One helper is added on top, `SampleUnit(seed, index, stream)`,
  which is `HashToUnit(LatticeHash(seed, index, stream))` and exists only to name
  the "seed, what is being drawn for, which of several draws" triple.
- **What a `Seed` guarantees**, stated in the library under
  *Determinism: what a Seed promises*: a Seed names a function, not a position.
  The dart at cell (i, j) on attempt a is a pure function of (Seed, i, j, a); the
  k-th sample of stratum s of (Seed, s, k). So one pattern record yields the same
  points in the same order on every backend (the mixing is wrapping whole-number
  arithmetic, not floating point), and for the closed-form families a caller may
  evaluate one point without evaluating the others. It reproduces **within one
  version of the library, not across versions**: the mixing constants belong to
  `noise.library.plato` and are explicitly not part of the contract.
- **Poisson-disk is grid dart-throwing, not Bridson.** Cells are radius/sqrt(2)
  across, so a cell holds at most one sample and its accepted point IS its
  occupancy record; a dart is compared only against the 5x5 cell block around it.
  Cells are visited in scan order and only the EARLIER half of each block is
  consulted, so every pair is tested exactly once. Bridson's active-list front was
  rejected because it needs a grid written to as it fills plus an active list with
  random removal — both need an affine builder, and `List`/`Buffer` have no
  runtime on the TypeScript backend the demos use. The cost of the choice is a
  faint directional bias from the sweep, which the radial spectrum reading shows.
- **Empty cells are a sentinel point, not a validity record.** A grid row is a
  plain `Array<Point2D>`; an empty slot holds a point four radii outside the
  region's minimum corner, which no in-region point can be within `radius` of.
  CONVENTIONS.md's three sanctioned partial-operation styles govern API surface;
  this is a private encoding inside one section and never escapes it.
- **No `List` / `Buffer` anywhere in this file**, so every entry point emits to
  every backend. The price is paid in constant factors: the sequential generators
  build their output with `Append`, whose immutable chain costs O(depth) per read,
  so the Poisson sweep compacts each grid ROW as it finishes rather than the whole
  grid at the end — that keeps every chain about a row long instead of about a
  point set long.
- **Lloyd relaxation rejected; repulsion relaxation shipped.** Discretized Lloyd
  needs the stone-to-point assignment STORED — computed once per stone, read once
  per point — or the pass degrades from stones x points to stones x points^2.
  Storing it needs an affine builder, see above. Repulsion needs no table.
- **Best-candidate (Mitchell) dropped.** Same output character as the grid
  generator, strictly worse cost here (every dart measured against every accepted
  point, through an append chain), and nothing it offers that the Poisson route
  does not.
- **`BlueNoisePattern2D` routes to Poisson-disk**, as the brief asked. It declares
  a Count and Poisson-disk takes a radius, so the two are joined by the random
  sequential adsorption saturation density (about 0.7 * area / radius^2 points):
  generation picks the radius that saturates at a fifth more than Count and then
  thins evenly back to exactly Count. Thinning keeps the minimum-distance property
  and loses maximality, which is visible at the low-frequency end of the spectrum.
- **New types are the missing vocabulary only**: `PlasticPattern2D/3D` (the R2/R3
  sequence the brief named), and the 3D counterparts `JitteredGridPattern3D`,
  `StratifiedPattern3D`, `HaltonPattern3D`. `SamplePattern` gains a `Plastic` case.
  **No `sampling.concepts.plato`**: an interface with no concrete implementer and
  library functions dispatching on it is an immediate LINT013, and the pattern
  records share no obligation beyond "has points" that `IPointSet2D` does not
  already name.
- **No 3D Poisson-disk or 3D blue noise.** The planar sweep gets away with a
  two-row sliding window; in three dimensions the window is two whole planes of
  occupancy that would have to be carried and randomly indexed, which is exactly
  the affine-builder problem again. `PlasticPattern3D` (R3) is the even spatial
  scatter to reach for meanwhile, and the omission is stated in both files.
- **The generalized golden ratio is computed, not tabulated.**
  `GeneralizedGoldenRatio(d)` Newton-solves x^(d+1) = x + 1, so R2 and R3 come
  from one definition and the identity that defines them is what a reader sees.

## Done means

- [x] Every declared `*Pattern2D` type in `sampling.types.plato` generates points
- [x] Poisson-disk via a background grid, not O(n^2) rejection
- [x] A low-discrepancy family including R2, with per-axis bases honoured
- [x] 3D counterparts for the patterns the demos need
- [x] At least one quality reading (nearest-neighbour distance, discrepancy or spectrum)
- [x] Same seed, same points — stated where a reader of the bodies will find it
- [x] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green — `lint --strict` PASS and
      the checker ratchet PASS. Two earlier runs were red on
      `stdlib/future/rigid-dynamics.library.plato` and
      `stdlib/geometry/remeshing.library.plato`, both sibling tracks mid-edit in
      this shared tree; those cleared. `sampling.types.plato` and
      `sampling.library.plato` contribute zero diagnostics across parse, resolve,
      types, sums, style and lint.
- [x] Design decisions recorded above
- [x] `stdlib/types-and-concepts.txt` regenerated — five new types and one sum case
      make it stale, and index freshness is the third gate in `check-stdlib-fast.ps1`
- [x] A browser demo drives it: `demos/webgl/sampling.html` + `src/demos/sampling.ts`,
      green under `npm run typecheck` and `npm run scenes`

## Not done

- The bodies are not EXECUTED by anything: the forward conformance law runner does
  not compile its generated code (`plato-308`), same standing gap `plato-413`
  recorded. Correctness rests on review of the bodies, not on a run.
- `stdlib/types-and-concepts.txt` is stale with respect to the five new types and
  the new `SamplePattern` case; the coordinating session regenerates it once after
  all six tracks land.
