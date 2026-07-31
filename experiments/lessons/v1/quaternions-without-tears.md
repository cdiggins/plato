---
lesson: quaternions-without-tears
title: Quaternions Without Tears
domain: Rotations
v3-files: [10-rotations.plato]
audience: Comfortable with 3D vectors and the idea of rotating points; no prior quaternion exposure assumed.
status: draft-v1
---

# Quaternions Without Tears

You need to store an object's orientation, compose two rotations, and blend smoothly from one pose to
another. A 3×3 rotation matrix works, but nine numbers is heavy, and interpolating matrix entries
does not preserve orthogonality. Euler angles are human-readable until a gimbal-lock pole makes two
different angle triples describe the same orientation — or none at all.

A **quaternion** is four numbers that encode a 3D rotation compactly, compose with one multiply,
and interpolate on a sphere instead of through a distorted cube. The mathematics looks exotic at
first; in practice it is a small set of conventions you can memorize once and reuse everywhere.

## Rotations as four numbers

Write a 3D rotation as an **axis-angle** pair: a unit axis $\mathbf{u}$ and an angle $\theta$
(following the right-hand rule). The quaternion that represents this rotation is built from the
**half angle** $\theta/2$:

$$
q = (\; X,\; Y,\; Z,\; W \;) =
\bigl(\; u_x \sin\frac{\theta}{2},\; u_y \sin\frac{\theta}{2},\; u_z \sin\frac{\theta}{2},\; \cos\frac{\theta}{2}\;\bigr)
$$

Read the tuple as two parts:

| Part | Fields | Meaning |
|------|--------|---------|
| Vector part | `X`, `Y`, `Z` | Points along the rotation axis, scaled by $\sin(\theta/2)$ |
| Scalar part | `W` | $\cos(\theta/2)$ |

**Why half the angle?** Composition of 3D rotations is nonlinear. Embedding the full angle in the
vector part would make `Multiply` fail the group laws. The half-angle construction turns rotation
composition into quaternion multiplication — the same trick that makes `Rotor2D` work in the plane
with `Scalar` and `XY` storing $\cos(\theta/2)$ and $\sin(\theta/2)$.

A **unit quaternion** satisfies $X^2 + Y^2 + Z^2 + W^2 = 1$. That is the invariant you must
preserve after repeated products; call `Normalize` when floating-point drift accumulates.

### Worked example: quarter turn about +Y

Rotate $90°$ about the world Y axis. With $\mathbf{u} = (0, 1, 0)$ and $\theta = 90°$:

```
sin(45°) ≈ 0.7071   cos(45°) ≈ 0.7071

q ≈ (0.0, 0.7071, 0.0, 0.7071)
```

The vector part lies entirely on Y; the scalar part is nonzero because the rotation is not $180°$.
The identity rotation is `(0, 0, 0, 1)` — zero vector part, scalar one.

### ASCII picture: the rotation axis pierces the "hemisphere"

```
              W (scalar, cos θ/2)
              ↑
              |     • q  (unit 4-vector on S³)
              |    /
              |   /  vector part (X,Y,Z) ∥ axis u
              |  /
              +----------→ (X,Y,Z) space
```

You do not rotate by adding quaternions. You rotate a vector with the **sandwich product**
implemented as `Transform`:

```
v' = Transform(v, q)     // rotates displacement v by q
```

In pure quaternion algebra: lift `v` to a pure quaternion $(v_x, v_y, v_z, 0)$, compute
$q \cdot v \cdot q^{-1}$, and read the vector part back. Plato keeps that inside `Transform` so
callers never hand-multiply $4×4$ expansions.

## Double cover: why `q` and `-q` are the same rotation

Unit quaternions live on the 3-sphere $S^3$ in $\mathbb{R}^4$. The map from $S^3$ to 3D rotations
is **two-to-one**: antipodal points encode the same physical orientation.

```
        S³ (all unit quaternions)
              |
              |  identify q ~ -q
              v
        SO(3) (3D rotations)
```

**Algebraic reason.** The conjugate $q^* = (-X, -Y, -Z, W)$ encodes the inverse rotation.
Applying $-q$ flips both factors:

$$
(-q)\, v\, (-q)^* = (-q)\, v\, (-q^*) = q\, v\, q^*
$$

So negating the quaternion leaves the sandwich product unchanged.

**Practical consequences:**

1. **Comparison.** `(0.0, 0.707, 0.0, 0.707)` and `(0.0, -0.707, 0.0, -0.707)` are the same
   orientation. Never compare quaternion components with `==`; compare the rotations they induce
   (via `AxisAngle`, a matrix, or a dot product test — see pitfalls).

2. **Interpolation.** When blending from `q_a` to `q_b`, the path through $S^3$ may be the long way
   ($> 180°$). `Slerp` picks the shorter arc by flipping the sign of one operand when their dot
   product is negative.

3. **The $720°$ fact.** Tracing a full $360°$ rotation in space returns the same orientation, but the
   quaternion may pick up a sign flip. Returning to the *same* quaternion components requires
   $720°$. This is not a bug — it reflects that $SO(3)$ is doubly covered by $S^3$. For game
   engines and pose blending you almost always care about orientation equivalence mod sign, not
   about quaternion path memory.

## Composition and conventions

Two rotations `a` then `b` (apply `a` first, then `b` on the result):

```
combined = a.Concatenate(b)
```

Plato splits two multiply APIs deliberately:

| Operation | Order meaning | Typical use |
|-----------|---------------|-------------|
| `a.Multiply(b)` | Hamilton product: **b first**, then a | Low-level algebra |
| `a.Concatenate(b)` | **a first**, then b | Scene-graph / pose chains |

The `Transforms` library doc comment states this explicitly so you do not reverse a chain by
accident. `Pose3D` composition uses `Concatenate` on orientations:

```
Compose(first, second) =>
    (first.Position.Transform(second),
     first.Orientation.Concatenate(second.Orientation))
```

Inverse rotation for a unit quaternion: `Conjugate` equals `Inverse`.

## In Plato

v3 declares `Quaternion` in `10-rotations.plato` as the canonical 3D rotation type:

```plato
// A unit quaternion encoding a 3D rotation. Invariant: X*X + Y*Y + Z*Z + W*W = 1.
type Quaternion
    implements Value, Multiplicative, Interpolatable
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}
```

Related rotation shapes in the same file:

```plato
type AxisAngle implements Value
{
    Axis: Direction3D;
    Angle: Angle;
}

type EulerAngles implements Value
{
    Yaw: Angle;
    Pitch: Angle;
    Roll: Angle;
    Order: RotationOrder;
}
```

`Quaternion` is the hub: `AxisAngle`, `EulerAngles`, `Rotor3D`, and `Matrix4x4` all convert through
it. Rigid placement types store it directly:

```plato
type Pose3D implements Value, Interpolatable
{
    Position: Point3D;
    Orientation: Quaternion;
}

type Transform3D implements Value
{
    Translation: Vector3D;
    Rotation: Quaternion;
    Scale: Number3;
}
```

### Building rotations

From axis and angle (axis must be a unit vector — use `Direction3D` when you have one):

```
var axis = Direction3D(Vector3D(0.0, 1.0, 0.0).Normalize);
var aa = AxisAngle(axis, 90.0.Angle);
var q = Quaternion(aa);
// equivalent factory:
var qY = Quaternion.CreateFromAxisAngle(Vector3D(0.0, 1.0, 0.0), 90.0.Angle);
```

Elemental world-axis rotations:

```
var qPitch = Quaternion.CreateRotationX(45.0.Angle);
var qYaw   = Quaternion.CreateRotationY(30.0.Angle);
var qRoll  = Quaternion.CreateRotationZ(15.0.Angle);
```

Chain them with explicit order (see `RotationOrder` and `ChainRotations` in `13-transforms.plato`):

```
var e = EulerAngles(30.0.Angle, 45.0.Angle, 15.0.Angle, RotationOrder.ZYX);
var qFromEuler = Quaternion(e);
```

### Applying and composing

```
var v = Vector3D(1.0, 0.0, 0.0);
var vRot = v.Transform(q);

var p = Point3D(2.0, 0.0, 0.0);
var pRot = p.Transform(q);

var qTotal = qA.Concatenate(qB);
var qBack  = q.Conjugate;    // inverse when unit length
```

Pose with oriented position:

```
var pose = Pose3D(Point3D(1.0, 2.0, 3.0), q);
var worldPoint = someLocalPoint.Transform(pose);
```

### Interpolation

`Quaternion` implements `Interpolatable`:

```
var halfway = qStart.Slerp(qEnd, 0.5);   // constant angular speed on S³
var naive   = qStart.Lerp(qEnd, 0.5);    // component lerp; does not preserve unit length
```

For full rigid poses, `Pose3D.Lerp` linearly blends position and spherically blends orientation:

```
var midPose = poseA.Lerp(poseB, t);
```

Prefer `Slerp` for orientations alone; use `Normalize` after `Lerp` if you must lerp components.

### Rotor isomorphism (optional mental model)

`Rotor3D` stores `(Scalar, Bivector3D)` — geometric-algebra phrasing of the same object.
Conversion is a field shuffle, not new math:

```
var r = Rotor3D(q);
var back = Quaternion(r);
```

The `Transforms` library maps bivector planes to quaternion vector components under the
isomorphism documented in `13-transforms.plato`. When you already think in bivectors, use
`Rotor3D`; when you compose poses and TRS transforms, use `Quaternion`.

## Pitfalls and fine print

**Normalization drift.** After many `Multiply` calls, $X^2+Y^2+Z^2+W^2$ may deviate from 1. A
slightly non-unit quaternion still rotates, but scales vectors subtly. Re-normalize before long
chains or before converting to `Matrix4x4`.

**Do not add rotations.** `Add` exists on quaternions for algebraic completeness, but
`qStart.Add(qDelta)` is not "apply a small rotation." Small angular updates belong in axis-angle
or tangent-space APIs, not component addition.

**Euler conversion is lossy at poles.** `EulerAngles(q)` documents gimbal-lock ambiguity when
pitch is $\pm 90°$. Prefer storing `Quaternion` (or `AxisAngle`) as the source of truth; treat
Euler as an authoring/export view.

**Handedness and axis direction.** `AxisAngle` uses `Direction3D` with the right-hand rule.
Negating the axis negates the angle's effect; combined with double cover, several quaternion
 tuples may represent one physical rotation.

**Shortest-path interpolation.** If `qStart.Dot(qEnd) < 0`, negate one quaternion before
`Slerp` — or rely on `Slerp` doing so internally (the intrinsic implementation should; verify
in your backend). Otherwise you spin the long way.

**Transform vs vector semantics.** `Transform(v, q)` rotates a displacement. Translation
components of poses are applied separately in `Transform(p, pose)`. Do not confuse rotating a
point about the origin with rotating a pose's position vector.

## Try it

<details>
<summary>Exercise 1 — Read the numbers</summary>

An object stores `Quaternion(0.0, 0.0, 0.866, 0.5)` (approximately). What axis and angle does
this represent?

**Answer.** Unit check: $0.866^2 + 0.5^2 \approx 1$. Vector part is $(0,0,0.866) \parallel +Z$.
$\cos(\theta/2) = 0.5 \Rightarrow \theta/2 = 60° \Rightarrow \theta = 120°$. Right-hand rule:
$120°$ counterclockwise when looking down $+Z$.
</details>

<details>
<summary>Exercise 2 — Same rotation, different signs</summary>

`q1 = Quaternion(0.0, 0.707, 0.0, 0.707)` and `q2 = Quaternion(0.0, -0.707, 0.0, -0.707)`.
True or false: `Transform(v, q1)` and `Transform(v, q2)` yield the same vector for every `v`.

**Answer.** True. `q2 = -q1`; double cover guarantees identical action on vectors.
</details>

<details>
<summary>Exercise 3 — Composition order</summary>

Start with `Vector3D(1, 0, 0)`. Apply a $90°$ rotation about Y, then a $90°$ rotation about Z.
Which expression matches "Y first, then Z"?

A) `qY.Multiply(qZ)`  
B) `qY.Concatenate(qZ)`  
C) `qZ.Concatenate(qY)`

**Answer.** B. `Concatenate` applies the receiver first: `qY.Concatenate(qZ)` applies Y, then Z.
Under `Multiply`, `qY.Multiply(qZ)` applies Z first (Hamilton order).
</details>

## Library recommendations

- **doc-comment** — `10-rotations.plato`: `Quaternion`'s doc states the unit-length invariant but
  not the double-cover equivalence (`q` and `Negative(q)` encode the same rotation). That fact is
  the single most common source of confusion for new users and should live on the type declaration.

- **doc-comment** — `10-rotations.plato`: `Rotor3D` says it is "structurally a quaternion" but does
  not note the component permutation `(Scalar, YZ, ZX, XY) ↔ (W, X, Y, Z)` documented only in
  `13-transforms.plato`. Hoist the isomorphism into the `Rotor3D` comment so readers of
  `10-rotations.plato` alone are not misled about field order.

- **missing-function** — `10-rotations.plato` / `Quaternion`: no declared `ApproximatelySameRotation(a, b, tolerance)`
  or `DotAbs` helper for orientation comparison mod sign. Every consumer must rediscover the
  `Dot(a,b) < 0` negation trick and the fact that component equality is wrong; a named predicate
  on the rotation types would encode the double cover in the API surface.

- **pedagogy** — `70-intrinsics.plato` hosts `CreateFromAxisAngle`, `Slerp`, and `Concatenate`,
  while `10-rotations.plato` declares the type with no `library` block. For teaching and
  discoverability, either mirror the key factories on a `library Rotations` beside the types, or
  cross-reference in the file banner — authors grep `10-rotations.plato` first and miss the hub
  conversions living in `13-transforms.plato`.
