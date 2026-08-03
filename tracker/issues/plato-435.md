---
id: plato-435
title: Compare Plato stdlib with external numerical and geometry libraries
type: debt
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed: 2026-08-03
links: [docs/reports/plato-stdlib-comparative-study-2026-08-03.md]
---

## Problem

The forward standard library has grown across numerics, geometry, fields, meshes,
graphics, and simulation, but the repository has no source-backed comparison with
established libraries in other languages. Without that comparison, roadmap choices
can be driven by local vocabulary breadth rather than by the capabilities, numerical
policies, topology models, and portability tradeoffs that mature ecosystems expose.

## Scope

Write a dated report that compares the shipping tiers of `stdlib/` with a deliberate
cross-section of numerical and geometry libraries. Treat Plato's pure, multi-target
code-generation model as a design constraint, distinguish declared vocabulary from
verified executable support, and derive recommendations rather than proposing a
symbol-for-symbol port.

## Done means

- [x] The report explains its comparison method and the limits of the evidence.
- [x] It compares Plato with representative numerical, fixed-size math, computational
      geometry, mesh-processing, and planar-geometry libraries using primary sources.
- [x] It identifies strengths, material gaps, and design differences with consequences
      for Plato's intended consumers.
- [x] It gives a prioritized set of recommendations that respects Plato's portability
      and purity constraints.
- [x] The dated report is linked from the documentation index.

## Verification

- Repository-local links checked from the report's directory on 2026-08-03.
- External citations opened from official documentation or project sources on
  2026-08-03.
- Report reviewed against `docs/documentation-conventions.md`; its dated filename and
  `docs/reports/` location identify it as a frozen assessment rather than authority.
