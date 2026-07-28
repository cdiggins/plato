---
lesson: signed-distance-fields
title: Signed Distance Fields — Shape as a Function
domain: Fields, implicits & noise
v3-files: [26-fields.plato, 27-implicit-sdf.plato]
audience: High-school algebra and basic programming; comfortable with points, vectors, and functions.
status: draft-v1
---

# Signed Distance Fields — Shape as a Function

A triangle mesh stores a shape as corners and edges. A bitmap stores inside/outside flags on a
grid. Both answer "is this point inside?" — but only after walking data structures or scanning
pixels.

A **signed distance field** (SDF) answers differently: it is a **function** mapping every position
to one number. That number tells you how far the point is from the boundary, and which side you
are on. Change the function and you change the shape — no retriangulation, no raster resize. Ray
marchers, collision queries, procedural modeling, and font rendering exploit this because
evaluation is local and composable.

The vocabulary is small: one scalar per point, a sign convention, and a gradient pointing toward
the nearest boundary.

## From membership to distance

An **implicit region** is the boolean version: inside or not, with no distance attached. Plato
declares `ImplicitRegion2D` as a `Procedural<Point2D, Boolean>` whose `Eval` returns `true` when
the point lies in the region. The 3D analogue is `ImplicitVolume3D`.

An SDF is strictly richer. At every point it returns a signed distance:

| Sign of the value | Meaning |
|-------------------|---------|
| negative | inside the solid |
| zero | on the boundary |
| positive | outside the solid |

The magnitude is distance to the nearest point on the boundary (exactly, or a conservative lower
bound after some operations). The sign encodes side.

```
        outside (+)          boundary (0)          inside (-)
    ·························|··························
              d = +3         d = 0                 d = -2
```

This sign convention matches Plato's doc comments on `SignedDistanceField2D` and
`SignedDistanceField3D`: negative inside, zero on the boundary, positive outside. Some graphics
texts flip the sign; always check the convention before mixing formulas or code.

## The circle SDF, derived by hand

Take a disk — Plato's `Circle` type stores `Center: Point2D` and `Radius: Number`. We want a
function $f(p)$ such that:

- $f(p) < 0$ when $p$ is strictly inside the disk,
- $f(p) = 0$ when $p$ is on the circle,
- $f(p) > 0$ when $p$ is outside,
- $|f(p)|$ is the Euclidean distance from $p$ to the nearest point on the circle.

Let $d = \|p - c\|$ be the distance from $p$ to the center $c$, and let $r$ be the radius.

**Outside** ($d > r$): the nearest boundary point is radial; distance from $p$ to the circle is
$d - r > 0$.

**Inside** ($d < r$): the nearest boundary point is still radial; we want a negative value, so
$f(p) = d - r$ works ($d - r < 0$).

**On the boundary:** $d = r$ gives $f(p) = 0$.

The closed form is therefore:

$$
f(p) = \|p - c\| - r
$$

Check three test points for a unit circle at the origin ($c = (0,0)$, $r = 1$):

| Point $p$ | $\|p\|$ | $f(p) = \|p\| - 1$ | Region |
|-----------|---------|---------------------|--------|
| $(0, 0)$ | $0$ | $-1$ | center, deep inside |
| $(1, 0)$ | $1$ | $0$ | on boundary |
| $(3, 0)$ | $3$ | $+2$ | outside |

The same formula lifts to 3D unchanged. A `Sphere` with `Center: Point3D` and `Radius: Number`
has SDF $f(p) = \|p - c\| - r$. Plato stores the geometric `Sphere` and the field concept
`SignedDistanceField3D` separately; the mathematics is identical, only the representation differs.

### ASCII picture

```
              y
              ^
              |
         ·····|·····  f = +1  (outside)
              |
    ----------+----------  f = 0   (boundary, radius r)
         ·····|·····
              |
              c --------> x

    Inside the circle: f < 0
```

## Gradient and the outward normal

Where the SDF is differentiable and $f(p) \neq 0$, the gradient $\nabla f$ points in the
direction of **steepest increase** of $f$. For our circle SDF, $f$ grows as you move away from the
center past the boundary, so $\nabla f$ points **outward** from the solid.

For $f(p) = \|p - c\| - r$ with $p \neq c$:

$$
\nabla f(p) = \frac{p - c}{\|p - c\|}
$$

That is a unit vector from center toward $p$: the outward normal at the nearest boundary point
when $p$ is outside, and still the radial direction when $p$ is inside.

At $p = c$ the gradient is undefined — every boundary point is equidistant from the center.
Renderers and simulators special-case such points or nudge $p$ by a tiny epsilon.

Plato's `DifferentiableScalarField2D` and `DifferentiableScalarField3D` concepts declare
`GradientAt(x: Self, point: Point2D): Vector2D` (respectively `Point3D` → `Vector3D`). The doc
comment on the 3D variant states that the gradient of a signed distance field is the outward
surface normal direction.

## Why SDFs are useful

**Inside/outside is one comparison.** A point is inside when $f(p) < 0$. No winding rules, no
parity tests — though see pitfalls below for numeric tolerance.

**The zero level set is the surface.** The set $\{p \mid f(p) = 0\}$ is the boundary curve or
surface. Ray marching steps along a ray using $f(p)$ as a safe step size when $f$ is exact.

**Fields compose.** Two SDFs can be combined with `min` and `max` to form unions and intersections;
Plato's `SdfTree2D`, `SdfTree3D`, and `SdfCombine` sum type encode those combinations as flat
node graphs over externally supplied primitive fields.

**Sampling is optional.** An exact formula can live in code; a `SampledSdf2D` or `SampledSdf3D`
discretizes values on a lattice inside `Bounds2D` / `Bounds3D` and interpolates when you need a
grid-like representation.

## In Plato

Plato separates **fields** (functions over space) from **implicit shapes** (membership and
distance). File `26-fields.plato` declares the field hierarchy; file `27-implicit-sdf.plato`
specializes scalars into signed distances and CSG trees.

### Field concepts

Every field is a pure mapping: side-effect free and deterministic for a fixed field value. The
root capability is `Field<TDomain, TValue>`, which inherits `Procedural<TDomain, TValue>`:

```plato
concept Procedural<TDomain, TRange>
{
    Eval(x: Self, input: TDomain): TRange;
}

concept Field<TDomain, TValue>
    inherits Procedural<TDomain, TValue>
{ }

concept ScalarField2D
    inherits Field<Point2D, Number>
{ }

concept ScalarField3D
    inherits Field<Point3D, Number>
{ }
```

Evaluating a planar scalar field at a point is `Eval(field, point)`.

### Signed distance concepts

```plato
concept SignedDistanceField2D
    inherits ScalarField2D
{ }

concept SignedDistanceField3D
    inherits ScalarField3D
{ }
```

The concepts add no new methods in v3; the contract lives in doc comments: signed distance,
negative inside, zero on the boundary, positive outside, exact or a conservative lower bound.

For contrast, membership without distance:

```plato
concept ImplicitRegion2D
    inherits Procedural<Point2D, Boolean>
{ }

concept ImplicitVolume3D
    inherits Procedural<Point3D, Boolean>
{ }
```

### Differentiable fields

```plato
concept DifferentiableScalarField2D
    inherits ScalarField2D
{
    GradientAt(x: Self, point: Point2D): Vector2D;
}

concept DifferentiableScalarField3D
    inherits ScalarField3D
{
    GradientAt(x: Self, point: Point3D): Vector3D;
}
```

Use `GradientAt` for normals, push vectors, or flow directions on an isosurface.

### Usage-shaped snippets

The circle SDF mirrors the derivation. `Point2D` implements `Difference<Vector2D>`; `Vector2D`
implements `Normed`:

```plato
// Illustrative Eval body for f(p) = ||p - c|| - r
let offset = Between(c, p);          // p - c as Vector2D
let d = Magnitude(offset);
let f = d - r;                       // negative inside, zero on circle, positive outside
```

Membership from the sign:

```plato
let inside = Eval(sdf, p) < 0;
```

Gradient at an exterior point (requires a differentiable implementation):

```plato
let g = GradientAt(sdf, p);          // Vector2D, outward for an exact circle SDF
```

A CSG union over two planar SDF primitives:

```plato
let tree = SdfTree2D {
    Nodes: [
        Leaf(Primitive: ItemIndex { Value: 0 }),
        Leaf(Primitive: ItemIndex { Value: 1 }),
        Interior(
            Combine: Union,
            Left: SdfNodeIndex { Value: 0 },
            Right: SdfNodeIndex { Value: 1 })
    ],
    Root: SdfNodeIndex { Value: 2 }
};
```

Discretized storage:

```plato
let sampled = SampledSdf3D {
    Values: grid,              // Array3D<Number>
    Bounds: axisAlignedBox    // Bounds3D
};
```

`ConstantScalarField2D` and `ConstantScalarField3D` (same `Value: Number` everywhere) are trivial
scalar fields useful as leaves in field expression graphs from `26-fields.plato`.

## Pitfalls and fine print

**Sign conventions differ.** Plato uses negative-inside. Some tools use the opposite sign or store
unsigned distance plus a separate bit. When porting a formula, flip the sign once and re-test three
known points (deep inside, on boundary, clearly outside).

**Exact vs bound.** After `SdfRoundingModifier`, smooth variants in `SdfCombine`, or
`SdfDisplacementModifier3D`, the returned value may be only a **lower bound** on true distance —
still safe for conservative ray marching but not for exact penetration depth.

**Zero is fragile.** `f(p) == 0` rarely happens with floating-point arithmetic. Treat "on the
boundary" as $|f(p)| < \varepsilon$ for a small tolerance.

**Gradient at the center.** For a ball SDF, $\nabla f$ is undefined at the center. Do not
normalize the zero vector.

**SDF $\neq$ distance to the obvious wall.** The signed value is distance to the **nearest** point
on the **entire** surface. In a concavity, a point may report small positive $f$ even when a
different feature feels "closer" visually.

**Implicit without distance.** Converting `Eval(region, p) == true` to an SDF requires extra work.
Prefer starting from an SDF when you need both membership and offset.

**2D vs 3D naming.** Planar fields use `Point2D` and `SignedDistanceField2D`; spatial fields use
`Point3D` and `SignedDistanceField3D`. The `D` suffix marks the dimension of the **domain**, not
a component count — consistent with Plato's vector naming rule.

## Try it

A `Circle` has `Center = (2, 0)` and `Radius = 3`. Use $f(p) = \|p - c\| - r$.

**1.** Compute $f(p)$ for $p = (2, 0)$, $(5, 0)$, and $(2, 4)$.

**2.** Which points lie inside, on, or outside the disk?

**3.** At $p = (5, 0)$, what is $\nabla f(p)$ (direction only)?

<details>
<summary>Answers</summary>

**1.**

- $p = (2, 0)$: $\|p - c\| = 0$, so $f = 0 - 3 = -3$.
- $p = (5, 0)$: $\|p - c\| = 3$, so $f = 3 - 3 = 0$.
- $p = (2, 4)$: $\|p - c\| = 4$, so $f = 4 - 3 = +1$.

**2.**

- $(2, 0)$: inside ($f < 0$).
- $(5, 0)$: on the boundary ($f = 0$).
- $(2, 4)$: outside ($f > 0$).

**3.**

$\nabla f(5, 0) = \frac{(5,0) - (2,0)}{\|(3,0)\|} = (1, 0)$ — unit vector along $+X$, outward
from the center through $p$.

</details>

## Library recommendations

- **missing-type** — `27-implicit-sdf.plato` declares `SignedDistanceField2D` /
  `SignedDistanceField3D` concepts and CSG trees, but no closed-form primitive types (e.g. a
  `Circle`-backed planar SDF or `Sphere`-backed spatial SDF) with the standard $\|p - c\| - r$
  implementation. The hand-derived circle formula has no named home in v3.

- **missing-function** — `SignedDistanceField2D` / `SignedDistanceField3D` carry the sign
  convention only in doc comments. Pedagogically central queries — `IsInside`, `ClearanceAt`
  (unsigned distance outside), `IsOnBoundary` with tolerance — are not declared on the concepts.
  Every snippet re-implements `Eval(sdf, p) < 0` by hand.

- **missing-function** — `DifferentiableScalarField2D.GradientAt` returns `Vector2D`, but shading
  and solvers want a unit `Direction2D` on the zero level set. A `NormalAt` returning
  `Direction2D` / `Direction3D` would match `08-vectors.plato` and avoid normalize guards at
  every call site.

- **doc-comment** — `ScalarField2D` / `ScalarField3D` doc comments list "signed distances" as a
  use case, but only `27-implicit-sdf.plato` defines `SignedDistanceField2D` /
  `SignedDistanceField3D`. A cross-reference in `26-fields.plato` would clarify that SDFs refine
  scalar fields rather than introducing a separate evaluation mechanism.

- **missing-type** — `Circle` and `Sphere` implement `ContainsPoint2D` / `ContainsPoint3D` and
  `NearestPoint2D` / `NearestPoint3D`, but v3 declares no bridge from those geometry types to
  `SignedDistanceField2D` / `SignedDistanceField3D`. The geometric and implicit representations
  of the same solid remain disconnected in the type graph.
