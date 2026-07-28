---
lesson: surfaces-of-revolution
title: Surfaces of Revolution
domain: Curves & surfaces
v3-files: [24-surfaces.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Surfaces of Revolution

A vase on a pottery wheel, a wine glass, a planet in a latitude/longitude globe, a
doughnut: take a plane curve and spin it about an axis. The swept locus is a **surface
of revolution**. CAD "lathe" tools, signed-distance recipes for tori and capsules, and
half of the textbook examples in multivariable calculus are this one construction.

Once you see it, extrusion, ruling, and path sweeps look like siblings — different ways
to drag a profile through space — and many "primitive solids" collapse to a profile plus
a motion.

## The idea

### Revolve

Let a profile curve $\pi(u)$ live in a plane containing the axis (or described so that
one coordinate is radial distance and the other runs along the axis). Revolving about the
axis by angle $v$ yields

$$
S(u, v) = R_v\bigl(\pi(u)\bigr)
$$

where $R_v$ is rotation about the axis. If $v$ covers a full turn, the surface closes in
the angular direction (a vase without a seam cut). A partial sweep leaves a wedge with
two flat radial edges.

```
   axis |        profile          after full revolve
        |       /
        |      /                  . -- .
        |     ●                     (  )
        |    /                     ' -- '
        |   /
```

Classic special cases:

| Profile | Surface |
|---|---|
| Line parallel to axis | Cylinder |
| Line at an angle | Cone |
| Circle offset from axis | Torus |
| Semicircle on the axis | Sphere |

### Extrude

Translate a profile along a straight direction for a fixed distance. U follows the
profile; V follows the extrusion. A polygon extruded becomes a prism; a curve extruded
becomes a generalized cylinder (not necessarily circular).

### Rule, loft, sweep, tube

- **Ruled surface** — straight segments join corresponding parameters on two curves.
- **Loft** — interpolate a stack of cross-sections.
- **Sweep** — carry a 2D profile along a 3D path in a moving frame.
- **Tube** — sweep a circle of fixed radius normal to a path (a pipe).

Revolution is the special sweep whose path is a circular arc about a fixed axis and
whose profile stays in a meridional plane.

## In Plato

File `24-surfaces.plato` declares the generated-surface family under
`ParametricSurface`.

### Surface of revolution

```plato
type SurfaceOfRevolution
    implements ParametricSurface
{
    Profile: Curve2D;
    Axis: Line3D;
    Angles: AngleInterval;
}
```

Doc contract: the profile's **X** is radial distance from the axis; its **Y** runs along
the axis. `Angles` is the sweep — a full turn closes the surface in U (angular
parameter). The axis is an infinite `Line3D` (origin + direction), not merely a segment.

```plato
profile := /* Curve2D: x = radius, y = height */
lathe := SurfaceOfRevolution(
    Profile: profile,
    Axis: Line3D(Origin: Point3D(0,0,0), Direction: Direction3D(0,0,1)),
    Angles: AngleInterval(Start: /* 0 */, End: /* 2*pi */))

p := Eval(lathe, UvCoordinate(U: 0.5, V: 0.25))
ClosedU(lathe)   // true when Angles spans a full turn
ClosedV(lathe)   // typically false — open along the profile
```

(Exact `Angle` constructors depend on quantity APIs; the type is `AngleInterval` of
`Angle`, never raw `Number`.)

### Extrusion and kin

```plato
type ExtrudedSurface
    implements ParametricSurface
{
    Profile: Curve3D;
    Direction: Direction3D;
    Distance: Number;
}

type RuledSurface
    implements ParametricSurface
{
    Start: Curve3D;
    End: Curve3D;
}

type LoftedSurface
    implements ParametricSurface
{
    Sections: Array<Polyline3D>;
    Closed: Boolean;
}

type SweptSurface
    implements ParametricSurface
{
    Profile: Curve2D;
    Path: Curve3D;
}

type TubeSurface
    implements ParametricSurface
{
    Path: Curve3D;
    Radius: Number;
}
```

```plato
wall := ExtrudedSurface(
    Profile: wallCurve,
    Direction: Direction3D(0, 0, 1),
    Distance: 3)

pipe := TubeSurface(Path: centerline, Radius: 0.1)

ribbon := SweptSurface(Profile: crossSection, Path: rail)
```

`SweptSurface` carries the profile in the path's **rotation-minimizing frame** — U along
the profile, V along the path. That choice avoids Frenet-frame twisting on near-straight
rails.

### Shared parametric surface concept

```plato
concept ParametricSurface
    inherits Surface, Procedural<UvCoordinate, Point3D>
{
    ClosedU(x: Self): Boolean;
    ClosedV(x: Self): Boolean;
}
```

All of the generators above evaluate through `Eval(surface, uv)` and report which
parameter directions seam shut.

### Offset and trim

```plato
type OffsetSurface
{
    Base: ParametricSurface;
    Distance: Number;
}

type TrimmedSurface
    implements Surface
{
    Base: ParametricSurface;
    OuterLoop: Polyline2D;
    Holes: Array<Polyline2D>;
}
```

A revolved vase with a hole cut in UV space is a `TrimmedSurface` over a
`SurfaceOfRevolution` base — CAD's everyday representation.

## Pitfalls / fine print

**Profile coordinates.** If you feed a profile whose X is "world X" instead of radius,
the revolve spins the wrong quantity. The doc comment's radial/axial convention is
mandatory, not suggestive.

**Profile crossing the axis.** Negative radii or a profile that crosses the axis create
self-intersections or folded parameterizations. Keep $x \ge 0$ unless you intend a
singular spindle.

**Partial vs full sweep.** `Angles` that do not cover a full turn leave open edges.
`ClosedU` should reflect that; meshes need caps if you want a solid.

**Extrude profile dimension.** `ExtrudedSurface.Profile` is `Curve3D`, while
`SurfaceOfRevolution.Profile` and `SweptSurface.Profile` are `Curve2D`. Mixing them is a
type error — intentional: extrusion translates an already-embedded space curve;
revolution and sweeps expect a planar profile law.

**Ruled surface correspondence.** Rulings join equal parameters on `Start` and `End`.
If the curves are parameterized at different speeds, rulings skew in surprising ways —
reparameterize before ruling when you need geometric correspondence.

**Loft point counts.** `LoftedSurface` requires sections to share point counts. Unequal
polygons need resampling first.

**Tube vs sweep of a circle.** `TubeSurface` is the specialized circular pipe.
`SweptSurface` with a circular profile can match it only if the profile framing matches
the tube's normal-plane convention.

**Solid vs surface.** `SurfaceOfRevolution` is a surface (`ParametricSurface`), not a
filled volume. Do not assume `Volume` / `SpatialMeasurable` queries exist on it — lathe
*solids* with caps are a separate representation.

## Try it

1. Profile is a vertical line segment at $x = R$, $y \in [0,H]$. What surface does a
   full revolve about the Y-axis produce?
2. Same axis, profile a circle in the $xy$-plane centered at $(R,0)$ with radius $r < R$.
   What do you get?
3. Why does `SweptSurface` document a rotation-minimizing frame instead of Frenet?

<details>
<summary>Answers</summary>

1. A right circular cylinder of radius $R$ and height $H$.
2. A torus with major radius $R$ and minor radius $r$.
3. Frenet normals flip at inflections and are undefined on straight spans; RMF keeps the
   swept profile from suddenly twisting — essential for rails and camera-like paths.

</details>

## Library recommendations

- **doc-comment** — `24-surfaces.plato`: `SurfaceOfRevolution` states the profile X/Y
  convention but not which UV map to angle vs profile parameter. Spell out
  "U ↔ angle within `Angles`, V ↔ profile parameter in $[0,1]$" (or the actual choice)
  so `ClosedU`/`ClosedV` are predictable.

- **missing-function** — `24-surfaces.plato`: no helpers to build the classic profiles
  (`Cylinder` as revolve of a line, `Sphere` as revolve of a semicircle, `Torus` as
  revolve of a circle). Teaching the table of special cases wants
  `AsSurfaceOfRevolution(sphere)`-style bridges — or factories on the primitive types.

- **wrong-shape** — `24-surfaces.plato`: `ExtrudedSurface.Profile` is `Curve3D` while
  revolve/sweep profiles are `Curve2D`. The asymmetry is defensible but undocumented as
  a design rule; a banner comment in the generated-surfaces section would prevent
  "why can't I extrude a Curve2D?" confusion.

- **missing-type** — `24-surfaces.plato`: partial revolves often need end-cap disks as
  part of a solid workflow; the surface type has no `Capped` flag. Caps today require a
  separate solid or mesh step — worth a documented companion or flag if lathe UX matters.
