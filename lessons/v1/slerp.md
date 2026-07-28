---
lesson: slerp
title: Spherical Linear Interpolation
domain: Rotations
v3-files: [02-concepts-algebra.plato, 10-rotations.plato]
audience: Knows what a quaternion orientation is at a high level (four numbers on a sphere); comfortable with ordinary lerp of scalars and vectors.
status: draft-v1
---

# Spherical Linear Interpolation

You animate a camera from one orientation to another. At $t = 0$ it faces north;
at $t = 1$ it faces east. What should it do at $t = 0.5$?

If you blend the four quaternion components with ordinary linear interpolation
and renormalize, the camera does turn — but not at constant angular speed. It
lingers near the endpoints and races through the middle (or the reverse,
depending on the angle). The path on the orientation sphere is a chord, not an
arc. **Slerp** — spherical linear interpolation — walks the short great-circle
arc at constant angle rate. That is the blend you usually want for rotations.

## The idea

### Ordinary lerp

The `Interpolatable` concept captures the straight-line blend:

$$
\mathrm{lerp}(a,b,t) = a + (b - a)\,t
$$

At $t=0$ you get $a$; at $t=1$ you get $b$; outside $[0,1]$ you extrapolate.
For points in Euclidean space this is perfect: constant velocity along a chord
that *is* the geodesic.

Unit quaternions live on the **3-sphere** $S^3$ (the set of $(x,y,z,w)$ with
unit length). The Euclidean chord between two unit quaternions cuts inside the
sphere. Renormalizing projects back, but the parameter $t$ no longer means
"fraction of angle." Angular speed varies.

```
        b
       /|
      / |   chord = lerp path (then normalize)
     /  |
    a---·   arc = slerp path (constant angle rate)
```

### Slerp on a sphere

Let $q_1$ and $q_2$ be unit quaternions and $\Omega$ the angle between them
from the 4D dot product:

$$
\cos\Omega = q_1 \cdot q_2
$$

Then:

$$
\mathrm{slerp}(q_1, q_2, t)
=
\frac{\sin((1-t)\Omega)}{\sin\Omega}\, q_1
+
\frac{\sin(t\Omega)}{\sin\Omega}\, q_2
$$

When $\Omega$ is tiny, this formula is numerically fragile ($\sin\Omega\approx 0$);
implementations fall back to normalized lerp (nlerp) for near-parallel inputs.

Properties you care about:

| Property | Lerp + normalize | Slerp |
|----------|------------------|-------|
| Ends at $q_1$, $q_2$ | yes | yes |
| Stays unit (approx.) | after normalize | yes |
| Constant angular speed | no | yes |
| Shortest path | if you flip signs | if you flip signs |

### The shortest-path flip

$q$ and $-q$ are the **same** rotation (the double cover of $SO(3)$). The angle
between $q_1$ and $q_2$ might be obtuse while the angle between $q_1$ and $-q_2$
is acute. Slerp along the long way spins the long way — a $350°$ turn instead of
$10°$.

Fix: if $q_1 \cdot q_2 < 0$, replace $q_2$ with $-q_2$ before blending. Good
`Slerp` implementations do this; verify when you wrap a backend.

### Nlerp as a cheap cousin

Normalize$(\mathrm{lerp}(q_1,q_2,t))$ — **nlerp** — is cheaper and often "good
enough" for small angles. It does not preserve constant speed. Prefer slerp for
cinematic cameras and long blends; nlerp can be acceptable for many game ticks
with small deltas.

### Why `Interpolatable` is not enough

`Interpolatable` only declares `Lerp`. That is the right default for vectors,
colors, and points. Rotations need a different geodesic. Plato therefore puts
`Slerp` on `Quaternion` specifically rather than pretending every interpolatable
type has a spherical metric.

## In Plato

From `02-concepts-algebra.plato`:

```plato
// Supports linear interpolation. The parameter t is unclamped: 0 yields a,
// 1 yields b, values outside [0,1] extrapolate.
concept Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}
```

`Quaternion` implements `Interpolatable` (so `Lerp` exists) **and** exposes
spherical blend as an intrinsic:

```plato
// From 70-intrinsics.plato (Quaternion section)
Lerp(self: Quaternion, quaternion2: Quaternion, amount: Number): Quaternion;
Slerp(self: Quaternion, quaternion2: Quaternion, amount: Number): Quaternion;
Dot(self: Quaternion, quaternion2: Quaternion): Number;
Normalize(self: Quaternion): Quaternion;
```

Usage-shaped expressions:

```plato
let start = Quaternion.CreateFromAxisAngle(
    Vector3D { X: 0, Y: 1, Z: 0 }, (0.0).Angle);
let end = Quaternion.CreateFromAxisAngle(
    Vector3D { X: 0, Y: 1, Z: 0 }, (1.5707963).Angle);  // 90°

// Constant-speed quarter turn about Y
let mid = start.Slerp(end, 0.5);
// ≈ 45° about Y

// Wrong tool for constant angle rate:
let midChord = start.Lerp(end, 0.5).Normalize;
// same endpoints, different intermediate angular speed
```

Dot product for the flip test and angle:

```plato
let cosOmega = start.Dot(end);
// if cosOmega < 0, the long arc is the default geodesic —
// rely on Slerp's implementation to negate, or do it yourself:

let endShort = if cosOmega < 0.0 then end.Negative else end;
let midShort = start.Slerp(endShort, t);
```

Pose interpolation already chooses slerp for orientation:

```plato
// Pose3D.Lerp in the transforms library:
//   position → Lerp, orientation → Slerp
let midPose = Lerp(poseA, poseB, t);
```

That is the pattern to copy: Euclidean blend where the space is flat, spherical
blend where the space is a rotation group.

`Rotation2D` is different — its `Lerp` blends stored angles on the line, not
necessarily the short arc. For planar spins you often want explicit angle
wrapping; for 3D orientations, reach for `Slerp`.

## Pitfalls / fine print

**Calling `Lerp` on quaternions out of habit.** It compiles — `Quaternion` is
`Interpolatable`. It is still the wrong default for orientation animation.

**Forgetting the double cover.** If your keyframes alternate $q$ and $-q$ for
"the same" pose, slerp may take the long way between keys. Continuously flip
signs along a track so consecutive dots stay non-negative.

**Extrapolating with $t \notin [0,1]$.** Both `Lerp` and `Slerp` take unclamped
$t$ in Plato's design philosophy. Extrapolating slerp past the endpoints
continues along the same great circle — useful for overshoot; surprising if you
expected clamping.

**Near-identical inputs.** When $|\Omega|$ is tiny, use the nlerp fallback (or
trust the intrinsic to do so). Homegrown slerp without that guard yields NaNs.

**Blending more than two orientations.** Slerp is binary. Multiple keyframes
need a curve scheme (squad, logarithmic interpolation, etc.). v3 does not
declare squad; sequence of slerps between adjacent keys is the simple approach.

**Thinking slerp applies to Euler angles.** Slerp is defined on the quaternion
sphere (or equivalently on rotors). Convert Euler angles to `Quaternion` first.

## Try it

1. $q_1 = (0,0,0,1)$ (identity), $q_2$ a $180°$ rotation about $X$. What is
   qualitatively wrong with taking `Lerp` at $t=0.5` without care?
2. Why does $q_1 \cdot (-q_1) = -1$ even though both represent the same
   orientation?
3. `Pose3D.Lerp` uses `Slerp` for orientation. If it used `Quaternion.Lerp`
   instead, what visible artifact would a slow camera pan show?

<details>
<summary>Answers</summary>

1. The midpoint of the chord through the sphere, even after normalize, does not
   sit at $90°$ of rotation about $X$ with constant-speed parameterization —
   and at exactly $180°$ some lerp paths pass near zero length and become
   unstable.
2. The 4D Euclidean dot product sees opposite points on $S^3$; the rotation
   group identifies them. Dot product is not "sameness of orientation."
3. Uneven angular speed: the pan would ease incorrectly even with linear $t$,
   looking like a poorly timed ease curve.

</details>

## Library recommendations

- **missing-concept** — `02-concepts-algebra.plato`: `Interpolatable` only has
  `Lerp`. A sibling concept such as `SphericallyInterpolatable` with
  `Slerp(a, b, t)` (implemented by `Quaternion`, maybe `Rotor3D`) would make the
  pose/animation choice discoverable from concepts instead of tribal knowledge
  that "quaternions use Slerp."

- **doc-comment** — `70-intrinsics.plato`: `Slerp` should state whether it
  performs the shortest-path sign flip and what it does for near-parallel
  inputs. This lesson cannot teach the contract from the declaration alone.

- **missing-function** — `10-rotations.plato`: `Rotor3D` and `Rotor2D` have
  `Multiply` / `Normalize` but no `Slerp`. They are isomorphic to unit complex /
  quaternion forms; animation code that prefers GA naming currently must
  convert to `Quaternion`, slerp, and convert back.

- **naming** — `70-intrinsics.plato`: `Lerp` on `Quaternion` is easy to grab by
  autocomplete when `Slerp` was intended. A doc comment on `Lerp` saying
  "chord blend; prefer Slerp for orientations" would match the teaching moral.
