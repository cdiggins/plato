# stdlib concept libraries — implementation bodies

The `*.library.plato` files in `stdlib/` hold `library` blocks that implement **derived
functionality for the concepts** declared in the domain declaration files. In Plato, a function
whose first parameter is concept-typed is like a C# extension method on an interface: it becomes
available on every type that implements the concept. These libraries are the v3 successor to
`stdlib-legacy/core.library.plato` / `geometry.library.plato` — build on their good ideas,
replace bad names, drop dead weight.

The library files live side-by-side with the declaration files they implement (they were flattened
out of the old `concept-library/` subfolder). Because `lint` enumerates `*.plato` non-recursively
(`Plato.CLI/Program.cs:101` / `:197` use `GetFiles("*.plato", TopDirectoryOnly)`), the lint command
below now genuinely covers these library bodies — which it did **not** while they sat in a subfolder.

## Ground rules

1. **One file per work package**, named `<domain>.library.plato` where the domain matches the
   lowest concept file it serves (e.g. `core-algebra.library.plato`). One `library` block
   per file, PascalCase name matching the domain (e.g. `library CoreAlgebra`).
2. **Function form**: `Name(self: ConceptName, ...): ReturnType => expression;` — pure
   expressions only, no statements, no mutation. First parameter is the concept.
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
6. **No new types, no new concepts.** Declarations live in the domain declaration files; the
   library files are bodies only. If a concept lacks a member you need, note it in a
   `// TODO(concept-gap):` comment and work around it or skip the function.
7. **Generic functions** may use type variables constrained by concepts where the concept is
   generic (`IntervalLike<T>`, `Field<TDomain,TValue>`); mirror the declaration's parameters.
8. **Angles are `Angle`**, never raw `Number`. Respect the unit conventions in
   [`README.md`](README.md).
9. **Validate before you finish**: run
   `dotnet run --project submodules/Plato/Plato.CLI -c Release -- lint submodules/Plato/stdlib`
   from `C:\Users\cdigg\git\studio` — zero parse errors, zero symbol-resolution errors.
   After every edit call `plato_reload` so the MCP index stays fresh.
10. **Scope discipline**: implement ONLY functions for the concepts in your package. Never
    edit the domain declaration files (`domain.plato` / `domain.concepts.plato`), and never edit
    another package's `*.library.plato` file. The boundary is by file kind and package ownership:
    declaration files declare, your one `*.library.plato` file implements.

## Work packages

| Pkg | Library file | Concept source files | Concepts |
|-----|-------------|---------------------|----------|
| P1 | `core-algebra.library.plato` | core.concepts, algebra.concepts | Equatable, Value, Hashable, Orderable, Comparable, Logical, BooleanAlgebra, Bitwise, Additive, Multiplicative, Divisible, Modular, Invertible, Arithmetic, Scalable, Interpolatable, NumericalLimits, Numerical, Real, Whole, Normed, MetricSpace, Lattice, Clampable, Difference |
| P2 | `collections-functional.library.plato` | collections.concepts, functional.concepts | Countable, Index, Indexable/2D/3D/4D, Sliceable, Concatenable, SetLike, MapLike, StackLike, QueueLike, Procedural, Bijective, Periodic, ParameterDomain |
| P3 | `numeric-structures.library.plato` | quantities, vectors, matrices, points | Quantity, Vector, MatrixLike, Coordinate |
| P4 | `intervals-transforms.library.plato` | intervals-bounds, transforms | IntervalLike, BoundsLike, Transformable, Deformable2D, Deformable3D |
| P5 | `geometry.library.plato` | geometry.concepts | Geometry family, Dimensioned, shape traits, Bounded2D/3D/4D, PointSet2D/3D/4D, measurables, centroids, containment, nearest-point, support mapping |
| P6 | `curves-surfaces.library.plato` | curves-surfaces.concepts | Curve1D–4D, ClosedCurve2D/3D, PeriodicCurve, DifferentiableCurve2D/3D, FramedCurve3D, PlanarCurve3D, PolarCurve2D, ArcLengthParameterized, Surface, ClosedSurface, ParametricSurface, DifferentiableSurface, HeightFieldSurface, Solid, ConvexSolid, ParametricVolume |
| P7 | `fields-implicits.library.plato` | fields, implicit-sdf | Field, ScalarField2D/3D/4D, VectorField2D/3D, DirectionField, ColorField, ComplexField2D, TensorField, Differentiable* fields, TimeVarying* fields, SignedDistanceField2D/3D, ImplicitRegion2D, ImplicitVolume3D |
| P8 | `meshes-spatial.library.plato` | topology, meshes, pointclouds-voxels, spatial-structures, spatial-queries | MeshTopology, HalfEdgeNavigable, Meshable2D/3D, TriangulatedGeometry3D, PointCloudable3D, Voxelizable3D, SpatialIndex2D/3D, RayIntersectable2D/3D, ClosestPointQueryable2D/3D, NearestNeighborQueryable2D/3D |
| P9 | `domain-traits.library.plato` | easing, keyframes-tracks, paths, images, texturing, cameras, lights, kinematics, rigid-dynamics, random, graphs, scientific-data, geo-spatial | EasingFunction, TimeVarying, PathLike, Image, ProceduralTexture, Camera, LightSource, Kinematic2D/3D, ForceModel2D/3D, ProbabilityDistribution, GraphLike, TimeSampled, GeoRegion |

Cross-package dependency: lower-package libraries may be referenced by higher ones (P5+ may
call P1 helpers), but prefer self-sufficiency; never create cycles.

11. **No duplication of lower-package generics.** Before writing a helper, check whether a
    lower-package library already provides it generically via an inherited concept (e.g. P1's
    `MidPoint(Interpolatable)`, `Clamp`/`Between` on `Orderable`, `Half(Scalable)`,
    `Double(Additive)`, `LerpClamped(Interpolatable)`, `Saturate(Real)`). If your concept
    inherits that concept, the generic already applies — do not re-declare a per-concept copy,
    and never under a different spelling. If a function only uses inherited members, it belongs
    in the package that owns the base concept; leave a `// TODO(cross-package):` note instead.
