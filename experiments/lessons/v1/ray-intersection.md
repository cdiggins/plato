---
lesson: ray-intersection
title: Ray Intersection
domain: Geometry primitives
v3-files: [16-lines.plato, 18-spatial-primitives.plato, 35-spatial-queries.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Ray Intersection

A laser pointer, a bullet trajectory, a mouse pick into a 3D view, and a primary camera
ray in a ray tracer are the same geometric object: a half-line that starts somewhere and
travels in one direction until it hits something — or doesn't. Computing that first hit
is **ray intersection**, and three cases dominate practice: ray vs plane, ray vs sphere,
and ray vs box.

The algebra is short. The engineering is in conventions: what $t$ means, which hit to
keep when there are two, how to report a miss, and how to package the answer so a
renderer, a physics engine, and a picking tool can share it.

## The idea

A **ray** is the set of points

$$
P(t) = O + t\,D, \quad t \ge 0
$$

where $O$ is the origin, $D$ is a unit direction, and $t$ is distance along the ray
(when $D$ is unit length). Contrast:

| Object | Parameter range |
|---|---|
| Line segment | $t \in [0, 1]$ between two endpoints |
| Ray | $t \ge 0$ from an origin |
| Infinite line | any real $t$ |

### Ray vs plane

A plane in Hesse form: points $p$ with $\mathrm{Dot}(N, p) = d$. Substitute the ray:

$$
\mathrm{Dot}(N, O + t D) = d \implies t = \frac{d - \mathrm{Dot}(N, O)}{\mathrm{Dot}(N, D)}
$$

If the denominator is ~0, the ray is parallel to the plane (miss, or lies in the plane).
If $t < 0$, the intersection is behind the origin (miss for a ray). Otherwise the hit
position is $O + t D$, and the outward normal is $N$ or $-N$ depending on which side
you treat as "outside."

### Ray vs sphere

Sphere center $C$, radius $r$. Set $\|O + t D - C\|^2 = r^{2}$:

$$
t^{2} + 2\,t\,\mathrm{Dot}(D, O-C) + \|O-C\|^2 - r^{2} = 0
$$

(assuming $\|D\| = 1$). Discriminant $\Delta < 0$ means miss; $\Delta = 0$ means
tangent; $\Delta > 0$ yields two roots. Keep the smallest $t \ge 0$. The outward normal
at the hit is $\mathrm{Normalize}(P - C)$.

```
        O -----> D
           \
            \  t_near
             ●========●  sphere
            /  t_far
```

### Ray vs box (slabs)

An axis-aligned box is the intersection of three **slabs** — regions between parallel
planes. For each axis, compute the $t$ interval where the ray is inside that slab, then
intersect the three intervals. If the combined interval $[t_{enter}, t_{exit}]$ is
empty or $t_{exit} < 0$, miss; else the hit is at $\max(t_{enter}, 0)$.

Oriented boxes use the same idea after transforming the ray into the box's local frame
(or testing slabs along the box axes).

## In Plato

### Rays and planes (`16-lines.plato`)

```plato
type Ray3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

type Plane
    implements Geometry3D, Connected, Manifold, Orientable, NearestPoint3D
{
    Normal: Direction3D;
    Distance: Number;
}

type Slab3D
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D
{
    Normal: Direction3D;
    Interval: NumberInterval;
}
```

`Direction3D` is unit-length by construction, so $t$ on a `Ray3D` is world distance.
`Plane.Distance` is the signed distance from the world origin along `Normal` — Hesse
form, not a point-on-plane plus normal.

### Targets (`18-spatial-primitives.plato`)

```plato
type Sphere
{
    Center: Point3D;
    Radius: Number;
}

type Box3D
{
    Center: Point3D;
    Size: Size3D;
    Orientation: Quaternion;
}
```

`Box3D` is oriented; the slab method either transforms the ray by the inverse
orientation or builds three `Slab3D` values from the box axes.

### Queries and hits (`35-spatial-queries.plato`)

Intersection is not only an equation — it is a *request* and a *result*:

```plato
type RayQuery3D
    implements Value
{
    Ray: Ray3D;
    MaxDistance: Number;
    FilterMask: Integer;
}

type RayHit3D
    implements Value
{
    Hit: Boolean;
    Distance: Number;
    Position: Point3D;
    Normal: Direction3D;
    Face: FaceIndex;
    Barycentric: BarycentricCoordinate;
    Uv: UvCoordinate;
}

concept RayIntersectable3D
{
    Raycast(x: Self, query: RayQuery3D): RayHit3D;
}
```

Usage-shaped sketches:

```plato
ray := Ray3D(Origin: eye, Direction: look)
query := RayQuery3D(Ray: ray, MaxDistance: 1000, FilterMask: -1)

hit := Raycast(sphere, query)
// hit.Hit, hit.Distance, hit.Position, hit.Normal

hit2 := Raycast(box, query)
```

Conventions from the file banner: distances are world-space and non-negative;
`MaxDistance` of zero or negative means unbounded; `FilterMask` of `-1` accepts
everything. When `Hit` is false, the remaining fields are meaningless.

For mesh targets, `Face`, `Barycentric`, and `Uv` locate the hit on a triangle. Against
an analytic `Sphere` or `Box3D`, expect `Face` to be the "none" sentinel (`-1` as
`FaceIndex`) and UV/barycentric to be unused defaults.

A thicker cousin of the ray is the sphere sweep:

```plato
type SweepQuery3D
{
    Ray: Ray3D;
    Radius: Number;
    MaxDistance: Number;
    FilterMask: Integer;
}
```

Same result shape idea — first contact of a moving ball — used for character controllers
and "fat" picking.

## Pitfalls / fine print

**Non-unit directions.** Plato's `Direction3D` is normalized. If you build a ray from a
raw vector difference, normalize first; otherwise $t$ is not distance and sphere
quadratics that assume $\|D\|=1$ go wrong.

**Two roots, one hit.** Always prefer the smallest $t \ge 0$ within `MaxDistance`. The
far root matters for exit points (shadow volumes, CSG) but not for "first hit" shading.

**Origin inside the solid.** For a sphere, one root is negative and one positive — the
positive root is the *exit*. First-hit rendering often wants that exit, or a special
"started inside" flag. `RayHit3D` has no inside-start field; callers must detect
`Contains(sphere, ray.Origin)` themselves.

**Plane sidedness.** The formula above can hit the backface. Renderers that cull
backfaces reject hits where $\mathrm{Dot}(N, D) > 0$ (ray and normal point the same
way). Physics contact often wants either side.

**AABB vs `Box3D`.** Slab tests are cheapest on axis-aligned bounds (`Bounds3D`). An
oriented `Box3D` needs a frame change. Do not silently ignore `Orientation`.

**Degenerate slabs.** A zero-thickness interval or a ray parallel to a slab pair needs an
epsilon policy. Floating-point "exactly parallel" is rare; "nearly parallel with huge $t$"
is common.

**Miss encoding.** `RayHit3D.Hit == false` is the miss signal. Do not overload
`Distance = Infinity` without checking `Hit` — the doc comment says other fields are
meaningless on a miss.

## Try it

1. Ray origin $(0,0,0)$, direction $(1,0,0)$, sphere center $(5,0,0)$, radius $1$.
   What are the two $t$ values, and which is the first hit?
2. Same ray, plane with normal $(0,1,0)$ and distance $0$ (the $xz$-plane). Does the
   ray hit?
3. Why store both `Distance` and `Position` on `RayHit3D` when one determines the other?

<details>
<summary>Answers</summary>

1. Solve $(t-5)^{2} = 1$ → $t = 4$ and $t = 6$. First hit at $t = 4$, position $(4,0,0)$.
2. Denominator $\mathrm{Dot}((0,1,0),(1,0,0)) = 0$ and the origin is on the plane —
   parallel and contained, or treat as degenerate; there is no unique isolated hit.
3. `Distance` sorts and compares hits without reconstructing points; `Position` is what
   shading and constraints consume. Redundant storage beats repeated multiply-adds and
   clarifies the record when the direction might later be non-unit in other APIs.

</details>

## Library recommendations

- **wrong-shape** — `35-spatial-queries.plato`: `RayHit3D` always carries mesh fields
  (`Face`, `Barycentric`, `Uv`) even for analytic targets. A sum type
  (`AnalyticHit | MeshHit(...)`) or optional sentinels documented per target would make
  the "meaningless on sphere" case explicit instead of relying on `-1` and `(0,0)`.

- **missing-function** — `35-spatial-queries.plato`: `RayIntersectable3D` declares
  `Raycast` but there is no companion `RaycastAny` / early-out boolean, and no
  `RaycastAll` returning multiple hits. Shadow rays and CSG need those shapes.

- **missing-type** — `35-spatial-queries.plato`: no `RayHit3D` field or adjacent type
  for "origin inside" / entry-vs-exit. Teaching ray-sphere with an interior start has to
  leave `RayHit3D` and call `ContainsPoint3D` separately.

- **doc-comment** — `16-lines.plato`: `Ray3D` states $t \ge 0$ but does not say that
  `Direction` is unit-length so $t$ equals world distance. One sentence tying
  `Direction3D`'s invariant to distance parameterization would lock the algebra to the
  type.
