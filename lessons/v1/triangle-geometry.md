---
lesson: triangle-geometry
title: Triangle Geometry
domain: Geometry primitives
v3-files: [17-planar-shapes.plato, 18-spatial-primitives.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Triangle Geometry

Every mesh is a pile of triangles. Rasterizers, ray tracers, finite-element codes, and
collision libraries all keep returning to the same small toolkit: area, normal, centroid,
barycentric coordinates, and a sober respect for the degenerate case where three points
forget to span a plane.

A triangle is the simplest polygon that is always planar and always convex (when
non-degenerate). That is why graphics pipelines standardize on it — and why the formulas
below show up in so many disguises.

## The idea

### Area and normal

Given vertices $A$, $B$, $C$ in order, the cross product of two edge vectors is both an
area measure and a normal direction:

$$
\mathbf{n}_{\text{raw}} = (B - A) \times (C - A),
\quad
\text{Area} = \tfrac{1}{2}\|\mathbf{n}_{\text{raw}}\|.
$$

In 2D, the “cross” reduces to a scalar $z$-component
$(B_x-A_x)(C_y-A_y) - (B_y-A_y)(C_x-A_x)$; half its absolute value is area, and its sign
is the winding (positive for counter-clockwise in the usual math convention).

```
        C
       / \
      /   \
     /     \
    A-------B

  CCW ⇒ positive signed area (Plato convention)
```

The unit normal is $\mathbf{n}_{\text{raw}} / \|\mathbf{n}_{\text{raw}}\|$. Right-hand
rule: curl fingers $A\to B\to C$, thumb points along $\mathbf{n}$.

### Centroid

The centroid (center of mass for uniform density) is the average of the vertices:

$$
G = \frac{A + B + C}{3}.
$$

It is also the intersection of the medians. For shading and labeling, $G$ is the usual
“put it in the middle” point.

### Circumcenter and incenter (classic extras)

- **Circumcenter** — center of the unique circle through $A$, $B$, $C$; intersection of
  perpendicular bisectors. Outside an obtuse triangle.
- **Incenter** — center of the inscribed circle; intersection of angle bisectors;
  weighted average of vertices by opposite side lengths.

These matter for meshing quality and circle packing; many engines compute them on demand
rather than storing them.

### Barycentric coordinates

Weights $(u, v, w)$ with $u + v + w = 1$ such that

$$
P = uA + vB + wC.
$$

Inside the triangle (including boundary) iff $u,v,w \ge 0$. This is the workhorse of
interpolation: colors, normals, and texture coordinates at $P$ are the same weighted
blend. Point-in-triangle tests are often barycentric tests in disguise.

### Degeneracy

If $A$, $B$, $C$ are collinear (or coincident), area is zero, the normal is undefined,
barycentric coordinates are unstable, and any algorithm that divides by area explodes.
Detect with $\|\mathbf{n}_{\text{raw}}\| < \varepsilon$ (or squared area) and reject or
repair.

## In Plato

Planar and spatial triangles are separate types — embedding dimension is in the name.

From `17-planar-shapes.plato`:

```plato
// Three vertices, counter-clockwise for positive area.
// Degenerate when the vertices are collinear.
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

From `18-spatial-primitives.plato`:

```plato
// Three vertices in space; the fundamental rendering and meshing primitive.
// The normal follows the right-hand rule around A, B, C.
type Triangle3D
    implements Geometry3D, ConvexShape, Connected, Manifold, Orientable,
               PlanarMeasurable, Bounded3D, PointSet3D, Centroid3D,
               NearestPoint3D, Deformable3D
{
    A: Point3D;
    B: Point3D;
    C: Point3D;
}
```

Both implement `PlanarMeasurable` (`Area`, `Perimeter`) and `Centroid2D`/`Centroid3D`.
`Triangle2D` is a `ClosedShape` with `ContainsPoint2D`. `Triangle3D` is an oriented flat
patch in space (`Manifold`, `Orientable`) — a surface element, not a solid — so it does
not implement `ContainsPoint3D` (a 3D point is almost never exactly in the plane).

Winding: file 17 states counter-clockwise is positive; file 18 states the normal follows
the right-hand rule around $A,B,C$.

`BarycentricCoordinate` in `11-points.plato` is the weight triple:

```plato
type BarycentricCoordinate
{
    U: Number;
    V: Number;
    W: Number;
}
```

Usage-shaped sketches:

```plato
let face = Triangle3D {
    A: Point3D { X: 0, Y: 0, Z: 0 },
    B: Point3D { X: 1, Y: 0, Z: 0 },
    C: Point3D { X: 0, Y: 1, Z: 0 }
};
// Area == 0.5; Centroid == (1/3, 1/3, 0)
// Normal points toward +Z (right-hand rule)

let tri = Triangle2D {
    A: Point2D { X: 0, Y: 0 },
    B: Point2D { X: 2, Y: 0 },
    C: Point2D { X: 0, Y: 2 }
};
// Contains(Point2D { X: 0.5, Y: 0.5 }) == true
// Perimeter == 2 + 2 + 2√2

let weights = BarycentricCoordinate { U: 0.5, V: 0.25, W: 0.25 };
// P = 0.5 A + 0.25 B + 0.25 C
```

Related solids: `Tetrahedron` (four points, signed volume from oriented base triangle)
and `Wedge` (triangle swept by an offset) build on the same vertex ordering discipline.

## Pitfalls / fine print

**Winding flips.** Importing a mesh with clockwise faces flips every normal and breaks
back-face culling. Plato’s positive convention is CCW / right-hand; convert on import.

**Area vs signed area.** Lighting wants a consistent normal; UI hit-tests sometimes want
absolute area. Know which API returns signed vs absolute.

**Obtuse circumcenter.** The circumcenter can lie far outside; using it as a “center” for
labels looks wrong. Prefer the centroid for annotations.

**Barycentric near degeneracy.** Even a slightly skinny triangle makes $u,v,w$ sensitive
to noise. Robust predicates exist; naive floats fail on near-zero area.

**Triangle3D is not a volume.** `Contains` for a solid tetrahedron is a different test
(half-space or barycentric in 3-simplex). Do not use `Triangle3D` as a collider volume;
use `Tetrahedron`, a mesh, or a thick `Capsule3D`.

**Perimeter of Triangle3D.** Sum of three edge lengths — well-defined even if you think
of it as a patch. `Area` uses the 3D cross-product magnitude.

## Try it

1. $A=(0,0)$, $B=(4,0)$, $C=(0,3)$ in 2D. Area? Centroid?
2. Same triangle. Is $P=(1,1)$ inside? Rough barycentric intuition?
3. $A,B,C$ collinear on a line in 3D. What fails first: area, normal, or centroid?

<details>
<summary>Answers</summary>

1. Area $= \tfrac{1}{2}\cdot 4\cdot 3 = 6$. Centroid $= (4/3,\; 1,\; 0)$ in 2D
   $(4/3, 1)$.
2. Yes — it lies in the first quadrant under the hypotenuse $x/4 + y/3 = 1$. Barycentric
   weights are all positive.
3. Area becomes 0 and the normal becomes undefined (divide by zero). The centroid formula
   $(A+B+C)/3$ still returns a point on the line — so centroid can look “fine” while
   everything orientation-based is already broken. Always test area/degeneracy first.

</details>

## Library recommendations

- **missing-function** — `17-planar-shapes.plato` / `18-spatial-primitives.plato`: no
  declared `Normal(Triangle3D)`, `SignedArea(Triangle2D)`, `Circumcenter`, `Incenter`,
  or `Barycentric(triangle, point)`. The lesson’s toolkit is classical; only `Area` /
  `Centroid` / `Contains` appear via concepts. Name the rest on the geometry library
  surface.

- **missing-function** — `18-spatial-primitives.plato`: `Triangle3D` has `NearestPoint3D`
  but no `Plane` extraction (`FromTriangle → Plane`). Clipping and BSP authors need that
  one-liner as a declared conversion.

- **naming** — `17-planar-shapes.plato`: `Triangle2D` documents CCW positive area;
  `Triangle3D` documents right-hand normals. A shared one-line “ordering convention”
  pointer between the two types would reduce import-time winding mistakes.

- **doc-comment** — `18-spatial-primitives.plato`: state explicitly that `Triangle3D` is a
  zero-thickness patch (no `ContainsPoint3D`) so learners do not expect solid
  containment.

> Resolved 2026-07-28: Normal(Triangle3D): Direction3D and FromTriangle(Triangle3D): Plane added to spatial-primitives.plato (438/439); SignedArea, Circumcenter, Incenter and Barycentric for Triangle2D added to planar-shapes.plato (438).
