# stdlib — comprehensive type & concept vocabulary

**Forward stdlib vocabulary** — domain declarations plus the concept-library implementation
bodies that build on them, co-located in this folder. Codegen and Studio still ship from
`stdlib-legacy`.

Third-generation Plato vocabulary. Successor to the `plato-src-v2` prototype (plato-228):
much broader coverage, `concept` keyword with bare names (no `I` prefix), and files grouped
by domain in dependency-layer order. The declaration files carry vocabulary only (concepts and
types, semantics in doc comments); the `*.library.plato` files carry the `library` blocks that
implement derived functionality on those concepts (see [`LIBRARIES.md`](LIBRARIES.md)). These
were formerly split — declarations here, bodies in a `concept-library/` subfolder — and are now
flattened together. `transforms.plato` also carries its own `library Transforms`.

Target applications: geometry (primary), 2D/3D and N-dimensional computation, animation,
numerical/mathematical/scientific computing, graphics and rendering, physics, motion
graphics, image processing, and engineering.

Current contents (2026-07-28): **84 source files (71 declaration files + 13 library
implementation files), 150 concepts, 1131 types**.

The library count grew from 9 to 13 with the `stdlib-legacy` port (see
[`../docs/stdlib-legacy-port-candidates-2026-07-28.md`](../docs/stdlib-legacy-port-candidates-2026-07-28.md)):
`constants.library.plato`, `angles.library.plato`, `quantities.library.plato` and
`color.library.plato` serve foundation domains rather than a P1–P9 concept package. Eleven
declaration files additionally carry their own inline `library` block — `transforms`,
`polynomials`, `primitives`, `intrinsics`, `curves-2d`, `curves-3d`, `splines`, `solids`,
`surfaces`, `fields`, `implicit-sdf` — which is the sanctioned home for bodies that belong
beside their declarations (`LIBRARIES.md` ground rules 1 and 10).
Fixed 4D geometry (Point4D, Bounds4D, Geometry4D, Curve4D, polytopes, 4D rotors) was removed
2026-07-28 — the practical 4D uses are numeric and live on `Number4`/`Quaternion`; N-dimensional
work uses `PointN`/`VectorN`. 4D **arrays** (`Array4D`/`Indexable4D`) are collections and remain.
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

This ordering governs intrinsics policy (`intrinsics.plato`): a function may be
declared intrinsic only if every priority-1..4 backend can supply it natively or with a
trivial shim. Anything host-specific (C# SIMD types, IEEE nextafter-grade functions,
midpoint-rounding variants) is excluded and noted in that file's porting notes; lower-
priority backends may polyfill (e.g. GLSL lacks double precision — `Number` maps to
`float` there).

## Validation

```
dotnet <path-to>/Plato.CLI.dll lint stdlib
```

The folder must parse and resolve with zero errors. It is self-contained (declares its own
primitives in `primitives.plato`). Because `Plato.CLI` enumerates `*.plato` non-recursively
(`Program.cs:101` / `:197`, `TopDirectoryOnly`), this single command now covers both the
declaration files and the `*.library.plato` implementation bodies (see [`LIBRARIES.md`](LIBRARIES.md));
the informational LINT001/LINT003 finding counts shift as the libraries implement more members.

## Conventions

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
  exhaustive `match`), the preferred encoding since plato-232. Payload-free variants are
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
  "none". `ItemIndex` (numbers.plato) is the general-purpose form for caller-supplied or pool
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
- **Naming:** domain declaration files are `domain.plato`; pure concept files are
  `domain.concepts.plato`; library files are `domain.library.plato`. There are no numeric
  prefixes; reading order lives in this README's layer table below.

## Layers and file map

This ordered index is now the canonical reading order.

| Layer | Files | Owner |
|-------|-------|-------|
| Foundation | primitives, core.concepts, algebra.concepts, collections.concepts, functional.concepts, numbers, quantities, time, vectors, matrices, rotations, points, intervals-bounds, transforms, color | core |
| Geometry primitives | geometry.concepts, lines, planar-shapes, spatial-primitives, polygons | agent A |
| Curves, surfaces, solids | curves-surfaces.concepts, curves-2d, curves-3d, splines, surfaces, solids | agent B |
| Fields, implicits, noise, sampling | fields, implicit-sdf, noise, sampling-grids | agent C |
| Topology, meshes, spatial structures | topology, meshes, mesh-attributes, pointclouds-voxels, spatial-structures, spatial-queries | agent D |
| Animation & motion | easing, keyframes-tracks, skeletal-animation, motion-graphics | agent E |
| Vector graphics & text | paths, vector-styling, text, scene2d | agent F |
| Color science & imaging | color-spaces, images, image-processing, texturing | agent G |
| Rendering | cameras, lights, materials, scene3d, render-settings | agent H |
| Physics & simulation | kinematics, rigid-dynamics, collision, joints-constraints, particles-simulation | agent I |
| Math, statistics, signals | statistics, random, signals, polynomials, optimization, uncertainty | agent J |
| Advanced & applied | differential-geometry, graphs, engineering, scientific-data, geo-spatial, higher-dimensions, intrinsics | agent K |
| Concept libraries (bodies) | core-algebra, collections-functional, numeric-structures, intervals-transforms, geometry, curves-surfaces, fields-implicits, meshes-spatial, domain-traits (all `.library.plato`) | see [`LIBRARIES.md`](LIBRARIES.md) |

The nine `*.library.plato` files hold the `library` blocks that implement derived functionality
on the concepts declared in the layers above (P1–P9 work packages). They are co-located here, not
in a subfolder. Ground rules and the package-to-concept table live in [`LIBRARIES.md`](LIBRARIES.md).
`transforms.plato` additionally carries its own inline `library Transforms`.

Foundation files:

- `primitives.plato` — compiler-assumed primitives, tuples, functions, arrays.
- `core.concepts.plato` — Equatable, Value, Hashable, Orderable, Comparable, Logical, Bitwise.
- `algebra.concepts.plato` — Additive..Arithmetic, ScalarArithmetic, Interpolatable, Numerical, Real, Whole, Normed, MetricSpace, Lattice, Difference.
- `collections.concepts.plato` — Countable, Index, Indexable family, Sliceable, Concatenable, SetLike, MapLike.
- `functional.concepts.plato` — Procedural, Bijective, Periodic, Boundable.
- `numbers.plato` — Complex, Rational, Proportion, Percent, Probability, ItemIndex, Cardinal.
- `quantities.plato` — Quantity concept, Dimension/UnitOfMeasure/DynamicQuantity, ~35 physical quantity types.
- `time.plato` — Instant, Duration, TimeInterval, FrameRate, FrameTime, Timecode, Tempo, BeatTime.
- `vectors.plato` — Vector concept, Number2/3/4/8, Vector2D/3D, VectorN, IntegerVector2/3/4, Direction2D/3D.
- `matrices.plato` — Matrix concept, Matrix2x2..4x4, Matrix3x2, Matrix4x3, SymmetricMatrix3x3, MatrixN, Tensor.
- `rotations.plato` — Quaternion, AxisAngle, EulerAngles, RotationOrder, Rotation2D, Rotor2D/3D, Bivector2D/3D.
- `points.plato` — Coordinate concept, Point2D/3D, PointN, homogeneous points, polar/cylindrical/spherical/barycentric, UvCoordinate, UvwCoordinate, GeoCoordinate.
- `intervals-bounds.plato` — Interval concept, NumberInterval, AngleInterval, Bounds concept, Bounds2D/3D, IntegerBounds2D/3D, Size2D/3D, IntegerSize2D/3D, Rect2D.
- `transforms.plato` — Transformable/Deformable concepts, Pose2D/3D, Transform2D/3D (TRS), affine/projective transforms, Frame2D/3D, Basis3D, Motor3D (dual quaternion), plus `library Transforms`: conversions between all transform representations, `p.Transform(t)` application, Compose/Inverse/Identity, and the Point2D/3D Difference + Lerp implementations.
- `color.plato` — Color (linear RGBA), Color8, ColorHSV, ColorHSL, ColorStop, ColorGradient.

## Cross-domain name registry

Types/concepts referenced across blocks. The **owner declares**; everyone else references.
Never re-declare a registry name. If you need something similar, qualify the name with your
domain (`ImageHistogram`, not a second `Histogram`).

| Name | Owner file |
|------|-----------|
| Foundation files (primitives…color) | foundation |
| `Geometry`, `Geometry2D/3D/ND`, `Bounded2D/3D`, `PointSet2D/3D` concepts | geometry.concepts.plato (A) |
| `Line2D/3D`, `Ray2D/3D`, `LineSegment2D/3D`, `Plane`, `HalfSpace` | lines.plato (A) |
| `Triangle2D`, `Quad2D`, `Circle`, `Ellipse`, `Capsule2D`, `RegularPolygon` | planar-shapes.plato (A) |
| `Sphere`, `Box3D`, `Cylinder`, `Cone`, `Capsule3D`, `Torus`, `Ellipsoid`, `Triangle3D`, `Quad3D`, `Tetrahedron` | spatial-primitives.plato (A) |
| `Polygon2D`, `Polygon3D`, `Polyline2D`, `Polyline3D`, `PolygonWithHoles2D` | polygons.plato (A) |
| `Curve1D/2D/3D`, `ClosedCurve2D/3D`, `Surface`, `ParametricSurface`, `Solid` concepts | curves-surfaces.concepts.plato (B) |
| `CircularArc2D`, `QuadraticBezier2D`, `CubicBezier2D` | curves-2d.plato (B) |
| `CubicBezier3D`, `Helix` | curves-3d.plato (B) |
| `BSplineCurve2D/3D`, `NurbsCurve2D/3D`, `HermiteCurve2D/3D`, `CatmullRomCurve2D/3D` | splines.plato (B) |
| `NurbsSurface`, `BezierPatch`, `SurfaceOfRevolution`, `ExtrudedSurface` | surfaces.plato (B) |
| `ScalarField2D/3D`, `VectorField2D/3D`, `SignedDistanceField2D/3D` concepts | fields.plato (C) |
| `VertexIndex`, `EdgeIndex`, `FaceIndex`, `CornerIndex`, `HalfEdgeIndex`, `VertexPair` | topology.plato (D) |
| `TriangleMesh3D`, `QuadMesh3D`, `PolygonMesh3D`, `LineSet3D`, `PointCloud3D`, `TriangleFace` | meshes.plato (D) |
| `RayHit2D`, `RayHit3D` | spatial-queries.plato (D) |
| `ClassicEasing`, `SpringParameters`, `Keyframe<T>`, `AnimationTrack<T>`, `AnimationClip` | easing.plato, keyframes-tracks.plato (E) |
| `Bone`, `Skeleton`, `SkeletonPose` (skeletal anim; physics never uses bare `Joint`) | skeletal-animation.plato (E) |
| `Path2D`, `PathSegment2D` | paths.plato (F) |
| `StrokeStyle`, `FillStyle` | vector-styling.plato (F) |
| `Image` concept, `Bitmap`, `PixelFormat` | images.plato (G) |
| `BlendMode` | image-processing.plato (G) |
| `Texture2D`, `Texture3D`, `TextureCube`, `TextureSampler`, `TextureBinding` | texturing.plato (G) |
| `PerspectiveCamera`, `OrthographicCamera` | cameras.plato (H) |
| `Material` (rendering PBR material) | materials.plato (H) |
| `RigidBody2D/3D`, `MassProperties2D/3D`, `BodyIndex` | rigid-dynamics.plato (I) |
| `Histogram`, `SummaryStatistics` | statistics.plato (J) |
| `RandomState`, `NormalDistribution`, `UniformDistribution` | random.plato (J) |
| `Spectrum`, `SampledSignal` | signals.plato (J) |
| `Polynomial` | polynomials.plato (J) |
| `Tolerance` | uncertainty.plato (J) |
| `Graph`, `GraphEdge` | graphs.plato (K) |
| `List<T>`, `Buffer<T>` (unique affine builders) | primitives.plato (foundation) |
| `RegularPyramid`, `SquarePyramid` | solids.plato (B) |
| `CylindricalShell` | spatial-primitives.plato (A) |
| `ScalarFunctionField2D/3D`, `VectorFunctionField2D/3D` | fields.plato (C) |
| `FunctionSdf2D/3D`, `FunctionRegion2D`, `FunctionVolume3D` | implicit-sdf.plato (C) |
| `CycloidOfCeva2D`, `TschirnhausenCubic2D`, `ConchoidOfDeSluze2D`, `SinusoidalSpiral2D`, `TrisectrixOfMaclaurin2D`, `ButterflyCurve2D` | curves-2d.plato (B) |

Generic nouns that MUST be domain-qualified wherever declared: Node, Layer, Track, Channel,
Sample, Grid, Cell, Filter, Kernel, Key, Frame, Edge, Vertex, Face, Segment, Weight, Style,
Event, Marker, Anchor, Handle, Buffer, Attribute, Region, Mask, Map, Range, Wave, State.
(Exception: registry entries above that already claim a bare name.)

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
`Color8.AliceBlue`. This matches the intrinsic constants in `intrinsics.plato` and the
`Identity(_: Quaternion)` family in `transforms.plato`.

`Angle` values are constructed through the unit constructors in `angles.library.plato` —
`0.25.Turns`, `90.Degrees`, `100.Gradians` — which is how the "angles are `Angle`, never raw
`Number`" rule above is satisfied in practice. The radians intrinsic is not the only door.
