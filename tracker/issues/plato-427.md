---
id: plato-427
title: "Finite elements: nodal stress smoothing, hexahedral and higher-order elements"
type: feature
status: ready
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [stdlib/future/finite-elements.library.plato, tracker/issues/plato-424.md]
---

## What and why

`plato-424` landed the linear-elastic analysis vocabulary in
`stdlib/future/finite-elements.*`: the constant-strain tetrahedron, the linear
plane triangle, coordinate-list assembly, a projected Jacobi-preconditioned
conjugate gradient, and the Euler-Bernoulli beam path. Four extensions were
deliberately left out of that issue so it stayed shippable. Each is a small,
self-contained addition to the same library; none of them changes the assembly
or the solver.

1. **Nodal stress smoothing.** Stress from a constant-strain element is constant
   per element, so a stress plot coloured from `VonMisesStresses` is faceted —
   one flat colour per tetrahedron. Every FEA viewer instead averages the
   incident element values at each node (area- or volume-weighted) and
   interpolates across the element. This is the single biggest visual
   improvement available to a results demo and needs only an incidence pass over
   the cells.

2. **The hexahedral element.** `HexahedralMesh3D` / `HexahedronCell`
   (`stdlib/geometry/meshes.types.plato`) still have no producer and no consumer.
   A trilinear hexahedron needs 2x2x2 Gauss integration, so unlike the
   tetrahedron its shape-function gradients are NOT constant — it cannot reuse
   `ConstantGradientStiffnessEntries` and needs a quadrature-point loop
   alongside it.

3. **Higher-order elements.** The 10-node tetrahedron and the 6-node triangle.
   The constant-strain tetrahedron is notoriously stiff: a coarse mesh of them
   under-predicts deflection badly, and a demo that shows a converging
   deflection against mesh refinement wants the quadratic element to converge
   to.

4. **Edge traction in the plane, and axial / torsional beam degrees of freedom.**
   `ElasticModel2D` carries nodal forces and gravity but no distributed edge
   load, and the beam model carries deflection and rotation only — so it cannot
   see axial force, buckling, or torsion, and `Pinned` and `Roller` are
   indistinguishable in it.

## Design decisions

_(fill in)_

## Done means

- [ ] Nodal (smoothed) von Mises stress alongside the per-element reading
- [ ] Trilinear hexahedral element with Gauss quadrature, over `HexahedralMesh3D`
- [ ] Quadratic tetrahedron and triangle
- [ ] Edge traction for `ElasticModel2D`
- [ ] Axial and torsional degrees of freedom on the beam path
- [ ] All four tiers parse and type-check
