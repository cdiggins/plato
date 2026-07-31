# Plato refactoring recommendations: what to do next

**Date:** 2026-07-12  
**Purpose:** consolidate the current Plato status, [`additions.plato`](../submodules/Plato/docs/additions.plato), the [Geometry3Sharp port assessment](geometry3sharp-port-candidates-for-plato.md), and the previous review, roadmap, reassessment, compiler, optimizer, and Earcut findings into one actionable refactoring direction.

## Executive recommendation

Plato should now pivot from broad compiler experimentation to **consumer-driven refactoring**: finish the optimizer work already in flight, repair the concrete language/runtime gaps exposed by Earcut, establish a small portable query vocabulary, and then grow geometry through immutable topology and spatial acceleration.

The recommended order is:

1. **Close the current optimizer mission cleanly.** Update the optimized recipe/golden documentation, run the pending full gate, and leave a known-green checkpoint.
2. **Fix the five compiler/runtime defects exposed by the first substantial functional algorithm port.** These are more valuable now than another speculative optimizer pass because they block real Plato programs.
3. **Refactor the portable foundation:** production builder signatures, `Option`/`Result`, tolerance/finiteness, array reducers/search/iteration, naming and geometric semantics.
4. **Build the computational-geometry kernel:** robust planar predicates, promote Earcut, primitive distance/intersection queries, and structured hit/closest-point results.
5. **Build immutable mesh infrastructure:** derived half-edge/adjacency tables, components, boundary loops, normals and mass properties, then a flat triangle AABB tree.
6. **Use those foundations for visible content:** curve sampling, SDF completion, surface constructors/frames, PRNG/noise, deformers, Marching Cubes, then one topology-changing mesh operation.
7. **Advance double precision alongside the query/topology work.** It is an author-approved near-term goal and matters for BIM coordinates and robust predicates. Let GLSL follow the real SDF content rather than lead it.

The central design choice is **not** to clone Geometry3Sharp's mutable `DMesh3` architecture. Plato should use immutable public values and flat arrays, with affine `List<T>`/`Buffer<T>` workspaces during construction. A dynamic topology runtime should be considered only if QEM reduction or plane cutting proves that this model is insufficient.

## Sources and precedence

This report treats newer code and status documents as authoritative over older proposals:

| Source | What it contributes | How it is used here |
|---|---|---|
| [`PROGRESS.md`](../submodules/Plato/PROGRESS.md) | Current optimizer mission and pending closeout | Defines the immediate first task |
| [`additions.plato`](../submodules/Plato/docs/additions.plato) | Proposed semantic types, queries, half-edge navigation, builders, and mesh operations | Reviewed item-by-item; promoted only where a current consumer justifies it |
| [Geometry3Sharp port assessment](geometry3sharp-port-candidates-for-plato.md) | Ranked geometry/data-structure candidates and dependency sequence | Supplies the query → topology → BVH → mesh-operation ladder |
| [`earcut/FINDINGS.md`](../submodules/Plato/earcut/FINDINGS.md) | Evidence from a real nontrivial Plato algorithm | Elevates concrete compiler/runtime gaps and validates functional geometry |
| [`plato-roadmap.md`](plato-roadmap.md) and [execution plan](plato-execution-plan-2026-07-09.md) | Decisions, completed work, and approved direction | Prevents re-planning completed phases |
| [Codebase assessment](plato-codebase-assessment-2026-07-10.md) and [type-checker handoff](../submodules/Plato/docs/type-checker-handoff.md) | Compiler duplication, checker completeness, and TIR status | Drives compiler simplification recommendations |
| [Library review](plato-library-review.md) and [library ideas](plato-library-roadmap-ideas.md) | Correctness policy, content priorities, naming/discoverability | Supplies content scope and guardrails |
| [Optimizer stage-2 plan](../submodules/Plato/docs/optimizer-stage2-plan.md) and [emitter phases](../submodules/Plato/docs/plato-emitter-phases.md) | Current TIR pass architecture | Defines what to finish, stabilize, or defer |

When these disagree, the order of authority is: current source and gates → current `PROGRESS.md` → dated status/DONE notes → approved execution-plan decisions → older recommendation documents.

## Reconciled current state

Several recommendations in older reports are no longer future work:

| Earlier recommendation | Current status | Consequence for the new plan |
|---|---|---|
| Fix associativity and the 36 known library bugs | **Done**; known-failure manifest empty | Do not schedule another correctness wave; retain laws and regression gates |
| Make `stdlib-legacy` writable and let content lead | **Decided and done** | New production content is authorized when gated |
| Add SDF primitives and CSG | **First 3D slice done** | Extend the real catalog; do not build a throwaway GLSL-only catalog |
| Implement solid surface evaluation | **Done for all 11 solids** | Move to constructors, frames, and discretization rather than repeating primitive `Eval` work |
| Unblock function-valued fields | **Done in `ScalarField3D`** | Use the capability; generalize only when a second concrete field family requires it |
| Add affine builders | **Runtime/types done** | Promote signatures into production and use them in real library algorithms |
| Move all writers to typed IR | **Done for C#, TypeScript, and Rust** | Stop maintaining new semantic behavior in legacy emit paths |
| Reset global symbol counters / remove ordinal churn | **Done** | The old reason for hiding builder signatures in test source is obsolete |
| Optimizer materialization, inlining, method shape, and loop lowering | **Implemented; final recipe/docs/gate closeout pending** | Finish and stabilize before adding more optimizer phases |
| General polygon triangulation | **Earcut proof passes 20/20 outside production** | Fix its exposed toolchain gaps, then promote predicates/types/triangulation deliberately |
| `Fraction` name decision | **Resolved: `Fraction`** | The remaining question is representation/conversion and migration, not naming |
| Double precision | **Approved near-term, not implemented** | Schedule beside geometry robustness, not in an indefinite target backlog |

## Refactoring principles

1. **One semantic compiler pipeline.** AST → typed IR → explicit TIR passes → target writer is the only path that should gain features. The legacy body writer may remain temporarily as a differential oracle, but not as a second implementation target.
2. **Consumer-driven language work.** A compiler or intrinsic change should be justified by a real Plato library/algorithm and carry that consumer as its regression test.
3. **Immutable public values, affine construction.** Results are ordinary Plato types and arrays; `List<T>`/`Buffer<T>` exist only inside construction pipelines and freeze at API boundaries.
4. **No sentinel APIs in new work.** Misses, degeneracy, non-invertibility, and non-convergence use `Option`/`Result` or named result records, not `-1`, null, booleans plus meaningless fields, or exceptions.
5. **Port behavior, not foreign class hierarchies.** Geometry3Sharp and host C# are references/oracles. Plato APIs should be small free functions and immutable records.
6. **A semantic type must pay rent.** Add a type only with its constructors, laws, consumers, and conversion policy. Avoid adding a catalog of wrappers before the algorithms that need them.
7. **Deterministic single-threaded semantics first.** Parallelism and host I/O belong in runtimes/adapters and must not define portable behavior.
8. **Double precision is a correctness target, not merely an optimization variant.** Robust geometry and large-coordinate BIM tests should help define its acceptance criteria.

## Unified recommendation matrix

| Priority | Workstream | Concrete recommendation | Why now | Dependencies / exit criteria |
|---|---|---|---|---|
| **P0** | In-flight optimizer closeout | Complete the pending recipe/golden/docs update and run `check-all.ps1` once | Avoids building new work on an unclosed optimizer state | Full gate green; `PROGRESS.md` records final recipe and no pending closeout |
| **P0** | Earcut-driven compiler fixes | Fix heterogeneous `Reduce` accumulator typing, `--inline` mixed array-return lambdas, scalar-erased `Integer` array leaks, and static-vs-instance receiver mismatch | These are observed failures in a real algorithm and weaken confidence in content-led development | Minimal TIR regression per bug; Earcut restores the full intended recipe; off-flag output unchanged |
| **P0** | Missing runtime intrinsic | Implement the declared `Concatenate` intrinsic or standardize on one `Concat` name | Removes a handwritten Earcut shim and a source/runtime contract hole | Generated library and Earcut compile without `EarcutSupport.cs`; naming decision documented |
| **P0** | Production affine API | Move `List<T>`/`Buffer<T>` observe/mutate/consume signatures from test-only declarations into production source | Types already ship; keeping their usable API test-only prevents real builder-backed library code. Prior ordinal-churn concern is resolved | C#, TS, and Rust generation/gates green; one production algorithm uses and freezes a builder |
| **P0** | Portable partiality | Add `Option<T>` first, then `Result<T,E>` only when an error-bearing consumer exists | Unlocks find/query/inversion APIs and removes repeated `-1` sentinels seen in Earcut and Geometry3Sharp | Laws for `Some`/`None`, map/bind/value-or; no unchecked `.Value` in core query APIs |
| **P0** | Numerical policy | Add `Tolerance`, `IApproximate`, and finiteness predicates; define absolute+relative comparison | Required by robust predicates, queries, fitting, double/float conformance, and near-zero behavior | Shared comparison laws across numeric/vector/point types; explicit default tolerances by scalar target |
| **P0** | Array algorithm substrate | Add `Sum`, `Average`, `FindIndex` returning `Option<Integer>`, `Scan`, `Iterate(n, seed, f)`, `Concatenate`, and a sortable/groupable edge-record path | These are direct needs from Earcut, topology construction, BVH construction, and mass properties | Each primitive has laws; optimized recipe emits loops without delegate/view pathologies |
| **P1** | Checker completeness | Burn down the remaining located checker diagnostics, prioritizing coercion and concept-dispatch cases exercised by new content | The checker is now the semantic authority; permissive holes undermine every backend | Diagnostic count trends to zero; permissive `Self`/syntactic fallbacks tightened only as tests permit |
| **P1** | Legacy compiler deletion | Freeze `--no-tir` after current optimizer stabilization, then delete legacy body heuristics and duplicate call-resolution paths in stages | The highest remaining compiler refactor is removing duplicate authorities | TIR flag differentials converted to snapshots/semantic tests; no production style depends on legacy writer |
| **P1** | Core numeric concepts | Introduce `IInnerProduct`; resolve `Length`/`Magnitude` naming; add `Matrix3x3` when covariance/inertia/normal-matrix work lands | Generalizes projection, closest-point, fitting, and mass-property code | ADR/naming rule; consumers and laws land with the concept/type |
| **P1** | `Fraction` migration | Define the wrapper/conversion policy and thread `Fraction` through interpolation in one reviewed breaking pass | Name is decided but half-adoption would create more overload ambiguity | API snapshot reviewed; all conformance targets pass; extrapolation semantics documented |
| **P1** | Geometry semantics cleanup | Separate finite `Segment2D/3D` from infinite line and ray concepts; document plane sign convention, winding, handedness, axes, and transform order | Existing `Line3D(A,B)` behaves like a segment in proposed queries; freezing ambiguous names will multiply bugs | ADR plus migration plan; query names match domain semantics |
| **P1** | Planar kernel | Promote orientation/turn, on-segment, proper crossing, point-in-triangle, point-in-polygon, signed area, and ring normalization to production | Earcut already proves the need; Geometry3Sharp report ranks robust predicates as foundational | Degenerate and winding tests; preferably double cross-checks; Earcut consumes the shared predicates |
| **P1** | Production triangulation | Promote `PolygonWithHoles` and the readable Earcut pipeline after the planar kernel and toolchain fixes | Converts a successful experiment into high-value portable geometry | Earcut 20/20 plus seeded/property tests under full optimized recipe; performance limits documented |
| **P1** | Primitive query kernel | Add point–segment, point–triangle, segment–segment, ray–plane, ray–triangle, and ray–AABB queries with named results | Highest-value Geometry3Sharp subset and foundation for picking, snapping, BVH, ICP, and cutting | `Option<RayHit3D>` / closest-point records; degeneracy and analytic differential tests |
| **P1** | Immutable mesh topology | Build sorted undirected edge records, twin half-edge array, triangle neighbors, vertex/corner offsets, boundary edges, components, and loops | Gives most of `Topology.cs`/G3 mesh-analysis value without importing `DMesh3` mutation | Manifold, boundary, bow-tie, and non-manifold result policies; flat immutable arrays after construction |
| **P1** | Mesh analysis | Add face/vertex normals, surface area, signed volume, centroid, watertightness, one-ring and cotangent weights | Cheap visible payoff from topology and additions.plato mass-property proposals | Preconditions represented or checked; compare against analytic shapes and Geometry3Sharp |
| **P1** | Flat triangle BVH | Implement an immutable array-backed AABB tree with ray and nearest-point queries | Replaces the most broadly useful G3 bridge dependency and accelerates later algorithms | Primitive query kernel, bounds, sort/partition, affine stack/builders; benchmark and G3 differential suite |
| **P1** | Double target | Implement `--scalar=double` plus double intrinsics and cross-precision conformance | Approved, important for BIM and robust geometry, and now has query/predicate workloads to validate it | Double generated project; seeded float/double differential; large-coordinate geometry tests |
| **P2** | Curve and surface content | Arc-length parameterization, resampling, Douglas–Peucker, rotation-minimizing frames, sweep/revolve/loft | Builds on array/query foundations and serves the content-led goal | Robust curve parameter policies; constructors produce tested meshes |
| **P2** | SDF/field content | Complete 2D/3D catalog, domain operations, normals, dense sampling grids, then Marching Cubes | Existing SDF and function-field work is ready to grow; GLSL can ride on it | Dense `IArray3D`, builder-backed mesh construction, CPU witnesses; deterministic Marching Cubes first |
| **P2** | Procedural variation | Hash PRNG, low-discrepancy sampling, noise, falloff fields, and deformers | Unlocks cloners, scatter, jitter, displacement, and showcase content | Determinism across targets; explicit seed and coordinate conventions |
| **P2** | First topology-changing mesh slice | Choose **one** of QEM reduction or plane cutting after topology/BVH; do not start both | Tests whether affine workspaces are sufficient before designing dynamic topology | Differential suite against Geometry3Sharp; invariant checks; explicit limited input contract |
| **P2** | GLSL | Build the writer/demo on the production SDF subset | Demonstrates multi-target value without diverting content into a PoC branch | CPU/GPU conformance for the same SDF functions |
| **Defer** | Semantic-type expansion | `Complex`, `UnitVector3`, `Direction3D`, `Normal3D`, and engineering measures as a bundle | Individually reasonable, but most are not on the immediate dependency path | Add each only with a concrete consumer, invariants, conversions, and naming policy |
| **Defer** | Mutable geometry clone | Full `DMesh3`, remesher, mesh auto-repair, sparse grids/hash grids, general mesh booleans | Very large mutable dependency cone and poor current Plato fit; G3 itself warns its boolean is not robust | Revisit after a real simplifier/cutter demonstrates immutable topology is inadequate |
| **Defer** | Broad numerical subsystem | General dense/sparse matrices, SVD, Cholesky, eigensolvers as one package | Oversized relative to near-term consumers | Cherry-pick `Matrix3x3`, specialized symmetric eigen, or matrix-free CG when fitting/deformation needs them |

## Recommended execution phases

### Phase 0 — establish a green handoff point

Finish the current optimizer mission exactly as `PROGRESS.md` requests: update the golden recipe/documentation, run the full gate, and record the result. Do not mix library/API refactors into this closeout. The purpose is a clean boundary between optimizer infrastructure and the next content-led phase.

### Phase 1 — make real algorithms first-class citizens

Fix the Earcut-discovered compiler/runtime defects and promote affine builder signatures to production. Add regression tests at the smallest TIR/emitter layer and retain Earcut as the end-to-end consumer. Add `Iterate`, `FindIndex`, `Sum`, `Concatenate`, and the minimum sorting/grouping support required by edge construction.

This phase should end with Earcut compiling under the full intended optimized recipe, without a C# shim, and with at least one builder-backed production Plato algorithm.

### Phase 2 — refactor the portable vocabulary

Add `Option`, tolerance/finiteness, query-result records, and the naming/geometry conventions ADR. Introduce `Fraction` in one deliberate migration. Add `IInnerProduct` only with projection/closest-point consumers. Avoid introducing `Complex`, the full measure hierarchy, and all direction/normal wrappers in the same change.

This phase should end with no new sentinel-returning APIs and a documented policy for precision, degeneracy, lines versus segments, winding, handedness, and transforms.

### Phase 3 — computational geometry and immutable topology

Land the planar predicate library and move Earcut into production. Add the primitive 2D/3D query kernel. Build `HalfEdgeMesh` as a derived immutable index structure: its navigation is pure arithmetic; only twin/adjacency construction uses affine buffers and sorting/grouping. Then add components, boundary loops, normals, weights, and mass properties.

This phase is the key architectural test: it should show that useful topology algorithms do not require a public mutable mesh.

### Phase 4 — spatial acceleration and precision

Build a flat immutable triangle AABB tree and run ray/nearest-point differentials against Geometry3Sharp. In parallel, land the double target and add large-coordinate and near-degenerate query tests. A double implementation should become the reference oracle for float conformance where appropriate.

### Phase 5 — visible geometry content

Use the foundation rather than opening new infrastructure tracks: curve arc-length and resampling; surface constructors and frames; extended SDF/fields; PRNG/noise and deformers; dense grids and deterministic Marching Cubes. Then choose QEM reduction or plane cutting as the first operation-specific topology workspace.

### Phase 6 — simplify and publish

Once the new content is stable, delete legacy emission/resolution machinery, generate API documentation and capability matrices from the compiler, and establish the new generated `Plato.Geometry` consumer. Add GLSL on the production SDF subset. This phase turns the refactoring into a comprehensible product surface rather than another parallel implementation.

## Proposed first ten backlog items

| Order | Item | Deliverable |
|---:|---|---|
| 1 | Finish optimizer recipe/golden/docs and full gate | Clean green checkpoint |
| 2 | Fix heterogeneous `Reduce` typing | TIR test + simplified Earcut `ClipAll` |
| 3 | Fix inline array-return mismatch and scalar-erasure array leaks | Earcut runs with full recipe |
| 4 | Implement/rename `Concatenate`; fix receiver mismatch | Remove `EarcutSupport.cs` and workarounds |
| 5 | Promote production `List<T>`/`Buffer<T>` signatures | First production builder consumer |
| 6 | Add `Option<T>`, `FindIndex`, `Iterate`, `Sum`, `Scan` | Portable algorithm vocabulary |
| 7 | Add `Tolerance`/`IApproximate`/finiteness | Shared numerical policy |
| 8 | Land planar predicates and migrate Earcut to them | Production planar kernel |
| 9 | Promote Earcut/`PolygonWithHoles` | General production triangulation |
| 10 | Add primitive distance/intersection query slice | Structured ray/closest-point results |

After item 10, the next coherent vertical slice is immutable mesh twins/adjacency → components/boundaries/normals/mass properties → flat AABB tree.

## Specific decisions on `additions.plato`

| Proposal | Decision | Refinement |
|---|---|---|
| `Fraction` | **Adopt** | Name is settled; design conversions and migrate once, with extrapolation preserved |
| `Tolerance`, `IApproximate`, `IFinite` | **Adopt early** | Use mixed absolute+relative comparison; distinguish invalid numeric input from geometric degeneracy |
| `Option<T>` | **Adopt early** | Prefer `Option<RayHit3D>` over a `RayHit3D.Hit` flag; add `Result` only with an error-bearing consumer |
| Array reducers | **Adopt early** | Empty `Min`/`Max`/`Average` must return `Option` or require a non-empty type/precondition |
| `IInnerProduct` | **Adopt with consumers** | Do not merely insert another interface layer; land projection/closest-point laws with it |
| `Matrix3x3` | **Adopt when needed** | Best trigger is covariance/plane fit, inertia, or normal-matrix work |
| `Complex` | **Defer** | Scientifically useful but unrelated to the next geometry dependency chain |
| Unit/direction/normal semantic types | **Selective defer** | `Normal3D` may land with hit records/normal transforms; avoid a three-type migration before consumers exist |
| Length/area/volume measures | **Defer** | Valuable BIM showcase after naming/conversion conventions stabilize |
| Universal query concepts | **Adopt cautiously** | First ship concrete query functions/results; extract concepts after two or more types share meaningful laws |
| Point/line/plane/bounds queries | **Adopt early** | Correct the line-versus-segment ambiguity and use `Option` for parallel ray/plane |
| Angle/rotation utilities | **Adopt** | Add explicit radians constructor and robust opposite-vector handling in `RotateTo` |
| Mass properties | **Adopt after query/topology base** | Return semantic `Area`/`Volume` only if the measure types have actually landed |
| `HalfEdgeMesh` | **Adopt as immutable derived topology** | Replace `-1` twin sentinel with boundary-aware query API where practical; retain a compact integer array internally if measured |
| Builder intrinsics | **Promote now** | Their test-only status is obsolete and blocks production use |
| Procedural meshes | **Adopt incrementally** | Triangulate quads first; polygon extrusion after production triangulation; subdivision after vertex-dedup strategy |

## What not to do next

- Do not begin a full `DMesh3` or general mutable half-edge port.
- Do not add a large family of semantic wrapper types without consumers and laws.
- Do not start general mesh booleans, remeshing, auto-repair, or sparse voxel infrastructure.
- Do not let GLSL create a second SDF catalog or dictate the portable API.
- Do not add more optimizer passes while current real algorithms fail existing passes for known reasons.
- Do not tighten the affine checker or remove the legacy differential oracle until the current optimized recipe is closed and consumer regressions exist.
- Do not expose new `-1`, null, exception, or success-flag query conventions.

## Success criteria for the next refactoring cycle

The next cycle is successful when:

1. The full optimized recipe and all existing gates are green from a documented checkpoint.
2. Earcut compiles without C# shims/workarounds under that recipe and remains 20/20.
3. Production Plato exposes builders, `Option`, numerical tolerance, and the array primitives needed by real algorithms.
4. Earcut and primitive geometry queries share production predicates/results instead of private copies and sentinels.
5. An immutable mesh-topology value can derive twins, boundaries, components, normals, area, and volume from `TriangleMesh3D`.
6. A flat BVH answers ray and nearest-point queries consistently with Geometry3Sharp.
7. The double target passes cross-precision and large-coordinate tests.
8. No new feature introduces a parallel compiler authority, mutable public geometry model, or untested semantic type.

## Bottom line

The infrastructure investment has paid off: Plato now has a typed IR, multi-target writers, conformance laws, affine builders, function-valued fields, and serious optimizer machinery. The next refactoring should make those capabilities disappear into a simpler authoring experience rather than continuing to enlarge the machinery itself.

The best proof is a ladder of increasingly capable geometry written clearly in Plato: **Earcut without workarounds → robust queries → immutable mesh topology → BVH → one real mesh operation**. That ladder combines the strongest recommendations from `additions.plato`, the Geometry3Sharp assessment, and the earlier roadmaps while giving every compiler change a real consumer and every new library abstraction a measurable reason to exist.
