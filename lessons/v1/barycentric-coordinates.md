---
lesson: barycentric-coordinates
title: Barycentric Coordinates
domain: Coordinate systems & bounds
v3-files: [11-points.plato, 17-planar-shapes.plato]
audience: Comfortable with 2D points and triangles; no graphics pipeline experience required.
status: draft-v1
---

# Barycentric Coordinates

Pick three corners of a triangle and a point floating somewhere in its plane.
There is a uniquely handy way to write that point as a weighted average of the
corners — weights that sum to one. Those weights are **barycentric
coordinates**. They are how GPUs know which pixel sits inside a triangle, how
textures know which texel mix to sample, and how finite-element codes know which
node values to blend.

Once you see them, they show up everywhere: interpolation over a simplex, area
ratios, convex combinations, and "is this point inside?" tests that never need
an angle.

## The idea

### Weights that sum to one

Given triangle vertices $A$, $B$, $C$ and a point $P$ in the same plane:

$$
P = u\,A + v\,B + w\,C
\quad\text{with}\quad
u + v + w = 1
$$

The triple $(u,v,w)$ is the barycentric coordinate of $P$ relative to
$(A,B,C)$. Solve for the weights using **signed areas** (2D) or ratios of
parallelogram areas:

$$
u = \frac{\mathrm{Area}(P,B,C)}{\mathrm{Area}(A,B,C)},\quad
v = \frac{\mathrm{Area}(A,P,C)}{\mathrm{Area}(A,B,C)},\quad
w = \frac{\mathrm{Area}(A,B,P)}{\mathrm{Area}(A,B,C)}
$$

```
         C
        / \
       / P \     w large near C
      /  ·  \    v large near B
     /_______ \  u large near A
    A         B
```

At a vertex the weight is $1$ on that vertex and $0$ on the others. On an edge
the opposite vertex's weight is $0$. At the centroid, $u=v=w=\tfrac13$.

### Inside the triangle

For a triangle with positive (counter-clockwise) orientation:

- $P$ is **inside** (or on the boundary) iff $u \ge 0$, $v \ge 0$, $w \ge 0$
  (and still $u+v+w=1$).
- If any weight is negative, $P$ lies outside, toward the opposite side of that
  vertex's facing edge.

That is the workhorse of software rasterization: compute barycentrics per pixel,
discard negatives, interpolate vertex attributes with the same weights.

### Interpolating attributes

Any quantity defined at vertices — color, UV, normal, temperature — blends as:

$$
f(P) = u\,f(A) + v\,f(B) + w\,f(C)
$$

This is the unique linear function matching the vertex values. Perspective-
correct GPU attribute interpolation modifies the weights with depth; the
underlying idea remains barycentric.

### Areas without drama

Signed area of triangle $(A,B,C)$ in 2D is half the 2D cross product:

$$
2\,\mathrm{Area} =
(B_x - A_x)(C_y - A_y) - (B_y - A_y)(C_x - A_x)
$$

Plato's planar winding convention: **counter-clockwise is positive**. Degenerate
(collinear) triangles have area $0$ — barycentrics are undefined.

### Outside the plane (3D)

On a triangle in space, the same area-ratio formulas use magnitudes of cross
products (or solve the linear system in the triangle's plane). Ray tracers store
barycentrics at hit points so they can interpolate UVs without rebuilding the
triangle later.

## In Plato

The coordinate type (`11-points.plato`):

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

The triangle (`17-planar-shapes.plato`):

```plato
// Three vertices, counter-clockwise for positive area. Degenerate when the
// vertices are collinear.
type Triangle2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, PointSet2D, Centroid2D,
               ContainsPoint2D, NearestPoint2D, Deformable2D
{
    A: Point2D;
    B: Point2D;
    C: Point2D;
}
```

Field naming alignment to teach carefully:

| Barycentric field | Vertex on `Triangle2D` |
|-------------------|------------------------|
| `U` | `A` |
| `V` | `B` |
| `W` | `C` |

(The type comment says "weights relative to a triangle's vertices" but does not
spell this binding — see recommendations.)

Usage-shaped expressions (conceptual — conversion helpers are thin in v3):

```plato
let tri = Triangle2D {
    A: Point2D { X: 0, Y: 0 },
    B: Point2D { X: 2, Y: 0 },
    C: Point2D { X: 0, Y: 2 }
};

let bary = BarycentricCoordinate { U: 0.5, V: 0.25, W: 0.25 };
// On the triangle: U+V+W = 1, all non-negative → inside

let centroidWeights = BarycentricCoordinate {
    U: 1.0/3.0, V: 1.0/3.0, W: 1.0/3.0
};
```

Point-in-triangle via the shape concept:

```plato
// Triangle2D implements ContainsPoint2D
let inside = tri.Contains(point);
// Implementations typically use half-plane or barycentric tests
```

Reconstructing a point from weights (affine combination) is ordinary point /
vector arithmetic once conversions exist:

$$
P = A + v\,(B-A) + w\,(C-A)
$$

(with $u = 1-v-w$). Equivalently $P = uA+vB+wC$ in homogeneous language.

**Declared elsewhere, useful to know:** hit records in spatial queries carry a
`Barycentric: BarycentricCoordinate` field so a ray-triangle intersection can
hand you the weights directly. The planar story still bottoms out on
`Triangle2D` + `BarycentricCoordinate`.

**Algebra concept note:** a generic `Barycentric(a,b,c,u,v)` helper appears in
concept-library numerical code as a blend of three `Numerical` values with
parameters $(u,v)$ and implied $w=1-u-v$. That is the attribute-interpolation
pattern; the geometric coordinate type is still `BarycentricCoordinate`.

## Pitfalls / fine print

**Assuming $U+V+W=1$ always.** The type stores three numbers; the invariant is
an author/document obligation for points *on the plane of* the triangle. Floating
error can drift the sum; renormalize or use only two free components $(v,w)$ with
$u=1-v-w$ when precision matters.

**Clockwise triangles.** Negative area flips all signed weights. A point that
looks "inside" visually may fail $u,v,w\ge 0$ if winding is reversed. Respect
`Triangle2D`'s CCW convention or flip tests.

**Degenerate triangles.** Zero area → division by zero in the ratio formulas.
`Triangle2D`'s doc comment flags collinear vertices; check before computing
barycentrics.

**Using barycentrics as Cartesian.** $(u,v,w)$ is not a position in space; it is
relative to a specific triple of vertices. Change the triangle, change the
meaning.

**Perspective.** Screen-space barycentrics are not the same as world-space ones
under perspective projection. Texture swimming bugs come from lerping attributes
with the wrong weights.

**Outside weights.** Negative components are useful (extrapolation, rejecting
hits). Do not clamp to $0$ unless you intend closest-point-on-triangle behavior.

## Try it

1. For an equilateral mental model, what are $(u,v,w)$ at vertex $B$?
2. Point on edge $AC$, halfway. What weights?
3. Why does $u+v+w=1$ matter for calling the combination "affine" rather than
   merely linear?

<details>
<summary>Answers</summary>

1. $(0,1,0)$ — all weight on $B$.
2. $(0.5,\; 0,\; 0.5)$ — no $B$ influence; equal $A$ and $C$.
3. If weights sum to $1$, translating all vertices by the same vector
   translates $P$ the same way. If they summed to another constant, $P$ would
   behave like a weighted vector from the origin — frame-dependent junk for
   positions.

</details>

## Library recommendations

- **missing-function** — `11-points.plato` / `17-planar-shapes.plato`: no
  `BarycentricCoordinate(tri: Triangle2D, p: Point2D)` or
  `Point2D(tri: Triangle2D, b: BarycentricCoordinate)`. The coordinate type and
  triangle type are stranded without the maps this lesson is about.

- **doc-comment** — `11-points.plato`: `BarycentricCoordinate` should state the
  binding `U→A`, `V→B`, `W→C` for `Triangle2D` (and the $U+V+W=1$ invariant on
  the support plane). Without that, `U/V/W` are meaningless labels.

- **missing-function** — `17-planar-shapes.plato`: no
  `SignedArea(self: Triangle2D): Number` on the easy surface (area may live
  behind `PlanarMeasurable`, but barycentric teaching wants the signed scalar
  used in the ratio formulas called out by name).

- **wrong-shape** — `11-points.plato`: storing three floats with a sum-to-one
  invariant invites drift. A two-component form `(V, W)` with
  `U = 1 - V - W` (plus an optional recovered triple view) would make the
  invariant unrepresentable-as-false — matching Plato's "illegal states
  unrepresentable" taste.

> Resolved 2026-07-28: BarycentricCoordinate doc now states the U->A/V->B/W->C vertex binding and the U+V+W=1 support-plane invariant (item 20). The two-component (V,W) reshape was REJECTED (item 22): three fields kept, invariant documented, and an `IsNormalized(bary, tolerance)` predicate added in numeric-structures.library.plato instead. (stdlib commit pending).

> Resolved 2026-07-28: planar-shapes.plato added SignedArea(Triangle2D) (21) and the Barycentric(Triangle2D, Point2D) / Point(Triangle2D, BarycentricCoordinate) maps (19); the BarycentricCoordinate type in points.plato was left untouched.
