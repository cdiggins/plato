---
lesson: solid-primitives
title: Solid Primitives
domain: Geometry primitives
v3-files: [18-spatial-primitives.plato, 25-solids.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Solid Primitives

A game level needs a barrel, a pipe, a ball, and a rounded pillar. You could mesh each
from scratch, or you could recognize that every one of them is a handful of numbers —
center, radius, height, axis — describing a *filled region* of space. Those filled
regions are **solid primitives**: the vocabulary of everyday 3D modeling before
booleans, sweeps, and free-form surfaces enter the picture.

The surprise is how far a short list goes. Spheres, boxes, cylinders, cones, capsules,
and tori already cover collision proxies, CSG leaves, LOD stand-ins, and most of the
shapes you sketch on a whiteboard. Plato's v3 library treats them as first-class
geometry with measured volume and surface area, not as mesh templates with loose
parameters.

## The idea

A **solid** is a closed, bounded region of 3D space — the volume *and* its watertight
boundary. Contrast that with a flat patch (a disk or triangle in space), which has area
but no enclosed volume. The classical primitives are the ones with closed-form formulas
for volume and surface area, and with parameters that map cleanly onto how humans
describe them.

### Sphere

The ball of all points within `Radius` of `Center`. Volume $\frac{4}{3}\pi r^{3}$,
surface area $4\pi r^{2}$. Radius is non-negative; zero is a degenerate point-ball.

```
         . -- .
       /         \
      |     C     |   radius r
       \         /
         ' -- '
```

### Box

An oriented solid box: `Size` is the *full* extents along local axes, `Orientation`
rotates those axes about `Center`. Axis-aligned when orientation is identity. Volume is
simply the product of the three extents; surface area is twice the sum of pairwise
extent products.

### Cylinder

A right circular cylinder with flat caps. `Center` is the midpoint of the axis segment;
the solid extends `Height/2` each way along `Axis`. Volume $\pi r^{2} h$; lateral area
$2\pi r h$; total surface area adds the two disk caps $2\pi r^{2}$.

### Cone

A right circular cone. `Axis` points from `Apex` toward the base; the base is the disk
of `Radius` at distance `Height` along the axis. Volume $\frac{1}{3}\pi r^{2} h$. The
slant height $\ell = \sqrt{r^{2}+h^{2}}$ gives lateral area $\pi r \ell$.

### Capsule

All points within `Radius` of the segment from `A` to `B`: a cylinder with hemispherical
ends. Beloved in physics because it is smooth, convex, and cheap to test. Volume is the
cylinder volume plus a full sphere of the same radius (two hemispheres).

### Torus

All points within `MinorRadius` of a circle of `MajorRadius` about `Center` in the plane
perpendicular to `Axis`. Volume $(2\pi R)(\pi r^{2}) = 2\pi^{2} R r^{2}$; surface area
$4\pi^{2} R r$. Self-intersecting (a spindle torus) when the minor radius exceeds the
major — still a valid solid description, but no longer a manifold without self-contact.

```
   major R ----+
               |
          .----o----.     o = tube centerline circle
         /     |     \    r = minor (tube) radius
        |   .--+--.   |
         \     |     /
          '----o----'
```

## In Plato

File `18-spatial-primitives.plato` declares the filled primitives. Each implements
`Geometry3D`, `ClosedShape`, and — for the measurable ones — `SpatialMeasurable`
(`Volume` and `SurfaceArea`). Most also implement `ContainsPoint3D`, `Bounded3D`, and
`Centroid3D`.

```plato
type Sphere
    implements Geometry3D, ClosedShape, ConvexShape, Connected,
               SpatialMeasurable, Bounded3D, Centroid3D, ContainsPoint3D,
               NearestPoint3D, SupportMappable3D
{
    Center: Point3D;
    Radius: Number;
}

type Box3D
    implements Geometry3D, ClosedShape, ConvexShape, Connected,
               SpatialMeasurable, Bounded3D, Centroid3D, ContainsPoint3D,
               NearestPoint3D, SupportMappable3D
{
    Center: Point3D;
    Size: Size3D;
    Orientation: Quaternion;
}

type Cylinder
{
    Center: Point3D;
    Axis: Direction3D;
    Radius: Number;
    Height: Number;
}

type Cone
{
    Apex: Point3D;
    Axis: Direction3D;
    Height: Number;
    Radius: Number;
}

type Capsule3D
{
    A: Point3D;
    B: Point3D;
    Radius: Number;
}

type Torus
{
    Center: Point3D;
    Axis: Direction3D;
    MajorRadius: Number;
    MinorRadius: Number;
}
```

Usage-shaped sketches:

```plato
ball := Sphere(Center: Point3D(0, 0, 0), Radius: 1)
Volume(ball)          // 4/3 * pi
SurfaceArea(ball)     // 4 * pi
Contains(ball, Point3D(0.5, 0, 0))  // true

pillar := Cylinder(
    Center: Point3D(0, 0, 1),
    Axis: Direction3D(0, 0, 1),
    Radius: 0.3,
    Height: 2)

proxy := Capsule3D(
    A: Point3D(0, 0, 0),
    B: Point3D(0, 0, 2),
    Radius: 0.4)
```

Related shapes in the same file round out the toolkit: `Ellipsoid` (oriented semi-axes),
`SphericalShell` (inner/outer radius), `SphericalCap`, `ConicalFrustum`, `RoundedBox3D`,
`Tetrahedron`, and `ConvexVolume` (intersection of half-spaces). Flat patches —
`Disk3D`, `Triangle3D`, `Quad3D` — live alongside them but are surfaces, not solids.

File `25-solids.plato` lifts the story one level: the `Solid` interface marks any closed
bounded volume, and constructive / profile-generated solids reuse primitives as leaves.

```plato
interface Solid
    inherits Geometry3D
{ }

type ExtrudedSolid implements Solid
{
    Profile: PolygonWithHoles2D;
    Placement: Frame3D;
    Height: Number;
}

type RevolvedSolid implements Solid
{
    Profile: PolygonWithHoles2D;
    Axis: Line3D;
    Angles: AngleInterval;
}
```

A `CsgTree3D` stores a flat array of `CsgNode3D` nodes (`Leaf` with a `PrimitiveIndex`,
or `Interior` with a `CsgOperation`). The classic primitives of file 18 are exactly what
those leaves point at — sphere minus box, cylinder union capsule, and so on.

```plato
type CsgOperation
    = Union
    | Intersection
    | Difference
    | SmoothUnion(Radius: Number)
    | SmoothIntersection(Radius: Number)
    | SmoothDifference(Radius: Number);
```

## Pitfalls / fine print

**Size vs half-extents.** `Box3D.Size` is full extents. Graphics APIs often store
half-sizes. Feeding a "radius-like" triple into `Size` silently doubles the box.

**Cylinder center vs base.** Plato's cylinder is centered on the midpoint of its axis
segment. Code that places a cylinder by its base point must offset by `Height/2` along
`Axis`.

**Cone axis direction.** `Axis` points from apex toward the base. Reversing it puts the
base on the wrong side of the apex.

**Capsule vs cylinder.** A capsule includes hemispherical caps; its length along the
segment is `|B−A| + 2r`, not `|B−A|`. Collision code that treats capsules as cylinders
will under-estimate extent.

**Torus self-intersection.** When `MinorRadius > MajorRadius` the solid intersects
itself. `Contains` and volume formulas still make sense; meshing and outward normals do
not without extra care. `Torus` deliberately does not implement `ConvexShape`.

**Orientation on boxes and ellipsoids.** `Quaternion` identity means axis-aligned. For a
pure AABB, prefer `Bounds3D` (file 12) over a `Box3D` with identity orientation — the
doc comment on `Box3D` says so explicitly.

**Solids vs surfaces.** `Sphere` denotes the *ball* (filled). The hollow spherical shell
is `SphericalShell`. Naming in English drifts ("sphere" vs "ball"); Plato's types do not.

## Try it

1. A unit sphere has volume $4\pi/3$. What is the volume of a `SphericalShell` with
   inner radius $1$ and outer radius $2$?
2. A `Capsule3D` has `A = (0,0,0)`, `B = (0,0,2)`, `Radius = 1`. Express its volume as
   cylinder volume plus sphere volume.
3. Why does `Torus` omit `ConvexShape` from its `implements` list while `Capsule3D`
   includes it?

<details>
<summary>Answers</summary>

1. Outer ball volume minus inner ball volume:
   $\frac{4}{3}\pi(8 - 1) = \frac{28}{3}\pi$.
2. Cylinder of radius 1 and height 2, plus a unit sphere:
   $\pi(1)^{2}(2) + \frac{4}{3}\pi(1)^{3} = 2\pi + \frac{4}{3}\pi = \frac{10}{3}\pi$.
3. A torus has a hole; the segment between two opposite points on the tube can leave the
   solid. A capsule is the Minkowski sum of a segment and a ball, which is always convex.

</details>

## Library recommendations

- **missing-function** — `18-spatial-primitives.plato`: primitives implement
  `SpatialMeasurable` (`Volume`, `SurfaceArea`) but v3 declares no evaluation helpers
  such as `LateralSurfaceArea` vs total area for `Cylinder`/`Cone`, and no
  `SlantHeight(Cone)` — teaching surface-area breakdowns has to invent those pieces.

- **doc-comment** — `18-spatial-primitives.plato`: `Sphere` correctly says it is the
  ball, but the type name remains `Sphere`. A one-line note in the banner that "the
  boundary sphere proper has no separate type; use `Radius` equality tests or an SDF"
  would prevent readers from hunting for a hollow-sphere primitive.

- **missing-type** — `18-spatial-primitives.plato` / `25-solids.plato`: there is no
  thin-shell or surface-only counterpart to `Cylinder`/`Cone` (lateral surface without
  caps). Profile-generated `RevolvedSolid` can approximate them, but a named
  `CylindricalShell` would match `SphericalShell`'s role for pipes and ducts.

- **naming** — `25-solids.plato`: `CsgOperation.Difference` is set-difference (A minus B),
  which is correct, but easy to confuse with the algebraic `Difference` interface on
  points. A doc-comment cross-warning — or renaming to `Subtract` — would reduce
  collisions when both appear in one module.
