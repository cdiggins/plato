---
lesson: planes-halfspaces
title: Planes and Half-Spaces
domain: Geometry primitives
v3-files: [16-lines.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Planes and Half-Spaces

Clipping a triangle against a view frustum, deciding which side of a wall a point lies
on, carving a convex polyhedron from flat cuts — all of these start with one question:
given a flat boundary, is this point in front, behind, or on it?

A **plane** is the boundary. A **half-space** is everything on one side of that boundary
(including the plane itself, for a closed half-space). Stack enough half-spaces and you
have a convex volume: a camera frustum, a collision brush, a linear-programming feasible
region. The atom is signed distance.

## The idea

### Hesse normal form

In 3D, a plane can be written

$$
\mathbf{n}\cdot\mathbf{p} = d
$$

where $\mathbf{n}$ is a unit normal and $d$ is the signed distance from the world origin
to the plane along $\mathbf{n}$. Equivalently $\mathbf{n}\cdot\mathbf{p} - d = 0$.

The **signed distance** from an arbitrary point $\mathbf{q}$ to the plane is

$$
s = \mathbf{n}\cdot\mathbf{q} - d.
$$

| Sign of $s$ | Meaning (normal points “outward”) |
|-------------|-------------------------------------|
| $s > 0$ | in front of the plane (outside a solid that uses outward normals) |
| $s = 0$ | on the plane |
| $s < 0$ | behind the plane (inside) |

```
            n ↗
              \
               \  plane
        outside \  inside
           s>0   \   s<0
                  \
```

Flipping $\mathbf{n}$ and $d$ together (negate both) represents the same plane with
opposite orientation. Half-spaces care about that orientation.

### Half-spaces and slabs

The closed half-space “on and behind” the plane is

$$
\mathbf{n}\cdot\mathbf{p} \le d.
$$

A **slab** is the region between two parallel planes: $s$ lies in a numeric interval
$[s_{\min}, s_{\max}]$. Ray–AABB tests are slab tests on $X$, $Y$, and $Z$. Separating-
axis collision tests are slab tests on many axes.

### 2D cousin

In the plane, a **half-plane** uses the same Hesse idea with a 2D normal: points
satisfying $\mathbf{n}\cdot\mathbf{p} \le d$. A line equation $Ax+By+C=0$ is the same
geometry with an unnormalized normal.

### Why BSP and clipping love this

Binary space partitioning splits space by a plane, classifies polygons by signed
distance, and recurses. Sutherland–Hodgman clipping cuts a polygon by one half-plane at
a time. Convex volumes are exactly finite intersections of half-spaces.

## In Plato

Planes and half-spaces live in `16-lines.plato` beside lines and rays — the flat affine
primitives.

```plato
// Hesse normal form: Dot(Normal, p) == Distance
type Plane
    implements Geometry3D, Connected, Manifold, Orientable, NearestPoint3D
{
    Normal: Direction3D;
    Distance: Number;
}

// Closed region on and behind Boundary: Dot(Normal, p) <= Distance
// Boundary normal points out of the half-space.
type HalfSpace
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D, NearestPoint3D
{
    Boundary: Plane;
}

type HalfPlane2D
    implements Geometry2D, ConvexShape, Connected, ContainsPoint2D, NearestPoint2D
{
    Normal: Direction2D;
    Distance: Number;
}

type Slab2D
    implements Geometry2D, ConvexShape, Connected, ContainsPoint2D
{
    Normal: Direction2D;
    Interval: NumberInterval;
}

type Slab3D
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D
{
    Normal: Direction3D;
    Interval: NumberInterval;
}
```

`Direction3D` makes the normal unit-length, so `Distance` is a true metric distance from
the origin (in coordinate units). `HalfSpace` is marked `ConvexShape` — any half-space
is convex; intersections of them remain convex.

Usage-shaped sketches:

```plato
let ground = Plane {
    Normal: /* +Z */,
    Distance: 0
};
// Points with Dot(+Z, p) == 0 — the XY plane
// Signed distance of q is Dot(Normal, q) - Distance

let below = HalfSpace { Boundary: ground };
// Contains(p) when Dot(Normal, p) <= Distance  — "underground" if +Z is up? 
// Careful: with +Z up and Distance 0, s = z, so s <= 0 is z <= 0 (below or on ground)

let wallBand = Slab3D {
    Normal: /* +X */,
    Interval: NumberInterval { Start: -1, End: 1 }
};
// |x| <= 1 — infinite vertical slab, workhorse of AABB and SAT tests
```

`NearestPoint3D` on a `Plane` projects by subtracting $s\,\mathbf{n}$. On a
`HalfSpace`, points inside map to themselves; points outside project to the boundary
plane.

A convex polyhedron with outward-facing planes is the intersection of the corresponding
half-spaces. Plato also names that assemblage elsewhere as `ConvexVolume` (array of
`Plane`) — same mathematics, packaged as a solid.

## Pitfalls / fine print

**Normal direction.** “In front” means nothing until you fix whether normals point
outward (common for collision meshes and frustums) or inward. Plato’s `HalfSpace`
comment: normal points **out** of the half-space, and the region is $\le d$.

**Non-unit normals.** If a plane were stored with a non-unit normal, $d$ would scale and
signed “distance” would not be metric. `Direction3D` removes that class of bug; do not
bypass it with a raw `Vector3D` normal without normalizing and rescaling $d$.

**Which side for cameras.** A view frustum plane’s outward normal points out of the
visible volume; a point with $s > 0$ is culled. Getting one plane flipped produces
classic “everything disappears” or “nothing culls” bugs.

**Floating-point on the plane.** $|s| < \varepsilon$ should often count as “on plane.”
Exact $s = 0$ is rare for transformed geometry.

**Parallel slabs.** If a ray is parallel to a slab’s normal axis, the intersection
interval is either empty or all $t$ — handle the degenerate divide-by-zero in ray–box
code.

**2D vs 3D naming.** `HalfPlane2D` inlines normal+distance; `HalfSpace` wraps a `Plane`.
Same idea; different packaging. Do not look for a `HalfSpace2D` name.

## Try it

1. Plane with Normal $= (0,1,0)$, Distance $= 2$. Signed distance of point $(5, 3, -1)$?
   Which side?
2. Same plane. Write the `HalfSpace` inequality. Is $(0, 2, 0)$ inside?
3. Why is a slab of interval $[2, 5]$ along normal $+Y$ the same as intersecting two
   half-spaces?

<details>
<summary>Answers</summary>

1. $s = 3 - 2 = 1 > 0$ — in front of the plane (outside if normal is outward).
2. $y \le 2$. Yes — on the boundary, and closed half-spaces include the boundary.
3. $y \ge 2$ and $y \le 5$ are two opposite-facing half-spaces (normals $-Y$ and $+Y$
   with appropriate distances), or equivalently one slab interval on $+Y$.

</details>

## Library recommendations

- **missing-function** — `16-lines.plato`: `Plane` has field `Distance` but no declared
  `SignedDistance(plane, point)` or `Side` / `Classify` helper. Every clipping lesson
  wants that name on the surface; leaving it implicit invites divergent implementations.

- **missing-function** — `16-lines.plato`: no `Flip(Plane)` / `Flip(HalfSpace)` that
  negates normal and distance together. Orientation bugs are common; an explicit flip
  makes the invariant teachable.

- **missing-function** — `16-lines.plato`: no `FromPointNormal(point, normal) → Plane`
  (sets `Distance = Dot(normal, point)`). Constructing Hesse form from a triangle is the
  usual path into this type.

- **wrong-shape** — `16-lines.plato`: `HalfPlane2D` stores normal+distance inline while
  `HalfSpace` stores `Boundary: Plane`. A `HalfPlane2D` that wrapped a 2D line/Hesse
  type (or a shared pattern) would make the 2D/3D story easier to teach in parallel.

- **doc-comment** — `16-lines.plato`: `Plane.Distance` is “signed distance from the world
  origin.” Emphasize that it is not the distance from an arbitrary reference point, and
  that units match the coordinate frame — easy to misread as a generic offset.
