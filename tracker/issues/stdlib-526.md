---
id: stdlib-526
title: Audit source-documented standard-library algorithm limitations
type: debt
status: idea
priority: p2
effort: L
risk: med
area: stdlib
sprint: 
created: 2026-10-25
closed:
links: [docs/reference/library/README.md]
---

The docs-523 source audit found algorithm limitations which must remain visible to callers.
These are reported by source comments/body inspection, not fresh numerical reproductions.

- CatmullRomCurve2D/3D exposes Alpha but Eval uses only the uniform case; callers selecting
  centripetal/chordal parameterization can get the wrong curve. Implement nonuniform tangents
  and boundary cases, or make the restricted surface explicit. Source: splines.library.plato.
- NaturalCubicSpline1D has no Eval implementation, so its type/accessors do not make it a
  usable evaluator. Implement the coefficient solve and piecewise evaluation separately.
- SweptSurface/TubeSurface use a fixed-up frame, while their type docs suggest a
  rotation-minimizing frame; callers can see twist or degeneracy. Decide and implement the
  frame contract. Source: surfaces.library.plato.
- OffsetSurface normals use central differences; a header claiming exactness overstates
  numerical behavior. Specify step/error behavior and correct that claim.
- Polygon-soup CSG is documented as unreliable for coplanar shared faces. Robust predicates
  and coplanar classification are substantial algorithm work. Source: solids-csg.library.plato.
- BVH Raycast reports broad-phase box entry with synthetic surface data; treating it as a
  true primitive intersection gives false hits/normals. Separate candidate traversal from
  narrow-phase intersection. Source: spatial-structures.library.plato.

This capture groups discovered limits for triage, not an instruction to fix all in one change.
Split accepted fixes into reproductions and independent implementation issues before execution.
