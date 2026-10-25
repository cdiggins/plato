---
id: stdlib-464
title: Stdlib laws unguarded against generated inputs fail in both backends (seven under cpu64, ten under gpu32)
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
`Law_Superformula3DPolesAreOnTheAxis` (sampling differs). None is a backend fault: each law is not
guarded against the generated inputs the runner draws, so it is evaluated where the function's
precondition does not hold.

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
Integer: the same lists. Each guard is a precondition on the law (`halfExtents > 0 ? ... : true`)
or a tolerance scaled to the profile's `RoundingUnit`.

## Done means

- [ ] Each of the eight laws is guarded by its function's precondition, and the three seam laws
      use a tolerance that holds under gpu32; the conformance runner and the interpreter report
      no failure from them under either profile.

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
