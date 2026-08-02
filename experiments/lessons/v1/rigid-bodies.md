---
lesson: rigid-bodies
title: Rigid Bodies
domain: Physics & simulation
v3-files: [54-rigid-dynamics.plato]
audience: High-school physics (force, mass, torque) and general programming background
status: draft-v1
---

# Rigid Bodies

A crate slides across a warehouse floor. It has a mass, a center of mass that is not the
geometric center (the heavy side is fuller), a linear velocity, and a spin. Kick it off-center
and it both translates and rotates. Kick it through the center of mass and it only translates.
That split — linear motion of the center versus rotation about the center — is the entire
content of rigid-body dynamics.

A **rigid body** is an object whose points stay at fixed distances from each other. The
whole configuration is captured by a pose (position plus orientation) plus how that pose
is changing (linear and angular velocity). Forces change the linear part; torques and
off-center forces change the angular part. Mass and the inertia tensor decide how stubborn
each part is.

## The idea

### Linear vs angular state

Newton's second law for translation is familiar:

$$
\mathbf{F} = m\,\mathbf{a}
\qquad\Rightarrow\qquad
\mathbf{a} = \mathbf{F}/m
$$

For rotation about the center of mass, the analog is:

$$
\boldsymbol{\tau} = \mathbf{I}\,\boldsymbol{\alpha}
$$

where $\boldsymbol{\tau}$ is torque, $\mathbf{I}$ is the **inertia tensor**, and
$\boldsymbol{\alpha}$ is angular acceleration. In 2D, $I$ collapses to a single scalar
(moment of inertia about the out-of-plane axis). In 3D it is a symmetric $3\times 3$
matrix: spin about different axes can cost different amounts of "effort," and the axes
couple.

The **center of mass** is the unique point where you can pretend all mass sits for
translation. Off-center forces produce torque relative to that point:

$$
\boldsymbol{\tau} = \mathbf{r} \times \mathbf{F}
$$

with $\mathbf{r}$ the vector from the center of mass to the application point.

```
        F (force)
        ↑
        │
   ●────┼────●     COM = center of mass
   body │
        ● COM
         \
          r × F  → torque (spin)
```

### Mass properties

A body's inertial identity is three numbers (2D) or a small package (3D):

- **Mass** $m$ — resists linear acceleration
- **Center of mass** — where $m$ acts for translation; expressed in the body's local frame
- **Inertia** — resists angular acceleration (scalar in 2D, tensor in 3D)

The inertia tensor is usually stored in **body space** about the center of mass. When you
need world-space angular dynamics you rotate it with the body's orientation. Principal axes
diagonalize $\mathbf{I}$; many engines diagonalize once and keep the body axes aligned with
those directions.

### Motion kinds

Not every "body" integrates forces the same way:

| Kind | Behavior |
|------|----------|
| **Static** | Never moves; infinite mass for collision response |
| **Kinematic** | Pose is set externally; treated as infinitely massive to dynamics |
| **Dynamic** | Integrates forces and impulses |

Static floors and kinematic elevator platforms are not "broken dynamic bodies" — they are
different participation modes.

### Forces, impulses, and environment

A **force** acts continuously (newtons). An **impulse** is an instantaneous change in
momentum (newton-seconds): collisions and explosions. Environmental models — uniform
gravity, point gravity, drag, buoyancy — produce forces from the body's current state
without naming a pair of colliding surfaces.

Surface **materials** (friction, restitution) do not live on the body state itself in
isolation; they merge when two surfaces touch. How coefficients combine (average, min,
max, multiply) is a modeling choice with visible gameplay consequences.

### Sleep and stepping

Bodies that barely move can be put to **sleep** to save work. Fixed-step integration with
substeps and solver iteration counts is how interactive simulations stay stable when the
render frame rate wobbles.

## In Plato

File `54-rigid-dynamics.plato` declares the vocabulary. Body identity in the world is a
typed index:

```plato
// A zero-based index of a body within an external body array.
// -1 means "none" / "the static environment".
type BodyIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type BodyMotion = Static | Kinematic | Dynamic;
```

Mass properties split by dimension:

```plato
type MassProperties2D
    implements Value
{
    Mass: Mass;
    CenterOfMass: Point2D;
    MomentOfInertia: MomentOfInertia;
}

type MassProperties3D
    implements Value
{
    Mass: Mass;
    CenterOfMass: Point3D;
    InertiaTensor: SymmetricMatrix3x3;
}
```

The 3D rigid body itself:

```plato
type RigidBody3D
    implements Value
{
    Pose: Pose3D;
    LinearVelocity: Vector3D;
    AngularVelocity: Vector3D;
    MassProperties: MassProperties3D;
    Motion: BodyMotion;
    LinearDamping: Number;
    AngularDamping: Number;
    GravityScale: Number;
}
```

`Pose` holds position and orientation. `LinearVelocity` is meters per second.
`AngularVelocity` is radians per second about each **world** axis (axis-scaled rates).
Damping fields are exponential decay rates (one per second; `0` means none).
`GravityScale` multiplies world gravity (`1` = normal, `0` = floating).

Applied loads name the point of application explicitly:

```plato
type AppliedForce3D
    implements Value
{
    Force: Vector3D;
    ApplicationPoint: Point3D;
}

type AppliedImpulse3D
    implements Value
{
    Impulse: Vector3D;
    ApplicationPoint: Point3D;
}

type RadialImpulse3D
    implements Value
{
    Center: Point3D;
    Magnitude: Impulse;
    Radius: Length;
}
```

Environmental force models implement interfaces:

```plato
interface ForceModel3D
{
    ForceOn(x: Self, body: RigidBody3D): Vector3D;
}

type UniformGravity3D
    implements Value
{
    Acceleration: Vector3D;   // e.g. (0, -9.81, 0)
}

type DragModel
    implements Value
{
    LinearCoefficient: Number;
    QuadraticCoefficient: Number;
}
```

Materials and combine rules:

```plato
type MaterialCombine = Average | Min | Max | Multiply;

type PhysicsMaterial
    implements Value
{
    StaticFriction: Number;
    DynamicFriction: Number;
    Restitution: Proportion;
    FrictionCombine: MaterialCombine;
    RestitutionCombine: MaterialCombine;
}
```

Simulation plumbing:

```plato
type SleepSettings
    implements Value
{
    LinearThreshold: Speed;
    AngularThreshold: AngularVelocity;
    TimeToSleep: Duration;
    Enabled: Boolean;
}

type TimeStepSettings
    implements Value
{
    FixedDeltaTime: Duration;
    Substeps: Integer;
    VelocityIterations: Integer;
    PositionIterations: Integer;
    MaxStepsPerFrame: Integer;
}
```

Usage-shaped sketches (illustrative — bodies not yet implemented):

```plato
let props = MassProperties3D {
    Mass: ...,
    CenterOfMass: Point3D { X: 0, Y: 0.2, Z: 0 },
    InertiaTensor: ...
};

let body = RigidBody3D {
    Pose: pose,
    LinearVelocity: Vector3D { X: 0, Y: 0, Z: 0 },
    AngularVelocity: Vector3D { X: 0, Y: 0, Z: 0 },
    MassProperties: props,
    Motion: Dynamic,
    LinearDamping: 0.05,
    AngularDamping: 0.05,
    GravityScale: 1
};

let kick = AppliedImpulse3D {
    Impulse: Vector3D { X: 50, Y: 0, Z: 0 },
    ApplicationPoint: Point3D { X: 0, Y: 1, Z: 0 }  // above COM → spin
};

let g = UniformGravity3D {
    Acceleration: Vector3D { X: 0, Y: -9.81, Z: 0 }
};
// ForceOn(g, body) → mass * Acceleration * GravityScale  (when implemented)
```

`OrbitalElements` in the same file is Keplerian state for spacecraft-style motion — related
physics, but a different representation than `RigidBody3D`.

## Pitfalls / fine print

**Torque about the wrong point.** Dynamics formulas assume torque about the center of mass
(or carefully transformed). Measuring $\mathbf{r}$ from a geometric origin that is not the
COM invents phantom spin.

**Inertia in the wrong frame.** Body-space $\mathbf{I}$ must be rotated to world space
before relating world torque to world $\boldsymbol{\alpha}$. Forgetting the frame change is
a classic "why does my tumbling look wrong" bug.

**Angular velocity representation.** `RigidBody3D.AngularVelocity` is a world-axis
`Vector3D`, not an `AngularVelocity` quantity type (2D uses the quantity type). Mixing
body-axis and world-axis rates without converting will desynchronize orientation updates.

**Static vs kinematic vs dynamic.** Applying forces to a kinematic body does nothing useful
if the integrator ignores them. Colliding a dynamic crate into a kinematic platform should
transfer impulse to the crate only — the platform's infinite mass is the model.

**Restitution and stacking.** High restitution plus many contacts makes stacks bounce
forever. Sleep thresholds and solver iteration counts are not optional polish; they are
part of the model.

**Damping units.** Linear and angular damping are rates (1/s), not dimensionless "percent
friction." Treating them as friction coefficients produces frame-rate-dependent mush once
variable steps appear.

## Try it

1. A force $\mathbf{F}$ is applied at the center of mass. What is the torque about the COM?
2. Same force, applied at a point with $\mathbf{r}$ from the COM nonzero and not parallel to
   $\mathbf{F}$. Does linear acceleration of the COM change compared to (1)?
3. Why might `BodyMotion.Kinematic` be preferred for a moving walkway the player stands on?

<details>
<summary>Answers</summary>

1. Zero — $\mathbf{r} = \mathbf{0}$, so $\boldsymbol{\tau} = \mathbf{0}$.
2. Linear acceleration of the COM depends only on net force and mass, so it is unchanged;
   angular acceleration appears because torque is nonzero.
3. The walkway's motion is scripted; treating it as dynamic would let players shove the
   building. Kinematic means "moves as told, infinite mass to the solver."

</details>

## Library recommendations

- **missing-function** — `54-rigid-dynamics.plato`: `ForceModel3D.ForceOn` is declared on
  the interface, but `UniformGravity3D`, `PointGravity`, `DragModel`, and `BuoyancyModel`
  do not yet list `implements ForceModel3D`. The lesson wants to say "gravity is a force
  model"; the implements clauses would make that teachable without a wink.

- **naming** — `54-rigid-dynamics.plato`: `RigidBody3D.AngularVelocity` is a `Vector3D`
  while `RigidBody2D.AngularVelocity` is the quantity type `AngularVelocity`. The asymmetry
  is documented, but a `Twist3D`-shaped field (or an explicit world-vs-body doc banner)
  would reduce confusion when teaching spatial angular rates.

- **missing-type** — `54-rigid-dynamics.plato`: there is no `RigidBodyWorld` / simulation
  container tying `Array<RigidBody3D>`, materials, and `TimeStepSettings` together. Bodies
  are free-floating records; pedagogy has to invent the "world array" that `BodyIndex`
  indexes into.

- **pedagogy** — `54-rigid-dynamics.plato`: `MassProperties3D.InertiaTensor` is a
  `SymmetricMatrix3x3` with no declared helper for "inertia of a solid box/sphere about
  COM." Deriving mass properties from shapes has nowhere to land those formulas in the
  vocabulary.
