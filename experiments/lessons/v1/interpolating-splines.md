---
lesson: interpolating-splines
title: Interpolating Splines
domain: Curves & surfaces
v3-files: [23-splines.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Interpolating Splines

You have a list of waypoints a camera must visit, or keyframes a joint angle must hit.
A polyline connects them with sharp corners. A Bézier curve shaped by *off-curve*
control points may miss the waypoints entirely. What you want is a curve that **passes
through every given point** and still looks smooth — an **interpolating spline**.

Animation software standardized on a few families: Hermite (explicit tangents),
Catmull-Rom (tangents from neighbors), and Kochanek-Bartels / TCB (artist knobs for
tension, continuity, bias). They are the same cubic idea in different packaging.

## The idea

A spline is piecewise: between consecutive knots, a low-degree polynomial; across knots,
joining rules enforce continuity. **Interpolating** means every data point lies on the
curve. (Approximating splines — B-splines, many Béziers — pull toward control points
without necessarily visiting the interior ones.)

### Cubic Hermite segment

Given endpoints $P_{0}, P_{1}$ and velocities $T_{0}, T_{1}$, the cubic Hermite blend is

$$
\gamma(t) = (2t^{3}-3t^{2}+1)\,P_{0} + (t^{3}-2t^{2}+t)\,T_{0}
          + (-2t^{3}+3t^{2})\,P_{1} + (t^{3}-t^{2})\,T_{1}
$$

for $t \in [0,1]$. It hits $P_{0}$ and $P_{1}$, with $\gamma'(0)=T_{0}$ and
$\gamma'(1)=T_{1}$. String segments together by sharing endpoints; match tangents for
$C^{1}$ continuity.

```
  T0         T1
   ↘         ↗
    P0 ●~~~~● P1
```

### Catmull-Rom

Catmull-Rom builds Hermite tangents automatically from neighbors:

$$
T_{i} = \frac{P_{i+1} - P_{i-1}}{2}
$$

(in the uniform case). Every interior point is visited; the curve is $C^{1}$. The
**alpha** parameterization chooses how chord lengths affect tangents:

| Alpha | Name | Behavior |
|---|---|---|
| $0$ | uniform | can loop/cusp on uneven spacing |
| $0.5$ | centripetal | cusp-free; usual default |
| $1$ | chordal | follows chord lengths more literally |

### Kochanek-Bartels (TCB)

TCB adds per-knot artist controls:

- **Tension** — tighten or loosen the curve through the knot
- **Continuity** — break or preserve tangent alignment ($C^{1}$ vs corner)
- **Bias** — lean the tangent toward the incoming or outgoing segment

Zero tension, continuity, and bias recovers a Catmull-Rom-like default. Nonzero values
are how classic keyframe animation "eases through" or "hits hard" at a pose.

### Linear and natural cubic

Piecewise-linear interpolation is the polyline as a function of $t$. A **natural cubic
spline** through scalar samples sets second derivatives to zero at the ends — the smooth
default for 1D channels (opacity, weight) when you only have values, not tangents.

## In Plato

File `23-splines.plato` groups interpolating families after Bézier/B-spline types.

### Hermite

```plato
type HermiteCurve3D
    implements Curve3D
{
    Start: Point3D;
    StartTangent: Vector3D;
    End: Point3D;
    EndTangent: Vector3D;
}

type HermiteSpline3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Tangents: Array<Vector3D>;
    Closed: Boolean;
}
```

One segment vs a piecewise chain. `Tangents` has the same count as `Points`. `Closed`
joins the last point back to the first. Plane variants `HermiteCurve2D` /
`HermiteSpline2D` are identical in shape.

```plato
seg := HermiteCurve3D(
    Start: p0,
    StartTangent: v0,
    End: p1,
    EndTangent: v1)
mid := Eval(seg, 0.5)

path := HermiteSpline3D(
    Points: waypoints,
    Tangents: velocities,
    Closed: false)
```

### Catmull-Rom

```plato
type CatmullRomCurve3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Alpha: Number;
    Closed: Boolean;
}
```

No tangent array — neighbors define them. Set `Alpha` to `0.5` for centripetal unless
you have a reason not to.

```plato
cam := CatmullRomCurve3D(
    Points: cameraKnots,
    Alpha: 0.5,
    Closed: false)
Eval(cam, 0)    // first point
Eval(cam, 1)    // last point (open) or back to first (closed)
```

### TCB

```plato
type TcbSpline3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Tensions: Array<Number>;
    Continuities: Array<Number>;
    Biases: Array<Number>;
    Closed: Boolean;
}
```

Parallel arrays: one tension, continuity, and bias per point. The classic keyframe
animation spline.

```plato
anim := TcbSpline3D(
    Points: keys,
    Tensions: zeros,
    Continuities: zeros,
    Biases: zeros,
    Closed: false)
```

### Linear and 1D natural cubic

```plato
type LinearSpline3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Closed: Boolean;
}

type NaturalCubicSpline1D
    implements Curve1D
{
    Parameters: Array<Number>;
    Values: Array<Number>;
}
```

`LinearSpline3D` is the curve-as-function view of a point chain (the geometric chain
type `Polyline3D` lives elsewhere). `NaturalCubicSpline1D` interpolates scalar
`Values` at strictly increasing `Parameters`, with zero second derivative at the ends.

## Pitfalls / fine print

**Tangent scale.** Hermite tangents are velocities with respect to the segment's $[0,1]$
parameter. Scaling a tangent by 2 pulls harder / overshoots. Catmull-Rom's automatic
tangents hide this; hand-authored Hermite does not.

**Uniform Catmull-Rom cusps.** With `Alpha = 0` and uneven point spacing, the curve can
form loops or cusps. Prefer `0.5` (centripetal) for user-placed points.

**Endpoint tangents.** Open Catmull-Rom needs a policy for the first and last tangents
(duplicate endpoints, phantom points, or one-sided differences). The type stores only
`Points` — the implementation chooses the endpoint rule; read it when libraries land.

**Closed loops.** `Closed: true` wraps indexing so the first and last segments use
neighbors across the seam. The point array should *not* repeat the first point at the
end; the flag handles joining.

**TCB array lengths.** `Tensions`, `Continuities`, and `Biases` must match `Points`
count. Mismatched parallel arrays are a documented invariant, not a structurally
enforced one.

**Interpolating vs approximating.** `BezierCurve3D` and `BSplineCurve3D` in the same
file generally do *not* pass through interior controls. If your mental model is
"control points are waypoints," you want Catmull-Rom / Hermite / TCB, not Bézier.

**Parameter bunching.** Equal $\Delta t$ on a Hermite spline is not equal arc length.
Camera paths that must move at constant speed still need arc-length reparameterization
on top of the interpolating spline.

## Try it

1. Points $(0,0)$, $(1,0)$, $(1,1)$. Roughly sketch why uniform Catmull-Rom might
   behave worse than centripetal near the corner.
2. A `HermiteCurve3D` with `StartTangent = EndTangent = (0,0,0)`. What shape is the
   segment?
3. Why does `NaturalCubicSpline1D` take separate `Parameters` instead of assuming
   evenly spaced samples?

<details>
<summary>Answers</summary>

1. The turn is sharp in chord length; uniform parameterization treats parameter steps
   equally and can overshoot. Centripetal scales by chord length$^{1/2}$ and avoids the
   cusp/loop.
2. Straight line from Start to End (cubic terms vanish; it reduces to linear
   interpolation of the endpoints).
3. Keyframes and samples are often uneven in time or in the driving parameter; natural
   cubics interpolate $(t_i, y_i)$ pairs, not only y-values on a fixed grid.

</details>

## Library recommendations

- **missing-function** — `23-splines.plato`: `CatmullRomCurve2D/3D` expose `Alpha` but
  declare no helpers to build tangents explicitly, and no conversion
  `ToHermiteSpline(catmull)`. Teaching "Catmull-Rom is Hermite with automatic tangents"
  wants that bridge as a named operation.

- **doc-comment** — `23-splines.plato`: `CatmullRomCurve3D` documents alpha values but
  not the open-curve endpoint tangent policy. One sentence on phantom points vs
  one-sided differences would remove a major implementation ambiguity for readers.

- **missing-type** — `23-splines.plato`: there is no shared
  `InterpolatingSpline3D` concept marking "passes through every point." `Curve3D` alone
  does not distinguish interpolating from approximating families in the type system.

- **naming** — `23-splines.plato`: `CatmullRomCurve3D` is a multi-span spline, while
  `HermiteCurve3D` is a single segment and `HermiteSpline3D` is multi-span. The
  Curve-vs-Spline naming is inconsistent across families (Catmull-Rom has no `Spline`
  sibling name).
