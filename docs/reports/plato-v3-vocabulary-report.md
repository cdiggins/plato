# Plato v3 vocabulary report

**Date:** 2026-07-27
**Subject:** [`submodules/Plato/stdlib/`](../submodules/Plato/stdlib/) (commits `7cec1e3`, `72912d8`)
**Compared against:** [`stdlib-legacy/`](../submodules/Plato/stdlib-legacy/) (production, "v1") and [`plato-src-v2/`](../submodules/Plato/plato-src-v2/) (prototype, plato-228)
**Companion doc:** [reports/plato-source-vocabulary-comparison.md](plato-source-vocabulary-comparison.md) (v1 vs v2)

This report was produced by an AI agent that also authored v3. Treat the counts as
mechanical (they are script-derived) and the judgements as arguable.

---

## 1. Headline numbers

| | stdlib-legacy (v1) | plato-src-v2 | stdlib |
|---|---|---|---|
| Files | 28 | 67 | 70 |
| Lines | 4,465 | 2,893 | 13,406 |
| Capability declarations | 81 `interface` | 332 `interface` | **154 `interface`** |
| Data declarations | 168 `type` | 606 `type` | **1,125 `type`** |
| Total declarations | 249 | 938 | **1,279** |
| `library` blocks | 22 | 0 | 0 |
| Library functions | ~1,150 | 0 | 0 |
| Executable? | yes | no | no |

**The answer to "how many interfaces and types": 154 interfaces and 1,125 types, 1,279 declarations total.**

Name-set comparison (case-sensitive, `I`-prefix normalized away so `IGeometry` ≡ `Geometry`):

| Relation | Count |
|---|---|
| Names in v3 that appear in neither v1 nor v2 | 964 |
| Names in v3 carried over from v1 and/or v2 | 315 |
| v1 names absent from v3 | 132 |
| v2 names absent from v3 | 595 |

v3 is not a superset of v2. It is a re-derivation that kept roughly a third of v2's names,
discarded the rest, and added ~950 new ones.

---

## 2. Structural profile of v3

| Block | Interfaces | Types | Lines |
|---|---|---|---|
| Foundation (primitives…color) | 49 | 162 | 1,448 |
| Geometry primitives (geometry.interfaces…polygons) | 29 | 56 | 862 |
| Curves / surfaces / solids (curves-surfaces.interfaces…solids) | 21 | 90 | 1,202 |
| Fields / SDF / noise / sampling (fields…sampling-grids) | 25 | 80 | 1,114 |
| Topology / meshes / spatial (topology…spatial-queries) | 15 | 95 | 1,254 |
| Animation / motion (easing…motion-graphics) | 2 | 68 | 749 |
| Vector graphics / text (paths…scene2d) | 1 | 54 | 617 |
| Color / imaging (color-spaces…texturing) | 2 | 94 | 1,004 |
| Rendering (cameras…render-settings) | 2 | 86 | 1,106 |
| Physics / simulation (kinematics…particles-simulation) | 4 | 97 | 1,314 |
| Math / statistics / signals (statistics…uncertainty) | 1 | 116 | 1,236 |
| Advanced / applied (differential-geometry…higher-dimensions, intrinsics) | 3 | 127 | 1,500 |

Other measured properties:

- 46 of 154 interfaces are **markers** (no functions); 108 declare at least one function.
- Total interface functions: **181**. That is roughly one function per seven declarations.
- 115 types are `*Kind` enumerations, all following the documented `Value: Integer` pattern.
- 274 types have exactly one field; average field count is 2.9; the widest is 10 (the cap).
- 179 types declare no `implements` clause at all.
- 47 of 154 interfaces are never implemented or inherited anywhere in the library.

---

## 3. What is new

**Whole domains that neither predecessor had.** v1 covered geometry, vectors, transforms,
colors, meshes, curves, and SDFs. v2 added sketch-level coverage of animation, imaging,
rendering, physics, statistics, and optimization. v3 adds, at genuine catalog depth:

- **Vector graphics and typography** (paths.plato, vector-styling.plato, text.plato, scene2d.plato) — SVG-shaped path segments, contours,
  fill rules, stroke/dash/join/cap styling, three gradient paints, font faces, text runs,
  glyph placement, text-on-path, and a 2D scene graph. Neither predecessor had any of this.
- **Motion graphics** (motion-graphics.plato) — After-Effects-shaped layer transforms, tweens, timelines,
  stagger, oscillators, wiggle, camera shake, time remapping, beat sync.
- **Color science** (color-spaces.plato) — chromaticity, white points, transfer functions, RGB primaries
  and named color spaces, XYZ/Lab/LCh/Luv/OkLab/OkLCh/YUV/YCbCr/CMYK/HWB, harmony and
  difference kinds. v1 had `ColorLAB`/`ColorLUV` and little else.
- **Image processing** (image-processing.plato) — convolution kernels, blur/sharpen/edge/morphology/threshold
  parameter records, tone curves, levels/curves/color-matrix adjustments, the full
  Porter-Duff and blend-mode enumerations, resampling and dither kinds.
- **Noise** (noise.plato) — Perlin, simplex, value, Worley, Gabor, fBm, turbulence, ridged, domain
  warp, and curl in 2D and 3D, each wired to the field interfaces.
- **Engineering** (engineering.plato) — material properties, section profiles and their properties, beam
  supports and loads, fits and tolerances, gears, bolted joints, springs, pressure vessels.
- **Geo-spatial** (geo-spatial.plato) — geodetic datums, reference ellipsoids, map projections, ECEF/ENU
  coordinates, tile indices, elevation grids.
- **Higher dimensions** (higher-dimensions.plato) — 4D bivectors and rotors, hyperspheres, hyperplanes, the six
  regular 4-polytopes, 4D→3D projection kinds, plus quaternion-Julia and Mandelbrot views.

**New foundation machinery:**

- A ~45-member **quantity catalog** with natural-unit field names (`Angle.Radians`,
  `Force.Newtons`, `Temperature.Kelvin`) plus a runtime `Dimension`/`UnitOfMeasure`/
  `DynamicQuantity` triple for unit systems resolved at load time.
- **Geometric algebra** types alongside quaternions: `Bivector2D/3D`, `Rotor2D/3D`,
  extended to `Bivector4D`/`Rotor4D` in higher-dimensions.plato.
- **Typed topology indices** (`VertexIndex`, `EdgeIndex`, `FaceIndex`, `CornerIndex`,
  `HalfEdgeIndex`, `CellIndex`) replacing raw `Integer` indices — this is the fix
  [ara3d-032](../tracker/issues/ara3d-032.md) asks for, at the vocabulary level.
- **Time vocabulary** covering production work: `FrameRate`, `FrameTime`, `Timecode`,
  `Tempo`, `BeatTime`, alongside `Instant`/`Duration`/`TimeInterval`.
- A documented **kind pattern** (`type FooKind { Value: Integer; }` with value meanings in
  the doc comment) giving Plato a consistent enumeration idiom it never had.

**New process artifacts:** the folder ships a `README.md` that states conventions and holds
a cross-domain name registry. That registry is what let eleven agents write 55 files
concurrently with only three name collisions, all caught and removed before commit.

---

## 4. What has changed

**Keyword and naming.** v1 and v2 both used `interface` with `I`-prefixed names. v3 uses
`interface` with bare names. Both spellings are accepted by the parser; `interface` is the term
the standard-library sources and the language documentation actually use for type classes.

**Documentation density.** v3 is 13,406 lines for 1,279 declarations (10.5 lines each);
v2 was 2,893 lines for 938 declarations (3.1 lines each). Every v3 declaration carries a
doc comment stating semantics, invariants, units, and sentinel conventions. This is the
single largest difference in the source as text, and the main reason v3 is 4.6× v2's size
for 1.4× the declarations.

**Algebra decomposed further.** v1 had a moderately deep tower (`IValue` → `IVectorLike` →
`INumerical` → `IVector`). v3 splits capabilities smaller — `Additive`, `Multiplicative`,
`Divisible`, `Modular`, `Invertible`, `Scalable`, `Interpolatable`, `NumericalLimits`,
`Normed`, `MetricSpace`, `Lattice`, `Clampable`, `Difference<T>` — and composes them.
`Arithmetic` and `Numerical` survive as composition points rather than as the primary tower.

**Matrices are row-vector records.** v1's `Matrix4x4` was a wrapper over intrinsics; v2's
was elementwise. v3 stores `Row1..Row4: Vector4`. This was forced by a compiler constraint
(see §6) but is arguably the better shape: row extraction is free and the type composes
with the vector interfaces.

**Points versus vectors is now enforced by the `Difference<T>` interface**, as in v2, rather
than by convention as in v1. `Point3D implements Difference<Vector3>`; subtracting two
points yields a vector, and adding a vector to a point yields a point.

**Dimension-suffix discipline.** v1 mixed `Sphere` (3D, unsuffixed) with `Circle2D`-style
names inconsistently, and used `IShape2D`/`IShape3D` parallel hierarchies. v3 keeps
unsuffixed names for shapes that exist in only one dimension (`Sphere`, `Torus`, `Plane`)
and suffixes only where a genuine 2D/3D pair exists (`Triangle2D`/`Triangle3D`).

**Scene graphs are flat and index-based.** Both the 2D (`Scene2D`) and 3D (`Scene3D`)
scenes store nodes in an array with `Parent: Integer` and `-1` sentinels, with resources in
parallel typed arrays. This is a deliberate move away from recursive/reference structures,
which Plato's value semantics do not express well.

---

## 5. What has been removed

**Executable behavior — the biggest loss, and it is by design.** v1's 22 `library` blocks
hold roughly 1,150 functions: `core.library` (96), `geometry.library` (122), `vectors` (98),
`meshes.library` (75), `curves` (74), `transforms` (48), `solids` (36), `intrinsics` (398),
plus constants. v3 has none. v3 is a vocabulary, not a library; nothing in it computes.

**132 v1 names are absent from v3.** The substantive groups:

- **The named analytic curve catalog** — `Epicycloid`, `Hypotrochoid`, `Cardoid`,
  `Limacon`, `Rose`, `LemniscateOfBernoulli`, `FermatsSpiral`, `TrisectrixOfMaclaurin`,
  `TschirnhausenCubic`, `ConchoidOfDeSluze`, `CycloidOfCeva`, `ButterflyCurve`,
  `SinusoidalSpiral`, and more. v3 kept a smaller set (`ArchimedeanSpiral2D`,
  `LogarithmicSpiral2D`, `RoseCurve2D`, `Cycloid2D`, `Lissajous2D`, `Catenary2D`,
  `Clothoid2D`, `CircleInvolute2D`, `Superformula2D`) and dropped the long tail.
- **Convenience transform types** — `Translation3D`, `Scaling3D`, `Rotation3D`,
  `Reflection2D`, `Skew2D`, `UniformScale2D`, `LookAt3D`, `Perspective3D`,
  `Orthographic3D`, `PlaneProjection3D`, `IdentityTransform3D`. v3 has the general forms
  (`Transform3D`, `AffineTransform3D`, `ProjectiveTransform3D`) but no named special cases.
  Note that `LookAtCamera` and the projection kinds reappear in the rendering block, so the
  loss is partial and slightly inconsistent.
- **The component protocol** — `IArrayLike<T>` with `NumComponents`, `Components`,
  `CreateFromComponents`, `CreateFromComponent`. v3 has `Indexable<T>` (read side) but
  **nothing on the construction side**. This is discussed in §8; it is the most
  consequential omission.
- **`Integer2`/`Integer3`/`Integer4`** — replaced by `IntegerVector2/3/4`, a rename rather
  than a removal.
- **`Function5` through `Function9`, `Vector8`** — arity ceilings lowered.
- **Coordinate variants** — `LogPolarCoordinate`, `HorizontalCoordinate`,
  `GeoCoordinateWithAltitude` (folded into `GeoCoordinate`, which now carries altitude).
- **`Time` as a measure** — replaced by the richer `Duration`/`Instant` split.
- **The primitive-collection interfaces** — `IPointGeometry2D/3D`,
  `IPrimitiveGeometry2D/3D`, `IIndexedGeometry3D`, `IGeometricPrimitive2D/3D`,
  `Points2D/3D`, `Lines2D/3D`, `Triangles3D`, `Quads3D`, `QuadGrid3D`, `LineMesh3D`.
  v3 covers this ground with `PointSet2D/3D`, `PointCloud2D/3D`, `LineSet2D/3D`,
  `TriangleMesh3D`, `QuadMesh3D`, so it is a redesign rather than a gap — except that the
  generic `IPrimitiveGeometry<T>` abstraction has no v3 counterpart.

**595 v2 names are absent from v3.** Most of this is v2's speculative surface that v3
deliberately did not reproduce: `ArbitraryPrecision`, `BigInteger`, `DualNumber`,
`Bicomplex`, `FixedPoint`, `DecimalNumber`, the `Atom3D`/`Molecule3D`/`CrystalStructure`
chemistry set, `ExperimentRun`/`Ensemble`/`MonteCarloSample`, the B-rep family
(`BrepFace3D`, `BrepLoop3D`, `BrepShell3D`), `FiniteElement`/`ShellElement`/`SolidElement`,
`SparseMatrix`/`BandedMatrix`/`EigenSystem`/`LinearSystem`, and the `StridedSlice`/
`StridesND`/`ArrayND` tensor-view machinery. Some of these are real gaps (see §8);
most were correctly judged out of scope for a first vocabulary.

---

## 6. Compiler constraints discovered

Two findings worth recording, both hit during v3 authoring:

1. **Types are limited to 10 fields.** The compiler synthesizes a `TupleN` constructor for
   every N-field type, and tuple support stops at `Tuple10`. Exceeding it produces
   `Value cannot be null. (Parameter 'def')` from `TypeResolver`/`TypeExpression` — a
   message that names neither the offending type nor the real cause. If a `Tuple11+`
   declaration exists the error improves to `Only tuples up to 10 fields are supported`.
   This is what forced matrices into row-vector form. **Recommendation:** make the
   diagnostic name the type and field count; consider raising the ceiling or suppressing
   tuple-constructor synthesis above it.
2. **Lint reports declaration-only sources as ~4,600 findings** (LINT001 ×1,481,
   LINT003 ×3,103), all of the form "never implemented" / "never read". These are correct
   for a vocabulary with no libraries, but they make the linter useless as a gate here.
   **Recommendation:** a `--declarations-only` mode, or per-file suppression.

---

## 7. Areas that may be over-specified

**Enumerations promoted to types (115 `*Kind` types).** Every enumeration costs a type
declaration, a doc comment listing integer meanings, and — once codegen runs — a generated
struct. `EasingKind` alone encodes 31 values in a comment. The information is real but the
representation is heavy, and the meanings live in prose that no checker validates.
This is a language gap (Plato has no enum) being paid for 115 times.

**Parameter and settings records (43 `*Parameters`, 15 `*Settings`).** `BoxBlurParameters`,
`GaussianBlurParameters`, `MotionBlurParameters`, `UnsharpMaskParameters`,
`DenoiseParameters`, `VignetteParameters`, `ChromaticAberrationParameters` and so on are
essentially argument bundles for functions that do not exist yet. Until the functions are
written, we cannot know whether those groupings are right. They are speculative in the
precise sense that plato-228's "case against" warned about.

**The physics block (97 types, 4 interfaces).** Nine 3D joint types, three 2D joint types,
`RagdollProfile`, `SphParameters`, `SoftBodySettings`, `Rope3D` — this is engine-shaped
API surface for an engine Plato does not have and, being pure and immutable, may never
have in this form. A rigid-body solver needs mutable state and a broadphase; the vocabulary
implies a runtime that the language does not support.

**The rendering block (86 types, 2 interfaces).** `RenderPipelineSettings`,
`FrameStatistics`, `RenderTargetDescriptor`, `VertexLayout`, `DisplayColorSpaceKind` are
GPU-pipeline description, not geometry. They belong to a renderer's configuration schema.
Their presence in a pure geometry-and-math vocabulary is defensible only if Plato is meant
to emit render-graph descriptions as data.

**Analytic-curve breadth versus depth.** curves-2d.plato has 23 curve types with no evaluation
functions. One `CubicBezier2D` with a real `Eval` is worth more today than 23 curves
without one.

**Near-duplicate interfaces across blocks.** `MorphTarget` (skeletal-animation.plato) and `MorphTarget3D` (mesh-attributes.plato),
`Pyramid` (solids.plato) and `Pyramid3D` (polygons.plato), `Prism` (solids.plato) and `Prism3D` (polygons.plato),
`SampledSdf3D` (implicit-sdf.plato) versus `LevelSetGrid3D` (pointclouds-voxels.plato), `SdfNode3D`/`SdfTree3D` (implicit-sdf.plato) versus
`CsgNode3D`/`CsgTree3D` (solids.plato). All are distinct names so nothing collides, but each pair is
a decision deferred rather than made.

---

## 8. Areas that are under-specified

**The construction side of the component protocol is missing, and this is the important
one.** v1's `IArrayLike<T>` supplies `Components`, `CreateFromComponents`, and
`CreateFromComponent`; `ArrayLibrary` then defines `MapComponents`, `ZipComponents`, and
`Reverse` generically for *every* vector-like type in one place. v3's `Vector` interface
inherits `Indexable<Number>` — read-only — so no generic component-wise function can be
written against it. Any library pass on v3 will have to add construction back before it can
write a single generic numeric algorithm. **This should be fixed in the vocabulary, not
worked around later.**

**Interface density collapses outside geometry.** Foundation plus the four geometry blocks
hold 124 of 154 interfaces (81%) against 483 types. The seven applied blocks — animation,
vector graphics, color/imaging, rendering, physics, math/statistics, advanced — hold **15
interfaces against 642 types**. Those blocks are record catalogs with almost no capability
abstraction. Concretely: nothing in the animation block abstracts "a thing that can be
sampled at a time" beyond one `TimeVarying<T>`; nothing in imaging abstracts "a thing with
pixels I can read" beyond a three-function `Image`; nothing in statistics abstracts
"a thing I can accumulate samples into". These are the reusable capabilities the library
was supposed to provide.

**47 interfaces are declared but never used** — `Transformable<T>` (zero implementers,
despite being the interface the whole geometry library exists to satisfy), `MetricSpace`,
`Clampable`, `SetLike`, `MapLike`, `StackLike`, `QueueLike`, `Sliceable`, `Concatenable`,
`Bijective`, `ParameterDomain`, all seven differentiable-field interfaces, all four
time-varying-field interfaces, both `Kinematic2D/3D`, both `ForceModel2D/3D`. Either the
concrete types should implement them or they should go.

**Conversions are entirely absent, and they are where the value is.** The library declares
`ColorXYZ`, `ColorLab`, `ColorOkLab`, `ColorSRGB`, `RgbColorSpace`, `WhitePointKind`, and
`ChromaticAdaptationKind` — everything needed to *describe* a color conversion and nothing
that performs one. The same holds for `PolarCoordinate` ↔ `Point2D`, `Quaternion` ↔
`EulerAngles` ↔ `Matrix3x3`, `GeoCoordinate` ↔ `EcefCoordinate`, quantity unit changes,
and mesh representation changes (`HalfEdgeMesh` ↔ `TriangleMesh3D`). A conversion interface
(`ConvertibleTo<T>`) or a documented naming convention would at least mark the intent.

**No constants.** v1 shipped `constants.plato` (35 functions) and `colors.constants.plato`
(141 named colors). v3 has no `Pi`, no `Epsilon`, no identity transform, no named color.
Some of this is unavoidable in a declaration-only pass — constants are functions — but the
absence should be tracked, because every consumer will need them immediately.

**Numeric-tower gaps.** No fixed-point, arbitrary-precision, decimal, or dual numbers
(v2 had all four; dual numbers in particular are how you get automatic differentiation,
which the differentiable-field interfaces silently assume someone will provide).

**Sparse and structured linear algebra.** `MatrixN` is dense; `Tensor` is dense. v2 had
`SparseMatrix`, `BandedMatrix`, `SymmetricTensor`, `EigenSystem`, `LinearSystem`. Anything
FEA-shaped, and the `KalmanFilterParameters` already declared in uncertainty.plato, needs these.

**No laws or witnesses.** `stdlib-legacy-tests` holds `Law_*`/`Witness_*` functions that state
what an interface's implementations must satisfy. v3 declares 181 interface functions with
zero laws. `Additive` does not say addition is associative; `Orderable` does not say the
order is total. The doc comments assert these properties in prose where the existing test
machinery could check them.

**Error and partiality handling.** Nothing expresses "this operation may fail":
no `Option`/`Maybe`, no `Result`. `RootFindResult` and `OptimizationResult` each invent
their own `converged: Boolean` convention; `RayHit3D` carries a `Hit: Boolean`. A shared
partiality vocabulary would remove that duplication.

---

## 9. Possible improvements

Ranked by value against effort, highest first.

1. **Restore component construction.** Add `CreateFromComponents`/`CreateFromComponent` to
   the `Vector` interface (or a new `ComponentConstructible<T>`). Roughly ten lines; unblocks
   every generic numeric library function. Do this before any library work starts.
2. **Prove the vocabulary with one vertical slice.** Pick a narrow path — `Point3D`,
   `Vector3`, `Transform3D`, `TriangleMesh3D`, `Transformable` — and write the libraries
   for it end to end, compiling to C#. That will surface shape errors that no amount of
   review will. Expect it to change the foundation.
3. **Delete or implement the 47 unused interfaces.** Start with `Transformable<T>`: either
   every geometry type implements it or it is not the abstraction we think it is.
4. **Add the missing capability interfaces in the applied blocks.** Even five per block
   (samplable, accumulable, blendable, resamplable, convertible) would move those 642 types
   from a catalog toward a library.
5. **Add laws for the foundation interfaces** using the existing `stdlib-legacy-tests` machinery.
   The algebra tower is small enough to make this cheap and it is exactly where correctness
   claims matter.
6. **Reconcile the near-duplicates** listed in §7 into single declarations.
7. **Decide the fate of the enumeration pattern.** Either accept 115 kind types as the
   idiom and generate them consistently, or propose a real `enum` construct for the
   language. The current state pays the cost of enums without the checking.
8. **Improve the two compiler diagnostics** in §6 — the field-count error message and a
   declarations-only lint mode.
9. **Decide the fate of v2.** Two prototype vocabularies now sit beside the production
   library; [plato-229](../../tracker/issues/plato-229.md) is still open against v2. Keeping
   both is confusing and the name "v2" already collides with the V2 codegen recipe.
10. **Scope the outer blocks explicitly.** If rendering and physics are not going to be
    implemented in Plato, mark them as a description schema, move them to a separate
    folder, or drop them. They are 183 types of maintenance obligation.

---

## 10. Questions to be asked

**Purpose and scope**

1. Is v3 meant to become the production library, to inform a refactor of `stdlib-legacy`, or to
   stay a reference catalog? The answer changes almost everything below.
2. Should Plato own rendering-pipeline, physics-engine, and typography vocabularies at all,
   or should those live in consuming systems (Ara3D.Studio, a renderer) with Plato
   supplying only the geometry and math they build on?
3. Is a declaration-only library valuable on its own, or is its value entirely contingent
   on the libraries that would implement it?

**Design**

4. Should the vocabulary be split into tiers — a small mandatory kernel plus optional
   domain packs — so a consumer can take geometry without taking chemistry-adjacent
   engineering types?
5. Is the flat index-based encoding (parent indices, CSR offsets, `-1` sentinels) the right
   general answer for graph-shaped data in a value language, or should the language grow a
   reference or handle interface?
6. Should `interface` functions carry default implementations? Much of the duplication across
   blocks exists because an interface can only declare, never provide.
7. Do we want an `enum` construct, given that 115 types are simulating one?
8. Should quantities be a generic `Quantity<TDimension>` rather than ~45 hand-written
   types? The current design is explicit and readable but cannot express derived units
   (`Force` divided by `Area` is `Pressure`) without a combinatorial function catalog.
9. Are `Proportion`, `Percent`, and `Probability` — three single-field wrappers over a
   `[0,1]`-ish `Number` — pulling their weight, or is one refinement type enough?

**Process**

10. What is the migration path from `stdlib-legacy` to v3 naming, given that `Plato.Generated`
    is consumed by `Ara3D.Geometry` and the ara3d-sdk? Is a rename map needed, or does v3
    live alongside indefinitely?
11. Should the parallel-agent authoring method (README registry, isolated per-block lint,
    integration sweep) be written down as a repeatable process? It worked — eleven agents,
    55 files, three collisions — and it is reusable for the library pass.
12. What is the acceptance test for "this vocabulary is good"? Coherence review, a
    compiling vertical slice, a port of an existing algorithm, or consumer adoption?

---

## 11. Validation status

- `Plato.CLI lint stdlib`: 0 parse errors, 0 symbol-resolution errors, no exceptions.
- 0 duplicate declaration names across all 70 files.
- No type exceeds the 10-field compiler limit.
- 4,584 LINT001/LINT003 findings, all expected for a declaration-only source (see §6).
- Never compiled to C#, TypeScript, or Rust. Never executed. No laws, no tests, no consumers.
