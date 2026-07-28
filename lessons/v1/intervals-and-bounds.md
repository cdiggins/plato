---
lesson: intervals-and-bounds
title: Intervals and Bounds
domain: Coordinate systems & bounds
v3-files: [12-intervals-bounds.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Intervals and Bounds

A game engine needs to know whether two characters overlap. A spreadsheet needs the
range of values in a column. A renderer needs the screen rectangle that covers a mesh.
All three problems are the same shape: describe a contiguous region along one or more
axes, then ask whether a value or another region lives inside it.

The cheap answer is a pair of numbers — min and max. The expensive bugs come from
forgetting whether the ends are inclusive, whether an empty range is allowed, and
whether “union” means the smallest covering box or the literal set union (which may not
be a box). Axis-aligned bounds are everywhere because they are fast, composable, and
good enough for broad-phase work — if you treat their invariants carefully.

## The idea

### One dimension: intervals

A **closed interval** $[a, b]$ is every real number $x$ with $a \le x \le b$. The
**length** (extent) is $b - a$. Two intervals **overlap** when they share at least one
point. Their **intersection** is the overlapping span (empty when they miss). Their
**union** as a set may be two pieces; the **bounding union** — the smallest single
interval covering both — is $[\min(a_1,a_2),\;\max(b_1,b_2)]$.

Directed intervals allow $a > b$. That encodes a reversed sweep (animation that runs
backward, an angular arc that goes the long way). Containment and overlap formulas that
assume $a \le b$ must reverse first.

```
  Start ----------> End     forward: Start <= End
  End   <---------- Start   reversed: Start > End

  Intersection empty when Start > End after taking
  max(starts) and min(ends).
```

### Higher dimensions: axis-aligned bounds

In 2D, an **axis-aligned bounding box** (AABB) is the product of an $x$-interval and a
$y$-interval: every point whose coordinates lie between a component-wise **Min** corner
and **Max** corner. In 3D the same idea yields a box with six faces, each perpendicular
to a world axis.

```
        Max ●──────────┐
            │          │
            │   AABB   │
            │          │
            └──────────● Min   (or Min lower-left, Max upper-right)
```

Why AABBs dominate broad-phase collision and culling:

- Overlap is three independent interval overlaps (or two in 2D) — a handful of compares.
- Growing a bound to include a point is component-wise min/max.
- Hierarchy: a parent bound is the union of child bounds.
- They are wrong for tight fit under rotation (an oriented box or sphere may be tighter),
  but they are never ambiguous and never need trigonometry.

An **empty** bound is a useful sentinel: no points enclosed yet. Implementations often
use inverted corners (`Min` > `Max` on some axis) or a separate flag. Growing from empty
must special-case the first point.

**Sizes** (`Width`, `Height`, `Depth`) are extents without a location. A rectangle can
be stored as center + size, or as min/max corners — same geometry, different algebra.

## In Plato

File `12-intervals-bounds.plato` separates **directed intervals**, **axis-aligned
bounds**, **sizes**, and a **center-size rectangle**.

```plato
concept IntervalLike<T>
{
    Start(x: Self): T;
    End(x: Self): T;
}

concept BoundsLike<TPoint>
{
    Min(x: Self): TPoint;
    Max(x: Self): TPoint;
}

type NumberInterval
    implements IntervalLike<Number>
{
    Start: Number;
    End: Number;
}

type Bounds2D
    implements BoundsLike<Point2D>
{
    Min: Point2D;
    Max: Point2D;
}

type Bounds3D
    implements BoundsLike<Point3D>
{
    Min: Point3D;
    Max: Point3D;
}

type Size2D
{
    Width: Number;
    Height: Number;
}

type Rect2D
{
    Center: Point2D;
    Size: Size2D;
}
```

Inclusive/exclusive policy is stated in doc comments:

| Type | Convention |
|------|------------|
| `NumberInterval`, `AngleInterval`, `LengthInterval` | closed at both ends (unless noted) |
| `IntegerInterval` | contains `Start`, excludes `End` (half-open index range) |
| `Bounds2D` / `Bounds3D` | inclusive Min and Max |
| `IntegerBounds2D` / `IntegerBounds3D` | contains Min, excludes Max (pixel/voxel boxes) |

Usage-shaped sketches (illustrative — library helpers live on the concept surface):

```plato
let span = NumberInterval { Start: 0, End: 10 };
// Extent = End - Start; Center = Lerp(0.5); Contains(value)

let a = Bounds2D {
    Min: Point2D { X: 0, Y: 0 },
    Max: Point2D { X: 2, Y: 3 }
};
let b = Bounds2D {
    Min: Point2D { X: 1, Y: 1 },
    Max: Point2D { X: 4, Y: 2 }
};
// Overlap on both axes ⇒ boxes collide in broad phase
// Bounding union grows Min component-wise lesser, Max greater

let pixel = IntegerBounds2D {
    Min: IntegerVector2 { X: 0, Y: 0 },
    Max: IntegerVector2 { X: 1920, Y: 1080 }
};
// Columns [0, 1920), rows [0, 1080) — classic half-open raster

let card = Rect2D {
    Center: Point2D { X: 0, Y: 0 },
    Size: Size2D { Width: 8.5, Height: 11 }
};
```

`AngleInterval` and `LengthInterval` carry the same `IntervalLike` shape with typed
endpoints — arc sweeps and physical ranges stay out of raw `Number` when units matter.
`IntegerSize2D` / `IntegerSize3D` are discrete extents for images and voxel grids.

The concept library already derives a rich `IntervalLike` toolkit: `Extent`, `IsForward`,
`Contains`, `Overlaps`, `Clamp`, `Union`, `Intersection`, `Grow`, `Prefix` / `Suffix`.
`BoundsLike` currently exposes little beyond `Center` and diagonal `Lerp` — containment
and union for boxes are the pedagogical workhorses that are still blocked at the concept
shape (see recommendations).

## Pitfalls / fine print

**Inclusive vs exclusive ends.** Mixing closed float bounds with half-open integer
ranges is a classic off-by-one factory. Pixel `(Max.X - Min.X)` is the width in pixels
for half-open integer bounds; for closed float bounds the same subtraction is still the
extent, but “how many samples” needs a different rule.

**Empty and inverted.** For directed intervals, `Start > End` after intersection means
disjoint — a feature. Accidentally constructing a `Bounds2D` with `Min.X > Max.X`
without intending emptiness is a silent bug: every containment test should treat that as
empty or normalize.

**Union is not set union.** The bounding union of two AABBs is always an AABB. The set
union of two AABBs may be an L-shape that is not convex and not a single box. Broad-phase
structures store the bounding union and refine later.

**Rotated geometry.** The AABB of a rotated rectangle is larger than the rectangle. Do
not treat AABB overlap as exact collision for oriented shapes — only as a filter.

**Center-size vs min-max.** `Rect2D` is convenient for layout; `Bounds2D` is convenient
for culling. Converting requires care at zero size and when you need inclusive pixel
coverage.

**Angles wrap.** `AngleInterval` is a directed span of `Angle` values; naive numeric
comparison does not understand that angles live on a circle. Antimeridian-style wraps
need an explicit convention (geospatial code hits the same issue in longitude).

## Try it

1. Intervals $[0, 5]$ and $[5, 10]$. Do they overlap if both are closed? If the second
   is half-open $[5, 10)$?
2. `Bounds2D` with Min $(0,0)$ and Max $(2,2)$, and another with Min $(2,2)$ and Max
   $(3,3)$. Inclusive overlap?
3. You grow an empty bound by including points one at a time. Why is
   `Min = lesser(Min, p); Max = greater(Max, p)` wrong for the first point if Min/Max
   start as $(0,0)$?

<details>
<summary>Answers</summary>

1. Closed: yes, they share $\{5\}$. Half-open second: no shared points if the first is
   $[0,5]$ closed and the second starts at 5 exclusive — wait: half-open $[5,10)$
   includes 5, so they still share 5. The classic miss is $[0,5)$ and $[5,10)$.
2. Yes — the shared corner $(2,2)$ is inside both inclusive bounds.
3. Because $(0,0)$ is already “inside” the bound even if no point was added. Empty must
   be a distinct state (or inverted corners) so the first point initializes both Min and
   Max to itself.

</details>

## Library recommendations

- **missing-function** — `12-intervals-bounds.plato` / `BoundsLike`: the concept library
  documents that `Contains`, `Union`, `Intersection`, `Expand`, and corner enumeration
  cannot be derived against `BoundsLike<TPoint>` as declared (no delta type, points are
  not a `Lattice`). Teaching AABBs without `Union`/`Contains` forces hand-waving —
  declare `BoundsLike<TPoint, TDelta>` or add Lattice on points, then mirror the
  `IntervalLike` surface.

- **missing-type** — `12-intervals-bounds.plato`: no explicit empty-bounds sentinel or
  `Optional`-style wrapper. Inverted Min/Max is an implicit encoding; a documented
  `Empty` factory (or sum type) would make the grow-from-empty story teachable and
  less error-prone.

- **naming** — `12-intervals-bounds.plato`: `Rect2D` is center+size while `Bounds2D` is
  min/max; both are rectangles. A doc-comment cross-link stating when to prefer each
  (layout vs culling) would reduce “which rectangle type?” confusion.

- **doc-comment** — `12-intervals-bounds.plato`: `NumberInterval` allows Start > End, but
  the file banner says bounds are inclusive Min/Max without stating whether inverted
  `Bounds2D` is a supported empty encoding. Pin the empty-bounds convention in the
  `Bounds2D`/`Bounds3D` comments.

> Resolved 2026-07-28: intervals-transforms.library.plato added concrete Bounds2D/3D Contains/Union/Overlaps/Expand/Empty/IsEmpty (items 29, 185, 186); Bounds2D/3D type comments now pin the inverted-Min>Max empty encoding (item 188). Corner enumeration deferred (no array literal).
