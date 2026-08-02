---
lesson: polygons-and-winding
title: Polygons and Winding
domain: Geometry primitives
v3-files: [19-polygons.plato, 41-vector-styling.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Polygons and Winding

Trace a fence counter-clockwise around a yard and the interior sits to your left. Trace
the same posts clockwise and “inside” flips. Self-intersect a bow-tie and suddenly you
need a policy for which pockets count as filled. Vector graphics, GIS rings, and mesh
silhouettes all encode that policy as **winding** — the oriented order of vertices —
plus an optional **fill rule** when contours overlap or cross.

Get winding wrong and holes become solid, fonts look like Swiss cheese inverted, and
normals on extruded walls point inward.

## The idea

### Simple polygons

A **simple polygon** is a closed chain of edges that does not cross itself. Plato’s
convention: vertices in order, last joins first; **counter-clockwise (CCW) encloses
positive area**.

Signed area via the shoelace formula:

$$
A = \tfrac{1}{2}\sum_{i=0}^{n-1}(x_i y_{i+1} - x_{i+1} y_i),
$$

with $(x_n,y_n) = (x_0,y_0)$. $A > 0$ for CCW, $A < 0$ for clockwise, $A = 0$ for a
degenerate collapse.

```
  CCW (positive):        CW (negative):
       1──2                  1──4
       │  │                  │  │
       0──3                  2──3
```

### Holes

A region with holes is “inside the outer boundary and outside every hole.” Hole rings
wind **opposite** the outer boundary: if the outer is CCW, holes are clockwise. That way
shoelace-style integrals subtract hole area automatically when orientations are respected.

### Fill rules (self-intersections and overlaps)

When paths cross themselves or multiple contours nest oddly, two classic rules decide
whether a point is inside:

| Rule | Idea |
|------|------|
| **NonZero** | Walk a ray to infinity; add $+1$ or $-1$ for each crossing by winding direction; inside if the total ≠ 0 |
| **EvenOdd** | Count crossings; inside if the count is odd |

```
  NonZero: nested CCW rings stay filled
  EvenOdd: nested rings alternate fill/hole
```

Bow-tie (self-intersecting quad): EvenOdd typically fills two opposite triangles;
NonZero’s result depends on traversal direction through the crossing.

### From geometry to paint

Winding is not only a boolean predicate. Stroking and filling consume the same oriented
contours: offsetting “to the left of travel” expands a CCW shape; markers orient along
the tangent of the directed path. Style types pair geometry with appearance but inherit
the orientation story.

## In Plato

`19-polygons.plato` declares the geometric polygons; winding is in the file banner and
invariants.

```plato
type Polygon2D
    implements Geometry2D, ClosedShape, Connected, PlanarMeasurable,
               Bounded2D, PointSet2D, Centroid2D, ContainsPoint2D,
               NearestPoint2D, Deformable2D
{
    Points: Array<Point2D>;
}

type ConvexPolygon2D
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, PointSet2D, Centroid2D,
               ContainsPoint2D, NearestPoint2D, SupportMappable2D, Deformable2D
{
    Points: Array<Point2D>;
}

type PolygonWithHoles2D
{
    Boundary: Polygon2D;
    Holes: Array<Polygon2D>;
}

type PolygonSet2D
{
    Polygons: Array<PolygonWithHoles2D>;
}

type Polyline2D
{
    Points: Array<Point2D>;
    Closed: Boolean;
}
```

Invariants (from comments): ≥ 3 vertices for `Polygon2D`, no edge crossings; holes lie
strictly inside, are disjoint, and wind opposite the boundary. `ConvexPolygon2D`
strengthens to strictly convex position (unlocks support mapping and faster contains).

Fill rules are declared on vector paths in `40-paths.plato` (used when outlines become
drawable paths):

```plato
type FillRule = NonZero | EvenOdd;

type Path2D
{
    Contours: Array<Contour2D>;
    FillRule: FillRule;
}
```

`Polygon2D` itself does not carry a `FillRule` — simple polygons without self-intersection
do not need one for containment; paths do.

`41-vector-styling.plato` attaches appearance. Orientation shows up in offset policy:

```plato
type FillStyle
{
    Paint: Paint;
    Opacity: Proportion;
}

type StrokeStyle
{
    Width: Number;
    Cap: LineCap;
    Join: LineJoin;
    MiterLimit: Number;
    Dash: DashPattern;
    Align: StrokeAlign;
}

type StyledPath2D
{
    Path: Path2D;
    Fill: FillStyle;
    Stroke: StrokeStyle;
}

type PathOffsetParameters
{
    Distance: Number;
    Join: LineJoin;
    MiterLimit: Number;
    Cap: LineCap;
}
```

Doc comment on `PathOffsetParameters`: positive distances offset to the **left** of each
contour’s direction of travel, which **expands counter-clockwise** closed contours.

Usage-shaped sketches:

```plato
let square = Polygon2D {
    Points: /* (0,0), (1,0), (1,1), (0,1) */  // CCW
};
// Area > 0; Contains(center) == true

let yard = PolygonWithHoles2D {
    Boundary: square,
    Holes: /* one CW rectangle inside */
};
// Area = outer - hole; Contains skips the hole interior

let path = Path2D {
    Contours: /* ... */,
    FillRule: EvenOdd
};

let drawn = StyledPath2D {
    Path: path,
    Fill: FillStyle { Paint: /* Solid(...) */, Opacity: /* 1 */ },
    Stroke: /* ... */
};
```

`RegularStar2D` is a built-in self-intersecting-friendly star; containment still needs a
consistent rule when interpreted as a filled region via a path.

## Pitfalls / fine print

**Silent CW data.** Many file formats store clockwise earth polygons (ring exterior
clockwise in some GIS standards). Import must reverse to match Plato’s CCW-positive
invariant or every `Area` sign and hole test lies.

**Hole winding.** A hole that accidentally matches the outer winding **adds** area
instead of subtracting when naively summed — or breaks `Contains`. Validate opposite
winding.

**FillRule on the wrong type.** Applying EvenOdd thinking to a guaranteed-simple
`Polygon2D` is unnecessary; omitting NonZero/EvenOdd on a self-intersecting `Path2D`
makes results renderer-dependent.

**Stroke alignment.** `StrokeAlign` (`Center` / `Inside` / `Outside`) interacts with
which side is “inside,” which depends on winding. Inside/Outside are undefined or odd
for open polylines.

**3D polygons.** `Polygon3D` uses the right-hand rule for its normal; coplanarity is an
invariant. Winding still matters, but fill rules are a 2D path concern.

## Try it

1. Vertices $(0,0)$, $(1,0)$, $(0,1)$ in that order. Signed shoelace area? Winding?
2. Same vertices in order $(0,0)$, $(0,1)$, $(1,0)$. What changes?
3. Outer CCW square, hole also CCW. Qualitatively, what goes wrong for area or
   contains?

<details>
<summary>Answers</summary>

1. $A = 1/2 > 0$ — counter-clockwise.
2. Sign flips to $-1/2$ — clockwise; absolute area the same.
3. The hole does not wind opposite the boundary, violating `PolygonWithHoles2D`
   invariants. Area sums may treat the hole as positive; contains tests that assume
   opposite winding mis-classify the hole interior.

</details>

## Library recommendations

- **missing-function** — `19-polygons.plato`: no `SignedArea`, `Winding`, or `Reverse`
  on `Polygon2D`. Teaching shoelace and fixing CW imports wants those names declared
  beside `PlanarMeasurable.Area` (which may be absolute — pin the sign in docs).

- **missing-type / wrong-shape** — `19-polygons.plato` vs `40-paths.plato`: `FillRule`
  lives on `Path2D` only. `PolygonWithHoles2D` / `PolygonSet2D` have no fill-rule field
  because they assume simple nesting — document that self-intersecting polygons must be
  promoted to `Path2D`, or allow an optional rule for authoring tools.

- **doc-comment** — `41-vector-styling.plato`: `PathOffsetParameters` already states left-
  of-travel / CCW expansion — excellent. Cross-reference the polygon winding banner in
  `19-polygons.plato` so style and geometry docs tell one story.

- **missing-function** — `19-polygons.plato`: no `ToPath(Polygon2D|PolygonWithHoles2D) →
  Path2D` implementing `PathLike`. Bridging filled polygons to `StyledPath2D` is the
  natural authoring path and is currently only implied by the path interface elsewhere.
