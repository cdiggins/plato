# plato-src-v3 — comprehensive type & concept vocabulary

Third-generation Plato vocabulary. Successor to the `plato-src-v2` prototype (plato-228):
much broader coverage, `concept` keyword with bare names (no `I` prefix), and files grouped
by domain in dependency-layer order. **Declarations only** — no `library` blocks, no
function bodies. Semantics live in doc comments; implementations come in a later pass.

Target applications: geometry (primary), 2D/3D/4D and N-dimensional computation, animation,
numerical/mathematical/scientific computing, graphics and rendering, physics, motion
graphics, image processing, and engineering.

Current contents (2026-07-27): **70 source files, 154 concepts, 1125 types** (~13.4K lines).
The folder parses and resolves cleanly (`lint`: 0 parse errors, 0 symbol-resolution errors);
LINT001/LINT003 findings are expected until libraries implement the declared surface.

## Validation

```
dotnet <path-to>/Plato.CLI.dll lint plato-src-v3
```

The folder must parse and resolve with zero errors. It is self-contained (declares its own
primitives in `00-primitives.plato`).

## Conventions

- `concept` = capability/trait (like a type class over `Self`). Bare PascalCase names, no
  `I` prefix: `Numerical`, `Curve3D`, `Transformable<T>`.
- `type` = immutable data, fields only. Concrete nouns: `Vector3`, `TriangleMesh3D`.
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
  survives only on the ~100 not-yet-migrated enum-style `XxxKind` types, pending the
  follow-up sweep — new declarations should use a sum, not a kind.
- Quantity types carry natural-unit field names (`Radians`, `Meters`, `Kelvin`).
- Collection fields use `Array<T>` (or `Array2D<T>`, `Array3D<T>`).
- Optional references use sentinel conventions noted in comments (e.g. `-1` for "no index",
  empty array for "none").
- **Hard limit: at most 10 fields per type** (the compiler synthesizes a `TupleN`
  constructor per type and supports tuples only up to `Tuple10`). Split bigger records
  into nested component types (e.g. matrices store row vectors).
- Angles are `Angle`, never raw `Number`. Distances/positions in unit-bearing contexts may
  still use `Number` when the domain is unit-agnostic (pure math), `Length` when physical.

## Layers and file map

| Layer | Files | Owner |
|-------|-------|-------|
| Foundation | 00-14 | core |
| Geometry primitives | 15-19 | agent A |
| Curves, surfaces, solids | 20-25 | agent B |
| Fields, implicits, noise, sampling | 26-29 | agent C |
| Topology, meshes, spatial structures | 30-35 | agent D |
| Animation & motion | 36-39 | agent E |
| Vector graphics & text | 40-43 | agent F |
| Color science & imaging | 44-47 | agent G |
| Rendering | 48-52 | agent H |
| Physics & simulation | 53-57 | agent I |
| Math, statistics, signals | 58-63 | agent J |
| Advanced & applied | 64-69 | agent K |

Foundation files:

- `00-primitives.plato` — compiler-assumed primitives, tuples, functions, arrays.
- `01-concepts-core.plato` — Equatable, Value, Hashable, Orderable, Comparable, Logical, Bitwise.
- `02-concepts-algebra.plato` — Additive..Arithmetic, ScalarArithmetic, Interpolatable, Numerical, Real, Whole, Normed, Metric, Lattice, Difference.
- `03-concepts-collections.plato` — Countable, Indexable family, Sliceable, Concatenable, SetLike, MapLike.
- `04-concepts-functional.plato` — Procedural, Bijective, Periodic, Boundable.
- `05-numbers.plato` — Complex, Rational, Proportion, Percent, Probability, Index.
- `06-quantities.plato` — Quantity concept, Dimension/UnitOfMeasure/DynamicQuantity, ~35 physical quantity types.
- `07-time.plato` — Instant, Duration, TimeInterval, FrameRate, FrameTime, Timecode, Tempo, BeatTime.
- `08-vectors.plato` — Vector concept, Vector2/3/4, VectorN, IntegerVector2/3/4, Direction2D/3D.
- `09-matrices.plato` — Matrix concept, Matrix2x2..4x4, Matrix3x2, Matrix4x3, SymmetricMatrix3x3, MatrixN, Tensor.
- `10-rotations.plato` — Quaternion, AxisAngle, EulerAngles, RotationOrder, Rotation2D, Rotor2D/3D, Bivector2D/3D.
- `11-points.plato` — Coordinate concept, Point2D/3D/4D, PointN, homogeneous points, polar/cylindrical/spherical/barycentric, UvCoordinate, UvwCoordinate, GeoCoordinate.
- `12-intervals-bounds.plato` — Interval concept, NumberInterval, AngleInterval, Bounds concept, Bounds2D/3D/4D, IntegerBounds2D/3D, Size2D/3D, IntegerSize2D/3D, Rect2D.
- `13-transforms.plato` — Transformable/Deformable concepts, Pose2D/3D, Transform2D/3D (TRS), affine/projective transforms, Frame2D/3D, Basis3D.
- `14-color.plato` — Color (linear RGBA), Color8, ColorHSV, ColorHSL, ColorStop, ColorGradient.

## Cross-domain name registry

Types/concepts referenced across blocks. The **owner declares**; everyone else references.
Never re-declare a registry name. If you need something similar, qualify the name with your
domain (`ImageHistogram`, not a second `Histogram`).

| Name | Owner file |
|------|-----------|
| Everything in files 00-14 | foundation |
| `Geometry`, `Geometry2D/3D/4D/ND`, `Bounded2D/3D`, `PointSet2D/3D` concepts | 15 (A) |
| `Line2D/3D`, `Ray2D/3D`, `LineSegment2D/3D`, `Plane`, `HalfSpace` | 16 (A) |
| `Triangle2D`, `Quad2D`, `Circle`, `Ellipse`, `Capsule2D`, `RegularPolygon` | 17 (A) |
| `Sphere`, `Box3D`, `Cylinder`, `Cone`, `Capsule3D`, `Torus`, `Ellipsoid`, `Triangle3D`, `Quad3D`, `Tetrahedron` | 18 (A) |
| `Polygon2D`, `Polygon3D`, `Polyline2D`, `Polyline3D`, `PolygonWithHoles2D` | 19 (A) |
| `Curve1D/2D/3D`, `ClosedCurve2D/3D`, `Surface`, `ParametricSurface`, `Solid` concepts | 20 (B) |
| `CircularArc2D`, `QuadraticBezier2D`, `CubicBezier2D` | 21 (B) |
| `CubicBezier3D`, `Helix` | 22 (B) |
| `BSplineCurve2D/3D`, `NurbsCurve2D/3D`, `HermiteCurve2D/3D`, `CatmullRomCurve2D/3D` | 23 (B) |
| `NurbsSurface`, `BezierPatch`, `SurfaceOfRevolution`, `ExtrudedSurface` | 24 (B) |
| `ScalarField2D/3D`, `VectorField2D/3D`, `SignedDistanceField2D/3D` concepts | 26 (C) |
| `VertexIndex`, `EdgeIndex`, `FaceIndex`, `CornerIndex`, `HalfEdgeIndex`, `VertexPair` | 30 (D) |
| `TriangleMesh3D`, `QuadMesh3D`, `PolygonMesh3D`, `LineSet3D`, `PointCloud3D`, `TriangleFace` | 31 (D) |
| `RayHit2D`, `RayHit3D` | 35 (D) |
| `EasingKind`, `SpringParameters`, `Keyframe<T>`, `AnimationTrack<T>`, `AnimationClip` | 36-37 (E) |
| `Bone`, `Skeleton`, `SkeletonPose` (skeletal anim; physics never uses bare `Joint`) | 38 (E) |
| `Path2D`, `PathSegment2D` | 40 (F) |
| `StrokeStyle`, `FillStyle` | 41 (F) |
| `Image` concept, `Bitmap`, `PixelFormatKind` | 45 (G) |
| `BlendModeKind` | 46 (G) |
| `Texture2D`, `Texture3D`, `TextureCube`, `TextureSampler`, `TextureBinding` | 47 (G) |
| `PerspectiveCamera`, `OrthographicCamera` | 48 (H) |
| `Material` (rendering PBR material) | 50 (H) |
| `RigidBody2D/3D`, `MassProperties2D/3D` | 54 (I) |
| `Histogram`, `SummaryStatistics` | 58 (J) |
| `RandomState`, `NormalDistribution`, `UniformDistribution` | 59 (J) |
| `Spectrum`, `SampledSignal` | 60 (J) |
| `Polynomial` | 61 (J) |
| `Tolerance` | 63 (J) |
| `Graph`, `GraphEdge` | 65 (K) |

Generic nouns that MUST be domain-qualified wherever declared: Node, Layer, Track, Channel,
Sample, Grid, Cell, Filter, Kernel, Key, Frame, Edge, Vertex, Face, Segment, Weight, Style,
Event, Marker, Anchor, Handle, Buffer, Attribute, Region, Mask, Map, Range, Wave, State.
(Exception: registry entries above that already claim a bare name.)
