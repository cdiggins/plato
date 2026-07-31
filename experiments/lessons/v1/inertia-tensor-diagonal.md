---
lesson: inertia-tensor-diagonal
title: Diagonalizing the Inertia Tensor
domain: Physics & simulation
v3-files: [54-rigid-dynamics.plato, 09-matrices.plato]
audience: Basic rigid-body physics; vectors and matrices; no continuum mechanics required
status: draft-v1
---

# Diagonalizing the Inertia Tensor

A spinning rigid body does not always rotate about the axis you pushed. Kick a box off-
center and the angular velocity vector tumbles in the body frame. The object that
encodes "how mass resists spinning about each axis" is the **inertia tensor** — a
symmetric $3\times 3$ matrix. In the right coordinates — the **principal axes** — that
matrix becomes diagonal, and Euler's equations simplify to three scalar ODEs.

This lesson is about what those six numbers mean, why symmetry matters, and what it
means to diagonalize them — using Plato's `SymmetricMatrix3x3` and `MassProperties3D`.

## The idea

### Angular momentum and the tensor

For a rigid body, angular momentum $\mathbf{L}$ and angular velocity $\boldsymbol{\omega}$
(both about the center of mass) are related by a linear map:

$$
\mathbf{L} = \mathbf{I}\,\boldsymbol{\omega}.
$$

$\mathbf{I}$ is the inertia tensor. In components about body axes,

$$
\mathbf{I}
=
\begin{pmatrix}
I_{xx} & I_{xy} & I_{xz} \\
I_{xy} & I_{yy} & I_{yz} \\
I_{xz} & I_{yz} & I_{zz}
\end{pmatrix}.
$$

The diagonal entries $I_{xx}, I_{yy}, I_{zz}$ are **moments of inertia** about the
coordinate axes. The off-diagonal entries $I_{xy},\ldots$ are **products of inertia**.
They vanish when the axes align with the body's symmetry — and in general they do not.

### Why six numbers, not nine

Inertia tensors are **symmetric**: $I_{xy}=I_{yx}$, etc. You only store six unique
values. That is not a compression trick; it follows from the definition

$$
I_{ij} = \int (\|\mathbf{r}\|^2\delta_{ij} - r_i r_j)\,\mathrm{d}m.
$$

Kinetic energy $\tfrac12\boldsymbol{\omega}^{T}\mathbf{I}\boldsymbol{\omega}$ is a
quadratic form; quadratic forms correspond to symmetric matrices.

### Principal axes

Because $\mathbf{I}$ is real symmetric, it is orthogonally diagonalizable: there exists
a rotation $R$ (principal frame) such that

$$
R^{T}\mathbf{I} R
=
\operatorname{diag}(I_1, I_2, I_3).
$$

$I_1,I_2,I_3$ are the **principal moments**. In that frame, products of inertia are
zero, and

$$
L_1 = I_1\omega_1,\quad
L_2 = I_2\omega_2,\quad
L_3 = I_3\omega_3.
$$

Bodies with three equal principal moments (a solid ball about its center) behave
isotropically: $\mathbf{L}$ stays parallel to $\boldsymbol{\omega}$ in the body frame.

### Parallel-axis shift (sanity check)

If you know inertia about the center of mass and move to a parallel axis through a
point displaced by $\mathbf{c}$, the tensor changes by a standard correction (parallel-
axis / Steiner theorem). Simulation code usually stores inertia **about the center of
mass in body space**, then transforms to world space with the body's orientation each
step — not by rebuilding from scratch.

### World vs body

`MassProperties3D` stores the tensor in **body space**. To apply $\mathbf{L}=I\boldsymbol{\omega}$
in world coordinates you need

$$
I_{\mathrm{world}} = R\, I_{\mathrm{body}}\, R^{T},
$$

where $R$ is the body's rotation matrix (from `Pose3D.Orientation`). Diagonalization is
almost always done once in body space offline; at runtime you rotate the tensor, not
re-diagonalize every frame.

## In Plato

### Symmetric storage

From `09-matrices.plato`:

```plato
// A symmetric 3x3 matrix stored as its six unique elements: inertia tensors,
// covariance, stress and strain states.
type SymmetricMatrix3x3
    implements Value
{
    M11: Number; M12: Number; M13: Number;
    M22: Number; M23: Number;
    M33: Number;
}
```

Mapping to inertia notation:

| Field | Inertia component |
|-------|-------------------|
| `M11` | $I_{xx}$ |
| `M12` | $I_{xy}$ |
| `M13` | $I_{xz}$ |
| `M22` | $I_{yy}$ |
| `M23` | $I_{yz}$ |
| `M33` | $I_{zz}$ |

There is no declared `ToMatrix3x3` / `FromSymmetric`, no `Eigenvalues`, and no
`Diagonalize` on this type in v3.

### Mass properties on a rigid body

From `54-rigid-dynamics.plato`:

```plato
// The inertial properties of a spatial rigid body: total mass, center of mass
// in the body's local frame (meters), and the inertia tensor about the center
// of mass in body space, in kilogram-square-meters.
type MassProperties3D
    implements Value
{
    Mass: Mass;
    CenterOfMass: Point3D;
    InertiaTensor: SymmetricMatrix3x3;
}

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

Contrast 2D: `MassProperties2D` stores a single `MomentOfInertia` scalar — the out-of-
plane moment — because planar bodies only spin about one axis. The 3D case needs the
full tensor.

`Mass` and `MomentOfInertia` are quantity types from `06-quantities.plato`
(`Kilograms`, `KilogramSquareMeters`). The tensor entries are bare `Number` inside
`SymmetricMatrix3x3` — the doc comment on `MassProperties3D` supplies the unit.

### Usage-shaped: a box aligned with body axes

For a solid box of mass $m$ and full side lengths $(w,h,d)$ about its center, the
principal tensor is already diagonal:

$$
I_{xx}=\tfrac{m}{12}(h^2+d^2),\;
I_{yy}=\tfrac{m}{12}(w^2+d^2),\;
I_{zz}=\tfrac{m}{12}(w^2+h^2).
$$

```plato
var m = 12.0; // kilograms, illustrative
var w = 2.0; var h = 1.0; var d = 4.0;
var inertia = SymmetricMatrix3x3 {
    M11: m / 12.0 * (h * h + d * d),
    M12: 0.0,
    M13: 0.0,
    M22: m / 12.0 * (w * w + d * d),
    M23: 0.0,
    M33: m / 12.0 * (w * w + h * h)
};
var props = MassProperties3D {
    Mass: Mass { Kilograms: m },
    CenterOfMass: Point3D { X: 0, Y: 0, Z: 0 },
    InertiaTensor: inertia
};
```

Products of inertia are zero because the box faces are aligned with the body axes —
those axes *are* principal.

### What diagonalization would look like (not declared)

Given a general `SymmetricMatrix3x3`, the missing API shape is:

```plato
// NOT in v3 — illustrative wish:
// Diagonalize(I) -> (principalMoments: Number3, principalFrame: Quaternion)
```

The quaternion would rotate body axes into principal axes; if you bake that rotation
into the collision shape's local pose, you can store a diagonal tensor at runtime and
save multiplies.

### Angular velocity on the body

`RigidBody3D.AngularVelocity` is a `Vector3D` in radians per second about world axes
(per the file comment). Integrating torque requires either transforming $\boldsymbol{\omega}$
into body space, applying $I^{-1}$ there, or working with $I_{\mathrm{world}}^{-1}$.
Either path needs an inverse of the symmetric tensor — also undeclared.

## Pitfalls / fine print

**Forgetting products of inertia.** Using only `(M11,M22,M33)` as if the tensor were
diagonal when the mesh is not aligned with body axes produces wrong torque response —
objects "mysteriously" wobble.

**Center of mass offset.** The tensor in `MassProperties3D` is about `CenterOfMass`, not
about the pose origin. If your mesh origin is not the COM, do not feed raw vertex
inertia about the origin into this field without a parallel-axis correction.

**Singular inertia.** A thin rod idealized with zero radial mass has a zero principal
moment about its long axis — $I^{-1}$ blows up. Real engines clamp or use a minimum
inertia.

**2D vs 3D fields.** Do not put a `SymmetricMatrix3x3` on a `RigidBody2D`; the planar
type wants `MomentOfInertia`.

**Units in `Number` slots.** `SymmetricMatrix3x3` is shared with covariance and stress.
When it rides inside `MassProperties3D`, treat entries as kg·m² even though the fields
are plain `Number`.

## Try it

1. For a sphere of mass $m$ and radius $r$, the inertia about the center is
   $\tfrac{2}{5}mr^2$ on each diagonal with zero products. Write the
   `SymmetricMatrix3x3` fields.

2. Why does diagonalizing `InertiaTensor` once at authoring time still require a world-
   space transform every simulation step?

3. A body has large $I_{zz}$ and small $I_{xx}, I_{yy}$. About which body axis is it
   hardest to start spinning?

<details>
<summary>Answers</summary>

1. `M11 = M22 = M33 = 2/5 m r²`, `M12 = M13 = M23 = 0`.

2. The principal frame is fixed in the *body*. As `Pose.Orientation` changes, the map
   from world $\boldsymbol{\omega}$ to body coordinates changes — rotate the tensor (or
   the vectors), do not assume world axes stay principal.

3. The $Z$ axis — larger moment means more angular momentum (and more torque) for the
   same $\omega$.

</details>

## Library recommendations

- **missing-function** — `09-matrices.plato`: `SymmetricMatrix3x3` has no
  `Eigenvalues` / `Diagonalize` / `Inverse` / `Multiply(SymmetricMatrix3x3, Vector3D)`.
  Rigid-body lessons cannot even *state* principal-axis extraction in declared ops.

- **missing-function** — `54-rigid-dynamics.plato`: no
  `PrincipalMoments(props: MassProperties3D): Number3` or
  `AlignToPrincipalAxes` helper that returns a corrected local pose + diagonal tensor.

- **naming** — `09-matrices.plato`: fields `M11…M33` are generic; when used as inertia,
  doc comments on `MassProperties3D.InertiaTensor` should restate the $I_{xx}$ mapping
  (partially present in the mass-properties comment, absent on the matrix type).

- **missing-type** — `54-rigid-dynamics.plato`: a `PrincipalInertia` record
  `{ Moments: Number3; Frame: Quaternion }` would make the diagonal form first-class
  instead of leaving authors to overload `SymmetricMatrix3x3` with near-zero off-
  diagonals.
