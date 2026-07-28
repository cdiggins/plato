---
lesson: axis-angle
title: Axis-Angle Rotation
domain: Rotations
v3-files: [10-rotations.plato]
audience: Comfortable with 3D unit vectors and angles; no quaternion algebra required.
status: draft-v1
---

# Axis-Angle Rotation

Every non-trivial 3D rotation has an axis: a line through the origin that stays
put while everything else spins around it. Say that in one sentence — "turn 40°
about the surface normal" — and you have already used the **axis-angle**
representation. It is the most geometric of the common encodings: one unit
direction, one signed angle, right-hand rule.

Physics engines love it for angular velocity (spin rate about an axis).
Constraint solvers love it for "rotate about this hinge." It is less ideal as a
format for blending many orientations or composing long chains — that is where
quaternions and matrices earn their keep — but as a *description* of a single
spin, axis-angle is hard to beat.

## The idea

**Euler's rotation theorem:** any orientation of a rigid body with one point
fixed is equivalent to a single rotation about some axis through that point.
So every rotation (except the identity) has an axis-angle form.

Given a unit axis $\mathbf{u}$ and angle $\theta$ (radians in the formulas;
Plato stores an `Angle`):

- Points on the axis are fixed.
- Points in the plane perpendicular to $\mathbf{u}$ rotate by $\theta$.
- The right-hand rule: thumb along $\mathbf{u}$, fingers curl in the positive
  $\theta$ direction.

```
            u (thumb)
            ↑
            |
      ······|······  plane ⟂ u
     ╱      |      ╲
    ●───θ──→●       points sweep circles
```

### Rodrigues' formula

To rotate a vector $\mathbf{v}$ by axis-angle $(\mathbf{u}, \theta)$ without
leaving vector algebra:

$$
\mathbf{v}' =
\mathbf{v}\cos\theta
+ (\mathbf{u} \times \mathbf{v})\sin\theta
+ \mathbf{u}\,(\mathbf{u}\cdot\mathbf{v})\,(1 - \cos\theta)
$$

Three pieces:

1. Keep the part of $\mathbf{v}$ parallel to $\mathbf{u}$ (the last term rebuilds
   it with the right weight).
2. Rotate the perpendicular part with $\cos$ / $\sin$ in that plane.
3. The cross product supplies the in-plane direction orthogonal to $\mathbf{v}$.

This is the intuition behind many "apply rotation" implementations, even when
the stored form is a matrix or quaternion.

### Half-angle bridge to quaternions

The unit quaternion for the same rotation uses the **half** angle:

$$
q =
\bigl(\mathbf{u}\sin\tfrac{\theta}{2},\;
\cos\tfrac{\theta}{2}\bigr)
=
(u_x s,\; u_y s,\; u_z s,\; c)
\quad\text{with } s=\sin(\theta/2),\; c=\cos(\theta/2)
$$

Why half? So that quaternion multiplication corresponds to composing full-angle
rotations. Axis-angle is the human-facing statement; the half-angle form is the
algebraic one.

### Where it shines

- **Small rotations:** $\theta$ near 0, axis well-defined; angular displacement
  $\boldsymbol{\omega}\,\Delta t$ integrates naturally as axis-angle steps.
- **Hinges and constraints:** the axis is part of the problem statement.
- **Debugging:** printing $(\mathbf{u}, \theta)$ is readable; printing four
  quaternion components rarely is.
- **Logarithmic map:** the pure-vector quaternion $\theta\mathbf{u}$ (scaled
  axis) is the Lie-algebra picture of $SO(3)$ — useful in optimization and
  filtering.

### Where it struggles

- **Identity:** $\theta = 0$ makes the axis arbitrary. Any $\mathbf{u}$ works.
- **Composition:** converting two axis-angles to one is not a simple formula;
  you go through quaternions or matrices.
- **Interpolation:** lerping axes and angles separately is wrong when axes
  differ; again, prefer spherical interpolation in quaternion space.
- **Large angles:** $\theta$ and $\theta + 2\pi$ are the same rotation; so are
  $(\mathbf{u}, \theta)$ and $(-\mathbf{u}, -\theta)$. Pick a canonicalization
  if you compare values.

## In Plato

```plato
// A rotation of Angle about a unit Axis, following the right-hand rule.
type AxisAngle
    implements Value
{
    Axis: Direction3D;
    Angle: Angle;
}
```

`Direction3D` is a unit vector by construction — the type system carries the
axis invariant that a bare `Vector3D` would not. `Angle` prevents "was that
degrees?" bugs.

The quaternion hub conversions live with the other rotation maps:

```plato
// Build from axis-angle
let aa = AxisAngle {
    Axis: Direction3D(Vector3D { X: 0, Y: 1, Z: 0 }),
    Angle: (0.7853982).Angle  // 45°
};

let q = Quaternion(aa);
// == Quaternion.CreateFromAxisAngle(aa.Axis.Vector, aa.Angle)

let back = AxisAngle(q);
// angle = 2 acos(w), axis = normalized vector part
```

Near the identity, recovery is delicate — v3's conversion documents the policy:

```plato
// From the library: if the vector part is ~0, Axis becomes world +X
let tiny = AxisAngle(Quaternion.Identity);
// Angle ≈ 0, Axis = Direction3D(+X) by convention
```

Matrix form for pipelines that want homogeneous maps:

```plato
let m = Matrix4x4(aa);
// == Matrix4x4.CreateFromAxisAngle(aa.Axis.Vector, aa.Angle)

let p2 = p.Transform(q);      // via quaternion
let p3 = p.PositionVector      // or rotate the displacement
    .Transform(q)
    .ToPoint;
```

Geometric-algebra twin:

```plato
let r = Rotor3D(aa);
// same rotation; plane of rotation is perpendicular to Axis
```

The bivector of that rotor spans the plane *of* the rotation (perpendicular to
the axis). Axis-angle names the axis; rotors name the plane — dual views of one
fact.

## Pitfalls / fine print

**Non-unit axes.** If you construct from a raw vector, normalize first. Plato
pushes you toward `Direction3D`, but `CreateFromAxisAngle` on the intrinsic
takes `Vector3D` — check whether the backend normalizes or assumes unit length.

**Angle sign and handedness.** Right-hand rule is mandatory in Plato's docs.
Left-handed world coordinates (some engines) flip the effective sign; convert
carefully at boundaries.

**Comparing axis-angles for equality.** Prefer converting to quaternions and
testing $q \approx \pm q'$ (double cover). Separately comparing `Axis` and
`Angle` misses the $(u,\theta)\sim(-u,-\theta)$ equivalence and $2\pi$ wraps.

**Integrating angular velocity as Euler angles.** Angular velocity is naturally
an axis-rate (a 3-vector). Integrating it by adding to yaw/pitch/roll is a
classic source of instability; integrate in quaternion form or use exponential
maps from axis-angle increments.

**Extracting axis-angle from a matrix with scale.** Only a pure rotation
(orthonormal, det $= +1$) has a clean axis. Polar-decompose or orthonormalize
first.

## Try it

1. Axis $\mathbf{u} = (0,0,1)$, $\theta = 90°$. Where does $(1,0,0)$ go?
2. Show that $(\mathbf{u}, \theta)$ and $(-\mathbf{u}, -\theta)$ produce the
   same quaternion.
3. Why does `AxisAngle(Quaternion.Identity)` need a conventional axis?

<details>
<summary>Answers</summary>

1. Under the right-hand rule about $+Z$, $(1,0,0)$ maps to $(0,1,0)$.
2. $\sin(-\theta/2)(-\mathbf{u}) = \sin(\theta/2)\mathbf{u}$ and
   $\cos(-\theta/2)=\cos(\theta/2)$, so $(X,Y,Z,W)$ matches.
3. When $\theta=0$, every axis is fixed-pointwise; without a convention the
   conversion could return any unit vector. v3 picks world $+X$.

</details>

## Library recommendations

- **missing-function** — `10-rotations.plato` / transforms library: no
  `Transform(v: Vector3D, aa: AxisAngle): Vector3D` overload. Callers must
  convert to `Quaternion` or `Matrix4x4` first. Rodrigues is the teaching
  formula; having it as a direct apply would match how the representation is
  motivated.

- **missing-function** — `10-rotations.plato`: no canonicalization helper such
  as `Canonical(aa: AxisAngle): AxisAngle` that forces $\theta \in [0,\pi]$ and
  a hemisphere choice for the axis. Equality and debugging need this; the
  identity-axis convention already shows the gap.

- **doc-comment** — `70-intrinsics.plato`: `CreateFromAxisAngle(_: Quaternion,
  axis: Vector3D, angle: Angle)` should state whether `axis` is assumed unit or
  normalized internally. `AxisAngle` uses `Direction3D`; the intrinsic's
  `Vector3D` is the inconsistency this lesson keeps tripping on.

- **pedagogy** — `10-rotations.plato`: `AxisAngle` does not implement
  `Interpolatable`. That is correct (naive lerp is harmful), but a doc comment
  pointing to `Quaternion.Slerp` as the supported blend path would stop readers
  from inventing `Lerp` on axes and angles.
