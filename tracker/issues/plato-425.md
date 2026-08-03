---
id: plato-425
title: Rigid body dynamics: fill in the future-tier integrator, contacts and constraints
type: feature
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

## What and why

`stdlib/future/rigid-dynamics.types.plato` already declares `RigidBody2D` /
`RigidBody3D` and neighbours; `rigid-dynamics.library.plato` is 52 lines and
`rigid-dynamics.concepts.plato` is 21. `collision.types.plato` and
`joints.types.plato` sit alongside with the same problem. There is a vocabulary and
almost no dynamics.

Scope: **`stdlib/future`** — not linted, not converted to C#, but it **must parse
and type-check** (`ForwardStdLib*` reads all four tiers), and nothing in a shipping
tier may reference it.

Read what is already there before adding anything; extend the existing types rather
than declaring parallel ones.

Subject matter, roughly:

- **State and step**: a rigid body as position, orientation (`Quaternion`), linear
  and angular velocity, mass and inertia tensor. A step is a pure function from world
  state to world state — semi-implicit Euler is the right default; say so and say why
  explicit Euler is not.
- **Inertia tensors** for the primitive shapes the tree already has (box, sphere,
  capsule, cylinder), and the parallel-axis shift.
- **Forces**: gravity, linear and angular damping, applied force and torque at a
  point.
- **Collision detection**: sphere-sphere, sphere-plane, box-plane, sphere-box, and
  a general convex pair via GJK/EPA if it fits the budget — otherwise say it was
  deferred. Contact manifold = point, normal, penetration depth.
- **Contact response**: sequential impulses with restitution and Coulomb friction,
  and Baumgarte or position-projection to remove penetration. Resting contact
  stability is what separates a demo that works from one that jitters — name the
  approach you took.
- **Constraints/joints**: distance, hinge, ball-and-socket, over the existing
  `joints.types.plato` and `SpringJoint`.

The hard part in a pure language is the solver iteration: sequential impulses is
naturally an in-place loop over contacts. Say how you expressed it — a fold over
contacts producing a new velocity state, a Jacobi-style parallel pass, or otherwise.

## Design decisions

_(fill in — integrator, solver shape, contact model, what you rejected)_

## Done means

- [ ] Rigid body state, inertia tensors for the primitives, parallel-axis shift
- [ ] A pure step function with a stated integrator
- [ ] Gravity, damping, applied force and torque
- [ ] The primitive-pair collision tests, producing a contact manifold
- [ ] Impulse-based response with restitution and friction, and a penetration fix
- [ ] At least the distance and ball-and-socket constraints
- [ ] All four tiers parse and type-check; no shipping-tier file references `future`
- [ ] Design decisions recorded above
