---
id: plato-428
title: Rigid dynamics: GJK/EPA, orientation joints, and a broad phase
type: feature
status: ready
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [plato-425]
---

## What and why

`plato-425` landed the rigid-body pipeline in `stdlib/future` — state, semi-implicit
Euler, primitive inertia tensors, the primitive-pair collision tests, sequential
impulses with restitution and Coulomb friction, split-impulse position correction,
and the distance / ball-and-socket joints. Three pieces of the original scope were
deliberately left out because each is a coherent job of its own rather than a
detail of that one.

**1. GJK/EPA for general convex pairs.** `collision.library.plato` has analytic
tests for sphere-sphere, sphere-plane, sphere-box, sphere-capsule and box-plane.
Anything else — box against box, capsule against capsule, convex hull against
anything — has no test. GJK finds the separating distance between two convex
shapes by iteratively refining a simplex in Minkowski-difference space, and EPA
recovers the penetration depth once they overlap. `stdlib/geometry` already
declares the support mapping both need (`ISupport3D`, and `Support` bodies for
`Box3D`, `Sphere`, `Capsule3D`, `Cylinder`, `Cone`, `Ellipsoid`), so the input
vocabulary exists. What does not exist is a decision about the iteration: both
algorithms terminate on a tolerance, not on a formula, and a pure functional
expression of "refine until converged" needs a bounded fold with an explicit
iteration cap. That choice, and the shape of the simplex record, is the work.

Also missing and belonging with it: **manifold generation for face contacts**.
GJK/EPA yields one deepest point; a box resting on a box needs four. That is
Sutherland-Hodgman clipping of one face against the other plus a reduction to at
most four points.

**2. The orientation joints.** `joints.library.plato` solves the two joints whose
constraint is purely positional. `HingeJoint`, `SliderJoint`, `FixedJoint`,
`GenericJoint`, `RevoluteJoint2D`, `PrismaticJoint2D`, `PulleyJoint` and
`GearConstraint` all constrain orientation as well, which needs an angular error
read off a quaternion difference and an angular effective mass that is the sum of
the two inverse inertias rather than a scalar. The whole limit-and-motor apparatus
(`AngularLimit`, `LinearLimit`, `JointMotor`, `JointBreakThreshold`) is declared
and unused for the same reason.

**3. A broad phase.** `BallSceneManifolds` in `collision.library.plato` tests
every ordered pair of bodies, so its cost is the square of the body count. A
sweep-and-prune over `Bounds3D`, or a BVH, cuts that to the pairs whose bounds
overlap. `stdlib/geometry`'s `spatial.concepts.plato` already declares the spatial
index interfaces such a phase would be written against.

Two smaller gaps worth folding in:

- **Planar contacts.** The solver is written against `SolverBody3D`. A faithful
  2D solver wants its own body record — a scalar angular velocity and a scalar
  moment, not a vector and a diagonal — rather than a degenerate use of the
  spatial one. The rotation-about-a-point algebra is identical, so this is a
  transcription rather than a design.
- **`SymmetricMatrix3x3` has no operations anywhere in the tree.** `plato-425`
  wrote the four it needed (`DiagonalInertia`, `ShiftedInertia`, `CombinedInertia`,
  `InertiaAbout`) inside `rigid-dynamics.library.plato` under domain names,
  because a general symmetric-matrix surface belongs in the foundation tier and
  adding one to serve a single caller is the wrong shape of dependency. A general
  surface — add, scale, multiply by a vector, invert, rotate by a quaternion,
  eigen-decompose to principal axes — would let a compound body carry a genuinely
  non-diagonal tensor, which is the one approximation `SolverBody3D` makes.

## Done means

- [ ] GJK separation and EPA penetration depth for the convex pairs, over `ISupport3D`
- [ ] Face-contact manifold generation (clip and reduce)
- [ ] Hinge, slider and fixed joints, with limits and motors
- [ ] A broad phase over `Bounds3D`
- [ ] All four tiers parse and type-check
