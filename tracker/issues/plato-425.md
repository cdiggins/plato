---
id: plato-425
title: Rigid body dynamics: fill in the future-tier integrator, contacts and constraints
type: feature
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed: 2026-08-03
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

### The solver is a fold over rows, threading the whole world

`SolveVelocityPass` is `Reduce` over the constraint-row indices with a
`RigidWorld3D` accumulator; `SolveVelocities` is `Reduce` over the iteration
counter with the same accumulator. The unit of the fold is `SolveRow(world, index)`
— "the world after solving this one contact once".

The accumulator is the whole world rather than a velocity array because a row visit
writes three things: two body velocities and the row's own accumulated impulses.
Threading the world makes each pass a total function of state with no aliasing
question, and makes every level independently callable and independently testable.

Rejected:

- **A Jacobi pass** (every row's impulse computed from one frozen state, then
  summed per body). It parallelizes and costs the same, and it does not converge on
  the case that matters: in a stack each contact computes the impulse to support the
  load alone, all apply at once, the stack over-corrects, and the next iteration
  over-corrects back. Under-relaxing removes the oscillation and destroys the
  convergence rate. Gauss-Seidel gets stacks right because row *n* sees row *n-1*'s
  work.
- **A velocity-array accumulator** with impulses returned alongside. Saves copying
  the settings through the fold; costs a second array that must stay the same length
  as the first, with one function able to violate that.

The cost is stated in the source rather than hidden: replacing one element of an
immutable array rebuilds it, so a row visit is linear in the body count and a pass
is rows times bodies. `ReplacedAt` is that primitive, written in
`rigid-dynamics.library.plato` because it belongs beside `Append` and `Concatenate`
in the foundation tier and this tier may not reach into that one.

### Semi-implicit (symplectic) Euler

Velocity is advanced by acceleration first; position is then advanced by the *new*
velocity. Explicit Euler uses the old velocity and gains energy every step on the
orbit and the spring — the two motions a rigid-body engine spends its life
integrating — so the motion spirals outward at any step size. Semi-implicit Euler is
symplectic: its energy error is bounded and oscillates rather than accumulating, at
identical arithmetic cost. Higher order (velocity-Verlet, RK4) was rejected for a
different reason: both want to evaluate forces more than once per step, and a step
containing a contact solve has no smooth force field to re-evaluate.

The gyroscopic term `-I^-1 (w x I w)` is omitted. Integrated explicitly, as it would
have to be here, it injects energy and a fast-spinning body diverges. Omitting it is
exact for any body with two equal moments (every ball, cylinder and capsule) and
costs a box its tumbling instability.

### Resting-contact stability: split impulse, not Baumgarte

Penetration is removed by a **separate pseudo-velocity pass** (`SolvePositions`):
the bodies are copied at rest, `PositionIterations` normal-only sweeps are run on
the copy driving each row toward a correction speed, and the resulting twists are
used only as a displacement rate in `IntegratePoseWith` and then discarded. No
energy enters the real velocities.

Rejected: **Baumgarte bias inside the velocity solve** — adding the penetration term
to the target separation speed of the ordinary solve. One line, and wrong in a way
that shows: the correction lands in the body's real velocity, so a deeply overlapped
body is launched and a resting stack breathes as the bias pumps energy in and
damping takes it out.

Three further things carry resting stability, each with its reason in the source:

- **Warm starting.** `ContactConstraint3D` carries its accumulated impulses, and
  `WarmStartFrom` copies them onto the next frame's rows matched by body pair and
  contact position. Without it a stack sinks: the solver spends its whole iteration
  budget rediscovering the support impulses it already found.
- **The penetration slop.** Correction applies only beyond `PenetrationSlop`, so a
  resting contact keeps a small overlap and collision detection keeps finding it. A
  solver that drives penetration to exactly zero loses the contact, regains it after
  the body falls back, and buzzes at the frame rate.
- **The restitution threshold.** A contact closing slower than
  `RestitutionThreshold` asks for zero separation speed. Without that cut-off a body
  on the floor bounces forever, because gravity re-supplies exactly the approach
  speed restitution hands back.

### The contact model

Accumulated impulses are clamped, never increments. Clamping increments would forbid
a row from undoing an over-correction an earlier row made in the same pass, and
over-correction is normal — each row solves its own contact exactly, in ignorance of
the others. It is also what makes the friction cone bound (mu times the accumulated
normal impulse) mean anything.

The friction basis is **stored in the row**, not recomputed per iteration: the
accumulated friction impulse is expressed in that basis, so a basis that flips
between iterations silently discards it. Both friction axes are measured against the
same post-normal state and clamped together against the cone, so the bound applies to
the friction impulse as a vector rather than per axis.

The restitution target is likewise resolved once at row-build time from the approach
speed *before* the solver runs; reading it inside the loop would sample a velocity
the loop has already changed and make bounce decay with the iteration count.

### `SolverBody3D` — motion kind becomes zero, tensor becomes a diagonal

The solver reads a derived record, not `RigidBody3D`. `Static` / `Kinematic` become
zero inverse mass and inertia, so the solver contains no case analysis at all. The
inertia tensor becomes its principal diagonal, so applying `I^-1` is a rotation into
the body frame, three multiplies and a rotation back — no matrix to assemble and
nothing to invert. `MobilityScale` is the single function in the library that reads
`BodyMotion`, which keeps every sum type out of the solver's hot path (and therefore
out of the TypeScript backend's way — sums are C#-only, `docs/SEMANTICS.md` §7).

### Ball-and-socket solved one axis at a time

Three scalar rows solved in turn rather than one coupled 3x3 block. The block solve
needs the inverse of a 3x3 symmetric effective-mass matrix and converges in a single
visit; the axis-at-a-time solve is Gauss-Seidel over the three rows and needs no
matrix inverse. Taken because a symmetric 3x3 inverse is foundation vocabulary that
does not yet exist, and adding one to serve a single call site is the wrong shape of
dependency. It costs iterations, not correctness — the fixed point is the same.

### Contacts are not regenerated inside `Step`

Detection needs shapes and `RigidWorld3D` holds none, so the caller refreshes
`Constraints` between steps. That is what lets one step function drive a scene of
spheres, a scene of boxes, or a scene whose contacts came from somewhere else.
`StepBallScene` is the worked composition for the ball-and-ground-plane case.

## Done means

- [x] Rigid body state, inertia tensors for the primitives, parallel-axis shift
- [x] A pure step function with a stated integrator
- [x] Gravity, damping, applied force and torque
- [x] The primitive-pair collision tests, producing a contact manifold
- [x] Impulse-based response with restitution and friction, and a penetration fix
- [x] At least the distance and ball-and-socket constraints
- [x] All four tiers parse and type-check; no shipping-tier file references `future`
- [x] Design decisions recorded above
- [x] A browser demo drives it: `demos/webgl/rigidbody.html` + `src/demos/rigidbody.ts`,
      green under `npm run typecheck` and `npm run scenes` — the latter steps every
      ticking scene and fails on a non-finite position, which is the check a solver
      needs. Blocked on the `ReplacedAt` shim (plato-429) before the solver runs at all

Deferred to `plato-428`: GJK/EPA for general convex pairs, face-contact manifold
generation, the orientation joints (hinge, slider, fixed, generic) with their limits
and motors, a broad phase, and a planar contact solver. `plato-429` records the
TypeScript writer gap that a browser demo of this work will hit.
