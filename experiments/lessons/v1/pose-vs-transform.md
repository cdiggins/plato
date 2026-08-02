---
lesson: pose-vs-transform
title: Pose vs Transform
domain: Matrices & transforms
v3-files: [13-transforms.plato]
audience: Comfortable with 3D points, vectors, and the idea of orientation; no matrix algebra required beyond "a matrix maps points."
status: draft-v1
---

# Pose vs Transform

A character stands somewhere in a room, facing a door. You need two facts: *where*
they are, and *which way they face*. That pair — position plus orientation — is a
**pose**. You do not need a scale factor. You do not need shear. Distances between
joints on the character stay fixed; only the rigid placement changes.

Elsewhere in the same scene, a prop artist non-uniformly scales a crate, then
rotates and slides it into place. That is a full **translate-rotate-scale (TRS)**
transform. Scale is first-class. Composition with another non-uniform scale can
leave the pure TRS family entirely.

Confusing the two is a common design bug: storing every object as a $4 \times 4$
matrix "just in case," then discovering that interpolating those matrices stretches
bones, that inverses get expensive, and that "is this rigid?" became a runtime
guess instead of a type.

## The idea

A **rigid motion** (isometry of Euclidean space that preserves orientation) is
exactly rotation plus translation. It preserves distances and angles. The set of
such motions forms a group under composition: compose two rigid motions and you
still have a rigid motion; every rigid motion has an inverse that is also rigid.

A **similarity** adds uniform scale. A general **affine** map adds non-uniform
scale and shear. A **projective** map adds perspective. Each widening step can
represent more maps and loses guarantees the narrower type had for free.

```
  rigid (pose)  ⊂  TRS  ⊂  affine  ⊂  projective
       |              |         |           |
   distances OK   scale OK   linear+T    homography
```

For animation skins, camera look-ats, and robot end-effectors, you almost always
want the left end of that chain. Scale belongs on assets and hierarchies, not on
every bone.

### What "apply a pose" means

Given a pose with position $t$ and orientation $R$:

$$
p' = R\,p + t
$$

Vectors (displacements, normals after renormalizing) get only $R$ — translation
does not act on free vectors. That split is why point and vector types matter
when you apply a transform.

### What "apply a TRS" means

Plato's convention (documented on `Transform3D`) is **scale, then rotate, then
translate**:

$$
p' = R\,(S\,p) + t
$$

Non-uniform $S$ does not commute with $R$. Rotating a stretched box leaves
something that is no longer "axis-aligned scale then rotate" in the same axes —
which is why TRS is **not closed under composition**. Compose two `Transform3D`
values through matrices or affine forms, not by multiplying their scale fields.

## In Plato

`13-transforms.plato` draws the line in the type system.

```plato
// A rigid placement in space: position plus orientation.
type Pose3D
    implements Value, Interpolatable
{
    Position: Point3D;
    Orientation: Quaternion;
}

// A translate-rotate-scale transform in 3D, applied scale first, then rotation,
// then translation.
type Transform3D
    implements Value
{
    Translation: Vector3D;
    Rotation: Quaternion;
    Scale: Number3;
}
```

Notice the field naming: pose uses `Position` (a `Point3D`) and `Orientation`;
TRS uses `Translation` (a `Vector3D`) and `Rotation`. Same geometric roles,
different types — position is a place; translation is a displacement from the
origin used as an offset in the TRS recipe.

Usage-shaped expressions:

```plato
let identityPose = Pose3D.Identity;
// => Position at origin, Orientation = Quaternion.Identity

let atDoor = Pose3D {
    Position: Point3D { X: 2, Y: 0, Z: 5 },
    Orientation: Quaternion.CreateFromAxisAngle(
        Vector3D { X: 0, Y: 1, Z: 0 },
        (1.5707963).Angle)  // quarter turn about Y
};

// Apply: rotate about origin, then translate to Position
let worldPoint = localPoint.Transform(atDoor);

// Compose: apply first, then second (documented on every Compose)
let parented = Compose(childPose, parentPose);

// Cheap rigid inverse: p' = R^{-1} (p - t)
let undo = Inverse(atDoor);
```

Widening and narrowing are explicit:

```plato
// Rigid pose → TRS with unit scale
let asTrs = Transform3D(atDoor);
// Scale is Number3(1, 1, 1)

// TRS → rigid part, discarding scale (lossy — named Pose, not Pose3D)
let rigidOnly = Pose(scaledCrate);
```

The library comment is deliberate: the conversion is named `Pose` rather than
`Pose3D` because throwing away scale must stay visible at the call site.

Homogeneous matrix form is available when you need a uniform pipeline:

```plato
let m = Matrix4x4(atDoor);
// rotation, then translation — same rigid motion

let recovered = Pose3D(m);
// precondition: m is rotation followed by translation
```

Interpolation respects the distinction. `Pose3D` implements `Interpolatable`;
its `Lerp` blends position linearly and orientation with `Slerp`:

```plato
let mid = Lerp(poseA, poseB, 0.5);
// mid.Position is the midpoint; mid.Orientation walks the short arc
```

`Transform3D` does **not** implement `Interpolatable`. Blending scale, rotation,
and translation independently is subtle (and often wrong); v3 refuses to pretend
a single `Lerp` is enough.

### The rest of the ladder

When you outgrow TRS:

| Type | Carries | Closed under compose? |
|------|---------|------------------------|
| `Pose3D` | rigid | yes |
| `Transform3D` | TRS | no (compose via matrix) |
| `AffineTransform3D` | linear + translation (`Matrix4x3`) | yes |
| `ProjectiveTransform3D` | full `Matrix4x4` | yes |
| `Motor3D` | rigid as dual quaternion | yes |

`Frame3D` (origin + three orthonormal axes) is another clothing of a rigid pose —
handy when you think in local bases rather than quaternions. Conversions
`Frame3D(pose)` / `Pose3D(frame)` exist with an orthonormality precondition.

## Pitfalls / fine print

**Storing scale in every pose "for convenience."** Unit scale is not free: it
tempts artists to squash bones, breaks distance-preserving assumptions in
physics, and makes `Inverse` a general matrix inverse instead of a conjugate
plus a cheap translation fixup.

**Composing two `Transform3D` values by multiplying scales and concatenating
quaternions.** That only works for uniform scale (or commuting special cases).
Non-uniform scale rotated by a non-aligned rotation needs the matrix product.

**Applying a pose to vectors the same way as points.** Translation must not
move free vectors. Plato's overloads encode this: `Transform(v: Vector3D, pose)`
uses only orientation.

**Extracting `Pose3D` from an arbitrary `Matrix4x4`.** The conversion's
precondition is rigidity. A matrix with scale or shear will still produce
*some* quaternion and translation; garbage in, plausible-looking garbage out
unless callers check.

**2D twin.** `Pose2D` / `Transform2D` tell the same story with `Rotation2D` and
`Number2` scale. Prefer them in planar tools instead of stuffing Z = 0 into 3D
types.

## Try it

1. A pose has `Position = (10, 0, 0)` and identity orientation. What is
   `Transform(Point3D(1, 2, 3), pose)`?
2. Why does `Transform3D` list `Scale` before `Rotation` in the doc comment's
   application order, even though the fields are written Translation, Rotation,
   Scale?
3. You call `Pose(t)` on a `Transform3D` whose scale is `(2, 1, 1)`. What
   information is lost, and what still matches the original on unit-scale
   inputs?

<details>
<summary>Answers</summary>

1. $(11, 2, 3)$ — identity rotation leaves the local point unchanged; then add
   the position.
2. Field order in the type is a storage layout choice; application order is a
   semantic convention documented separately: scale, then rotate, then translate.
3. Non-uniform stretch along X is discarded. Position and rotation of the TRS
   remain; applying the recovered pose to a point does **not** reproduce the
   scaled TRS result.

</details>

## Library recommendations

- **missing-function** — `13-transforms.plato`: `Transform3D` has no
  `Compose(Transform3D, Transform3D)` and the file banner says to compose through
  matrix/affine forms, but there is no helper such as
  `ComposeTrs(first, second): AffineTransform3D` that returns the right closed
  type. Teaching "TRS is not a group" wants a one-liner that lands in
  `AffineTransform3D` without forcing callers to remember the path.

- **naming** — `13-transforms.plato`: `Pose(t: Transform3D): Pose3D` is easy to
  misread as a constructor. A name like `RigidPart` or `DiscardScale` would make
  the lossy step louder at call sites the pose-vs-transform lesson keeps
  emphasizing.

- **missing-interface** — `13-transforms.plato`: `Pose3D` implements
  `Interpolatable` but `Transform3D` does not, with no interface such as
  `RigidMotion` marking distance-preserving maps. A small marker interface would
  let generic code (constraints, IK, skinning) require rigidity without listing
  `Pose3D | Motor3D` by hand.

- **doc-comment** — `13-transforms.plato`: `Transform3D` fields are ordered
  Translation, Rotation, Scale while application is S-R-T. A field-level note
  ("storage order ≠ application order") would prevent the confusion this lesson
  has to spell out in prose.
