---
lesson: inverse-transforms
title: Inverse Transforms
domain: Matrices & transforms
v3-files: [09-matrices.plato, 13-transforms.plato]
audience: High-school matrices and general programming / graphics curiosity
status: draft-v1
---

# Inverse Transforms

Every time you place an object in the world, something else needs the reverse
map: a mouse ray from screen to world, a child returning to parent space, an
undo stack. The inverse transform is "put it back." Not every machine has an
inverse — squashing space flat loses information — and even when one exists,
the *cheap* formula depends on what kind of transform you started with.

Rigid motions invert with a transpose and a negated translation. General
matrices need a full invert. Normals follow yet another rule.

## The idea

If $T$ maps points to points, an inverse $T^{-1}$ satisfies

$$
T^{-1}(T(p)) = p = T(T^{-1}(p))
$$

whenever $T$ is invertible. For linear maps, that is "non-zero determinant."
For affine maps, the linear part must be invertible; translation always undoes
by subtraction in the right space.

**Rigid (rotation + translation).** A rotation matrix is orthogonal:
$R^{-1} = R^{\mathsf{T}}$. Undoing a rigid pose is:

$$
p_{\text{local}} = R^{\mathsf{T}}(p_{\mathrm{world}} - \mathbf{t}).
$$

No general $4\times4$ Gauss-Jordan required — transpose and one subtract.

**Uniform scale.** Uniform scale $s$ inverts as $1/s$ (if $s \neq 0$).
Non-uniform scale inverts per-axis. Combined with rotation, invert the linear
block carefully (or invert the matrix).

**Normals are special.** Displacements transform by the linear part $L$.
Unit surface normals transform by $(L^{-1})^{\mathsf{T}}$ so angles with
tangent planes stay correct under non-uniform scale. Using $L$ on normals
shears lighting.

```
  Points:     p' = L p + t
  Vectors:    v' = L v
  Normals:    n' ∝ (L^{-1})^{T} n     (then renormalize)
```

**Composition.** $(AB)^{-1} = B^{-1} A^{-1}$ — reverse the order. Undo the
last operation first.

## In Plato

Matrices that can fail to invert (`09-matrices.plato` types, verbs in
intrinsics / library):

```plato
type Matrix4x4 implements MatrixLike, Multiplicative { ... }

// Intrinsics used by transforms:
Invert(self: Matrix4x4): Matrix4x4;
CanInvert(self: Matrix4x4): Boolean;
Transpose(self: Matrix4x4): Matrix4x4;
```

Affine wrappers in `13-transforms.plato`:

```plato
type AffineTransform3D { Matrix: Matrix4x3; }

// The inverse map. Precondition: CanInvert (a non-zero determinant).
Inverse(a: AffineTransform3D): AffineTransform3D
    => a.Matrix4x4.Invert.AffineTransform3D;

CanInvert(a: AffineTransform3D): Boolean
    => a.Matrix4x4.CanInvert;
```

Rigid poses get the cheap inverse:

```plato
// The pose that undoes this one: p' = R^-1 (p - t).
Inverse(pose: Pose3D): Pose3D {
    var invQ = pose.Orientation.Inverse;
    return (pose.Position.PositionVector.Negative.Transform(invQ).ToPoint, invQ);
}
```

Same pattern in 2D with `Pose2D` / `Rotation2D.Inverse` (negate angle).
Rotors and motors expose `Inverse` as reverse / dual-quaternion conjugate for
unit elements.

Identity and round-trip:

```plato
let p = Point3D { X: 3.0, Y: 1.0, Z: 0.0 };
let pose = Pose3D { ... };
let world = p.Transform(pose);
let back = world.Transform(pose.Inverse);  // ≈ p
```

Composition law in API form — `Compose(first, second)` means apply first then
second, so undoing uses reversed inverses:

```plato
let ab = Compose(a, b);
let undo = Compose(b.Inverse, a.Inverse);
```

Vectors vs points under affine maps:

```plato
Transform(p: Point3D, a: AffineTransform3D): Point3D  // uses translation row
Transform(v: Vector3D, a: AffineTransform3D): Vector3D // skips translation
```

There is `TransformNormal` on the 2D affine path (`Matrix3x2`); the 3D affine
`Transform` for vectors is the linear part only — **not** the
inverse-transpose normal rule. That gap matters for lighting.

Usage-shaped snippets:

```plato
let M = someMatrix4x4;
if (M.CanInvert) {
    let Minv = M.Invert;
    let p2 = /* map with Minv */;
}

let aff = AffineTransform3D { Matrix: someMatrix4x3 };
if (aff.CanInvert) {
    let undo = aff.Inverse;
}

// Rigid: prefer pose inverse over matrix invert
let parent = Pose3D.Identity;
let child = somePose;
let childInParent = child; // already local
let parentFromChild = child.Inverse;
```

## Pitfalls / fine print

**Singular maps.** Zero scale on an axis, projections, and degenerates make
`CanInvert` false. Calling `Invert` without the check is undefined territory.

**Normals ≠ vectors.** After non-uniform scale, transforming a normal like a
vector breaks diffuse lighting. Need inverse-transpose of the $3\times3$
linear block, then normalize.

**Inverse of TRS.** `Transform3D` has no declared `Inverse`. Invert via
`Matrix4x4` / `AffineTransform3D`, or invert pieces: $1/\mathrm{scale}$,
conjugate quaternion, then adjusted translation.

**Row-vector order.** `a.Multiply(b)` applies `a` then `b`. Inverses reverse
that sequence. Mixing column-vector chalkboard notes with Plato's multiply
flips every undo.

**Floating point.** $T^{-1}(T(p))$ is approximately $p$, not bit-exact.
Rigid inverses usually drift less than general `Invert` on messy matrices.

**Reflections.** Improper rotations (det $-1$) still invert, but flip
handedness; normals and winding need care.

## Try it

1. Pose: translation $(10,0,0)$, identity rotation. Where does world point
   $(12,0,0)$ go under `Inverse`?
2. Why is the rigid inverse cheaper than `Matrix4x4.Invert`?
3. If $A$ then $B$ were applied, in what order do you apply $A^{-1}$ and
   $B^{-1}$ to undo?

<details>
<summary>Answers</summary>

1. Subtract translation in the unrotated frame → $(2,0,0)$.
2. Rotation inverse is conjugate/transpose; no general $4\times4$ elimination.
3. $B^{-1}$ first, then $A^{-1}$ — reverse order of composition.

</details>

## Library recommendations

- **missing-function** — `13-transforms.plato`: 3D path has
  `Transform(v: Vector3D, a: AffineTransform3D)` (linear part) but no
  `TransformNormal(n: Vector3D, a: AffineTransform3D)` using
  $(L^{-1})^{\mathsf{T}}$. The 2D side already uses `TransformNormal` on
  `Matrix3x2`; 3D lighting needs the sibling.

- **missing-function** — `13-transforms.plato`: no `Inverse(t: Transform3D)`
  even for the common invertible case (nonzero per-axis scale). Authors must
  drop to matrices; a documented closed-form TRS inverse would match
  `Inverse(Pose3D)`.

- **missing-function** — `09-matrices.plato`: `Matrix3x3` has no declared
  `Invert` / `Transpose` / `CanInvert` beside what `Matrix4x4` gets in
  intrinsics. Normal transforms are $3\times3$ problems; the square linear
  block deserves the same verbs.

- **doc-comment** — `Inverse(pose: Pose3D)`: excellent formula in the comment;
  add one line that this is the map world→local when `pose` was local→world,
  tying frames and inverses together for API readers.
