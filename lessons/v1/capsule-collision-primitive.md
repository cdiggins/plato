---
lesson: capsule-collision-primitive
title: The Capsule as a Collision Primitive
domain: Physics & simulation
v3-files: [17-planar-shapes.plato, 18-spatial-primitives.plato, 55-collision.plato]
audience: Basic 3D vectors/points and general programming background
status: draft-v1
---

# The Capsule as a Collision Primitive

Game characters, ragdoll limbs, and swept projectiles rarely want a perfect mesh
collider. They want something **convex**, **smooth**, and **cheap** that still looks
like a limb: a line segment thickened into a sausage. That shape is the **capsule** —
every point within a fixed radius of a segment.

Capsules are beloved in collision because the distance from a point to a segment is
simple, the Minkowski sum with a ball is obvious, and GJK/MPA support mappings stay
easy. Spheres are capsules with zero-length segments; cylinders with flat caps are
harder at the rims.

## The idea

In 3D, fix endpoints $A$, $B$ and radius $r \ge 0$. The solid capsule is

$$
\{\, p : \mathrm{dist}(p, \overline{AB}) \le r \,\}
$$

where $\overline{AB}$ is the closed segment. Equivalently: a **cylinder** of radius
$r$ about the segment, capped with **hemispheres** of radius $r$ at each end.

```
        ____
       /    \          hemispherical caps
      |      |
      |      |         cylindrical midsection
      |      |
       \____/
       A    B          axis segment
```

**Signed distance** from point $p$ to the capsule is
$\mathrm{dist}(p,\overline{AB}) - r$. Negative means inside.

**Closest point** on the capsule surface (or solid) reduces to: project $p$ onto the
segment, then move radially by at most $r$.

**Why not a cylinder?** Flat end caps have sharp circular rims. Contact normals jump.
A capsule's boundary is $C^1$ (tangent plane turns continuously), which stabilizes
physics solvers.

**2D cousin.** A **stadium**: all points within $r$ of a planar segment — the convex
hull of two disks of radius $r$ centered at the endpoints.

**Degenerate cases.** $A = B$ yields a sphere (disk in 2D). $r = 0$ yields the segment
itself (measure-zero solid — usually avoided for dynamic colliders).

## In Plato

Spatial solid from `18-spatial-primitives.plato`:

```plato
// All points within Radius of the segment from A to B:
// a cylinder with hemispherical ends.
type Capsule3D
    implements Geometry3D, ClosedShape, ConvexShape, Connected,
               SpatialMeasurable, Bounded3D, Centroid3D,
               ContainsPoint3D, NearestPoint3D, SupportMappable3D
{
    A: Point3D;
    B: Point3D;
    Radius: Number;
}
```

Planar stadium from `17-planar-shapes.plato`:

```plato
type Capsule2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, Centroid2D,
               ContainsPoint2D, NearestPoint2D, SupportMappable2D
{
    A: Point2D;
    B: Point2D;
    Radius: Number;
}
```

Collision wrappers place a shape in a body's local frame via a pose
(`55-collision.plato`):

```plato
type CapsuleCollider
{
    Shape: Capsule3D;
    LocalPose: Pose3D;
}

type CapsuleCollider2D
{
    Shape: Capsule2D;
    LocalPose: Pose2D;
}

type CompoundCollider3D
{
    Spheres: Array<SphereCollider>;
    Boxes: Array<BoxCollider>;
    Capsules: Array<CapsuleCollider>;
    Meshes: Array<MeshCollider>;
}
```

Usage-shaped setup (illustrative):

```plato
// Limb along local Y from -0.4 to 0.4, radius 0.1
let bone = Capsule3D(
    Point3D(0, -0.4, 0),
    Point3D(0,  0.4, 0),
    0.1);

let collider = CapsuleCollider(bone, localPose);

// Sphere as degenerate capsule
let ball = Capsule3D(center, center, radius);
```

Queries and contacts reuse the shared collision results:

```plato
type ContactPoint3D
{
    Position: Point3D;
    Normal: Direction3D;
    Penetration: Length;
    NormalImpulse: Impulse;
    FrictionImpulse: Vector3D;
}

type ShapeCastResult3D
{
    Hit: Boolean;
    Fraction: Proportion;
    Position: Point3D;
    Normal: Direction3D;
    Body: BodyIndex;
}
```

`SupportMappable3D` on `Capsule3D` is the green light for GJK: the support point in
direction $d$ is the farther endpoint's support of a sphere — pick $A$ or $B$ by dot
product with $d$, then add $r\,\hat{d}$.

## Pitfalls / fine print

**Axis in local vs world.** `Capsule3D.A`/`B` are shape-local points; `LocalPose` then
places the whole shape on the body. Baking a world-space segment into `A`/`B` *and*
setting a nontrivial pose double-applies the transform.

**Radius units.** Comments in collision land say positions are meters; `Capsule3D.Radius`
is a bare `Number` in the geometry file (unit-agnostic math). Keep one unit system in
the simulation.

**Cylinder vs capsule.** `Cylinder` in the same primitives file has flat caps and a
center/axis/height parameterization. Do not substitute it for character controllers
expecting rounded ends.

**AABB size.** Bounds must cover hemispheres, not just the segment — extend by $r$ in
every direction from the segment's AABB. Underestimating bounds drops broadphase hits.

**End-cap contacts.** When the closest segment feature is an endpoint, the contact
normal is radial from that point (sphere-like). When it is the interior, the normal is
perpendicular to $B-A$. Feature classification bugs cause flickering normals.

**Zero-length numerical noise.** Nearly coincident $A$ and $B$ should be treated as a
sphere; normalizing $B-A$ without a guard yields NaN support mappings.

## Try it

1. Capsule $A=(0,0,0)$, $B=(0,2,0)$, $r=0.5$. Is $p=(0,1,0.4)$ inside?
2. Same capsule: closest point on the *axis segment* to $q=(1,3,0)$? Distance from $q$
   to the solid?
3. Why is `SupportMappable3D` listed on `Capsule3D` important for collision libraries?

<details>
<summary>Answers</summary>

1. Axis distance from $p$ to segment is $0.4 < 0.5$ → inside.
2. Closest axis point is $B=(0,2,0)$ (projection past the end). Distance to axis point
   is $|(1,1,0)|=\sqrt{2}\approx1.414$; distance to solid is $\sqrt{2}-0.5\approx0.914$
   (outside).
3. GJK and related algorithms query support points; a capsule's support is closed-form
   from endpoints and radius, so the shape participates in convex narrowphase without
   a mesh.

</details>

## Library recommendations

- **missing-function** — `18-spatial-primitives.plato`: no
  `Distance(Capsule3D, Point3D)` / signed-distance helper despite
  `ContainsPoint3D` and `NearestPoint3D`. Capsule pedagogy and collision debug draws
  want the scalar $d - r$ explicitly.

- **missing-function** — `18-spatial-primitives.plato`: no `Capsule3D` factory from
  `Sphere` + segment length, or `FromCylinder` with hemispherical caps — conversion
  from artist cylinders is a frequent pipeline need.

- **wrong-shape** — `55-collision.plato`: `CapsuleCollider` stores full `Capsule3D`
  (already two points) plus `LocalPose`. Document whether `A`/`B` are in shape space
  before pose (assumed) and consider offering axis+height+radius in body space to
  match how engines author limbs.

- **doc-comment** — `17-planar-shapes.plato` / `18-spatial-primitives.plato`: state the
  $A=B$ sphere/disk degeneracy and $r=0$ segment degeneracy as supported invariants so
  collision code can branch without guessing.
