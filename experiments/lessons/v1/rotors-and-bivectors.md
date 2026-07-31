---
lesson: rotors-and-bivectors
title: Rotors and Bivectors
domain: Rotations
v3-files: [10-rotations.plato]
audience: Comfortable with 2D/3D vectors and the idea that rotations compose; no prior geometric algebra required.
status: draft-v1
---

# Rotors and Bivectors

The cross product $\mathbf{a} \times \mathbf{b}$ is a beloved 3D trick: it builds
a vector perpendicular to a plane. It also does not exist in 2D the same way,
does not exist in 4D as a single vector, and quietly confuses "plane of rotation"
with "axis of rotation."

**Geometric algebra** splits those ideas. An oriented plane element is a
**bivector**. A rotation is a **rotor** — a scalar plus a bivector — applied with
a sandwich product. In 3D, rotors are isomorphic to unit quaternions; the payoff
is a language that still works when you leave three dimensions, and that names
the *plane* you rotate in rather than only the axis perpendicular to it.

## The idea

### Bivectors: oriented area

A vector is an oriented length. A **bivector** is an oriented area — think of a
little parallelogram with a sense of circulation.

In 2D there is only one plane (the plane of the page). So a 2D bivector has a
single component:

$$
B = B_{xy}\; e_x \wedge e_y
$$

In 3D there are three independent plane directions, matching the three ways to
pick two axes:

$$
B = B_{yz}\, e_y\wedge e_z
+ B_{zx}\, e_z\wedge e_x
+ B_{xy}\, e_x\wedge e_y
$$

```
   2D: one plane element          3D: three plane elements
        +----→                         YZ, ZX, XY
        | ####  B_xy                   (dual to X, Y, Z axes)
        ↓ ####
```

The **duality in 3D**: the bivector with components $(B_{yz}, B_{zx}, B_{xy})$
corresponds to the axis vector $(B_{yz}, B_{zx}, B_{xy})$ — the normal to the
plane. That is why quaternions can store "axis-like" data in their vector part
while rotors store "plane-like" data: same three numbers, dual interpretation.

### Rotors: rotate in a plane

A **rotor** for a rotation by angle $\theta$ in the plane of unit bivector $B$
(with $B^2 = -1$ in the geometric product) is:

$$
R = \cos\tfrac{\theta}{2} + B\sin\tfrac{\theta}{2}
$$

Same half-angle pattern as unit quaternions and unit complex numbers. Apply to a
vector with the **sandwich**:

$$
\mathbf{v}' = R\,\mathbf{v}\,\widetilde{R}
$$

where $\widetilde{R}$ is the reverse (negate the bivector part). The half angles
compose so the net turn is $\theta$, not $\theta/2$.

In 2D, $R = (\cos(\theta/2),\; \sin(\theta/2))$ is exactly a unit complex number.
In 3D, $R = (\text{scalar},\; \text{bivector})$ matches a quaternion under the
component map Plato documents:

$$
(w, x, y, z) = (\text{Scalar},\; B_{yz},\; B_{zx},\; B_{xy})
$$

### Why bother if quaternions exist?

- **Plane-first thinking:** mirrors, orbits, and "spin in the plane spanned by
  these two vectors" are bivector statements; axes are the dual.
- **Uniform 2D/3D story:** `Rotor2D` and `Rotor3D` are the same idea; cross
  products are not.
- **Path to motors:** rigid motions in geometric algebra extend rotors to
  **motors** (rotation + translation) without bolting on a separate dual
  quaternion story — though Plato also exposes `Motor3D` in dual-quaternion
  form for practical interop.

You can use quaternions every day and still benefit from reading rotors when a
paper or algorithm speaks GA.

## In Plato

```plato
type Bivector2D
    implements Value
{
    XY: Number;
}

type Bivector3D
    implements Value
{
    YZ: Number;
    ZX: Number;
    XY: Number;
}

// Equivalent to a unit complex number.
type Rotor2D
    implements Value, Multiplicative
{
    Scalar: Number;
    XY: Number;
}

// Structurally a quaternion, expressed in plane-based components.
type Rotor3D
    implements Value, Multiplicative
{
    Scalar: Number;
    Bivector: Bivector3D;
}
```

Building and converting:

```plato
let spin = Rotation2D { Angle: (0.5235988).Angle };  // 30°
let r2 = Rotor2D(spin);
// Scalar = cos(15°), XY = sin(15°)

let aa = AxisAngle {
    Axis: Direction3D(Vector3D { X: 0, Y: 0, Z: 1 }),
    Angle: (0.5235988).Angle  // 30°
};
let r3 = Rotor3D(aa);
// rotation in the XY plane: Bivector.XY dominates

let q = Quaternion(r3);
let rAgain = Rotor3D(q);
// isomorphism: (w,x,y,z) ↔ (Scalar, YZ, ZX, XY)
```

Composition and inverse follow the quaternion conventions through that map:

```plato
let combined = a.Multiply(b);
// geometric product; a.Multiply(b) applies b first (Hamilton order)

let undo = Inverse(r3);
// reverse: negate bivector part

let unit = Normalize(r3);
// restore Scalar² + |B|² = 1 after drift
```

Applying to geometry:

```plato
let v2 = Vector2D { X: 1, Y: 0 }.Transform(r2);
let p3 = point.Transform(r3);
// sandwich R v ~R under the hood
```

Identity rotors:

```plato
let id2 = Rotor2D.Identity;  // (1, 0)
let id3 = Rotor3D.Identity;  // (1, Bivector3D(0,0,0))
```

## Pitfalls / fine print

**Forgetting the sandwich.** Multiplying $R\mathbf{v}$ once (as if $R$ were a
matrix) is wrong. Vectors are rotated by $R v \widetilde{R}$. Plato's
`Transform(v, r)` encapsulates this; raw products are for composing rotors with
rotors.

**Mixing axis and plane pictures.** A positive rotation about $+Z$ is a positive
spin *in* the $XY$ plane. The bivector $e_x\wedge e_y$ generates that spin; the
axis is its dual. Sign errors often come from flipping one picture without the
other.

**Non-unit rotors.** After many `Multiply` calls, renormalize. The sandwich with
a non-unit rotor scales as well as rotates.

**Assuming bivectors are "just vectors."** In 3D the component count matches, but
under change of basis and in dimensions other than 3 the objects diverge. Keep
the type: `Bivector3D`, not `Vector3D`.

**2D angle extraction.** `Rotation2D(r: Rotor2D)` uses a double-angle recovery.
Near ambiguous branch cuts, prefer composing in rotor form and converting once
at the boundary.

## Try it

1. A `Rotor2D` with `Scalar = XY = \sqrt{2}/2`. What full rotation angle does it
   encode?
2. Under Plato's map, which quaternion components correspond to a pure $XY$
   bivector rotation?
3. Why does `Bivector2D` have one field while `Bivector3D` has three?

<details>
<summary>Answers</summary>

1. Half-angle $45°$, full angle $90°$ (quarter turn).
2. Scalar → $W$, $XY$ → $Z$, and $YZ=ZX=0$ → $X=Y=0$. So $(0,0,z,w)$.
3. There is one independent plane in 2D space and three in 3D space
   ($\binom{n}{2}$ plane directions in $n$ dimensions).

</details>

## Library recommendations

- **missing-function** — `10-rotations.plato`: no constructor
  `Rotor3D(plane: Bivector3D, angle: Angle)` (normalize plane, half-angle
  formula). The GA teaching path wants plane+angle; today you must go
  axis-angle or quaternion first.

- **missing-function** — `10-rotations.plato`: `Bivector3D` has no
  `Dual: Vector3D` / `FromDual(Vector3D)` pair documenting the 3D isomorphism
  used in the quaternion map. The lesson has to state the duality in prose
  without a named API.

- **doc-comment** — `10-rotations.plato`: `Rotor3D` says it is "structurally a
  quaternion" but does not spell the component order `(Scalar, YZ, ZX, XY) ↔
  (W, X, Y, Z)` on the type. That map currently lives in the transforms
  library conversion — it belongs on the type banner for GA readers.

- **missing-concept** — `10-rotations.plato`: no shared `Rotor` concept tying
  `Rotor2D`/`Rotor3D` (sandwich `Transform`, `Inverse` as reverse, `Normalize`).
  Generic GA code and this lesson's "same idea in 2D and 3D" claim would use it.
