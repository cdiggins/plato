# stdlib concept libraries — implementation bodies

The `*.library.plato` files in `stdlib/` hold `library` blocks that implement **derived
functionality for the concepts** declared in the domain declaration files. In Plato, a function
whose first parameter is concept-typed is like a C# extension method on an interface: it becomes
available on every type that implements the concept. These libraries implement
derived functionality on the concepts declared in the domain files — build on
good ideas from earlier vocabulary, replace bad names, drop dead weight.

The library files live side-by-side with the declaration files they implement (they were flattened
out of the old `concept-library/` subfolder). Because `lint` enumerates `*.plato` non-recursively
(`Plato.CLI/Program.cs:101` / `:197` use `GetFiles("*.plato", TopDirectoryOnly)`), the lint command
below now genuinely covers these library bodies — which it did **not** while they sat in a subfolder.

## Ground rules

1. **One `library` block per file**, named `<domain>.library.plato` where the domain matches
   the concept or declaration file it serves, with a PascalCase block name matching the stem
   (`collections-indexable.library.plato` holds `library CollectionsIndexable`).

   This is exact and has no exceptions:
   `grep -l "^library " *.plato` returns **exactly the 121 `*.library.plato` files** and
   nothing else. **No declaration file carries an inline `library` block.** The former inline
   blocks in `transforms`, `polynomials`, `solids`, `surfaces`, `implicit-sdf`, `primitives`,
   `intrinsics`, `curves-2d`, `curves-3d`, `splines` and `fields` were all moved into their own
   `*.library.plato` files; the sanctioned home for bodies that belong beside a declaration is
   now a sibling `.library.plato` file with the same stem, not the declaration file itself.

   A work *package* may therefore span several library files - P1 is five files, P7 is nine -
   because the twelve-declaration-per-file cap applies to libraries too. Fifty-four of the 121
   files belong to the P1–P9 packages below; the other sixty-seven serve foundation domains or
   carry concrete per-type bodies and are listed in "Libraries outside the package table".
2. **Function form**: `Name(self: ConceptName, ...): ReturnType => expression;` — first
   parameter is the concept. Prefer a single expression where one reads well.

   **Local bindings and affine mutation are permitted.** `var` is a pure local binding and
   changes nothing about referential transparency; write `{ var q = ...; return ...; }` when a
   formula needs a shared subterm. Mutation of a `unique` (affine) builder is likewise pure *by
   uniqueness*: a builder has exactly one reference, so `xs = xs.Add(p)` is a linear update, not
   observable mutation of shared state — this is the entire point of `List`/`Buffer`
   (`primitives.plato`), and an earlier "no mutation" reading forbade it outright.

   What stays banned: side effects, and mutation of anything aliased. Outside `unique` types the
   language does not offer either.

   Precedent for statement bodies: the transform bodies (now the `transforms-*.library.plato`
   files) have used `var` throughout since well before the v3 libraries existed;
   `polynomials.library.plato`, `solids.library.plato`, `surfaces.library.plato` and the
   `implicit-sdf-*.library.plato` files follow it.

   Caveat when choosing a body form: **LINT003 cannot see field reads inside statement blocks or
   `var` initializers** — only reads in the returned/final expression register
   (`Plato.Compiler/Analysis/Linter.cs`, `CheckUnusedFields`). Converting an expression body to a
   statement body can therefore make unread-field findings *rise* for fields that are genuinely
   read. That is a linter defect, not a signal about your code: do not contort a body to chase
   the number, and do not read LINT003 deltas as a measure of coverage.

   Status note: the affine builders' single-parameter members — `Count`, `Freeze` and
   `EmptyList` — are now declared in `primitives.plato` (host signatures in
   `intrinsics.library.plato`). `FunctionInstance.cs` used to abort the
   whole compilation on any generic function of one or fewer parameters; the guard now permits
   such a function when every type variable is determined by its parameter(s) (as these are),
   and only rejects a type variable reachable solely through the return type. A builder can now
   be constructed, mutated, observed with `Count`, and consumed with `Freeze`.
3. **Only call what exists**: functions declared on the concept itself, its inherited
   concepts, or functions you define in your own library file. Verify every member name
   with the plato-navigation MCP tools (`plato_search_symbols` → `plato_definition` /
   `plato_source`). Never guess a member name.
4. **Better names than v1**: no abbreviations (`Sqr` → `Square`), no cryptic forms
   (`FromOne(x)` → `OneMinus(x)`), intent-revealing (`MultiplyEpsilon` → gone; fold into
   `AlmostEqual` with an explicit tolerance overload). Keep universally-understood names:
   `Lerp`, `Clamp`, `Dot`, `Cross`.
5. **Doc comments**: every function gets a `//` comment stating what it computes and any
   preconditions. Section banners use `//==`.
6. **No new types, no new concepts** *while implementing a package*. Declarations live in the
   domain declaration files; the library files are bodies only. If a concept lacks a member you
   need, note it in a `// TODO(concept-gap):` comment and work around it or skip the function.

   Those TODOs are a **burn-down queue, not a permanent ban**. A concept surface may be extended
   deliberately, as its own piece of work with its own justification — `Vector.FromComponents` /
   `Broadcast` and `Quantity.FromAmount` were both added exactly that way, and each discharged
   hundreds of LINT001 obligations that were otherwise unreachable. Extending a concept adds an
   obligation to **every** implementor tree-wide, so grep `implements <Concept>` across all files
   first and fill the new obligations in the same change.
7. **Generic functions** may use type variables constrained by concepts where the concept is
   generic (`IntervalLike<T>`, `Field<TDomain,TValue>`); mirror the declaration's parameters.
8. **Angles are `Angle`**, never raw `Number`. Respect the unit conventions in
   [`README.md`](README.md).
9. **Validate before you finish**: run
   `dotnet run --project submodules/Plato/Plato.CLI -c Release -- lint submodules/Plato/stdlib`
   from `C:\Users\cdigg\git\studio` — zero parse errors, zero symbol-resolution errors.
   After every edit call `plato_reload` so the MCP index stays fresh.
10. **Scope discipline**: implement ONLY functions for the concepts in your package, and never
    edit another package's `*.library.plato` files. The boundary is by file kind and package
    ownership: declaration files declare, your package's `*.library.plato` files implement.

    Editing a domain declaration file is out of bounds *as a side effect of implementing a
    package*. It is in bounds as **deliberate, separately-justified work** — extending a concept
    surface (rule 6), or adding a new sibling `*.library.plato` file for bodies that belong
    beside a declaration (rule 1). Say which you are doing and why; do not drift into a
    declaration file to unblock a body.

## Work packages

| Pkg | Library files | Concept source files | Concepts |
|-----|--------------|---------------------|----------|
| P1 | `core-comparison`, `core-logic`, `algebra-operations`, `algebra-numeric`, `algebra-metric` | `core-comparison.concepts`, `core-logic.concepts`, `algebra-operations.concepts`, `algebra-numeric.concepts`, `algebra-metric.concepts` | Equatable, Value, Hashable, Orderable, Comparable, Logical, BooleanAlgebra, Bitwise, Additive, Multiplicative, Divisible, Modular, Invertible, Arithmetic, Scalable, Interpolatable, NumericalLimits, Numerical, Real, Whole, Normed, MetricSpace, Lattice, Clampable, Difference |
| P2 | `collections-indexable`, `collections-containers`, `collections-grids`, `collections-sampling`, `functional-procedural` | `collections-indexable.concepts`, `collections-containers.concepts`, `functional.concepts` | Countable, Index, Indexable/2D/3D/4D, Sliceable, Concatenable, SetLike, MapLike, StackLike, QueueLike, Procedural, Bijective, Periodic, ParameterDomain |
| P3 | `numeric-structures-quantity`, `numeric-structures-vector`, `numeric-structures-components`, `numeric-structures-matrix`, `numeric-structures-coordinate`, `numeric-structures-algebra` | `quantities.concepts`, `vectors.concepts`, `matrices.concepts`, `points.concepts` | Quantity, Vector, MatrixLike, Coordinate |
| P4 | `intervals-transforms-interval`, `intervals-transforms-bounds`, `intervals-transforms-transformable`, `intervals-transforms-deformable` | `intervals-bounds.concepts`, `transforms.concepts` | IntervalLike, BoundsLike, Transformable, Deformable2D, Deformable3D |
| P5 | `geometry`, `geometry-measures`, `geometry-pointsets`, `geometry-queries`, `geometry-kernels` | `geometry.concepts`, `geometry-measures.concepts`, `geometry-queries.concepts` | Geometry family, shape traits, Bounded2D/3D, PointSet2D/3D, measurables, centroids, containment, nearest-point, support mapping |
| P6 | `curves`, `curves-capabilities`, `curves-sampling`, `surfaces-solids` | `curves.concepts`, `curves-capabilities.concepts`, `surfaces-solids.concepts` | Curve<TRange>, Curve1D-3D, ClosedCurve2D/3D, PeriodicCurve, DifferentiableCurve2D/3D, FramedCurve3D, PlanarCurve3D, PolarCurve2D, ArcLengthParameterized, Surface, ClosedSurface, ParametricSurface, DifferentiableSurface, HeightFieldSurface, Solid, ConvexSolid, ParametricVolume |
| P7 | `fields-implicits-core`, `fields-implicits-differentiable`, `fields-implicits-distance`, `fields-implicits-function`, `fields-implicits-metaballs`, `fields-implicits-nodes`, `fields-implicits-sampled`, `fields-implicits-shapes`, `fields-implicits-time-varying` | `fields.concepts`, `fields-differentiable.concepts`, `fields-time-varying.concepts`, `implicit-sdf.concepts` | Field, ScalarField2D/3D, VectorField2D/3D, DirectionField, ColorField, ComplexField2D, TensorField, the Differentiable and TimeVarying field families, SignedDistanceField2D/3D, ImplicitRegion2D, ImplicitVolume3D |
| P8 | `meshes-topology`, `meshes-geometry`, `meshes-polygon-incidence`, `spatial-queries` | `topology.concepts`, `meshes.concepts`, `pointclouds-voxels.concepts`, `spatial-structures.concepts`, `spatial-queries.concepts` | MeshElementCounts, MeshIncidence, HalfEdgeNavigable, Meshable2D/3D, TriangulatedGeometry3D, PointCloudable3D, Voxelizable3D, SpatialIndex2D/3D, RayIntersectable2D/3D, ClosestPointQueryable2D/3D, NearestNeighborQueryable2D/3D |
| P9 | `easing`, `time-varying`, `paths`, `images`, `texturing`, `cameras`, `lights`, `kinematics`, `rigid-dynamics`, `random-distributions`, `graphs`, `scientific-data`, `geo-spatial` | `easing.concepts`, `time-varying.concepts`, `paths.concepts`, `images.concepts`, `texturing.concepts`, `cameras.concepts`, `lights.concepts`, `kinematics.concepts`, `rigid-dynamics.concepts`, `random.concepts`, `graphs.concepts`, `scientific-data.concepts`, `geo-spatial.concepts` | EasingFunction, TimeVarying, PathLike, Image, ProceduralTexture, Camera, LightSource, Kinematic2D/3D, ForceModel2D/3D, ProbabilityDistribution, GraphLike, TimeSampled, GeoRegion |

File names in the two middle columns omit the `.plato` extension: `core-logic` means
`core-logic.library.plato`, `core-logic.concepts` means `core-logic.concepts.plato`. The nine
package rows account for 54 of the 121 library files and for all 42 `*.concepts.plato` files.

**P9 was formerly a single monolithic file.** One 499-line `library DomainTraits` served thirteen
unrelated concepts - easing curves, animation tracks, vector paths, rasters, procedural
textures, cameras, lights, kinematics, force models, probability distributions, graphs, time
series and geodetic regions. It is now thirteen files, one per concept, each named after the
domain it serves. `random-distributions.library.plato` carries that longer stem because
`random.library.plato` was already taken by the concrete per-distribution bodies.

Cross-package dependency: lower-package libraries may be referenced by higher ones (P5+ may
call P1 helpers), but prefer self-sufficiency; never create cycles.

11. **No duplication of lower-package generics.** Before writing a helper, check whether a
    lower-package library already provides it generically via an inherited concept (e.g. P1's
    `MidPoint(Interpolatable)`, `Clamp`/`Between` on `Orderable`, `Half(Scalable)`,
    `Double(Additive)`, `LerpClamped(Interpolatable)`, `Saturate(Real)`). If your concept
    inherits that concept, the generic already applies — do not re-declare a per-concept copy,
    and never under a different spelling. If a function only uses inherited members, it belongs
    in the package that owns the base concept; leave a `// TODO(cross-package):` note instead.

## Libraries outside the package table

These sixty-six files serve foundation domains or hold concrete per-type bodies rather than a
P1–P9 concept package (ground rule 1). All follow the same one-block-per-file naming.

**Foundation domains (23):**

| Library file | Block | Serves |
|---|---|---|
| `constants.library.plato` | `Constants` | irrationals, unit conversions, axis vectors, canonical unit shapes |
| `angles.library.plato` | `Angles` | `Angle` unit constructors/accessors (Turns, Degrees, Gradians, ArcMinutes, ArcSeconds) and Sec/Csc/Cot |
| `quantities.library.plato` | `Quantities` | the concrete `Quantity` obligation fills for ~50 quantity types |
| `quantities-projections.library.plato` | `QuantityProjections` | the `Amount` / `FromAmount` pair per concrete quantity type |
| `color.library.plato` | `Colors` | `Color` arithmetic |
| `color-named.library.plato` | `NamedColors` | the 141 CSS/X11 named colors as sRGB `Color8` |
| `axes.library.plato` | `Axes` | `Axis3D` operations |
| `axes-2d.library.plato` | `Axes2D` | `Axis2D` operations |
| `axes-signed.library.plato` | `SignedAxes` | `SignedAxis3D` operations |
| `intrinsics.library.plato` | `Intrinsics` | the host contract, reduced to an irreducible kernel by plato-378 and only over `primitive` types: the scalar kernel, the five-function array kernel, and the `unique` `List<T>` / `Buffer<T>` surface; carries the intrinsics preamble, the admission rule and the porting notes |
| `primitives.library.plato` | `Primitives` | the primitive concept obligations the host contract does not supply: the identity conversions, `Number.Inverse`, the two `Lerp`s, and the derived Boolean surface |
| `primitives-number.library.plato` | `PrimitivesNumber` | reference bodies for every derivable `Number` member (Abs, Sign, Min/Max, Ceiling/Truncate, Cbrt, the logarithms, the hyperbolics and the whole inverse-trig family, Zero/One/Tau/E, IsFinite) |
| `primitives-integer.library.plato` | `PrimitivesInteger` | reference bodies for the derivable `Integer` members: Abs, Sign, Min/Max, Zero/One, Range |
| `primitives-arrays.library.plato` | `PrimitivesArrays` | reference bodies for the whole derived array surface over the `MapRange` / `Reduce` kernel, plus the `Array2D` / `Array3D` construction and traversal that used to be intrinsic |
| `angle-trig.library.plato` | `AngleTrig` | `Angle` arithmetic and trig, and the `Angle`-returning inverse trig on `Number`, over the radians kernel |
| `vectors-tuples-ops.library.plato` | `VectorsTuplesOps` | reference bodies for `Number2/3/4/8` component-wise math and `Number4` transforms |
| `vectors-geometric-ops.library.plato` | `VectorsGeometricOps` | reference bodies for `Vector2D`/`Vector3D` displacement algebra, reflection and transforms |
| `matrices-ops.library.plato` | `MatricesOps` | reference bodies for `Matrix3x2`/`Matrix4x4`, including invert, decompose and the `Create*` factories |
| `rotations-ops.library.plato` | `RotationsOps` | reference bodies for `Quaternion`, including `Slerp` and `CreateFromRotationMatrix` |
| `hashing.library.plato` | `Hashing` | `CombineHash`, the fold the per-type `Hash` reference bodies are written against |
| `transforms-points.library.plato` | `TransformsPoints` | Point2D/3D Difference, Lerp, point-vector converters |
| `transforms-pose.library.plato` | `TransformsPose` | Pose2D/3D application and composition |
| `transforms-trs.library.plato` | `TransformsTrs` | Transform2D/3D (TRS) application and conversion |
| `transforms-affine.library.plato` | `TransformsAffine` | affine and projective transform application |
| `transforms-frames.library.plato` | `TransformsFrames` | Frame2D/3D and Basis3D |
| `transforms-motor.library.plato` | `TransformsMotor` | Motor3D (dual quaternion) |
| `transforms-rotations.library.plato` | `TransformsRotations` | quaternion / rotor / Rotation2D application and conversion |
| `transforms-identities.library.plato` | `TransformsIdentities` | the `Identity(_: T)` family across every transform representation |
| `deformations.library.plato` | `Deformations` | `Deformation2D/3D` catalog Eval bodies and Deformable lifts |

**Concrete geometry bodies (19):** `lines`, `lines-planes`, `planar-triangles`,
`planar-circles`, `planar-ellipses`, `planar-boxes`, `planar-shapes`, `polygons`,
`polygons-polylines`, `polygons-spatial`, `polygons-kernels`, `spatial-spheres`,
`spatial-boxes`, `spatial-cylinders`, `spatial-capsules`, `spatial-tori`, `spatial-patches`,
`spatial-simplices`, `spatial-primitives` (all `.library.plato`).

**Concrete curve / surface / solid bodies (10):** `curves-2d-arcs`, `curves-2d-polar`,
`curves-2d-spirals`, `curves-3d`, `splines-bezier`, `splines-bspline`, `splines-hermite`,
`splines-interpolating`, `surfaces`, `solids` (all `.library.plato`).

**Concrete field / implicit bodies (6):** `fields-graphs`, `implicit-sdf-primitives`,
`implicit-sdf-operators`, `implicit-sdf-modifiers`, `implicit-sdf-sampled`,
`implicit-sdf-trees` (all `.library.plato`).

**Concrete math bodies (8):** `polynomials`, `statistics`, `statistics-correlation`, `random`,
`random-continuous`, `random-continuous-gamma`, `random-continuous-tails`, `random-discrete`
(all `.library.plato`).

Constants use the tree's constant idiom — dispatch on an ignored receiver of the result type,
`GoldenRatio(_: Number): Number`, read as `Number.GoldenRatio` / `Vector3D.UnitX` /
`Color8.AliceBlue`. Zero-argument functions are not used anywhere in this tree.
