---
lesson: points-vs-vectors
title: Points vs Vectors
domain: Foundations & vectors
v3-files: [02-interfaces-algebra.plato, 08-vectors.plato, 11-points.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Points vs Vectors

You have two markers on a map: **home** and **school**. You also have a set of driving
instructions: "go 3 km east, then 2 km north." The markers are *places*. The instructions
are a *displacement* — they tell you how to move, but they are not themselves a place.

In everyday code, both ideas are often stored as `(x, y, z)` triples of numbers. That works
until you write `home + school` and get a third "location" that has no physical meaning, or
you pass a displacement to an API that expects a world position and nothing complains. The bug
is subtle: the arithmetic type-checks, but the geometry is wrong.

Affine geometry separates **points** (positions) from **vectors** (displacements and
directions). Plato makes that separation a type distinction, not a comment you hope future
you will read.

## The idea

### Two different jobs for triples of numbers

A **point** answers: *where* is something?

A **vector** answers: *how far and in which direction* should something move — or, if you
think of it as an arrow from the origin, what displacement does the arrow represent?

Both can be written with coordinates `(x, y, z)`, but they participate in different
operations.

| Operation | Points | Vectors |
|-----------|--------|---------|
| Point + vector | New point (translate) | — |
| Point − vector | New point (translate back) | — |
| Point − point | — | Vector (displacement from first to second) |
| Vector + vector | — | New vector (combine displacements) |
| Scalar × vector | — | Scaled vector |
| Point + point | **Meaningless** in affine space | — |

The last row is the one that bites in `(float, float, float)` codebases. Adding two GPS
coordinates does not give you a meaningful third location — yet three floats added happily
anyway.

### Affine space in one picture

Think of space as a sheet of paper covered with arrows:

```
        school •
              ↑  displacement d = Between(home, school)
              |
        home • --------→ 3 km east (another vector)
```

- **Points** are dots on the sheet.
- **Vectors** are arrows; they have length and direction but no fixed "address" until you
  attach them to a point.
- **Point + vector** moves the dot along the arrow.
- **Between(a, b)** (point minus point) is the arrow that starts at `a` and ends at `b`.

There is a special point called the **origin**, and a vector from the origin to a point is
sometimes called the point's **position vector**. That vector is *not* the same type as the
point, even when the numeric components match: one is a place, the other is a displacement
from a reference place.

### What you *can* do with vectors

Vectors form a vector space: you can add them, subtract them, scale them, take dot products,
and measure their length. Points inherit a smaller toolkit — chiefly comparison,
interpolation between positions, and the point/vector algebra above.

Distance between two points is not "subtract and ignore the type"; it is the **length** of
the displacement between them:

$$\text{distance}(a, b) = \| \text{Between}(a, b) \|$$

In Plato, length is `Magnitude` on types that implement the `Normed` interface; geometric
vectors like `Vector3D` do.

## In Plato

Plato's v3 vocabulary encodes affine geometry through two parallel type families and one
generic interface.

### The `Difference` interface — affine algebra as a capability

```plato
// A position-like type whose difference is a separate delta type: subtracting two
// points yields a vector; adding a vector to a point yields a point.
interface Difference<TDelta>
{
    Add(x: Self, delta: TDelta): Self;
    Subtract(x: Self, delta: TDelta): Self;
    Between(a: Self, b: Self): TDelta;
}
```

Read `TDelta` as "the vector type that pairs with this point type." The interface does three
jobs:

1. **`Add`** — translate a point by a vector.
2. **`Subtract`** — translate a point by the negation of a vector.
3. **`Between`** — the displacement from `a` to `b` (the point−point operation).

Nothing here adds two points together. The type system refuses to express that operation
on `Difference` types.

### Points are positions, not vectors

```plato
// A position in some coordinate space. Marker interface for all point-like types.
interface Coordinate
    inherits Value, Equatable, Interpolatable
{ }

// A position in 3D Cartesian space.
type Point3D
    implements Coordinate, Difference<Vector3D>, Hashable
{
    X: Number;
    Y: Number;
    Z: Number;
}
```

`Point3D` is a **position** in 3D Cartesian space. It implements `Coordinate` (a point-like
value you can compare and interpolate) and `Difference<Vector3D>` (the affine operations
with `Vector3D` as the delta type).

2D and N-dimensional variants follow the same pattern:

```plato
type Point2D
    implements Coordinate, Difference<Vector2D>, Hashable
{
    X: Number;
    Y: Number;
}

type PointN
    implements Coordinate, Difference<VectorN>
{
    Components: Array<Number>;
}
```

Surface parameters use the same mechanism — a texture coordinate is a point in `[0,1]²`, and
its difference is a 2D vector:

```plato
type UvCoordinate
    implements Coordinate, Difference<Vector2D>
{
    U: Number;
    V: Number;
}
```

### Vectors are displacements, not positions

```plato
// A displacement or direction in 3D space.
type Vector3D
    implements Vector
{
    X: Number;
    Y: Number;
    Z: Number;
}
```

The file header states the design intent plainly: *"Points (file 11) are positions;
geometric vectors are the differences between them."*

`Vector3D` implements the `Vector` interface, which bundles numeric vector-space operations:

```plato
interface Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}
```

So `Vector3D` supports component-wise addition, scaling (`Scalable` via `Numerical`),
interpolation, `Magnitude`, `Dot`, and so on — the full vector-space toolbox. It does **not**
implement `Difference`; a vector is not a position.

### Naming: `Number3` is not `Vector3D`

Plato enforces a suffix rule (see `stdlib/README.md`):

- A **bare number** counts components: `Number3` is a 3-tuple of numbers for raw
  component-wise math (SIMD lanes, RGBA, homogeneous tuples).
- A **`D` suffix** means the type lives in a geometric **D**-dimensional space:
  `Vector3D`, `Point3D`, `Ray3D`.

There is no type named `Vector3`. If you need a geometric displacement in 3D, the name is
`Vector3D`. If you need three unrelated numbers that happen to have `X/Y/Z` fields, that is
`Number3` — same shape, different meaning.

### Usage-shaped expressions

Suppose you track a camera and a target in world space:

```plato
var eye    = Point3D { X: 0, Y: 2, Z: 5 };
var target = Point3D { X: 0, Y: 0, Z: 0 };

// Displacement from eye to target (target - eye in math notation).
var viewOffset = Between(eye, target);

// How far apart are they?
var span = Magnitude(viewOffset);

// Move eye halfway toward target (Coordinate inherits Interpolatable).
var halfway = Lerp(eye, target, 0.5);

// Step eye forward one unit along the view direction (scale then translate).
var step = Divide(viewOffset, span);
var closer = Add(eye, step);

// Undo the step.
var restored = Subtract(closer, step);
```

A few readings of that snippet:

- **`Between(eye, target)`** returns a `Vector3D`, not a `Point3D`. Assign it only to
  vector variables or pass it to APIs that expect displacements.
- **`Add` / `Subtract`** on points always take a **vector** second argument. There is no
  `Between` overload that returns a point.
- **`Lerp`** on points stays in the point type: the result is still a position on the
  segment, not a vector.
- **`Divide(viewOffset, span)`** uses vector scaling to produce a unit-length displacement
  before adding it to a point.

For a dedicated unit direction (magnitude exactly one), Plato also declares:

```plato
// A 3D direction: a Vector3D with the invariant that its magnitude is one.
type Direction3D
    implements Value
{
    Vector: Vector3D;
}
```

`Direction3D` documents the intent: this vector is normalized by construction, not merely by
convention in the variable name.

### Side-by-side type summary

| | `Point3D` | `Vector3D` |
|---|-----------|------------|
| Meaning | Position in space | Displacement or direction |
| Implements | `Coordinate`, `Difference<Vector3D>` | `Vector` |
| + same type | Not provided | `Add` (vector + vector) |
| + other | `Add(point, vector)` → point | — |
| point − point | `Between` → `Vector3D` | — |
| Length | via `Magnitude(Between(...))` | `Magnitude` directly |
| Interpolation | `Lerp` between positions | `Lerp` between displacements |

## Pitfalls / fine print

**Treating points as vectors.** It is tempting to reuse a `Point3D` where a displacement is
wanted because the fields look identical. Resist: pass `Between(origin, p)` or an explicit
displacement vector instead of passing a point to mean "offset from origin."

**Treating vectors as points.** A displacement has no inherent location until you add it to a
reference point. The numeric triple `(3, 0, 0)` is a vector; the corner of the room you are
standing in is a point. Conflating them causes wrong transforms, wrong collision bounds, and
wrong camera math.

**Adding two points.** Affine geometry has no principled "sum of two locations." If you ever
want the midpoint, use `Lerp(a, b, 0.5)`, not `Add(a, b)`.

**`Between` order matters.** `Between(a, b)` is the displacement from `a` toward `b`
(mathematically $b - a$). Swapping arguments negates the vector. When you chain paths, keep
the start and end consistent.

**`Number3` vs `Vector3D`.** Both store three `Number` components. The compiler will not stop
you from conceptual confusion if you pick the wrong type for the job — choose based on
meaning: geometric displacement → `Vector3D`; raw numeric triple → `Number3`.

**Distance is not on `Point3D` directly.** The `MetricSpace` interface declares `Distance`, but
`Point3D` does not implement it in v3. The geometric distance is
`Magnitude(Between(a, b))`. That is correct mathematically; it is just more verbose than a
point-type `Distance` would be.

**Normalization.** v3 declares `Direction2D` / `Direction3D` for unit directions, and
`Normed.Magnitude` on vectors, but does not declare a `Normalize` function on `Vector3D`
itself. Building a unit direction from an arbitrary displacement may require library support
not yet on the declared surface.

**Homogeneous and alternate coordinates.** `HomogeneousPoint3D`, `PolarCoordinate`,
`CylindricalCoordinate`, and `SphericalCoordinate` are separate types for other coordinate
systems — they are not drop-in replacements for `Point3D` in `Difference` algebra without
explicit conversion functions.

## Try it

Work these with pencil and paper; expand the answers when you are ready.

1. Let `A = Point3D { X: 1, Y: 0, Z: 0 }` and `B = Point3D { X: 4, Y: 0, Z: 3 }`. What are
   the components of `Between(A, B)`? What is `Magnitude` of that vector?

2. With the same `A` and `B`, what point does `Lerp(A, B, 0.5)` produce?

3. Let `v = Vector3D { X: 0, Y: 3, Z: 4 }`. Which of these expressions are well-typed in
   Plato's v3 vocabulary, and which are not?
   - `Add(A, v)`
   - `Add(v, v)`
   - `Between(A, v)`
   - `Subtract(B, v)`

<details>
<summary>Answers</summary>

1. `Between(A, B)` is `Vector3D { X: 3, Y: 0, Z: 3 }` — the component-wise difference
   $B - A$. Its magnitude is $\sqrt{3^2 + 0^2 + 3^2} = \sqrt{18} = 3\sqrt{2} \approx 4.24$.

2. `Lerp(A, B, 0.5)` is the midpoint `Point3D { X: 2.5, Y: 0, Z: 1.5 }`.

3. Well-typed: `Add(A, v)` (point + vector → point), `Add(v, v)` (vector + vector → vector),
   `Subtract(B, v)` (point − vector → point). **Not well-typed:** `Between(A, v)` — both
   arguments must be the same point type; a vector is not a point.

</details>

## Library recommendations

- **missing-interface** — `Point3D` (and `Point2D`, `PointN`) do not implement `MetricSpace`
  in `11-points.plato`, even though distance between positions is one of the first questions
  students ask. Either add `MetricSpace` to point types with
  `Distance(a, b) => Magnitude(Between(a, b))`, or add a short doc comment on `Coordinate`
  pointing callers at that idiom.

- **missing-function** — `08-vectors.plato`: `Vector3D` implements `Normed` but there is no
  declared `Normalize(x: Vector3D): Vector3D` (nor an interface method on `Normed`). The lesson
  needs unit directions for almost every geometry example; today only `Direction3D` encodes
  normalization as a type invariant, with no declared bridge from an arbitrary `Vector3D`.

- **pedagogy** — `02-interfaces-algebra.plato`: `Difference` names the delta type parameter
  `TDelta` but the doc comment never states the standard `Between` convention explicitly
  (displacement from first argument toward second, i.e. $b - a$). One line in the interface
  comment would prevent sign flips in every downstream transform and animation snippet.

- **missing-function** — `11-points.plato`: no declared `ToPoint` / `PositionVector` pair on
  the point types themselves (only implied by transform libraries elsewhere). For teaching
  origin-relative vectors, explicit declared converters on `Point3D` ↔ `Vector3D` would keep
  the lesson inside the foundation files without referring to transform libraries.

> Resolved 2026-07-28: added concrete `Distance`/`DistanceSquared` for Point2D/3D/N (`Between(a,b).Magnitude`) rather than a MetricSpace obligation on Coordinate (the generic-arity change the numeric-structures Coordinate marker rejects); `Normalize`/`Direction3D` factory (`FromVector`) now reachable + documented from Vector3D (items 295/296, stdlib commit pending).
