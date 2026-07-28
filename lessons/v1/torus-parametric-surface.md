---
lesson: torus-parametric-surface
title: Torus as Solid and Parametric Surface
domain: Spatial primitives & surfaces
v3-files: [18-spatial-primitives.plato]
audience: Comfortable with 3D points, circles, and UV parameters; no differential geometry assumed.
status: draft-v1
---

# Torus as Solid and Parametric Surface

A doughnut is the everyday model of a **torus**: a circle swept around another circle.
In solid modeling you ask "is this point inside the tube?" In rendering and CAD you often
need the **surface** — a map from UV parameters to points in space, closed in both
directions like a tire's skin.

Plato's `Torus` in `18-spatial-primitives.plato` is the solid form: center, axis, major
and minor radii. The same geometry underlies a `ParametricSurface` evaluation. This
lesson connects the solid fields to the classic $(u,v)$ parameterization and the
self-intersection case when the tube is thicker than the ring.

## Two radii, one axis

Fix a center $C$, a unit axis $\mathbf{n}$, a **major radius** $R$ (distance from $C$
to the tube centerline), and a **minor radius** $r$ (tube radius).

```
              axis n
                ↑
                |
           , - ~ ~ - ,
       , '   tube r    ' ,
     ,      .-------.      ,
    ,      /    R    \      ,
    |     (     ●C    )     |
    ,      \         /      ,
     ,      '-------'      ,
       ' ,             , '
           ' - , _ , - '
```

The **solid** torus is every point whose distance to the major circle (centerline) is at
most $r$. The **surface** is the boundary — distance exactly $r$.

| Field | Meaning |
|-------|---------|
| `Center` | Ring center |
| `Axis` | Normal to the plane of the major circle |
| `MajorRadius` | $R$ — centerline radius |
| `MinorRadius` | $r$ — tube radius |

**Self-intersection.** When $r > R$, the tube overlaps itself through the hole. Plato's
doc comment states this explicitly; the type still stores the numbers — validity is a
modeling concern, not a type-system refusal.

### Standard UV parameterization

Place $C$ at the origin and $\mathbf{n}$ along $+Z$ for a moment. With angles
$u, v \in [0, 2\pi)$:

$$
\begin{aligned}
x &= (R + r\cos v)\cos u \\
y &= (R + r\cos v)\sin u \\
z &= r\sin v
\end{aligned}
$$

- $u$ walks around the major circle (the hole).
- $v$ walks around the tube cross-section.

On the unit square used by Plato's `ParametricSurface` concept, map
$u = 2\pi\,U$, $v = 2\pi\,V$ with `UvCoordinate` components in $[0,1]$.

```
  V (tube)     closed: V=0 meets V=1
    ↑
    |  ··············
    |  ·            ·
    |  ·   surface  ·
    |  ·            ·
    |  ··············
    +----------------→ U (around hole)
         closed: U=0 meets U=1
```

Both seams close: a torus is the textbook example of `ClosedU` and `ClosedV` both true.

### Worked example: unit ring

`MajorRadius = 2`, `MinorRadius = 0.5`, center at origin, axis $+Z$.

| $(U,V)$ | Angles | Point (approx) |
|---------|--------|----------------|
| $(0,0)$ | $u=0,v=0$ | $(2.5, 0, 0)$ outer equator |
| $(0,0.5)$ | $u=0,v=\pi$ | $(1.5, 0, 0)$ inner equator |
| $(0.25,0)$ | $u=\pi/2,v=0$ | $(0, 2.5, 0)$ |
| $(0,0.25)$ | $u=0,v=\pi/2$ | $(2, 0, 0.5)$ top of tube |

Volume of the solid (when $r \le R$) is $(2\pi R)(\pi r^2) = 2\pi^2 R r^2$. Surface area
is $(2\pi R)(2\pi r) = 4\pi^2 R r$.

## In Plato

The solid declaration:

```plato
// A solid torus: all points within MinorRadius of the circle of MajorRadius
// about Center in the plane perpendicular to Axis. Self-intersecting when
// MinorRadius exceeds MajorRadius.
type Torus
    implements Geometry3D, ClosedShape, Connected, SpatialMeasurable,
               Bounded3D, Centroid3D, ContainsPoint3D
{
    Center: Point3D;
    Axis: Direction3D;
    MajorRadius: Number;
    MinorRadius: Number;
}
```

`SpatialMeasurable` supplies `Volume` and `SurfaceArea`. `ContainsPoint3D` answers the
solid query. `Bounded3D` yields an axis-aligned `Bounds3D` enclosure.

```
var donut = Torus(
    Point3D(0.0, 0.0, 0.0),
    Direction3D(Vector3D(0.0, 0.0, 1.0)),
    2.0,
    0.5);

var inside = Contains(donut, Point3D(2.0, 0.0, 0.0));  // on centerline → true
var hole   = Contains(donut, Point3D(0.0, 0.0, 0.0));  // through the hole → false
```

### Relating to `ParametricSurface`

The surface concept in the curves/surfaces layer:

```plato
concept ParametricSurface
    inherits Surface, Procedural<UvCoordinate, Point3D>
{
    ClosedU(x: Self): Boolean;
    ClosedV(x: Self): Boolean;
}
```

`Torus` itself is declared as a solid (`Geometry3D`, `ClosedShape`), not as
`ParametricSurface`. The UV formulas above are how you *evaluate* the boundary; a
future library function or a thin wrapper type would implement `Eval(torus, uv)` and
report `ClosedU`/`ClosedV` as true.

`DifferentiableSurface` would add tube/ring tangents and an outward normal:

```plato
concept DifferentiableSurface
    inherits ParametricSurface
{
    TangentUAt(x: Self, uv: UvCoordinate): Vector3D;
    TangentVAt(x: Self, uv: UvCoordinate): Vector3D;
    NormalAt(x: Self, uv: UvCoordinate): Direction3D;
}
```

### Oriented placement

`Axis` is a `Direction3D` (unit `Vector3D`). Rotating the torus is changing that
direction and optionally `Center` — there is no separate quaternion field on `Torus`.
If you already have a `Pose3D`, apply it to generated surface points, or transform the
center and axis consistently.

Neighbor primitives in the same file (`Sphere`, `Cylinder`, `Capsule3D`, `Ellipsoid`)
follow the same pattern: minimal defining data, measures via concepts, no baked mesh.

## Pitfalls and fine print

**Major vs minor.** Swapping $R$ and $r$ changes the shape drastically. Major is the
ring; minor is the tube. Names are explicit — use them, do not abbreviate to
"radius" alone.

**$r > R$.** The type allows it. `Contains` and volume formulas for the non-overlapping
case no longer match intuition; treat as a modeling error unless you intentionally want
a spindle torus.

**Solid vs surface.** `ContainsPoint3D` is about the filled tube. Points on the
mathematical surface satisfy equality of distance to the centerline with $r$; interior
points are strictly closer.

**UV singularity of language, not geometry.** The torus surface is smooth; the
parameterization has periodic seams. Do not confuse seam wrapping with a geometric cusp
(the cardioid has a cusp; the standard torus surface does not).

**Centroid.** For a uniform solid torus with $r \le R$, the centroid is `Center`.
Self-intersecting cases need care; rely on the concept implementation's documented
convention.

## Try it

<details>
<summary>Exercise 1 — Outer and inner equators</summary>

`MajorRadius = 3`, `MinorRadius = 1`, axis $+Z$, center origin. What are the points at
$(U,V) = (0,0)$ and $(0,0.5)$ under $u=2\pi U$, $v=2\pi V$?

**Answer.** $(0,0)$ → $(R+r, 0, 0) = (4,0,0)$. $(0,0.5)$ → $(R-r, 0, 0) = (2,0,0)$.
</details>

<details>
<summary>Exercise 2 — Contains through the hole</summary>

Same torus. Is `Point3D(0,0,0)` inside the solid? Is `Point3D(3,0,0)` inside?

**Answer.** Origin is in the hole (distance to centerline is $R=3 > r$) → outside.
`(3,0,0)` lies on the major circle → distance $0 \le r` → inside.
</details>

<details>
<summary>Exercise 3 — Closure flags</summary>

For a torus surface evaluated as a `ParametricSurface`, what should `ClosedU` and
`ClosedV` return?

**Answer.** Both `true` — the surface seams shut in the ring direction and the tube
direction.
</details>

## Library recommendations

- **missing-concept** — `18-spatial-primitives.plato`: `Torus` is a solid only. There is
  no declared `ParametricSurface` (or boundary-surface) view, so UV evaluation and
  `ClosedU`/`ClosedV` live only as folklore. A `TorusSurface` type or
  `implements ParametricSurface` on a boundary companion would make the UV map first-class.

- **missing-function** — no declared `Eval(Torus, UvCoordinate): Point3D`,
  `CenterlinePoint`, or `DistanceToCenterline`. Every mesher reinvents the standard
  formulas; naming them on the solid would lock conventions (angle zero, normal sense).

- **doc-comment** — the self-intersection note is good, but the comment does not state
  the volume/surface-area formulas for the $r \le R$ case. Adding them would match how
  `SpatialMeasurable` is taught elsewhere.

- **pedagogy** — `Direction3D` for `Axis` is correct, yet authors often pass a non-unit
  `Vector3D`. A factory `Torus.Create(center, axisVector, R, r)` that normalizes (or
  refuses) would reduce silent tilt errors when the axis is built from two points.
