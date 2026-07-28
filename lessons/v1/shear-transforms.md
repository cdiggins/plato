---
lesson: shear-transforms
title: Shear Transforms
domain: Matrices & transforms
v3-files: [09-matrices.plato, 13-transforms.plato, 70-intrinsics.plato]
audience: Comfortable with 2D/3D vectors and matrix multiplication as composition of maps
status: draft-v1
---

# Shear Transforms

Take a rectangle and push its top edge sideways while keeping the bottom edge fixed. The
shape becomes a parallelogram. Areas stay the same (in 2D), angles do not, and lengths
along most directions stretch. That deformation is a **shear** (also called a skew).

Shear is the transform that TRS pipelines pretend does not exist — until a matrix
decomposition fails, a font engine leans a glyph into italic, or a physics engine needs
to express strain. Understanding shear is understanding why "scale, rotate, translate"
is a convenient subset of affine maps, not the whole story.

## The idea

### Horizontal shear in the plane

A horizontal shear by factor $k$ maps $(x, y)$ to $(x + k y,\; y)$:

$$
\begin{pmatrix} x' \\ y' \end{pmatrix}
=
\begin{pmatrix} 1 & k \\ 0 & 1 \end{pmatrix}
\begin{pmatrix} x \\ y \end{pmatrix}
$$

```
  before                after (k > 0)
  +----+                +----+
  |    |               /    /
  |    |              /    /
  +----+             +----+
```

Vertical lines tilt; horizontal lines stay horizontal. The determinant is $1$, so
oriented area is preserved. The map is invertible: shear by $-k$ undoes it.

### Why shear is not scale

Non-uniform scale stretches along fixed axes: $(x,y)\mapsto(s_x x,\; s_y y)$. The images
of the unit axes remain orthogonal when the axes were the coordinate axes. Shear tilts
axes toward each other — the images of $\mathbf{e}_1$ and $\mathbf{e}_2$ are no longer
perpendicular (unless $k = 0$).

You can factor some shears as rotate–scale–rotate (SVD), but the intermediate rotation
means the shear is **not** a member of the TRS family when expressed in a fixed world
frame. That is why `Decompose` on a sheared matrix reports failure or returns a
factorization that does not round-trip as "scale then rotate then translate" in the
original axes.

### Shear in 3D

In 3D there are six elementary shears (each pair of distinct axes): for example, shear
$X$ by $Z$ with factor $k$ sends $(x,y,z)\mapsto(x + k z,\; y,\; z)$. Volumes are
preserved (determinant $1$). Composing shears with rotations and scales generates the
full special linear group $\mathrm{SL}(3)$ of volume-preserving linear maps — far larger
than the diagonal scales used in `Transform3D.Scale`.

### Affine embedding

A linear shear about the origin extends to an affine map by adding a translation row.
In Plato's row-vector convention (see `13-transforms.plato`), a 2D affine map is a
`Matrix3x2` (two linear rows + one translation row); a 3D affine map is a `Matrix4x3`.

## In Plato

### Matrix types that can hold a shear

From `09-matrices.plato`:

```plato
// A 2x2 matrix: 2D linear maps (rotation, scale, shear).
type Matrix2x2
    implements MatrixLike, Multiplicative
{
    Row1: Number2;
    Row2: Number2;
}

// A 3x3 matrix: 3D linear maps, 2D homogeneous transforms, orientation bases.
type Matrix3x3
    implements MatrixLike, Multiplicative
{
    Row1: Number3;
    Row2: Number3;
    Row3: Number3;
}

// A 3-row, 2-column matrix: a 2D affine map (2x2 linear part plus a translation
// row).
type Matrix3x2
    implements MatrixLike
{
    Row1: Number2;
    Row2: Number2;
    Row3: Number2;
}
```

The doc comment on `Matrix2x2` explicitly lists shear alongside rotation and scale — the
vocabulary knows shear belongs here. There is, however, no `CreateShear` factory in
`70-intrinsics.plato` (contrast `CreateScale`, `CreateRotation`, `CreateTranslation` on
`Matrix3x2` / `Matrix4x4`).

### Building a shear by hand

Horizontal shear with factor $k$ as a `Matrix2x2` (rows are row vectors):

```plato
var k = 0.5;
var shear = Matrix2x2 {
    Row1: Number2 { X: 1.0, Y: k },   // first row: (1, k)
    Row2: Number2 { X: 0.0, Y: 1.0 }  // second row: (0, 1)
};
```

Under row-vector multiplication $v' = v \cdot M$, the point $(x,y)$ becomes
$(x, y) \cdot M = (x + 0\cdot y,\; k x + y)$ depending on layout — **verify against your
backend's row/column convention**. Plato's Transforms library documents row-vector style
matching System.Numerics: `a.Multiply(b)` applies `a` first, then `b`. When constructing
raw rows, keep that convention consistent with how `Transform` applies matrices.

An affine 2D shear with no translation:

```plato
var shearAffine = AffineTransform2D {
    Matrix: Matrix3x2 {
        Row1: Number2 { X: 1.0, Y: 0.0 },
        Row2: Number2 { X: k,   Y: 1.0 },
        Row3: Number2 { X: 0.0, Y: 0.0 }
    }
};
```

(Exact row entries depend on whether the shear mixes $x$ from $y$ or $y$ from $x$; the
point is that `AffineTransform2D` is the typed home for "linear part + translation.")

### TRS cannot absorb shear

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
```

And the conversion comment:

```plato
// The TRS decomposition of a matrix. Precondition: the matrix is a
// scale-rotate-translate composition (Decompose succeeds); shear and
// projection are not representable.
Transform3D(m: Matrix4x4): Transform3D {
    var d = m.Decompose;
    return (d.X2, d.X1, d.X0);
}
```

`Matrix4x4.Decompose` returns `Tuple4<Number3, Quaternion, Vector3D, Boolean>` — scale,
rotation, translation, and a success flag. When the linear part contains shear, that
Boolean is the signal: **do not trust the TRS triple**.

### Composition escapes TRS

Even without an intentional shear, composing two `Transform3D` values with non-uniform
scale and intervening rotation produces shear in the product. The library comment states
TRS is **not closed under composition** — compose through matrix or affine forms instead:

```plato
var a = Transform3D { /* non-uniform Scale, some Rotation */ };
var b = Transform3D { /* another Rotation */ };
// Prefer:
var combined = Compose(
    AffineTransform3D(a),
    AffineTransform3D(b));
// rather than inventing a Transform3D product that cannot stay in TRS.
```

### Identity and invertibility

A pure shear has determinant $1$, so `CanInvert` on the affine wrapper should succeed.
The inverse is the shear with negated factor — cheap when you built it that way; use
`Inverse` on `AffineTransform2D` / `AffineTransform3D` when you only have the matrix.

## Pitfalls / fine print

**Italic text is shear, not rotation.** A common UI bug is to rotate glyphs a few degrees
to fake italic. Real italic is closer to a horizontal shear; rotation lifts the baseline.

**Normals and shear.** Shearing positions without transforming normals with the
inverse-transpose leaves lighting wrong. Use `TransformNormal` when applying a general
affine map to mesh attributes.

**Decompose is not SVD.** A failed `Decompose` does not mean the matrix is singular — it
means it is outside the scale-rotate-translate model. Singular matrices fail for a
different reason (`CanInvert` false).

**Row construction bugs.** Because v3 has no `CreateShear`, every shear is hand-built.
Off-by-one placement of $k$ in `Row1` vs `Row2` is the classic silent error — write a
unit test that maps $(0,1)$ and $(1,0)$ to the expected images.

**2D intrinsics gap.** `CreateRotation` / `CreateScale` exist on `Matrix3x2`, but
`Matrix2x2` and `Matrix3x3` have almost no intrinsic surface in `70-intrinsics.plato`.
Shear pedagogy outruns the declared factories.

## Try it

1. For the map $(x,y)\mapsto(x + 2y,\; y)$, what is the image of the unit square with
   corners $(0,0)$, $(1,0)$, $(1,1)$, $(0,1)$? What is the area of the image?

2. Why can `Transform3D` store non-uniform `Scale: Number3` but still fail to represent
   a pure horizontal shear in the $XY$ plane?

3. You compose `CreateScale(Matrix4x4, Number3(2,1,1))` with a $45°$ rotation about $Z$,
   then with the inverse scale. Is the result still a pure rotation? Why does this matter
   for TRS caches?

<details>
<summary>Answers</summary>

1. Images: $(0,0)$, $(1,0)$, $(3,1)$, $(2,1)$ — a parallelogram. Determinant $1$, so area
   remains $1$.

2. `Scale` stretches along the local axes *before* rotation, and those axes stay orthogonal
   in the object's frame. A shear tilts axes; it cannot be written as diagonal scale in
   that frame without an extra rotation that `Transform3D` does not insert for you.

3. The product is a shear (classic "scale, rotate, unscale" construction). A TRS-only
   cache that multiplies scales and rotations separately will mis-represent the composed
   map; store an `AffineTransform3D` or `Matrix4x4` instead.

</details>

## Library recommendations

- **missing-function** — `70-intrinsics.plato`: no `CreateShear` / `CreateSkew` on
  `Matrix3x2`, `Matrix4x4`, or `Matrix2x2`, despite `CreateScale`, `CreateRotation`, and
  `CreateTranslation`. The `Matrix2x2` doc comment names shear as a primary use case, but
  authors must hand-fill rows.

- **missing-function** — `09-matrices.plato` / `70-intrinsics.plato`: `Matrix2x2` and
  `Matrix3x3` declare `Multiplicative` but have no intrinsic `Multiply`, `Determinant`, or
  `Invert` in `70-intrinsics.plato` (unlike `Matrix3x2` / `Matrix4x4`). Shear lessons need
  those operations on the exact types that store linear shears.

- **doc-comment** — `13-transforms.plato`: `Decompose` / `Transform3D(Matrix4x4)` mention
  shear as unsupported; add the same note to `Pose3D(Matrix4x4)` (already says no shear)
  and to a short banner on `Transform3D` itself so readers learn the limitation before
  hitting the conversion.

- **pedagogy** — `09-matrices.plato`: `SymmetricMatrix3x3` is documented for inertia and
  strain, but there is no link from shear-as-transform (this lesson) to shear-as-strain in
  `66-engineering.plato`. A cross-file doc pointer would help — without requiring lesson
  cross-links.
