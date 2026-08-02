---
lesson: extrusion-along-path
title: Extrusion Along a Path
domain: Curves & surfaces
v3-files: [24-surfaces.plato, 40-paths.plato, 20-interfaces-curves-surfaces.plato]
audience: Comfortable with parametric curves (a function of t) and 2D outlines; no CAD background assumed
status: draft-v1
---

# Extrusion Along a Path

Push a cookie-cutter outline through space and you get a solid or a surface:
a pipe, a railing, a extruded logo, a tunnel. The outline is the **profile**.
The trajectory it follows is the **path**. When the path is a straight line,
everyone calls it extrusion. When the path bends, graphics people say **sweep**;
CAD people still often say “extrude along path.” Same geometric idea, stricter
bookkeeping once the path curves.

## The idea

### Three cousins

1. **Linear extrusion.** Translate a profile along a fixed direction by a
   distance. Every cross-section is a congruent copy; rulings are straight and
   parallel.
2. **Sweep.** Carry a profile along an arbitrary space curve, rotating it so it
   stays in the path’s local frame (usually a rotation-minimizing frame to
   avoid Frenet torsion spikes).
3. **Tube.** Special sweep whose profile is a circle of fixed radius — a pipe
   around the path.

```
  profile          linear extrude           sweep along curve
     ___                ___                    __
    /   \              /   \                 /   \___
    \___/              \___/  ---->         \___/    \___
                         |                              path
                         v direction
```

Parameterization: $U$ runs around (or along) the profile; $V$ advances along
the extrusion or path. That UV rectangle is a `ParametricSurface`.

### Why the frame matters

At each path parameter $v$, you need a basis: tangent $T$, plus two normals
$N, B$ that hold the 2D profile. Frenet–Serret frames flip where curvature is
near zero. **Rotation-minimizing frames** (RMF) keep twist minimal — v3’s
`SweptSurface` doc comment commits to that choice.

### 2D vector paths vs geometric curves

UI and font stacks describe outlines as **path verbs** (move, line, cubic,
arc, close) — SVG/`Path2D`. Geometry stacks want a `Curve2D` you can
`Eval` at $t\in[0,1]$. Bridging them means flattening or converting segments
into a continuous curve (or a polyline approximation within a tolerance).

## In Plato

Linear and curved extrusions are separate surface types. 2D outline exchange
uses `Path2D`.

From `24-surfaces.plato`:

```plato
// The surface swept by translating a profile curve along a straight direction
// for a distance: U follows the profile, V the extrusion.
type ExtrudedSurface
    implements ParametricSurface
{
    Profile: Curve3D;
    Direction: Direction3D;
    Distance: Number;
}

// The surface swept by a 2D profile carried along a 3D path in the path's
// rotation-minimizing frame: U follows the profile, V the path.
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

type RuledSurface
    implements ParametricSurface
{
    Start: Curve3D;
    End: Curve3D;
}
```

From `40-paths.plato`:

```plato
type PathSegment2D
    implements Value
    = Move(EndPoint: Point2D)
    | Line(EndPoint: Point2D)
    | Quadratic(Control: Point2D, EndPoint: Point2D)
    | Cubic(Control1: Point2D, Control2: Point2D, EndPoint: Point2D)
    | Arc(Radii: Number2, AxisRotation: Angle, LargeArc: Boolean, Sweep: Boolean, EndPoint: Point2D)
    | Close;

type Contour2D
    implements Value
{
    Segments: Array<PathSegment2D>;
    Closed: Boolean;
}

type Path2D
    implements Value
{
    Contours: Array<Contour2D>;
    FillRule: FillRule;
}

type PathFlattenParameters
    implements Value
{
    Tolerance: Number;
    MaxSegmentCount: Integer;
}

interface PathLike
{
    ToPath(x: Self): Path2D;
}
```

Usage-shaped sketches:

```plato
// Logo outline as an SVG-style path (2D), then — eventually — a Curve2D profile.
logo = Path2D {
    Contours: [outline];
    FillRule: NonZero;
}

flatten = PathFlattenParameters {
    Tolerance: 0.01;
    MaxSegmentCount: -1;
}

// Straight prism: profile already a space curve in its plane
prism = ExtrudedSurface {
    Profile: profile3D;              // Curve3D
    Direction: Direction3D {
        Vector: Vector3D { X: 0.0; Y: 0.0; Z: 1.0 };
    };
    Distance: 5.0;
}

// Banister: 2D section carried along a 3D rail
rail = SweptSurface {
    Profile: section2D;              // Curve2D in the profile plane
    Path: centerline;                // Curve3D
}

pipe = TubeSurface {
    Path: centerline;
    Radius: 0.05;
}
```

**Important asymmetry:** `ExtrudedSurface.Profile` is `Curve3D`, while
`SweptSurface.Profile` is `Curve2D`. Linear extrusion wants the profile already
embedded in space; sweep builds the embedding from the path frame. `Path2D` is
neither — it is an outline exchange type via `PathLike.ToPath`.

v3 does **not** declare a conversion `Path2D → Curve2D` (or → `Polyline2D`).
Flattening parameters exist so a library *could* approximate; the operation is
not named yet. Likewise there is no `Extrude(Path2D, distance)` convenience —
you compose types by hand.

`RuledSurface` is the “straight lines between two curves” cousin: if those
curves are parallel translates of one profile, you recover a linear extrude;
if not, you get a more general ruled patch.

## Pitfalls / fine print

- **Open vs closed profile.** A closed profile sweep can bound a solid; an open
  profile is only a ribbon. `Path2D` contours may set `Closed: true` or end with
  `Close` — geometry curves use `ClosedCurve2D` as a marker interface instead.
- **Self-intersection.** Tight path curvature with a large profile (or tube
  radius) folds the surface through itself. Tube radius must stay under the
  path’s local radius of curvature for a simple pipe.
- **Scale along the path.** Classic sweep keeps profile size constant in the
  local frame. “Draft angles” or varying radius need a different generator
  (not declared as a typed sweep-with-law in v3).
- **`Distance: Number` on `ExtrudedSurface`.** Not a `Length` quantity — same
  unitless geometry convention as much of the curves stack.
- **FillRule is for 2D painting**, not for 3D sweeping. Multi-contour `Path2D`
  holes do not automatically become hollow pipes.
- **Name collision:** `PathFlattenParameters.Tolerance` is a bare `Number`
  (max deviation), unrelated to the engineering `Tolerance` type in uncertainty.

## Try it

1. You want a rectangular duct along a curved hallway centerline. Which type —
   `ExtrudedSurface`, `SweptSurface`, or `TubeSurface` — matches, and what are
   the profile/path types?
2. Why is a Frenet frame a bad default for a nearly straight path with one
   gentle bend in the middle?
3. You have a font glyph as `Path2D` and want embossed lettering
   (`ExtrudedSurface`). What vocabulary gap blocks a one-liner?

<details>
<summary>Answers</summary>

1. `SweptSurface` with a rectangular `Curve2D` profile and the hallway
   `Curve3D` path. `TubeSurface` forces a circular section;
   `ExtrudedSurface` cannot follow a bent path.
2. Frenet normals are undefined or discontinuous where curvature ≈ 0, so the
   profile can suddenly spin when the path straightens. RMF avoids that spin.
3. No declared `Path2D → Curve3D` (or planar `Curve2D` + plane placement) and
   no helper that builds `ExtrudedSurface` from a path outline. You must
   invent the conversion outside the vocabulary.

</details>

## Library recommendations

- **missing-function** — `40-paths.plato` → `24-surfaces.plato`: no
  `ToCurve(path: Path2D, flatten: PathFlattenParameters): Curve2D` (or
  `Polyline2D`) conversion. Extrusion-along-path teaching always starts from
  SVG-like outlines and then needs a sweep profile.
- **naming** — `24-surfaces.plato`: `ExtrudedSurface` vs `SweptSurface` is
  correct CAD usage, but many users search for “extrude along path” and will
  miss `SweptSurface`. A doc-comment alias note (“also called extrude-along-path”)
  on `SweptSurface` would save a lot of wrong type choices.
- **wrong-shape** — `24-surfaces.plato`: `ExtrudedSurface.Profile: Curve3D`
  vs `SweptSurface.Profile: Curve2D` is teachable but surprising. Consider a
  shared `Profile2D` + explicit `Plane`/`Frame3D` for linear extrude so both
  generators consume the same outline type.
- **missing-type** — `24-surfaces.plato`: no sweep with a radius/scale law along
  $V$ (tapered pipes, draft). `TubeSurface` is constant `Radius` only;
  terrain-style lessons invent ad-hoc workarounds.
- **missing-interface** — `20-interfaces-curves-surfaces.plato` / `24-surfaces.plato`:
  `FramedCurve3D.FrameAt` is the right primitive under sweeps, but
  `SweptSurface` does not require `Path` to implement `FramedCurve3D` in the
  type declaration — only in the doc comment’s RMF promise. Encoding that as
  an interface constraint would make the lesson’s frame discussion type-checkable.
