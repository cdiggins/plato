---
lesson: collision-basics
title: Collision Basics
domain: Physics & simulation
v3-files: [55-collision.plato]
audience: Basic 3D geometry (spheres, boxes, normals) and general programming background
status: draft-v1
---

# Collision Basics

Two characters swing swords. Before the sparkly VFX, the engine must answer three
questions, cheaply and then precisely: *might* these shapes meet, *do* they meet, and if
so *where* and *how deep*? Games and CAD tools spend surprising fractions of their budgets
on that pipeline. The vocabulary for the answers — filters, colliders, contact points,
manifolds, events, and shape queries — is what collision systems are made of.

## The idea

### Broad phase vs narrow phase

Brute force checks every pair of $N$ colliders: $O(N^2)$ tests. At hundreds of bodies that
is already painful; at thousands it is dead. The usual split:

1. **Broad phase** — discard pairs that cannot possibly collide (far-apart bounds, different
   collision layers). Cheap, approximate, over-inclusive.
2. **Narrow phase** — for surviving pairs, compute exact contact geometry: points, normals,
   penetration depths.

```
  all pairs ──► broad phase ──► candidate pairs ──► narrow phase ──► contacts
                 (AABB/grid)                         (GJK/SAT/...)
```

Filtering bits and layer matrices are broad-phase policy: they decide *who is allowed* to
generate candidates, not how deep the overlap is.

### Separating axis (intuition)

For two **convex** shapes, there is no overlap if there exists a direction (an axis) onto
which their projections do not overlap — a **separating axis**. If every candidate axis
shows overlap, the shapes intersect. Box–box and polyhedron tests build on this idea;
sphere–sphere reduces to a distance check (the axis is the line of centers).

Concave meshes are harder: either decompose into convex pieces, or special-case
static-mesh vs primitive tests. Dynamic vs dynamic mesh collision usually requires the
mesh to be treated as convex (or convex-decomposed).

### Contact points and manifolds

A single contact is not enough for stable stacking. A box resting on a floor needs a
small set of contacts that span the support — a **contact manifold**. Each contact carries:

- **Position** — where in the world the touch is modeled
- **Normal** — direction to push the bodies apart (convention: from A toward B)
- **Penetration** — how deep they overlap (should be driven toward zero)
- **Impulses** — what the solver applied last step (warm-starting)

Friction lives in the tangent plane; restitution (bounciness) scales the separating
velocity along the normal after the collision response.

### Why capsules are beloved

A **capsule** is a line segment thickened by a radius — a sphere swept along a segment.
Character controllers, limbs, and many props approximate well as capsules:

- Smooth contact normals (no box corners catching on stairs)
- Cheap distance tests (point–segment distance, then compare to radius)
- Stable against small orientation noise

Spheres are even cheaper but roll and climb oddly. Boxes are axis-aligned or oriented and
stack well but snag. Triangle meshes are exact for static level geometry and expensive for
moving concave props.

### Queries beyond "are we touching?"

Simulation also needs **shape casts** (sweep a shape along a motion, report first hit
fraction) and **overlaps** (list everyone intersecting a volume). Triggers report overlap
events without applying contact forces — doorways, checkpoints, damage volumes.

## In Plato

File `55-collision.plato` encodes filters, colliders, contacts, events, and query results.

### Filtering

```plato
type CollisionFilter
    implements Value
{
    CategoryBits: Integer;
    MaskBits: Integer;
    GroupIndex: Integer;
}
```

Two colliders interact when each one's category intersects the other's mask, unless
`GroupIndex` overrides: shared positive group always collides, shared negative never,
`0` defers to the bits.

```plato
type CollisionLayerMatrix
    implements Value
{
    LayerCount: Integer;
    Enabled: Array<Boolean>;  // row-major LayerCount²
}
```

### Colliders

3D primitives attach a shape plus a pose in the owning body's local frame:

```plato
type SphereCollider
    implements Value
{
    Shape: Sphere;
    LocalPose: Pose3D;
}

type BoxCollider
    implements Value
{
    Shape: Box3D;
    LocalPose: Pose3D;
}

type CapsuleCollider
    implements Value
{
    Shape: Capsule3D;
    LocalPose: Pose3D;
}

type MeshCollider
    implements Value
{
    Mesh: TriangleMesh3D;
    Convex: Boolean;
    LocalPose: Pose3D;
}

type CompoundCollider3D
    implements Value
{
    Spheres: Array<SphereCollider>;
    Boxes: Array<BoxCollider>;
    Capsules: Array<CapsuleCollider>;
    Meshes: Array<MeshCollider>;
}
```

`Convex: true` on `MeshCollider` is the license for dynamic–dynamic mesh pairs. Trigger
volumes reuse compounds but skip force generation:

```plato
type TriggerVolume3D
    implements Value
{
    Colliders: CompoundCollider3D;
    Pose: Pose3D;
    Filter: CollisionFilter;
}
```

2D mirrors the pattern with `CircleCollider2D`, `BoxCollider2D`, `CapsuleCollider2D`, and
`CompoundCollider2D`.

### Contacts

```plato
type CollisionPair
    implements Value
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
}

type ContactPoint3D
    implements Value
{
    Position: Point3D;
    Normal: Direction3D;
    Penetration: Length;
    NormalImpulse: Impulse;
    FrictionImpulse: Vector3D;
}

type ContactManifold3D
    implements Value
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
    Points: Array<ContactPoint3D>;
    Normal: Direction3D;
    Friction: Number;
    Restitution: Proportion;
}
```

The manifold shares one normal and combined friction/restitution for the pair; individual
points carry positions, penetrations, and solved impulses.

### Events and queries

```plato
type CollisionPhase = Begin | Stay | End;

type CollisionEvent
    implements Value
{
    Phase: CollisionPhase;
    Pair: CollisionPair;
    Time: Instant;
}

type ShapeCastResult3D
    implements Value
{
    Hit: Boolean;
    Fraction: Proportion;   // [0, 1] along the sweep
    Position: Point3D;
    Normal: Direction3D;
    Body: BodyIndex;
}

type OverlapResult
    implements Value
{
    BodyIndices: Array<BodyIndex>;
}
```

Usage-shaped sketches:

```plato
let head = SphereCollider {
    Shape: sphere,
    LocalPose: Pose3D { ... }   // offset above body origin
};

let torso = CapsuleCollider {
    Shape: capsule,
    LocalPose: Pose3D {
        Position: Point3D { X: 0, Y: 0, Z: 0 },
        Orientation: Quaternion { X: 0, Y: 0, Z: 0, W: 1 }
    }
};

let compound = CompoundCollider3D {
    Spheres: [head],
    Boxes: [],
    Capsules: [torso],
    Meshes: []
};

// After narrow phase for bodies i and j:
let manifold = ContactManifold3D {
    BodyA: BodyIndex { Value: i },
    BodyB: BodyIndex { Value: j },
    Points: contacts,
    Normal: Direction3D { ... },
    Friction: 0.6,
    Restitution: Proportion { ... }
};
```

## Pitfalls / fine print

**Normal direction.** Plato documents normals as pointing from A toward B. Flipping the
label of A/B without flipping the normal double-applies correction the wrong way.

**Penetration sign.** Penetration is overlap depth (non-negative length in the doc
comments). Using a signed distance without agreeing on "positive means penetrating"
breaks solvers.

**Mesh concave dynamics.** `MeshCollider` with `Convex: false` is for static (or carefully
restricted) use. Enabling dynamic–dynamic on raw concave meshes is a stability and
performance trap.

**Filter vs layer matrix.** Bitmasks on colliders and a global `CollisionLayerMatrix` can
both exist; inconsistent authorship ("I set bits but the layer table says no") looks like
a narrow-phase bug.

**Manifold persistence.** Solvers warm-start from previous impulses. Rebuilding manifolds
from scratch every frame without matching contact IDs causes jittery stacks.

**Triggers vs solid colliders.** `TriggerVolume3D` reports overlap; it must not silently
also generate `ContactManifold3D` forces, or "sensor" volumes become invisible walls.

**Shape-cast fraction.** `Fraction` is normalized to the sweep segment $[0,1]$, not an
absolute time. Interpreting it as seconds causes tunneling diagnostics to lie.

## Try it

1. Two spheres, centers $2\,\mathrm{m}$ apart, radii $1.2\,\mathrm{m}$ and $1.0\,\mathrm{m}$.
   Do they overlap? Roughly how deep?
2. Why might a character capsule be preferred over a box for stair climbing?
3. A `CollisionPhase` sequence for one pair is `Begin`, then several `Stay`, then `End`.
   What physical story does that tell?

<details>
<summary>Answers</summary>

1. Sum of radii is $2.2\,\mathrm{m} > 2\,\mathrm{m}$, so yes; penetration about $0.2\,\mathrm{m}$
   along the line of centers.
2. Rounded contacts slide over stair edges instead of catching a sharp corner and launching
   or sticking the character.
3. First contact, continued touching across frames, then separation — the lifespan of one
   collision relationship.

</details>

## Library recommendations

- **missing-type** — `55-collision.plato`: there is no `Collider3D` sum type unifying
  `SphereCollider | BoxCollider | CapsuleCollider | MeshCollider`. Compounds store
  parallel arrays per kind; teaching "attach one collider" has no single-variant noun.

- **missing-function** — `55-collision.plato`: contact and query types are pure data, with
  no declared narrow-phase operations (`Intersect`, `ClosestPoints`, `BuildManifold`).
  The lesson can describe SAT and capsules but cannot point at a vocabulary home for them.

- **doc-comment** — `55-collision.plato`: `ContactPoint3D.FrictionImpulse` is a `Vector3D`
  while `ContactPoint2D` splits `NormalImpulse` / `TangentImpulse` as scalar `Impulse`
  quantities. A note that 3D friction may be a single tangent-plane vector (or two basis
  impulses) would clarify the dimensional asymmetry.

- **missing-type** — `55-collision.plato`: broad-phase structures (AABB pairs, sweep-and-prune
  proxy, spatial hash cell) are absent. Filters assume a broad phase exists; pedagogy has
  to leave that stage unnamed in v3.
