---
lesson: cameras-and-projection
title: Cameras and Projection
domain: Rendering
v3-files: [48-cameras.plato]
audience: Comfortable with 3D points, matrices at a high level, and the idea of a view frustum.
status: draft-v1
---

# Cameras and Projection

A renderer’s job, reduced to one sentence: take a point in the scene and decide which
pixel it paints — and whether it is visible at all. A **camera** answers the geometric
half of that question. It places an eye in the world and defines how rays (or
half-spaces) map 3D points onto an image plane.

Two families dominate real-time graphics: **perspective** (things farther away look
smaller) and **orthographic** (parallel lines stay parallel; used in CAD and 2D-ish
games). Everything else — fisheye, panoramas, off-axis CAVE walls — is a variation on
the same pipeline.

## The idea

### The point-to-pixel pipeline

For a pinhole perspective camera:

1. **World → view (eye) space.** Rigid transform from the camera pose: place the eye at
   the origin, looking down the local forward axis.
2. **View → clip space.** Projective transform encoding field of view, aspect ratio, and
   near/far planes. After this multiply, points are homogeneous 4-vectors.
3. **Perspective divide.** $(x,y,z,w) \mapsto (x/w,\, y/w,\, z/w)$ yields normalized
   device coordinates (NDC), typically in cubes like $[-1,1]^3$ or $[0,1]$ depth.
4. **Viewport.** Map NDC $x,y$ into pixel coordinates; map depth into the depth buffer
   range.

```
  world p  --view-->  eye  --proj-->  clip  --÷w-->  ndc  --viewport-->  pixel
```

### Perspective vs orthographic

Perspective scales $x,y$ by a factor that depends on depth (the classic $x' = f\,x/z$
pinhole). Orthographic ignores that foreshortening: the view volume is a box, not a
pyramid.

```
 perspective              orthographic
      \  |  /               |      |
       \ | /                |      |
        \|/                 |      |
         * eye              | eye  |
```

### Field of view

Vertical FOV $\theta$ and aspect $a = \mathrm{width}/\mathrm{height}$ fix the horizontal
FOV. Larger $\theta$ means a wider pyramid and more perspective distortion at the edges.
Near and far clip planes cut the pyramid into a frustum; geometry outside is clipped.
Invariant: $0 < n < f$.

## In Plato

### Shared camera face

```
interface Camera
{
    Pose(x: Self): Pose3D;
    Near(x: Self): Number;
    Far(x: Self): Number;
}
```

`Pose3D` is eye position plus orientation; the view direction is the pose’s forward axis.
Distances `Near`/`Far` are unit-agnostic scene numbers.

### Perspective and orthographic

```
type PerspectiveCamera
{
    Pose: Pose3D;
    VerticalFov: Angle;
    AspectRatio: Number;
    Near: Number;
    Far: Number;
}

type OrthographicCamera
{
    Pose: Pose3D;
    OrthoHeight: Number;      // vertical extent in scene units
    AspectRatio: Number;
    Near: Number;
    Far: Number;
}
```

Usage-shaped construction (vertical FOV is an `Angle`, never a raw `Number`):

```
fov60: Angle = …                 // π/3 radians; no bare 60
cam = PerspectiveCamera(pose, fov60, 16/9, 0.1, 1000)

ortho = OrthographicCamera(pose, 10, 1, 0.1, 100)
```

### Authoring helpers

```
type LookAtCamera
{
    Position: Point3D;
    Target: Point3D;
    Up: Direction3D;
    VerticalFov: Angle;
    AspectRatio: Number;
    Near: Number;
    Far: Number;
}
```

`LookAtCamera` is the human way to aim; it does **not** implement `Camera` in v3 — it is
an authoring record meant to be converted into a `PerspectiveCamera` / pose.

Physical photography parameterization:

```
type PhysicalCamera
{
    Pose: Pose3D;
    FocalLength: Length;       // 50mm lens → 0.05 m
    SensorSize: Size2D;        // millimeters by photo convention
    FStop: Number;
    FocusDistance: Length;
    Iso: Number;
    ShutterTime: Duration;
    Near: Number;
    Far: Number;
}
```

FOV follows from focal length and sensor size; exposure from $N$, shutter, and ISO.

### Special projections

```
type CameraProjection = Perspective | Orthographic | Fisheye | Equirectangular | Panoramic;

type FisheyeCamera { … Mapping: FisheyeMapping; FieldOfView: Angle; … }
type EquirectangularCamera { … LongitudeSweep, LatitudeSweep: AngleInterval; … }
type PanoramicCamera { … HorizontalSweep: Angle; VerticalFov: Angle; … }
```

Off-axis / CAVE:

```
type ProjectionPlane { LowerLeft, LowerRight, UpperLeft: Point3D; }
type OffAxisCamera { Eye: Point3D; Screen: ProjectionPlane; Near, Far: Number; }
```

Stereo:

```
type StereoCameraRig
{
    Template: PerspectiveCamera;
    EyeSeparation: Length;           // ~0.063 m human
    ConvergenceDistance: Length;     // zero-parallax plane
}
```

### Output region and optics extras

```
type Viewport { PixelBounds: IntegerBounds2D; DepthRange: NumberInterval; }
type LensDistortion { K1, K2, K3, P1, P2: Number; }   // Brown–Conrady
type LensShift { Horizontal, Vertical: Number; }
type DepthOfFieldSettings { Enabled: Boolean; FocusDistance: Length; FStop: Number; … }
```

## Worked micro-example

Eye at the origin, looking down $-Z$, vertical FOV $90°$, aspect $1$, near $1$, far
$100$. A point on the optical axis at $(0,0,-2)$ in eye space projects to the image
center. A point $(1,0,-2)$ sits on the right edge of the NDC square when the horizontal
extent at $z=-2$ matches the frustum half-width (for $90°$ vertical FOV and aspect 1,
half-width at distance $d$ equals $d$ — so $x=1$ at $z=-2$ is near the fringe). The
exact matrix form varies by API (OpenGL vs DirectX depth), but the geometric story —
divide by depth after the projective multiply — does not.

## Pitfalls / fine print

**Near plane too close.** Depth precision concentrates near the eye; an excessively small
`Near` destroys far-depth resolution (fighting in the depth buffer).

**FOV vs focal length.** Matching a real lens requires sensor size; a lone FOV number
cannot recover aperture bokeh.

**LookAt roll.** If `Up` is parallel to the view axis, the basis is undefined. Keep a
sensible world up and handle zenith singularities.

**Infinite far planes.** Some engines use reverse-Z and infinite projections; v3’s
`Camera` still requires finite `Far`. Model that as a large Far or extend the vocabulary
later.

**OffAxisCamera / LookAtCamera vs Camera.** Not every camera-shaped record implements the
`Camera` interface. Code that only accepts `Camera` must convert first.

**Viewport depth range.** Default $[0,1]$ vs $[-1,1]$ is API-specific; `DepthRange` makes
the choice explicit.

## Try it

<details>
<summary>Exercise 1 — Ortho width</summary>

`OrthographicCamera` with `OrthoHeight: 10` and `AspectRatio: 2`. What is the horizontal
extent of the view volume?

**Answer.** $20$ scene units — width = height × aspect.
</details>

<details>
<summary>Exercise 2 — Clip invariant</summary>

Is `Near: 5, Far: 5` valid?

**Answer.** No — the invariant is $0 < \mathrm{Near} < \mathrm{Far}$.
</details>

<details>
<summary>Exercise 3 — Stereo offset</summary>

`EyeSeparation` is $0.06$ m. How far from the rig center is each eye along the right
axis?

**Answer.** $0.03$ m — half the separation each way.
</details>

## Library recommendations

- **missing-function** — `48-cameras.plato`: no declared
  `ViewMatrix(camera)`, `ProjectionMatrix(camera)`, `Project(camera, point) → …`, or
  `WorldRay(camera, uv)`. The lesson’s pipeline is universal; without these operations
  the types are inert records.

- **wrong-shape** — `48-cameras.plato`: `LookAtCamera` and `OffAxisCamera` do not
  implement `Camera` despite carrying near/far and producing a view. Either implement the
  interface (Pose derived from look-at / eye+screen) or document them as *builders* with a
  `ToPerspectiveCamera` / `ToCamera` conversion in the type comment.

- **missing-function** — `48-cameras.plato`: `PhysicalCamera` docs say FOV follows from
  focal length and sensor size, but no
  `VerticalFov(PhysicalCamera): Angle` is declared. Teachers and renderers both need that
  bridge to compare with `PerspectiveCamera`.

- **doc-comment** — `48-cameras.plato`: state the assumed view-space convention
  (handedness, which axis is forward, whether Y is up in view space) on the `Camera`
  interface. Projection matrices are meaningless without it, and v3 currently leaves the
  convention implicit in "the pose's forward axis."
