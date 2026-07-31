---
lesson: joint-constraints-preview
title: Joints and Limits — Constraining Rigid Bodies
domain: Physics & simulation
v3-files: [06-quantities.plato, 13-transforms.plato, 56-joints-constraints.plato]
audience: Basic rigid-body / degrees-of-freedom intuition and general programming background
status: draft-v1
---

# Joints and Limits — Constraining Rigid Bodies

A free rigid body in 3D has **six degrees of freedom**: three translations, three
rotations. A **joint** nails some of those DOFs relative to another body (or the
world) so a door only swings, a piston only slides, or a shoulder only cones within
anatomical limits. Constraints are how articulated figures, machines, and ragdolls
stop being piles of unrelated bodies.

This is a preview of the vocabulary: limits, motors, and the standard joint zoo —
enough to read a hinge definition and know which freedoms remain.

## The idea

Think of each joint as removing equations of motion. A **hinge** keeps two anchors
coincident and aligns two axes, leaving **one** relative rotation. A **slider**
leaves **one** relative translation. A **fixed** joint leaves **zero**. A
**ball-socket** leaves **three** relative rotations (then limits carve a cone).

```
  Body A ●========● Body B     hinge axis
              ^
              anchors coincide; spin about the shared axis only
```

**Limits** bound the remaining free coordinate: an angular interval for hinges, a
linear interval for sliders. Disable the limit and that coordinate is unbounded
(until something else collides).

**Motors** push toward a target relative *velocity* (not pose), capped by max force
or torque — powered wheels, servo hinges, conveyor rates.

**Break thresholds** remove the joint when constraint force/torque exceeds a limit —
breakable ragdoll bones, frangible fixtures.

Anchors and axes are expressed in each body's **local frame**. Body index $-1$ means
"attach this side to the static world" (a door hinged to a building).

## In Plato

Shared limit and motor records (`56-joints-constraints.plato`):

```plato
type AngularLimit
{
    Min: Angle;
    Max: Angle;
    Enabled: Boolean;
}

type LinearLimit
{
    Min: Number;    // meters along the joint axis
    Max: Number;
    Enabled: Boolean;
}

type JointMotor
{
    TargetVelocity: Number;  // m/s linear or rad/s angular
    MaxForce: Force;
    MaxTorque: Torque;
    Enabled: Boolean;
}

type JointBreakThreshold
{
    MaxForce: Force;
    MaxTorque: Torque;
}
```

`Angle`, `Force`, `Torque`, `Length`, and related quantities come from
`06-quantities.plato`. Poses use `Pose3D` from transforms.

### One-DOF classics

```plato
type HingeJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    AnchorA: Point3D;
    AnchorB: Point3D;
    AxisA: Direction3D;
    AxisB: Direction3D;
    Limit: AngularLimit;
    Motor: JointMotor;
}

type SliderJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    AnchorA: Point3D;
    AnchorB: Point3D;
    AxisA: Direction3D;
    Limit: LinearLimit;
    Motor: JointMotor;
}

type FixedJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    RelativePose: Pose3D;   // B in A's frame
}
```

Usage-shaped door hinge (illustrative):

```plato
let limit = AngularLimit(
    Angle(-0.1),   // slightly past closed, radians
    Angle(1.8),    // ~103° open
    true);

let hinge = HingeJoint(
    doorBody, wallBody,   // or wallBody = -1 for world
    anchorOnDoor,
    anchorOnWall,
    axisInDoor,
    axisInWall,
    limit,
    disabledMotor);
```

### Soft and distance constraints

```plato
type DistanceJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    AnchorA: Point3D;
    AnchorB: Point3D;
    MinLength: Length;
    MaxLength: Length;
}

type SpringJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    AnchorA: Point3D;
    AnchorB: Point3D;
    RestLength: Length;
    Stiffness: Stiffness;
    Damping: DampingCoefficient;
}
```

A rope is a distance joint with `MinLength` zero; a rod sets min equal to max.

### Ball-socket with swing/twist limits

```plato
type BallSocketJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    AnchorA: Point3D;
    AnchorB: Point3D;
    TwistAxisA: Direction3D;
    SwingLimit: AngularLimit;
    TwistLimit: AngularLimit;
}
```

Anchors coincide; orientation is free until swing (cone away from `TwistAxisA`) and
twist (rotation about that axis) limits engage — the usual shoulder/hip model.

### Generic 6-DOF and groupings

```plato
type GenericJoint
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    FrameA: Pose3D;
    FrameB: Pose3D;
    LinearX: LinearLimit;
    LinearY: LinearLimit;
    LinearZ: LinearLimit;
    AngularX: AngularLimit;
    AngularY: AngularLimit;
    AngularZ: AngularLimit;
}

type RagdollProfile
{
    HingeJoints: Array<HingeJoint>;
    BallSocketJoints: Array<BallSocketJoint>;
    FixedJoints: Array<FixedJoint>;
    BreakThresholds: Array<JointBreakThreshold>;
}
```

2D counterparts (`RevoluteJoint2D`, `PrismaticJoint2D`, `DistanceJoint2D`) mirror the
same ideas with planar anchors and a single out-of-plane rotation for revolute joints.

**Naming note.** Skeletal animation owns the bare name `Bone`; every type here is
`*Joint` or `*Constraint`-suffixed so physics and animation do not collide.

## Pitfalls / fine print

**Axis agreement.** `AxisA` and `AxisB` should describe the *same* world axis when
bodies are in the reference pose. If they disagree, the solver fights itself and the
hinge jitters.

**Limit units.** `AngularLimit` uses `Angle` (radians field). `LinearLimit` uses bare
`Number` meters. `JointMotor.TargetVelocity` is also a bare `Number` whose unit
depends on whether the joint is linear or angular — easy to pass deg/s by mistake.

**Enabled flags.** Limits and motors carry `Enabled`. Leaving stale Min/Max filled
while `Enabled = false` is fine; reading Min/Max without checking Enabled is not.

**World attachment.** `BodyIndex` $-1$ is static world. Anchors for the world side are
still stored as points — interpret them in world space per engine convention (document
in the implementation pass).

**FixedJoint vs locked GenericJoint.** Functionally similar if all generic limits are
collapsed to zero width; `FixedJoint` is the clear, cheaper statement of intent.

**Motors target velocity, not angle.** Driving a hinge to an *angle* needs a servo
loop outside `JointMotor` (measure error, set target velocity). The type is not a
position PID.

**Ragdoll break order.** `BreakThresholds` aligns to declaration order: hinges first,
then ball-sockets, then fixed — empty array means unbreakable.

## Try it

1. How many relative DOFs does a `HingeJoint` allow when its angular limit is disabled?
2. Door welded shut: which joint type, and what does `RelativePose` represent?
3. Why might a `SpringJoint` be preferable to a `DistanceJoint` for a soft hanging lamp?

<details>
<summary>Answers</summary>

1. One — rotation about the shared axis, unbounded if `Limit.Enabled` is false.
2. `FixedJoint`; `RelativePose` is body B's pose expressed in body A's frame at the
   welded configuration.
3. Distance joints enforce hard min/max length (rope/rod). A spring applies
   $-k(x-x_0) - c\dot{x}$ forces, allowing oscillation and soft compliance instead of
   sudden constraint impulses.

</details>

## Library recommendations

- **wrong-shape** — `56-joints-constraints.plato`: `JointMotor.TargetVelocity` is an
  untyped `Number` serving both m/s and rad/s. Prefer a sum
  `LinearVelocity Target | AngularVelocity Target` (quantity types) so hinges cannot
  silently take linear units.

- **doc-comment** — `56-joints-constraints.plato`: `BallSocketJoint` swing/twist limits
  need a precise cone definition (is `SwingLimit.Max` the half-angle from
  `TwistAxisA`?). The fields are teachable only after that sentence exists.

- **missing-type** — `56-joints-constraints.plato`: no position/angle *servo* motor
  (target angle + gains), only velocity `JointMotor`. Door closers and IK-ish drives
  need it; the lesson currently tells readers to roll their own outer loop.

- **missing-function** — `56-joints-constraints.plato`: no query for relative hinge
  angle or slider offset given two body poses. Authoring limits and debugging motors
  requires that measurement as a declared helper on `HingeJoint` / `SliderJoint`.
