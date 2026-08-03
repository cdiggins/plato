# Plato Library — Brainstorm of New Additions

*Companion to `reports/plato-library-review.md`. Grounded in `stdlib-legacy`, `Ara3D.Geometry`, and
`ara3d-sdk/examples/Ara3D.Studio.Examples` (Deformers, Clone, SweepDemo, VoxelDemo, SurfaceGenerators…).*

## 0. Two cross-cutting design ideas that multiply everything below

### 0.1 Scalar fields as the universal modulator ("effectors")
Almost every feature requested here — deformations with falloff, cloner effectors, soft selection,
vertex-color-from-analysis, SDF blending — is the same idea: **a scalar field `IProcedural<Point3D, Number>`
modulating an operation.** The MoGraph/Houdini insight. If the library treats "weight field" as a
first-class interface, then:
- every deformation gets falloff for free: `Deform(mesh, warp, weightField)` = lerp(p, warp(p), w(p));
- every cloner gets effectors for free (scale/rotate/color clones by field value);
- selection becomes a field (`SphereFalloff`, `BoxFalloff`, `NoiseField`, `DistanceToMeshField`);
- analysis results (curvature, height, distance-to-object) plug in as inputs.
One interface, one composition rule, dozens of features. This should shape the API before any individual
deformer is added.

### 0.2 Un-comment `Procedural` (function-valued fields) — or use combinator *types*
The whole compositional layer (SDF booleans, domain warps, falloffs, curve reparameterization) needs
values that hold functions. Two routes:
1. Finish compiler support for `Procedural<TIn,TOut>` holding a `Function1` (the library body already
   sketched in `procedurals.plato` is commented out).
2. Meanwhile: **combinator types** — pure data, no lambdas:
   ```plato
   type SmoothUnion3D implements ISdf3D { A: ISdf3D?; ... }   // needs existentials — hard
   ```
   …which shows why route 1 matters: without function fields or existentials, every combination must be
   monomorphized at the use site. Generic library functions (`Union(a: ISdf3D, b: ISdf3D)`) *do* work when
   the concrete types are known statically — good enough for codegen targets like GLSL, actually — but a
   dynamic scene graph needs route 1 on the C# side (which `Sdf3D` already hand-implements).

---

## 1. Signed distance fields (IQ's catalogs)

The single best content addition. SDFs are pure formulas with citations — maximally Plato-shaped — and
they're the bridge to voxelization, marching cubes, booleans, shelling, filleting, and (later) GLSL.
`IDistanceField2D/3D` already exist in `geometry.interfaces.plato`; today nothing implements them.

### 1.1 3D primitives (from https://iquilezles.org/articles/distfunctions/)
Sphere, Box, RoundBox, BoxFrame, Torus, CappedTorus, Link, InfiniteCylinder, Cone, InfiniteCone, Plane
(half-space), HexPrism, TriPrism, Capsule/VerticalCapsule, CappedCylinder, RoundedCylinder, CappedCone,
SolidAngle, CutSphere, CutHollowSphere, DeathStar, RoundCone, Ellipsoid (approx — note IQ's caveat),
Rhombus, Octahedron (exact + fast approx), Pyramid, Triangle (unsigned), Quad (unsigned).
Most are 2–6 lines. The existing solids in `solids.plato` should each get `Distance(x, p)` so the same
`Sphere` value is simultaneously a parametric surface (§2) and an SDF — one type, two views. That duality
is a Plato signature move no other library does cleanly.

### 1.2 2D primitives (from https://iquilezles.org/articles/distfunctions2d/)
Circle, RoundedBox, Box, OrientedBox, Segment, Rhombus, Trapezoid, Parallelogram, EquilateralTriangle,
IsoscelesTriangle, Triangle, UnevenCapsule, Pentagon, Hexagon, Octagon, Hexagram, Star5, StarN, Pie,
CutDisk, Arc, Ring, Horseshoe, Vesica, Moon, RoundedCross, Egg, Heart, Cross, RoundedX, Polygon (array),
Ellipse (the good iterative one), Parabola, ParabolaSegment, QuadraticBezier, Tunnel, Stairs, Hyperbola.
Bonus: these give the 2D layer (currently the poor cousin) real content, and `Lens`/`Ring`/`Arc` types
that today have zero functions get their first real operations.

### 1.3 Operators
- **Booleans**: Union/Intersect/Subtract + smooth variants (polynomial `smin` — quadratic, cubic; note
  the review's `Union => Max` in the commented-out procedurals library has the SDF convention *backwards*:
  SDF union is `Min`).
- **Modifiers**: Round (offset), Onion (shell — instant "3D-printable wall thickness" feature), Elongate,
  and Scale (with the division-by-scale correction).
- **Domain operations** (these double as §3 deformations): Symmetry (abs), finite & infinite Repetition
  (the instancing-without-instances trick), Twist, CheapBend, Displacement (needs noise §9).
- **Queries**: normal via central differences (6-tap) and tetrahedron trick (4-tap); `GradientAt`;
  sphere-trace `RayMarch(sdf, ray, ...)` — pure, portable, and the basis of the eventual shader target.
- **Conversions**: `Extrude(sdf2d, height) → ISdf3D`, `Revolve(sdf2d, offset) → ISdf3D` (IQ's exact
  dimension-lifting ops — cheap and extremely useful), `Slice(sdf3d, plane) → ISdf2D`.

### 1.4 Fields beyond distance
Once `IProcedural<Point3D, Number>` has a family: density fields, falloff fields (sphere/box/cone/curve
falloff with smoothstep profiles), metaballs (sum of kernels — classic), and the effector layer of §0.1.

---

## 2. Parametric surfaces

`SurfaceGenerators.cs` currently reflects over the ChatGPT-drafted C# `SurfaceFunctions`; all of these are
one-liner formulas that belong in Plato with citations.

### 2.1 Catalog (types implementing IProceduralSurface, like the curve pattern)
- **Quadrics & friends**: Paraboloid, Hyperboloid (1 & 2 sheet), HyperbolicParaboloid (saddle), Superellipsoid
  (Barr's exponents — subsumes sphere/box/cylinder/octahedron in one 2-parameter type), Supertoroid.
- **Minimal & classic**: Helicoid, Catenoid (and the helicoid↔catenoid isometric morph — a killer demo),
  Enneper, Scherk. Möbius strip (exists as an example — promote), Klein bottle (figure-8 immersion),
  Boy's surface, Roman surface, Whitney umbrella, Dini's surface, pseudosphere.
- **Shells & organic**: the seashell family (turritella/nautilus — logarithmic spiral sweep with parameters),
  torus knot tube (combine existing TorusKnot curve with tube sweep §2.2).
- **Terrain/graph**: `Graph(f: IExplicitSurface)` — the height-field lift already half-exists.

### 2.2 Surface *constructors* (more valuable than any single surface)
The examples (SweepDemo, RuledSurface, PipesDemo, Revolve in Primitives.cs) show these are the real
workhorses. Make them Plato-level algebra:
- `Extrude(profile: ICurve2D, height)` / `ExtrudeAlong(profile, direction)`
- `Revolve(profile: ICurve2D, axis, angleRange)` — surfaces of revolution (subsumes sphere, torus, vase…)
- `Ruled(a: ICurve3D, b: ICurve3D)` — already C#; trivial in Plato
- `Loft(profiles: IArray<ICurve3D>)` — ruled generalized with a V-direction spline
- `Sweep(profile: ICurve2D, path: ICurve3D, frames)` / `Tube(path, radius)` — needs frames (§2.3)
- `CoonsPatch(c0, c1, c2, c3)` — boundary-defined patches
- `BilinearPatch(quad)`, `BezierPatch(4×4 points)`, B-spline/NURBS patch (arrays of control points — fits)
- Domain operations: `TransposeUV`, `FlipU/V`, `SubPatch(surface, uvBounds)`, periodic wrapping.

### 2.3 Frames along curves (prerequisite for sweep/tube done right)
`GetTransforms` in GeometryUtil uses look-at with a fixed up — it will flip on vertical tangents.
Add: FrenetFrame (exists as type, no functions!), and **rotation-minimizing frames** (Wang et al.'s double
reflection method — the correct default for sweeps; it's ~10 lines of pure math over sampled points).
Plus `TangentAt/NormalAt/BinormalAt(curve, t)` via central differences generically for any ICurve3D.

---

## 3. Geometric operations & deformations

### 3.1 Space warps (pure, Plato-level — `Point3D → Point3D`)
The example deformers (Twist, Skew, Spherify, Cubify) are all pure formulas trapped in C# classes.
Catalog: Twist (about arbitrary axis), Bend (circular arc bend — the classic Barr deformer trio is
**Twist/Taper/Bend**, only Twist exists), Taper (linear/curve-profiled), Shear/Skew, Bulge, Squash&Stretch
(volume-ish preserving), Wave/Ripple (radial & planar), Spherify/Cubify, Mirror, RandomJitter (needs hash
§9), FFD-lite (trilinear lattice of control points — an `IArray3D<Vector3>` field, no mutation needed),
Wrap-around-cylinder / wrap-around-sphere (map a flat mesh onto a curved substrate — great for facades),
ProjectOntoPlane/Sphere/SDF-surface (project along direction or gradient).
All get falloff via §0.1. All work on anything `IDeformable3D` — meshes, polylines, point clouds — for free.

Note on correctness: `Deform` currently moves points only; normals need the inverse-transpose Jacobian.
Either document "recompute normals after deform" as the contract, or add `DeformWithJacobian` for the
analytic warps (all Barr deformers have closed-form Jacobians).

### 3.2 Mesh operations (C#-side, using the Topology library)
- **Smoothing** (user-requested): Laplacian (uniform & cotangent weights), **Taubin λ/μ** (no shrinkage —
  the one people actually want), HC-Laplacian, per-feature-preserving (limit by dihedral angle — the
  crease machinery already exists in FaceGroupsByCreaseAngle).
- **Subdivision**: Catmull-Clark (QuadMesh3D — natural fit, the naive quad Subdivide exists), Loop
  (TriangleMesh3D), simple linear subdivide + project-to-SDF (poor man's remesher).
- **Slicing** (user-requested): MeshPlaneClip exists; add: multi-plane slicing → contour stacks
  (3D-printing style `Slice(mesh, plane spacing) → IArray<PolyLine3D loops>`), boolean-with-half-space,
  cap generation for cut faces (PolygonTriangulator exists — wire it in), `SliceByGrid` for kitchen-sink
  sectioning diagrams (WallPlanDiagram example wants this).
- **Shelling/offset**: offset surface along vertex normals (Push exists) → closed shell (offset + flip +
  stitch rims) — hugely useful for BIM/printing; exact version via SDF Onion + remesh.
- **Insetting/extrusion per face**: face extrude/inset (the Quad3D Inset exists; generalize to mesh faces
  via topology) — this plus §4 Conway operators covers a lot of procedural modeling.
- **Decimation**: edge-collapse with quadric error metrics (QEM) — the standard, pairs with the existing
  IsotropicRemesher.
- **Repair**: fill holes (boundary loops already extractable), fix winding orientation (BFS over adjacency),
  remove degenerate/sliver triangles, weld (exists), non-manifold split.

---

## 4. Polyhedra & primitive geometry

### 4.1 Catalog (data, mostly, not formulas)
Platonic (exist) → **Archimedean** (13: cuboctahedron, icosidodecahedron, truncated & snub forms…),
**Catalan** duals (rhombic dodecahedron & triacontahedron especially — they tile / they're zonohedra),
**prisms & antiprisms** (parametric N — trivial), selected **Johnson solids** (J1 square pyramid, J12/J13
bipyramids, J26 gyrobifastigium for fun), **Kepler–Poinsot** star polyhedra (render candy),
**geodesic spheres** (icosphere subdivision — arguably the most-used primitive missing from the whole SDK)
and their duals the **Goldberg polyhedra** (hex-tiled spheres), **zonohedra** from vector stars,
**rhombic tilings**. Store as canonical vertex/face data tables (like PlatonicSolids.cs) or generate.

### 4.2 Conway operators (the multiplier)
Instead of hand-coding 40 polyhedra, implement **Conway polyhedron notation** on top of the Topology
library: dual (d), ambo (a), kis (k), truncate (t), join (j), gyro (g), snub (s), ortho (o), expand (e),
bevel (b), meta (m). Every Archimedean/Catalan solid is then a 1–3 character program on a Platonic seed
(`aC` = cuboctahedron, `dtI` = pentakis dodecahedron…). Compact, compositional, very on-brand, and it
exercises/showcases the topology layer. `Icosphere = uu(I)` style subdivision falls out too.

### 4.3 Other primitives
Rounded box / rounded cylinder meshes (from their SDFs or direct), wedge, torus segment, spring/coil
(helix tube — PipesDemo wants it), gear profile (2D) + extrude, arrow (exists in Primitives — generalize),
LineMesh frames/gizmos: axis tripod, grid, protractor arcs (Studio UI needs these constantly).

---

## 5. Cloning & instancing

The examples (GridClone, CloneCubeOnMesh, ClonePyramidOnFace, CurveCloneDemo) show the pattern: generate
`IReadOnlyList<Point3D>` or transforms, call `mesh.Clone(material, positions)`. Formalize the *generator*
side in Plato — pure `Integer → Pose3D` / arrays of poses:
- **Distributors**: LinearArray(count, step), GridArray (2D/3D, exists ad hoc), RadialArray (circle with
  optional aim-at-center), CurveDistribute (by parameter or by arc length §7), SurfaceDistribute (UV grid,
  or per-face with barycentric jitter — ClonePyramidOnFace generalized), HelixDistribute, honeycomb/hex
  grid, Fibonacci sphere/disk (phyllotaxis — beautiful and 3 lines), Poisson-disk scatter (needs PRNG §9).
- **Effectors** (§0.1): per-clone transform/color/visibility modulated by scalar fields — `Falloff(center,
  radius)`, noise fields, `DistanceToCurve` fields. `Fractal.cs` and facade examples are hand-rolled
  versions of exactly this.
- **Instance-aware types**: a Plato `Pose3D`-array is the portable currency; `Model3D`/`InstanceStruct`
  stays the C# render-side sink. Consider `ClonedGeometry<T> { Prototype: T; Poses: IArray<Pose3D> }` as a
  lazy value — SDF repetition (§1.3) is its continuous cousin; nice conceptual symmetry.
- **L-systems / rewriting** (stretch): turtle interpretation producing pose arrays — pure, compact, huge
  demo value (trees, coral for RadialObjectAnalyzer-style scenes).

---

## 6. Additional transform types

- **Mirror/Reflection3D** — `Reflection2D` exists as an *empty* type (no fields, no Matrix); both need
  `Plane`-based definition (`Matrix4x4.CreateReflection` intrinsic already wrapped!).
- **RotateAboutPoint3D / ScaleAboutPoint3D** — the 2D `Offset*` variants exist; 3D missing. Constantly needed.
- **Shear3D** (Skew2D exists, 3D doesn't — SkewDeformer example hand-rolls it).
- **Similarity3D** (rigid + uniform scale) — closed under composition, preserves shape; the natural type for
  instancing.
- **Closed composition for rigid types**: `Pose3D * Pose3D → Pose3D`, `Invert(Pose3D) → Pose3D` without
  round-tripping through Matrix4x4 (precision + keeps the rigid invariant). Same for Rotation3D
  (quaternion multiply — exists as intrinsic, expose as transform composition).
- **DualQuaternion** — for blending rigid transforms (skinning, smooth pose interpolation; ScLERP). Pure,
  well-cited, fills the "interpolate a Pose3D" hole that Matrix lerp does wrong.
- **ChangeOfBasis / CoordinateFrame transforms**: `Frame3D` exists C#-side; add Plato `Frame3D` (origin +
  orthonormal basis) with ToWorld/ToLocal — then Y-up↔Z-up and RH↔LH conversions become named constants
  (`YUpToZUp`), documenting the convention (§5 of the review) executably.
- **Projections**: PlaneProjection3D exists; add oblique/cabinet, spherical/cylindrical (environment
  mapping, panoramas), and `ProjectOntoSphere`.
- **TransformPath**: `IProcedural<Number, Pose3D>` — a *transform-valued curve* (what GetTransforms
  approximates). Unifies sweeps, cloners along curves, and camera paths in one interface.

---

## 7. Curves: UV/parameter machinery

- **Arc-length reparameterization**: `ArcLengthTable(curve, samples) → IArray<Number>` + `EvalByLength`,
  `EvalUniform`. Prerequisite for even cloner spacing and correct sweeps. Pure (arrays, no mutation).
- **Curve queries**: `TangentAt`, `NormalAt`, `CurvatureAt` (finite difference generic; analytic where
  Algebra derivatives exist — they're already written!), `ClosestParameter(curve, point)` (Newton +
  sampling seed), `Length(curve)`.
- **Curve ops**: `Subdivide/Trim(curve, t0, t1)` (interval Subdivide exists — connect it), `Join`,
  piecewise curves (`PolyCurve { Segments: IArray<ICurve> }`), fillet between segments, `Offset(curve2D,
  distance)` (approximate via normal offset + resample), Douglas-Peucker simplification for polylines,
  smoothing (Chaikin corner-cutting — pure and elegant), B-spline & Catmull-Rom *curve types* (the basis
  functions exist in Algebra; the array-of-control-points types don't!).
- **UV domain warps for surfaces** (`Vector2 → Vector2`): rotate/scale/tile/mirror UV, seam shift,
  equal-area sphere mapping. Cheap, composable, makes the surface catalog (§2) far more expressive.

---

## 8. Statistics, analysis, topology

- **Mass properties**: surface area (sum exists?), **enclosed volume** (divergence theorem — signed tet
  sum, 5 lines), centroid, **inertia tensor** (Mirtich/Eberly — well-cited), is-watertight, is-convex.
- **Curvature**: Gaussian via angle defect (needs topology — cheap), mean via cotan Laplacian, principal
  directions via quadric fitting. Output as vertex scalar fields → feeds §0.1 effectors and vertex-color
  examples directly (VertexNormalColor.cs shows the demand).
- **Shape descriptors**: convex hull (quickhull — C#-side), OBB (PCA exists — wire to OrientedBox3D),
  minimal enclosing sphere (Welzl), best-fit plane/line/sphere/cylinder (GeometryFitting exists — extend),
  elongation/flatness ratios (from PCA eigenvalues), Euler characteristic & genus (Topology has the data),
  distributions: edge length, dihedral angle, triangle quality histograms (TessellationStats exists —
  round out).
- **Descriptive stats in Plato**: mean/variance/stddev/median/percentile/histogram over `IArray<Number>`,
  covariance over `IArray<Vector3>`. Pure, universally useful, trivially property-testable.
- **Topology exposure**: the half-edge library is a differentiator — surface it as *queries*: boundary
  loops, connected components (exists), vertex/face rings, geodesic distance (Dijkstra now; heat method
  later), shortest edge path, mesh Laplacian as a reusable operator (smoothing §3.2, spectral analysis
  later). Consider a read-only `ITopologyQueries` facade so examples stop re-deriving adjacency.

---

## 9. Randomness & noise (enabler for half the above)

Missing today and blocking scatter/jitter/displacement:
- **Deterministic hash-PRNG**: PCG/xxHash-style `Hash(Integer) → Integer`, `HashFloat(Integer) → Number`,
  `Hash(Integer2/3)`, `RandomVector3(seed, i)`. Pure, seedable, replayable, portable — the *right* PRNG
  story for a deterministic language (no mutable generator state).
- **Low-discrepancy**: Halton, R2/golden-ratio sequences, Fibonacci sphere (§5), jittered grid,
  Poisson-disk (Bridson — needs arrays only).
- **Noise in Plato**: Perlin & Worley exist in C# and are pure — port; add value noise, simplex-ish
  gradient noise, **fbm/turbulence/ridged combinators**, domain warping, curl noise (divergence-free
  vector fields — particle/hair demos). These reach GLSL when that backend lands; noise is the #1 shader
  library content.
- **IQ cosine palettes**: `Palette(t, a, b, c, d) = a + b·cos(2π(c·t + d))` — one line, infinite pretty
  gradients, and starts the color-function family (`IProcedural<Number, Color>` ramps, viridis/turbo
  coefficients as constants).

---

## 10. Voxelization & discretization

- Existing: `Sdf3D.Voxelize`, `MarchingCubes`, `VoxelizedField`, `CellGridBuilder3D`. Additions:
- **Mesh → voxels**: surface voxelization (conservative triangle-box overlap — Akenine-Möller test is a
  nice pure predicate for Plato) and **solid** voxelization (scanline parity / winding number).
- **Mesh → SDF grid**: exact point-triangle distance + sign from angle-weighted pseudonormals
  (Bærentzen–Aanæs) — unlocks *mesh* input to the whole SDF pipeline (§1), i.e. mesh booleans via
  voxelize→combine→march. That's the pragmatic CSG answer without exact arithmetic.
- **Dual contouring / surface nets** — sharp features that marching cubes loses; pairs with hermite data
  from SDF gradients.
- **Adaptive sampling with interval arithmetic** (§11): octree subdivision pruned by SDF range bounds —
  the payoff that justifies interval math.
- Distance transforms on grids (fast sweeping/FMM), voxel morphology (dilate/erode = SDF offset,
  discrete), connected-component labeling of voxel grids (RoomDaylightVoxels-style analyses).

---

## 11. Interval math (do it properly)

Current `NumberInterval` has structural ops (Lerp/Split/Contains) but **no arithmetic**. A real interval
type is very Plato (pure, algebraic laws to property-test — containment monotonicity):
- `Add/Sub/Mul/Div(NumberInterval, NumberInterval)` (with the min/max-of-4 products), `Sqr`, `Sqrt`,
  `Abs`, `Min/Max`, monotone function lifting.
- **Trig over `AnglePair`**: `Sin(AnglePair) → NumberInterval` respecting extrema — enables tight bounds
  of every parametric curve in the library: `EvalInterval(curve, AnglePair) → Bounds2D`. Curve/surface
  AABBs without sampling — feeding the AABB tree correctly.
- **Range analysis of SDFs**: `EvalInterval(sdf, Bounds3D) → NumberInterval` → certified inside/outside/
  straddling classification → adaptive voxelization (§10), certified collision, root isolation.
- Vector intervals = `Bounds2D/3D` (already there — unify the vocabulary: a Bounds3D *is* an interval
  Vector3; the generic `IInterval`/`IBounds` split in core.interfaces could merge around this).
- Stretch: affine arithmetic for tighter bounds; Lipschitz-bound tracking on SDF combinators.

---

## 12. Bounded vs. unbounded shapes (type-system cleanup)

Today `Plane` is infinite, `Ray3D` is semi-infinite, `Line3D` is a *segment* (name collision with the
mathematical line), and nothing distinguishes them from bounded geometry. Proposal:
- `interface IBounded2D/3D { Bounds(x: Self): Bounds2D/3D; }` — implemented by all finite geometry
  (audit-generating the implementations catches today's gaps, e.g. Line2D has no Bounds2D).
- Unbounded primitives as first-class types: `InfiniteLine3D` (point+direction), `HalfSpace` (= oriented
  Plane with inside), `InfiniteCylinder`, `InfiniteCone`, `Slab` (two parallel planes). IQ's 3D list
  needs these anyway (§1.1); intersection/culling code wants them (Frustum is six half-spaces — the C#
  Frustum type would collapse into `IArray<HalfSpace>` + generic containment).
- Clipping then becomes uniform: `Clip(x, HalfSpace)` for meshes/polylines/segments — MeshPlaneClip
  generalizes.
- Consider renaming `Line3D`→`Segment3D` in the fullness of time (or at least alias + document); the
  current name will keep generating bugs at the boundary between segment-math and line-math.

---

## 13. Priorities (if forced to choose)

| Rank | Item | Why |
|---|---|---|
| 1 | SDF 2D+3D catalog + ops (§1) | Highest content-per-line, unlocks §10, showcase-quality, GLSL-ready |
| 2 | Hash/PRNG + noise in Plato (§9) | Unblocks scatter, jitter, displacement — half the fun features |
| 3 | Surface constructors + RMF frames (§2.2–2.3) | The workhorses every example hand-rolls today |
| 4 | Space-warp deformers + falloff fields (§3.1, §0.1) | Converts the whole Deformers example family to Plato |
| 5 | Icosphere + Conway operators (§4.2) | One operator set = forty polyhedra; showcases topology |
| 6 | Arc-length + curve queries (§7) | Prerequisite for even distribution & correct sweeps |
| 7 | Interval arithmetic (§11) | Foundational; pays off through §10 adaptive methods |
| 8 | Mass properties + curvature (§8) | Cheap, high-utility analysis; feeds effectors/colors |
| 9 | Taubin smoothing + Catmull-Clark (§3.2) | The two most-asked-for mesh ops |
| 10 | Bounded/unbounded type cleanup (§12) | Best done before the SDF catalog freezes names |

Everything above assumes the correctness work from `reports/plato-library-review.md` lands first — a bigger
library on an untested base just multiplies the surface for the next Rose/Lissajous.
