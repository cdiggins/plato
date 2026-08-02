---
lesson: helix
title: The Helix
domain: Curves & surfaces
v3-files: [22-curves-3d.plato]
audience: High-school math and general programming background
status: draft-v1
---

# The Helix

Springs, screws, spiral staircases, DNA cartoons, and the path of a point on a spinning
bolt all share one curve: the **circular helix**. It is the simplest path that is
genuinely three-dimensional — not planar — yet still has constant curvature and constant
torsion. If you only memorize one space curve beyond lines and circles, make it this one.

## The idea

Fix a radius $R > 0$ and a **pitch** $P$ — the height gained in one full turn. In a
coordinate frame whose $Z$ axis is the helix axis, a right-handed helix is

$$
\gamma(\theta) = \bigl(R\cos\theta,\; R\sin\theta,\; \tfrac{P}{2\pi}\,\theta\bigr)
$$

as $\theta$ runs over an interval of angles. One full turn of $\theta$ advances $Z$ by
exactly $P$.

```
        Z
        |    /·
        |  /·
        |/·
        ·-------- XY (circle of radius R)
         ·\
           ·\
             ·\
```

### Pitch and handedness

- **Positive pitch** — rises along $+Z$ while turning with the usual right-hand rule
  (fingers curl in the rotation sense, thumb along $+Z$).
- **Negative pitch** — a left-handed screw; same circle, opposite rise.

Pitch is a length (world units per turn), not an angle. Slope intuition:
$\tan\phi = P / (2\pi R)$ relates pitch angle $\phi$ to $P$ and $R$.

### Why it is special

On a circular helix, **curvature $\kappa$ and torsion $\tau$ are constant**:

$$
\kappa = \frac{R}{R^{2} + b^{2}}, \quad
\tau = \frac{b}{R^{2} + b^{2}}, \quad
b = \frac{P}{2\pi}
$$

Planar curves have $\tau = 0$. A helix is the constant-torsion counterpart of the
circle. Among other things, that makes it a clean test case for differential-geometry
code: evaluate anywhere, expect the same $\kappa$ and $\tau$.

### Arc length

Speed is constant:

$$
\|\gamma'(\theta)\| = \sqrt{R^{2} + b^{2}}
$$

so arc length is proportional to angle. Equal angle steps are equal distance steps —
unlike many polynomial curves.

### Relatives

- **Conical spiral** — radius changes while climbing (spiral on a cone).
- **Spherical spiral (loxodrome)** — constant bearing on a sphere, pole to pole.
- **Torus knot** — closed helix-like winding on a torus.

The circular helix is the constant-radius, constant-pitch baseline those generalize.

## In Plato

File `22-curves-3d.plato` declares analytic space curves. The helix sits with spirals:

```plato
type Helix
    implements Curve3D
{
    Frame: Frame3D;
    Radius: Number;
    Pitch: Number;
    Angles: AngleInterval;
}
```

Semantics from the doc comment:

- Winds about the **frame's Z axis** at distance `Radius`
- Rises `Pitch` per full turn
- Right-handed for **positive** `Pitch`
- Swept over `Angles` (an `AngleInterval`)

`Frame` places and orients the helix: `Origin` is a point on the axis; `ZAxis` is the
screw axis; `XAxis` defines where $\theta = 0$ sits (the angle interval is measured
from the frame's X axis in the usual sense of the file's planar arcs).

```plato
axisFrame := Frame3D(
    Origin: Point3D(0, 0, 0),
    XAxis: Direction3D(1, 0, 0),
    YAxis: Direction3D(0, 1, 0),
    ZAxis: Direction3D(0, 0, 1))

spring := Helix(
    Frame: axisFrame,
    Radius: 1,
    Pitch: 0.5,
    Angles: AngleInterval(Start: /* 0 */, End: /* 6*pi */))  // three turns

p0 := Eval(spring, 0)
p1 := Eval(spring, 1)
```

Canonical parameter $t \in [0,1]$ maps onto the documented sweep `Angles` — so $t$ is
fraction of the angular interval, not necessarily fraction of a single turn.

### Sibling spirals in the same file

```plato
type ConicalSpiral3D
    implements Curve3D
{
    Frame: Frame3D;
    Radii: NumberInterval;
    Height: Number;
    Angles: AngleInterval;
}

type SphericalSpiral3D
    implements Curve3D
{
    Frame: Frame3D;
    Radius: Number;
    TurnCount: Number;
}
```

`ConicalSpiral3D` interpolates winding radius across `Radii` while climbing `Height` over
`Angles`. `SphericalSpiral3D` is a loxodrome on a sphere of `Radius`, running pole to
pole along the frame's Z axis through `TurnCount` revolutions.

```plato
coneSpring := ConicalSpiral3D(
    Frame: axisFrame,
    Radii: NumberInterval(Start: 2, End: 0.2),
    Height: 5,
    Angles: AngleInterval(Start: /* 0 */, End: /* 8*pi */))
```

### Closed relatives

```plato
type TorusKnot
    implements ClosedCurve3D
{
    Frame: Frame3D;
    AxisWindings: Integer;
    TubeWindings: Integer;
    MajorRadius: Number;
    MinorRadius: Number;
}
```

A torus knot winds like a helix wrapped onto a torus and joined shut — `ClosedCurve3D`
promises `Eval(0) = Eval(1)`. The open `Helix` does not close unless you forcibly
identify ends (and even then pitch would have to be zero to meet in space — i.e. it
would no longer be a helix).

### Planar arcs for contrast

```plato
type CircularArc3D
    implements PlanarCurve3D
{
    Frame: Frame3D;
    Radius: Number;
    Angles: AngleInterval;
}
```

Same framing pattern — `Frame` + `Radius` + `Angles` — but `Pitch` is absent and the
curve stays in the frame's XY plane (`PlanarCurve3D`). A helix is what you get when that
circle also climbs.

## Pitfalls / fine print

**Pitch per turn, not per radian.** Formulas sometimes use $b = P/(2\pi)$ as rise per
radian. Plato stores `Pitch` per full turn. Mixing the two off-by-$2\pi$ is the classic
bug.

**Angles vs turns.** `Angles: AngleInterval` over $6\pi$ radians is three turns, not
six. Count carefully when porting from APIs that take `turnCount: Number`.

**Zero radius.** `Radius = 0` collapses to motion along the axis (a line). Curvature
formulas that divide by radius-related terms need a separate line path.

**Zero pitch.** `Pitch = 0` is a planar circular arc in a plane parallel to XY (actually
in a plane $z = \mathrm{const}$ through the start). You could have used `CircularArc3D`.

**Frame handedness.** The frame must be orthonormal and right-handed for the documented
right-hand pitch rule to match intuition. A reflected frame silently flips screw sense.

**Parameter not arc length.** Even though speed is constant in $\theta$, the canonical
$[0,1]$ parameter is normalized over the whole `Angles` span. Arc-length fraction equals
$t$ only because speed is constant — still convert explicitly if you need world distance.

**Infinite helix.** The type is a *finite* sweep over `Angles`. An endless spring is
either a huge interval or a `PeriodicCurve` view that v3 does not attach to `Helix`
today.

## Try it

1. `Radius = 1`, `Pitch = 2\pi`. What is $b$, and what are $\kappa$ and $\tau$?
2. How many turns does `Angles` from $0$ to $4\pi$ contain?
3. Why can't a nontrivial circular helix implement `ClosedCurve3D`?

<details>
<summary>Answers</summary>

1. $b = P/(2\pi) = 1$. Then $\kappa = \tau = 1/(1+1) = 1/2$.
2. Two full turns.
3. After a nonzero pitch advance, the point at angle $\theta+2\pi$ is shifted along Z
   relative to the point at $\theta$; they cannot coincide. Closing would require $P=0$
   (a circle) or a different topology (torus knot on a closed tube).

</details>

## Library recommendations

- **missing-interface** — `22-curves-3d.plato`: `Helix` implements only `Curve3D`, not
  `DifferentiableCurve3D` or `FramedCurve3D`, despite being the canonical constant
  curvature/torsion / easy framing example. Declaring those implementations (once
  libraries exist) would match the math story.

- **missing-function** — `22-curves-3d.plato`: no `TurnCount(Helix): Number`,
  `ArcLength(Helix)`, or `PitchAngle(Helix)` helpers. Teaching pitch vs slope needs
  either doc formulas or named projections from `Radius`/`Pitch`.

- **doc-comment** — `22-curves-3d.plato`: `Helix` says angles are swept but does not
  state how canonical $t \in [0,1]$ maps into `Angles` (linear in angle is the obvious
  choice — write it down). Same gap exists for `ConicalSpiral3D`.

- **naming** — `22-curves-3d.plato`: `SphericalSpiral3D` uses `TurnCount: Number` while
  `Helix` uses `Angles: AngleInterval`. A parallel `TurnCount` view (or a factory from
  turn count) would make multi-turn springs easier to author without hand-building
  angle intervals.
