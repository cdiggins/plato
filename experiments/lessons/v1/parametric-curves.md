---
lesson: parametric-curves
title: Parametric Curves
domain: Curves & surfaces
v3-files: [20-interfaces-curves-surfaces.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Parametric Curves

Draw a circle. One way: the set of points at distance $r$ from a center — an *implicit*
equation. Another way: walk around with a clock hand,

$$
\gamma(t) = (r\cos(2\pi t),\; r\sin(2\pi t)), \quad t \in [0,1]
$$

That second form is a **parametric curve**: a function from a real parameter to a
position. Animation paths, font outlines, camera tracks, and CAD edges are almost always
parametric, because "evaluate at $t$" is the operation you need — not "solve for the
points that satisfy an equation."

## The idea

A parametric curve in the plane or in space is a continuous map

$$
\gamma: I \to \mathbb{R}^{n}
$$

In Plato's conventions, the canonical interval $I$ is the **unit interval** $[0,1]$
unless a type documents otherwise. Closed curves satisfy $\gamma(0) = \gamma(1)$ with
matching continuity at the seam.

### Parameter vs arc length

The number $t$ is *not* "fraction of distance traveled" unless you choose it that way.
On the unit-speed circle above, $t$ *is* fraction of turn, and arc length is $2\pi r\,t$.
On a naive linear parameterization of a stretched ellipse, equal $\Delta t$ steps bunch
up where the curve is sharp and stretch out where it is flat.

**Arc length** $s(t) = \int_{0}^{t} \|\gamma'(u)\|\,du$ reparameterizes the curve so that
equal parameter steps mean equal distance. Many algorithms (dashed strokes, constant-speed
animation, resampling) want arc-length parameter even when the authoring curve used a
convenient but uneven $t$.

```
  t=0 ●----●----●----●----● t=1     equal Δt, unequal spacing

  s=0 ●--●--●--●--●--●--● s=L     equal Δs, equal spacing
```

### Open vs closed

An open curve has distinct ends. A closed curve returns to its start. Periodically
traversable closed curves can be evaluated for any real $t$ by wrapping. Continuity at
the join matters: a square loop is closed as a point set, but has derivative jumps at
corners.

### Derivatives

Where $\gamma$ is smooth, $\gamma'(t)$ is the **velocity** (tangent vector, not
necessarily unit). Curvature measures how fast the tangent turns. In the plane, signed
curvature distinguishes left vs right bends; in space, curvature is unsigned and
**torsion** measures departure from a single osculating plane.

## In Plato

File `20-interfaces-curves-surfaces.plato` encodes curves as interfaces over
`Procedural<Number, Point>` — evaluation is the core operation.

```plato
interface Curve1D
    inherits Procedural<Number, Number>
{ }

interface Curve2D
    inherits Geometry2D, Procedural<Number, Point2D>
{ }

interface Curve3D
    inherits Geometry3D, Procedural<Number, Point3D>
{ }

interface ClosedCurve2D inherits Curve2D { }
interface ClosedCurve3D inherits Curve3D { }
```

`Procedural` (file 04) is simply:

```plato
interface Procedural<TDomain, TRange>
{
    Eval(x: Self, input: TDomain): TRange;
}
```

So a curve is used like:

```plato
p := Eval(curve, 0.25)   // Point2D or Point3D at parameter 1/4
```

### Differentiability and framing

```plato
interface DifferentiableCurve2D
    inherits Curve2D
{
    TangentAt(x: Self, t: Number): Vector2D;
    CurvatureAt(x: Self, t: Number): Number;
}

interface DifferentiableCurve3D
    inherits Curve3D
{
    TangentAt(x: Self, t: Number): Vector3D;
    CurvatureAt(x: Self, t: Number): Number;
    TorsionAt(x: Self, t: Number): Number;
}

interface FramedCurve3D
    inherits DifferentiableCurve3D
{
    FrameAt(x: Self, t: Number): Frame3D;
}
```

`TangentAt` returns velocity — not necessarily unit length. Plane `CurvatureAt` is
signed (positive turning left). Space `CurvatureAt` is unsigned. `FrameAt` places a
`Frame3D` whose origin lies on the curve and whose Z axis is tangent — the basis for
sweeps and camera paths.

### Arc length

```plato
interface ArcLengthParameterized<TPoint>
{
    ArcLength(x: Self): Number;
    PointAtLength(x: Self, length: Number): TPoint;
    ParameterAtLength(x: Self, length: Number): Number;
}
```

`ArcLength` is total length over the canonical domain. `PointAtLength` walks distance
from the start. `ParameterAtLength` converts a distance back to the authoring parameter
$t$ — the expensive inverse that dashed strokes and constant-speed motion need.

### Other curve capabilities

```plato
interface PeriodicCurve
    inherits Periodic<Number>
{ }

interface PlanarCurve3D
    inherits Curve3D
{
    CurvePlane(x: Self): Plane;
}

interface PolarCurve2D
    inherits Curve2D
{
    RadiusAt(x: Self, angle: Angle): Number;
}
```

`PeriodicCurve` makes the repeat period explicit. `PlanarCurve3D` asserts the image lies
in one plane (circular arcs in space, for example). `PolarCurve2D` is the radius-as-a-
function-of-angle view; the canonical $[0,1]$ parameter maps to one full turn unless
documented otherwise.

### Surfaces in the same file

The same procedural idea lifts to two parameters:

```plato
interface ParametricSurface
    inherits Surface, Procedural<UvCoordinate, Point3D>
{
    ClosedU(x: Self): Boolean;
    ClosedV(x: Self): Boolean;
}
```

A curve is `Eval` of one number; a surface is `Eval` of a `UvCoordinate`. Closed flags
report seams (cylinder closes in one direction, torus in both).

## Pitfalls / fine print

**Assuming $t$ is arc length.** `Eval(curve, 0.5)` is the midpoint *in parameter*, not
the halfway-along-the-path point, unless the curve is arc-length parameterized.
Constant-speed motion needs `PointAtLength` / `ParameterAtLength`.

**Closed as points vs closed as a smooth loop.** `ClosedCurve2D` promises
`Eval(0) = Eval(1)` with continuous closure. A polyline that repeats its first point is
closed as data but may not implement `ClosedCurve2D` if corner continuity fails the
type's contract.

**Domain outside $[0,1]$.** Concrete types may document other domains (angle intervals,
knot spans). Interface-level teaching assumes $[0,1]$; always read the concrete type.

**Zero speed.** Where $\gamma'(t) = 0$, unit tangents and curvature formulas blow up.
Cusps and stopped parameterizations are legal curves but not nice `DifferentiableCurve`
samples.

**Tangent vs direction.** `TangentAt` returns a `Vector2D`/`Vector3D`. Frames and
normals need a `Direction3D` — normalize when the speed is nonzero. Do not assume the
library already unitized the vector.

**Curve1D is not a path.** `Curve1D` maps `Number → Number` — profiles, envelopes,
channels. It is a curve in the 1D line, not a geometric stroke in the plane.

## Try it

1. For $\gamma(t) = (t, t^{2})$ on $[0,1]$, is $t = 0.5$ the arc-length midpoint?
2. Why does `DifferentiableCurve3D` expose unsigned curvature while
   `DifferentiableCurve2D` exposes signed curvature?
3. If `ArcLength(curve) = 10` and you want the point 3 units from the start, which
   function do you call?

<details>
<summary>Answers</summary>

1. No. Speed $\|\gamma'(t)\| = \sqrt{1 + 4t^{2}}$ grows with $t$, so more length lies in
   the second half of the parameter interval than the first.
2. In the plane there is a consistent left/right (the 90° rotate of the tangent). In
   space there is no canonical "left"; curvature is the magnitude of the turning rate,
   and torsion carries the out-of-plane sense.
3. `PointAtLength(curve, 3)` — or `Eval(curve, ParameterAtLength(curve, 3))` if you
   need the parameter as well.

</details>

## Library recommendations

- **missing-interface** — `20-interfaces-curves-surfaces.plato`: there is no
  `UnitSpeedCurve` / marker for "parameter equals arc length," and
  `DifferentiableCurve3D.TangentAt` returns a raw velocity with no
  `UnitTangentAt`. Teaching constant-speed motion has to narrate a normalize step that
  the interface surface does not name.

- **missing-function** — `20-interfaces-curves-surfaces.plato`:
  `ArcLengthParameterized` has total length and conversions, but no
  `LengthBetween(x, t0, t1)` for partial spans. Dash patterns and subpath measuring need
  it constantly.

- **doc-comment** — `20-interfaces-curves-surfaces.plato`: the banner states the canonical
  domain is $[0,1]$, but `PolarCurve2D` and angle-swept concrete curves (elsewhere) use
  angle domains. A sentence on when concrete types override the canonical domain would
  prevent interface/type mismatch in readers' heads.

- **pedagogy** — `FramedCurve3D.FrameAt` returns `Frame3D` with "Z axis is tangent," but
  the interface does not say whether the frame is Frenet or rotation-minimizing. Sweeps
  care deeply which; the ambiguity should be documented or split into distinct interface
  functions.
