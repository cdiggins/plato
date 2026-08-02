---
lesson: bsplines-and-nurbs
title: B-Splines and NURBS
domain: Curves & surfaces
v3-files: [23-splines.plato, 24-surfaces.plato]
audience: High-school math and general programming background
status: draft-v1
---

# B-Splines and NURBS

Bézier curves are wonderful until you need a long path. Raise the degree and every
control point influences the whole curve — edit one shoulder and the far end twitches.
**B-splines** fix that with *local control*: each control point only affects a span of
the parameter domain. **NURBS** (Non-Uniform Rational B-Splines) add weights so conic
sections — circles, ellipses, hyperbolas — are exact, not approximated. That exactness
is why CAD and STEP interchange standardized on NURBS.

## The idea

### From Bézier to B-spline

A Bézier curve of degree $d$ uses $d+1$ controls and a single Bernstein blend over
$[0,1]$. A B-spline of degree $d$ with $n$ controls uses a **knot vector** — a
non-decreasing sequence of parameter values — to partition the domain into spans. On
each span the curve is a polynomial of degree $d$, and only $d+1$ controls are live.

Invariant: for $n$ control points of degree $d$, the knot vector holds $n + d + 1$
values.

```
  knots:  0 0 0 0  1  2  3  4 4 4 4     (cubic, 7 controls, clamped ends)
  spans:        [--][--][--][--]
```

**Clamped** (open) knots repeat the end value $d+1$ times so the curve interpolates the
first and last controls — Bézier-like ends, B-spline middle. **Uniform** knots are evenly
spaced; **non-uniform** spacing stretches and compresses spans (the "NU" in NURBS).

### Local control and continuity

Moving one control point moves only the spans where its basis function is nonzero — a
window of $d+1$ knot intervals. Interior continuity is $C^{d-1}$ for simple knots;
repeating a knot lowers continuity (a double knot in a cubic yields a $C^{1}$ joint,
and so on). That is how NURBS encode sharp creases without splitting into separate
curves.

### Rational weights — NURBS

A NURBS curve is a B-spline in homogeneous coordinates, projected back:

$$
\gamma(t) = \frac{\sum_i w_i N_{i,d}(t)\,P_i}{\sum_i w_i N_{i,d}(t)}
$$

Weight $w_i > 1$ pulls the curve toward $P_i$; $0 < w_i < 1$ pushes away. Equal weights
reduce to an ordinary B-spline. Circles and other conics need the rational form —
polynomial B-splines cannot produce an exact circle.

### Surfaces

Tensor-product NURBS surfaces use a control *net* (`Array2D` of points), degrees and
knot vectors in $U$ and $V$, and a matching weight net. Same local-control and conic
story, now for patches, fillets, and CAD faces.

## In Plato

### Knots and curves (`23-splines.plato`)

```plato
type KnotVector
    implements Value
{
    Knots: Array<Number>;
}

type BSplineCurve3D
    implements Curve3D
{
    ControlPoints: Array<Point3D>;
    Degree: Integer;
    Knots: KnotVector;
}

type NurbsCurve3D
    implements Curve3D
{
    ControlPoints: Array<Point3D>;
    Weights: Array<Number>;
    Degree: Integer;
    Knots: KnotVector;
}
```

`Weights` has the same count as `ControlPoints`. Plane variants `BSplineCurve2D` /
`NurbsCurve2D` mirror the fields. Arbitrary-degree Bézier and rational Bézier sit
alongside as the single-span special cases:

```plato
type BezierCurve3D
    implements Curve3D
{
    ControlPoints: Array<Point3D>;
}

type RationalBezierCurve3D
    implements Curve3D
{
    ControlPoints: Array<Point3D>;
    Weights: Array<Number>;
}
```

A clamped B-spline with one span and Bernstein-equivalent knots *is* a Bézier curve;
NURBS with one span is a rational Bézier.

```plato
knots := KnotVector(Knots: [0, 0, 0, 0, 1, 2, 3, 3, 3, 3])
curve := BSplineCurve3D(
    ControlPoints: pts,   // 6 points
    Degree: 3,
    Knots: knots)

nurbs := NurbsCurve3D(
    ControlPoints: pts,
    Weights: weights,
    Degree: 3,
    Knots: knots)
Eval(nurbs, 0.5)
```

### Surfaces (`24-surfaces.plato`)

Control nets use `Array2D` with U along columns and V along rows:

```plato
type BSplineSurface
    implements ParametricSurface
{
    ControlPoints: Array2D<Point3D>;
    UDegree: Integer;
    VDegree: Integer;
    UKnots: KnotVector;
    VKnots: KnotVector;
}

type NurbsSurface
    implements ParametricSurface
{
    ControlPoints: Array2D<Point3D>;
    Weights: Array2D<Number>;
    UDegree: Integer;
    VDegree: Integer;
    UKnots: KnotVector;
    VKnots: KnotVector;
}

type BezierPatch
    implements ParametricSurface
{
    ControlPoints: Array2D<Point3D>;
}
```

Knot counts: `UKnots` holds column count + `UDegree` + 1 values; `VKnots` holds row
count + `VDegree` + 1. `BezierPatch` degrees are inferred from net dimensions
(degree = count − 1 each way).

```plato
patch := NurbsSurface(
    ControlPoints: net,
    Weights: wNet,
    UDegree: 3,
    VDegree: 3,
    UKnots: uKnots,
    VKnots: vKnots)
p := Eval(patch, UvCoordinate(U: 0.5, V: 0.25))
```

`ParametricSurface` also reports `ClosedU` / `ClosedV` — a periodic tube or torus-like
patch closes in one or both directions via suitable knot/control wrapping.

## Pitfalls / fine print

**Off-by-one knot counts.** The rule $n + d + 1$ is merciless. One missing knot and
basis evaluation is undefined. Validate before `Eval`.

**Non-decreasing knots.** Knot vectors must be non-decreasing. A decrease is not a
creative parameterization — it is invalid input.

**Weights must be positive** for the usual projective interpretation (zero weights drop
a control; negatives break the convex-hull comfort zone). Plato's type is `Number` with
no enforcement in the declaration.

**Degree vs control count.** You need at least $d+1$ controls. A cubic with three
points cannot exist.

**Local edit ≠ small edit in screen space.** Local control limits the *parameter*
support, not the perceived magnitude: a large weight or a control far from the curve
still moves the shape dramatically inside its window.

**Exact circle recipe.** Building a NURBS circle needs specific control angles and
weights (quarter-circle rational Bézier segments are the usual brick). "NURBS can do
circles" does not mean "any four points with weight 1 make a circle."

**Interpolating confusion.** B-spline / NURBS controls are generally *not* waypoints.
If you need the curve to pass through given points, solve an interpolation system (or
use an interpolating spline family) — do not place controls on the samples and hope.

**Surface UV orientation.** Plato's convention is U ↔ columns, V ↔ rows. Mixing that
with a row-major mental model flips degree and knot associations.

## Try it

1. Cubic ($d=3$) B-spline with $n=5$ controls. How many knots are required?
2. All weights equal to $1$ on a `NurbsCurve3D`. How does it relate to `BSplineCurve3D`?
3. Why does repeating an interior knot reduce continuity?

<details>
<summary>Answers</summary>

1. $n + d + 1 = 5 + 3 + 1 = 9$ knots.
2. It is the same curve as the B-spline with the same controls, degree, and knots —
   rational projection cancels when weights are constant.
3. Basis smoothness at a knot drops with multiplicity: each extra repeat removes one
   order of continuity, eventually allowing a sharp corner.

</details>

## Library recommendations

- **missing-function** — `23-splines.plato`: no declared
  `IsValid(BSplineCurve3D)` / knot-count check, no `ClampedKnots(n, degree)`, and no
  `InsertKnot` / `ElevateDegree`. Teaching and authoring both need knot construction
  helpers; the types alone leave the algebra off-stage.

- **missing-function** — `23-splines.plato`: no conversion
  `ToNurbs(BezierCurve3D)` or `ToBezierSpans(BSplineCurve3D)`. The pedagogical
  "Bézier is a special B-spline" story wants explicit bridges.

- **doc-comment** — `24-surfaces.plato`: `NurbsSurface` says it represents quadrics
  exactly, but does not warn that weights and net must be specialized. A pointer to the
  standard unit-circle / sphere constructions would prevent false confidence.

- **wrong-shape** — `23-splines.plato`: `Degree` is a free `Integer` on the curve while
  knot/control invariants are comment-only. A factory type or constrained constructor
  interface would make illegal $(n,d,knots)$ triples harder to represent.
