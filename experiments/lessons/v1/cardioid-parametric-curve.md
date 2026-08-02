---
lesson: cardioid-parametric-curve
title: The Cardioid as a Parametric Curve
domain: Curves & surfaces
v3-files: [21-curves-2d.plato]
audience: Comfortable with polar coordinates and basic trigonometry; no prior curve-API experience assumed.
status: draft-v1
---

# The Cardioid as a Parametric Curve

A circle rolling around a fixed circle of the same size traces a heart-shaped path.
That path is the **cardioid**. It shows up in caustics of a coffee cup, in microphone
pickup patterns, and as the boundary case of a limaçon. In Plato it is a named closed
plane curve with a single radius parameter — not a hand-rolled polar formula in every
caller.

This lesson builds the polar equation, maps it onto the canonical curve parameter, and
shows how `Cardioid2D` sits among related figure curves in `21-curves-2d.plato`.

## Polar form and rolling-circle picture

Place a fixed circle of radius $a$ centered at the origin. Roll an equal circle around
its outside without slipping. A marked point on the moving circle's rim traces:

$$
r(\theta) = 2a\,(1 + \cos\theta)
$$

in polar coordinates about the origin, with $\theta$ measured from $+X$. Equivalently
in Cartesian form:

$$
x = 2a\,(1 + \cos\theta)\cos\theta,\qquad
y = 2a\,(1 + \cos\theta)\sin\theta
$$

```
        y
        ↑
        |     ·····
        |   ·       ·
        |  ·         ·
        | ·     ●     ·     ← cusp at origin when θ = π
        |  ·         ·
        |   ·       ·
        |     ·····
        +----------------→ x
              (widest at θ = 0: r = 4a)
```

| Property | Value |
|----------|-------|
| Total width (along $+X$) | $4a$ |
| Cusp | Origin, when $\theta = \pi$ |
| Closed | Yes — one full turn returns to the same point |
| Area enclosed | $6\pi a^2$ |

The parameter $a$ is the rolling-circle radius. Doubling $a$ scales the whole figure
uniformly; the shape stays self-similar.

### Worked numbers: $a = 1$

| $\theta$ | $r$ | $(x, y)$ approx |
|----------|-----|-----------------|
| $0$ | $4$ | $(4, 0)$ |
| $\pi/2$ | $2$ | $(0, 2)$ |
| $\pi$ | $0$ | $(0, 0)$ cusp |
| $3\pi/2$ | $2$ | $(0, -2)$ |
| $2\pi$ | $4$ | $(4, 0)$ again |

The curve pinches to a single point at the origin, then opens again. That cusp is
smooth in the rolling-circle construction but singular in the polar graph (radius
vanishes while the angle still advances).

## In Plato

`Cardioid2D` stores only the rolling-circle radius and implements `ClosedCurve2D`:

```plato
// The cardioid r = 2 * Radius * (1 + cos theta): the heart-shaped path traced
// by a circle of radius Radius rolling around an equal fixed circle. Always
// closed; total width is 4 * Radius.
type Cardioid2D
    implements ClosedCurve2D
{
    Radius: Number;
}
```

`ClosedCurve2D` is a marker on top of `Curve2D`: a continuous map from a canonical
parameter in $[0,1]$ to `Point2D`, with `Eval(0) = Eval(1)`.

```plato
interface Curve2D
    inherits Geometry2D, Procedural<Number, Point2D>
{ }

interface ClosedCurve2D
    inherits Curve2D
{ }
```

So sampling looks like any other closed plane curve:

```
var heart = Cardioid2D(1.0);
var tip   = Eval(heart, 0.0);     // widest point on +X for Radius = 1
var cusp  = Eval(heart, 0.5);     // origin (θ = π halfway through [0,1])
var again = Eval(heart, 1.0);     // same as tip
```

The type lives in canonical position (cusp at the origin, lobe along $+X$). Place it
elsewhere with a 2D transform — the type itself does not carry a center or rotation
field.

### Polar siblings and the limaçon family

Nearby types in the same file share the polar/figure-curve section:

```plato
type RoseCurve2D
    implements PolarCurve2D
{
    Radius: Number;
    PetalFrequency: Number;
}

type Limacon2D
    implements ClosedCurve2D
{
    Offset: Number;
    Amplitude: Number;
}

type Lemniscate2D
    implements ClosedCurve2D
{
    Scale: Number;
}
```

`PolarCurve2D` adds an explicit polar evaluator:

```plato
interface PolarCurve2D
    inherits Curve2D
{
    RadiusAt(x: Self, angle: Angle): Number;
}
```

`Cardioid2D` implements `ClosedCurve2D` directly rather than `PolarCurve2D`, even
though its defining equation is polar. The limaçon $r = b + c\cos\theta$ degenerates
to the cardioid when $b = c$; Plato exposes that case as its own type so callers do
not have to remember the special Offset/Amplitude equality.

| Type | Equation sketch | Closure |
|------|-----------------|---------|
| `Cardioid2D` | $r = 2a(1+\cos\theta)$ | Always closed |
| `Limacon2D` | $r = b + c\cos\theta$ | Always closed; inner loop when $b < c$ |
| `RoseCurve2D` | $r = a\cos(k\theta)$ | Closes after one or more turns |
| `Lemniscate2D` | $r^2 = a^2\cos(2\theta)$ | Figure-eight through origin |

### Relating parameter $t \in [0,1]$ to angle

Unless a type documents otherwise, the canonical curve domain is $[0,1]$. For the
cardioid, map one full turn by $\theta = 2\pi t$:

```
// Conceptual evaluation (library body, not yet in declarations-only v3):
// θ = 2π * t
// r = 2 * Radius * (1 + cos θ)
// return Point2D(r * cos θ, r * sin θ)
```

Arc-length parameterization is a different contract (`ArcLengthParameterized`);
uniform $t$ is **not** uniform speed — the cusp region packs angle quickly while
Cartesian speed drops toward zero.

## Pitfalls and fine print

**`Radius` is not the polar maximum.** The field is the rolling-circle radius $a$.
The farthest point from the origin is at distance $4a$, not $a$ or $2a$.

**Canonical placement.** There is no `Center` field. If you need a cardioid around
another point, compose with `Transform2D` / `Pose2D` rather than inventing a second
cardioid type.

**Cusp sampling.** Near $t = 0.5$, consecutive samples collapse toward the origin.
Tessellation by equal $t$ steps undersamples the wide lobe and oversamples the cusp;
prefer curvature-aware or arc-length sampling for rendering.

**Limaçon equality.** `Limacon2D(Offset: a, Amplitude: a)` is geometrically a
cardioid, but it is a different Plato type. Prefer `Cardioid2D` when that is what
you mean — the single-field form cannot accidentally open an inner loop.

**Closed vs polar interfaces.** Implementing `ClosedCurve2D` does not automatically
expose `RadiusAt`. If you need polar queries on a cardioid, either convert angle
yourself or ask the library to add `PolarCurve2D` (see recommendations).

## Try it

<details>
<summary>Exercise 1 — Width from Radius</summary>

A `Cardioid2D` with `Radius = 2.5` is drawn. What is the horizontal span from cusp
to the rightmost tip?

**Answer.** Total width is $4 \times \mathrm{Radius} = 10$. The cusp sits at the
origin; the tip is at $x = 4a = 10$ when the figure is in canonical pose.
</details>

<details>
<summary>Exercise 2 — Parameter of the cusp</summary>

Assuming $\theta = 2\pi t$ on $[0,1]$, which $t$ evaluates to the cusp? Which $t$
evaluates to the rightmost tip?

**Answer.** Cusp at $\theta = \pi$ → $t = 1/2$. Tip at $\theta = 0$ (and $2\pi$) →
$t = 0$ and $t = 1$.
</details>

<details>
<summary>Exercise 3 — Limaçon boundary</summary>

You construct `Limacon2D(Offset: 3.0, Amplitude: 3.0)`. Is the path a cardioid? Should
you store it as `Cardioid2D` instead for API clarity?

**Answer.** Geometrically yes (Offset equals Amplitude). Prefer `Cardioid2D(3.0)` so
the intent is a single radius and callers cannot drift Offset independently.
</details>

## Library recommendations

- **missing-interface** — `21-curves-2d.plato`: `Cardioid2D` implements `ClosedCurve2D`
  but not `PolarCurve2D`, despite a polar defining equation and a polar sibling section.
  Adding `PolarCurve2D` (or documenting why it is omitted) would let callers use
  `RadiusAt` uniformly with `RoseCurve2D` and spirals.

- **missing-function** — no declared `Eval` body yet (v3 is declarations-only), and no
  helper `PointAtAngle(cardioid, angle)` on the type. A named angle-based sampler would
  match how textbooks write the cardioid and avoid every consumer re-deriving
  $\theta = 2\pi t$.

- **doc-comment** — the type comment states total width $4 \times \mathrm{Radius}$ but
  does not mention the cusp at the origin or the area $6\pi a^2$. One extra sentence on
  the cusp would prevent "empty hole at center" confusion when plotting samples.

- **pedagogy** — `Limacon2D`'s comment notes that equality of Offset and Amplitude yields
  the cardioid, but `Cardioid2D` does not cross-link the limaçon degeneration. A brief
  "prefer this type when Offset would equal Amplitude" note on `Limacon2D` would steer
  authors toward the one-field form.
