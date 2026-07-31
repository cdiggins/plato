---
lesson: aabb-ray-intersection
title: Ray vs Axis-Aligned Bounding Box
domain: Geometry primitives
v3-files: [12-intervals-bounds.plato, 16-lines.plato, 35-spatial-queries.plato]
audience: Basic 3D vectors; comfort with parametric lines
status: draft-v1
---

# Ray vs Axis-Aligned Bounding Box

Before you test a ray against a million triangles, you ask a cheaper
question: does the ray even hit the object's bounding box? Axis-aligned
bounding boxes (AABBs) turn that question into three interval problems — one
per axis — and a handful of comparisons. It is the workhorse of ray tracing
broad-phase, picking, and collision sweeps.

## The idea

A ray is a half-line:

$$
R(t) = O + t\, D, \qquad t \ge 0
$$

with origin $O$ and direction $D$ (usually unit length, but the algebra
works with any non-zero $D$ if you are careful with $t$ scaling).

An AABB is the set of points whose coordinates lie between a minimum and
maximum corner:

$$
B = \{ p \mid p_x \in [x_{\min}, x_{\max}],\;
             p_y \in [y_{\min}, y_{\max}],\;
             p_z \in [z_{\min}, z_{\max}] \}.
$$

Equivalently, an AABB is the intersection of three **slabs** — infinite
regions between pairs of parallel planes, one pair per axis.

```
        y_max ................
             .              .
             .    AABB      .
             .              .
        y_min ................
             x_min        x_max
```

For the $X$ slab, solve $O_x + t D_x = x_{\min}$ and $= x_{\max}$:

$$
t_{x1} = (x_{\min} - O_x) / D_x, \qquad
t_{x2} = (x_{\max} - O_x) / D_x.
$$

Let $t_{x0} = \min(t_{x1}, t_{x2})$ and $t_{x1}' = \max(t_{x1}, t_{x2})$ be
the entry and exit parameters for that slab. Repeat for $Y$ and $Z$. The ray
hits the box if the three parameter intervals overlap in a range that also
intersects $t \ge 0$:

$$
t_{\mathrm{enter}} = \max(t_{x0}, t_{y0}, t_{z0}), \qquad
t_{\mathrm{exit}}  = \min(t_{x1}', t_{y1}', t_{z1}').
$$

Hit when $t_{\mathrm{enter}} \le t_{\mathrm{exit}}$ and $t_{\mathrm{exit}}
\ge 0$. The actual hit distance along a unit direction is
$\max(t_{\mathrm{enter}}, 0)$ when the origin starts outside (or inside, if
you want the exit instead).

This is the **slab method** (Kay–Kajiya). Degenerate cases: $D_x = 0$ means
the ray is parallel to the $X$ planes — either miss forever or stay inside
the $X$ range for all $t$.

## In Plato

Bounds and intervals:

```plato
// An axis-aligned bounding box in 3D.
type Bounds3D
    implements BoundsLike<Point3D>
{
    Min: Point3D;
    Max: Point3D;
}

type NumberInterval
    implements IntervalLike<Number>
{
    Start: Number;
    End: Number;
}
```

Rays and the slab primitive that makes the algorithm literal:

```plato
type Ray3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

// The closed 3D region between two parallel planes: the points p whose signed
// distance Dot(Normal, p) lies within Interval. The workhorse of separating-axis
// and ray-box tests.
type Slab3D
    implements Geometry3D, ConvexShape, Connected, ContainsPoint3D
{
    Normal: Direction3D;
    Interval: NumberInterval;
}
```

An AABB is exactly three axis-aligned `Slab3D` values whose normals are the
unit axes and whose intervals are $[Min.X, Max.X]$, etc.

Query and result records live in `35-spatial-queries.plato`:

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

Illustrative slab overlap for one axis (pseudo-structured Plato):

```plato
let o = query.Ray.Origin;
let d = query.Ray.Direction.Vector; // unit direction
let b = box; // Bounds3D

// X slab entry/exit (handle d.X == 0 separately in real code)
let tx1 = (b.Min.X - o.X) / d.X;
let tx2 = (b.Max.X - o.X) / d.X;
let txEnter = Min(tx1, tx2);
let txExit  = Max(tx1, tx2);
// ... similarly tyEnter/tyExit, tzEnter/tzExit ...

let tEnter = Max(txEnter, Max(tyEnter, tzEnter));
let tExit  = Min(txExit,  Min(tyExit,  tzExit));

let hit = (tEnter <= tExit) && (tExit >= 0.0)
          && (tEnter <= query.MaxDistance || query.MaxDistance <= 0.0);
```

When `Hit` is true, `Distance` is typically `Max(tEnter, 0)` for the first
non-negative intersection along a unit ray, and `Position` is
`Add(Origin, Multiply(Direction.Vector, Distance))`. For pure AABB hits,
`Face` is often the none sentinel (`FaceIndex` with `Value: -1`), and
barycentric/UV fields are meaningless — `RayHit3D` is shared with triangle
hits.

`MaxDistance` of zero or less means unbounded per the spatial-queries file
banner — match that convention when clipping $t_{\mathrm{exit}}$.

## Pitfalls / fine print

**Division by zero.** Axis-parallel rays need an "inside interval or miss"
branch, not a reciprocal of $D_x$. Using huge epsilon reciprocals works but
needs care with overflow.

**Origin inside the box.** $t_{\mathrm{enter}}$ may be negative while
$t_{\mathrm{exit}}$ is positive. Whether that counts as a hit depends on the
query (many pickers say yes; some shadow rays want the exit).

**Inclusive bounds.** `Bounds3D` is inclusive on Min and Max. Rays that
graze a face should count as hits; floating-point may still miss — use
consistent rounding or slight inflation for conservative tests.

**Non-unit directions.** If `Direction` were not unit-length, $t$ would be
scaled. Plato's `Ray3D` stores `Direction3D` (unit by invariant), so
distance equals $t$. Do not feed an unnormalized vector into
`Direction3D`.

**AABB vs OBB.** This algorithm assumes axes aligned with the world. Oriented
boxes need a transform of the ray into box space first (or separating-axis
tests).

**Hit normal.** For shading an AABB you may want the normal of the entered
face (which slab provided $t_{\mathrm{enter}}$). `RayHit3D.Normal` is the
field; filling it is part of a complete implementation, not just the boolean
overlap test.

## Try it

1. Box $[0,1]^3$, ray origin $(-1, 0.5, 0.5)$, direction $(1,0,0)$. What are
   $t_{\mathrm{enter}}$ and $t_{\mathrm{exit}}$?
2. Same box, origin $(0.5, 0.5, 0.5)$, same direction. What is
   $t_{\mathrm{enter}}$? Is the origin inside?
3. Why is an AABB test usually done before a triangle mesh raycast?

<details>
<summary>Answers</summary>

1. Enter at $t=1$ (plane $x=0$), exit at $t=2$ (plane $x=1$).
2. $t_{\mathrm{enter}}$ is negative (already inside); exit at $t=0.5$. Yes,
   origin is inside.
3. Cost: a few divisions/comparisons vs walking every triangle; most rays
   miss the box and skip the mesh entirely.

</details>

## Library recommendations

- **missing-function** — `35-spatial-queries.plato` / `12-intervals-bounds.plato`:
  no `Raycast(bounds: Bounds3D, query: RayQuery3D): RayHit3D` and
  `Bounds3D` does not declare `RayIntersectable3D`. The slab story is
  documented on `Slab3D` but not connected to `Bounds3D` by a function.

- **missing-function** — `16-lines.plato`: no helper to build the three
  axis `Slab3D` values from a `Bounds3D`. Teaching the equivalence currently
  requires hand-built normals and intervals.

- **doc-comment** — `35-spatial-queries.plato`: `RayHit3D` should say which
  fields are significant for non-mesh targets (AABB, sphere, plane) so
  callers know `Face` / `Barycentric` / `Uv` may be sentinels.

- **missing-function** — `12-intervals-bounds.plato`: an interval overlap
  primitive `Overlap(a: NumberInterval, b: NumberInterval): NumberInterval`
  (or boolean) would make the three-slab reduction a direct composition
  instead of ad-hoc min/max chains in every ray-box implementation.

> Resolved 2026-07-28: lines.plato added Slabs(Bounds3D): Array<Slab3D> building the three axis-aligned slabs (item 2). Item 4 (NumberInterval Overlap) is left to the intervals-bounds owner.

> Resolved 2026-07-28: intervals-transforms.library.plato added Overlap(NumberInterval, NumberInterval): NumberInterval (item 4), a concrete-typed interval intersection with the inverted empty encoding for slab-chaining.
