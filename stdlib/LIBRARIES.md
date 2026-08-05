# stdlib interface libraries — implementation bodies

The `*.library.plato` files in `stdlib/` hold `library` blocks that implement **derived
functionality for the interfaces** declared in the domain declaration files. In Plato, a function
whose first parameter is interface-typed is like a C# extension method on an interface: it becomes
available on every type that implements the interface. These libraries implement
derived functionality on the interfaces declared in the domain files — build on
good ideas from earlier vocabulary, replace bad names, drop dead weight.

The library files live side-by-side with the declaration files they implement (they were flattened
out of the old `interface-library/` subfolder). Because `lint` enumerates `*.plato` non-recursively
(`Plato.CLI/Program.cs:101` / `:197` use `GetFiles("*.plato", TopDirectoryOnly)`), the lint command
below now genuinely covers these library bodies — which it did **not** while they sat in a subfolder.

## Ground rules

1. **One `library` block per file**, named `<domain>.library.plato` after the domain it serves,
   with a PascalCase block name derived from the stem (`collections.library.plato` holds
   `library Collections`).

   This is exact and has no exceptions:
   `grep -l "^library " *.plato` returns **exactly the `*.library.plato` files** and
   nothing else. **No declaration file carries an inline `library` block.** The sanctioned home
   for bodies that belong beside a declaration is a sibling `.library.plato` file with the same
   stem, not the declaration file itself.

   Inside a block, `//==` section banners divide the subjects the domain covers; a domain that
   grew several separate bodies keeps them as sections of one library rather than as separate
   files. A work *package* may still span several library files where its bodies serve several
   domains. The P1–P9 packages below claim some of the library files; the rest serve foundation
   domains or carry concrete per-type bodies and are listed in "Libraries outside the package
   table".
2. **Function form**: `Name(self: ConceptName, ...): ReturnType => expression;` — first
   parameter is the interface. Prefer a single expression where one reads well.

   **Local bindings and affine mutation are permitted.** `var` is a pure local binding and
   changes nothing about referential transparency; write `{ var q = ...; return ...; }` when a
   formula needs a shared subterm. Mutation of a `unique` (affine) builder is likewise pure *by
   uniqueness*: a builder has exactly one reference, so `xs = xs.Add(p)` is a linear update, not
   observable mutation of shared state — this is the entire point of `List`/`Buffer`
   (`primitives.plato`), and an earlier "no mutation" reading forbade it outright.

   What stays banned: side effects, and mutation of anything aliased. Outside `unique` types the
   language does not offer either.

   Precedent for statement bodies: the transform bodies (now `transforms.library.plato`) have
   used `var` throughout since well before the v3 libraries existed;
   `polynomials.library.plato`, `solids.library.plato`, `surfaces.library.plato` and
   `implicit-sdf.library.plato` follow it.

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
3. **Only call what exists**: functions declared on the interface itself, its inherited
   interfaces, or functions you define in your own library file. Verify every member name
   with the plato-navigation MCP tools (`plato_search_symbols` → `plato_definition` /
   `plato_source`). Never guess a member name.
4. **Better names than v1**: no abbreviations (`Sqr` → `Square`), no cryptic forms
   (`FromOne(x)` → `OneMinus(x)`), intent-revealing (`MultiplyEpsilon` → gone; fold into
   `AlmostEqual` with an explicit tolerance overload). Keep universally-understood names:
   `Lerp`, `Clamp`, `Dot`, `Cross`.
5. **Doc comments**: every function gets a `//` comment stating what it computes and any
   preconditions. Section banners use `//==`.
6. **No new types, no new interfaces** *while implementing a package*. Declarations live in the
   domain declaration files; the library files are bodies only. If an interface lacks a member you
   need, note it in a `// TODO(interface-gap):` comment and work around it or skip the function.

   Those TODOs are a **burn-down queue, not a permanent ban**. An interface surface may be extended
   deliberately, as its own piece of work with its own justification — `Vector.FromComponents` /
   `Broadcast` and `Quantity.FromAmount` were both added exactly that way, and each discharged
   hundreds of LINT001 obligations that were otherwise unreachable. Extending an interface adds an
   obligation to **every** implementor tree-wide, so grep `implements <Interface>` across all files
   first and fill the new obligations in the same change.
7. **Generic functions** may use type variables constrained by interfaces where the interface is
   generic (`IInterval<T>`, `Field<TDomain,TValue>`); mirror the declaration's parameters.
8. **Angles are `Angle`**, never raw `Number`. Respect the unit conventions in
   [`README.md`](README.md).
9. **Validate before you finish**: run
   `dotnet run --project submodules/Plato/Plato.CLI -c Release -- lint submodules/Plato/stdlib`
   from `C:\Users\cdigg\git\studio` — zero parse errors, zero symbol-resolution errors.
   After every edit call `plato_reload` so the MCP index stays fresh.
10. **Scope discipline**: implement ONLY functions for the interfaces in your package, and never
    edit another package's `*.library.plato` files. The boundary is by file kind and package
    ownership: declaration files declare, your package's `*.library.plato` files implement.

    Editing a domain declaration file is out of bounds *as a side effect of implementing a
    package*. It is in bounds as **deliberate, separately-justified work** — extending an interface
    surface (rule 6), or adding a new sibling `*.library.plato` file for bodies that belong
    beside a declaration (rule 1). Say which you are doing and why; do not drift into a
    declaration file to unblock a body.

## Work packages

| Pkg | Library files | Interface source files | Interfaces |
|-----|--------------|---------------------|----------|
| P1 | `core`, `algebra` | `core.interfaces`, `algebra.interfaces` | Equatable, Value, Hashable, Orderable, Comparable, Logical, BooleanAlgebra, Bitwise, Additive, Multiplicative, Divisible, Modular, Invertible, Arithmetic, Scalable, Interpolatable, NumericalLimits, Numerical, Real, Whole, Normed, MetricSpace, Lattice, Clampable, Difference |
| P2 | `collections`, `functional-procedural` | `collections.interfaces`, `functional.interfaces` | Countable, Index, Indexable/2D/3D/4D, Sliceable, Concatenable, ISet, IMap, IStack, IQueue, Procedural, Bijective, Periodic, ParameterDomain |
| P3 | `numeric-structures` | `quantities.interfaces`, `vectors.interfaces`, `matrices.interfaces`, `points.interfaces` | Quantity, Vector, IMatrix, Coordinate |
| P4 | `intervals`, `intervals-transforms-deformable` | `intervals-bounds.interfaces`, `transforms.interfaces` | IInterval, IBounds, Transformable2D, Transformable3D, Deformable2D, Deformable3D |
| P5 | `geometry` | `geometry.interfaces` | Geometry family, shape traits, Bounded2D/3D, PointSet2D/3D, measurables, centroids, containment, nearest-point, support mapping |
| P6 | `curves`, `surfaces` | `curves.interfaces`, `surfaces-solids.interfaces` | Curve<TRange>, Curve1D-3D, ClosedCurve2D/3D, PeriodicCurve, DifferentiableCurve2D/3D, FramedCurve3D, PlanarCurve3D, PolarCurve2D, ArcLengthParameterized, Surface, ClosedSurface, ParametricSurface, DifferentiableSurface, HeightFieldSurface, Solid, ConvexSolid, ParametricVolume |
| P7 | `fields-implicits` | `fields.interfaces`, `implicit-sdf.interfaces` | Field, ScalarField2D/3D, VectorField2D/3D, DirectionField, ColorField, ComplexField2D, TensorField, the Differentiable and TimeVarying field families, SignedDistanceField2D/3D, ImplicitRegion2D, ImplicitVolume3D |
| P8 | `meshes`, `meshes-polygon`, `spatial-structures` | `topology.interfaces`, `meshes.interfaces`, `pointclouds-voxels.interfaces`, `spatial.interfaces` | MeshElementCounts, MeshIncidence, IHalfEdge, IMesh2D/3D, TriangulatedGeometry3D, IPointCloud3D, IVoxel3D, SpatialIndex2D/3D, IRaycast2D/3D, IClosestPoint2D/3D, INearestNeighbor2D/3D |
| P9 | `easing`, `time-varying`, `paths`, `images`, `texturing`, `cameras`, `lights`, `kinematics`, `rigid-dynamics`, `random`, `graphs`, `scientific-data`, `geo-spatial` | `easing.interfaces`, `time-varying.interfaces`, `paths.interfaces`, `images.interfaces`, `texturing.interfaces`, `cameras.interfaces`, `lights.interfaces`, `kinematics.interfaces`, `rigid-dynamics.interfaces`, `random.interfaces`, `graphs.interfaces`, `scientific-data.interfaces`, `geo-spatial.interfaces` | EasingFunction, TimeVarying, IPath, Image, ProceduralTexture, Camera, LightSource, Kinematic2D/3D, ForceModel2D/3D, ProbabilityDistribution, IGraph, TimeSampled, GeoRegion |

File names in the two middle columns omit the `.plato` extension: `core` means
`core.library.plato`, `core.interfaces` means `core.concepts.plato`.

A package row names the file a package's bodies live in, not a file the package owns outright.
Where a domain's interface bodies and its concrete per-type bodies share a domain they share a
file: `random.library.plato` holds both the `ProbabilityDistribution` traits of P9 and the
per-distribution bodies, separated by section banners rather than by file.

Cross-package dependency: lower-package libraries may be referenced by higher ones (P5+ may
call P1 helpers), but prefer self-sufficiency; never create cycles.

11. **No duplication of lower-package generics.** Before writing a helper, check whether a
    lower-package library already provides it generically via an inherited interface (e.g. P1's
    `MidPoint(Interpolatable)`, `Clamp`/`Between` on `Orderable`, `Half(Scalable)`,
    `Double(Additive)`, `LerpClamped(Interpolatable)`, `Saturate(Real)`). If your interface
    inherits that interface, the generic already applies — do not re-declare a per-interface copy,
    and never under a different spelling. If a function only uses inherited members, it belongs
    in the package that owns the base interface; leave a `// TODO(cross-package):` note instead.

## Libraries outside the package table

The remaining `*.library.plato` files serve foundation domains or hold concrete per-type bodies
rather than a P1–P9 interface package (ground rule 1). All follow the same one-block-per-file
naming; `grep "^library " */*.library.plato` is the live list.

**Foundation domains:**

| Library file | Block | Serves |
|---|---|---|
| `constants.library.plato` | `Constants` | irrationals, unit conversions, axis vectors, canonical unit shapes |
| `angles.library.plato` | `Angles` | `Angle` unit constructors/accessors (Turns, Degrees, Gradians, ArcMinutes, ArcSeconds), Sec/Csc/Cot, and the `Angle` arithmetic and trig over the radians kernel |
| `quantities.library.plato` | `Quantities` | the concrete `Quantity` obligation fills, and the `Amount` / `FromAmount` pair per concrete quantity type |
| `color.library.plato` | `Colors` | `Color` arithmetic, and the CSS/X11 named colors as sRGB `Color8` |
| `axes.library.plato` | `Axes` | `Axis3D`, `Axis2D` and `SignedAxis3D` operations |
| `intrinsics.library.plato` | `Intrinsics` | the host contract, reduced to an irreducible kernel by plato-378 and only over `primitive` types: the scalar kernel, the five-function array kernel, and the `unique` `List<T>` / `Buffer<T>` surface; carries the intrinsics preamble, the admission rule and the porting notes |
| `primitives.library.plato` | `Primitives` | the primitive interface obligations the host contract does not supply (identity conversions, `Number.Inverse`, the two `Lerp`s, the derived Boolean surface), plus reference bodies for the derivable `Number`, `Integer` and array surfaces |
| `numbers.library.plato` | `Numbers` | the special-function vocabulary (gamma, beta, erf, Bessel) and the dual-number surface |
| `vectors.library.plato` | `Vectors` | reference bodies for `Number2/3/4/8` component-wise math and for `Vector2D`/`Vector3D` displacement algebra, reflection and transforms |
| `matrices.library.plato` | `Matrices` | dense and sparse matrix bodies |
| `matrices-ops.library.plato` | `MatricesOps` | reference bodies for `Matrix3x2`/`Matrix4x4`, including invert, decompose and the `Create*` factories |
| `rotations-ops.library.plato` | `RotationsOps` | reference bodies for `Quaternion`, including `Slerp` and `CreateFromRotationMatrix` |
| `hashing.library.plato` | `Hashing` | `CombineHash`, the fold the per-type `Hash` reference bodies are written against |
| `sorting.library.plato` | `Sorting` | `SortedIndices` / `Sort` — a stable, recursion-free bottom-up merge sort over an index `Buffer`. A reference body, not an intrinsic (plato-378): backends recover native speed through the override table |
| `transforms.library.plato` | `Transforms` | point conversion, pose, TRS, affine and projective application, frames and bases, motors, rotation conversion, and the `Identity(_: T)` family |
| `statistics.library.plato` | `Statistics` | univariate summary bodies and the bivariate covariance / correlation / fit readings |
| `random.library.plato` | `Random` | the shared numerical helpers and the per-distribution bodies, alongside P9's `ProbabilityDistribution` traits |
| `polynomials.library.plato` | `Polynomials` | polynomial evaluation and the cubic bases |
| `time.library.plato` | `Time` | `Instant` / `Duration` arithmetic |
| `deformations.library.plato` | `Deformations` | `Deformation2D/3D` catalog Eval bodies and Deformable lifts |

**Concrete geometry bodies:** `lines`, `planar`, `polygons`, `polyhedra`, `spatial-primitives`,
`brep`, `pointclouds`, `topology` (all `.library.plato`).

**Concrete curve / surface / solid bodies:** `curves-shapes`, `splines`, `surfaces`,
`surfaces-shapes`, `solids`, `solids-csg` (all `.library.plato`).

**Concrete field / implicit bodies:** `fields-graphs`, `implicit-sdf`, `noise`, `sampling`
(all `.library.plato`).

Constants use the tree's constant idiom — dispatch on an ignored receiver of the result type,
`GoldenRatio(_: Number): Number`, read as `Number.GoldenRatio` / `Vector3D.UnitX` /
`Color8.AliceBlue`. Zero-argument functions are not used anywhere in this tree.
