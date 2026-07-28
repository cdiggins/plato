---
lesson: contact-manifold-basics
title: Contact Manifold Basics
domain: Physics & simulation
v3-files: [55-collision.plato]
audience: Familiar with rigid bodies and the idea of collision detection; no solver experience required
status: draft-v1
---

# Contact Manifold Basics

When two rigid bodies touch, a physics engine does not merely say "yes, overlapping."
It needs enough geometry to *push them apart and apply friction*: where they touch,
which way is "out," how deep the overlap is, and how sticky the materials are. That
bundle of data for one body pair in one time step is a **contact manifold**.

Think of a crate resting on a floor. A single contact point at the center of the bottom
face is enough for resting, but a slight tip needs contacts near the edges so the crate
does not sink or see-saw through the plane. Box–box collision routinely returns up to
four points sharing one normal — a small manifold, not one lonely point.

## The idea

### From overlap to contact

Narrow-phase collision answers: do shapes $A$ and $B$ overlap? If so, produce:

1. A **contact normal** $\mathbf{n}$ — unit direction pointing from $A$ toward $B$
   (conventions vary; pick one and stick to it).
2. One or more **contact points** $\mathbf{p}_i$ in world space.
3. A **penetration depth** $d_i \ge 0$ — how far to move along the normal to separate.

The solver then applies impulses along $\mathbf{n}$ (to resolve penetration and normal
velocity) and in the tangent plane (friction).

### Why a *manifold* of points

A single point cannot resist torque about the normal the way a face can. Multiple
points around a contact patch approximate the distributed pressure of a real face–face
touch:

```
      B
  +-------+
  |       |
  +--*--*-+   * = contact points, shared normal upward
  |       |
  +-------+
      A (floor)
```

Typical caps: sphere–sphere → 1 point; capsule–plane → 1–2; box–box → up to 4;
convex–convex → small fixed budget (e.g. 4) even if the true patch is a polygon.

### Shared normal

In many engines (and in Plato's `ContactManifold3D`), **one normal is stored on the
manifold**, and each point reuses it. That models a locally flat contact. Curved
patches (sphere vs sphere) still use one normal — the line of centers — with a single
point.

### Warm starting

Solvers iterate. Caching last frame's `NormalImpulse` / friction impulses on matching
points (**warm starting**) drastically improves resting stability. That is why contact
points carry impulse fields that are zero before solving and nonzero after.

### Lifetime events

Besides geometric manifolds, games want notifications: first touch, still touching,
separated. Those are **collision phases**, not part of the manifold itself, but they
travel with the same body pair identity.

## In Plato

### Body pairs

From `55-collision.plato`:

```plato
// An unordered pair of colliding bodies, stored as zero-based indices into
// the simulation's body array (-1 = none).
type CollisionPair
    implements Value
{
    BodyA: BodyIndex;
    BodyB: BodyIndex;
}

type BodyIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}
```

`BodyIndex` is declared in `54-rigid-dynamics.plato` and referenced here. $-1$ means
none / static environment per that file's comment.

### A single spatial contact

```plato
// A single spatial contact. Position is the world contact point in meters;
// Normal points from body A toward body B; Penetration is the overlap depth;
// NormalImpulse is the solver impulse along the normal in newton-seconds;
// FrictionImpulse is the tangential impulse in newton-seconds (0 before
// solving).
type ContactPoint3D
    implements Value
{
    Position: Point3D;
    Normal: Direction3D;
    Penetration: Length;
    NormalImpulse: Impulse;
    FrictionImpulse: Vector3D;
}
```

Read the fields carefully:

| Field | Role |
|-------|------|
| `Position` | World-space point (meters) |
| `Normal` | From A toward B (unit) |
| `Penetration` | Overlap depth as `Length` |
| `NormalImpulse` | Accumulated normal impulse (`Impulse`) |
| `FrictionImpulse` | Tangential impulse as `Vector3D` (3D has a plane of friction directions) |

Compare 2D: `ContactPoint2D` stores `TangentImpulse` as a scalar `Impulse` — one
tangent axis in the plane.

### The manifold

```plato
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

- `Points` — the contact set for this pair this step (may be empty if you keep manifolds
  for bookkeeping, but typically non-empty when generated from overlap).
- `Normal` — shared contact normal, A→B, matching the point normals in well-formed data.
- `Friction` — combined dimensionless coefficient.
- `Restitution` — combined bounciness in $[0,1]$ as `Proportion`.

2D twin: `ContactManifold2D` with `ContactPoint2D` and `Direction2D`.

### Materials feed friction and restitution

Combining surface properties is defined on bodies' materials in file 54
(`PhysicsMaterial`, `MaterialCombine`). The manifold stores the **already combined**
scalars — collision narrow phase merges materials before the solver runs.

### Events alongside manifolds

```plato
type CollisionPhase = Begin | Stay | End;

type CollisionEvent
    implements Value
{
    Phase: CollisionPhase;
    Pair: CollisionPair;
    Time: Instant;
}
```

`Begin` / `Stay` / `End` track pair lifetime. A `Stay` event often accompanies an
updated manifold; `End` means no manifold (or an empty one) this step.

### Colliders that produce contacts

```plato
type SphereCollider  { Shape: Sphere; LocalPose: Pose3D; }
type BoxCollider     { Shape: Box3D;  LocalPose: Pose3D; }
type CapsuleCollider { Shape: Capsule3D; LocalPose: Pose3D; }
type MeshCollider    { Mesh: TriangleMesh3D; Convex: Boolean; LocalPose: Pose3D; }

type CompoundCollider3D
{
    Spheres: Array<SphereCollider>;
    Boxes: Array<BoxCollider>;
    Capsules: Array<CapsuleCollider>;
    Meshes: Array<MeshCollider>;
}
```

Each collider is in the owning body's local frame via `LocalPose`. Narrow phase
transforms shapes to world space, then writes world `ContactPoint3D.Position` values.

Triggers skip force-generating manifolds:

```plato
type TriggerVolume3D
{
    Colliders: CompoundCollider3D;
    Pose: Pose3D;
    Filter: CollisionFilter;
}
```

They report overlap events instead of contact forces — `OverlapResult` lists body
indices, not manifolds.

### Usage-shaped: packing a sphere–plane contact

```plato
var manifold = ContactManifold3D {
    BodyA: BodyIndex { Value: 0 },   // ball
    BodyB: BodyIndex { Value: -1 },  // static ground
    Points: Array {
        ContactPoint3D {
            Position: Point3D { X: 0, Y: 0, Z: 0 },
            Normal: Direction3D { Vector: Vector3D { X: 0, Y: 1, Z: 0 } },
            Penetration: Length { Meters: 0.02 },
            NormalImpulse: Impulse { NewtonSeconds: 0.0 },
            FrictionImpulse: Vector3D { X: 0, Y: 0, Z: 0 }
        }
    },
    Normal: Direction3D { Vector: Vector3D { X: 0, Y: 1, Z: 0 } },
    Friction: 0.6,
    Restitution: Proportion { Value: 0.0 }
};
```

## Pitfalls / fine print

**Normal direction.** Plato says A→B. Flipping A and B without flipping the normal
double-swaps forces and pulls bodies together. Keep pair order and normal consistent.

**Point normals vs manifold normal.** Each `ContactPoint3D` *also* stores `Normal`. If
they disagree with `ContactManifold3D.Normal`, solvers disagree with themselves. Prefer
copying the shared normal into every point at generation time.

**Penetration units.** `Penetration` is `Length` (meters). Mixing it with unitless
`Number` shape radii without conversion is a layer bug.

**FrictionImpulse as Vector3D.** The vector should lie in the tangent plane (orthogonal
to the normal). A solver that writes a normal component into friction corrupts energy.

**Empty manifolds.** An empty `Points` array with a pair id may mean "cached pair, no
contact this step" or a bug — define the convention in the simulation, not ad hoc.

**Mesh colliders.** Non-convex `MeshCollider` (`Convex: false`) usually collides only
against dynamics as static triangle soup; dynamic–dynamic often requires `Convex: true`
or compounds of convex pieces — see the type's doc comment.

**Filtering.** `CollisionFilter` and `CollisionLayerMatrix` can prevent a pair from
ever generating a manifold. Absence of a manifold is not always "shapes miss."

## Try it

1. Sphere at $(0,1,0)$ radius $1$ resting on the plane $y=0$, slight overlap so the
   lowest point is at $y=-0.01$. What is a reasonable `Position`, `Normal`, and
   `Penetration` for a one-point manifold with the sphere as body A?

2. Why might a box on a plane use four contact points instead of one at the center of
   the bottom face?

3. After the solver runs, which fields on `ContactPoint3D` are expected to change, and
   which should stay fixed for the step?

<details>
<summary>Answers</summary>

1. With A = ball and B = floor, Plato's A→B rule gives normal $(0,-1,0)$ (from ball
   toward floor). Position near $(0,0,0)$; penetration $0.01$ m. (Engines that store
   "outward from B" would flip both pair order and normal together.)

2. Four corners resist tipping torque; one center point lets the box pivot unrealistically
   about that point under offset forces.

3. `NormalImpulse` and `FrictionImpulse` update; `Position`, `Normal`, and `Penetration`
   are geometric inputs for that step (regenerated next step, possibly with warm-start
   matching).

</details>

## Library recommendations

- **doc-comment** — `55-collision.plato`: `ContactPoint3D.Normal` duplicates
  `ContactManifold3D.Normal`. State whether points must match the manifold normal, or
  whether per-point normals are allowed to differ for curved contacts (and then why the
  manifold still stores one).

- **wrong-shape** — `55-collision.plato`: `FrictionImpulse: Vector3D` is unit-bearing in
  spirit (newton-seconds) but not an `Impulse` quantity, while `NormalImpulse` is
  `Impulse`. Prefer `FrictionImpulse: Vector3D` documented as newton-seconds **or** a
  dedicated tangent-impulse type so units are consistent.

- **missing-function** — `55-collision.plato`: no `Flip(manifold)` that swaps `BodyA` /
  `BodyB` and negates normals — the operation every broad-phase does when it canonicalizes
  pair order.

- **missing-type** — `55-collision.plato`: no explicit feature-id / contact-hash on
  `ContactPoint3D` for warm-start matching across frames. Engines invent parallel arrays;
  a `ContactId` field would make the manifold self-contained for persistence.
