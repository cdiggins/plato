---
lesson: affine-combinations
title: Affine Combinations of Points
domain: Foundations & vectors
v3-files: [02-concepts-algebra.plato, 11-points.plato, 08-vectors.plato, 13-transforms.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Affine Combinations of Points

A midpoint is not "half of A plus half of B" in the naive sense of adding street
addresses. It is a weighted blend of positions that stays a position. Artists
call this "averaging points"; graphics calls it an affine combination; CAD calls
it a barycentric blend. The same algebra powers triangle shading, Bezier
evaluation, and centroid formulas — and it is exactly where the point/vector
split either helps you or bites you.

## The idea

Take points $P_0, P_1, \ldots, P_n$ and weights $w_0, w_1, \ldots, w_n$. The
expression

$$
Q = w_0 P_0 + w_1 P_1 + \cdots + w_n P_n
$$

is an **affine combination** when the weights sum to one:

$$
w_0 + w_1 + \cdots + w_n = 1.
$$

Why the sum-to-one rule? Rewrite relative to $P_0$:

$$
Q = P_0 + w_1 (P_1 - P_0) + \cdots + w_n (P_n - P_0).
$$

Each $(P_i - P_0)$ is a **vector** (a displacement). Scaling and adding vectors
is legal. Adding $P_0$ back yields a **point**. The identity $w_0 = 1 - (w_1 +
\cdots + w_n)$ is what makes the rewrite work — that is the affine condition.

If the weights sum to something other than one, the result depends on where you
put the origin. Move the coordinate frame and the "sum of points" jumps. That
is why adding raw positions is not a geometric operation.

Special cases:

| Weights | Name | Result |
|---------|------|--------|
| $(1-t),\; t$ | Linear interpolation | Point on the segment |
| $1/3,\; 1/3,\; 1/3$ | Centroid of a triangle | Average of vertices |
| $u,\; v,\; w$ with $u+v+w=1$ | Barycentric combination | Point in the plane of a triangle |
| all $w_i \ge 0$ and sum to 1 | Convex combination | Point inside the convex hull |

Convex combinations are the safest subset: every weight non-negative, sum one.
The result cannot leave the convex hull of the input points. Affine
combinations allow negative weights and can extrapolate outside the hull.

```
        P2
        ●
       /|\
      / | \
     /  ●Q \     Q = u P0 + v P1 + w P2
    /   |   \    with u + v + w = 1
   ●----+----●
  P0         P1
```

## In Plato

Plato separates positions (`Point2D` / `Point3D`) from displacements
(`Vector2D` / `Vector3D`). Points implement `Difference<TDelta>`, which is the
algebraic home of "point ± vector" and "point − point → vector."

From `02-concepts-algebra.plato`:

```plato
concept Difference<TDelta>
{
    Add(x: Self, delta: TDelta): Self;
    Subtract(x: Self, delta: TDelta): Self;
    Between(a: Self, b: Self): TDelta;
}
```

From `11-points.plato`:

```plato
type Point3D
    implements Coordinate, Difference<Vector3D>, Hashable
{
    X: Number;
    Y: Number;
    Z: Number;
}

// Weights relative to a triangle's vertices; U + V + W = 1 on the triangle.
type BarycentricCoordinate
    implements Value
{
    U: Number;
    V: Number;
    W: Number;
}
```

`Coordinate` inherits `Interpolatable`, so two-point blends are already named:

```plato
concept Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}
```

The `Transforms` library spells the displacement convention explicitly:
`Between(a, b)` is $b - a$ (from `a` toward `b`). The affine rewrite of a
two-point blend is therefore:

```plato
// Midpoint: weights 1/2, 1/2
let mid = Add(a, Multiply(Between(a, b), 0.5));

// Same as Lerp with t = 0.5
let mid2 = Lerp(a, b, 0.5);

// Extrapolation beyond b (affine, not convex): t = 1.5
let past = Lerp(a, b, 1.5);
```

A triangle blend with barycentric weights uses three points and the same
rewrite:

```plato
let bary = BarycentricCoordinate { U: 0.5, V: 0.3, W: 0.2 };
// Q = P0 + v*(P1-P0) + w*(P2-P0)   (since u = 1 - v - w)
let q = Add(
    p0,
    Add(
        Multiply(Between(p0, p1), bary.V),
        Multiply(Between(p0, p2), bary.W)));
```

Vectors themselves are `Numerical` (hence `Scalable` and `Additive`), so the
weighted sum of displacements is ordinary vector arithmetic. The only illegal
step would be summing the points as if they were vectors — and `Point3D` does
not implement self-`Additive`, so that mistake is a type error rather than a
silent origin bug.

## Pitfalls / fine print

**Weights that do not sum to one.** If you average three points with weights
$0.5, 0.5, 0.5$, you have scaled the figure about the origin by $1.5$ in
disguise. Always normalize weights, or use the rewrite form that builds from
one anchor point and vectors.

**Negative weights.** Affine combinations allow them; convex combinations do
not. Negative weights are useful (extrapolation, finite differences) but they
leave the convex hull. Rasterization and mesh blending usually want the
convex subset ($w_i \ge 0$).

**Barycentric on vs off the triangle.** `BarycentricCoordinate` documents
$U + V + W = 1$ *on* the triangle. Off the plane of the triangle you need a
different story (orthogonal projection first, or tetrahedron volumes). Off
the face but still in the plane, one weight goes negative — still affine,
no longer convex.

**Lerp is unclamped.** `Interpolatable.Lerp` does not clamp $t$ to $[0,1]$.
That matches affine extrapolation. If you want a segment clamp, you must
clamp $t$ yourself (or ask the library for a segment-restricted helper).

**Homogeneous shortcut.** In homogeneous coordinates, an affine combination of
points ($w=1$) is component-wise, then optionally re-normalized. Mixing in a
vector ($w=0$) changes the story. Prefer the typed `Point3D` /
`Vector3D` path unless you are already in a projective pipeline.

## Try it

1. Points $A=(0,0)$, $B=(4,0)$, $C=(0,6)$. What point is
   $Q = \tfrac13 A + \tfrac13 B + \tfrac13 C$?
2. Express $Q = 2B - A$ as an affine combination. Do the weights sum to 1?
   Is it a convex combination?
3. Using `Between` and `Add`, write the Plato-shaped form of
   $Q = (1-t)A + tB$ for $t=0.25$.

<details>
<summary>Answers</summary>

1. Centroid: $((0+4+0)/3,\; (0+0+6)/3) = (4/3,\; 2)$.
2. Weights: $-1$ on $A$, $+2$ on $B$. Sum is $1$, so affine. Not convex
   (negative weight) — it extrapolates past $B$.
3. `Add(A, Multiply(Between(A, B), 0.25))`, or `Lerp(A, B, 0.25)`.

</details>

## Library recommendations

- **missing-function** — `11-points.plato` / `13-transforms.plato`: there is no
  `AffineCombine(points, weights)` (or `WeightedSum` restricted to
  sum-weights-one) on `Point2D`/`Point3D`. The lesson must assemble the
  operation from `Between` + `Multiply` + `Add`; a named helper would make the
  invariant (weights sum to 1) checkable and teachable.

- **missing-function** — `11-points.plato`: `BarycentricCoordinate` stores
  weights but v3 declares no `Evaluate(bary, p0, p1, p2): Point3D` (or 2D)
  that applies them. Triangle shading and ray-hit reconstruction both need
  this one-liner.

- **doc-comment** — `02-concepts-algebra.plato`: `Difference.Between` should
  state the minuend/subtrahend convention in the concept comment itself
  (`Between(a,b) = b - a`), matching the `Transforms` library body, so affine
  rewrite examples do not depend on reading implementation files.

- **pedagogy** — `11-points.plato`: a short note on `BarycentricCoordinate`
  distinguishing affine ($u+v+w=1$) from convex ($u,v,w \ge 0$) would prevent
  the most common misuse when teaching combinations.
