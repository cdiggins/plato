---
lesson: circles-ellipses
title: Circles and Ellipses
domain: Geometry primitives
v3-files: [17-planar-shapes.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Circles and Ellipses

A circle is the set of points at a fixed distance from a center — the simplest interesting
curve in the plane. Stretch it by different amounts along two axes and you get an
**ellipse**. Both show up as collision shapes, orbit approximations, font curves’
cousins, and UI ornaments. One of them has a perimeter you can write in closed form with
elementary functions; the other is famous for refusing.

## The idea

### Circle

Center $C$, radius $r \ge 0$. Boundary equation $\|P - C\| = r$. Disk (filled) inequality
$\|P - C\| \le r$.

Parametric form with angle $\theta$ from $+X$, counter-clockwise:

$$
P(\theta) = C + r(\cos\theta,\;\sin\theta).
$$

Tangent direction is proportional to $(-\sin\theta,\;\cos\theta)$; the outward unit
normal is $(\cos\theta,\;\sin\theta)$.

```
            θ=π/2
              |
      θ=π ----C---- θ=0
              |
            θ=3π/2
```

Area $\pi r^2$, circumference $2\pi r$. Closest point on the disk to a query: if outside,
project onto the boundary along the ray from $C$; if inside, the query itself.

### Ellipse

Semi-axes $a$ along local $X$, $b$ along local $Y$, optional rotation $R$ about center
$C$. In the local frame:

$$
\left(\frac{x}{a}\right)^2 + \left(\frac{y}{b}\right)^2 = 1.
$$

Parametric form (eccentric anomaly $t$, not arc-length):

$$
P(t) = C + R\bigl(a\cos t,\; b\sin t\bigr).
$$

When $a = b$ you recover a circle. Area is still easy: $\pi a b$.

### Why perimeter is hard

The ellipse circumference is the complete elliptic integral of the second kind:

$$
P = 4a \int_0^{\pi/2} \sqrt{1 - e^2\sin^2\theta}\;d\theta,
\quad e^2 = 1 - \frac{b^2}{a^2}\ (a \ge b).
$$

There is no finite formula using only elementary functions. Practice uses series
(Ramanujan approximations), numerical quadrature, or “good enough” estimates. That is why
APIs often expose area long before they expose an accurate perimeter for `Ellipse`.

### Tangents and support

For collision (GJK), the **support point** in direction $\mathbf{d}$ on a circle is
$C + r\,\hat{\mathbf{d}}$. On a rotated ellipse, transform $\mathbf{d}$ into local
space, support the axis-aligned ellipse, transform back. Circles and ellipses are convex,
so support maps are well-defined.

### Related regions

- **Annulus** — ring between inner and outer radius.
- **Circular sector** — pizza slice (two radii + arc).
- **Circular segment** — region between a chord and its arc.
- **Superellipse** — $|x/a|^n + |y/b|^n \le 1$; $n=2$ is the ellipse.

## In Plato

`17-planar-shapes.plato` names the filled disk `Circle` (doc comment: disk of points
within radius; boundary is the circle proper) and gives a full ellipse region:

```plato
type Circle
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, Centroid2D,
               ContainsPoint2D, NearestPoint2D, SupportMappable2D
{
    Center: Point2D;
    Radius: Number;
}

type Annulus
{
    Center: Point2D;
    InnerRadius: Number;
    OuterRadius: Number;
}

type CircularSector
{
    Circle: Circle;
    Sweep: AngleInterval;
}

type CircularSegment
{
    Circle: Circle;
    Sweep: AngleInterval;
}

type Ellipse
    implements Geometry2D, ClosedShape, ConvexShape, Connected,
               PlanarMeasurable, Bounded2D, Centroid2D, ContainsPoint2D
{
    Center: Point2D;
    SemiAxes: Number2;
    Rotation: Angle;
}

type SuperEllipse
{
    Center: Point2D;
    SemiAxes: Number2;
    Rotation: Angle;
    Exponent: Number;
}
```

`PlanarMeasurable` gives `Area` and `Perimeter`. For `Circle`, both are classical. For
`Ellipse`, `Perimeter` is the hard integral (implementations must approximate). `Circle`
implements `SupportMappable2D` and `NearestPoint2D`; `Ellipse` currently lists containment
and measures but not support or nearest-point interfaces — a surface gap for collision
parity.

Angles in sectors use `AngleInterval` measured from $+X$, counter-clockwise (file
banner). `SemiAxes` is `Number2` — $(a, b)$ — not a bespoke struct.

Usage-shaped sketches:

```plato
let disk = Circle {
    Center: Point2D { X: 0, Y: 0 },
    Radius: 2
};
// Area == 4π; Perimeter == 4π
// Contains(Point2D { X: 1, Y: 1 }) == true
// Support(disk, /* +X */) == (2, 0)

let oval = Ellipse {
    Center: Point2D { X: 0, Y: 0 },
    SemiAxes: /* (3, 1) */,
    Rotation: /* 0 */
};
// Area == 3π
// Perimeter ≈ elliptic integral — not 2π * something simple

let ring = Annulus {
    Center: Point2D { X: 0, Y: 0 },
    InnerRadius: 1,
    OuterRadius: 2
};

let slice = CircularSector {
    Circle: disk,
    Sweep: AngleInterval { Start: /* 0 */, End: /* π/2 */ }
};
```

`Capsule2D` (stadium) is the convex hull of two equal circles — circle tooling plus a
segment — and also implements `SupportMappable2D`.

## Pitfalls / fine print

**Circle vs disk naming.** Plato’s `Circle` type is the filled disk (region). The
boundary alone is rarely a separate type; nearest-point and contains behave like a solid
disk. Read the doc comment before assuming a hollow curve.

**Parametric $t$ ≠ arc length on an ellipse.** Equal steps in $t$ bunch near the
pointy ends of a skinny ellipse. Arc-length parameterization needs numerical work.

**Rotation of axes.** `Ellipse.Rotation` turns the local frame; swapping $a$ and $b$ with
a $90^\circ$ rotation represents the same shape. Canonicalize if you compare ellipses.

**Perimeter accuracy.** Using the circle formula $2\pi \sqrt{(a^2+b^2)/2}$ (RMS) is a
rough approximation only. Do not use it for CNC path length without stating the error.

**Degenerate radii.** $r = 0$ is a point disk; $a = 0$ or $b = 0$ collapses an ellipse to
a segment. Support and contains still have limits; normals do not.

**Annulus non-convex?** The filled ring is not convex — and `Annulus` does not implement
`ConvexShape`, correctly. Do not feed it to GJK.

## Try it

1. Circle radius $5$. Area and circumference?
2. Ellipse $a=5$, $b=5$. What shape is it? Perimeter?
3. Why might `Support` be declared on `Circle` but not on `Ellipse` in the current
   vocabulary, even though both are convex?

<details>
<summary>Answers</summary>

1. Area $25\pi$, circumference $10\pi$.
2. A circle of radius 5; perimeter $10\pi$.
3. Likely an incomplete interface surface — ellipses are convex and have a known support
   map. The gap is a library/declaration omission, not a mathematical one. (Calling that
   out is exactly what recommendations are for.)

</details>

## Library recommendations

- **missing-interface** — `17-planar-shapes.plato`: `Ellipse` implements `ConvexShape` and
  `ContainsPoint2D` but not `SupportMappable2D` or `NearestPoint2D`, while `Circle` has
  both. Collision and closest-point lessons want feature parity on the ellipse.

- **missing-function** — `17-planar-shapes.plato`: no `PointAt(circle|ellipse, Angle)` or
  `Tangent` evaluators. Parameterization is the first thing teachers write on the board;
  it should be a named function once libraries land.

- **doc-comment** — `17-planar-shapes.plato`: `PlanarMeasurable.Perimeter` on `Ellipse`
  should note that the value is non-elementary (elliptic integral) so implementers do not
  ship a silent wrong closed form.

- **naming** — `17-planar-shapes.plato`: type `Circle` denotes a disk region. A brief
  alias note (“filled disk; boundary is the circle proper” is already there — good)
  could additionally warn exporters that map hollow `Circle` curves from other APIs.
