---
lesson: polar-cylindrical-spherical
title: Polar, Cylindrical, and Spherical Coordinates
domain: Coordinate systems & bounds
v3-files: [11-points.plato]
audience: High-school trigonometry and Cartesian $(x,y,z)$; general programming background.
status: draft-v1
---

# Polar, Cylindrical, and Spherical Coordinates

Some problems fight Cartesian coordinates. Motion around a circle wants a radius
and an angle. A spiral staircase wants height plus an angle. Antenna patterns and
planet surface math want a direction on a sphere. In each case the right chart
makes the hard part of the equation disappear — and the wrong chart buries you
in nested square roots.

Plato treats these charts as **distinct types**, not as a comment that says
"these three numbers are spherical today." That stops you from feeding a
cylindrical triple into an API that expected Cartesian $z$-up without a
conversion.

## The idea

### Polar (2D)

A point in the plane as distance from the origin and angle from $+X$:

$$
x = r\cos\theta,\quad y = r\sin\theta
$$

$$
r = \sqrt{x^2+y^2},\quad
\theta = \mathrm{atan2}(y,x)
$$

```
        Y
        |
        |   ● (r, θ)
        |  /
        | / θ
        |/____ X
       O
```

**Use when:** circular motion, radial symmetry, complex-number arguments,
2D steering toward a heading.

**Pain points:** $\theta$ undefined at the origin; $\theta$ jumps across the
branch cut (usually $\pm\pi$) if you unwrap carelessly.

### Cylindrical (3D)

Polar in the $XY$ plane, plus height along $Z$:

$$
x = r\cos\phi,\quad y = r\sin\phi,\quad z = h
$$

```
        Z (height)
        |
        |    ●
        |   /|
        |  / |
        | /  | h
        |/φ  |
        +------ XY plane
```

**Use when:** objects of revolution, pipes, drill paths, anything invariant
under rotation about a fixed axis.

**Pain points:** same polar singularity on the axis $r=0$; "radius" is distance
to the *axis*, not to the origin.

### Spherical (3D)

Distance from the origin, plus two angles. Plato's convention (read the fields):

- `Radius` — distance from origin
- `Azimuth` — angle about $+Z$, measured from $+X$ (same idea as cylindrical
  azimuth)
- `Inclination` — angle from the **$+Z$ pole** (not elevation from the equator)

$$
x = r\sin\iota\cos\phi,\quad
y = r\sin\iota\sin\phi,\quad
z = r\cos\iota
$$

with azimuth $\phi$ and inclination $\iota$.

```
        +Z  (ι = 0 at north pole)
        |
        | ι
        | ·
        |/φ
        +------ XY
```

**Use when:** directions on a sphere, radial falloff from a point, lat/long-like
charts (careful: geographic latitude is elevation from the equator —
complementary to inclination).

**Pain points:** poles make azimuth irrelevant; radius 0 is fully singular;
physics texts disagree on whether the second angle is inclination or elevation.

### Choosing a chart

| Situation | Prefer |
|-----------|--------|
| Spin about an axis + move along it | Cylindrical |
| Distance to a point + direction | Spherical |
| Planar heading + range | Polar |
| Axis-aligned boxes, grids, GPUs | Cartesian `Point2D`/`Point3D` |

Conversion is cheap; clarity is the win. Store the chart that matches the
invariant of your problem, convert at boundaries.

## In Plato

From `11-points.plato`:

```plato
// A planar position as distance from origin and angle from the positive X axis.
type PolarCoordinate
    implements Value
{
    Radius: Number;
    Angle: Angle;
}

// A 3D position as planar polar coordinates plus height along Z.
type CylindricalCoordinate
    implements Value
{
    Radius: Number;
    Azimuth: Angle;
    Height: Number;
}

// A 3D position as radius, azimuth (about Z from +X), and inclination from the
// +Z pole.
type SphericalCoordinate
    implements Value
{
    Radius: Number;
    Azimuth: Angle;
    Inclination: Angle;
}
```

Angles are `Angle`, not `Number` — you cannot silently pass radians as if they
were unitless floats without going through the quantity type.

Cartesian counterparts remain the workhorses:

```plato
type Point2D { X: Number; Y: Number; }
type Point3D { X: Number; Y: Number; Z: Number; }
```

Usage-shaped authoring (building the chart types directly):

```plato
let p = PolarCoordinate {
    Radius: 5.0,
    Angle: (0.7853982).Angle  // 45°
};

let cyl = CylindricalCoordinate {
    Radius: 2.0,
    Azimuth: (1.5707963).Angle,  // 90°
    Height: 10.0
};

let sph = SphericalCoordinate {
    Radius: 1.0,
    Azimuth: (0.0).Angle,
    Inclination: (0.0).Angle
};
// Inclination 0 → on the +Z pole; Azimuth unused
```

**Gap to be honest about:** v3 declares these types but does **not** yet declare
conversion functions such as `Point2D(PolarCoordinate)` or
`SphericalCoordinate(Point3D)` in the points/transforms surface. The formulas
above are the mathematical contract; until conversions land, show the gap rather
than inventing APIs:

```plato
// Not declared in v3 yet — do not pretend this compiles:
// let cartesian = Point3D(sph);
// let back = SphericalCoordinate(cartesian);
```

When conversions appear, they should honor Plato's documented azimuth /
inclination conventions exactly — that is the whole point of typing the chart.

Related: `GeoCoordinate` (latitude, longitude, altitude) is a *geodetic* chart
on an ellipsoid, not the same as `SphericalCoordinate`. Do not cast between them
without a reference ellipsoid model.

## Pitfalls / fine print

**Inclination vs elevation vs polar angle.** Plato: inclination from $+Z$. Many
graphics APIs use elevation from $XY$ or polar angle with different zero points.
Read field docs; do not trust the word "phi" alone.

**Azimuth origin and direction.** Plato: from $+X$ about $+Z$. Some CAD systems
measure from $+Y$. Off-by-$90°$ bugs are rampant in imports.

**Negative radii.** A signed radius can encode a point reflection through the
origin, but it doubles representations. Prefer $r \ge 0$ and fold signs into
angles unless you have a domain reason otherwise.

**Unwrapping angles for animation.** Interpolating `Angle` across a branch cut
spins the long way. Convert to a continuous heading channel or use careful
unwrap before blending cylindrical/spherical paths.

**Using spherical radius as cylindrical radius.** Spherical $r$ is distance to
the origin; cylindrical $r$ is distance to the $Z$ axis. Mixing them breaks
energy calculations and collision radii.

**Origin singularities.** At $r=0$ (polar/spherical) or on the axis
(cylindrical/spherical poles), angle components are not unique. Pick a
convention when converting from Cartesian (e.g. azimuth $= 0$).

## Try it

1. Polar $(r,\theta) = (2, 90°)$. What Cartesian `(X,Y)` results?
2. Cylindrical $(r,\phi,h) = (0, 123°, 5)$. Why is azimuth irrelevant?
3. Spherical inclination $= 0$, radius $= 3$. Where is the point, and what
   happens if you change azimuth?

<details>
<summary>Answers</summary>

1. $(0, 2)$ — on the positive $Y$ axis.
2. Radius $0$ means the point sits on the $Z$ axis at height $5$; every azimuth
   describes the same place.
3. At $(0,0,3)$, the $+Z$ pole. Changing azimuth leaves the Cartesian point
   unchanged.

</details>

## Library recommendations

- **missing-function** — `11-points.plato` / `13-transforms.plato`: no
  `Point2D(PolarCoordinate)` / `PolarCoordinate(Point2D)` pair (and the 3D
  cylindrical/spherical analogs). The types exist; without conversions the
  chart types cannot participate in the point pipeline this lesson describes.

- **missing-function** — `11-points.plato`: no
  `IsSingular(p: PolarCoordinate): Boolean` (radius near 0) or spherical
  `IsNearPole`. Callers need a shared policy for the undefined-angle cases.

- **doc-comment** — `11-points.plato`: `SphericalCoordinate.Inclination` should
  explicitly contrast with geographic latitude / elevation-from-equator. One
  sentence would prevent the most common convention bug the lesson warns about.

- **naming** — `11-points.plato`: `PolarCoordinate.Angle` vs
  `CylindricalCoordinate.Azimuth` vs `SphericalCoordinate.Azimuth` — the 2D
  field is the odd name. Renaming to `Azimuth` (or documenting synonymy) would
  make the polar↔cylindrical relationship obvious.
