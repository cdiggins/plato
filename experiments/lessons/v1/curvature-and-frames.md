---
lesson: curvature-and-frames
title: Curvature and Frames
domain: Curves & surfaces
v3-files: [20-interfaces-curves-surfaces.plato, 22-curves-3d.plato, 64-differential-geometry.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Curvature and Frames

Lay a ribbon along a space curve — a camera path, a road centerline, a sweep rail. At
every sample you need more than a point: you need an orientation. Which way is "up"?
Which way does the ribbon twist? **Curvature** says how sharply the path bends;
a **moving frame** says how to attach axes to the path so geometry can ride along it.

The Frenet–Serret frame is the mathematically famous answer. It is also famously
unstable wherever the curve goes straight or passes through an inflection. Production
sweeps usually prefer a **rotation-minimizing frame** that refuses to spin without
cause.

## The idea

### Curvature in the plane

For a smooth plane curve $\gamma(t)$, with velocity $v = \gamma'$ and acceleration $a$,

$$
\kappa = \frac{v_x a_y - v_y a_x}{\|v\|^3}
$$

Signed curvature is positive when the curve turns left (toward the left normal). The
**osculating circle** has radius $1/|\kappa|$ and matches position and first two
derivatives at the contact point.

### Curvature and torsion in space

In 3D, curvature is nonnegative:

$$
\kappa = \frac{\|\gamma' \times \gamma''\|}{\|\gamma'\|^3}
$$

**Torsion** $\tau$ measures how fast the osculating plane rotates about the tangent —
the curve's out-of-plane corkscrewing. A planar space curve has $\tau = 0$. A circular
helix has constant $\kappa$ and constant $\tau$.

### The Frenet–Serret frame

Where $\kappa > 0$, define

- **T** — unit tangent $\gamma' / \|\gamma'\|$
- **N** — unit principal normal, direction of $T'$ (bend direction)
- **B** — binormal $T \times N$

The Frenet–Serret equations relate their derivatives to $\kappa$ and $\tau$. Beautiful —
and undefined or discontinuous when $\kappa = 0$ (straight segments, inflections). There
the normal flips or jumps, and anything built on Frenet (a swept cross-section, a camera
roll) flickers.

```
        N
        ↑
        ●----→ T
       /
      B (out of page)
```

### Rotation-minimizing frames

A **rotation-minimizing frame** (RMF, parallel-transport frame) keeps the tangent
aligned with the curve but advances the normal with as little rotation about $T$ as
possible. It stays well-defined through inflections and straight runs. Sweeps, loft
rails, and camera paths almost always want RMF, not Frenet.

## In Plato

### Curve capabilities (`20-interfaces-curves-surfaces.plato`)

```plato
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

`CurvatureAt` on a 3D curve is unsigned space curvature; on
`DifferentiableCurve2D` it is signed plane curvature. `FrameAt` returns a `Frame3D`
whose origin lies on the curve and whose Z axis is tangent — but the interface does not
name Frenet vs RMF (see recommendations).

### A concrete curve (`22-curves-3d.plato`)

The circular helix is the textbook constant-curvature, constant-torsion example:

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

For radius $R$ and pitch $P$ (rise per turn), curvature and torsion are constant and
related to $R$ and $P$ by standard formulas — a perfect probe for `CurvatureAt` /
`TorsionAt` once libraries implement them. Other analytic curves in the same file
(`CircularArc3D`, `ConicalSpiral3D`, knots) stress different curvature profiles.

### Differential-geometry records (`64-differential-geometry.plato`)

```plato
type FrenetFrame3D
    implements Value
{
    Position: Point3D;
    Tangent: Direction3D;
    Normal: Direction3D;
    Binormal: Direction3D;
    Curvature: Number;
    Torsion: Number;
}

type RotationMinimizingFrame3D
    implements Value
{
    Position: Point3D;
    Tangent: Direction3D;
    Normal: Direction3D;
    Binormal: Direction3D;
    Parameter: Number;
}

type FrenetFrame2D
    implements Value
{
    Position: Point2D;
    Tangent: Direction2D;
    Normal: Direction2D;
    Curvature: Number;
}
```

Usage-shaped sketches:

```plato
ff := /* sample Frenet at t */
// ff.Curvature, ff.Torsion, ff.Normal

rmf := /* sample RMF at t */
// rmf.Parameter records where on the curve
```

Derivative jets feed frame construction:

```plato
type CurveJet3D
    implements Value
{
    Parameter: Number;
    Position: Point3D;
    FirstDerivative: Vector3D;
    SecondDerivative: Vector3D;
    ThirdDerivative: Vector3D;
}
```

Third order is enough for torsion. Plane curves use `CurveJet2D` (second order). The
osculating circle and curvature comb are visualization aids:

```plato
type OsculatingCircle2D
{
    Center: Point2D;
    Radius: Number;
    ContactPoint: Point2D;
}

type CurvatureComb2D
{
    CurvePoints: Array<Point2D>;
    CombTips: Array<Point2D>;
    Scale: Number;
}
```

When the curve lies on a surface, the **Darboux frame** mixes curve and surface normals:

```plato
type DarbouxFrame3D
{
    Position: Point3D;
    Tangent: Direction3D;
    SurfaceNormal: Direction3D;
    TangentNormal: Direction3D;
    NormalCurvature: Number;
    GeodesicCurvature: Number;
    GeodesicTorsion: Number;
}
```

### Surface curvature (same file)

Surfaces carry their own curvature vocabulary — principal curvatures, Gaussian and mean
curvature — related but distinct from curve curvature:

```plato
type SurfaceCurvature
{
    Gaussian: Number;
    Mean: Number;
    Principal: PrincipalCurvatures;
}

type SurfacePointShape
    = Elliptic | Parabolic | Hyperbolic | Planar | Umbilic;
```

## Pitfalls / fine print

**Frenet at zero curvature.** Doc comments on `FrenetFrame3D` say it is undefined where
curvature vanishes. Do not sample Frenet on polylines with straight spans or on
S-curves at the inflection.

**Signed vs unsigned.** Plane `CurvatureAt` / `FrenetFrame2D.Curvature` are signed.
Space `CurvatureAt` / `FrenetFrame3D.Curvature` are nonnegative. Mixing the conventions
flips formulas that assume a sign.

**Tangent from `TangentAt` is not unit.** Normalize before building frames. Division by
near-zero speed is the cusp hazard.

**RMF still needs an initial normal.** Parallel transport is relative — pick a starting
normal at $t=0$, then transport. A bad initial choice rolls the whole sweep.

**Frame Z = tangent.** Plato's `FramedCurve3D` uses Z as the tangent axis. Graphics code
that assumes Y-up along the path must remap axes when consuming `Frame3D`.

**Comb scale units.** `CurvatureComb2D.Scale` maps reciprocal-world-unit curvature to a
world-unit comb height (squared world units). Wrong scale makes combs invisible or
enormous.

**Helix pitch sign.** Positive pitch is right-handed rise along the frame's Z axis
(per `Helix` docs). Handedness of torsion follows the same orientation.

## Try it

1. A straight line segment. What is $\kappa$? Is `FrenetFrame3D` defined along it?
2. Why does `RotationMinimizingFrame3D` store `Parameter` while `FrenetFrame3D` does not?
3. A plane circle of radius $R$. What is $|\kappa|$, and what is the osculating circle?

<details>
<summary>Answers</summary>

1. $\kappa = 0$. Frenet is undefined (no unique principal normal).
2. RMF samples are often computed in a sequence along the curve; `Parameter` tags where
   each sample sits. Frenet is a local differential quantity that does not need a
   transport history — though recording $t$ would still be useful (and is a library gap).
3. $|\kappa| = 1/R$; the osculating circle *is* the circle itself (center and radius
   match).

</details>

## Library recommendations

- **doc-comment** / **pedagogy** — `20-interfaces-curves-surfaces.plato`:
  `FramedCurve3D.FrameAt` does not specify Frenet vs rotation-minimizing. Split into
  `FrenetFrameAt` / `RmfFrameAt`, or document the choice — sweeps depend on it.

- **missing-function** — `64-differential-geometry.plato`: rich frame *types* exist, but
  no interface functions like `FrenetAt(curve, t): FrenetFrame3D` or
  `RmfAt(curve, t0, t1, ...)` are declared on curve interfaces. The records are orphaned
  from `DifferentiableCurve3D` until libraries invent the glue.

- **missing-function** — `64-differential-geometry.plato`: `FrenetFrame3D` has no
  `Parameter` field (unlike RMF). Adding it would make batched frame arrays align for
  debugging and visualization.

- **naming** — `20-interfaces-curves-surfaces.plato`: `CurvatureAt` means signed in 2D and
  unsigned in 3D under the same function name. `SignedCurvatureAt` / `CurvatureAt` split
  (or a doc banner on each interface) would prevent formula mix-ups.
