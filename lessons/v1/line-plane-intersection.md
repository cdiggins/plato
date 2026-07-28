---
lesson: line-plane-intersection
title: Line–Plane Intersection
domain: Geometry primitives
v3-files: [16-lines.plato]
audience: High-school vectors; comfortable with parametric equations
status: draft-v1
---

# Line–Plane Intersection

Where does a laser hit a wall? Where does a camera ray meet a ground plane? Where does
an edge of a CAD solid pierce a cutting plane? All three are the same geometric query:
intersect an infinite line (or a ray, or a segment) with a plane.

The algebra is short — one division — but the edge cases are where production code
fails: lines parallel to the plane, rays that point away, segments that stop short, and
planes that were never normalized.

## The idea

### Parametric line

An infinite line through point $\mathbf{o}$ with unit direction $\mathbf{d}$ is

$$
\mathbf{p}(t) = \mathbf{o} + t\,\mathbf{d}, \qquad t \in \mathbb{R}.
$$

- $t = 0$ is the origin.
- $t > 0$ is forward along $\mathbf{d}$.
- $t < 0$ is backward.

A **ray** uses the same formula but restricts $t \ge 0$. A **segment** from $A$ to $B$
can be written $\mathbf{p}(t) = A + t\,(B-A)$ with $t \in [0,1]$, or as a line whose
direction is the normalized $(B-A)$ with a bounded parameter interval.

### Plane in Hesse form

A plane with unit normal $\mathbf{n}$ and signed origin distance $d$ is the set of
points satisfying

$$
\mathbf{n}\cdot\mathbf{p} = d.
$$

The value $\mathbf{n}\cdot\mathbf{q} - d$ is the **signed distance** from point $\mathbf{q}$
to the plane: positive on the side $\mathbf{n}$ points toward, negative on the other,
zero on the plane.

### Solving for the hit

Substitute the line into the plane equation:

$$
\mathbf{n}\cdot(\mathbf{o} + t\,\mathbf{d}) = d
\qquad\Rightarrow\qquad
t = \frac{d - \mathbf{n}\cdot\mathbf{o}}{\mathbf{n}\cdot\mathbf{d}}.
$$

Then $\mathbf{p}(t)$ is the intersection point — when the denominator is not zero.

```
            n
            ^
            |     p(t)
   =========+========  plane
           /
          /
         o ----d---->
```

### Parallel and coincident cases

If $\mathbf{n}\cdot\mathbf{d} \approx 0$, the line is parallel to the plane:

| Also true | Meaning |
|-----------|---------|
| $|\mathbf{n}\cdot\mathbf{o} - d| \approx 0$ | Line lies *in* the plane (infinitely many hits) |
| otherwise | Line never meets the plane |

There is no single point answer in either subcase; callers need an explicit status.

### Rays and segments

After computing $t$ for the infinite line:

- **Ray:** accept only $t \ge 0$.
- **Segment** with $\mathbf{p}(t)=\mathbf{o}+t\mathbf{d}$ and $\mathbf{d}$ unit: accept
  $t$ in $[0, L]$ where $L$ is segment length; or use $t\in[0,1]$ with non-unit
  $\mathbf{d}=B-A$ and the formula adjusted ($t = (d-\mathbf{n}\cdot A)/(\mathbf{n}\cdot(B-A))$).

## In Plato

### The three "straight" types and the plane

From `16-lines.plato`:

```plato
type Line3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

type Ray3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

type LineSegment3D
    implements Geometry3D, OpenShape, Connected, Bounded3D, LengthMeasurable,
               PointSet3D, Centroid3D, NearestPoint3D, Deformable3D
{
    A: Point3D;
    B: Point3D;
}

type Plane
    implements Geometry3D, Connected, Manifold, Orientable, NearestPoint3D
{
    Normal: Direction3D;
    Distance: Number;
}
```

`Direction3D` wraps a unit `Vector3D`, so `Line3D.Direction` and `Ray3D.Direction` are
normalized by type. `LineSegment3D` stores endpoints; its direction is derived.

### Reading the plane fields

`Plane.Distance` is the Hesse $d$, **not** a `Length` quantity and not "distance from
camera to plane." For a plane through the origin, `Distance` is $0$. For the plane
$y = 5$ with normal $(0,1,0)$, `Distance` is $5$.

### Usage-shaped intersection (illustrative)

v3 does **not** declare an `Intersect(line: Line3D, plane: Plane)` function. The query
is expressed from fields:

```plato
var line = Line3D {
    Origin: Point3D { X: 0, Y: 10, Z: 0 },
    Direction: Direction3D { Vector: Vector3D { X: 0, Y: -1, Z: 0 } }
};
var ground = Plane {
    Normal: Direction3D { Vector: Vector3D { X: 0, Y: 1, Z: 0 } },
    Distance: 0.0
};

var n = ground.Normal.Vector;
var d = ground.Distance;
var o = line.Origin;           // need PositionVector for dots with Vector3D
var dir = line.Direction.Vector;

var denom = Dot(n, dir);
// if |denom| is below a tolerance → parallel / coincident branch

var t = (d - Dot(n, PositionVector(o))) / denom;
var hit = Add(o, Multiply(dir, t));
```

For a ray, add `t >= 0`. For a segment from `A` to `B`:

```plato
var seg = LineSegment3D { A: a, B: b };
var delta = Between(seg.A, seg.B);   // B - A
var denom = Dot(n, delta);
var t = (d - Dot(n, PositionVector(seg.A))) / denom;
// accept when t is in [0, 1]
var hit = Lerp(seg.A, seg.B, t);
```

### Half-spaces as "which side"

```plato
type HalfSpace
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D, NearestPoint3D
{
    Boundary: Plane;
}
```

`HalfSpace` is the closed region $\mathbf{n}\cdot p \le d$. Clipping a segment to a
half-space is the same intersection math, keeping the portion on the inside.

### Slabs: two parallel planes

```plato
type Slab3D
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D
{
    Normal: Direction3D;
    Interval: NumberInterval;
}
```

A slab is the region whose signed distance lies in an interval — the workhorse of
ray-box tests as three slabs, one per axis. Intersecting a line with a slab means
intersecting with two planes and taking the overlapping $t$-interval.

### 2D analogue

`Line2D` / `Ray2D` / `LineSegment2D` plus `LineEquation2D` ($Ax+By+C=0$) or
`HalfPlane2D` play the same roles in the plane. The parameter solve is identical with
2D dots.

## Pitfalls / fine print

**Unnormalized normals.** `Plane.Normal` is a `Direction3D`, so it should be unit. If you
construct a plane from a raw cross product and forget to wrap it as `Direction3D`
properly, `Distance` stops meaning signed distance and the formula's $t$ drifts.

**Denominator epsilon.** Comparing `denom` to exact zero is fragile. Use a tolerance
scaled to your world size; a line nearly parallel to a wall may "hit" at an enormous $t$.

**Ray vs line.** Picking a point behind the camera ($t < 0$) is the classic
"intersection with the plane behind me" bug. Always gate on $t$ for `Ray3D`.

**Segment parameterization.** Mixing unit-direction $t\in[0,L]$ with lerp $t\in[0,1]$
without converting is a frequent off-by-$L$ error.

**No hit type in file 16.** Ray hits in the broader vocabulary live as `RayHit3D` in the
spatial-structures layer (`35`), not beside `Line3D`. File 16 gives you the primitives;
it does not give you a result record for this query.

**Coincident lines.** Returning one arbitrary point when the line lies in the plane
hides the true geometry. Prefer an explicit enum-like status (miss / point / line) even
though v3 does not yet declare one for this pair.

## Try it

1. Line origin $(0,5,0)$, direction $(0,-1,0)$, plane normal $(0,1,0)$, distance $0$.
   What is $t$ and the hit point?

2. Same line and plane, but direction $(1,0,0)$. What happens?

3. Segment $A=(0,1,0)$, $B=(0,-1,0)$, same ground plane. Using $t\in[0,1]$ on $A+t(B-A)$,
   what is $t$ at the hit? Is it inside the segment?

<details>
<summary>Answers</summary>

1. $\mathrm{denom} = (0,1,0)\cdot(0,-1,0) = -1$,
   $t = (0 - 5)/(-1) = 5$, hit $(0,0,0)$.

2. $\mathrm{denom} = 0$ and $|\mathbf{n}\cdot\mathbf{o}-d|=5\neq 0$: parallel, no
   intersection.

3. $\delta=(0,-2,0)$, $\mathrm{denom}=-2$, $t=(0-1)/(-2)=1/2$. Yes — midpoint $(0,0,0)$.

</details>

## Library recommendations

- **missing-function** — `16-lines.plato`: no `Intersect(Line3D, Plane)`,
  `Intersect(Ray3D, Plane)`, or `Intersect(LineSegment3D, Plane)` despite these being the
  most common queries on the file's own types. A sum-typed result
  (`Miss | Point(Point3D) | Line(Line3D)` for the infinite case) would match v3's sum-type
  conventions.

- **missing-function** — `16-lines.plato`: no `SignedDistance(plane: Plane, point: Point3D)`
  even though the Hesse form makes it a one-liner and every intersection/side test needs
  it.

- **missing-type** — `16-lines.plato`: no local hit/result type for line–plane queries;
  authors reach for `RayHit3D` from file 35 or invent ad hoc tuples. A small
  `PlaneHit3D { Point: Point3D; Parameter: Number }` beside the primitives would keep
  foundation geometry self-contained.

- **doc-comment** — `16-lines.plato`: `Plane.Distance` should state the unit-normal
  precondition in the field comment (it is implied by `Direction3D` but readers still
  confuse $d$ with Euclidean distance to an arbitrary point).

> Resolved 2026-07-28: lines.plato now declares PlaneHit3D (item 207) and Intersect(Line3D/Ray3D/LineSegment3D, Plane) returning it (205); SignedDistance(Plane, Point3D) already existed (206); the Plane.Distance field doc-comment now states the unit-normal precondition (208).
