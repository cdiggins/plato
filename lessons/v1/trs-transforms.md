---
lesson: trs-transforms
title: TRS Transforms
domain: Matrices & transforms
v3-files: [13-transforms.plato]
audience: High-school vectors and general programming / game-engine curiosity
status: draft-v1
---

# TRS Transforms

Almost every object in a 3D scene is placed with three knobs: where it sits
(translation), which way it faces (rotation), and how big it is (scale). That
trio is so common it gets its own type. Plato calls it `Transform3D` — a
translate-rotate-scale record with a fixed application order.

The catch is composition. Two TRS transforms, each perfectly happy alone, may
combine into something that is no longer a pure TRS (non-uniform scale plus
rotation produces shear in the product). Order is not a detail; it is the
definition.

## The idea

A **TRS** transform applies, in a stated order:

1. **Scale** — stretch axes (uniform or per-axis)
2. **Rotate** — change orientation
3. **Translate** — move the origin

So a local point $\mathbf{p}$ becomes

$$
\mathbf{p}' = R(S\mathbf{p}) + \mathbf{t}
$$

in schematic form (exact matrix layout depends on row/column convention).
Authoring likes this order because artists think "size, then turn, then place."

```
  local p  --S-->  scaled  --R-->  oriented  --T-->  world p'
```

**Composition.** Doing transform $A$ then $B$ means $B(A(p))$. If both are
TRS, the combined map is still affine, but the middle may no longer factor as
"one scale, one rotation, one translation" when scales are non-uniform —
rotation of a stretched box looks sheared in the parent's axes.

**Decomposition.** Going the other way: given a matrix, recover scale,
rotation, and translation (when possible). That is how editors show TRS
gizmos for a matrix that came from a parent chain.

**Rigid subset.** When scale is identically $(1,1,1)$, TRS collapses to a
rigid pose — position plus orientation — which *is* closed under composition.

## In Plato

From `13-transforms.plato`:

```plato
// A translate-rotate-scale transform in 3D, applied scale first, then rotation,
// then translation.
type Transform3D
    implements Value
{
    Translation: Vector3D;
    Rotation: Quaternion;
    Scale: Number3;
}

// A translate-rotate-scale transform in 2D, applied scale first, then rotation,
// then translation.
type Transform2D
    implements Value
{
    Translation: Vector2D;
    Rotation: Rotation2D;
    Scale: Number2;
}
```

The doc comments fix the order in the type banner — not a tribal convention.
Scale is `Number3` / `Number2` (per-axis factors), not a single `Number`, so
non-uniform scale is representable.

Rigid poses are the scale-free siblings:

```plato
type Pose3D
{
    Position: Point3D;
    Orientation: Quaternion;
}
```

Application and matrix form from the `Transforms` library:

```plato
// Apply the transform: scale, rotate, then translate.
Transform(p: Point3D, t: Transform3D): Point3D
    => p.PositionVector.Multiply(t.Scale)
        .Transform(t.Rotation)
        .Add(t.Translation)
        .ToPoint;

// Vectors get scale and rotation only (no translation).
Transform(v: Vector3D, t: Transform3D): Vector3D
    => v.Multiply(t.Scale).Transform(t.Rotation);

// The homogeneous matrix: S * R * T in row-vector order.
Matrix4x4(t: Transform3D): Matrix4x4
    => Matrix4x4.CreateScale(t.Scale)
        .Multiply(t.Rotation.Matrix4x4)
        .Multiply(Matrix4x4.CreateTranslation(t.Translation));
```

Composition is intentionally **not** declared on `Transform3D`. The library
comment states the type is not closed under composition; compose through
matrix or affine forms instead:

```plato
let a: Transform3D = ...;
let b: Transform3D = ...;
let combined = a.Matrix4x4.Multiply(b.Matrix4x4);  // A then B
// Optional recover:
let again = combined.Transform3D;  // via Decompose; may fail if sheared
```

Decompose path:

```plato
// Precondition: Decompose succeeds; shear and projection are not representable.
Transform3D(m: Matrix4x4): Transform3D {
    var d = m.Decompose;
    return (d.X2, d.X1, d.X0);  // translation, rotation, scale
}
```

Widening / narrowing with poses:

```plato
Transform3D(pose: Pose3D): Transform3D
    => (pose.Position.PositionVector, pose.Orientation, Number3(1.0, 1.0, 1.0));

Pose(t: Transform3D): Pose3D
    => (t.Translation.ToPoint, t.Rotation);  // discards scale; explicit name
```

Identity:

```plato
let id = Transform3D.Identity;
// Translation 0, Quaternion identity, Scale (1,1,1)
```

Usage-shaped scene snippet:

```plato
let local = Point3D { X: 1.0, Y: 0.0, Z: 0.0 };
let trs = Transform3D {
    Translation: Vector3D { X: 10.0, Y: 0.0, Z: 0.0 },
    Rotation: Quaternion.Identity,
    Scale: Number3 { X: 2.0, Y: 2.0, Z: 2.0 }
};
let world = local.Transform(trs);  // (12, 0, 0): scale then translate
```

## Pitfalls / fine print

**Order is part of the type.** Other engines use T∗R∗S or different matrix
conventions. Reading Plato's `Transform` body — scale, rotate, translate — is
mandatory before porting formulas.

**Non-uniform scale + rotation.** Parenting a non-uniformly scaled object under
a rotated parent leaves the TRS family. Forcing the product back into
`Transform3D` via `Decompose` can fail or approximate; prefer
`AffineTransform3D` / `Matrix4x4` for the chain.

**Scale on vectors vs normals.** `Transform(v, t)` scales and rotates
displacements. Surface normals need the inverse-transpose of the linear part
under non-uniform scale — not this path.

**Lossy `Pose`.** `Pose(t: Transform3D)` drops scale on purpose and is named
`Pose`, not `Pose3D`, so the loss stays visible at the call site.

**Negative scale.** Mirrors and improper rotations show up as negative scale
factors; determinant sign and handedness flip. Decompose behavior around
reflections is engine-sensitive.

**2D vs 3D.** `Transform2D` uses `Rotation2D` and `Number2`; same S→R→T story,
smaller matrices (`Matrix3x2`).

## Try it

1. Point $(1,0,0)$, scale $(2,1,1)$, identity rotation, translation $(0,0,5)$.
   What is the image under `Transform3D`?
2. Why does Plato refuse a direct `Compose(Transform3D, Transform3D)`?
3. Starting from a `Pose3D`, how do you get a `Transform3D`, and what scale
   appears?

<details>
<summary>Answers</summary>

1. Scale → $(2,0,0)$; rotate → unchanged; translate → $(2,0,5)$.
2. TRS is not closed under composition when scales are non-uniform; the library
   pushes composition to matrices/affines that can represent the result.
3. `Transform3D(pose)` — unit scale `Number3(1,1,1)`.

</details>

## Library recommendations

- **missing-function** — `13-transforms.plato`: no `Compose` / `Inverse` on
  `Transform3D` (documented as intentional), but also no
  `TryCompose(a, b): Optional<Transform3D>` that succeeds when the product
  stays in-family (uniform scales, or compatible axes). Authors currently
  only see the negative space.

- **missing-function** — `13-transforms.plato`: `Transform3D(m: Matrix4x4)`
  assumes `Decompose` succeeded and unpacks `Tuple4` as `(X2,X1,X0)` without
  checking the Boolean. A `TryTransform3D(m): Optional<Transform3D>` matching
  the precondition comment would make failure teachable.

- **naming** — `Scale: Number3` is correct per the vector naming rule, but
  newcomers look for `Vector3D`. A doc comment on the field ("per-axis scale
  factors; not a geometric displacement") would prevent that wrong turn.

- **doc-comment** — `Transform(v: Vector3D, t: Transform3D)`: mention that
  normals are not displacements under non-uniform scale, so this is the wrong
  helper for lighting normals.
