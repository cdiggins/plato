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

**Files.** `stdlib/future/finite-elements.types.plato` and
`finite-elements.library.plato` (block `FiniteElements`), rather than `fea.*` —
the style guide bans abbreviations and the spelled-out name costs nothing.
`LameParameters` was added to `engineering.types.plato` beside
`EngineeringMaterial`, and its conversions to `engineering-materials.library.plato`,
because they are material vocabulary and not analysis vocabulary. **No
`.concepts.plato`**: no interface earned its place. An `IFiniteElementModel`
obligation was considered and rejected — the beam cannot satisfy it (it needs an
element count that `Beam` does not carry), which would leave a two-implementer
interface over `ElasticModel3D` and `ElasticModel2D` whose only shared operations
are already plain overloads on those two types.

**Matrix representation: coordinate-list `SparseMatrix`** (the foundation type,
`matrices.types.plato`). Its stated invariant is that the value at a position is
the SUM of the entries naming it, so concatenating entry lists is matrix
addition. Assembly is therefore a `FlatMap` over the elements and nothing else:
no scatter, no accumulation, no ordering requirement, no mutable global matrix,
and no hash map from (row, column) to a slot. That is the property that makes
assembly tractable in a pure language, and it is the reason for the choice.

Rejected: a dense `MatrixN` — quadratic memory in the degree-of-freedom count,
and Gaussian elimination on it needs row swapping, which in a pure language means
rebuilding the matrix per pivot. Rejected: CSR — it buys random access this code
never uses, at the cost of a sort or a counting pass plus a second index array.

**Solver: Jacobi-preconditioned conjugate gradient, projected onto the free
degrees of freedom.** This follows from the representation rather than being an
independent choice. A coordinate list has no random access — `ElementAt` on it
is a scan of every entry — so any method that INDEXES the matrix is quadratic per
element read before it does any arithmetic. A Krylov method never indexes the
matrix; it only multiplies by it, and that product is one sweep of the entries.
Jacobi preconditioning is five extra lines and is what makes the iteration count
usable at demo sizes.

Boundary conditions are applied by PROJECTION rather than by editing the matrix:
a per-degree-of-freedom multiplier vector (0 at a constrained one, 1 elsewhere),
applied to the residual and to every search direction. The iterate starts at the
prescribed displacements, so a non-zero prescribed value — a settlement, a
stretched fixture — needs no special case, and the assembled matrix stays a plain
concatenation of element entries that can be built once and solved under several
sets of restraints.

**One element kernel, two element families.** For an ISOTROPIC material the
whole of `B^T D B` collapses, because `D` holds only the two Lame constants:

    K(i r, j s) = measure * (lambda * gi_r * gj_s
                             + mu * (gi_s * gj_r + (r == s ? gi . gj : 0)))

Nothing builds a strain-displacement matrix or multiplies three dense matrices.
The same expression serves the tetrahedron and the plane triangle, because the
plane element is the same variational form with the third gradient component
zero. Plane stress and plane strain differ ONLY in the first Lame parameter
(`E nu / (1 - nu^2)` versus `E nu / ((1 + nu)(1 - 2 nu))`), so `PlaneCondition`
selects a number, not a code path. A side benefit: no `Multiply(MatrixN, MatrixN)`
was needed, which foundation does not have and `future` may not add.

**Loads are resolved before they are summed.** Every load kind produces an
`Array<DofLoad>` — a degree of freedom and an amount — and one shared
`ScatterLoads` sums them. That separates "which degrees of freedom does this
load reach, and in what proportion" (different per load type and per element)
from the accumulation (one function), and it is what lets nodal forces, gravity
and face tractions be independent pure functions that are simply concatenated.

Gravity and face traction share their element measure equally among the element's
nodes. That is exact, not lumped: a linear tetrahedron's shape functions each
integrate to a quarter of its volume and a linear triangle's to a third of its
area. The beam's distributed load is integrated properly instead — the four
Hermite antiderivatives are written out so a load covering PART of an element
still gets its exact consistent nodal loads.

**The beam is deliberately transverse-only.** Two degrees of freedom per node,
deflection then rotation, no axial degree of freedom. Stated consequences: it
cannot see axial force or buckling, and `Pinned` and `Roller` restrain the same
thing in it. Supports and point loads snap to the nearest node rather than
splitting an element; the doc comment says to pick an element count that lands a
node on every feature.

**Verified against closed forms.** The generated TypeScript was executed
(`Plato.CLI --typescript` over all four tiers, run under Node with shims for the
five TypeScript-writer gaps catalogued in `plato-419`). Unit cube split into six
tetrahedra under 1 MPa uniaxial traction: displacement 5.000000e-6 m against
`sigma L / E` = 5.000000e-6, transverse contraction -1.500000e-6 against
`-nu sigma / E`, von Mises 1.000000e+6 Pa in all six cells, 17 iterations.
Unit square as two plane-stress triangles: identical numbers, 4 iterations.
Gravity load vector on the cube totals -77008.500000 N against `rho V g`.
Euler-Bernoulli beam, relative error against the textbook deflection: cantilever
tip load 9e-15, cantilever UDL 4e-15, simply supported centre load 2e-16, simply
supported UDL 2e-15, cantilever half-span UDL 8e-14.

**Known: the TypeScript target cannot run this as emitted.** Not a defect of this
vocabulary — every gap is already catalogued in `plato-419`, and each one is hit
by the shipping tiers too. The FEA path needs: `Arr.FlatMap` / `Concatenate` /
`Zip` (the prelude stops at `At`/`Count`/`Map`/`Reduce`); a `Buffer<T>` runtime;
definitions for functions whose first parameter is `Array<ConcreteType>` (they
are declared and called but the "Array functions over concrete element types"
section is emitted empty); integer division (`Integer` and `Number` share
`Number.prototype.Divide`, so `k / 3` is float division and indexes fractionally);
and sum-type emission (CHK320 skips `PlaneCondition`, `BeamRestraint` and
`BeamLoad`, so `SolveBeam` and the 2D path have unresolvable references).
The 3D solid path uses no sum type and is otherwise complete.

## Done means

- [x] Linear-elastic material and its constitutive matrix
- [x] Element stiffness for the linear tetrahedron and the linear triangle
- [x] Global assembly over a `TetrahedralMesh3D` with a stated matrix representation
- [x] Supports, nodal forces and gravity as boundary conditions
- [x] A solve producing per-node displacement and per-element von Mises stress
- [x] A beam path whose answers can be checked against closed-form deflection
- [x] All four tiers parse and type-check; no shipping-tier file references `future`
- [x] Design decisions recorded above
- [ ] A browser demo drives it: `demos/webgl/fea.html` + `src/demos/fea.ts`, green
      under `npm run typecheck` and `npm run scenes`. The 3D solid path uses no sum
      type and is the one to build on; the plane and beam paths need a
      `PlaneCondition` / `BeamRestraint` shim first (plato-419 defect 5)

Follow-up filed as `plato-427` (nodal stress smoothing, the hexahedral element,
quadratic elements, plane edge traction, axial/torsional beam degrees of
freedom).
