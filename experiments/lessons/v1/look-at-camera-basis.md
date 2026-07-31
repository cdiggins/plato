---
lesson: look-at-camera-basis
title: Look-At Cameras and Orthonormal Bases
domain: Matrices & transforms
v3-files: [13-transforms.plato, 48-cameras.plato, 70-intrinsics.plato, 08-vectors.plato]
audience: Basic 3D vectors; familiarity with "camera" as an eye in a scene
status: draft-v1
---

# Look-At Cameras and Orthonormal Bases

You know where the camera sits and what it should stare at. You still need
three perpendicular unit axes — right, up, and forward — before you can build
a view matrix or a `Pose3D`. That construction is the look-at problem: turn
an aim direction plus a roll hint into a rigid frame.

## The idea

An orthonormal frame in 3D is an origin plus three mutually perpendicular
unit vectors $(X, Y, Z)$. In camera language:

- **Forward** — from the eye toward the target
- **Right** — horizontal axis of the sensor
- **Up** — vertical axis of the sensor (after removing tilt that would roll
  incorrectly)

Given eye position $E$, target $T$, and a world-up hint $U$ (often
$(0,1,0)$):

1. Forward $f = \mathrm{normalize}(T - E)$
2. Right $r = \mathrm{normalize}(f \times U)$   (or $U \times f$, depending
   on handedness — be consistent)
3. True up $u = r \times f$   (re-orthogonalized; may differ from $U$)

```
        u (camera up)
        ^
        |
        +----> r (camera right)
       /
      / f (look direction)
     E ●·················> T
```

The world-up hint resolves the free roll about the view axis. When the
camera looks nearly parallel to $U$, the cross product shrinks and the basis
becomes unstable — the classic look-at singularity at the poles.

The resulting axes plus origin $E$ are a rigid placement: a `Frame3D` or a
`Pose3D`. The view matrix is the inverse of the camera-to-world transform:
it maps world points into camera space.

## In Plato

Cameras that author by aiming use `LookAtCamera` in `48-cameras.plato`:

```plato
// A perspective camera authored by aiming: positioned at Position, looking at
// Target, with Up resolving the roll about the view axis.
type LookAtCamera
    implements Value
{
    Position: Point3D;
    Target: Point3D;
    Up: Direction3D;
    VerticalFov: Angle;
    AspectRatio: Number;
    Near: Number;
    Far: Number;
}
```

Most other cameras store a finished `Pose3D` instead:

```plato
type PerspectiveCamera
    implements Camera
{
    Pose: Pose3D;
    VerticalFov: Angle;
    AspectRatio: Number;
    Near: Number;
    Far: Number;
}

type Pose3D
    implements Value, Interpolatable
{
    Position: Point3D;
    Orientation: Quaternion;
}
```

Frames and bases live in `13-transforms.plato`:

```plato
// An orthonormal coordinate frame in space.
type Frame3D
    implements Value
{
    Origin: Point3D;
    XAxis: Direction3D;
    YAxis: Direction3D;
    ZAxis: Direction3D;
}

// Three basis vectors in 3D; not necessarily orthonormal.
type Basis3D
    implements Value
{
    X: Vector3D;
    Y: Vector3D;
    Z: Vector3D;
}
```

`Direction3D` wraps a unit `Vector3D`. Building axes uses vector intrinsics
(`Normalize`, `Cross`, `Dot`) from `70-intrinsics.plato`, plus
`Between` for point differences:

```plato
let eye = look.Position;
let target = look.Target;
let upHint = look.Up.Vector;

// Forward: from eye toward target
let forward = Normalize(Between(eye, target));

// Right: perpendicular to forward and world-up hint
let right = Normalize(Cross(forward, upHint));

// True camera up: completes a right-handed orthonormal triad
let trueUp = Cross(right, forward);

let frame = Frame3D {
    Origin: eye,
    XAxis: Direction3D { Vector: right },
    YAxis: Direction3D { Vector: trueUp },
    ZAxis: Direction3D { Vector: forward }
};
```

Handedness note: some engines use `-forward` as the camera's local $Z$
(OpenGL-style view space). Plato's `Frame3D` simply stores three axes; the
camera convention must be documented when converting to a view matrix.

A matrix form is already an intrinsic:

```plato
CreateLookAt(_: Matrix4x4,
    cameraPosition: Point3D,
    cameraTarget: Point3D,
    cameraUpVector: Vector3D): Matrix4x4;
```

```plato
let view = Matrix4x4.CreateLookAt(
    look.Position,
    look.Target,
    look.Up.Vector);
```

Converting a rigid pose to a frame (the inverse teaching direction) is in
`Transforms`:

```plato
let pose = Pose3D { Position: eye, Orientation: q };
let frameFromPose = Frame3D(pose);   // rotated unit axes at Position
let basis = Basis3D(q);              // axis images; origin not included
```

`Basis3D` from a quaternion is the pure orientation part — useful when you
care about axes without a camera origin. `Frame3D` adds the origin and
requires orthonormal `Direction3D` axes.

## Pitfalls / fine print

**Look-at singularity.** When `forward` is parallel to `Up`, `Cross` yields
near-zero and `Normalize` blows up. Detect `|Dot(forward, upHint)|` near 1
and fall back to an alternate up (e.g. world X) or freeze the previous
right vector.

**Up is a hint, not a constraint.** After orthogonalization, `trueUp` may
differ from the authored `Up`. That is correct: you cannot keep an arbitrary
up while also looking exactly at the target unless they are already
compatible.

**Position vs Target coincide.** `Between(eye, target)` is the zero vector;
normalization is undefined. Reject equal points before building the frame.

**View vs camera-to-world.** `CreateLookAt` typically builds the matrix that
transforms *into* view space (world-to-camera). `Frame3D.Matrix4x4` in
`Transforms` maps *from* frame-local to world. Inverting one yields the
other for rigid frames — do not compose them as if they were the same.

**Interpolating look-at parameters.** Lerping `Position` and `Target`
independently can swing the forward vector through the singularity and
change path length oddly. Prefer interpolating the resulting `Pose3D`
(`Orientation` via `Slerp`) when animating cameras.

## Try it

1. Eye at $(0,0,5)$, target at $(0,0,0)$, up hint $(0,1,0)$. What is
   forward? What direction should "right" point in a right-handed system
   with $r = \mathrm{normalize}(f \times U)$?
2. Why rebuild `trueUp` with a cross product instead of reusing the hint?
3. Name one situation where `Basis3D` is preferable to `Frame3D`.

<details>
<summary>Answers</summary>

1. Forward $= (0,0,-1)$ after normalize. $f \times U = (0,0,-1)\times(0,1,0)
   = (-1,0,0)$ (up to sign conventions of the cross product order) — right
   points along $-\!X$ or $+X$ depending on exact cross order; the point of
   the exercise is that right is horizontal and perpendicular to forward.
2. The hint need not be perpendicular to forward; the second cross restores
   orthonormality and consistent handedness.
3. When you need oriented axes without an origin — e.g. transforming normals
   or building a rotation-only matrix from `Basis3D(q)`.

</details>

## Library recommendations

- **missing-function** — `48-cameras.plato` / `13-transforms.plato`: no
  `Frame3D(look: LookAtCamera)` or `Pose3D(look: LookAtCamera)` conversion
  is declared. Authors must hand-roll Cross/Normalize or jump to
  `Matrix4x4.CreateLookAt`, which skips the typed frame entirely.

- **missing-function** — `13-transforms.plato`: `Frame3D` has
  `Matrix4x4(f)` (local-to-world) but no named `ViewMatrix(f)` /
  `WorldToFrame` inverse helper. Look-at teaching constantly needs both
  directions.

- **doc-comment** — `48-cameras.plato`: `LookAtCamera` should state which
  axis is forward in the resulting pose (local $+Z$, $-Z$, etc.) and the
  cross-product order used when lowered to `CreateLookAt`, so handedness
  bugs are not rediscovered per backend.

- **pedagogy** — `13-transforms.plato`: `Basis3D` docs say "not necessarily
  orthonormal" while `Frame3D` requires orthonormal `Direction3D` axes. A
  one-line "use Frame3D for rigid cameras; Basis3D for general linear
  frames" would steer API choice during look-at construction.
