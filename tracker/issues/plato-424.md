---
id: plato-424
title: Finite element analysis vocabulary: elements, assembly, and a linear-elastic solve
type: feature
status: in-progress
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`stdlib/future/engineering.types.plato` names `Beam`, `BeamSupport`, `Material`,
`Stiffness` and the quantity vocabulary, and `stdlib/geometry` has
`TetrahedralMesh3D` / `TetrahedronCell` — but there is no analysis: nothing forms an
element stiffness matrix, assembles a global system, applies boundary conditions or
solves. Finite element analysis is the clearest demonstration that a pure geometry
language reaches past geometry, and it is the natural consumer of the tetrahedral
mesh type nobody currently produces or reads.

Scope: **`stdlib/future`** — the aspirational tier. It is not linted and not
converted to C#, but it **must parse and type-check** (`ForwardStdLib*` in
`tests/PlatoTests` reads all four tiers), and nothing in a shipping tier may
reference it. That is the lower bar that makes this issue tractable; do not let it
become an excuse for sloppy vocabulary.

Subject matter, roughly:

- **Material model**: linear isotropic elasticity — Young's modulus, Poisson ratio,
  density, and the constitutive matrix derived from them. Reuse
  `engineering-materials.library.plato` if it already names the constants.
- **Elements**: start with the constant-strain tetrahedron (CST/T4) and the linear
  triangle for 2D plane stress / plane strain. Element stiffness from the shape
  function gradients and the element volume.
- **Assembly**: local-to-global degree-of-freedom mapping over a
  `TetrahedralMesh3D`, into a sparse or dense global stiffness matrix. Deciding the
  matrix representation in a pure language is the real design work — say what you
  chose.
- **Boundary conditions and loads**: fixed / pinned / roller supports, nodal forces,
  body forces (gravity), and traction on a boundary face.
- **Solve**: conjugate gradient (matrix-free is legitimate and may be the better fit
  here) or dense Gaussian elimination for small systems. Then the derived readings —
  displacement per node, element strain, element stress, and von Mises stress, which
  is what a demo colours by.
- **A 1D/beam path** as the cheap, checkable case: Euler-Bernoulli beam elements
  over the existing `Beam` / `BeamSupport` types, whose answers can be checked
  against closed-form cantilever and simply-supported deflections.

## Design decisions

_(fill in — matrix representation, solver choice, element family, and what you rejected)_

## Done means

- [ ] Linear-elastic material and its constitutive matrix
- [ ] Element stiffness for the linear tetrahedron and the linear triangle
- [ ] Global assembly over a `TetrahedralMesh3D` with a stated matrix representation
- [ ] Supports, nodal forces and gravity as boundary conditions
- [ ] A solve producing per-node displacement and per-element von Mises stress
- [ ] A beam path whose answers can be checked against closed-form deflection
- [ ] All four tiers parse and type-check; no shipping-tier file references `future`
- [ ] Design decisions recorded above
