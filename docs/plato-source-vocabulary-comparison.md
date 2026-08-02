# Plato source vocabulary comparison

**Date:** 2026-07-27  
**Old source:** [`submodules/Plato/stdlib-legacy/`](../submodules/Plato/stdlib-legacy/)  
**New source:** [`submodules/Plato/plato-src-v2/`](../submodules/Plato/plato-src-v2/)  
**New-source commit:** `8a252a4`

## Executive summary

`plato-src-v2` is not a drop-in revision of `stdlib-legacy`. It is a broad,
declaration-only vocabulary prototype.

The old library combines a relatively compact set of data types and interfaces with
25 libraries containing constants, conversions, queries, and algorithms. It is
strongest in executable 2D/3D geometry: vectors, transforms, analytic curves, solids,
meshes, fields, and signed-distance operations.

The new set contains no libraries or function bodies. Instead, it expands the modeled
domain from 168 concrete types to 606 and separates the vocabulary into 41 focused
files. It adds explicit 4D and dimension-generic types, physical quantities, animation,
imaging, rendering, physics, engineering, statistics, optimization, signals,
uncertainty, scientific data, differential geometry, and computational-result types.

The central tradeoff is therefore:

> The old library is narrow enough to execute; the new library is broad enough to
> describe.

The next phase should not replace the old source wholesale. It should first stabilize
a smaller foundational kernel, define migration mappings, and selectively port
behavior from the old libraries.

## Quantitative comparison

Counts below are derived directly from top-level `type`, `interface`, and `library`
declarations in the two source folders.

| Metric | Old `stdlib-legacy` | New `plato-src-v2` | Difference |
|---|---:|---:|---:|
| `.plato` files | 28 | 41 | +13 |
| Physical source lines | 4,478 | 1,432 | −3,046 |
| Concrete types | 168 | 606 | +438 |
| Interfaces/interfaces | 81 | 66 | −15 |
| Libraries | 25 | 0 | −25 |
| Type + interface declarations | 249 | 672 | +423 |

The new set has about 3.6 times as many concrete types in less than one third of the
source lines because it contains structure but no implementations.

### Name continuity

| Relationship | Count |
|---|---:|
| Exact type/interface names shared by both sets | 73 |
| Names retained with casing changes | 4 |
| Old declarations without an exact-name counterpart | 176 |
| New declarations without an exact-name predecessor | 599 |

The four casing-only changes are:

| Old | New |
|---|---|
| `ByteRGB` | `ByteRgb` |
| `ByteRGBA` | `ByteRgba` |
| `PolyLine2D` | `Polyline2D` |
| `PolyLine3D` | `Polyline3D` |

Only 29% of the old declarations retain an exact name, and those matches do not imply
signature or semantic compatibility.

## Architectural contrast

| Concern | Old set | New set |
|---|---|---|
| Primary goal | Executable geometry standard library | Cross-domain type vocabulary |
| Domain center | 2D/3D geometry and numerics | Geometry plus animation, science, graphics, and engineering |
| Behavior | 25 libraries with function bodies | No libraries or function bodies |
| Organization | Some large mixed files, notably curves and transforms | One focused topic per numbered file |
| Dimensionality | Primarily 2D/3D; selected `Vector4`/`Vector8` support | Explicit 1D/2D/3D/4D plus dimension-generic `N` types |
| Interfaces | Deep, geometry-specific capability hierarchy | Smaller generic capabilities composed across domains |
| Units | `Angle`, `Time`, and general `IMeasure` | Explicit quantities, SI dimensions, units, instants, and durations |
| Geometry naming | Several dimension-implicit names | Dimension-explicit names such as `Circle2D`, `Plane3D`, and `Sphere3D` |
| Partial results | Mostly direct values and booleans | Named result and diagnostic records |
| Backend readiness | Proven through the existing generated library | Parser/resolver/codegen proven; runtime semantics unimplemented |

## Core semantic changes

### Points are no longer vectors

In the old set, `Point2D` and `Point3D` implement `IVectorLike`. This gives points
scalar arithmetic and interpolation through a broad numerical hierarchy. They also
implement `IDifference<Vector2>` or `IDifference<Vector3>`, correctly acknowledging
that point minus point produces a vector.

The new points implement geometry and metric interfaces instead:

- `Point2D implements IGeometry2D, IMetric<Number>`
- `Point3D implements IGeometry3D, IMetric<Number>`
- `Point4D` and `PointN` extend the same distinction

This removes the most questionable implication—component-wise point arithmetic—but
also drops the useful typed difference contract. A stable v2 kernel should restore
point/vector affine-space operations with a narrower interface rather than making points
full vectors again.

### Bounded and unbounded linear geometry is explicit

The old names conflate mathematical lines and bounded segments:

- `Line2D` and `Line3D` are actually endpoint pairs.
- `Plane` is unbounded, but its dimensionality is implicit.
- Rays and lines do not participate in a systematic boundedness model.

The new vocabulary separates:

- `Segment2D`, `Segment3D`, `Segment4D`, and `SegmentN`
- `Ray2D`, `Ray3D`, `Ray4D`, and `RayN`
- `InfiniteLine2D`, `InfiniteLine3D`, `InfiniteLine4D`, and `InfiniteLineN`
- `Plane2D`, `Plane3D`, `Hyperplane4D`, and `HyperplaneN`
- `HalfPlane2D`, `HalfSpace3D`, `HalfSpace4D`, and `HalfSpaceN`
- `IBounded<TBounds>`

This is one of the clearest improvements because illegal interpretation is reduced by
the type names themselves.

### Dimension is more consistently encoded

Several old names rely on geometry context:

| Old | New direction |
|---|---|
| `Circle` | `Circle2D` |
| `Ellipse` | `Ellipse2D` |
| `Sphere` | `Sphere3D` |
| `Box` | `Box3D` |
| `Cylinder` | `Cylinder3D` |
| `Plane` | `Plane3D` |
| `Rect2D` | `Rectangle2D` |
| `Polygon` | `Polygon2D` |

The new set also supplies 4D and dimension-generic counterparts where they are
meaningful: `Point4D`, `PointN`, `Bounds4D`, `BoundsN`, `VectorN`, `Matrix`,
`AffineTransform4D`, `AffineTransformN`, `Polytope4D`, and `PolytopeN`.

The benefit is discoverability and safer cross-dimensional code. The cost is more
verbose names for the dominant 2D/3D cases.

### Generic interfaces replace parallel dimensional interfaces

The old set often duplicates interfaces by dimension:

- `IDistanceField2D` / `IDistanceField3D`
- `ITransformable2D` / `ITransformable3D`
- `IDeformable2D` / `IDeformable3D`
- `IPointGeometry2D` / `IPointGeometry3D`
- `IPrimitiveGeometry2D<T>` / `IPrimitiveGeometry3D<T>`

The new set introduces reusable generic forms:

- `IDistanceField<TPoint, TDistance>`
- `ITransform<TPoint>`
- `ITransformable<TTransform>`
- `IDeformable<TPoint>`
- `IField<TDomain, TValue>`
- `IParametric<TParameter, TPoint>`
- `IDiscreteGeometry<TPoint>`
- `IIndexedGeometry<TPoint, TIndex>`

Dimension-specific curve and geometry markers remain for ergonomic constraints. This
hybrid approach is more composable than either a wholly generic or wholly duplicated
hierarchy.

### Time separates instants from durations

The old `Time` type represents a single measure without stating whether it is an
absolute position on a timeline or an elapsed amount.

The new set distinguishes:

- `Instant`
- `Duration`
- `TimeInterval`
- `FrameRate`
- `FrameTime`
- `Timecode`
- `Date`, `TimeOfDay`, and `DateTime`

This prevents common invalid operations such as adding two absolute instants or using a
timestamp where an animation duration is required.

### Quantities and units become first-class

The old library has `Angle` and `Time` as measures and an `IMeasure` interface. The new
set adds:

- a seven-base-dimension `Dimension` record;
- `Unit` and dynamic `Quantity`;
- named quantities including `Length`, `Area`, `Volume`, `Mass`, `Temperature`,
  `Speed`, `Acceleration`, `Force`, `Torque`, `Energy`, `Power`, `Pressure`, and
  electrical quantities.

Named wrapper types improve API legibility and prevent accidental interchange. The
current prototype does not yet encode derived-unit algebra in types, so it cannot prove
that length divided by duration is speed.

### Transform intent is more explicit

The old transform catalog is rich in useful named forms, including look-at,
yaw/pitch/roll, offset transforms, and projections. Its general types are
`Transform2D`, `Transform3D`, `Pose2D`, and `Pose3D`.

The new set emphasizes mathematical invariants:

- `RigidTransform2D` / `RigidTransform3D`
- `SimilarityTransform3D`
- `AffineTransform2D` / `AffineTransform3D` / `AffineTransform4D` / `AffineTransformN`
- `ProjectiveTransform3D`
- `DualQuaternion`
- generic `ITransform`, `IAffineTransform`, and `IRigidTransform`

This is a better foundation for composition and animation, but it currently omits
several convenient old representations such as `LookAt3D`, `YawPitchRoll`, and
`PlaneProjection3D`.

## Domain coverage

| Domain | Old coverage | New coverage |
|---|---|---|
| Scalar numerics | Number/integer algebra and intrinsics | Adds rational, complex, dual, fixed-point, big integer, decimal, polynomial |
| Linear algebra | Fixed vectors and selected matrices | Adds dynamic/sparse/banded matrices, tensors, bivectors, `VectorN` |
| Geometry | Strong 2D/3D primitives and operations | Broader representations across 1D–4D–N, but declarations only |
| Curves | Large analytic curve catalog with evaluation libraries | Bezier/B-spline/NURBS/Catmull–Rom, jets, samples, and arc-length records |
| Surfaces | Procedural/explicit surface interfaces | Analytic surfaces, patches, B-spline/NURBS, trimmed and subdivision surfaces |
| Solids | Parametric primitive types and libraries | Dimension-explicit solids plus sweep/revolve/extrude and boundary representation |
| Meshes | Line/triangle/quad grids and mesh operations | Adds polygon/tetrahedral meshes, attributes, topology, BVH, octree, k-d tree |
| Fields | Scalar fields, SDF3D, procedurals | Generic scalar/vector/tensor fields, time-varying fields, sampled grids |
| Color | Several spaces, constants, conversions | Adds linear/sRGB distinction, OKLab, spectral color, color-space metadata |
| Animation | Minimal time and transform support | Keys, tracks, clips, layers, skeletons, skinning, morph targets |
| Imaging/rendering | Largely outside the library | Images, volumes, channels, camera calibration, materials, lights, render scenes |
| Physics | Largely outside the library | Rigid bodies, particles, contacts, fluids, stress and strain |
| Engineering | Largely outside the library | Tolerances, material properties, sections, loads, finite-element models |
| Statistics/science | Largely outside the library | Distributions, PCA, regression, uncertainty, experiments, molecules, crystals |
| Signals/control | Largely outside the library | Signals, spectra, filters, state-space and transfer-function models |
| Optimization | Largely outside the library | Variables, objectives, constraints, iterations, LP/QP, root-finding results |

## Capabilities retained or improved

The following old foundations have recognizable successors:

- compiler primitives, tuples, functions, and arrays;
- `Vector2`, `Vector3`, `Vector4`, `Vector8`, `Matrix3x2`, `Matrix4x4`, and
  `Quaternion`;
- 2D/3D points, bounds, rays, triangles, quads, and common mesh forms;
- quadratic and cubic Bezier curves;
- polar, cylindrical, and spherical coordinates;
- scalar fields;
- translations, rotations, scales, and reflections;
- core arithmetic, ordering, interpolation, invertibility, arrays, curves, and
  geometry interfaces.

Most are no longer contract-equivalent. Examples include:

- `Bounds2D.Min/Max` becoming `Minimum/Maximum`;
- old points losing `IVectorLike` and `IDifference<T>`;
- old triangles participating in polygon/array/deformation interfaces while new
  triangles expose only geometry and bounds interfaces;
- old transforms returning matrices through libraries while new transform records
  have no behavior;
- old color types having conversion libraries while new color records are inert.

## Capabilities not carried forward

The new vocabulary does not yet replace several valuable parts of the old library.

### Executable libraries

All 25 old libraries are absent, including:

- `Core`, `Intrinsics`, `Vectors`, `Algebra`, and `Integers`;
- `Geometry`, `Meshes`, `Transforms`, and `Solids`;
- curve evaluation libraries and polar/angular curve libraries;
- constants, color constants, measures, intervals, arrays, fields, procedurals, and
  SDF functions.

### Named analytic curve catalog

The old catalog includes Lissajous, torus knots, cycloids, trochoids, roses,
lemniscates, named spirals, and many other analytic curves. The new set favors general
control-point curve representations and retains almost none of those named types.

This is a reduction in immediately usable geometry content, even though the new curve
representation model is broader.

### Convenience transform types

The following old interfaces have no direct new declaration:

- `LookAt3D` and `LookDirection3D`
- `YawPitch` and `YawPitchRoll`
- `TRSTransform2D` and `TRSTransform3D`
- `OffsetScale2D` and `OffsetRotation2D`
- `Perspective3D`, `Orthographic3D`, and `PlaneProjection3D`
- `FrenetFrame`

Some are representable by new types, but the intent is no longer directly named.

### Interface-provided behavior

Old geometry interfaces expose queries such as points, primitives, face indices,
closedness, deformation, bounds, and evaluation. Many new interfaces are markers, and
many new concrete types implement no interface at all. As a result, the new catalog is
less useful for generic algorithms until its capability assignments are audited.

## Validation status

The complete new folder:

- parses successfully;
- reports zero symbol-resolution errors;
- passes non-strict lint;
- generates 602 C# files with the current extension/scalar-erasure recipe;
- contains no `library` declarations or function bodies.

Non-strict lint reports 1,513 expected findings:

| Finding | Count | Meaning in this prototype |
|---|---:|---|
| `LINT001` | 121 | Declared interfaces lack concrete implementations |
| `LINT003` | 1,392 | Fields are not consumed because there are no libraries |

These findings are expected for a declaration-only pass, but they are also a useful
measure of how much behavior remains before this can become a functioning standard
library.

## Risks and design debt in the new set

1. **Breadth exceeds validation.** Many domain types are plausible schemas, not designs
   proven by algorithms or consumers.
2. **V2 naming collision.** The repository already uses “V2” for the live
   extension-style, scalar-erased code-generation recipe. `plato-src-v2` means a
   different source vocabulary.
3. **Stringly typed policies.** `InterpolationMode`, `SubdivisionScheme`,
   `AttributeAssociation`, query status, material models, and several rendering
   policies contain strings because Plato currently lacks sum types.
4. **Weak dimension algebra.** Named physical quantities prevent some mistakes but do
   not yet compose units statically.
5. **Function-valued fields need backend proof.** Curve segments, trimmed surfaces,
   fields, optimization problems, and charts store `FunctionN` values. Their behavior
   across C#, TypeScript, and Rust needs explicit validation.
6. **Some containers are underspecified.** `IndexLoop` is reused for polygon faces,
   cells, facets, and finite elements without enforcing arity or orientation.
7. **Interface coverage is uneven.** Core points and meshes implement interfaces, while
   many surfaces, solids, physics records, and engineering records are only data.
8. **No laws or witnesses exist.** Algebraic, geometric, interpolation, transform, and
   unit invariants are not yet tested.
9. **No compatibility aliases exist.** Even simple changes such as `Circle` to
   `Circle2D` require consumer migration.

## Recommended integration strategy

### 1. Stabilize a foundational kernel

Start with the smallest set needed to express the rest:

- compiler primitives and collections;
- scalars, vectors, matrices, points, and bounds;
- time, quantities, and ranges;
- generic field, transform, parametric, bounded, and indexed-geometry interfaces.

Resolve naming, laws, and interface implementation rules here before expanding behavior.

### 2. Define an explicit migration map

For each old public declaration, choose one outcome:

- retain unchanged;
- rename with a temporary compatibility alias;
- split into more precise types;
- replace with a general representation;
- retire with a documented reason.

The first priority should be high-traffic types: points, vectors, bounds, rays,
segments, matrices, transforms, colors, arrays, triangle meshes, and intervals.

### 3. Port behavior by coherent vertical slices

Avoid implementing all 606 types shallowly. Complete small useful slices, for example:

1. scalar/vector/point/bounds arithmetic;
2. segments, rays, planes, and intersection result types;
3. rigid/affine transforms;
4. Bezier curves and curve samples;
5. triangle meshes, attributes, and bounds;
6. fields, uniform grids, and sampled fields.

Each slice should include types, interfaces, libraries, laws, generated-code validation,
and at least one real consumer.

### 4. Preserve valuable old catalogs

Named analytic curves, transform conveniences, constants, and SDF operations should be
ported or deliberately retired. General representations such as NURBS do not eliminate
the usefulness of compact, discoverable analytic types.

### 5. Rename the source prototype before adoption

To avoid confusion with the existing V2 runtime/code-generation recipe, consider a
purpose-oriented temporary name such as:

- `plato-src-next`
- `plato-src-foundation`
- `plato-src-vocabulary`

The final source can return to `stdlib-legacy` after an intentional migration.

## Conclusion

The new set improves the conceptual reach and naming precision of Plato, particularly
for dimensionality, affine geometry, boundedness, time, units, fields, animation,
scientific data, and engineering. Its file organization is also substantially easier
to navigate and extend.

The old set remains far more complete as an executable library. It contains behavior,
tested code-generation paths, and a deep catalog of immediately useful geometry.

The strongest path forward is a synthesis: use the new vocabulary and organization as
the design space, retain the old library as the behavioral reference, and migrate in
small vertical slices until each new interface is supported by real functions, laws, and
consumers.
