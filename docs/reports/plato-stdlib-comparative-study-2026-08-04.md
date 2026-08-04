# Plato standard library compared with established numerical and geometry libraries

**Date:** 2026-08-04

**Revision:** expands the [2026-08-03 study](plato-stdlib-comparative-study-2026-08-03.md)
with Open CASCADE Technology, Wolfram Language, and Three.js. The earlier report remains
a frozen snapshot.

**Scope:** the shipping `foundation`, `geometry`, and `graphics` tiers of the forward
standard library

**Question:** what does Plato do differently, where is it materially behind mature
libraries, and which lessons should shape its next standard-library work?

## Executive conclusion

Plato is not currently a substitute for NumPy/SciPy, Eigen, CGAL, Open CASCADE
Technology (OCCT), libigl, Geometry Central, Shapely, Wolfram Language, or Three.js.
Those systems offer deeper numerical kernels, more robust predicates and constructions,
industrial CAD modeling, integrated symbolic-numeric computation, or complete browser
rendering workflows backed by mature implementations. Plato's forward library also has
an important execution gap: its vocabulary is checked more broadly than its generated
behavior can currently be run.

Plato nevertheless occupies a useful position that none of the comparison libraries
does by itself. It combines:

- semantic types for points, displacement vectors, directions, angles, quantities,
  frames, topology indices, fields, and geometry;
- a single pure source language intended to generate several host and shader targets;
- analytic, parametric, implicit, discrete, and graphics vocabulary in one type
  system; and
- reference bodies that can state portable behavior independently of any one host
  library.

The right goal is therefore not to reproduce every competing API. Plato should be a
small, semantically strong, portable geometry kernel with deliberate escape hatches to
specialized native engines and runtimes. The immediate priority is executable
conformance. The next priorities are numerical policy, robust predicates, a small
solver/operator spine, complete planar and mesh workflows, and explicit CAD and
graphics interop boundaries. Plato should not attempt to reproduce OCCT's CAD kernel,
Wolfram's symbolic engine, or Three.js's renderer. Adding more declaration-only breadth
before those foundations would make the library look more complete without making it
more useful.

## Method and limits

The Plato side was inspected from the source tree, principally the tier definition in
[`stdlib/README.md`](../../stdlib/README.md), the domain contract in
[`stdlib/CONVENTIONS.md`](../../stdlib/CONVENTIONS.md), the generated declaration index
in [`stdlib/types-and-concepts.txt`](../../stdlib/types-and-concepts.txt), and the
`*.library.plato` bodies. The `future` tier is excluded because it is aspirational and
is not shipped or converted by the standard recipes. The legacy standard library is
also excluded.

The external side uses official manuals and project documentation. The selected
projects are archetypes, not a claim that they are the only or universally best
libraries in their categories:

| Archetype | Libraries | Why they are in the comparison |
|---|---|---|
| Array and scientific computing | [NumPy](https://numpy.org/doc/stable/user/whatisnumpy.html), [SciPy](https://docs.scipy.org/doc/scipy/tutorial/index.html) | The dominant model of homogeneous n-dimensional arrays plus a broad algorithm ecosystem |
| C++ linear algebra | [Eigen](https://libeigen.gitlab.io/eigen/docs-nightly/group__TutorialMatrixClass.html) | Fixed and dynamic shapes, generic scalars, expression-oriented computation, dense and sparse solvers |
| Rust linear algebra and geometry | [nalgebra](https://nalgebra.rs/docs/user_guide/vectors_and_matrices/) | Compile-time shapes, ownership-aware views, and strongly typed points and transformations |
| Graphics mathematics | [GLM](https://github.com/g-truc/glm) | A compact C++ API deliberately shaped like GLSL |
| Robust computational geometry | [CGAL](https://doc.cgal.org/latest/Manual/packages.html) | Kernel selection, exact or filtered predicates, and a large catalog of constructions and topology algorithms |
| Industrial CAD kernel | [Open CASCADE Technology](https://dev.opencascade.org/doc/overview/html/) | Boundary representation, curves and surfaces, solid modeling, shape healing, CAD interchange, and modeling history |
| Matrix-oriented mesh processing | [libigl](https://libigl.github.io/tutorial/) | Dense/sparse linear algebra as the organizing model for discrete differential geometry |
| Topology-oriented mesh processing | [Geometry Central](https://geometry-central.net/) | Halfedge connectivity, intrinsic geometry, and dynamic surface algorithms |
| Planar set operations | [Shapely/GEOS](https://shapely.readthedocs.io/en/stable/index.html) | A coherent point-set model with predicates, Boolean operations, constructive operations, and spatial indexing |
| Integrated symbolic-numeric geometry | [Wolfram Language](https://reference.wolfram.com/language/guide/ComputationalGeometry.html) | Region algebra connected to exact and numerical computation, optimization, discretization, and equation solving |
| Browser 3D runtime | [Three.js](https://threejs.org/manual/en/fundamentals.html) | A working scene graph, GPU geometry, materials, animation, and WebGL/WebGPU rendering |

OCCT is included as a direct comparator for Plato's B-rep and solid-modeling vocabulary.
Wolfram Language and Three.js are adjacent systems rather than like-for-like standard
libraries: they show what integrated region computation and a complete graphics runtime
look like, respectively. This is an architectural and capability study, not a benchmark.
It does not measure runtime speed, memory consumption, numerical error, API coverage by
symbol count, or
ecosystem adoption. An operation is credited to Plato only when the source contains an
implementation body or the report explicitly calls it vocabulary. A declared type is
not treated as equivalent to an externally tested algorithm.

## Plato's actual profile

The forward library is unusually broad. `foundation` covers primitive wrappers,
collections, algebraic interfaces, quantities, vectors, fixed and dynamic matrices,
sparse and banded matrix records, transforms, statistics, special functions, random
distributions, graphs, colors, and time. `geometry` covers analytic primitives,
polygons, curves, splines, parametric surfaces, solids, SDFs and other fields,
deformations, spatial structures, point clouds, voxels, B-reps, meshes, topology,
triangulation, sampling, and remeshing. `graphics` adds cameras, lights, materials,
images, paths, scenes, text, animation, and texturing. It does not yet constitute a
renderer, resource manager, or browser scene runtime comparable to Three.js.

Its programming model is more important than that catalog. Values are immutable;
interfaces are monomorphized type-class constraints; functions live in libraries and
use their first argument as the receiver; and most behavior is expected to be
expressible as pure Plato. Mutation is confined to affine `unique` builders whose state
cannot be aliased. The same source is then intended to feed C#, TypeScript, Rust, GLSL,
C++, and CUDA writers, subject to each backend's representability limits. See
[`plato-for-agents.md`](../plato-for-agents.md) for the live recipes.

Three maturity levels must be kept separate:

1. **Vocabulary:** types, interfaces, and signatures that parse and resolve.
2. **Reference behavior:** Plato bodies that type-check and can be lowered.
3. **Verified target behavior:** generated code that compiles and executes laws on a
   target.

Plato is strongest at the first level and has substantial work at the second. The
third is currently narrower. The foundation C# artifact has a build recipe, while the
full forward conformance runner remains blocked by generated-code failures described
in [`plato-308`](../../tracker/issues/plato-308.md) and
[`plato-323`](../../tracker/issues/plato-323.md). TypeScript, Rust, GLSL, C++, and CUDA
demonstrate selected subsets rather than the whole forward library. This is the largest
difference between Plato and every mature comparator in this report.

## Comparative map

| System | Primary abstraction | Shape and scalar model | Geometric center of gravity | Main advantage over Plato | Main tradeoff relative to Plato |
|---|---|---|---|---|---|
| Plato | Named immutable values plus generic library functions | Named fixed shapes, runtime `VectorN`/`MatrixN`, one `Number` representation | Analytic, parametric, implicit, discrete, and graphics vocabulary | One semantic source intended for several targets | Much less verified execution and native numerical depth |
| NumPy/SciPy | Homogeneous n-dimensional array | Runtime shape, many dtypes, broadcasting and views | Scientific arrays; spatial algorithms are one subdomain | Bulk array operations and comprehensive scientific algorithms | Weak semantic distinction between point, vector, unit, frame, and index |
| Eigen | Generic matrix/expression templates | Compile-time or runtime dimensions; generic coefficient type | Linear algebra and transformations | Dense/sparse solvers, decompositions, views, and host optimization | C++-specific and matrix-centric rather than a domain vocabulary |
| nalgebra | Generic matrix plus typed geometric transformations | Compile-time or runtime dimensions; generic scalar and storage | Linear algebra, points, rotations, isometries, projections | Closest match to Plato's type safety, with much deeper matrix machinery | Rust-only; does not provide Plato's broad curves, fields, solids, and graphics model |
| GLM | GLSL-shaped vectors, matrices, and functions | Small fixed shapes and several native scalar types | Real-time graphics mathematics | Familiar shader conventions and direct C++ implementation | Portability is by convention or separate ports, not one generated source |
| CGAL | Kernel concepts, geometric objects, and algorithms | Selectable number types and robustness kernels | Exact/robust computational geometry | Reliable predicates, constructions, arrangements, hulls, triangulations, and mesh algorithms | Heavy host-specific machinery that cannot map uniformly to shader targets |
| OCCT | Topological B-rep entities linked to geometric curves and surfaces | Floating-point geometry governed by tolerances | Industrial surface and solid modeling | Production Booleans, sweeps, lofts, fillets, healing, STEP/IGES exchange, and document history | Large mutable C++ CAD kernel; tolerance and object-identity semantics do not map uniformly across Plato targets |
| libigl | Vertex/face and operator matrices | Eigen dense/sparse matrices, usually host-native scalar types | Discrete differential geometry and mesh processing | Complete operators and workflows built on mature sparse algebra | Less semantic typing; topology is often reconstructed from matrices |
| Geometry Central | Halfedge mesh plus associated geometry | Native buffers and typed mesh elements | Intrinsic and extrinsic surface geometry | Efficient traversal, dynamic topology, geodesics, direction fields, and intrinsic Delaunay tools | Mutable C++ data structures do not translate directly to Plato's pure value model |
| Shapely/GEOS | OGC-style planar point sets | Double-precision coordinate sequences | 2D topology, predicates, Boolean and constructive operations | Coherent, well-established planar behavior and robust workflows | Strictly planar analysis; no analytic curves, shaders, or high-dimensional numeric layer |
| Wolfram Language | Symbolic expressions and geometric regions | Exact, arbitrary-precision, and machine numeric evaluation | Integrated geometry, algebra, solvers, and visualization | One region model participates in symbolic reduction, integration, optimization, discretization, and PDE workflows | A heavyweight execution environment rather than a portable generated library |
| Three.js | Mutable scene graph plus GPU-oriented buffer geometry | JavaScript numbers and typed attribute buffers | Interactive browser 3D rendering | Complete scene, material, animation, loader, and WebGL/WebGPU runtime | Weaker domain semantics and no host-neutral code generation; behavior is coupled to its renderer and resource lifecycle |

## Findings by design axis

### 1. Semantic typing is Plato's clearest advantage

NumPy treats positions, vectors, normals, colors, temperatures, and indices as arrays
whose meaning lives outside the type. Eigen and GLM add compile-time shapes, but a
three-component vector commonly carries many unrelated roles. Plato instead makes
`Point3D`, `Vector3D`, `Direction3D`, `Color`, `Length`, `Angle`, `VertexIndex`, and
`FaceIndex` separate nominal types. The consequences are valuable: translation can
apply to a point without applying to a displacement; an angle need not be confused
with an arbitrary scalar; and topology indices from different domains do not
interchange accidentally.

nalgebra is the closest external comparison. It also distinguishes points from vectors
and provides dedicated rotations, isometries, similarities, and projections; its
[transformation guide](https://www.nalgebra.rs/docs/user_guide/points_and_transformations/)
explains how those distinctions constrain legal operations. Its `Unit<T>` wrapper also
shows a useful step beyond naming: the type represents a checked normalization
invariant. Plato should preserve its richer nominal vocabulary while auditing whether
types such as directions, unit axes, valid polygons, and rigid frames merely state an
intent or actually establish it at construction boundaries.

The cost is conversion and generic complexity. Every nominal wrapper needs operations,
hashing, equality, serialization, and interop. That cost is justified for domain
semantics but not for arbitrary shape aliases. Plato should continue to create named
types when they prevent a real category error, not just to mirror names found in other
libraries.

### 2. Plato has a numeric vocabulary, not yet a numerical computing engine

[NumPy's](https://numpy.org/doc/stable/user/whatisnumpy.html) central abstraction is the
homogeneous `ndarray`; vectorization and broadcasting make elementwise behavior the
default. [SciPy](https://docs.scipy.org/doc/scipy/tutorial/index.html) builds FFTs,
integration, interpolation, linear algebra, image processing, optimization, sparse
arrays, spatial algorithms, special functions, and statistics on that base. Eigen and
nalgebra take a different route but still provide generic scalar types, fixed and
dynamic dimensions, submatrix views, and choices such as LU, QR, Cholesky, SVD, and
eigendecomposition. Their solver depth is documented in
[Eigen's linear-algebra guide](https://libeigen.gitlab.io/eigen/docs-nightly/group__TutorialLinearAlgebra.html)
and [nalgebra's decomposition guide](https://nalgebra.rs/docs/user_guide/decompositions_and_lapack/).

Plato has useful pieces: component vectors, fixed transformation matrices, `MatrixN`,
`SparseMatrix`, `BandedMatrix`, descriptive statistics, distributions, special
functions, polynomials, complex and rational records, and forward-mode dual numbers.
Its fixed transform bodies are directly useful for geometry and graphics. What it does
not yet have is the connective numerical substrate of the mature libraries:

- no general strided or borrowed array/matrix views;
- no NumPy-style shape algebra or broadcasting contract;
- no scalar-generic matrix family spanning float32, float64, complex, and user types;
- no general decomposition and solver catalog; and
- no demonstrated backend dispatch to BLAS, LAPACK, or equivalent native kernels.

This should not trigger a SciPy imitation project. Most of SciPy cannot be represented
meaningfully in GLSL, and a pure source implementation would usually lose to established
host kernels. Plato needs a small portable layer—small matrices, reductions, selected
special functions, and algorithms needed by its own geometry—plus explicit backend
overrides for expensive operations. The planned override mechanism in
[`plato-368`](../../tracker/issues/plato-368.md) is therefore more strategically
important than adding a large declaration catalog.

### 3. Plato can be better than GLM at semantics, but only if generated targets agree

GLM deliberately follows GLSL names and conventions, which makes C++ and shader code
look alike. Its project documentation also lists separate adaptations for other
languages. Plato's proposition is stronger: write the behavior once and generate the
host and shader forms. It can also improve on GLM's raw-vector style through `Angle`,
points versus displacements, frames, poses, and explicit affine/projective types.

Today that advantage is only partially realized. The writers support different subsets,
and the whole forward library is not execution-gated across targets. A portable API is
not established merely because each backend can spell `Vector3D`; representative laws
must prove that multiplication order, handedness, angle units, interpolation, boundary
conditions, and failure behavior agree. Plato's conventions already decide many of
these matters. Cross-target law execution is what would turn those decisions into a
credible advantage.

### 4. Three.js exposes the graphics tier's missing runtime boundary

[Three.js](https://github.com/mrdoob/three.js/) is not a numerical library or geometry
kernel. It is a general-purpose JavaScript 3D library whose value comes from connecting
math and geometry to an operational browser runtime. Its
[fundamental model](https://threejs.org/manual/en/fundamentals.html) combines a renderer,
scene, camera, meshes, geometry, materials, lights, and a render loop. The
[scene graph](https://threejs.org/manual/en/scenegraph.html) supplies hierarchical local
transforms, while
[`BufferGeometry`](https://threejs.org/docs/pages/BufferGeometry.html) defines the
indexed attributes, morph targets, groups, and GPU-buffer conventions consumed by the
renderers. Current renderer documentation covers both
[WebGL](https://threejs.org/docs/pages/WebGLRenderer.html) and
[WebGPU](https://threejs.org/docs/pages/WebGPURenderer.html), with a WebGL 2 fallback for
the latter.

Plato's `graphics` tier names many of the same concepts, but naming a scene, camera,
material, or texture is not equivalent to managing mutable object hierarchies, GPU
resources, loaders, animation state, and frame submission. Recreating that runtime in
the standard library would also conflict with Plato's pure, multi-target role. The
useful boundary is therefore declarative: Plato should generate portable scene and
geometry data, perform pure preprocessing, and define stable buffer, transform, and
material semantics; a TypeScript adapter can then construct and update Three.js
objects. Other targets should be able to bind the same semantics to their native
renderers without pretending that renderer APIs are portable.

Three.js also supplies an important verification target. A small generated scene that
reaches an actual renderer would test coordinate conventions, matrix ordering, normals,
indices, texture coordinates, colors, and ownership rules together. That is more
valuable than adding further graphics declarations without an end-to-end consumer.

### 5. Analytic and procedural geometry is a real Plato strength

CGAL, Shapely, libigl, and Geometry Central are strongest when geometry is already
represented as exact primitives, planar point sets, polygonal meshes, or discrete
operators. Plato covers those forms but also gives first-class vocabulary to parametric
curves and surfaces, scalar/vector fields, SDFs, deformations, procedural textures,
sampling, and conversions between continuous and discrete views.

That breadth is unusually coherent. A curve can participate in evaluation,
differentiation, framing, deformation, and sampling through shared interfaces. A shape
can expose bounds, containment, support, a parametric surface, or an SDF without being
reduced to one universal matrix representation. This is a better fit for procedural
modeling, shaders, educational geometry, and generated kernels than the data-first
models of the comparison libraries.

The weakness is that many of these types are catalogs rather than completed workflows.
A broad set of surface names is less valuable than reliable evaluation, tessellation,
normal generation, bounds, and cross-target examples for a smaller set. Future content
work should be judged by whether it completes such a path.

### 6. Wolfram shows how one region model can connect geometry and solvers

The [Wolfram Language computational-geometry
guide](https://reference.wolfram.com/language/guide/ComputationalGeometry.html) treats
geometric regions as symbolic expressions that can be generated, transformed,
queried, rendered, and combined with the rest of the language. Its
[mesh-region system](https://reference.wolfram.com/language/guide/MeshRegions.html)
connects boundary and volumetric meshes to Delaunay and Voronoi constructions,
discretization, repair, simplification, and smoothing. The
[geometric-solvers guide](https://reference.wolfram.com/language/guide/GeometricSolvers.html)
then lets regions participate in exact or approximate integration, equation and
inequality solving, optimization, and PDE workflows.

This is an adjacent comparison, not evidence that Plato should acquire a computer
algebra system. Wolfram can defer representation and numerical choices inside a large
symbolic runtime; Plato must lower explicit types and bodies ahead of time into several
targets with much smaller execution models. The transferable lesson is narrower: a
region should retain consistent containment, boundary, measure, transformation, and
discretization semantics as it moves between analytic, implicit, and mesh forms.

Plato already has much of the vocabulary needed for that composition. Its opportunity
is to make conversions and solver inputs explicit and testable: for example, define
when a field or parametric surface can be discretized, what approximation contract the
result satisfies, and which integration or optimization routines accept the result.
It should borrow Wolfram's uniformity without copying its symbolic execution model.

### 7. Robust computational geometry is the largest technical gap

[CGAL's kernels and packages](https://doc.cgal.org/latest/Manual/packages.html)
deliberately separate geometric objects and algorithms from number types and robustness
choices. The catalog includes exact or filtered arithmetic, convex hulls, Delaunay
triangulations, arrangements, polygon repair, regularized Boolean operations, offsets,
and mesh processing. [Shapely's manual](https://shapely.readthedocs.io/en/stable/manual.html)
describes how it delegates planar operations to GEOS and exposes a consistent point-set
model with validity predicates, relationships, Boolean operations, constructive
operations, and an STR tree.

Plato currently uses a `Number` wrapper over a host `float` on C#, and there is no
equivalent exact/filtered kernel abstraction. It has many elementary intersections,
containment tests, polygon validation helpers, ear-clipping triangulation, bounds,
spatial records, and SDF Boolean combinators. Those are valuable, but they are not a
replacement for robust orientation/incircle predicates, arrangement construction, or
topology-preserving polygon overlay. The library can represent convex-hull results, for
example, while its own comments state that hull construction still needs missing
sorting support.

Plato cannot simply adopt CGAL's exactness everywhere: arbitrary precision does not fit
uniformly on GPU targets. It needs an explicit policy:

- portable approximate predicates with stated tolerances and degeneracy behavior;
- filtered or exact CPU implementations where topology depends on a sign decision;
- backend capability reporting when an operation has no faithful target form; and
- common test vectors that exercise nearly degenerate cases.

The existing robust-predicate item
[`plato-255`](../../tracker/issues/plato-255.md) is foundational, not optional polish.

### 8. OCCT defines a separate industrial CAD interoperability problem

[Open CASCADE Technology's overview](https://dev.opencascade.org/doc/overview/html/)
describes a C++ platform spanning geometric curves and surfaces, topological B-rep
entities, intersections and projections, tessellation, shape validation and healing,
feature construction, and visualization. Its high-level modeling algorithms include
primitives, sweeps, revolutions, pipes, lofts, fillets, chamfers, shells, drafts, and
solid Booleans. The dedicated
[Boolean operations](https://dev.opencascade.org/doc/overview/html/specification__boolean_operations.html)
include fuse, common, cut, section, and splitting with history information. Its
[data-exchange facilities](https://dev.opencascade.org/about/data_exchange) cover STEP,
IGES, glTF, VRML, and STL, with extended document metadata and shape-healing support.

This makes OCCT more relevant to Plato's B-rep, curve, surface, and solid vocabulary
than another mesh library would be. It also exposes a categorical gap. A production CAD
kernel needs persistent topological identity, tolerance propagation, geometric and
topological validity, repair, modeling-operation history, rich surface types, and
industrial interchange. A collection of immutable B-rep records and portable reference
bodies is not equivalent to that system, even when both expose names such as face,
shell, solid, Boolean, or loft.

Plato should not reproduce OCCT. It should define the portable semantic subset that is
useful in generated kernels—profiles, sampled curves and surfaces, topology records,
transformations, and well-specified tessellation inputs and outputs—and establish a
host capability boundary for full CAD operations. An OCCT adapter should preserve
tolerances, provenance, identity, and failure diagnostics rather than flattening every
result immediately to a triangle mesh. This also means that CAD interchange and shape
healing belong behind a CPU-host service contract, not in a supposedly universal GPU
standard library.

### 9. Plato's mesh model sits between libigl and Geometry Central

[libigl's canonical triangle mesh](https://libigl.github.io/tutorial/) is a vertex
matrix `V` plus a face-index matrix `F`. That representation is compact, serializable,
and immediately compatible with Eigen's dense and sparse operations. It enables a deep
chain of cotangent Laplacians, mass matrices, curvature, parameterization, deformation,
geodesics, and optimization.

[Geometry Central](https://geometry-central.net/) starts from explicit halfedge
connectivity and associated geometry. Its design supports efficient neighborhood
traversal, dynamic topology, intrinsic edge-length geometry, direction fields,
geodesic distance, and intrinsic Delaunay triangulations. This is stronger when
algorithms repeatedly navigate or edit topology.

Plato supports indexed triangle, quad, polygon, tetrahedral, and hexahedral data, typed
indices, incidence interfaces, a halfedge record, and substantial pure remeshing bodies
for welding, splitting, collapsing, flipping, and smoothing. That is promising: it can
retain a serialization-friendly array representation while deriving richer topology.
However, immutable reconstruction may allocate much more than Geometry Central's
dynamic mesh, and Plato lacks libigl's mature sparse-operator spine. The remeshing work
is also newer than the current executable conformance path.

The next mesh milestone should connect existing vocabulary into a verified sequence:
mesh validation and incidence, normals and mass properties, cotangent and mass
operators, curvature/geodesic queries, then topology edits with preserved invariants.
The relevant existing work includes [`plato-423`](../../tracker/issues/plato-423.md)
for remeshing and [`plato-427`](../../tracker/issues/plato-427.md) for finite-element
operators. More mesh type names are not the bottleneck.

### 10. Planar geometry needs one coherent point-set contract

Plato has polygons with holes and sets, signed area, winding normalization, simplicity
checks, containment, nearest-point operations, and triangulation. It does not expose an
end-to-end boundary-representation workflow comparable to Shapely/GEOS or CGAL for
polygon union, intersection, difference, buffering/offsetting, repair, and validity.
SDF unions and offsets are different operations: they combine fields and approximate
boundaries rather than preserving an exact polygonal point set.

Shapely's value here is not its Python syntax. It is the coherence of the data model:
interior, boundary, and exterior define predicates and set operations; invalid inputs
have explicit diagnostics; and single and multi-geometries share a predictable result
model. Plato should define that semantic contract before implementing isolated Boolean
functions. [`plato-415`](../../tracker/issues/plato-415.md) already captures the
polygon-expansion work and should depend on the numerical robustness policy rather than
inventing a separate tolerance convention.

### 11. Purity and code generation change what “standard library” should mean

The external projects can rely on host mutation, dynamic allocation, platform SIMD,
threading, native dependencies, and exceptions or panics. Plato intentionally cannot
assume all of those features. That restriction is productive for small mathematical
kernels: reference bodies are inspectable, deterministic, composable, and portable.
It becomes counterproductive when Plato reimplements large decompositions or mutable
topology engines solely to preserve source purity.

The standard library should therefore distinguish three implementation classes:

1. **Portable reference:** compact pure bodies expected to run everywhere.
2. **Backend-accelerated:** the same semantics implemented by Eigen, LAPACK, GEOS,
   hardware intrinsics, or another host facility where available.
3. **Backend-specific capability:** useful operations, such as exact polygon overlay,
   OCCT-backed CAD modeling, or Three.js scene realization, that honestly cannot promise
   the same implementation or representation on every target.

That split is more honest and useful than forcing the least capable backend to define
the whole library.

## Recommendations

| Priority | Recommendation | Consequence |
|---|---|---|
| Now | Finish executable forward conformance before expanding the surface | A standard-library claim will mean generated code actually compiles and laws execute, not only that declarations resolve. |
| Now | Make maturity/capability machine-readable per domain and backend | Consumers can distinguish vocabulary, lowered bodies, compiled targets, and executed laws. [`plato-388`](../../tracker/issues/plato-388.md) is the natural home for the shared policy data. |
| Now | Define a numerical and degeneracy policy | Comparisons, normalization, inversion, intersections, and topology-changing predicates will share explicit behavior instead of local epsilon choices. |
| Next | Implement the backend override boundary | Portable bodies remain the specification while host libraries supply expensive kernels without forking the API. |
| Next | Build only geometry-driven numerical kernels | Prioritize symmetric 2x2/3x3 eigenproblems, small linear solves, matrix-free iteration, and sparse operators demanded by fitting, curvature, FEM, and remeshing; do not clone SciPy. |
| Next | Complete planar point-set operations as one workflow | Polygon validity, repair, overlay, offset, and triangulation will compose under one result and robustness model. |
| Next | Complete a mesh-operator spine | Indexed storage, incidence, halfedges, discrete operators, and topology edits will form a usable pipeline rather than parallel vocabularies. |
| Next | Specify interop layouts and zero-copy boundaries | Generated code can hand dense buffers, sparse structures, vertices, faces, and scalar spans to mature native libraries without conversions hidden in hot paths. |
| Next | Define a graphics-runtime adapter boundary, using Three.js as the first executable consumer | Scene data, transforms, materials, buffers, and ownership will have portable semantics while browser rendering remains in the host runtime. |
| Next | Define a CAD capability and provenance boundary for OCCT-class hosts | Plato can request and exchange B-rep operations without claiming that tolerance-sensitive modeling, healing, history, or STEP/IGES support is portable to every backend. |
| Next | Connect regions to discretization and selected solvers | Plato gains Wolfram-like compositional value across analytic, implicit, and mesh forms without attempting to embed a symbolic algebra system. |
| Always | Preserve semantic types and strengthen their invariants | Plato keeps the advantage that NumPy, Eigen, libigl, GLM, Three.js, and many CAD APIs generally leave to caller discipline. |
| Avoid | Treating every external symbol as a port candidate | The library stays small enough to verify and portable enough to generate. |
| Avoid | Building a CAD kernel, symbolic engine, or renderer inside the standard library | OCCT, Wolfram Language, and Three.js remain host capabilities and integration targets rather than portability constraints. |

## Bottom line

Plato's most credible identity is **not** “NumPy or CGAL rewritten in a small language,”
and it should not become a smaller OCCT, Wolfram Language, or Three.js.
It is a semantic geometry vocabulary with portable reference implementations and
target-specific acceleration. Its best features—points distinct from vectors, explicit
angles and quantities, typed topology, shared continuous/discrete abstractions, and one
source for several targets—are genuinely differentiated.

Its present risk is breadth without operational confidence. NumPy/SciPy, Eigen, CGAL,
OCCT, libigl, Geometry Central, Shapely, Wolfram Language, and Three.js all demonstrate
that useful systems are built around a small number of powerful representations plus
deep, tested workflows. Plato
should take that lesson without copying their host-specific machinery: verify the core,
state numerical behavior, finish the workflows its geometry needs, and delegate mature
native problems through explicit backend contracts.
