---
lesson: lines-rays-segments
title: Lines, Rays, and Segments
domain: Geometry primitives
v3-files: [16-lines.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Lines, Rays, and Segments

“Draw a straight line from A to B” sounds like one idea. In geometry code it is three.

A **segment** stops at both ends — a wall edge, a cable, a pick ray clipped to a room.
A **ray** starts at a point and runs forever one way — a camera pick, a light path, a
bullet with unlimited range. A **line** runs forever both ways — the supporting line of
an edge, the infinite axis of a cylinder, the solution set of a linear equation.

If you store only two points and forget which of the three you meant, closest-point
queries, intersections, and “is this in front of me?” tests all grow silent wrong
branches.

## The idea

### Parameterization

All three live on the same parametric family:

$$
p(t) = O + t\,D
$$

where $O$ is an origin point and $D$ is a direction. The difference is the allowed $t$:

| Object | Allowed $t$ | Picture |
|--------|-------------|---------|
| Line | all real $t$ | `<----●---->` |
| Ray | $t \ge 0$ | `●---->` |
| Segment | $t \in [0, 1]$ if $D = B - A$, or $t \in [0, L]$ if $D$ is unit and $L$ is length | `●-----●` |

```
  Line:      <──────────────>
  Ray:                ●──────>
  Segment:            ●──────●
```

For a segment with endpoints $A$ and $B$, the usual form is
$p(t) = A + t(B - A)$ with $t \in [0, 1]$. Midpoint at $t = 1/2$. Length $\|B - A\|$.

### Closest point

Given a query point $Q$, project onto the supporting line:

$$
t^\* = \frac{(Q - O)\cdot D}{D\cdot D}
$$

(for non-unit $D$; with unit $D$ the denominator is $1$). Then:

- **Line:** use $t^\*$ as-is.
- **Ray:** clamp $t^\*$ to $[0, \infty)$.
- **Segment:** clamp $t^\*$ to $[0, 1]$ (endpoint form).

That single clamp is why the types must stay distinct. Using a segment algorithm on a
ray incorrectly pulls hits behind the origin onto the origin; using a line algorithm on
a segment reports points past the endpoints as if the edge continued.

### Implicit line in 2D

In the plane, $Ax + By + C = 0$ is the same geometric line with a normal $(A, B)$.
Useful for sidedness tests; less convenient for marching along the line with arc length.

## In Plato

`16-lines.plato` encodes extent in the type name and fields.

```plato
type LineSegment2D
    implements Geometry2D, OpenShape, Connected, Bounded2D,
               LengthMeasurable, PointSet2D, Centroid2D, NearestPoint2D, Deformable2D
{
    A: Point2D;
    B: Point2D;
}

type LineSegment3D
    implements Geometry3D, OpenShape, Connected, Bounded3D,
               LengthMeasurable, PointSet3D, Centroid3D, NearestPoint3D, Deformable3D
{
    A: Point3D;
    B: Point3D;
}

type Line2D
    implements Geometry2D, Connected, NearestPoint2D
{
    Origin: Point2D;
    Direction: Direction2D;
}

type Line3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

type Ray2D
    implements Geometry2D, Connected, NearestPoint2D
{
    Origin: Point2D;
    Direction: Direction2D;
}

type Ray3D
    implements Geometry3D, Connected, NearestPoint3D
{
    Origin: Point3D;
    Direction: Direction3D;
}

type LineEquation2D
{
    A: Number;
    B: Number;
    C: Number;
}
```

Doc comments pin the parameterization: line and ray use `Origin + Direction * t`, with
ray requiring $t \ge 0$. `Direction2D` / `Direction3D` are unit directions by
construction (normalized vectors), so $t$ is arc length along the axis for lines and
rays.

Segments implement `LengthMeasurable` and `Bounded2D`/`Bounded3D`; infinite lines and
rays do not claim finite bounds or length — the type system refuses “length of a line.”

`NearestPoint2D` / `NearestPoint3D` supply `ClosestPoint` — the clamp rules above belong
in those implementations.

Usage-shaped sketches:

```plato
let edge = LineSegment3D {
    A: Point3D { X: 0, Y: 0, Z: 0 },
    B: Point3D { X: 10, Y: 0, Z: 0 }
};
// Length(edge) == 10; ClosestPoint clamps onto [A, B]

let pick = Ray3D {
    Origin: cameraPosition,
    Direction: viewDirection   // Direction3D, unit
};
// ClosestPoint(pick, worldPoint) never goes behind the camera

let axis = Line3D {
    Origin: Point3D { X: 0, Y: 0, Z: 0 },
    Direction: /* +Y */
};
// Cylinder axis, infinite; no Bounds(axis)

let implicit = LineEquation2D { A: 0, B: 1, C: -3 };
// Horizontal line y = 3
```

Degenerate segment: $A = B$. Length zero; direction undefined; closest point is $A$;
many algorithms need an explicit guard.

## Pitfalls / fine print

**Non-unit direction on a ray.** Plato stores `Direction2D`/`Direction3D` (unit). If you
build a ray from a raw displacement, normalize first — or you stretch $t$ and every
intersection formula that assumes unit direction silently skews.

**Segment $t$ domain.** Endpoint form uses $t \in [0,1]$ with non-unit $B-A$. Mixing that
with unit-direction $+[0,L]$ formulas without converting is a frequent off-by-$L$ bug.

**Skew lines in 3D.** Two `Line3D` values may be neither parallel nor intersecting.
Closest points still exist (unique for non-parallel skew lines); intersection does not.
Do not assume 2D intuition.

**Ray vs segment for picking.** A pick ray is infinite; a laser pointer with max range is
a segment (or a ray clamped by a far plane). Using the wrong one clips or fails to clip
hits.

**Implicit vs parametric.** `LineEquation2D` coefficients need not be normalized.
Distance-to-line formulas must divide by $\sqrt{A^2+B^2}$. Parallel coefficient scales
represent the same line.

**OpenShape.** Segments are open in the topological marker sense (they have free ends) —
they are not closed loops. That is unrelated to open/closed intervals on $t$.

## Try it

1. Segment $A=(0,0)$, $B=(4,0)$. Closest point on the segment to $Q=(5,1)$? To
   $Q=(-1,1)$? To $Q=(2,3)$?
2. Same points, but interpret $A$ as origin and $(B-A)$ direction as a **ray**. Closest
   point to $Q=(5,1)$?
3. Why does `Line3D` omit `Bounded3D` while `LineSegment3D` includes it?

<details>
<summary>Answers</summary>

1. $(4,0)$ (past B, clamp to B); $(0,0)$ (before A); $(2,0)$ (orthogonal foot on the
   segment).
2. $(5,0)$ — the ray continues past $B$'s distance; no upper clamp.
3. An infinite line has no finite axis-aligned bound; a segment does. The concept
   `Bounded3D` would be a lie on `Line3D`.

</details>

## Library recommendations

- **missing-function** — `16-lines.plato`: no declared `Parameter(segment|ray|line,
  point)` returning the unclamped or clamped $t$, and no `PointAt(t)` evaluator. Closest-
  point teaching wants both; today only `ClosestPoint` appears via `NearestPoint*`.

- **missing-function** — `16-lines.plato`: no `ClosestPoints(Line3D, Line3D)` or
  segment–segment pair query. Skew-line distance is a standard 3D need and is awkward to
  invent ad hoc beside the types.

- **missing-function** — `16-lines.plato`: no conversion helpers
  `LineSegment3D → Line3D` / `→ Ray3D` (supporting line / ray from A through B). Almost
  every mesh algorithm needs “the infinite line of this edge.”

- **doc-comment** — `16-lines.plato`: `LineSegment2D` says “degenerate when A equals B”
  but does not state the $t\in[0,1]$ parameterization explicitly (rays/lines do state
  theirs). Align the segment comment with the same parametric contract.
