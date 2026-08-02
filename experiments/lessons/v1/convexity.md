---
lesson: convexity
title: Convexity
domain: Geometry primitives
v3-files: [17-planar-shapes.plato, 19-polygons.plato, 55-collision.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Convexity

Take any two points inside a shape and draw the segment between them. If that segment
always stays inside, the shape is **convex**. Disks, boxes, triangles, and capsules pass.
Crescents, stars, and most floor-plan polygons fail.

Convexity is the single property that turns many geometry problems from “research project”
into “textbook algorithm.” Containment, separation, and collision detection all get
faster — sometimes from exponential nightmares to reliable iterative methods — once every
operand is convex.

## The idea

### Definition

A set $S$ is convex iff for all $P, Q \in S$ and all $t \in [0,1]$,

$$
(1-t)P + tQ \in S.
$$

Equivalently: $S$ is an intersection of half-spaces; equivalently: every internal angle of
a convex polygon is $\le 180^\circ$, and every line crosses the boundary at most twice.

```
  Convex:              Not convex:
  ●─────●              ●──●
  │     │               \  ╲___
  ●─────●               ●──────●
  (segment inside)     (segment exits)
```

### Why algorithms care

1. **Support mapping.** The farthest point of $S$ in direction $\mathbf{d}$ is unique
   enough to drive GJK / EPA collision. Defined cleanly only for convex compact sets.
2. **Separating axis.** Two convex shapes are disjoint iff a separating line/plane exists.
   SAT for polyhedra relies on this.
3. **Local ⇒ global.** A local minimum of distance between convex shapes is global — no
   false “closest features” traps that plague concave meshes.
4. **Hulls.** The convex hull of any set is the smallest convex superset; broad-phase
   proxies often store hulls of complex art.

### Convex vs concave polygons

A concave polygon has at least one reflex vertex (internal angle $> 180^\circ$). Point-in-
polygon still works (winding / ray cast), but support mapping does not mean what GJK
wants, and the polygon is not the intersection of its edge half-planes alone in the
“solid convex” sense.

### Collision practice

Engines prefer spheres, capsules, boxes, and convex hulls for **dynamic** colliders.
Concave triangle meshes are usually **static** (or decomposed into convex pieces). A flag
saying “treat this mesh as convex” is a performance contract — wrong flags tunnel or
miss.

## In Plato

`ConvexShape` is a marker interface in `15-interfaces-geometry.plato`:

```plato
// Marks a shape that contains the entire straight segment between any two of
// its points.
interface ConvexShape
{ }
```

Many analytic primitives in `17-planar-shapes.plato` opt in: `Triangle2D`,
`Parallelogram2D`, `Circle`, `CircularSegment`, `Capsule2D`, `Ellipse`,
`RoundedRect2D`, `OrientedBox2D`, `RegularPolygon`. Deliberate absences: `Annulus`
(not convex), `Quad2D` (not necessarily convex), `SuperEllipse` for exponents $< 1$.

Polygons split by promise:

```plato
type Polygon2D
    implements /* ... notably NOT ConvexShape ... */
{
    Points: Array<Point2D>;
}

type ConvexPolygon2D
    implements /* ... ConvexShape ..., SupportMappable2D ... */
{
    Points: Array<Point2D>;
}
```

`ConvexPolygon2D` comments: vertices in strictly convex position, counter-clockwise —
unlocking constant-cost support and logarithmic containment.

Support maps:

```plato
interface SupportMappable2D
{
    Support(x: Self, direction: Direction2D): Point2D;
}

interface SupportMappable3D
{
    Support(x: Self, direction: Direction3D): Point3D;
}
```

Only convex shapes implement these in the vocabulary (circles, capsules, oriented boxes,
convex polygons, spheres, …).

`55-collision.plato` shows the engineering consequence. Colliders wrap convex-friendly
shapes; meshes carry an explicit convex claim:

```plato
type SphereCollider
{
    Shape: Sphere;
    LocalPose: Pose3D;
}

type BoxCollider
{
    Shape: Box3D;
    LocalPose: Pose3D;
}

type CapsuleCollider
{
    Shape: Capsule3D;
    LocalPose: Pose3D;
}

type MeshCollider
{
    Mesh: TriangleMesh3D;
    Convex: Boolean;
    LocalPose: Pose3D;
}

type CircleCollider2D
{
    Shape: Circle;
    LocalPose: Pose2D;
}

type BoxCollider2D
{
    Shape: OrientedBox2D;
    LocalPose: Pose2D;
}

type CapsuleCollider2D
{
    Shape: Capsule2D;
    LocalPose: Pose2D;
}
```

Doc comment on `MeshCollider`: when `Convex` is true the mesh is treated as (or
decomposed to) a convex hull, allowing dynamic-versus-dynamic collision. Concave meshes
do not get that privilege casually.

`CompoundCollider3D` / `CompoundCollider2D` group multiple convex pieces — the standard
way to approximate a concave object as a union of convex atoms (the union need not be
convex, but each piece is).

Usage-shaped sketches:

```plato
let hull = ConvexPolygon2D {
    Points: /* CCW convex chain */
};
// Support(hull, dir) — farthest vertex in dir
// Contains uses binary search over half-planes (log n)

let blob = Polygon2D {
    Points: /* maybe concave */
};
// Contains still exists; SupportMappable2D does not apply

let dyn = MeshCollider {
    Mesh: heroMesh,
    Convex: true,   // contract: mesh must actually be a convex hull
    LocalPose: /* ... */
};
```

Half-spaces, slabs, spheres, and boxes are convex by construction — the marker appears
on those types in the geometry files as well.

## Pitfalls / fine print

**Union of convex is not convex.** Two overlapping boxes can form an L. Compounds collide
piecewise; do not assume the compound’s AABB tightness equals a single hull.

**Convex flag lies.** Marking a bowl-shaped mesh `Convex: true` makes the simulator
believe the interior is solid fill of the hull — characters sit on an invisible lid.

**Numerical convexity.** A polygon that is “almost” convex with a tiny reflex dent will
fail strict convex algorithms. Snap, repair, or hull it.

**Support ties.** Flat faces have a whole edge/face of maximizers; any extreme point is
allowed. Algorithms must tolerate that.

**Quad2D.** Four vertices are not assumed convex in Plato — correctly. A square stored as
`Quad2D` still happens to be convex, but the type does not advertise `ConvexShape`.

**Stars and holes.** `RegularStar2D` and `PolygonWithHoles2D` are not convex shapes;
holey regions fail the segment test immediately.

## Try it

1. Is a circular annulus (ring) convex? Why does Plato omit `ConvexShape` on `Annulus`?
2. Triangle with vertices $A,B,C$. Pick two interior points — must the segment stay
   inside? What if the “triangle” is degenerate (collinear)?
3. Why does GJK need `Support`, and why is `Support` restricted to convex shapes?

<details>
<summary>Answers</summary>

1. No — the segment between two opposite points on the outer rim cuts through the hole
   (outside the ring). Not convex; marker omitted on purpose.
2. For a non-degenerate filled triangle, yes — triangles are convex. Degenerate collinear
   vertices are a zero-area convex set (a segment) still convex as a point set, but a
   terrible triangle for normals/area.
3. GJK iteratively builds a simplex in the Minkowski difference using support points.
   For non-convex shapes the support point does not characterize the whole set; the
   algorithm can miss collisions or report nonsense. Convexity makes the support oracle
   sufficient.

</details>

## Library recommendations

- **missing-function** — `19-polygons.plato`: no `IsConvex(Polygon2D) → Boolean` and no
  `ConvexHull(PointSet) → ConvexPolygon2D`. Teaching “promote to convex” needs a path
  from `Polygon2D` to `ConvexPolygon2D` beyond manual authoring.

- **missing-function** — `15-interfaces-geometry.plato` / geometry library: `ConvexShape` is
  a pure marker with no members. A documented `SegmentInside` predicate is redundant with
  the definition, but an `IsConvex` free function on polygons is still required (markers
  cannot be queried at runtime without reflection).

- **doc-comment** — `55-collision.plato`: `MeshCollider.Convex` should warn that a true
  flag means the **convex hull** is the solid, not the concave surface — the most common
  authoring misunderstanding in physics engines.

- **missing-type** — `55-collision.plato`: 2D compounds omit a mesh/polygon collider
  analogous to `MeshCollider` (only circle/box/capsule arrays). Concave 2D levels often
  need polygon colliders; the gap pushes users to fake geometry with capsule soup.

- **pedagogy** — `17-planar-shapes.plato`: `Quad2D` is not `ConvexShape` while
  `Parallelogram2D` is — correct, but a one-line comment on `Quad2D` saying “use
  `ConvexPolygon2D` or `OrientedBox2D` when convexity is required” would steer authors.
