---
lesson: bezier-curves
title: Bézier Curves
domain: Curves & surfaces
v3-files: [21-curves-2d.plato, 22-curves-3d.plato]
audience: High-school polynomials and parametric curves; vector graphics familiarity helpful but not required.
status: draft-v1
---

# Bézier Curves

Every TrueType outline, every SVG path, every UI easing handle you drag is probably a **cubic
Bézier**. The curve never stores thousands of samples. It stores a handful of **control points**
and a rule that turns a parameter $t \in [0,1]$ into a position. Editors show the control polygon;
renderers evaluate the polynomial; fonts ship the same cubics that designers drew.

The mathematical gift is de Casteljau's algorithm: linear interpolation, applied recursively. If
you can lerp, you can draw a Bézier.

## The idea

### Linear case (the forgotten Bézier)

Two points $P_0$, $P_1$. The linear Bézier is ordinary lerp:

$$
B(t) = (1-t)\,P_0 + t\,P_1
$$

That is the segment from $P_0$ to $P_1$. Everything richer is "lerp the lerps."

### Quadratic: one handle

Three points $P_0$, $P_1$, $P_2$. De Casteljau:

1. Lerp $P_0\to P_1$ to get $A(t)$.
2. Lerp $P_1\to P_2$ to get $B(t)$.
3. Lerp $A\to B$ to get the curve point $Q(t)$.

Closed form:

$$
Q(t) = (1-t)^2 P_0 + 2(1-t)t\, P_1 + t^2 P_2
$$

$P_0$ and $P_2$ lie on the curve. $P_1$ usually does **not** — it pulls the arc. The tangent at
$t=0$ aims from $P_0$ toward $P_1$; at $t=1$, from $P_2$ back toward $P_1$.

```
P1 ●
   / \
  /   \
 /     \
P0 ●······● P2     ····· = curve
```

### Cubic: the workhorse

Four points $P_0..P_3$. Same recursion one level deeper, or the Bernstein form:

$$
C(t) = (1-t)^3 P_0 + 3(1-t)^2 t\, P_1 + 3(1-t)t^2 P_2 + t^3 P_3
$$

Endpoints $P_0$, $P_3$ are interpolated. Interior controls $P_1$, $P_2$ shape the tangents:

- Leaving $P_0$, the curve heads toward $P_1$.
- Arriving at $P_3$, the curve comes from the direction of $P_2$.

```
P1 ●--------● P2
  /          \
 /            \
P0 ●············● P3
```

### Convex hull and variation diminishing

A Bézier curve lies inside the **convex hull** of its control points. That single fact powers
culling, hit-testing bounds, and "the curve cannot escape this box" reasoning in editors.

Moving one control point changes the whole curve (global support). That is why long font outlines
are split into many short cubics: local edits stay local when each glyph is a chain of small arcs.

### Why fonts and UIs run on cubics

- **Compact:** four points vs hundreds of polyline samples.
- **Smooth enough:** $C^1$ joins are easy (align handles); $C^2$ is possible with care.
- **Stable under affine maps:** transform the controls, the curve transforms.
- **Subdivision:** de Casteljau at $t=1/2$ splits one cubic into two cubics — the engine behind
  adaptive flattening for screens and lasers.

Quadratics appear in older TrueType; modern OpenType CFF and most design tools standardize on
cubics. Plato carries both.

## In Plato

### 2D arcs

From `21-curves-2d.plato`:

```plato
// A quadratic Bezier arc: interpolates P0 to P2, shaped by the middle control
// point P1. Tangents at the ends aim at P1.
type QuadraticBezier2D
    implements Curve2D
{
    P0: Point2D;
    P1: Point2D;
    P2: Point2D;
}

// A cubic Bezier arc: interpolates P0 to P3, shaped by the interior control
// points P1 and P2. The workhorse of vector graphics and font outlines.
type CubicBezier2D
    implements Curve2D
{
    P0: Point2D;
    P1: Point2D;
    P2: Point2D;
    P3: Point2D;
}
```

`Curve2D` inherits `Procedural<Number, Point2D>`, so evaluation is `Eval` on the unit interval:

```
curve = CubicBezier2D(p0, p1, p2, p3)
start = curve.Eval(0)      // equals P0
end   = curve.Eval(1)      // equals P3
mid   = curve.Eval(0.5)    // not the midpoint of the chord in general
```

### 3D arcs

From `22-curves-3d.plato` — the same control structure, positions in space:

```plato
type QuadraticBezier3D
    implements Curve3D
{
    P0: Point3D;
    P1: Point3D;
    P2: Point3D;
}

type CubicBezier3D
    implements Curve3D
{
    P0: Point3D;
    P1: Point3D;
    P2: Point3D;
    P3: Point3D;
}
```

```
path3d = CubicBezier3D(a, b, c, d)
p = path3d.Eval(t)         // Point3D on the space curve
```

A cubic Bézier in 3D is still a polynomial of degree 3; it need not lie in a plane unless the four
controls are coplanar. For planar arcs with an explicit supporting plane, Plato also offers
`CircularArc3D` / `EllipticalArc3D` (frame-based) — different tools, not Béziers.

### Parameter vs arc length

The canonical parameter $t$ is **not** arc-length fraction. `Eval(0.5)` is halfway in parameter
space, not necessarily halfway along the ink. Animation that must travel at constant speed needs
arc-length reparameterization (`ArcLengthParameterized` exists as a concept for curves that
provide it; the Bézier types do not currently claim that concept).

### Building a path

A font outline or SVG path is a polyline of Bézier segments sharing endpoints:

```
seg0 = CubicBezier2D(a, b, c, d)
seg1 = CubicBezier2D(d, e, f, g)   // G0 continuity: shared point d
// For G1: (e - d) parallel to (d - c)
```

Plato's path vocabulary (`Path2D` / `PathSegment2D` elsewhere) consumes these arcs; the Bézier
types themselves stay minimal records of four points.

## Pitfalls / fine print

**Control points are not samples.** Sampling only $P_0..P_3$ as if they were polyline vertices
draws the control polygon, not the curve.

**$t$ is not distance.** Constant-$\Delta t$ sampling clusters near high-curvature regions for some
shapes and races through flat regions. Adaptive subdivision (flatten until flatness tolerance)
fixes drawing; constant-speed motion needs a different parameterization.

**Degree elevation ≠ more freedom in the middle.** You can write a quadratic as a cubic with
dependent controls; that does not add a new independent handle.

**Self-intersections and loops.** Cubics can loop; convex-hull tests still hold, but "simple arc"
assumptions in boolean ops can fail.

**Cusps.** When consecutive controls coincide or handles collapse, the tangent can vanish.
`DifferentiableCurve2D.TangentAt` would report a zero velocity — guard before normalizing.

**Global support.** Editing $P_1$ moves the entire cubic. For local edits, split into multiple
segments (de Casteljau subdivision) instead of raising degree.

## Try it

<details>
<summary>Exercise 1 — Endpoint check</summary>

For `CubicBezier2D`, evaluate the Bernstein form at $t=0$ and $t=1$. Which control points appear?

**Answer.** $C(0)=P_0$, $C(1)=P_3$. Interior controls drop out.
</details>

<details>
<summary>Exercise 2 — Tangent direction</summary>

Quadratic with $P_0=(0,0)$, $P_1=(1,2)$, $P_2=(2,0)$. In which direction does the curve leave $P_0$?

**Answer.** Toward $P_1$: direction $(1,2)$ (not necessarily unit length).
</details>

<details>
<summary>Exercise 3 — Predict Eval(0.5) for a line-like cubic</summary>

$P_0=(0,0)$, $P_1=(1,0)$, $P_2=(2,0)$, $P_3=(3,0)$. What is $C(0.5)$?

**Answer.** Everything is colinear on the x-axis; $C(0.5)=(1.5, 0)$ — the midpoint — because this
cubic is actually the linear segment in disguise (degree elevation of a line).
</details>

## Library recommendations

- **missing-concept** — `21-curves-2d.plato` / `22-curves-3d.plato`: `QuadraticBezier2D` and
  `CubicBezier2D` (and 3D twins) implement only `Curve2D` / `Curve3D`, not
  `DifferentiableCurve2D` / `DifferentiableCurve3D`. Béziers have closed-form tangents; claiming
  the differentiable concepts would unlock `TangentAt` without host-side special cases.

- **missing-function** — Bézier types: no declared `Subdivide(t)`, `Split`, `Derivative`, or
  `BoundingBounds` helpers. De Casteljau subdivision and AABB-from-controls are the two operations
  every renderer needs; teaching them immediately surfaces the gap on the type surface.

- **missing-function** — no `ArcLengthParameterized` implementation story for cubics (numeric only).
  A documented `ApproximateArcLength` or sampled LUT type keyed off `CubicBezier2D` would make the
  parameter-vs-length pitfall actionable in API form.

- **doc-comment** — `CubicBezier2D`: state explicitly that $t$ is the Bernstein parameter on
  $[0,1]$, not arc length, and that the curve lies in the convex hull of $\{P_0..P_3\}$. Those two
  sentences prevent the most common misuse more effectively than the current "workhorse" gloss alone.

- **pedagogy** — linear Bézier (two points) is absent as a named type; `LineSegment2D` covers the
  geometry, but a `LinearBezier2D` alias or doc cross-link from the quadratic comment would complete
  the de Casteljau ladder readers expect when learning the family.
