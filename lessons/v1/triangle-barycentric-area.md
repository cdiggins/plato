---
lesson: triangle-barycentric-area
title: Triangle Area and Barycentric Coordinates
domain: Geometry primitives
v3-files: [17-planar-shapes.plato, 11-points.plato]
audience: High-school geometry; vectors helpful but not required
status: draft-v1
---

# Triangle Area and Barycentric Coordinates

Every point inside a triangle is a weighted average of the triangle's three vertices.
The weights are **barycentric coordinates**. They are how GPUs interpolate colors across
a triangle, how you test whether a click hit a mesh face, and how you express the
centroid as "one third each vertex" without special cases.

The same ingredients that give you the weights also give you the triangle's area: the
magnitude of the cross product of two edge vectors. Area and barycentrics are one story.

## The idea

### Signed area in the plane

For a triangle with vertices $A$, $B$, $C$ in the plane, the signed area is

$$
\operatorname{Area}_{\mathrm{signed}}(A,B,C)
  = \tfrac{1}{2}\bigl((B_x-A_x)(C_y-A_y) - (B_y-A_y)(C_x-A_x)\bigr).
$$

That expression is half the 2D "cross product" of $\overrightarrow{AB}$ and
$\overrightarrow{AC}$. Counter-clockwise order yields a **positive** signed area;
clockwise yields negative; collinear vertices yield zero.

The geometric area is the absolute value. Plato's planar winding convention
(file 17) is: **counter-clockwise is positive**.

### Barycentric coordinates

Any point $P$ in the plane of $ABC$ can be written

$$
P = u\,A + v\,B + w\,C
\quad\text{with}\quad
u + v + w = 1.
$$

The triple $(u,v,w)$ is the **barycentric coordinate** of $P$ with respect to $ABC$.
Solving via areas (the most geometric derivation):

$$
u = \frac{\operatorname{Area}(P,B,C)}{\operatorname{Area}(A,B,C)},
\quad
v = \frac{\operatorname{Area}(A,P,C)}{\operatorname{Area}(A,B,C)},
\quad
w = \frac{\operatorname{Area}(A,B,P)}{\operatorname{Area}(A,B,C)}.
$$

Using **signed** areas, this works for points outside the triangle too: some weights
become negative. Inside (including boundary) all of $u,v,w \ge 0$ (and they still sum
to $1$).

```
         C
        / \
       / P \     P = u A + v B + w C
      /     \    u+v+w = 1
     A-------B
```

### Special points

| Point | $(u,v,w)$ |
|-------|-----------|
| Vertex $A$ | $(1,0,0)$ |
| Vertex $B$ | $(0,1,0)$ |
| Vertex $C$ | $(0,0,1)$ |
| Midpoint of $BC$ | $(0, 1/2, 1/2)$ |
| Centroid | $(1/3, 1/3, 1/3)$ |

The centroid formula is why "average the vertices" works: equal weights.

### 3D triangles

For `Triangle3D`, the cross product $\overrightarrow{AB}\times\overrightarrow{AC}$ is a
vector whose magnitude is twice the area and whose direction is the normal (right-hand
rule around $A,B,C$). Barycentric weights still use the ratios of the magnitudes of
cross products of sub-triangles (or, equivalently, ratios of parallelogram areas in the
plane of the triangle). The weights remain three scalars summing to one.

## In Plato

### Planar triangle

From `17-planar-shapes.plato`:

```plato
// Three vertices, counter-clockwise for positive area. Degenerate when the
// vertices are collinear.
type Triangle2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected, PlanarMeasurable,
               Bounded2D, PointSet2D, Centroid2D, ContainsPoint2D, NearestPoint2D,
               Deformable2D
{
    A: Point2D;
    B: Point2D;
    C: Point2D;
}
```

`PlanarMeasurable` (declared in `15-concepts-geometry.plato`) requires:

```plato
concept PlanarMeasurable
{
    Area(x: Self): Number;
    Perimeter(x: Self): Number;
}
```

So `Area(triangle)` is the vocabulary's spelling of geometric area. Signed area for
orientation tests is **not** separately declared — a gap when you need the sign for
barycentrics or winding checks.

`Centroid2D` gives `Centroid(triangle)` — for a uniform triangle this is the mean of
`A`, `B`, `C`, i.e. barycentric $(1/3,1/3,1/3)$.

`ContainsPoint2D` is the membership test; barycentrics are the standard implementation
strategy (all weights non-negative), though the concept does not prescribe the method.

### Barycentric as a type

From `11-points.plato`:

```plato
// Weights relative to a triangle's vertices; U + V + W = 1 on the triangle.
type BarycentricCoordinate
    implements Value
{
    U: Number;
    V: Number;
    W: Number;
}
```

The invariant $U+V+W=1$ is documented for points *on* the triangle's plane/affine hull.
The type does not say which vertex $U$ multiplies — by convention matching the field
order of `Triangle2D`, read:

$$
P = U\cdot A + V\cdot B + W\cdot C.
$$

There is no declared conversion `BarycentricCoordinate(triangle, point)` or
`Point(triangle, bary)` in v3 — those are the functions this lesson wants.

### Usage-shaped evaluation

```plato
var tri = Triangle2D {
    A: Point2D { X: 0, Y: 0 },
    B: Point2D { X: 2, Y: 0 },
    C: Point2D { X: 0, Y: 2 }
};

var area = Area(tri);          // 2.0 for this right triangle
var mid = Centroid(tri);       // ((0+2+0)/3, (0+0+2)/3) = (2/3, 2/3)

var bary = BarycentricCoordinate { U: 0.5, V: 0.25, W: 0.25 };
// Point from barycentric (illustrative — not a declared API):
// P = U*A + V*B + W*C  via repeated Lerp / weighted Adds on PositionVectors
```

Reconstructing a point from weights using only declared affine ops:

```plato
// P = A + V*(B-A) + W*(C-A)  when U+V+W=1 (so U = 1-V-W)
var p = Add(
    Add(tri.A, Multiply(Between(tri.A, tri.B), bary.V)),
    Multiply(Between(tri.A, tri.C), bary.W));
```

### Spatial triangle

`18-spatial-primitives.plato` defines `Triangle3D` with the same `A,B,C` fields and
`PlanarMeasurable` — so `Area` exists in 3D embedding too. Barycentric coordinates remain
the same `BarycentricCoordinate` type; the point type becomes `Point3D`.

### Degeneracy

When `Area` is zero (collinear vertices), barycentric denominators vanish.
Guard with a tolerance on `Area(tri)` before dividing; v3 does not yet declare a
dedicated `SignedArea` or degenerate predicate on `Triangle2D` itself.

## Pitfalls / fine print

**Signed vs absolute area.** Implementing barycentrics with absolute areas breaks outside
tests and can mis-classify points when the triangle is clockwise. Prefer signed area
throughout; take `abs` only when displaying "how many square units."

**Vertex order in `BarycentricCoordinate`.** The type fields are `U,V,W` with no
`VertexA` labels. Document at call sites that $U\leftrightarrow A$, $V\leftrightarrow B$,
$W\leftrightarrow C$, or wrap a pair `(Triangle2D, BarycentricCoordinate)`.

**Sum not exactly one.** Floating-point interpolation can drift. For shading, renormalize
or use the $U=1-V-W$ formulation so only two degrees of freedom are stored.

**Contains on the boundary.** `ContainsPoint2D` counts the boundary as inside. Barycentric
tests should use $\ge 0$ with a small epsilon, or points on edges flicker in/out.

**3D point not on the plane.** Classic barycentric area ratios assume $P$ lies in the
triangle's plane. For a general `Point3D`, first project onto the plane (or use volume
coordinates of the tetrahedron with an extra vertex) — otherwise $u+v+w$ may not be $1$
in a way that reconstructs $P$.

## Try it

1. Triangle $A=(0,0)$, $B=(4,0)$, $C=(0,6)$. What is the signed area? What is
   `BarycentricCoordinate` of the centroid?

2. Same triangle, point $P=(1,1)$. Estimate $(u,v,w)$ using sub-triangle areas.

3. If a triangle is stored clockwise, what happens to `Area` if the implementation
   returns signed area vs absolute area? Which choice breaks a barycentric inside test
   that checks $u,v,w \ge 0$?

<details>
<summary>Answers</summary>

1. Signed area $\tfrac12(4\cdot6-0\cdot0)=12$. Centroid weights
   `BarycentricCoordinate { U: 1/3, V: 1/3, W: 1/3 }`.

2. $\operatorname{Area}(P,B,C)=\tfrac12( (4-1)(6-1)-(0-1)(0-1) )=\tfrac12(15+1)=8$,
   so $u=8/12=\tfrac23$. Other weights follow similarly; they sum to $1$.

3. Absolute `Area` stays positive either winding; signed area flips sign when clockwise.
   A barycentric formula using signed sub-areas still works if the denominator uses the
   same signed orientation. Mixing absolute denominator with signed numerators breaks the
   $\ge 0$ inside test.

</details>

## Library recommendations

- **missing-function** — `11-points.plato` / `17-planar-shapes.plato`: no
  `Barycentric(triangle: Triangle2D, point: Point2D): BarycentricCoordinate` or the inverse
  `Point(triangle, bary)`. The type exists; the maps that give it meaning do not.

- **missing-function** — `17-planar-shapes.plato`: `PlanarMeasurable.Area` does not specify
  signed vs absolute in the concept (`15-concepts-geometry.plato`). Triangles need
  `SignedArea` explicitly for barycentrics and winding; document or split the API.

- **doc-comment** — `11-points.plato`: `BarycentricCoordinate` should state the vertex
  binding ($U\to$ first vertex of the triangle, etc.) and clarify behavior when
  $U+V+W\neq 1$ (off-plane / degenerate input).

- **missing-function** — `17-planar-shapes.plato`: no `IsInside` spelled in terms of
  barycentrics alongside `Contains`. Teaching materials re-derive the weight test every
  time; a single documented implementation would lock the epsilon policy.
