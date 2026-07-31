---
lesson: sphere-sphere-intersection
title: Sphere–Sphere Intersection
domain: Geometry primitives
v3-files: [18-spatial-primitives.plato, 55-collision.plato]
audience: High-school geometry; comfortable with 3D distance
status: draft-v1
---

# Sphere–Sphere Intersection

Two soap bubbles touch, overlap, or miss. The same classification drives broad-phase
physics (do these `SphereCollider`s need a contact?), constructive solid geometry, and
"how far can I stand from this unit and still be in range" gameplay queries.

The mathematics is elementary — compare the distance between centers to the two radii —
but the *shape* of the intersection is easy to get wrong: when spheres overlap, they do
not meet in a point or a disk filled solid; their boundaries meet in a **circle** lying
in a plane perpendicular to the line of centers.

## The idea

### Setup

Sphere $A$ has center $\mathbf{c}_A$ and radius $r_A$; sphere $B$ has $\mathbf{c}_B$,
$r_B$. Let

$$
\mathbf{d} = \mathbf{c}_B - \mathbf{c}_A,
\qquad
\delta = \|\mathbf{d}\|.
$$

Assume $\delta > 0$ (distinct centers). Along the line of centers, the radical plane that
contains the intersection circle sits at distance $h$ from $\mathbf{c}_A$:

$$
h = \frac{\delta^2 + r_A^2 - r_B^2}{2\delta}.
$$

The intersection circle has radius

$$
\rho = \sqrt{\max\bigl(0,\; r_A^2 - h^2\bigr)},
$$

and its center is $\mathbf{c}_A + (h/\delta)\,\mathbf{d}$.

### Classification by distance

| Condition | Configuration | Boundary intersection |
|-----------|---------------|------------------------|
| $\delta > r_A + r_B$ | Separate | Empty |
| $\delta = r_A + r_B$ | External tangency | Single point |
| $\|r_A - r_B\| < \delta < r_A + r_B$ | Proper overlap | Circle of radius $\rho > 0$ |
| $\delta = \|r_A - r_B\|$ | Internal tangency | Single point |
| $\delta < \|r_A - r_B\|$ | One inside the other, no touch | Empty |
| $\delta = 0$ and $r_A = r_B$ | Coincident spheres | Whole sphere (degenerate) |
| $\delta = 0$ and $r_A \neq r_B$ | Concentric, different size | Empty |

```
  separate     touch      overlap       nested
  (o)  (o)     (o)(o)     (o o)         ( (o) )
```

### Solid vs surface

Plato's `Sphere` type denotes the **ball** — the filled region of points within `Radius`
of `Center` (see the doc comment in `18-spatial-primitives.plato`). Collision overlap
asks whether the *balls* intersect (including boundary). The classical "intersection
circle" is about the *surfaces*. Both questions use the same $\delta$ vs radii tests;
only the interpretation of "intersection set" changes:

- **Solid overlap** if $\delta \le r_A + r_B$ and not ($\delta < |r_A-r_B|$ wait — actually
  nested balls still have solid intersection: the smaller ball is inside the larger).
  Solid balls intersect whenever $\delta \le r_A + r_B$.
- **Surface circle** only in the proper-overlap row of the table above.

For contact generation in physics, you usually want the **shallowest penetration** of
solids: penetration depth $r_A + r_B - \delta$ when overlapping from the outside, with
contact normal along $\mathbf{d}$.

## In Plato

### The sphere type

From `18-spatial-primitives.plato`:

```plato
// The ball of all points within Radius of Center; its boundary is the sphere
// proper. Radius is non-negative.
type Sphere
    implements Geometry3D, ClosedShape, ConvexShape, Connected, SpatialMeasurable,
               Bounded3D, Centroid3D, ContainsPoint3D, NearestPoint3D, SupportMappable3D
{
    Center: Point3D;
    Radius: Number;
}
```

`Centroid` of a solid ball is its `Center`. `Contains` is $\|\mathbf{p}-\mathbf{c}\| \le r$.
`Support` in direction $\mathbf{u}$ is $\mathbf{c} + r\mathbf{u}$ — the GJK building block.

Related types in the same file:

```plato
type SphericalShell   // 0 <= InnerRadius <= OuterRadius
type SphericalCap     // ball cut by a polar angle
type Ellipsoid        // Sphere when SemiAxes are equal
```

### Distance between centers

Centers are `Point3D`. The displacement and length use vector operations:

```plato
var a = Sphere { Center: Point3D { X: 0, Y: 0, Z: 0 }, Radius: 2.0 };
var b = Sphere { Center: Point3D { X: 3, Y: 0, Z: 0 }, Radius: 2.0 };

var delta = Between(a.Center, b.Center);   // Vector3D, B - A
var sep = Length(delta);                   // 3.0
var overlapping = sep <= a.Radius + b.Radius;  // true (3 <= 4)
// Nested (no surface touch): sep + smallerRadius < largerRadius — false here
```

(`Length` on `Vector3D` is declared with the vector intrinsics in `70-intrinsics.plato`.)

### Intersection circle (when surfaces cross)

```plato
// Proper overlap: compute circle center and radius in the radical plane.
var invSep = 1.0 / sep;
var h = (sep * sep + a.Radius * a.Radius - b.Radius * b.Radius) * (0.5 * invSep);
var circleCenter = Add(a.Center, Multiply(delta, h * invSep));
var rhoSq = a.Radius * a.Radius - h * h;
// rho = Sqrt(Max(0, rhoSq)); plane normal = Normalize(delta)
```

v3 does **not** declare a `Circle3D` or `Intersect(Sphere, Sphere)` result type. The
nearest typed cousin for a flat circular patch is `Disk3D`:

```plato
type Disk3D
{
    Center: Point3D;
    Normal: Direction3D;
    Radius: Number;
}
```

A `Disk3D` is the *filled* disk; the boundary circle alone has no dedicated type in
file 18.

### Collision layer: spheres as colliders

From `55-collision.plato`:

```plato
type SphereCollider
    implements Value
{
    Shape: Sphere;
    LocalPose: Pose3D;
}

type ContactPoint3D
{
    Position: Point3D;
    Normal: Direction3D;
    Penetration: Length;
    NormalImpulse: Impulse;
    FrictionImpulse: Vector3D;
}
```

For two sphere colliders transformed into world space, a typical contact is:

```plato
// World centers C_a, C_b; radii r_a, r_b; sep = Length(Between(C_a, C_b))
// When 0 < sep <= r_a + r_b:
var normal = Direction3D { Vector: Normalize(Between(C_a, C_b)) };
var penetration = /* Length quantity from */ (r_a + r_b - sep);
var position = Add(C_a, Multiply(normal.Vector, r_a - 0.5 * (r_a + r_b - sep)));
// pack into ContactPoint3D / ContactManifold3D
```

When `sep` is nearly $0$, the normal is undefined — pick a stable fallback axis.

### Capsules as sphere sweeps

`Capsule3D` is "all points within Radius of the segment from A to B" — the Minkowski
sum of a segment and a ball. Sphere–capsule tests reduce to distance from sphere center
to segment, compared against radius sums; sphere–sphere is the special case where the
segment collapses to a point.

## Pitfalls / fine print

**Solid vs surface.** `Contains` and collision care about balls. Rendering a
"intersection curve" cares about surfaces. Using the solid overlap test to draw a
circle will happily draw when one ball is nested and the surfaces never meet.

**Negative or zero radius.** The type says radius is non-negative; negative radii from
bad data invert the classification. Zero-radius spheres are points.

**Equal centers.** Division by $\delta$ in the circle formula requires a guard. Coincident
equal spheres are a set-equality problem, not a circle.

**Squared distance.** Prefer comparing $\delta^2$ to $(r_A+r_B)^2$ to avoid a square root
when you only need a Boolean overlap — but the circle radius still needs squares carefully
arranged to avoid catastrophic cancellation when $\delta \approx r_A+r_B$.

**Ellipsoids.** Two ellipsoids do **not** reduce to this algebra. Only after a linear
map that turns both into spheres (rarely possible simultaneously) does the test look
similar. Use GJK / `SupportMappable3D` for general convex pairs.

## Try it

1. Spheres at $(0,0,0)$ radius $2$ and $(3,0,0)$ radius $2$. Overlap? Surface circle
   radius $\rho$?

2. Same centers, radii $5$ and $1$. Solid intersection? Surface intersection?

3. Why is penetration for external overlap $r_A+r_B-\delta$ measured as a `Length` on
   `ContactPoint3D`, while `Sphere.Radius` is a bare `Number`?

<details>
<summary>Answers</summary>

1. $\delta=3 < 4$: solid overlap. $h=(9+4-4)/6=1.5$, $\rho=\sqrt{4-2.25}=\sqrt{1.75}$.

2. $\delta=0 < |5-1|$: smaller ball nested inside larger — solids intersect; surfaces do
   not meet (no real $\rho$).

3. Collision is a unit-bearing physics domain (`55-collision.plato` uses meters);
   `Sphere` in the geometry layer is unit-agnostic pure math (`Number`). Bridging them is
   deliberate layering, not an accident.

</details>

## Library recommendations

- **missing-function** — `18-spatial-primitives.plato`: no `Intersect(Sphere, Sphere)` (or
  Boolean `Overlaps`) despite both `ContainsPoint3D` and `SupportMappable3D` being present.
  A sum result `Separate | ExternalTouch | OverlapCircle(Disk3D) | InternalTouch | Nested`
  would encode the classification table.

- **missing-type** — `18-spatial-primitives.plato`: `Disk3D` is a filled patch; there is no
  `Circle3D` (curve only). Surface–surface intersection naturally returns a circle, not a
  disk — the vocabulary nudges authors to over-report a filled region.

- **missing-function** — `55-collision.plato`: no declared helper to build a
  `ContactManifold3D` from two world-space `Sphere` values (or `SphereCollider` + poses).
  Every engine reimplements the same normal/penetration formulas.

- **doc-comment** — `18-spatial-primitives.plato`: the `Sphere` comment carefully says
  "ball" vs "sphere proper"; `SphericalShell` could cross-reference that the region between
  radii is *not* what `Sphere`–`Sphere` overlap means, reducing solid/surface confusion.
