---
lesson: matrices-as-machines
title: Matrices as Machines
domain: Matrices & transforms
v3-files: [09-matrices.plato]
audience: High-school linear algebra intuition and general programming background
status: draft-v1
---

# Matrices as Machines

A matrix is not a spreadsheet. It is a machine that eats a vector and spits
out another vector. Once you see every column (or row, depending on
convention) as "where a basis vector goes," you can read a matrix the way a
mechanic reads a wiring diagram — at a glance.

Graphics, robotics, and data science all lean on this: rotation, scale, shear,
change of basis, and projection are linear machines, and matrices are how we
write them down.

## The idea

A map $f$ from vectors to vectors is **linear** when it respects addition and
scaling:

$$
f(\mathbf{u} + \mathbf{v}) = f(\mathbf{u}) + f(\mathbf{v}), \qquad
f(c\mathbf{v}) = c\,f(\mathbf{u}).
$$

Linearity is extremely restrictive — and extremely useful. If you know where
the standard basis vectors $\mathbf{e}_1, \mathbf{e}_2, \mathbf{e}_3$ go, you
know $f$ everywhere:

$$
f(x\mathbf{e}_1 + y\mathbf{e}_2 + z\mathbf{e}_3)
  = x\,f(\mathbf{e}_1) + y\,f(\mathbf{e}_2) + z\,f(\mathbf{e}_3).
$$

Pack $f(\mathbf{e}_1)$, $f(\mathbf{e}_2)$, $f(\mathbf{e}_3)$ as the columns of
a matrix $M$, and the machine is matrix–vector multiplication. In **column
vector** convention, $\mathbf{v}' = M\mathbf{v}$. In **row vector** convention
(Plato / System.Numerics style), $\mathbf{v}' = \mathbf{v}\,M$, and the images
of the basis live in the **rows** instead.

```
  Column view (math textbooks):     Row view (Plato matrices):

  [ |  |  | ] [x]                   [x y z] [ — row1 — ]
  [ e1 e2 e3] [y]  = M v            [     ] [ — row2 — ]  = v M
  [ |  |  | ] [z]                   [     ] [ — row3 — ]
```

Either way: **read the matrix as "where the axes went."** A pure rotation's
axes stay unit length and perpendicular. A scale stretches them. A shear
slides them. A singular matrix flattens space so some direction collapses to
zero — the machine loses information and cannot be inverted.

Matrix–matrix multiplication is machine composition: apply one linear map,
then another. Order matters; $AB \neq BA$ in general.

## In Plato

From `09-matrices.plato`, the family interface:

```plato
// The family of matrix types: element access by zero-based row and column.
interface MatrixLike
    inherits Value, Additive, Scalable
{
    RowCount(x: Self): Integer;
    ColumnCount(x: Self): Integer;
    ElementAt(x: Self, row: Integer, column: Integer): Number;
}
```

Fixed-size machines for geometry and graphics, stored as **row vectors**:

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

// A 4x4 matrix: 3D homogeneous transforms and projections.
type Matrix4x4
    implements MatrixLike, Multiplicative
{
    Row1: Number4;
    Row2: Number4;
    Row3: Number4;
    Row4: Number4;
}
```

Doc comment on storage: element $(r, c)$ is component $c$ of row $r$. Reading
a `Matrix3x3` at a glance means reading three `Number3` rows.

Affine maps (linear part plus translation) use skinny matrices that do not
need a full homogeneous row for teaching linear machines — but they are still
matrices:

```plato
type Matrix3x2  // 2D affine: 2x2 linear + translation row
{
    Row1: Number2;
    Row2: Number2;
    Row3: Number2;
}

type Matrix4x3  // 3D affine: 3x3 linear + translation row
{
    Row1: Number3;
    Row2: Number3;
    Row3: Number3;
    Row4: Number3;
}
```

Symmetric special case for inertia / covariance:

```plato
type SymmetricMatrix3x3
{
    M11: Number; M12: Number; M13: Number;
    M22: Number; M23: Number;
    M33: Number;
}
```

Runtime shape for scientific work: `MatrixN`, and `Tensor` for higher rank.

Usage-shaped snippets (illustrative):

```plato
// Identity 3x3: each row is a standard basis vector
let I = Matrix3x3 {
    Row1: Number3 { X: 1.0, Y: 0.0, Z: 0.0 },
    Row2: Number3 { X: 0.0, Y: 1.0, Z: 0.0 },
    Row3: Number3 { X: 0.0, Y: 0.0, Z: 1.0 }
};

// Scale by 2 on X, 0.5 on Y: read the diagonal from the rows
let S = Matrix2x2 {
    Row1: Number2 { X: 2.0, Y: 0.0 },
    Row2: Number2 { X: 0.0, Y: 0.5 }
};

// Element access via MatrixLike
let a01 = S.ElementAt(0, 1);   // row 0, col 1 → 0.0

// Composition: Multiplicative on square matrices
let M = A.Multiply(B);         // apply A then B in row-vector convention
```

Intrinsics (declared in the broader v3 surface) supply builders and algebra
used throughout transforms: `Matrix4x4.CreateScale`, `CreateTranslation`,
`Transpose`, `Invert`, `CanInvert`, `Decompose`. The matrix *types* in
`09-matrices.plato` are the nouns; those operations are the verbs.

## Pitfalls / fine print

**Row vs column convention.** Mixing OpenGL-style column vectors with
row-vector matrices silently transposes every formula. Plato documents
row-vector style: `a.Multiply(b)` applies `a` first, then `b`.

**Translation is not linear.** Pure `Matrix2x2` / `Matrix3x3` linear maps fix
the origin. Moving the origin requires affine or homogeneous forms
(`Matrix3x2`, `Matrix4x3`, `Matrix4x4`).

**Reading the wrong strip.** In row convention, basis images are rows; in
column convention, columns. Looking at the "wrong" strip inverts your mental
picture of the axes.

**Singular machines.** If determinant is zero (columns/rows dependent),
`Invert` fails. Scales with a zero factor, projections, and degenerates all
hit this.

**`MatrixLike` is not `Multiplicative`.** The interface only promises shape and
element access plus add/scale. Square fixed matrices opt into
`Multiplicative` separately. `Matrix3x2` / `Matrix4x3` do not implement
`Multiplicative` on the type declaration — composition often goes through
widening to square homogeneous form.

**Symmetric storage.** `SymmetricMatrix3x3` stores six numbers. Treating it
like a full `Matrix3x3` without expansion loses the invariant or wastes space.

## Try it

1. Write the `Matrix2x2` (row form) that scales X by 3 and Y by 1.
2. What does the identity `Matrix3x3` do to any vector, and how do you see
   that from its rows?
3. If machine $A$ rotates 90° and machine $B$ scales X by 2, why might
   $A$ then $B$ differ from $B$ then $A$?

<details>
<summary>Answers</summary>

1. Row1 = $(3, 0)$, Row2 = $(0, 1)$.
2. Rows are $\mathbf{e}_1, \mathbf{e}_2, \mathbf{e}_3$; every vector maps to
   itself.
3. Non-uniform scale and rotation do not commute: rotating a stretched axis
   is not the same as stretching a rotated axis. Matrix order encodes that.

</details>

## Library recommendations

- **missing-function** — `09-matrices.plato`: `MatrixLike` exposes
  `ElementAt` but not `Row(i)` / `Column(i)` returning `Number2/3/4`. Teaching
  "columns/rows are basis images" wants a first-class column/row accessor on
  the interface, especially under row-storage where columns are gathered.

- **missing-function** — `09-matrices.plato`: no `Determinant`, `Trace`, or
  `IsOrthogonal` on the matrix types/interface. Those are the natural
  vocabulary for "is this a rotation?" and "does this machine squash volume?"

- **missing-interface** — `09-matrices.plato`: there is no `LinearMap` /
  `SquareMatrix` interface that requires `Multiplicative` + matching row/column
  counts. `MatrixLike` alone cannot express invertibility or composition.

- **doc-comment** — `Matrix3x3` / `Matrix4x4`: state in one line that Plato
  uses row-vector multiplication ($\mathbf{v} M$) so readers do not assume
  textbook column convention when interpreting `Row1` as a basis image.
