# stdlib — comprehensive type & concept vocabulary

**Forward stdlib vocabulary** — domain declarations plus the concept-library implementation
bodies that build on them, co-located in this folder.

Comprehensive Plato vocabulary: broad domain coverage, `concept` keyword with bare names
(no `I` prefix), and files grouped by domain in dependency-layer order. The declaration
files carry vocabulary only (concepts and types, semantics in doc comments); the
`*.library.plato` files carry the `library` blocks that implement derived functionality
on those concepts (see [`LIBRARIES.md`](LIBRARIES.md)). Declarations and bodies sit
side by side in each tier folder; **every** `library` block lives in its own
`*.library.plato` file — no declaration file carries an inline one.

Target applications: geometry (primary), 2D/3D and N-dimensional computation, animation,
numerical/mathematical/scientific computing, graphics and rendering, physics, motion
graphics, image processing, and engineering.

Current contents (2026-07-30): **399 source files (189 `*.types.plato` files + 45
`*.concepts.plato` files + 165 `*.library.plato` files), 157 concepts, 1152 types**.

Every file holds exactly
one **kind** of declaration — `<stem>.concepts.plato` holds concepts, `<stem>.types.plato` holds
types, `<stem>.library.plato` holds exactly one `library` block — and at most twelve top-level
declarations. The move was mechanical: the sorted type-and-concept inventory is byte-identical
to the pre-refactor baseline, so nothing was added, removed, or renamed — only relocated.
Fixed 4D geometry (Point4D, Bounds4D, Geometry4D, Curve4D, polytopes, 4D rotors) was removed
2026-07-28 — the practical 4D uses are numeric and live on `Number4`/`Quaternion`; N-dimensional
work uses `PointN`/`VectorN`. 4D arrays (`Array4D`/`Indexable4D`) initially survived that purge
as "collections", then were removed 2026-07-29: nothing declared, constructed, or backed them
(no storage field), and the real 4D-array use case (time-varying volumes) models better as
`Array<Array3D<T>>` or a dedicated named-axis type when the need actually arrives.
The folder parses and resolves cleanly (`lint`: 0 parse errors, 0 symbol-resolution errors);
LINT001 (unimplemented members) / LINT003 (unread fields) findings are expected until the
concept libraries implement the full declared surface. Lint now enumerates the `*.library.plato`
bodies alongside the declarations (they used to sit in a subfolder lint did not reach), so those
finding counts reflect the implemented surface — implemented members drop out of LINT001 and
library-read fields drop out of LINT003.

## Target backends

Code generated from this vocabulary targets, **in priority order**:

1. **C#** (primary; the reference runtime)
2. **C++**
3. **CUDA**
4. **TypeScript**
5. Others as capacity allows: **GLSL**, **Rust**, **Python**

This ordering governs intrinsics policy (the `intrinsics-*.library.plato` files, whose shared
preamble and porting notes live in `intrinsics-scalars.library.plato`): a function may be
declared intrinsic only if every priority-1..4 backend can supply it natively or with a
trivial shim. Anything host-specific (C# SIMD types, IEEE nextafter-grade functions,
midpoint-rounding variants) is excluded and noted in those porting notes; lower-
priority backends may polyfill (e.g. GLSL lacks double precision — `Number` maps to
`float` there).

## Validation

```
dotnet <path-to>/Plato.CLI.dll lint stdlib/foundation stdlib/geometry stdlib/graphics stdlib/future
```

Each root is enumerated **top-directory-only**, so the tier folders are named explicitly —
`lint stdlib` on its own would find zero files. The tiers are dependency-ordered and
cumulative, so any prefix of that list is a valid, faster subset gate:

```
lint stdlib/foundation                                    # ~130 files
lint stdlib/foundation stdlib/geometry                    # + geometry
lint stdlib/foundation stdlib/geometry stdlib/graphics    # + graphics
```

A folder may reference only itself and the folders before it; `future` may reach anything,
nothing reaches into `future`.

The tree must parse and resolve with zero errors. It is self-contained (declares its own
primitives in `foundation/primitives.types.plato`). Declaration files and the
`*.library.plato` implementation bodies sit together in each tier folder, so one command
covers both (see [`LIBRARIES.md`](LIBRARIES.md)); the informational LINT001/LINT003 finding
counts shift as the libraries implement more members.

## Conventions and style

Two companion docs — read both before editing this folder:

| Doc | Role |
| --- | --- |
| [`CONVENTIONS.md`](CONVENTIONS.md) | **Semantics:** world Z-up (Studio), CCW winding, System.Numerics-compatible matrices, `-1` index sentinel, radians-canonical `Angle`, inclusive bounds + empty encoding, linear-light straight-alpha `Color`, camera-local view space, top-left UV origin, epsilon policy, no generic `Optional<T>` |
| [`STYLE_GUIDE.md`](STYLE_GUIDE.md) | **Authoring:** small pure functions, array literals vs `MapRange`, operators, comments, Wikipedia links, AGENTS-derived API style |

Owning declaration files cite conventions with a one-line
`// Convention: see CONVENTIONS.md - <section>` comment rather than restating them.

- `concept` = capability/trait (like a type class over `Self`). Bare PascalCase names, no
  `I` prefix: `Numerical`, `Curve3D`, `Transformable<T>`.
- `type` = immutable data, fields only. Concrete nouns: `Vector3D`, `TriangleMesh3D`.
- **Vector naming rule:** a bare number counts components (`Number3`, `Tuple3`,
  `IntegerVector3`); a `D` suffix means the type lives in that-dimensional space
  (`Vector3D`, `Point3D`, `Ray3D`). `Number2/3/4/8` are the low-level intrinsic
  numeric tuples; `Vector2D/3D` are geometric displacements. There is no `Vector2/3/4`.
- A concept name must never equal a type name.
- Every declaration has a `//` doc comment stating what it is and any invariants.
  Section banners use `//==`.
- Every concept function takes `Self` as its first parameter.
- Tagged/variant choices use **sum types** (`type X = Case(...fields) | Case | ...;` with
  exhaustive `match`), the preferred encoding for discriminated unions. Payload-free variants are
  enums (`type FillRule = NonZero | EvenOdd;`); variants with conditional per-case data are
  true sums (`PathSegment2D`, `Paint`, `MaskSource2D`, `ScalarFieldNode2D/3D`, `WindowFunction`
  — the wave-3 flagship migrations). Spec: [`../docs/plato-sum-types-design-2026-07-27.md`](../docs/plato-sum-types-design-2026-07-27.md).
  The older *kind pattern* (`type FooKind { Value: Integer; }` + a value-meaning doc comment)
  is fully retired — every kind-pattern type is now a sum. New declarations must use a sum,
  not a kind. (The only remaining single-`Value: Integer` records are the typed index
  wrappers implementing `Index`, plus opaque codes like `MortonCode2D/3D` and `FontWeight`.)
- Quantity types carry natural-unit field names (`Radians`, `Meters`, `Kelvin`).
- Collection fields use `Array<T>` (or `Array2D<T>`, `Array3D<T>`).
- Cross-array references use typed index types implementing the `Index` concept
  (`VertexIndex`, `BoneIndex`, `GraphVertexIndex`, ...), never raw `Integer`; `-1` means
  "none". `ItemIndex` (numbers.types.plato) is the general-purpose form for caller-supplied or pool
  lists. CSR/offset arrays stay `Array<Integer>` (they hold one-past-end boundaries, not
  element references), as do bitmasks, counts, labels, axis selectors, and opaque host
  handles.
- Optional references use sentinel conventions noted in comments (e.g. `-1` for "no index",
  empty array for "none").
- **Hard limit: at most 10 fields per type** (the compiler synthesizes a `TupleN`
  constructor per type and supports tuples only up to `Tuple10`). Split bigger records
  into nested component types (e.g. matrices store row vectors).
- Angles are `Angle`, never raw `Number`. Distances/positions in unit-bearing contexts may
  still use `Number` when the domain is unit-agnostic (pure math), `Length` when physical.
- **Naming:** domain type-declaration files are `domain.types.plato`; pure concept files are
  `domain.concepts.plato`; library files are `domain.library.plato`. There are no numeric
  prefixes; reading order lives in this README's layer table below. Files live in one of four
  dependency-ordered **tier folders** — `foundation/`, `geometry/`, `graphics/`, `future/` —
  which double as compile subsets (see Validation above). `future/` holds the aspirational
  domains (physics, engineering, scientific computing, geo-spatial): real vocabulary, kept
  off the priority path so its debt never blocks 2D/3D geometry and graphics work. **This grammar is now
  exact, not aspirational:** every file in the folder holds
  exactly one kind of declaration, so a file's suffix tells you what is inside it without
  opening it — a `.concepts.plato` file contains only `concept` blocks, a `.types.plato` file
  only `type` declarations (there are no bare `<stem>.plato` files), and a `.library.plato` file exactly one `library` block whose
  PascalCase name matches the stem.

## Layers and file map

This ordered index is the canonical reading order. Every one of the 398 `*.plato` files in the
folder appears in exactly one row; file names are given without the `.plato` extension, so
`core-logic.concepts` means `core-logic.concepts.plato` and a bare stem like `primitives`
means `primitives.types.plato`. Layer numbers are the reading order and
also the rough dependency order — a layer may use anything from a lower-numbered layer and should
not reach upward. (An old "Owner" column named the agent that wrote each group during
initial build-out; it described a work assignment, not the tree, and is gone.)

| # | Layer | Files |
|---|-------|-------|
| 1 | Foundation — primitives, core comparison & logic | `core-comparison.concepts`, `core-comparison.library`, `core-logic.concepts`, `core-logic.library`, `primitives`, `primitives-arrays`, `primitives-builders`, `primitives-builders.library`, `primitives-functions`, `primitives-tuples` |
| 2 | Foundation — algebra, collections, functional | `algebra-metric.concepts`, `algebra-metric.library`, `algebra-numeric.concepts`, `algebra-numeric.library`, `algebra-operations.concepts`, `algebra-operations.library`, `collections-containers.concepts`, `collections-containers.library`, `collections-grids.library`, `collections-indexable.concepts`, `collections-indexable.library`, `collections-jagged`, `collections-jagged.concepts`, `collections-jagged.library`, `collections-sampling.library`, `functional-procedural.library`, `functional.concepts` |
| 3 | Foundation — numbers, quantities, angles, constants, time | `angles.library`, `constants.library`, `numbers`, `quantities-dynamic`, `quantities-electromagnetic`, `quantities-geometric`, `quantities-kinematic`, `quantities-material`, `quantities-mechanical`, `quantities-photometric`, `quantities-projections.library`, `quantities-thermal`, `quantities.concepts`, `quantities.library`, `time` |
| 4 | Foundation — vectors, matrices, rotations, points, axes | `axes`, `axes-2d.library`, `axes-signed.library`, `axes.library`, `matrices`, `matrices.concepts`, `numeric-structures-algebra.library`, `numeric-structures-components.library`, `numeric-structures-coordinate.library`, `numeric-structures-matrix.library`, `numeric-structures-quantity.library`, `numeric-structures-vector.library`, `points`, `points-curvilinear`, `points-parametric`, `points.concepts`, `rotations`, `vectors-geometric`, `vectors-integer`, `vectors-tuples`, `vectors.concepts` |
| 5 | Foundation — intervals, bounds, sizes, transforms | `deformations`, `deformations.concepts`, `deformations.library`, `intervals`, `intervals-bounds`, `intervals-bounds.concepts`, `intervals-sizes`, `intervals-transforms-bounds.library`, `intervals-transforms-deformable.library`, `intervals-transforms-interval.library`, `intervals-transforms-transformable.library`, `transforms-affine`, `transforms-affine.library`, `transforms-frames`, `transforms-frames.library`, `transforms-identities.library`, `transforms-motor`, `transforms-motor.library`, `transforms-points.library`, `transforms-pose`, `transforms-pose.library`, `transforms-rotations.library`, `transforms-trs`, `transforms-trs.library`, `transforms.concepts` |
| 6 | Foundation — color | `color`, `color-named.library`, `color.library` |
| 7 | Intrinsics (host-provided) | `intrinsics-arrays.library`, `intrinsics-numeric-tuples.library`, `intrinsics-scalars.library`, `intrinsics-transforms.library`, `intrinsics-vectors.library` |
| 8 | Geometry concepts & primitive shapes | `geometry-kernels.library`, `geometry-measures.concepts`, `geometry-measures.library`, `geometry-pointsets.library`, `geometry-queries.concepts`, `geometry-queries.library`, `geometry.concepts`, `geometry.library`, `lines`, `lines-planes`, `lines-planes.library`, `lines.library`, `planar-boxes`, `planar-boxes.library`, `planar-circles`, `planar-circles.library`, `planar-ellipses`, `planar-ellipses.library`, `planar-shapes.library`, `planar-triangles`, `planar-triangles.library`, `polygons`, `polygons-kernels.library`, `polygons-polylines`, `polygons-polylines.library`, `polygons-spatial`, `polygons-spatial.library`, `polygons.library`, `spatial-boxes`, `spatial-boxes.library`, `spatial-capsules.library`, `spatial-cylinders`, `spatial-cylinders.library`, `spatial-patches`, `spatial-patches.library`, `spatial-primitives.library`, `spatial-simplices`, `spatial-simplices.library`, `spatial-spheres`, `spatial-spheres.library`, `spatial-tori`, `spatial-tori.library` |
| 9 | Curves, splines, surfaces, solids | `curves-2d-arcs`, `curves-2d-arcs.library`, `curves-2d-polar`, `curves-2d-polar.library`, `curves-2d-spirals`, `curves-2d-spirals.library`, `curves-3d`, `curves-3d.library`, `curves-capabilities.concepts`, `curves-capabilities.library`, `curves-sampling.library`, `curves.concepts`, `curves.library`, `solids-csg`, `solids-generated`, `solids-polyhedra`, `solids.library`, `splines-bezier`, `splines-bezier.library`, `splines-bspline`, `splines-bspline.library`, `splines-hermite`, `splines-hermite.library`, `splines-interpolating`, `splines-interpolating.library`, `surfaces-generated`, `surfaces-patches`, `surfaces-solids.concepts`, `surfaces-solids.library`, `surfaces-special`, `surfaces.library` |
| 10 | Fields, implicits/SDF, noise, sampling | `fields-constant`, `fields-differentiable.concepts`, `fields-function`, `fields-graphs`, `fields-graphs.library`, `fields-implicits-core.library`, `fields-implicits-differentiable.library`, `fields-implicits-distance.library`, `fields-implicits-function.library`, `fields-implicits-metaballs.library`, `fields-implicits-nodes.library`, `fields-implicits-sampled.library`, `fields-implicits-shapes.library`, `fields-implicits-time-varying.library`, `fields-time-varying.concepts`, `fields.concepts`, `implicit-sdf-function`, `implicit-sdf-metaballs`, `implicit-sdf-modifiers`, `implicit-sdf-modifiers.library`, `implicit-sdf-operators.library`, `implicit-sdf-primitives.library`, `implicit-sdf-sampled`, `implicit-sdf-sampled.library`, `implicit-sdf-trees`, `implicit-sdf-trees.library`, `implicit-sdf.concepts`, `noise`, `noise-basis`, `noise-fractal`, `noise-warped`, `sampling-curves`, `sampling-fields`, `sampling-grids`, `sampling-patterns`, `sampling-resampling` |
| 11 | Topology, meshes, point clouds, spatial structures & queries | `mesh-attributes`, `meshes`, `meshes-face-arrays`, `meshes-face-arrays.library`, `meshes-faces.library`, `meshes-geometry.library`, `meshes-indexed.library`, `meshes-lines-points`, `meshes-polygon-incidence.library`, `meshes-sections`, `meshes-topology.library`, `meshes-volumetric`, `meshes.concepts`, `pointclouds`, `pointclouds-voxels.concepts`, `spatial-grids`, `spatial-implicits`, `spatial-kdtrees`, `spatial-queries-overlap`, `spatial-queries-proximity`, `spatial-queries-rays`, `spatial-queries.concepts`, `spatial-queries.library`, `spatial-structures.concepts`, `spatial-trees`, `topology-adjacency`, `topology-adjacency.library`, `topology-classification`, `topology-half-edges`, `topology-indices`, `topology.concepts`, `voxels` |
| 12 | Animation & motion | `easing`, `easing.concepts`, `easing.library`, `keyframes-blending`, `keyframes-clips`, `keyframes-tracks`, `keyframes-tracks.concepts`, `keyframes-tracks.library`, `motion-graphics`, `motion-graphics-procedural`, `motion-graphics-timing`, `skeletal-animation`, `skeletal-animation-constraints` |
| 13 | Vector graphics, text, 2D scenes | `paths`, `paths.concepts`, `paths.library`, `scene2d`, `text-fonts`, `text-glyphs`, `text-layout`, `vector-styling-paint`, `vector-styling-stroke` |
| 14 | Color science & imaging | `color-spaces`, `color-spaces-models`, `color-spaces-palettes`, `color-spaces-video`, `image-processing-adjustments`, `image-processing-compositing`, `image-processing-filters`, `image-processing-resampling`, `images`, `images-containers`, `images.concepts`, `images.library`, `texturing`, `texturing-sampling`, `texturing.concepts`, `texturing.library` |
| 15 | Rendering | `cameras`, `cameras-optics`, `cameras.concepts`, `cameras.library`, `lights`, `lights-settings`, `lights.concepts`, `lights.library`, `materials`, `materials-layers`, `render-post-processing`, `render-settings`, `render-vertex-layout`, `scene3d`, `scene3d-indices`, `scene3d-instancing` |
| 16 | Physics & simulation | `collision`, `collision-contacts`, `joints-2d`, `joints-3d`, `joints-limits`, `kinematics`, `kinematics-trajectories`, `kinematics.concepts`, `kinematics.library`, `particles-force-fields`, `particles-simulation`, `particles-soft-bodies`, `rigid-dynamics`, `rigid-dynamics-forces`, `rigid-dynamics.concepts`, `rigid-dynamics.library` |
| 17 | Math, statistics, signals, optimization | `optimization`, `optimization-programs`, `optimization-solvers`, `polynomials`, `polynomials-composite`, `polynomials-series`, `polynomials.library`, `random`, `random-continuous`, `random-continuous-gamma`, `random-continuous-gamma.library`, `random-continuous-tails.library`, `random-continuous.library`, `random-discrete`, `random-discrete.library`, `random-distributions.library`, `random-multivariate`, `random.concepts`, `random.library`, `signals`, `signals-filters`, `signals-generators`, `statistics`, `statistics-correlation`, `statistics-correlation.library`, `statistics.library`, `uncertainty`, `uncertainty-estimation` |
| 18 | Advanced & applied | `differential-geometry-frames`, `differential-geometry-geodesics`, `differential-geometry-surfaces`, `engineering-beams`, `engineering-machine-elements`, `engineering-materials`, `engineering-sections`, `geo-spatial`, `geo-spatial-rasters`, `geo-spatial-reference-systems`, `geo-spatial.concepts`, `geo-spatial.library`, `graphs`, `graphs-algorithms`, `graphs.concepts`, `graphs.library`, `higher-dimensions`, `higher-dimensions-fractals`, `scientific-data-records`, `scientific-data-series`, `scientific-data.concepts`, `scientific-data.library` |

The 120 `*.library.plato` files hold the `library` blocks that implement derived functionality on
the concepts and types declared beside them. Fifty-four of them belong to the P1–P9 concept work
packages; the other sixty-six serve foundation domains or supply the concrete bodies that used to
sit inline in a declaration file. Ground rules and the package-to-file table live in
[`LIBRARIES.md`](LIBRARIES.md).

Foundation reading order, file by file:

- `primitives.types.plato` — Number, Integer, Boolean, String, Character, Dynamic, Object.
- `primitives-tuples.types.plato` — Tuple2..Tuple10 (the compiler's synthesized constructor arity).
- `primitives-functions.types.plato` — Function0..Function4.
- `primitives-arrays.types.plato` — Array, Array2D, Array3D.
- `primitives-builders.types.plato` (+ `.library`) — the `unique` affine builders `List<T>` / `Buffer<T>`.
- `core-comparison.concepts.plato` — Equatable, Value, Hashable, Orderable, Comparable.
- `core-logic.concepts.plato` — Logical, BooleanAlgebra, Bitwise.
- `algebra-operations.concepts.plato` — Additive, Multiplicative, Divisible, Modular, Invertible, Arithmetic, Scalable, Interpolatable.
- `algebra-numeric.concepts.plato` — NumericalLimits, Numerical, Real, Whole.
- `algebra-metric.concepts.plato` — Normed, MetricSpace, Lattice, Clampable, Difference.
- `collections-indexable.concepts.plato` — Countable, Index, Indexable, Indexable2D/3D/4D.
- `collections-containers.concepts.plato` — Sliceable, Concatenable, SetLike, MapLike, StackLike, QueueLike.
- `functional.concepts.plato` — Procedural, Bijective, Periodic, ParameterDomain.
- `numbers.types.plato` — Complex, Rational, Proportion, Percent, Probability, ItemIndex, Cardinal, ComparisonTolerance.
- `quantities.concepts.plato` — the `Quantity` concept.
- `quantities-geometric.types.plato` / `-kinematic` / `-mechanical` / `-thermal` / `-electromagnetic` / `-photometric` / `-material` / `-dynamic` — the ~50 physical quantity types (`Angle` and `Length` live in `quantities-geometric.types.plato`), plus Dimension / UnitOfMeasure / DynamicQuantity.
- `time.types.plato` — Duration, Instant, TimeInterval, FrameRate, FrameTime, Timecode, Tempo, BeatTime.
- `vectors.concepts.plato` — the `Vector` concept.
- `vectors-tuples.types.plato` — Number2, Number3, Number4, Number8 (the low-level intrinsic tier).
- `vectors-geometric.types.plato` — Vector2D, Vector3D, VectorN, Direction2D/3D.
- `vectors-integer.types.plato` — IntegerVector2/3/4.
- `matrices.concepts.plato` — the `MatrixLike` concept.
- `matrices.types.plato` — Matrix2x2, Matrix3x3, Matrix4x4, Matrix3x2, Matrix4x3, SymmetricMatrix3x3, MatrixN, Tensor.
- `rotations.types.plato` — Quaternion, AxisAngle, RotationOrder, EulerAngles, Rotation2D, Bivector2D/3D, Rotor2D/3D.
- `points.concepts.plato` — the `Coordinate` concept.
- `points.types.plato` — Point2D, Point3D, PointN, HomogeneousPoint2D/3D.
- `points-parametric.types.plato` — BarycentricCoordinate, UvCoordinate, UvwCoordinate.
- `points-curvilinear.types.plato` — PolarCoordinate, CylindricalCoordinate, SphericalCoordinate, GeoCoordinate.
- `axes.types.plato` — Axis3D, Axis2D, SignedAxis3D (the typed cardinal-axis sums).
- `intervals-bounds.concepts.plato` — IntervalLike, BoundsLike.
- `intervals.types.plato` — NumberInterval, AngleInterval, LengthInterval, IntegerInterval.
- `intervals-bounds.types.plato` — Bounds2D/3D, IntegerBounds2D/3D, Rect2D.
- `intervals-sizes.types.plato` — Size2D/3D, IntegerSize2D/3D.
- `transforms.concepts.plato` — Transformable, Deformable2D, Deformable3D.
- `transforms-pose.types.plato` — Pose2D, Pose3D.
- `transforms-trs.types.plato` — Transform2D, Transform3D (TRS).
- `transforms-affine.types.plato` — AffineTransform2D/3D, ProjectiveTransform2D/3D.
- `transforms-frames.types.plato` — Frame2D, Frame3D, Basis3D.
- `transforms-motor.types.plato` — Motor3D (dual quaternion).
- `color.types.plato` — Color (linear RGBA), Color8, ColorHSV, ColorHSL, ColorStop, ColorGradient.

The transform *bodies* — conversions between all representations, `p.Transform(t)` application,
Compose/Inverse/Identity, and the Point2D/3D Difference + Lerp implementations — live in the eight
`transforms-*.library.plato` files (`transforms-points`, `-pose`, `-trs`, `-affine`, `-frames`,
`-motor`, `-rotations`, `-identities`).

## Cross-domain name registry

Types/concepts referenced across domains. The **owner declares**; everyone else references.
Never re-declare a registry name. If you need something similar, qualify the name with your
domain (`ImageHistogram`, not a second `Histogram`). Owner files below were re-derived from the
tree; when in doubt, `grep -n "^type X\|^concept X" *.plato` is
the authority, not this table.

| Name | Owner file |
|------|-----------|
| `Number`, `Integer`, `Boolean`, `String`, `Character`, `Dynamic`, `Object` | `primitives.types.plato` |
| `List<T>`, `Buffer<T>` (unique affine builders) | `primitives-builders.types.plato` |
| `Array`, `Array2D`, `Array3D` | `primitives-arrays.types.plato` |
| `Jagged` concept (CSR row packing) | `collections-jagged.concepts.plato` |
| `JaggedArray<T>` | `collections-jagged.types.plato` |
| `ComparisonTolerance`, `ItemIndex`, `Complex`, `Rational`, `Proportion`, `Percent`, `Probability`, `Cardinal` | `numbers.types.plato` |
| `Quantity` concept | `quantities.concepts.plato` |
| `Angle`, `Length` (and the other ~50 quantity types, by branch) | `quantities-geometric.types.plato` and the other `quantities-*.plato` files |
| `Number2/3/4/8` | `vectors-tuples.types.plato` |
| `Vector2D/3D`, `VectorN`, `Direction2D/3D` | `vectors-geometric.types.plato` |
| `Point2D/3D`, `PointN`, homogeneous points | `points.types.plato` |
| `UvCoordinate`, `UvwCoordinate`, `BarycentricCoordinate` | `points-parametric.types.plato` |
| `PolarCoordinate`, `CylindricalCoordinate`, `SphericalCoordinate`, `GeoCoordinate` | `points-curvilinear.types.plato` |
| `Axis3D`, `Axis2D`, `SignedAxis3D` | `axes.types.plato` |
| `NumberInterval`, `AngleInterval`, `LengthInterval`, `IntegerInterval` | `intervals.types.plato` |
| `Bounds2D/3D`, `IntegerBounds2D/3D`, `Rect2D` | `intervals-bounds.types.plato` |
| `Size2D/3D`, `IntegerSize2D/3D` | `intervals-sizes.types.plato` |
| `Color`, `Color8`, `ColorHSV`, `ColorHSL`, `ColorStop`, `ColorGradient` | `color.types.plato` |
| `Geometry`, `Geometry2D/3D/ND`, shape-trait concepts | `geometry.concepts.plato` |
| `Bounded2D/3D`, `PointSet2D/3D`, `Centroid2D/3D`, the measurable concepts | `geometry-measures.concepts.plato` |
| `ContainsPoint2D/3D`, `NearestPoint2D/3D`, `SupportMappable2D/3D` | `geometry-queries.concepts.plato` |
| `Line2D/3D`, `Ray2D/3D`, `LineSegment2D/3D` | `lines.types.plato` |
| `Plane`, `HalfSpace` | `lines-planes.types.plato` |
| `Triangle2D`, `Quad2D` | `planar-triangles.types.plato` |
| `Circle`, `Capsule2D` | `planar-circles.types.plato` |
| `Ellipse` | `planar-ellipses.types.plato` |
| `RegularPolygon` | `planar-boxes.types.plato` |
| `Sphere`, `Ellipsoid` | `spatial-spheres.types.plato` |
| `Box3D` | `spatial-boxes.types.plato` |
| `Cylinder`, `Cone`, `Capsule3D`, `CylindricalShell` | `spatial-cylinders.types.plato` |
| `Torus` | `spatial-tori.types.plato` |
| `Triangle3D`, `Quad3D` | `spatial-patches.types.plato` |
| `Tetrahedron` | `spatial-simplices.types.plato` |
| `Polygon2D`, `PolygonWithHoles2D` | `polygons.types.plato` |
| `Polyline2D`, `Polyline3D` | `polygons-polylines.types.plato` |
| `Polygon3D`, `Prism3D`, `Pyramid3D` | `polygons-spatial.types.plato` |
| `Curve<TRange>`, `Curve1D/2D/3D`, `ClosedCurve2D/3D` concepts | `curves.concepts.plato` |
| `PeriodicCurve`, `DifferentiableCurve2D/3D`, `FramedCurve3D`, `PolarCurve2D`, `ArcLengthParameterized` | `curves-capabilities.concepts.plato` |
| `Surface`, `ParametricSurface`, `Solid`, `ConvexSolid`, `ParametricVolume` concepts | `surfaces-solids.concepts.plato` |
| `CircularArc2D`, `QuadraticBezier2D`, `CubicBezier2D` | `curves-2d-arcs.types.plato` |
| `CycloidOfCeva2D`, `TschirnhausenCubic2D`, `ConchoidOfDeSluze2D`, `SinusoidalSpiral2D`, `TrisectrixOfMaclaurin2D`, `ButterflyCurve2D` | `curves-2d-polar.types.plato` |
| `CubicBezier3D`, `Helix` | `curves-3d.types.plato` |
| `BSplineCurve2D/3D`, `NurbsCurve2D/3D` | `splines-bspline.types.plato` |
| `HermiteCurve2D/3D` | `splines-hermite.types.plato` |
| `CatmullRomCurve2D/3D` | `splines-interpolating.types.plato` |
| `NurbsSurface`, `BezierPatch` | `surfaces-patches.types.plato` |
| `SurfaceOfRevolution`, `ExtrudedSurface` | `surfaces-generated.types.plato` |
| `RegularPyramid`, `SquarePyramid` | `solids-polyhedra.types.plato` |
| `ScalarField2D/3D`, `VectorField2D/3D` concepts | `fields.concepts.plato` |
| `ScalarFunctionField2D/3D`, `VectorFunctionField2D/3D` | `fields-function.types.plato` |
| `SignedDistanceField2D/3D`, `ImplicitRegion2D`, `ImplicitVolume3D` concepts | `implicit-sdf.concepts.plato` |
| `FunctionSdf2D/3D`, `FunctionRegion2D`, `FunctionVolume3D` | `implicit-sdf-function.types.plato` |
| `VertexIndex`, `UndirectedEdgeIndex`, `FaceIndex`, `CornerIndex`, `HalfEdgeIndex` | `topology-indices.types.plato` |
| `MeshElementCounts`, `MeshIncidence`, `HalfEdgeNavigable` concepts | `topology.concepts.plato` |
| `VertexPair`, `UndirectedEdgeList`, `UndirectedEdgeAdjacency` | `topology-adjacency.types.plato` |
| `WindingOrder` | `topology-classification.types.plato` |
| `TriangleMesh3D`, `QuadMesh3D`, `PolygonMesh3D`, `TriangleFace` | `meshes.types.plato` |
| `LineSet3D`, `PointCloud3D` | `meshes-lines-points.types.plato` |
| `TriangleArray3D`, `QuadArray3D` (unwelded) | `meshes-face-arrays.types.plato` |
| `RayHit2D`, `RayHit3D` | `spatial-queries-rays.types.plato` |
| `ClassicEasing`, `SpringParameters` | `easing.types.plato` |
| `Keyframe<T>`, `AnimationTrack<T>` | `keyframes-tracks.types.plato` |
| `AnimationClip` | `keyframes-clips.types.plato` |
| `Bone`, `Skeleton`, `SkeletonPose`, `MorphTarget` (physics never uses bare `Joint`) | `skeletal-animation.types.plato` |
| `Path2D`, `PathSegment2D` | `paths.types.plato` |
| `StrokeStyle` | `vector-styling-stroke.types.plato` |
| `FillStyle`, `Paint` | `vector-styling-paint.types.plato` |
| `Image` concept | `images.concepts.plato` |
| `Bitmap`, `PixelFormat`, `ImageOrigin` | `images.types.plato` |
| `BlendMode` | `image-processing-compositing.types.plato` |
| `Texture2D`, `Texture3D`, `TextureCube` | `texturing.types.plato` |
| `TextureSampler`, `TextureBinding` | `texturing-sampling.types.plato` |
| `Camera` concept | `cameras.concepts.plato` |
| `PerspectiveCamera`, `OrthographicCamera`, `CameraProjection` | `cameras.types.plato` |
| `Material` (rendering PBR material) | `materials.types.plato` |
| `RenderLayer` | `scene3d.types.plato` |
| `RigidBody2D/3D`, `MassProperties2D/3D`, `BodyIndex` | `rigid-dynamics.types.plato` |
| `Histogram`, `SummaryStatistics` | `statistics.types.plato` |
| `RandomState` | `random.types.plato` |
| `NormalDistribution`, `UniformDistribution` | `random-continuous.types.plato` |
| `Spectrum`, `SampledSignal` | `signals.types.plato` |
| `Polynomial` | `polynomials.types.plato` |
| `Tolerance` | `uncertainty.types.plato` |
| `Graph`, `GraphEdge` | `graphs.types.plato` |

Generic nouns that MUST be domain-qualified wherever declared: Node, Layer, Track, Channel,
Sample, Grid, Cell, Filter, Kernel, Key, Frame, Edge, Vertex, Face, Segment, Weight, Style,
Event, Marker, Anchor, Handle, Buffer, Attribute, Region, Mask, Map, Range, Wave, State.
(Exception: registry entries above that already claim a bare name.)

`Edge` is the sharpest case, because the two mesh readings differ by a factor of two: a mesh
edge is either an **`UndirectedEdge`** (a deduplicated vertex pair — `UndirectedEdgeIndex`,
`UndirectedEdgeCount`, `UndirectedEdgeList`) or a **`HalfEdge`** (one directed side —
`HalfEdgeIndex`). Nothing in the mesh domain is named a bare `Edge`; a name that does not say
which reading it means is a bug report waiting to happen (an Euler characteristic computed from
half-edges is silently wrong). Other domains keep their own sense of the word — graph theory's
`GraphEdge` / `EdgeCount` (directedness is a property of the `Graph`, not the name), image
processing's `EdgeDetection` / `ClampToEdge` (a border, not an incidence relation).

`List` and `Buffer` are a standing exception to that rule: they are **compiler-intrinsic names**
— the compiler maps Plato `List`/`Buffer` onto the handwritten `PlatoList`/`PlatoBuffer` runtime
types — so they cannot be domain-qualified.

## Constants

Constants dispatch on an **ignored receiver of the result type**, never as zero-argument
functions (there are none in this tree):

```
GoldenRatio(_: Number): Number => (1.0 + 5.0.Sqrt) / 2.0;
```

read at the call site as `Number.GoldenRatio`, `Vector3D.UnitX`, `Circle.UnitCircle`,
`Color8.AliceBlue`. This matches the intrinsic constants in
`intrinsics-scalars.library.plato` and the `Identity(_: Quaternion)` family in
`transforms-identities.library.plato`.

`Angle` values are constructed through the unit constructors in `angles.library.plato` —
`0.25.Turns`, `90.Degrees`, `100.Gradians` — which is how the "angles are `Angle`, never raw
`Number`" rule above is satisfied in practice. The radians intrinsic is not the only door.
