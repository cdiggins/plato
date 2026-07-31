---
lesson: numerical-integration
title: Numerical Integration of Motion
domain: Physics & simulation
v3-files: [53-kinematics.plato, 57-particles-simulation.plato]
audience: High-school calculus intuition (derivative as rate) and general programming background
status: draft-v1
---

# Numerical Integration of Motion

Closed-form motion is a luxury. A ballistic arc under constant gravity has a formula. A
particle buffeted by vortex fields, attractors, and gusting wind does not. You advance the
state a little, recompute accelerations, advance again. That loop is **numerical
integration** of ordinary differential equations — and the naive version of it, taught in
every first game-physics tutorial, will quietly explode.

## The idea

### From continuous to discrete

Continuous Newtonian motion for a point mass:

$$
\mathbf{v}'(t) = \mathbf{a}(t),\qquad
\mathbf{x}'(t) = \mathbf{v}(t)
$$

with acceleration from forces $\mathbf{a} = \mathbf{F}/m$ (and whatever fields you attach).
A simulator picks a step $\Delta t$ and replaces derivatives with finite updates.

### Explicit Euler (the trap)

The most obvious scheme:

$$
\begin{aligned}
\mathbf{v}_{n+1} &= \mathbf{v}_n + \mathbf{a}_n\,\Delta t \\
\mathbf{x}_{n+1} &= \mathbf{x}_n + \mathbf{v}_n\,\Delta t
\end{aligned}
$$

Position uses the **old** velocity. For oscillatory systems (springs, orbits, anything with
restoring force), explicit Euler pumps energy: orbits spiral out, springs grow amplitude
until NaNs appear.

```
  true orbit:     Euler orbit:
      ●●●●            ●
     ●    ●          ● ●
    ●      ●        ●   ●  ← radius grows each lap
     ●    ●          ● ●
      ●●●●            ●
```

### Semi-implicit Euler (symplectic Euler)

Update velocity first, then move with the **new** velocity:

$$
\begin{aligned}
\mathbf{v}_{n+1} &= \mathbf{v}_n + \mathbf{a}_n\,\Delta t \\
\mathbf{x}_{n+1} &= \mathbf{x}_n + \mathbf{v}_{n+1}\,\Delta t
\end{aligned}
$$

Still first-order accurate, but far better energy behavior for mechanics. This is the
default in many real-time engines for rigid bodies and particles.

### Verlet (positions remember the past)

**Verlet** stores two positions instead of position + velocity:

$$
\mathbf{x}_{n+1} = 2\mathbf{x}_n - \mathbf{x}_{n-1} + \mathbf{a}_n\,(\Delta t)^2
$$

Velocity is recovered when needed as
$(\mathbf{x}_{n+1} - \mathbf{x}_{n-1})/(2\Delta t)$. Cloth and rope solvers love Verlet /
position-based variants: constraints project positions, and the "velocity" is whatever
the positions imply.

### Closed form when you can

When acceleration is constant (ballistics) or the motion is circular / harmonic by
definition, prefer an exact sampler over stepping. Numerical methods are for the leftover
cases — and for systems where constraints and collisions interrupt any pretty formula.

### Stability vs step size

All explicit methods have a maximum stable $\Delta t$ that shrinks as stiffness rises
(stiff springs, tiny particles, huge forces). Substeps: take many small integrations inside
one display frame. Implicit methods can take larger steps but cost linear solves; real-time
graphics often stays explicit and pays in substeps.

## In Plato

### Kinematic state (`53-kinematics.plato`)

Instantaneous linear state is exactly what integrators read and write:

```plato
type KinematicState3D
    implements Value, Interpolatable
{
    Position: Point3D;
    Velocity: Vector3D;
    Acceleration: Vector3D;
}
```

Rigid bodies package pose with rates:

```plato
type RigidKinematicState3D
    implements Value
{
    Pose: Pose3D;
    LinearVelocity: Vector3D;
    AngularVelocity: Vector3D;
    LinearAcceleration: Vector3D;
    AngularAcceleration: Vector3D;
}
```

Spatial velocity as a twist (linear + angular) is available when you think in screw theory:

```plato
type Twist3D
    implements Value, Interpolatable
{
    Linear: Vector3D;
    Angular: Vector3D;
}
```

Sampled histories are first-class (`Velocities` may be empty when unknown):

```plato
type Trajectory3D
    implements Value
{
    Times: Array<Duration>;
    Positions: Array<Point3D>;
    Velocities: Array<Vector3D>;
}
```

Closed-form motions avoid stepping when applicable:

```plato
type BallisticTrajectory
    implements Value
{
    InitialPosition: Point3D;
    InitialVelocity: Vector3D;
    Gravity: Vector3D;
}

type SimpleHarmonicMotion
    implements Value
{
    Offset: Number;
    Amplitude: Number;
    AngularFrequency: AngularVelocity;
    Phase: Angle;
}

concept Kinematic3D
{
    PositionAt(x: Self, time: Duration): Point3D;
    VelocityAt(x: Self, time: Duration): Vector3D;
}
```

A ballistic sample is $\mathbf{x}_0 + \mathbf{v}_0 t + \frac{1}{2}\mathbf{g} t^2$ — exact
for constant $\mathbf{g}$, no Euler required.

### Particles and Verlet cloth (`57-particles-simulation.plato`)

Particles carry the Euler-friendly state (position, velocity, mass):

```plato
type Particle3D
    implements Value
{
    Position: Point3D;
    Velocity: Vector3D;
    Age: Duration;
    Lifetime: Duration;
    Size: Number;
    Color: Color;
    Rotation: Angle;
    AngularVelocity: AngularVelocity;
    Mass: Mass;
}
```

Force fields supply accelerations the integrator will apply:

```plato
type ParticleGravity
    implements Value
{
    Acceleration: Vector3D;
}

type ParticleDrag
    implements Value
{
    LinearCoefficient: Number;
    QuadraticCoefficient: Number;
}

type ParticleSystem3D
    implements Value
{
    Particles: Array<Particle3D>;
    Emitters: Array<ParticleEmitter3D>;
    Gravities: Array<ParticleGravity>;
    Drags: Array<ParticleDrag>;
    Vortices: Array<ParticleVortex>;
    Attractors: Array<ParticleAttractor>;
    Turbulences: Array<ParticleTurbulence>;
    Winds: Array<WindModel>;
    Time: Instant;
}
```

Cloth vertices are explicitly Verlet-shaped:

```plato
type ClothVertex
    implements Value
{
    Position: Point3D;
    PreviousPosition: Point3D;
    InverseMass: Number;
    Pinned: Boolean;
}
```

`PreviousPosition` is the position at the last step — the second sample Verlet needs.
Constraints then correct positions:

```plato
type ClothDistanceConstraint
    implements Value
{
    VertexA: VertexIndex;
    VertexB: VertexIndex;
    RestLength: Length;
    Compliance: Number;   // 0 = rigid
}
```

Usage-shaped semi-implicit step for one particle (illustrative):

```plato
// a := sum of field accelerations at p.Position, p.Velocity
let v2 = p.Velocity + a * dt;
let x2 = p.Position + v2 * dt;   // uses updated velocity
// new Particle3D { Position: x2, Velocity: v2, ... }
```

Usage-shaped Verlet step for cloth:

```plato
let xPrev = vertex.PreviousPosition;
let x = vertex.Position;
let xNext = x + (x - xPrev) + a * dt * dt;
// ClothVertex { Position: xNext, PreviousPosition: x, ... }
```

## Pitfalls / fine print

**Explicit Euler on springs.** If a demo "blows up," check whether position advanced with
old velocity. Switching to semi-implicit often fixes it without changing $\Delta t$.

**Variable frame time.** Using raw frame $\Delta t$ as the integration step couples
stability to frame rate. Prefer a fixed simulation step and accumulate remainder time.

**Units of drag.** `ParticleDrag` coefficients multiply velocity to produce acceleration
(doc: one per second / one per meter). Confusing them with force-domain drag on rigid
bodies (`DragModel` elsewhere) mixes $F$ and $a$ formulas.

**Pinned cloth vertices.** `Pinned: true` or `InverseMass: 0` means infinite mass — do not
integrate those vertices, or constraints will fight the pin every step.

**Empty velocities on trajectories.** `Trajectory3D.Velocities` may be empty; reconstructing
velocity by finite differences from positions is a second approximation on top of whatever
produced the path.

**Angular integration.** Updating `Quaternion` orientation from angular velocity is not the
same as adding a vector to a point. Tiny-step exponential maps or normalized integration
belong here; treating orientation components as independent Euler angles reintroduces
singularities.

## Try it

1. Spring force $a = -\omega^2 x$ in 1D. After one explicit Euler step from $(x,v)=(1,0)$,
   is $|x|$ likely larger or smaller than the true oscillation amplitude suggests over many
   steps?
2. Why does semi-implicit Euler use $v_{n+1}$ when advancing $x$?
3. A `ClothVertex` has `Position` and `PreviousPosition` equal. What is the implied velocity?

<details>
<summary>Answers</summary>

1. Larger over many steps — explicit Euler tends to inject energy into oscillatory systems.
2. So the position update sees the post-force velocity; that symplectic ordering improves
   energy behavior versus using the stale $v_n$.
3. Zero (for a standard central-difference / Verlet interpretation): the particle did not
   move between the last two stored positions.

</details>

## Library recommendations

- **missing-type** — `53-kinematics.plato` / `57-particles-simulation.plato`: there is no
  `Integrator` / `IntegrationScheme` sum (`ExplicitEuler | SemiImplicitEuler | Verlet | ...`).
  The lesson teaches named methods that have no vocabulary hook; cloth implies Verlet via
  `PreviousPosition` only by documentation.

- **wrong-shape** — `53-kinematics.plato`: `Trajectory3D` stores parallel arrays but declares
  no invariant that `Times`, `Positions`, and `Velocities` lengths match (except the empty-
  velocities escape hatch). A doc-comment invariant or a dedicated sampled-motion concept
  would harden the teaching examples.

- **missing-function** — `53-kinematics.plato`: `BallisticTrajectory` and
  `SimpleHarmonicMotion` look like `Kinematic3D` implementors, but the file never says
  `implements Kinematic3D`. Wiring that would let `PositionAt` be the single verb for both
  closed-form and (later) numerically sampled motion.

- **pedagogy** — `57-particles-simulation.plato`: `SoftBodySettings` and SPH types sit beside
  particles without a shared "advance by `Duration`" operation. Teaching integration across
  particles vs cloth requires inventing a step function the declarations do not name.
