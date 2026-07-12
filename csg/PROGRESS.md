# CSG.plato port — progress

DONE (2026-07-12). Fold-based re-expression of evanw/csg.js; no BSP tree.

- csg.plato: types Facet/Solid; Fragment (Sutherland-Hodgman half-space clips
  folded over the other solid's planes) + Inside (ray-parity) + Union/Intersect/
  Subtract. Compiles under earcut recipe.
- Ara3D.Csg.Tests: 13/13 volume-invariant tests pass (boxes: overlap, disjoint,
  contained, rotated-45 intersect). 1 Explicit test documents the coplanar
  shared-face limitation (returns 14.667 vs 16).
- regen: .\regen-csg.ps1 -Test  (from anywhere; merges plato-src + csg.plato).

Open: coplanar shared faces unresolved (see FINDINGS.md §compromise); fast BSP
pass is future work.
