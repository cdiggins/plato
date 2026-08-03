---
id: plato-430
title: Sampling library: measured behaviour contradicts three doc claims, and RelaxedPoints2D is 13x slower than its own loop
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [plato-422]
---

## What and why

`plato-422` landed the sampling generators; the `demos/webgl/sampling.ts` page
(`plato-422`'s demo box) is the first thing that ever **executed** them, and it
measured four things the library source states differently. Three are doc claims
that measurement contradicts; one is a performance defect in a shipped
`stdlib/geometry` member.

None of these makes a generator wrong. The point sets are correct and the quality
readings rank the families exactly as the theory predicts — that part checked out.
These are the places where the source says something a reader would rely on and
the number disagrees.

## The four findings

**1. `RelaxedPoints2D` is 13x slower than driving `RepelledPoints2D` per pass,
for a bit-identical answer.** This is the serious one. The folded member reads its
own lazy `Append` chains, so an n-step fold pays compounding traversal to read one
element. At n = 256 over 8 passes: **1092 ms folded against 83 ms** with
materialisation between passes. The two agree to 0.0 in every coordinate, so this
is pure cost. It is the same eager-fold trap that `demos/webgl/README.md` warns a
demo author about — except here it is biting a shipping library member rather than
a demo's `tick`, which means the warning belongs in the library too, and probably
the fix belongs in the member.

**2. The Poisson sweep's directional bias is not "faint".**
`sampling.library.plato` describes the scan-order bias from grid dart-throwing as
faint, and along the scan axis it is. Across it, it is not. At count 160 the
background grid is 24x24, so the sweep period sits at ~23.4 cycles; `WavePower`
there reads **27.6 along Y against 3.7 along X**, versus ~2.1 off the frequency —
roughly a 13x spike across the scan direction. Bridson was rejected for good
reasons (plato-422 records them) and this is the accepted cost, but the source
should describe the anisotropy honestly, because a caller choosing blue noise for
sampling will care which axis it is on.

**3. Thinning costs blue noise 3-4x of its low-frequency suppression.**
`BlueNoisePattern2D` reaches its exact count by generating a maximal Poisson-disk
set and thinning it evenly. plato-422 predicted the direction (thinning keeps the
minimum-distance property and loses maximality); the magnitude was not known.
Maximal Poisson reads bands 1-8 of the radial spectrum at 0.08-0.14; the thinned
result reads the same bands at 0.17-0.51.

**4. `PlasticPattern2D`'s doc comment claims a lower discrepancy in the plane than
Halton, and measures worse.** Under `StarDiscrepancyEstimate` at n = 140 it ranks
8th of 10 and loses to Halton. Two possible causes and this issue does not settle
which: the estimator is a lower-bound approximation and may not be resolving the
difference, or the claim is wrong. Worth an hour with a reference implementation
before editing either.

**Not a defect, but worth a doc line:** band 0 of `RadialPowerSpectrum` measures
the window rather than the pattern — it reads 0.9-3.5 for every family including a
regular grid, because one cycle across a square is a fractional period in most
averaged directions. The demo scene says so; the library does not.

## Simplest fix

Findings 2, 3 and the band-0 note are doc-comment edits in
`stdlib/geometry/sampling.library.plato`, stating what was measured rather than
what was expected. Finding 4 needs a check against a reference before the comment
is either corrected or kept. Finding 1 is the only one that touches a body.

## Done means

- [ ] `RelaxedPoints2D` either materializes between passes or its doc comment
      tells callers to drive `RepelledPoints2D` themselves, with the cost stated
- [ ] The Poisson doc comment describes the bias as directional and says which
      axis, instead of "faint"
- [ ] The `BlueNoisePattern2D` doc comment states what thinning costs
- [ ] `PlasticPattern2D`'s discrepancy claim is checked against a reference and
      then either corrected or kept with the estimator's limitation noted
- [ ] `RadialPowerSpectrum` says band 0 measures the window
