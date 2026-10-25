---
id: stdlib-464
title: Stdlib conformance failures: establish domains and repair contracts
type: bug
status: in-progress
priority: "?"
effort: "?"
risk: "?"
area: stdlib
sprint: 
created: 2026-10-18
closed:
links: []
---

## Problem

The forward conformance runner (the stdlib tiers plus `stdlib/tests`, through the C# backend)
fails seven laws under cpu64 and ten under gpu32, and the interpreter fails the same seven plus
`Law_Superformula3DPolesAreOnTheAxis` (sampling differs). These are historical observations, not proof that each input violates a
precondition. Agreement between backends can also expose a shared implementation
defect. Establish each function's documented domain and actual failing arguments
before choosing a repair; do not weaken a valid law to obtain a passing verdict.

- `ImplicitSdfLaws.Law_BoxIsBoxSlice`: negative half extents.
- `Law_VerticalCapsuleIsCapsuleAlongY`: negative height and radius.
- `Law_EqualRadiusCappedConeIsCylinder`: negative half height.
- `Law_ConeIsDegenerateCappedCone`.
- `SurfacesShapesLaws.Law_HyperboloidOfOneSheetSatisfiesItsEquation` and
  `Law_HyperboloidOfTwoSheetsSatisfiesItsEquation`: a reversed interval.
- `Law_SuperellipsoidPolesAreOnTheAxis` (and `Law_Superformula3DPolesAreOnTheAxis` in the
  interpreter): negative exponents.
- gpu32 only: `Law_CatenoidSeamsInU`, `Law_HyperboloidOfOneSheetSeamsInU`,
  `Law_HyperboloidOfTwoSheetsSeamsInU`, tolerance edges of binary32.

Measured 2026-10-18 for TKT-315 (bim-open-toolkit), Plato 78be1798, before and after the 64-bit
Integer: the same lists. Precondition guards and profile-scaled tolerances were proposed at that time,
but remain unverified for the outstanding laws. The current capsule diagnosis
establishes a valid signed-height segment and repairs its implementation while
preserving the original law and tolerance.

## Done means

- [ ] Establish each listed law's actual input domain and repair violated implementation
      contracts; justify any necessary property/tolerance change against that domain.
      The conformance runner and interpreter report no failures under either profile.
- [x] Vertical capsule supports signed height; the unchanged original law and six independent
      distance regressions pass on CSharp and differential under cpu64 and gpu32.
- [ ] Independently diagnose and verify every other listed law under both profiles.

The original aggregate criterion prescribed guards for all eight laws and changed
seam tolerances. It was never completed; the capsule counterexample disproves that
prescribed remedy, while the required both-profile correctness outcome is retained.

## Current capsule diagnosis (2026-10-25)

The current cpu64 failure is reproduced against fc4b5 in plato-brep commit
12487045, with exact argument bits and unchanged inputs. Negative finite height
reverses the specialized Clamp bounds while the point lies inside the documented
origin-to-(0,height,0) segment. Negative radius is not necessary for this defect.
The shipping capsule function has no positive-height precondition. For this law,
repair endpoint ordering and retain the original property; the historical blanket
guard diagnosis above does not establish the documented domain. Independent fixed
distance regressions are staged; no production fix or acceptance is claimed yet.
The other listed laws and gpu32 remain unverified. Do not tick the combined
acceptance box based on the capsule repair alone.

## Verified capsule repair (2026-10-25)

Reorder the clamped Y interval only for negative height. Positive and zero height
retain their original branch, and the radius subtraction is unchanged. Six fixed
laws cover negative endpoints, interior radial and exterior distances, positive
and zero height, and the existing negative-radius offset behavior. The original
sampled property and tolerance are byte-unchanged.

Each baseline engine/profile preserved two passes and five logical failures;
each candidate passed all seven selected laws (25 sampled cases in the original,
one exhaustive case in each fixed law), with zero differential mismatches and
zero registered witnesses. Native runtime, full inputs and outputs are hash-bound.
CPU64 receipt: 792521024ec538d85832ae7e5a3029aa45971e1d366f757c1e4a29128d863b10.
GPU32 receipt: b6317fd97571d2323e3d2366f9bb93e579385ba37cd4223b29df9788dfecb626.
Plato-brep staging/maprange-consumer/inventory/campaign/capsule/fix preserves
the source, harness and results. No whole-stdlib sweep or other listed-law success
is claimed; this issue remains in progress.
