---
id: plato-435
title: Compare Plato stdlib with external numerical and geometry libraries
type: debt
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

- [ ] The report explains its comparison method and the limits of the evidence.
- [ ] It compares Plato with representative numerical, fixed-size math, computational
      geometry, mesh-processing, and planar-geometry libraries using primary sources.
- [ ] It identifies strengths, material gaps, and design differences with consequences
      for Plato's intended consumers.
- [ ] It gives a prioritized set of recommendations that respects Plato's portability
      and purity constraints.
- [ ] The dated report is linked from the documentation index.

## Verification

- Check all repository-local links in the report.
- Check all external citations resolve to official documentation or project sources.
- Review the report against `docs/documentation-conventions.md`.
