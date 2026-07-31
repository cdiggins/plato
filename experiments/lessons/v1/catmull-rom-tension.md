---
lesson: catmull-rom-tension
title: Catmull-Rom Splines, Alpha, and Tension
domain: Curves & surfaces
v3-files: [23-splines.plato]
audience: Comfortable with interpolating a list of points; optional familiarity with cubic Bezier helps but is not required
status: draft-v1
---

# Catmull-Rom Splines, Alpha, and Tension

Keyframe a camera through waypoints and you want a curve that **passes through
every point**, with a tangent at each point that “looks right,” without hand-
authoring those tangents. **Catmull-Rom** splines do exactly that: the tangent
at $P_i$ is derived from the neighbours $P_{i-1}$ and $P_{i+1}$. One knob —
historically called **tension** — tightens or loosens those tangents. A second,
different knob — **alpha** — changes how chord lengths influence the
parameterization. Confusing the two knobs is how you get cusps you cannot
explain.

## The idea

### Tangents from neighbours

On the segment from $P_i$ to $P_{i+1}$, a cubic Catmull-Rom piece uses the four
points $P_{i-1}, P_i, P_{i+1}, P_{i+2}$. The classic *uniform* tangent at an
interior point is

$$
T_i = \frac{P_{i+1} - P_{i-1}}{2}
$$

(up to a tension scale — below). That is also a cubic Hermite segment with
those endpoints and tangents, so Catmull-Rom is “Hermite with automatic
tangents.”

```
  P0           P1           P2           P3
   •------------•------------•------------•
         T1 ~ (P2-P0)/2   T2 ~ (P3-P1)/2
```

### Tension (Barry–Goldman / Kochanek)

A common generalization multiplies the neighbour difference by $(1-\tau)$:

$$
T_i = (1-\tau)\,\frac{P_{i+1} - P_{i-1}}{2}
$$

- $\tau = 0$: standard Catmull-Rom tangents  
- $\tau \to 1$: tangents shrink → curve approaches the polyline  
- $\tau < 0$: overshoot / looser loops  

Kochanek–Bartels (TCB) splines expose **Tension**, **Continuity**, and **Bias**
per knot. Catmull-Rom is the special case with continuity = bias = 0 and a
shared tension. If you only need “tighter through the points,” TCB tension is
the historically correct dial.

### Alpha (centripetal vs chordal)

Separately, **parameterization** chooses how parameter distance relates to
chord length $\|P_{i+1}-P_i\|$:

$$
t_{i+1} = t_i + \|P_{i+1}-P_i\|^\alpha
$$

| $\alpha$ | Name | Behaviour |
|---|---|---|
| $0$ | Uniform | Equal parameter steps; can cusp / loop on uneven spacing |
| $0.5$ | Centripetal | Usually cusp-free; default for animation and drawing |
| $1$ | Chordal | Parameter tracks chord length; can overshoot |

Alpha is **not** tension. Alpha changes *time along the chord*; tension changes
*tangent magnitude*. You can need both: centripetal $\alpha=0.5$ to avoid
cusps, then a mild TCB tension to pull closer to corners.

### Closed curves

For a closed loop, wrap indices mod $N$ so the first and last segments see
neighbours across the seam. Open curves need an endpoint policy (repeat end
points, or phantom points) — otherwise $P_{-1}$ does not exist.

## In Plato

v3’s Catmull-Rom types carry **Alpha**, not tension. Tension lives on TCB
splines. Hermite is there when you want explicit tangents.

From `23-splines.plato`:

```plato
// Alpha selects the parameterization: 0 = uniform, 0.5 = centripetal
// (cusp-free), 1 = chordal.
type CatmullRomCurve2D
    implements Curve2D
{
    Points: Array<Point2D>;
    Alpha: Number;
    Closed: Boolean;
}

type CatmullRomCurve3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Alpha: Number;
    Closed: Boolean;
}

// Kochanek-Bartels (TCB): per-point Tension, Continuity, and Bias
type TcbSpline3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Tensions: Array<Number>;
    Continuities: Array<Number>;
    Biases: Array<Number>;
    Closed: Boolean;
}

type HermiteSpline3D
    implements Curve3D
{
    Points: Array<Point3D>;
    Tangents: Array<Vector3D>;
    Closed: Boolean;
}
```

Usage-shaped sketches:

```plato
// Animation path: centripetal Catmull-Rom through waypoints
camPath = CatmullRomCurve3D {
    Points: waypoints;
    Alpha: 0.5;
    Closed: false;
}

// Same points, uniform parameterization — watch for cusps if spacing varies
uniformPath = CatmullRomCurve3D {
    Points: waypoints;
    Alpha: 0.0;
    Closed: false;
}

// Want "tighter to the corners"? That is TCB tension, not Alpha.
tight = TcbSpline3D {
    Points: waypoints;
    Tensions: [0.5, 0.5, 0.5, 0.5];
    Continuities: [0.0, 0.0, 0.0, 0.0];
    Biases: [0.0, 0.0, 0.0, 0.0];
    Closed: false;
}

// Fully manual: Catmull-Rom is Hermite with chosen tangents
manual = HermiteSpline3D {
    Points: waypoints;
    Tangents: authoredTangents;
    Closed: false;
}

p = camPath.Eval(0.25);   // Curve3D via Procedural<Number, Point3D>
```

There is **no** `Tension` field on `CatmullRomCurve2D/3D`. If a paper or engine
says “Catmull-Rom tension $\tau$,” map it to `TcbSpline*` with zero continuity
and bias, or to Hermite tangents scaled by $(1-\tau)$ — do not overload
`Alpha`.

## Pitfalls / fine print

- **Alpha vs tension naming in APIs.** Some libraries call their parameterization
  knob “tension.” In v3, believe the field names: `Alpha` on Catmull-Rom,
  `Tensions` on TCB.
- **Uneven point spacing + $\alpha=0$.** Classic source of unexpected loops.
  Prefer $0.5$ unless you know the samples are evenly spaced in space *and*
  you want uniform parameter speed anyway.
- **Endpoints on open curves.** The first and last points need a neighbour
  convention; different hosts disagree (repeat point vs reflect). v3’s
  declarations do not yet document which convention `Eval` must use.
- **Parallel array lengths.** TCB requires `Tensions`/`Continuities`/`Biases`
  to match `Points` count — documented invariant, not a type-system proof.
- **Closed flag.** Forgetting `Closed: true` on a loop leaves a gap (or a bad
  tangent) between last and first.
- **Overshoot.** Even centripetal CR can leave the convex hull of local
  points; collision-sensitive paths may need clamping or Bezier conversion
  with hull tests.

## Try it

1. Four colinear points spaced evenly. Does changing `Alpha` from $0$ to $1$
   change the image curve?
2. You set `Alpha: 0.8` hoping to “pull the curve tighter to corners.” What
   did you actually change, and what type should you have used?
3. Express standard Catmull-Rom ($\tau=0$) as a `TcbSpline3D` configuration.

<details>
<summary>Answers</summary>

1. Almost not at all — for equal chord lengths every $\alpha$ yields the same
   $t$-increments up to a global scale, so the geometric image matches; only
   the mapping from canonical $[0,1]$ domain to chord progress may rescale.
2. You changed parameterization (chord weighting), not tangent magnitude. Use
   `TcbSpline3D` with positive `Tensions` (and zero continuity/bias) or scale
   Hermite tangents.
3. `Tensions` all $0$, `Continuities` all $0`, `Biases` all $0$, same `Points`
   and `Closed`.

</details>

## Library recommendations

- **missing-function** / **doc-comment** — `23-splines.plato`:
  `CatmullRomCurve2D/3D` should document endpoint tangent conventions for
  open curves. Without that, two implementations can both “be Catmull-Rom”
  and disagree near the ends — fatal for golden-master tests.
- **pedagogy** — `23-splines.plato`: the `Alpha` doc comment is excellent; add
  one explicit sentence: “Alpha is not tension; use `TcbSpline*` for per-point
  tension.” The lesson’s entire confusion class is people treating them as
  synonyms.
- **missing-type** / **missing-function** — `23-splines.plato`: a single
  optional `Tension: Number` on Catmull-Rom (uniform $\tau$ applied to all
  automatic tangents) would match what many graphics APIs expose as
  “CatmullRom(tension).” Today authors misuse `Alpha` or switch types.
- **wrong-shape** — `23-splines.plato`: `TcbSpline*` stores three parallel
  `Array<Number>` channels. A `TcbParameters { Tension; Continuity; Bias }`
  per point (or `Array<TcbParameters>`) would make the equal-length invariant
  local and teachable as one record per knot.
- **missing-function** — `23-splines.plato`: no
  `ToHermite(cr: CatmullRomCurve3D): HermiteSpline3D` conversion. Teaching
  “CR is Hermite with derived tangents” wants that bridge as a named operation.
