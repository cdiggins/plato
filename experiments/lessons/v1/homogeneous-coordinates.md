---
lesson: homogeneous-coordinates
title: Homogeneous Coordinates
domain: Matrices & transforms
v3-files: [09-matrices.plato, 11-points.plato]
audience: High-school vectors and general programming / graphics curiosity
status: draft-v1
---

# Homogeneous Coordinates

Ordinary 3×3 matrices are linear: they always leave the origin fixed. Real
scenes need translation — move everything three meters east — and cameras need
perspective — parallel railroad tracks meeting at a vanishing point. Both are
outside pure linear algebra in 3D.

The trick is to add a coordinate. Work in a space one dimension higher, use
ordinary matrix multiplication there, then (when needed) divide by the extra
component to get back. That extra component is $w$, and the coordinates are
called **homogeneous**.

## The idea

A Cartesian 3D point $(x, y, z)$ lifts to a homogeneous 4-tuple
$(x, y, z, w)$ with $w \neq 0$. The Cartesian point recovered is

$$
\left(\frac{x}{w},\;\frac{y}{w},\;\frac{z}{w}\right).
$$

Usually we store finite points with $w = 1$, so the divide is a no-op until a
perspective matrix messes with $w$.

**Why translation works.** With $w = 1$, a matrix can add constants to $x,y,z$
using the last row/column (convention-dependent). In row-vector form, the
translation lives in the last row's first three entries. The fourth component
stays 1 for affine maps.

**Why vectors differ from points.** A displacement should not be translated.
Give vectors $w = 0$: the translation row contributes nothing, while the
linear $3\times3$ block still rotates and scales. Same matrix, different $w$,
correct behavior for points vs directions.

**Perspective.** A projective matrix can write a multiple of $z$ into $w$.
After the divide, farther objects shrink — foreshortening — in one algebraic
step. Affine matrices keep $w$ unchanged (up to a global scale); projective
ones do not.

```
  Point  (x, y, z, 1)  --M-->  (x', y', z', w')  --÷w-->  Cartesian
  Vector (x, y, z, 0)  --M-->  (x', y', z', 0)   (no divide; still a direction)
```

In 2D the same story uses three components and $3\times3$ matrices:
$(x, y, w)$ with Cartesian $(x/w, y/w)$.

## In Plato

Homogeneous points are first-class types in `11-points.plato`:

```plato
// A 2D point in homogeneous form; the Cartesian point is (X/W, Y/W).
type HomogeneousPoint2D
{
    X: Number;
    Y: Number;
    W: Number;
}

// A 3D point in homogeneous form; the Cartesian point is (X/W, Y/W, Z/W).
type HomogeneousPoint3D
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}
```

Ordinary Cartesian points stay separate:

```plato
type Point2D { X: Number; Y: Number; }
type Point3D { X: Number; Y: Number; Z: Number; }
```

The matrix machines live in `09-matrices.plato`:

```plato
// A 3x3 matrix: 3D linear maps, 2D homogeneous transforms, orientation bases.
type Matrix3x3 ...

// A 4x4 matrix: 3D homogeneous transforms and projections.
type Matrix4x4 ...
```

Affine storage without a full fourth column of freedom:

```plato
type Matrix3x2  // 2D affine (linear + translation row)
type Matrix4x3  // 3D affine (linear + translation row)
```

Projective maps wrap the square homogeneous matrices (declared with transforms,
applied to Cartesian points by lifting internally):

```plato
// Apply the homography: lift to homogeneous coordinates, map, and divide
// by the resulting weight.
Transform(p: Point3D, t: ProjectiveTransform3D): Point3D {
    var v = Number4(p.X, p.Y, p.Z, 1.0).Transform(t.Matrix);
    return (v.X / v.W, v.Y / v.W, v.Z / v.W);
}
```

That body is the whole lesson in three lines: lift with $w=1$, multiply by
`Matrix4x4`, perspective-divide by $w$.

Usage-shaped snippets:

```plato
let p = Point3D { X: 1.0, Y: 2.0, Z: 3.0 };

// Explicit homogeneous form (w = 1 for a finite point)
let h = HomogeneousPoint3D { X: 1.0, Y: 2.0, Z: 3.0, W: 1.0 };

// A direction / vector at infinity: w = 0
let dir = HomogeneousPoint3D { X: 0.0, Y: 1.0, Z: 0.0, W: 0.0 };

// Projective map owns a Matrix4x4
let proj = ProjectiveTransform3D { Matrix: someMatrix4x4 };
let q = p.Transform(proj);   // lift, multiply, divide

// Affine path: Matrix4x3 has no projective w-write; translation in Row4
let aff = AffineTransform3D { Matrix: someMatrix4x3 };
let r = p.Transform(aff);
```

Identity homogeneous matrix (row-vector): ones on the diagonal of
`Matrix4x4`, zeros elsewhere — the do-nothing machine, including $w'=w$.

## Pitfalls / fine print

**Forgetting the divide.** After a perspective matrix, $x'/w$ is the screen
space coordinate, not $x'$. Skipping the divide produces nonsense that still
"looks like numbers."

**$w = 0$.** Dividing by zero means the point landed on the plane/line at
infinity. The projective transform comments state this precondition
explicitly.

**Points vs vectors.** Applying a translation matrix to data that meant
"direction" but was stored with $w=1$ slides the direction as if it were a
position. Use $w=0$ or APIs that transform `Vector3D` without translation.

**Scaling homogeneity.** $(2, 4, 6, 2)$ is the same Cartesian point as
$(1, 2, 3, 1)$. Equality of homogeneous tuples is projective, not componentwise.

**Affine vs projective types.** `AffineTransform3D` (`Matrix4x3`) cannot
express perspective. `ProjectiveTransform3D` (`Matrix4x4`) can. Widening
affine → projective pads the missing column with $(0,0,0,1)$.

**`HomogeneousPoint3D` is under-connected.** The transform library often lifts
through `Number4` rather than `HomogeneousPoint3D`. The type exists for clarity;
conversions are not yet a complete API.

## Try it

1. Homogeneous $(2, 4, 6, 2)$ corresponds to which Cartesian `Point3D`?
2. Why does translating with $w=0$ leave a pure direction unchanged by the
   translation part?
3. If a matrix writes $w' = z$ (roughly), what happens to points with larger
   $z$ after the divide?

<details>
<summary>Answers</summary>

1. $(1, 2, 3)$ after dividing by $w=2$.
2. The translation terms are multiplied by $w$; zero kills them. Only the
   linear block acts.
3. Dividing by a larger $w$ shrinks $x/w$ and $y/w$ — farther objects appear
   smaller (perspective foreshortening).

</details>

## Library recommendations

- **missing-function** — `11-points.plato`: no `ToHomogeneous(p: Point3D): HomogeneousPoint3D`,
  `ToPoint(h: HomogeneousPoint3D): Point3D` (with nonzero-$W$ precondition), or
  `ToHomogeneous(v: Vector3D): HomogeneousPoint3D` with $W=0$. The types are
  declared; the lift/project API is missing, so call sites invent `Number4`
  instead.

- **missing-function** — `09-matrices.plato` / transforms: no
  `Transform(h: HomogeneousPoint3D, m: Matrix4x4): HomogeneousPoint3D` that
  stays in homogeneous space without forcing an immediate divide — useful for
  clipping pipelines that need $w$ before the divide.

- **doc-comment** — `HomogeneousPoint3D`: state the $W=1$ point / $W=0$
  vector convention explicitly on the type, not only in surrounding prose.
  That convention is the entire reason the type exists beside `Point3D`.

- **naming** — perspective divide is only described inside
  `Transform(Point3D, ProjectiveTransform3D)`. A named
  `PerspectiveDivide(h: HomogeneousPoint3D): Point3D` would make the step
  teachable and reusable.
