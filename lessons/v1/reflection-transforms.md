---
lesson: reflection-transforms
title: Reflection Transforms
domain: Matrices & transforms
v3-files: [13-transforms.plato, 09-matrices.plato, 16-lines.plato, 70-intrinsics.plato]
audience: Comfortable with 3D points, vectors, and the idea of a matrix as a linear map
status: draft-v1
---

# Reflection Transforms

A mirror does something no rotation can: it flips handedness. Walk toward a bathroom
mirror and raise your right hand — the image raises what looks like a left hand. That
flip is not a rigid motion. Distances and angles are preserved, but orientation is
reversed. In geometry code the same operation shows up as reflection across a plane:
mirrors in ray tracers, billboards that face a wall, and "flip this mesh to make a
symmetric twin."

If you try to store a mirror as a `Pose3D` or a unit `Quaternion`, you will fail. Those
types encode orientation-preserving rigid placements. Reflection needs a linear map whose
determinant is negative — and that is a matrix story.

## The idea

### Reflection of a vector across a plane through the origin

Let $\mathbf{n}$ be a unit normal. Any vector $\mathbf{v}$ decomposes into a part parallel
to $\mathbf{n}$ and a part perpendicular to it:

$$
\mathbf{v}_\parallel = (\mathbf{v}\cdot\mathbf{n})\,\mathbf{n},
\qquad
\mathbf{v}_\perp = \mathbf{v} - \mathbf{v}_\parallel
$$

Reflection keeps the parallel part and negates the perpendicular part — or equivalently,
subtracts twice the parallel part:

$$
\operatorname{reflect}(\mathbf{v}, \mathbf{n})
  = \mathbf{v} - 2\,(\mathbf{v}\cdot\mathbf{n})\,\mathbf{n}
$$

```
        v
       /
      /   reflected
     /      \
 ───●────────●─── plane (n upward)
     \      /
      \   v'
       \
```

Geometrically: drop a perpendicular from the tip of $\mathbf{v}$ onto the plane, then
continue the same distance on the other side.

### Homogeneous reflection across an arbitrary plane

A plane not through the origin needs translation as well as the linear flip. In Hesse
normal form a plane is the set of points $p$ with $\mathbf{n}\cdot p = d$. The Householder
reflection that mirrors across that plane is a $4\times 4$ homogeneous matrix. Applied to
a point with weight $1$, it produces the mirrored point; applied to a direction with
weight $0$, it produces the mirrored direction (translation ignored).

The determinant of a reflection matrix is $-1$. That single number is the algebraic
signature of "handedness flipped." A pure rotation has determinant $+1$. A scale that
flips one axis also has determinant $-1$ and is a reflection composed with a stretch —
same orientation class, different metric effect.

### What reflections do not preserve

- **Orientation / winding.** A counter-clockwise triangle becomes clockwise.
- **Cross-product handedness.** $\mathbf{a}\times\mathbf{b}$ after reflection equals the
  reflection of $\mathbf{b}\times\mathbf{a}$ (order swapped), not the reflection of
  $\mathbf{a}\times\mathbf{b}$.
- **Membership in the rotation group.** You cannot slerp a reflection; it is not a unit
  quaternion.

They *do* preserve distances, angles (up to orientation), and incidence (points on the
plane stay on the plane).

## In Plato

### Planes as the mirror surface

From `16-lines.plato`:

```plato
// An infinite plane in Hesse normal form: the points p with Dot(Normal, p)
// equal to Distance. Distance is the signed distance from the world origin to
// the plane along Normal.
type Plane
    implements Geometry3D, Connected, Manifold, Orientable, NearestPoint3D
{
    Normal: Direction3D;
    Distance: Number;
}
```

`Normal` is a `Direction3D` (unit by construction). `Distance` is the signed offset of the
plane from the world origin along that normal — not a Euclidean "how far is this object,"
but the Hesse constant $d$ in $\mathbf{n}\cdot p = d$.

### Reflecting a displacement

`70-intrinsics.plato` declares vector reflection:

```plato
Reflect(self: Vector3D, normal: Vector3D): Vector3D;
Reflect(self: Vector2D, normal: Vector2D): Vector2D;
```

Usage-shaped:

```plato
var n = Direction3D { Vector: Vector3D { X: 0, Y: 1, Z: 0 } };
var incoming = Vector3D { X: 1, Y: -1, Z: 0 };
var bounced = Reflect(incoming, n.Vector);
// bounced ≈ (1, 1, 0) — the Y component flipped across the XZ plane
```

This is the Householder formula on vectors. The `normal` argument is typed as `Vector3D`,
not `Direction3D` — callers must pass a unit-length vector for the geometric meaning to
hold (the intrinsic does not re-normalize in the declaration).

### Building a reflection matrix

Also in `70-intrinsics.plato`:

```plato
CreateReflection(_: Matrix4x4, value: Plane): Matrix4x4;
```

```plato
var mirror = Plane {
    Normal: Direction3D { Vector: Vector3D { X: 0, Y: 1, Z: 0 } },
    Distance: 0.0
};
var M = CreateReflection(Matrix4x4, mirror);

var p = Point3D { X: 1, Y: 2, Z: 3 };
// Apply via the homogeneous map (row-vector style, v * M, per Transforms conventions).
var pMirrored = Transform(PositionVector(p), M); // then recover as a point
```

`CreateReflection` is the right factory when you need to transform whole meshes, normals
(carefully — see pitfalls), or compose the mirror with other maps.

### Where reflections sit in the transform taxonomy

From `13-transforms.plato`:

| Type | Can represent a reflection? |
|------|-----------------------------|
| `Pose3D` | No — rotation + translation only; doc requires no reflection |
| `Transform3D` (TRS) | No — scale is per-axis stretch, not a plane Householder |
| `Quaternion` / `Motor3D` | No — orientation-preserving |
| `AffineTransform3D` / `Matrix4x3` | Yes — linear part may have $\det = -1$ |
| `ProjectiveTransform3D` / `Matrix4x4` | Yes — widest family; `CreateReflection` lands here |
| `Matrix3x3` | Yes — linear reflection through a plane containing the origin |

`Pose3D(m: Matrix4x4)` documents its precondition explicitly: the matrix must be a rotation
followed by a translation — **no scale, shear, or reflection**. Feeding it a mirror matrix
is undefined in the vocabulary.

`Matrix4x4.Decompose` similarly expects a scale-rotate-translate factorization; a pure
reflection is not in that family, so the success flag in the returned
`Tuple4<Number3, Quaternion, Vector3D, Boolean>` should be treated as false.

### Determinant as a quick test

```plato
var det = Determinant(M);
// det < 0  →  orientation-reversing (reflection or odd number of axis flips)
// det > 0  →  orientation-preserving
```

`Determinant` is declared on `Matrix4x4` and `Matrix3x2` in the intrinsics surface.

## Pitfalls / fine print

**Normals after reflection.** Transforming a surface normal with the same matrix used for
positions is wrong whenever the linear part is not a pure rotation. For a reflection the
correct map on normals is the inverse-transpose of the linear block — which for an
orthogonal Householder reflection coincides with the reflection itself, but the moment you
compose with non-uniform scale the shortcut dies. Prefer `TransformNormal` when a matrix
may contain scale.

**Winding flips break lighting and culling.** After mirroring a triangle mesh, face
winding reverses. Either flip index winding or negate face normals — do both or neither,
consistently.

**`Reflect` vs `CreateReflection`.** `Reflect(v, n)` mirrors a single vector across a plane
*through the origin* with normal `n`. `CreateReflection(plane)` builds a full homogeneous
map for an arbitrary `Plane` (possibly offset). Do not pass a non-unit normal into
`Reflect` and expect the geometric mirror.

**Double cover with scales.** `CreateScale` with a negative factor on one axis is also
orientation-reversing. It is a reflection composed with stretch about coordinate planes,
not about an arbitrary `Plane`. Use `CreateReflection` when the mirror surface is given as
a `Plane`.

**Rigid APIs reject mirrors silently if you ignore preconditions.** `Pose3D(m)`,
`Transform3D(m)`, and `Quaternion(m)` all require orthonormal / TRS structure. A reflection
matrix will not round-trip through those conversions.

## Try it

1. Let $\mathbf{n} = (0,1,0)$ and $\mathbf{v} = (3, 4, 0)$. Compute
   $\operatorname{reflect}(\mathbf{v}, \mathbf{n})$ by hand.

2. Does `Pose3D` implement a reflection across the plane $y = 0$? Why or why not, given
   v3's type definitions?

3. A triangle has vertices in counter-clockwise order as seen along $+Z$. After
   `CreateReflection` across the $YZ$ plane (normal $+X$), is the projected winding still
   counter-clockwise when viewed along $+Z$?

<details>
<summary>Answers</summary>

1. $\mathbf{v}\cdot\mathbf{n} = 4$, so
   $\mathbf{v} - 2\cdot 4\cdot\mathbf{n} = (3,4,0) - (0,8,0) = (3,-4,0)$.

2. No. `Pose3D` stores `Position: Point3D` and `Orientation: Quaternion`. Quaternions
   represent rotations (determinant $+1$). There is no field for a reflection; the
   `Pose3D(Matrix4x4)` conversion forbids reflection in its precondition.

3. No — reflection reverses orientation, so the winding becomes clockwise when viewed
   along the same axis.

</details>

## Library recommendations

- **missing-function** — `09-matrices.plato` / `70-intrinsics.plato`: `CreateReflection` exists
  for `Matrix4x4` but there is no `CreateReflection` for `Matrix3x3` or `Matrix2x2` (origin-
  centered linear mirrors). Teaching planar reflections forces a jump to homogeneous 4×4
  even when the problem is 2D.

- **naming** — `70-intrinsics.plato`: `Reflect(self: Vector3D, normal: Vector3D)` takes a
  bare `Vector3D` for the normal while `CreateReflection` takes a `Plane` whose normal is
  already a `Direction3D`. Prefer `Reflect(self: Vector3D, normal: Direction3D)` (or
  document that the vector must be unit) so the unit invariant is not caller folklore.

- **doc-comment** — `13-transforms.plato`: `Pose3D(m: Matrix4x4)` and `Transform3D(m)` mention
  that reflection is forbidden, but `AffineTransform3D` never states that its linear block
  *may* have negative determinant. One sentence on orientation-reversing affine maps would
  tell readers where mirrors actually live.

- **missing-function** — `16-lines.plato`: no `SignedDistance(plane: Plane, point: Point3D)`
  helper. Reflection lessons (and almost every plane query) need
  $\mathbf{n}\cdot p - d$; today callers re-derive it from fields ad hoc.
