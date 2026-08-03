---
id: plato-414
title: SdfBendModifier3D doc says it bends about the Y axis; the code rotates about Z
type: bug
status: idea
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: [stdlib/geometry/implicit-sdf.types.plato, stdlib/geometry/implicit-sdf.library.plato]
---

## Problem

Two doc comments name the wrong rotation axis for the bend modifier.

- `SdfBendModifier3D` (`implicit-sdf.types.plato`): "Bends a spatial shape about the Y
  axis with the given curvature (turning angle per unit of length along X)".
- `ApplyToDomain(SdfBendModifier3D)` (`implicit-sdf.library.plato`): "The unbent position
  for a shape bent about the Y axis".

The body is `RotatedAboutZ(point, (self.Curvature * point.X).Angle)`, and `RotatedAboutZ`
turns the X and Y components while leaving Z alone. An angle accumulating with X, applied
as a rotation in the XY plane, bends about **Z**, not Y. This matches IQ's `opCheapBend`,
which is a 2x2 rotation of `p.xy`.

Cost: a reader building a bent shape aims it along the wrong axis and gets a result
rotated 90 degrees from what the comment promised. Nothing computes the wrong answer —
only the description is wrong.

## Approaches

Fix the two comments to say "about the Z axis" (or "in the XY plane", which is how the
`RotatedAboutZ` helper reads). Confirm against `RotatedAboutZ` rather than against the
comment being replaced. Consider whether `Curvature`'s doc should also name the plane the
bend happens in, since "turning angle per unit of length along X" is correct as written
and is the part a caller actually sets.

## Found while

Documenting distance fidelity for plato-411. Out of scope there — that change was
confined to what an SDF's *value* means, and this is a claim about geometry — so it was
filed rather than fixed inline.

## Done means

- [ ] Both comments name the axis the body actually rotates about
- [ ] `.\tools\check-stdlib-fast.ps1` green
