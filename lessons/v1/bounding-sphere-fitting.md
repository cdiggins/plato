---
lesson: bounding-sphere-fitting
title: Bounding Sphere Fitting
domain: Coordinate systems & bounds
v3-files: [12-intervals-bounds.plato, 18-spatial-primitives.plato]
audience: Comfortable with 3D points and the idea of a bounding volume
status: draft-v1
---

# Bounding Sphere Fitting

An axis-aligned box is easy to compute: take component-wise min and max. A **bounding
sphere** is often better for culling and collision — rotation-invariant, one center and
one radius, cheap distance tests — but finding a *tight* sphere for an arbitrary point
set is a different problem. The smallest enclosing sphere is a classic computational-
geometry result (Emo Welzl's randomized algorithm, among others). Most real-time systems
settle for a fast **approximate** fit that is still correct (the sphere contains the
points) even if it is not minimal.

This lesson is about that engineering trade: AABB vs sphere, naive fits from bounds, and
what Plato's vocabulary already gives you to say "this solid fits in this ball."

## The idea

### Why spheres

| Volume | Pros | Cons |
|--------|------|------|
| `Bounds3D` (AABB) | Trivial to build/union; good for grids | Rotates poorly — a diagonal rod's AABB is huge |
| `Box3D` (OBB) | Tighter under rotation | More expensive tests; orientation to maintain |
| `Sphere` | Rotation-invariant; simple overlap | Can be loose for long thin shapes |

A sphere with center $\mathbf{c}$ and radius $r$ contains a point set $S$ iff
$\|\mathbf{p}-\mathbf{c}\| \le r$ for every $\mathbf{p}\in S$. Equivalently, $r$ is at
least the distance from $\mathbf{c}$ to the farthest point of $S$.

### Fit from an AABB (fast, often loose)

Given an axis-aligned box with corners $\mathbf{m}$ (min) and $\mathbf{M}$ (max):

1. Center $\mathbf{c} = \tfrac12(\mathbf{m}+\mathbf{M})$ (box center).
2. Radius $r = \tfrac12\|\mathbf{M}-\mathbf{m}\|$ (half the space diagonal).

Every point of the box lies inside this sphere; the sphere is the **circumsphere of the
box**, not of an arbitrary mesh. For points that only fill a corner of the box, the
sphere is larger than necessary.

```
   M +--------+
     |        |     sphere centered at box center
     |   c    |     radius = half diagonal
     |        |
   m +--------+
```

### Fit from a point set (better center)

A common improvement (Ritter's algorithm sketch):

1. Find a pair of points that are extreme on some axis (or a diameter estimate).
2. Start with the sphere having that pair as diameter.
3. For each remaining point outside, grow the sphere to include it (update center and
   radius in one pass).

The result is correct but not always minimal. Exact smallest enclosing sphere needs
more machinery (boundary defined by 2–4 support points in 3D).

### Sphere from centroid + max radius

For a mesh with known centroid $\mathbf{g}$:

$$
r = \max_i \|\mathbf{v}_i - \mathbf{g}\|.
$$

Simple, correct, often decent for blob-like shapes; poor for shapes whose centroid is
far from the Chebyshev center of the set.

## In Plato

### Axis-aligned bounds

From `12-intervals-bounds.plato`:

```plato
// An axis-aligned bounding box in 3D.
type Bounds3D
    implements BoundsLike<Point3D>
{
    Min: Point3D;
    Max: Point3D;
}

concept BoundsLike<TPoint>
    inherits Value
{
    Min(x: Self): TPoint;
    Max(x: Self): TPoint;
}
```

`Size3D` holds width/height/depth extents; `Box3D` in file 18 is an *oriented* solid box
with `Center`, `Size`, and `Orientation` — prefer `Bounds3D` when alignment to world axes
is the point.

The concept library notes a gap: `Union`, `Expand`, `Contains`, and `Diagonal` helpers
cannot yet be written generically against `BoundsLike` as declared (missing delta type /
lattice on points). Callers still think in those operations; they are just not declared
on the concept surface.

### Sphere as a bounding volume

From `18-spatial-primitives.plato`:

```plato
type Sphere
    implements Geometry3D, ClosedShape, ConvexShape, Connected, SpatialMeasurable,
               Bounded3D, Centroid3D, ContainsPoint3D, NearestPoint3D, SupportMappable3D
{
    Center: Point3D;
    Radius: Number;
}
```

`Bounded3D` means a sphere can report its own axis-aligned bounds (the cube of side
$2r$ about the center). Going the other way — bounds to a fitted sphere — is the
operation this lesson needs and v3 does **not** declare.

`Contains(sphere, point)` is the membership test you validate a fit against.

### Usage-shaped: sphere from bounds

```plato
var b = Bounds3D {
    Min: Point3D { X: -1, Y: -2, Z: -3 },
    Max: Point3D { X:  1, Y:  2, Z:  3 }
};

// Center as the midpoint of the Min–Max diagonal.
var c = Lerp(b.Min, b.Max, 0.5);
var halfDiagonal = Multiply(Between(b.Min, b.Max), 0.5);
var r = Length(halfDiagonal);

var fit = Sphere { Center: c, Radius: r };
```

Every corner of `b` lies on or inside `fit`. The midpoints of faces lie strictly inside.

### Usage-shaped: verify containment of samples

```plato
var p = Point3D { X: 1, Y: 2, Z: 3 }; // a corner
// Contains(fit, p) should be true (within epsilon of the boundary)
```

### Oriented boxes and capsules

`Box3D` already carries a center — a quick sphere is the center plus half the space
diagonal of its local size (independent of `Orientation`, since spheres ignore
rotation). `Capsule3D` has a natural bounding sphere: midpoint of `A` and `B`, radius
$\tfrac12\|B-A\| + \texttt{Radius}$.

```plato
type Capsule3D
{
    A: Point3D;
    B: Point3D;
    Radius: Number;
}
```

### Ellipsoids

An `Ellipsoid` with semi-axes $(a,b,c)$ is bounded by a sphere of radius
$\max(a,b,c)$ about the same center — tight only when the ellipsoid is already a sphere.

## Pitfalls / fine print

**AABB sphere is not mesh-tight.** Computing `Bounds3D` from vertices then circumscribing
the box can be much looser than fitting the vertices directly — especially for diagonal
geometry.

**Empty / inverted bounds.** If `Min` components exceed `Max` (invalid box), the center
and radius formulas produce nonsense. Define an empty-bounds policy before fitting.

**Squared radius.** Store $r^2$ for comparisons when you only test containment; take
`Sqrt` only when you need the `Sphere.Radius` field.

**Growing spheres.** Naively setting $r = \max(r, \|\mathbf{p}-\mathbf{c}\|)$ without
moving $\mathbf{c}$ only works if $\mathbf{c}$ is already fixed. Online algorithms that
update both center and radius must follow a proven update rule or they skip points.

**Unit mismatch.** `Sphere.Radius` is `Number` (unit-agnostic geometry). Physics code that
wants meters should document the unit at the boundary between layers.

**No `Fit` in vocabulary.** Do not invent `FitSphere(points)` in snippets as if it were
declared — show the construction from fields, and record the gap as a recommendation.

## Try it

1. Bounds from $(-1,-1,-1)$ to $(1,1,1)$. What center and radius does the diagonal
   formula give? Does the sphere contain $(1,1,1)$?

2. Same eight corners of that cube, but you wrongly take radius $=$ half the *edge*
   length ($1$) instead of half the diagonal. Which points escape?

3. A thin rod from $(0,0,0)$ to $(10,0,0)$. Compare the AABB-circumsphere radius to a
   diameter sphere with endpoints as diameter.

<details>
<summary>Answers</summary>

1. Center $(0,0,0)$, half diagonal $\sqrt{3}$, radius $\sqrt{3}$. The corner is at
   distance $\sqrt{3}$ — on the boundary.

2. Radius $1$ fails: corner distance $\sqrt{3}>1$. Face-center points like $(1,0,0)$
   would still fit.

3. AABB is $[0,10]\times\{0\}\times\{0\}$ (degenerate in $Y,Z$ if only the segment);
   if you pad to a thin box, the diagonal sphere still has radius about $5$ plus pad.
   A diameter sphere on the endpoints has center $(5,0,0)$ and radius $5$ — typically
   tighter for the rod itself.

</details>

## Library recommendations

- **missing-function** — `12-intervals-bounds.plato` / `18-spatial-primitives.plato`: no
  `BoundingSphere(bounds: Bounds3D): Sphere` or `BoundingSphere(points: Array<Point3D>)`.
  The AABB-diagonal construction is universal and three lines; it belongs next to
  `Bounds3D`.

- **missing-function** — `12-intervals-bounds.plato`: `BoundsLike` still lacks
  `Diagonal` / `Extent` / `Union` / `Expand` (called out as a TODO in
  `concept-library/12-intervals-transforms.library.plato`). Fitting lessons keep
  re-deriving `Between(Min, Max)`.

- **wrong-shape** — `BoundsLike<TPoint>` carries no `TDelta` parameter, so the natural
  return type of `Extent` (`Vector3D` for `Bounds3D`) cannot be expressed. Reintroduce
  `BoundsLike<TPoint, TDelta>` as the library TODO suggests.

- **missing-function** — `18-spatial-primitives.plato`: solids that implement `Bounded3D`
  have no reverse `BoundingSphere` sugar (`Box3D`, `Capsule3D`, `Triangle3D`). Each has a
  known closed form worth declaring beside `Volume` / `Centroid`.
