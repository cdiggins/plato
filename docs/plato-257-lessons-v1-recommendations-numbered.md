# plato-257 Lessons V1 — numbered library recommendations

Full text of every recommendation from **110** lesson files under `lessons/v1/`.
Each row is one recommendation with a link to the originating lesson. Total: **463**.

Use this list to triage and plan Plato library work.

> **Read the [Reviewer pass](#reviewer-pass--2026-07-28) below before assigning items to agents.**
> It resolves cross-item conflicts, dedupes the list into work packets, drops a handful of
> items, and adds new recommendations 464–476. Item numbers 1–463 are stable — do not renumber.

---

## Reviewer pass — 2026-07-28

Review by Claude (against `stdlib` as of 2026-07-28). The 463 lesson-derived items below
are largely sound, but they were written per-lesson in isolation: many restate the same gap,
several conflict with each other, and a few conflict with the language itself. Parallel agents
that pick items independently will produce three styles of the same API. Resolve sections A and
D first; assign section B packets per-file; skip section C items.

### A. Coordination gates — decide once, before any assignment

**A1. There is no generic `Optional<T>` — and there cannot be one.** Plato sum types shipped
2026-07-27 but generic sums are rejected (CHK306). Every item that asks for `Try*` /
`Optional<...>` returns (205, 331, 397, 442, 443, 456) must land on one of three concrete
styles instead:
  - *fallback parameter* for total-ish value ops: `NormalizeOr(v, fallback)` — as item 252 already suggests;
  - *concrete result record* with a validity field for queries: `PlaneHit3D { Hit: Boolean; Point; Parameter }`;
  - *concrete (non-generic) sum* where the classification IS the payload: item 397's
    `SphereSphereIntersection = Separate | ExternalTouch | OverlapCircle(...) | ...` is the model case.
Pick this trio as the repo rule, write it in the v3 README, and reject any agent output that
invents a fourth style.

**A2. One conventions document, not forty doc-comments.** Roughly a quarter of the
**doc-comment** items restate a *global* convention on one type: row-vector multiplication
(231, 234), right-handed / CCW winding (240, 243, 440), `-1` index sentinel (455, 457),
radians-canonical angles (113), inclusive bounds and the empty-bounds encoding (186, 188),
straight-vs-premultiplied alpha (63), linear-light `Color` (68, 211), view-space handedness
(39, 226). Create `stdlib/CONVENTIONS.md` (or a `00-conventions` banner file) stating
each convention exactly once; the per-type fix then becomes a one-line citation. This collapses
~40 items into one packet plus mechanical cross-references.

**A3. One epsilon policy.** Items 124–127, 300, 320, 345 each invent a tolerance signature.
Decide once: a `{ Absolute; Relative }` comparison record in `numbers.plato` (per item 125),
`AlmostEqual` overloads take it, engineering `Tolerance` (uncertainty.plato) is explicitly NOT it
(126). All predicate items then reuse the one record.

**A4. One angle-periodicity kit.** Items 9, 12, 13, 24, 30, 113 are one coherent addition to
`quantities.plato`: `Normalize`, `Wrap(period)`, `LerpShortest`, `EquivalentAngle(a,b)`,
plus `Degrees(n)` / `Turns(n)` constructors (10, 113). Assign as a single packet; done
piecemeal these will disagree on the canonical interval.

### B. Work packets — dedupe map for parallel assignment

Assign **per-packet (≈ per-file), never per-item** — two agents editing one `.plato` file will
conflict. The heavy-traffic files are `lines`, `transforms`, `vectors`, `splines`,
`intervals-bounds`, `collision`, `noise`. Major duplicate clusters:

| Packet | Items (merge these) |
|---|---|
| Normalize on `Normed` + safe variants + `Direction` factories | 158, 252, 253, 254, 256, 259, 296, 358 |
| Cross / Wedge / Perp / ScalarTriple | 86, 87, 88, 89, 90, 236 |
| `Plane`: SignedDistance, FromPointNormal, Flip, plane-of-triangle | 206, 208, 286, 287, 288, 290, 338, 439 |
| Barycentric maps + docs | 6, 19, 20, 434, 436, 437, 438 (drop 22 — see C) |
| `BoundsLike<TPoint, TDelta>` + Union/Contains/Expand + empty encoding | 29, 30, 64, 66, 185, 186, 188 |
| `RayHit3D` shape + RaycastAny/All | 3, 331, 332, 333, 395 (ruling D1) |
| Catmull-Rom ↔ Hermite + endpoint tangent policy | 48, 49, 52, 102, 110, 181, 182, 184 |
| Grid parameterization unification (29 vs 33 vs 45) | 291, 293, 351, 352, 353, 459, 460, 463 |
| Color conversion signatures + encoding tags | 65, 66, 215, 217, 218 |
| Quaternion double cover: docs + SameRotation + Canonicalize | 319, 320, 321, 322, 323, 325 |
| `MetricSpace` on points/vectors + DistanceSquared | 257, 258, 295 |
| InverseLerp / Remap on `Interpolatable` | 209, 210, 466 |
| Coordinate-chart conversions (polar/homogeneous/geo) | 134, 165, 166, 167, 168, 298, 299 |
| Polynomial `Evaluate` + conversions | 307, 308, 311, 312 |
| Mesh normals: compute, orient, flip | 236, 237, 240, 241, 413, 417 |
| Line/ray/segment: PointAt, Parameter, conversions, closest-pair | 220, 221, 222, 223 |
| Intersection functions on `lines` | 205, 207 (+ Plane packet above) |
| `SignedArea` / `Orient2D` robustness kit | 21, 48, 90, 303, 343, 344, 435 |

Everything not in a cluster can be assigned by file in document order.

### C. Dropped / demoted — do not assign as written

- **22** (two-component barycentric storage): rejected. `(V, W)` with implicit `U` trades a
  documented invariant for permanent asymmetry in every formula and debugger view. Keep three
  fields; add the invariant doc (20) and an `IsNormalized` predicate instead.
- **106** (uniform `Tension` knob on Catmull-Rom): rejected — it manufactures the exact
  Alpha-vs-tension confusion item 49 exists to prevent, and TCB already owns per-point tension.
  Do 49 (doc) + 52 (`ToHermite` bridge); users who want tension go through Hermite or TCB.
- **216** (rename `Color` → `LinearColor`): rejected as churn across the whole corpus for a
  doc-fixable problem. Keep the amplified doc banner half of the item; no rename.
- **242** (per-mesh `WindingOrder` field): rejected. A runtime flag every consumer must consult
  is strictly worse than one global CCW convention (A2) plus an import-time
  `OrientConsistent` repair pass (413). Keep the doc-comment half (243).
- **316** (rename `Pose(t: Transform3D)` to `DiscardScale`): demoted. v3's conversion
  convention is target-type-named constructors; breaking it once creates a second convention.
  Add a "lossy: drops scale" doc line instead.
- **448** (forbidden-name list in file banner): demoted to tooling — this is a linter rule
  (LINT00x), not documentation. File a tracker item against `PlatoCompiler/Analysis/Linter.cs`.
- **461** (split `InterpolationScheme` into 2D/3D sums): demoted to doc-comment. The misuse is
  caught at the use site; two parallel sums add surface for marginal safety.
- **60** (broad-phase types) and **341** (`RigidBodyWorld` container): deferred, not dropped.
  Simulation-container shape is an architecture decision that deserves its own design doc;
  drive-by types from a lessons pass will be wrong. File as a tracker idea; do not assign here.

### D. Shape rulings — where items conflict, this is the decision

- **D1. `RayHit3D` stays a single record.** Items 331/395 want a sum, item 3 wants documented
  sentinels. Ruling: keep the record — raycasts are hot-loop APIs, and per-target sums would
  multiply every query signature. Do item 3 (document which fields are meaningful per target),
  add `RaycastAny` (332). An `AnalyticHit`-vs-`MeshHit` sum may be revisited if profiling ever
  says hits are not hot.
- **D2. `Keyframe<T>` stays a record.** Item 199's `SimpleKey | EasedKey | TangentKey` sum is
  **not expressible** — `Keyframe<T>` is generic and sums cannot be generic (CHK306). Document
  which fields are ignored per `Interpolation` case instead.
- **D3. `JointMotor` velocity sum (193): accepted** — it is a concrete, non-generic sum;
  exactly what the sum-type feature is for.
- **D4. Easing parameter records (101, 405):** accept the direction of 101 — parameterized sum
  cases (`ElasticEased(Phase, ElasticParameters)`) are concrete sums and legal. Pair with 405's
  `SpringState` so the spring story lands in one packet, and note springs are `TimeVarying`,
  not `EasingFunction` (102, 404).

### E. New recommendations (reviewer additions)

464. **doc-comment / conventions** — `points.plato` `UvCoordinate`: no file states the UV
origin convention (top-left vs bottom-left, V direction), yet `mesh-attributes` ("uv"
channel), `images`, and `texturing` all silently assume one. Pin it once in
CONVENTIONS.md (A2) and cite from all three files. This is the same class of bug as winding,
and currently invisible because no lesson happened to trip on it.

465. **missing-function** — `random.plato`: no geometric samplers at all — `PointInDisk`,
`PointOnSphere`, `PointInTriangle` (via barycentric), `PointInBounds3D`, `DirectionOnHemisphere(normal)`.
Items 129/327 ask for scalar distribution sampling but every Monte-Carlo, scattering, and
ambient-occlusion workload needs the geometric versions, and each has a well-known
correct-vs-naive pitfall (e.g. rejection vs sqrt for the disk) that belongs in a doc comment.
Pure `(value, RandomState)` return shape per the file's own convention.

466. **missing-function** — `intervals-bounds.plato`: `NumberInterval` should be the remap
primitive: `At(interval, t)` (lerp), `ParameterOf(interval, x)` (inverse lerp), `Clamp(interval, x)`.
Then item 210's `Remap` is `At(to, ParameterOf(from, x))` and the interval type earns its keep
in every shader-style calculation. Assign together with the 209/210 packet.

467. **process** — every packet in section B that adds functions must ship matching `Law_*` /
`Witness_*` entries in `stdlib-legacy-tests` and a green `regen-conformance.ps1 -Test` in the same
change. State this in the assignment template; an API addition without a law is not done.
(Prevents the v3 declarations-only gap from simply moving one layer down.)

468. **process** — when an item is resolved, the closing agent must add a one-line resolution
note to the originating lesson file under `lessons/v1/` (the doc's link column gives the file).
Otherwise the lessons corpus keeps teaching gaps that no longer exist and the next audit
re-derives this list.

469. **missing-function** — `collections.concepts.plato` / `topology.plato`: item 455
asks for `IsNone` per index type; generalize once instead — put `IsNone` / `IsValid` on the
`Index` concept (memory: the typed-index sweep landed 19 index types on one concept) so all of
them inherit the sentinel predicates. One edit, nineteen types, and item 457's doc ask lands on
the concept too.

470. **missing-function** — `quantities.plato`: the quantity types implement `Quantity` but
there are no `Min` / `Max` / `Clamp` / `Abs` helpers on the concept despite `Compare` existing.
Engineering and animation code clamps lengths and angles constantly; without concept-level
helpers each generated library re-derives them from `Compare` or leaks to `Number`.

471. **wrong-shape** — `optimization.plato` comment says "Optional limits use -1 for 'no
limit'" — a `Number`-typed sentinel, unlike the index `-1` convention which at least rides a
typed wrapper. When touching this file (141–144, 269–272), replace magic `-1` limits with
explicit fields (`MaxIterations: Integer` + documented `0 = unlimited`, or a dedicated
`IterationBudget` record) rather than propagating the sentinel into new signatures.

472. **missing-function** — `transforms.plato`: item 82 asks for `ToWorld`/`ToLocal` on
`Frame3D`; add the same pair on `Pose3D` in the same packet — pose is the type scene graphs
actually store (scene3d.plato), and teaching "a pose IS a frame change" needs the verbs on both
or readers conclude the types are unrelated.

473. **doc-comment** — `intrinsics.plato`: the intrinsics hub is the de-facto API surface
for `Quaternion`/`Matrix4x4`/`Vector3D`, but nothing in vectors.plato, matrices.plato, rotations.plato tells a reader it exists
(item 326 notes this for rotations only). Add a one-line "operations for this type live in
intrinsics.plato" banner to every type whose operations are hub-hosted. Cheap, corpus-wide,
and it removes the single most common "missing function" false alarm — several items in this
very list are near-misses of that kind (e.g. 86: `Cross` exists, it is just invisible from
`vectors.plato`).

474. **missing-function** — `fields.plato` / `implicit-sdf.plato`: item 380 notes shapes
don't bridge to SDFs. The minimal high-value version: declare `Sdf(sphere): /* impl SignedDistanceField3D */`
for exactly `Sphere`, `Box3D`, `Capsule3D`, `HalfSpace` — the four with trivial exact
formulas — and stop there. A full geometry→SDF bridge is a project; these four are one packet
and unlock the CSG lessons (364–367).

475. **process** — priority ordering for the whole list: (1) section A gates, (2) packets whose
items are **missing-function on foundation files** (02, 08, 11, 12, 16 — everything else keeps
re-deriving these), (3) doc-comment sweep via CONVENTIONS.md citations, (4) domain-file packets
(collision, signals, geo, engineering) in any order, fully parallel. The **wrong-shape** items
should be executed first *within* their file's packet — shape changes invalidate function work
done before them.

476. **process** — cap the first wave at the ~20 packets in section B rather than fanning out
all 463 items. The long tail (single-file doc comments, pedagogy notes) is cheap cleanup an
agent can batch per-file *after* the shape rulings and conventions doc exist; assigning it
early just creates rebase noise against the packet work.

---

1. [aabb-ray-intersection](../lessons/v1/aabb-ray-intersection.md) — **missing-function** — `spatial-queries.plato` / `intervals-bounds.plato`: no `Raycast(bounds: Bounds3D, query: RayQuery3D): RayHit3D` and `Bounds3D` does not declare `RayIntersectable3D`. The slab story is documented on `Slab3D` but not connected to `Bounds3D` by a function.

2. [aabb-ray-intersection](../lessons/v1/aabb-ray-intersection.md) — **missing-function** — `lines.plato`: no helper to build the three axis `Slab3D` values from a `Bounds3D`. Teaching the equivalence currently requires hand-built normals and intervals.

3. [aabb-ray-intersection](../lessons/v1/aabb-ray-intersection.md) — **doc-comment** — `spatial-queries.plato`: `RayHit3D` should say which fields are significant for non-mesh targets (AABB, sphere, plane) so callers know `Face` / `Barycentric` / `Uv` may be sentinels.

4. [aabb-ray-intersection](../lessons/v1/aabb-ray-intersection.md) — **missing-function** — `intervals-bounds.plato`: an interval overlap primitive `Overlap(a: NumberInterval, b: NumberInterval): NumberInterval` (or boolean) would make the three-slab reduction a direct composition instead of ad-hoc min/max chains in every ray-box implementation.

5. [affine-combinations](../lessons/v1/affine-combinations.md) — **missing-function** — `points.plato` / `transforms.plato`: there is no `AffineCombine(points, weights)` (or `WeightedSum` restricted to sum-weights-one) on `Point2D`/`Point3D`. The lesson must assemble the operation from `Between` + `Multiply` + `Add`; a named helper would make the invariant (weights sum to 1) checkable and teachable.

6. [affine-combinations](../lessons/v1/affine-combinations.md) — **missing-function** — `points.plato`: `BarycentricCoordinate` stores weights but v3 declares no `Evaluate(bary, p0, p1, p2): Point3D` (or 2D) that applies them. Triangle shading and ray-hit reconstruction both need this one-liner.

7. [affine-combinations](../lessons/v1/affine-combinations.md) — **doc-comment** — `algebra.concepts.plato`: `Difference.Between` should state the minuend/subtrahend convention in the concept comment itself (`Between(a,b) = b - a`), matching the `Transforms` library body, so affine rewrite examples do not depend on reading implementation files.

8. [affine-combinations](../lessons/v1/affine-combinations.md) — **pedagogy** — `points.plato`: a short note on `BarycentricCoordinate` distinguishing affine ($u+v+w=1$) from convex ($u,v,w \ge 0$) would prevent the most common misuse when teaching combinations.

9. [angles-as-types](../lessons/v1/angles-as-types.md) — **missing-function** — `quantities.plato` / `intrinsics.plato`: `Angle` has `Add`, `Subtract`, and `Compare`, but no declared `Wrap(self, period: Angle): Angle` or `Normalize(self): Angle` to reduce to a canonical period (e.g. $(-\pi, \pi]$). Wrapping and compass comparisons need this on day one.

10. [angles-as-types](../lessons/v1/angles-as-types.md) — **missing-function** — `quantities.plato`: no `FromDegrees(n: Number): Angle` or `FromTurns(n: Number): Angle` constructors. Without them, the type prevents conflating units at call sites but pushes every caller to hand-roll conversion constants — the exact bug farm the type is meant to eliminate.

11. [angles-as-types](../lessons/v1/angles-as-types.md) — **missing-function** — `intervals-bounds.plato`: `AngleInterval` declares only `Start`/`End` fields via `IntervalLike`. There is no `Contains(interval, angle: Angle): Boolean`, `Span(interval): Angle`, or `Union`/`Intersection` for overlapping angular ranges. `CircularSector` and `CircularArc2D` need these operations; teaching intervals without them stops at data shape.

12. [angles-as-types](../lessons/v1/angles-as-types.md) — **missing-function** — `quantities.plato`: `Angle` implements `Interpolatable` through `Quantity`, but no `LerpShortest(a, b, t: Number): Angle` (or doc comment on `Lerp` stating it uses the long path). Heading interpolation is a daily operation; the default lerp semantics are actively wrong for many inputs.

13. [angles-as-types](../lessons/v1/angles-as-types.md) — **missing-concept** — no periodic equality (e.g. `EquivalentModPeriod(a, b, period)`). `Compare` on raw radians cannot express "same heading" near the $0/2\pi$ seam.

14. [angles-as-types](../lessons/v1/angles-as-types.md) — **doc-comment** — `intervals-bounds.plato`: `AngleInterval` should document inclusive endpoints and wrap-crossing behavior for arc consumers (`CircularArc2D`, `CircularSector`).

15. [axis-angle](../lessons/v1/axis-angle.md) — **missing-function** — `rotations.plato` / transforms library: no `Transform(v: Vector3D, aa: AxisAngle): Vector3D` overload. Callers must convert to `Quaternion` or `Matrix4x4` first. Rodrigues is the teaching formula; having it as a direct apply would match how the representation is motivated.

16. [axis-angle](../lessons/v1/axis-angle.md) — **missing-function** — `rotations.plato`: no canonicalization helper such as `Canonical(aa: AxisAngle): AxisAngle` that forces $\theta \in [0,\pi]$ and a hemisphere choice for the axis. Equality and debugging need this; the identity-axis convention already shows the gap.

17. [axis-angle](../lessons/v1/axis-angle.md) — **doc-comment** — `intrinsics.plato`: `CreateFromAxisAngle(_: Quaternion, axis: Vector3D, angle: Angle)` should state whether `axis` is assumed unit or normalized internally. `AxisAngle` uses `Direction3D`; the intrinsic's `Vector3D` is the inconsistency this lesson keeps tripping on.

18. [axis-angle](../lessons/v1/axis-angle.md) — **pedagogy** — `rotations.plato`: `AxisAngle` does not implement `Interpolatable`. That is correct (naive lerp is harmful), but a doc comment pointing to `Quaternion.Slerp` as the supported blend path would stop readers from inventing `Lerp` on axes and angles.

19. [barycentric-coordinates](../lessons/v1/barycentric-coordinates.md) — **missing-function** — `points.plato` / `planar-shapes.plato`: no `BarycentricCoordinate(tri: Triangle2D, p: Point2D)` or `Point2D(tri: Triangle2D, b: BarycentricCoordinate)`. The coordinate type and triangle type are stranded without the maps this lesson is about.

20. [barycentric-coordinates](../lessons/v1/barycentric-coordinates.md) — **doc-comment** — `points.plato`: `BarycentricCoordinate` should state the binding `U→A`, `V→B`, `W→C` for `Triangle2D` (and the $U+V+W=1$ invariant on the support plane). Without that, `U/V/W` are meaningless labels.

21. [barycentric-coordinates](../lessons/v1/barycentric-coordinates.md) — **missing-function** — `planar-shapes.plato`: no `SignedArea(self: Triangle2D): Number` on the easy surface (area may live behind `PlanarMeasurable`, but barycentric teaching wants the signed scalar used in the ratio formulas called out by name).

22. [barycentric-coordinates](../lessons/v1/barycentric-coordinates.md) — **wrong-shape** — `points.plato`: storing three floats with a sum-to-one invariant invites drift. A two-component form `(V, W)` with `U = 1 - V - W` (plus an optional recovered triple view) would make the invariant unrepresentable-as-false — matching Plato's "illegal states unrepresentable" taste.

23. [bezier-curves](../lessons/v1/bezier-curves.md) — **missing-concept** — `curves-2d.plato` / `curves-3d.plato`: `QuadraticBezier2D` and `CubicBezier2D` (and 3D twins) implement only `Curve2D` / `Curve3D`, not `DifferentiableCurve2D` / `DifferentiableCurve3D`. Béziers have closed-form tangents; claiming the differentiable concepts would unlock `TangentAt` without host-side special cases.

24. [bezier-curves](../lessons/v1/bezier-curves.md) — **missing-function** — Bézier types: no declared `Subdivide(t)`, `Split`, `Derivative`, or `BoundingBounds` helpers. De Casteljau subdivision and AABB-from-controls are the two operations every renderer needs; teaching them immediately surfaces the gap on the type surface.

25. [bezier-curves](../lessons/v1/bezier-curves.md) — **missing-function** — no `ArcLengthParameterized` implementation story for cubics (numeric only). A documented `ApproximateArcLength` or sampled LUT type keyed off `CubicBezier2D` would make the parameter-vs-length pitfall actionable in API form.

26. [bezier-curves](../lessons/v1/bezier-curves.md) — **doc-comment** — `CubicBezier2D`: state explicitly that $t$ is the Bernstein parameter on $[0,1]$, not arc length, and that the curve lies in the convex hull of $\{P_0..P_3\}$. Those two sentences prevent the most common misuse more effectively than the current "workhorse" gloss alone.

27. [bezier-curves](../lessons/v1/bezier-curves.md) — **pedagogy** — linear Bézier (two points) is absent as a named type; `LineSegment2D` covers the geometry, but a `LinearBezier2D` alias or doc cross-link from the quadratic comment would complete the de Casteljau ladder readers expect when learning the family.

28. [bounding-sphere-fitting](../lessons/v1/bounding-sphere-fitting.md) — **missing-function** — `intervals-bounds.plato` / `spatial-primitives.plato`: no `BoundingSphere(bounds: Bounds3D): Sphere` or `BoundingSphere(points: Array<Point3D>)`. The AABB-diagonal construction is universal and three lines; it belongs next to `Bounds3D`.

29. [bounding-sphere-fitting](../lessons/v1/bounding-sphere-fitting.md) — **missing-function** — `intervals-bounds.plato`: `BoundsLike` still lacks `Diagonal` / `Extent` / `Union` / `Expand` (called out as a TODO in `concept-library/intervals-transforms.library.plato`). Fitting lessons keep re-deriving `Between(Min, Max)`.

30. [bounding-sphere-fitting](../lessons/v1/bounding-sphere-fitting.md) — **wrong-shape** — `BoundsLike<TPoint>` carries no `TDelta` parameter, so the natural return type of `Extent` (`Vector3D` for `Bounds3D`) cannot be expressed. Reintroduce `BoundsLike<TPoint, TDelta>` as the library TODO suggests.

31. [bounding-sphere-fitting](../lessons/v1/bounding-sphere-fitting.md) — **missing-function** — `spatial-primitives.plato`: solids that implement `Bounded3D` have no reverse `BoundingSphere` sugar (`Box3D`, `Capsule3D`, `Triangle3D`). Each has a known closed form worth declaring beside `Volume` / `Centroid`.

32. [bsplines-and-nurbs](../lessons/v1/bsplines-and-nurbs.md) — **missing-function** — `splines.plato`: no declared `IsValid(BSplineCurve3D)` / knot-count check, no `ClampedKnots(n, degree)`, and no `InsertKnot` / `ElevateDegree`. Teaching and authoring both need knot construction helpers; the types alone leave the algebra off-stage.

33. [bsplines-and-nurbs](../lessons/v1/bsplines-and-nurbs.md) — **missing-function** — `splines.plato`: no conversion `ToNurbs(BezierCurve3D)` or `ToBezierSpans(BSplineCurve3D)`. The pedagogical "Bézier is a special B-spline" story wants explicit bridges.

34. [bsplines-and-nurbs](../lessons/v1/bsplines-and-nurbs.md) — **doc-comment** — `surfaces.plato`: `NurbsSurface` says it represents quadrics exactly, but does not warn that weights and net must be specialized. A pointer to the standard unit-circle / sphere constructions would prevent false confidence.

35. [bsplines-and-nurbs](../lessons/v1/bsplines-and-nurbs.md) — **wrong-shape** — `splines.plato`: `Degree` is a free `Integer` on the curve while knot/control invariants are comment-only. A factory type or constrained constructor concept would make illegal $(n,d,knots)$ triples harder to represent.

36. [cameras-and-projection](../lessons/v1/cameras-and-projection.md) — **missing-function** — `cameras.plato`: no declared `ViewMatrix(camera)`, `ProjectionMatrix(camera)`, `Project(camera, point) → …`, or `WorldRay(camera, uv)`. The lesson’s pipeline is universal; without these operations the types are inert records.

37. [cameras-and-projection](../lessons/v1/cameras-and-projection.md) — **wrong-shape** — `cameras.plato`: `LookAtCamera` and `OffAxisCamera` do not implement `Camera` despite carrying near/far and producing a view. Either implement the concept (Pose derived from look-at / eye+screen) or document them as *builders* with a `ToPerspectiveCamera` / `ToCamera` conversion in the type comment.

38. [cameras-and-projection](../lessons/v1/cameras-and-projection.md) — **missing-function** — `cameras.plato`: `PhysicalCamera` docs say FOV follows from focal length and sensor size, but no `VerticalFov(PhysicalCamera): Angle` is declared. Teachers and renderers both need that bridge to compare with `PerspectiveCamera`.

39. [cameras-and-projection](../lessons/v1/cameras-and-projection.md) — **doc-comment** — `cameras.plato`: state the assumed view-space convention (handedness, which axis is forward, whether Y is up in view space) on the `Camera` concept. Projection matrices are meaningless without it, and v3 currently leaves the convention implicit in "the pose's forward axis."

40. [capsule-collision-primitive](../lessons/v1/capsule-collision-primitive.md) — **missing-function** — `spatial-primitives.plato`: no `Distance(Capsule3D, Point3D)` / signed-distance helper despite `ContainsPoint3D` and `NearestPoint3D`. Capsule pedagogy and collision debug draws want the scalar $d - r$ explicitly.

41. [capsule-collision-primitive](../lessons/v1/capsule-collision-primitive.md) — **missing-function** — `spatial-primitives.plato`: no `Capsule3D` factory from `Sphere` + segment length, or `FromCylinder` with hemispherical caps — conversion from artist cylinders is a frequent pipeline need.

42. [capsule-collision-primitive](../lessons/v1/capsule-collision-primitive.md) — **wrong-shape** — `collision.plato`: `CapsuleCollider` stores full `Capsule3D` (already two points) plus `LocalPose`. Document whether `A`/`B` are in shape space before pose (assumed) and consider offering axis+height+radius in body space to match how engines author limbs.

43. [capsule-collision-primitive](../lessons/v1/capsule-collision-primitive.md) — **doc-comment** — `planar-shapes.plato` / `spatial-primitives.plato`: state the $A=B$ sphere/disk degeneracy and $r=0$ segment degeneracy as supported invariants so collision code can branch without guessing.

44. [cardioid-parametric-curve](../lessons/v1/cardioid-parametric-curve.md) — **missing-concept** — `curves-2d.plato`: `Cardioid2D` implements `ClosedCurve2D` but not `PolarCurve2D`, despite a polar defining equation and a polar sibling section. Adding `PolarCurve2D` (or documenting why it is omitted) would let callers use `RadiusAt` uniformly with `RoseCurve2D` and spirals.

45. [cardioid-parametric-curve](../lessons/v1/cardioid-parametric-curve.md) — **missing-function** — no declared `Eval` body yet (v3 is declarations-only), and no helper `PointAtAngle(cardioid, angle)` on the type. A named angle-based sampler would match how textbooks write the cardioid and avoid every consumer re-deriving $\theta = 2\pi t$.

46. [cardioid-parametric-curve](../lessons/v1/cardioid-parametric-curve.md) — **doc-comment** — the type comment states total width $4 \times \mathrm{Radius}$ but does not mention the cusp at the origin or the area $6\pi a^2$. One extra sentence on the cusp would prevent "empty hole at center" confusion when plotting samples.

47. [cardioid-parametric-curve](../lessons/v1/cardioid-parametric-curve.md) — **pedagogy** — `Limacon2D`'s comment notes that equality of Offset and Amplitude yields the cardioid, but `Cardioid2D` does not cross-link the limaçon degeneration. A brief "prefer this type when Offset would equal Amplitude" note on `Limacon2D` would steer authors toward the one-field form.

48. [catmull-rom-tension](../lessons/v1/catmull-rom-tension.md) — **missing-function** / **doc-comment** — `splines.plato`: `CatmullRomCurve2D/3D` should document endpoint tangent conventions for open curves. Without that, two implementations can both “be Catmull-Rom” and disagree near the ends — fatal for golden-master tests.

49. [catmull-rom-tension](../lessons/v1/catmull-rom-tension.md) — **pedagogy** — `splines.plato`: the `Alpha` doc comment is excellent; add one explicit sentence: “Alpha is not tension; use `TcbSpline*` for per-point tension.” The lesson’s entire confusion class is people treating them as synonyms.

50. [catmull-rom-tension](../lessons/v1/catmull-rom-tension.md) — **missing-type** / **missing-function** — `splines.plato`: a single optional `Tension: Number` on Catmull-Rom (uniform $\tau$ applied to all automatic tangents) would match what many graphics APIs expose as “CatmullRom(tension).” Today authors misuse `Alpha` or switch types.

51. [catmull-rom-tension](../lessons/v1/catmull-rom-tension.md) — **wrong-shape** — `splines.plato`: `TcbSpline*` stores three parallel `Array<Number>` channels. A `TcbParameters { Tension; Continuity; Bias }` per point (or `Array<TcbParameters>`) would make the equal-length invariant local and teachable as one record per knot.

52. [catmull-rom-tension](../lessons/v1/catmull-rom-tension.md) — **missing-function** — `splines.plato`: no `ToHermite(cr: CatmullRomCurve3D): HermiteSpline3D` conversion. Teaching “CR is Hermite with derived tangents” wants that bridge as a named operation.

53. [circles-ellipses](../lessons/v1/circles-ellipses.md) — **missing-concept** — `planar-shapes.plato`: `Ellipse` implements `ConvexShape` and `ContainsPoint2D` but not `SupportMappable2D` or `NearestPoint2D`, while `Circle` has both. Collision and closest-point lessons want feature parity on the ellipse.

54. [circles-ellipses](../lessons/v1/circles-ellipses.md) — **missing-function** — `planar-shapes.plato`: no `PointAt(circle|ellipse, Angle)` or `Tangent` evaluators. Parameterization is the first thing teachers write on the board; it should be a named function once libraries land.

55. [circles-ellipses](../lessons/v1/circles-ellipses.md) — **doc-comment** — `planar-shapes.plato`: `PlanarMeasurable.Perimeter` on `Ellipse` should note that the value is non-elementary (elliptic integral) so implementers do not ship a silent wrong closed form.

56. [circles-ellipses](../lessons/v1/circles-ellipses.md) — **naming** — `planar-shapes.plato`: type `Circle` denotes a disk region. A brief alias note (“filled disk; boundary is the circle proper” is already there — good) could additionally warn exporters that map hollow `Circle` curves from other APIs.

57. [collision-basics](../lessons/v1/collision-basics.md) — **missing-type** — `collision.plato`: there is no `Collider3D` sum type unifying `SphereCollider | BoxCollider | CapsuleCollider | MeshCollider`. Compounds store parallel arrays per kind; teaching "attach one collider" has no single-variant noun.

58. [collision-basics](../lessons/v1/collision-basics.md) — **missing-function** — `collision.plato`: contact and query types are pure data, with no declared narrow-phase operations (`Intersect`, `ClosestPoints`, `BuildManifold`). The lesson can describe SAT and capsules but cannot point at a vocabulary home for them.

59. [collision-basics](../lessons/v1/collision-basics.md) — **doc-comment** — `collision.plato`: `ContactPoint3D.FrictionImpulse` is a `Vector3D` while `ContactPoint2D` splits `NormalImpulse` / `TangentImpulse` as scalar `Impulse` quantities. A note that 3D friction may be a single tangent-plane vector (or two basis impulses) would clarify the dimensional asymmetry.

60. [collision-basics](../lessons/v1/collision-basics.md) — **missing-type** — `collision.plato`: broad-phase structures (AABB pairs, sweep-and-prune proxy, spatial hash cell) are absent. Filters assume a broad phase exists; pedagogy has to leave that stage unnamed in v3.

61. [color-alpha-compositing](../lessons/v1/color-alpha-compositing.md) — **missing-function** — `image-processing.plato`: `BlendMode` and `PorterDuff` are declared as data, but no `Composite(src: Color, dst: Color, mode: BlendMode, op: PorterDuff): Color` (or Bitmap-level) function exists. The lesson can name the operators but cannot show a real call.

62. [color-alpha-compositing](../lessons/v1/color-alpha-compositing.md) — **missing-type** — `color.plato`: no `PremultipliedColor` (or a tag on `Color`) distinguishing associated vs straight alpha. Without it, compositing APIs cannot make the dangerous conversion a typed boundary.

63. [color-alpha-compositing](../lessons/v1/color-alpha-compositing.md) — **doc-comment** — `color.plato`: `Color.A` should state whether the canonical `Color` is straight or premultiplied. Computation types need a single documented convention; silence guarantees mismatched callers.

64. [color-alpha-compositing](../lessons/v1/color-alpha-compositing.md) — **missing-function** — `color.plato`: `Lerp` on `Color` should be documented as component-wise numerical interpolation, explicitly *not* alpha compositing, to prevent the most common misuse when teaching layers.

65. [color-spaces](../lessons/v1/color-spaces.md) — **missing-function** — `color.plato` / `color-spaces.plato`: conversions are explicitly deferred, but teaching needs at least declared signatures such as `ToLinear(ColorSRGB): Color`, `ToSRGB(Color): ColorSRGB`, `ToOkLab(Color): ColorOkLab`, and `ToHSV(Color): ColorHSV`. Types without maps strand every consumer.

66. [color-spaces](../lessons/v1/color-spaces.md) — **missing-function** — `color-spaces.plato`: `ColorDifference` names formulas but there is no `DeltaE(a, b, formula)` declaration. Palette tooling and tests need it.

67. [color-spaces](../lessons/v1/color-spaces.md) — **wrong-shape** — hue types split across files: `ColorHSV`/`ColorHSL` in `color.plato`, `ColorLCh`/`ColorOkLCh`/`ColorHWB` in `color-spaces.plato`. Either document 14 as "UI companions to Color" or colocate all cylindrical models so discoverability matches the conceptual family.

68. [color-spaces](../lessons/v1/color-spaces.md) — **doc-comment** — `color.plato`: `Color` should state up front "do not construct from sRGB hex/bytes without decoding" and point at `ColorSRGB` / `Color8`. The linear invariant is necessary but not sufficient pedagogy for the #1 misuse.

69. [complex-numbers-rotate](../lessons/v1/complex-numbers-rotate.md) — **missing-function** — `numbers.plato`: `Complex` implements `Numerical` but not `Multiplicative`, so there is no declared `Multiply(Complex, Complex)`. Teaching complex-as-rotation needs that product (or an explicit `Rotate(Vector2D, Complex)`). Without it, authors must leave `Complex` and switch to `Rotor2D` mid-explanation.

70. [complex-numbers-rotate](../lessons/v1/complex-numbers-rotate.md) — **missing-function** — `numbers.plato` / `rotations.plato`: no conversions `Rotor2D(c: Complex)` / `Complex(r: Rotor2D)` even though the `Rotor2D` doc comment says the types are equivalent. A total conversion on the unit circle (and a documented precondition for non-unit `Complex`) would close the teaching bridge.

71. [complex-numbers-rotate](../lessons/v1/complex-numbers-rotate.md) — **missing-concept** — `numbers.plato`: `Complex` does not implement `Normed` or expose `Argument: Angle`. Magnitude and argument are the two polar coordinates of a complex number; without them, the "magnitudes multiply, angles add" slogan cannot be typed against `Complex`.

72. [complex-numbers-rotate](../lessons/v1/complex-numbers-rotate.md) — **pedagogy** — `rotations.plato`: `Rotor2D` fields are `Scalar` and `XY`, while `Complex` uses `Real` and `Imaginary`. Parallel field names (or a doc comment table mapping Real↔Scalar, Imaginary↔XY) would make the isomorphism obvious without a prose lecture.

73. [contact-manifold-basics](../lessons/v1/contact-manifold-basics.md) — **doc-comment** — `collision.plato`: `ContactPoint3D.Normal` duplicates `ContactManifold3D.Normal`. State whether points must match the manifold normal, or whether per-point normals are allowed to differ for curved contacts (and then why the manifold still stores one).

74. [contact-manifold-basics](../lessons/v1/contact-manifold-basics.md) — **wrong-shape** — `collision.plato`: `FrictionImpulse: Vector3D` is unit-bearing in spirit (newton-seconds) but not an `Impulse` quantity, while `NormalImpulse` is `Impulse`. Prefer `FrictionImpulse: Vector3D` documented as newton-seconds **or** a dedicated tangent-impulse type so units are consistent.

75. [contact-manifold-basics](../lessons/v1/contact-manifold-basics.md) — **missing-function** — `collision.plato`: no `Flip(manifold)` that swaps `BodyA` / `BodyB` and negates normals — the operation every broad-phase does when it canonicalizes pair order.

76. [contact-manifold-basics](../lessons/v1/contact-manifold-basics.md) — **missing-type** — `collision.plato`: no explicit feature-id / contact-hash on `ContactPoint3D` for warm-start matching across frames. Engines invent parallel arrays; a `ContactId` field would make the manifold self-contained for persistence.

77. [convexity](../lessons/v1/convexity.md) — **missing-function** — `polygons.plato`: no `IsConvex(Polygon2D) → Boolean` and no `ConvexHull(PointSet) → ConvexPolygon2D`. Teaching “promote to convex” needs a path from `Polygon2D` to `ConvexPolygon2D` beyond manual authoring.

78. [convexity](../lessons/v1/convexity.md) — **missing-function** — `geometry.concepts.plato` / geometry library: `ConvexShape` is a pure marker with no members. A documented `SegmentInside` predicate is redundant with the definition, but an `IsConvex` free function on polygons is still required (markers cannot be queried at runtime without reflection).

79. [convexity](../lessons/v1/convexity.md) — **doc-comment** — `collision.plato`: `MeshCollider.Convex` should warn that a true flag means the **convex hull** is the solid, not the concave surface — the most common authoring misunderstanding in physics engines.

80. [convexity](../lessons/v1/convexity.md) — **missing-type** — `collision.plato`: 2D compounds omit a mesh/polygon collider analogous to `MeshCollider` (only circle/box/capsule arrays). Concave 2D levels often need polygon colliders; the gap pushes users to fake geometry with capsule soup.

81. [convexity](../lessons/v1/convexity.md) — **pedagogy** — `planar-shapes.plato`: `Quad2D` is not `ConvexShape` while `Parallelogram2D` is — correct, but a one-line comment on `Quad2D` saying “use `ConvexPolygon2D` or `OrientedBox2D` when convexity is required” would steer authors.

82. [coordinate-frames](../lessons/v1/coordinate-frames.md) — **missing-function** — `transforms.plato`: no `ToWorld(local: Point3D, frame: Frame3D): Point3D` / `ToLocal(world: Point3D, frame: Frame3D): Point3D` sugar. Everything goes through `Pose3D` or `Matrix4x4`, which is correct but heavier than the teaching vocabulary "express this point in that frame."

83. [coordinate-frames](../lessons/v1/coordinate-frames.md) — **missing-function** — `transforms.plato`: `Basis3D` has a constructor from `Quaternion` but no `Matrix3x3(basis)`, `Orthonormalize`, or `Frame3D(origin, basis)` that checks/consumes a basis. The split is clear; the bridges are thin.

84. [coordinate-frames](../lessons/v1/coordinate-frames.md) — **missing-function** — no `Compose(parent: Frame3D, childLocal: Frame3D)` or equivalent parent-child helper, even though pose composition exists. Scene graphs are frames of frames; the API stops at poses.

85. [coordinate-frames](../lessons/v1/coordinate-frames.md) — **doc-comment** — `Frame3D`: state right-handed orthonormal invariant and "matrix maps local coordinates to the parent/world space of the axes" on the type itself so `Matrix4x4(f)`'s meaning is unambiguous.

86. [cross-product](../lessons/v1/cross-product.md) — **missing-function** — `vectors.plato`: `Cross` lives on the intrinsic hub for `Vector3D` but is not mentioned on the `Vector` concept or in the `Vector3D` type doc. Surface `Cross(Self, Self): Self` in a 3D-only concept (e.g. `CrossProduct3D`) or at least document it on the type so readers of `vectors.plato` alone discover it.

87. [cross-product](../lessons/v1/cross-product.md) — **missing-function** — no `Wedge(Vector3D, Vector3D): Bivector3D` dual to `Cross`. The lesson's GA nod has nowhere to land; `rotations.plato` declares `Bivector3D` without constructors from vector pairs.

88. [cross-product](../lessons/v1/cross-product.md) — **missing-function** — no `ScalarTriple(a,b,c): Number` or `AreParallel(a,b, eps)` helpers. Volume and degeneracy checks are the first two call sites after "what is Cross," and both are currently handwritten.

89. [cross-product](../lessons/v1/cross-product.md) — **doc-comment** — `Bivector3D`: document the dual relationship to `Vector3D` cross products (component pairing `YZ/ZX/XY` ↔ `(X,Y,Z)` of the cross) under the right-hand convention shared with `AxisAngle`. Without that, rotations and vector algebra feel like unrelated dialects.

90. [cross-product](../lessons/v1/cross-product.md) — **pedagogy** — `Vector2D` lacks a named `Perp` / `Orthogonal` / signed-area helper (`a.X*b.Y - a.Y*b.X`). 2D code then either invents one or lifts to fake 3D crosses; a small planar primitive would keep the "why only 3D" story honest in APIs.

91. [curvature-and-frames](../lessons/v1/curvature-and-frames.md) — **doc-comment** / **pedagogy** — `curves-surfaces.concepts.plato`: `FramedCurve3D.FrameAt` does not specify Frenet vs rotation-minimizing. Split into `FrenetFrameAt` / `RmfFrameAt`, or document the choice — sweeps depend on it.

92. [curvature-and-frames](../lessons/v1/curvature-and-frames.md) — **missing-function** — `differential-geometry.plato`: rich frame *types* exist, but no concept functions like `FrenetAt(curve, t): FrenetFrame3D` or `RmfAt(curve, t0, t1, ...)` are declared on curve concepts. The records are orphaned from `DifferentiableCurve3D` until libraries invent the glue.

93. [curvature-and-frames](../lessons/v1/curvature-and-frames.md) — **missing-function** — `differential-geometry.plato`: `FrenetFrame3D` has no `Parameter` field (unlike RMF). Adding it would make batched frame arrays align for debugging and visualization.

94. [curvature-and-frames](../lessons/v1/curvature-and-frames.md) — **naming** — `curves-surfaces.concepts.plato`: `CurvatureAt` means signed in 2D and unsigned in 3D under the same function name. `SignedCurvatureAt` / `CurvatureAt` split (or a doc banner on each concept) would prevent formula mix-ups.

95. [dot-product](../lessons/v1/dot-product.md) — **missing-function** — `vectors.plato` / `Vector3D`: no `Project(Self, onto: Self)` or `Reject(Self, onto: Self)`. The projection story is half of every dot-product lesson; without both, every caller rewrites `(a.Dot(u))*u` and hopes `u` was normalized.

96. [dot-product](../lessons/v1/dot-product.md) — **missing-function** — no `AngleBetween(Vector3D, Vector3D): Angle` (with documented stability guarantees). Teaching $\cos\theta$ immediately needs a typed `Angle` result, not a raw `Number` radians footgun.

97. [dot-product](../lessons/v1/dot-product.md) — **naming** — `Normed.Magnitude` / `MagnitudeSquared` vs intrinsic `Length` / `LengthSquared` on `Vector3D`. The dual vocabulary is confusing in examples; pick one as canonical in docs and make the other an alias.

98. [dot-product](../lessons/v1/dot-product.md) — **doc-comment** — `Vector.Dot`: state the geometric formula and that inputs need not be unit; mention $\mathbf{a}\cdot\mathbf{a} = \|\mathbf{a}\|^2$. The concept currently names the function with no semantic gloss.

99. [dot-product](../lessons/v1/dot-product.md) — **missing-function** — `Direction3D`: no `Dot(Direction3D, Direction3D)` that bypasses `.Vector` noise. Facing checks are the primary use of directions; a first-class dot would encode unit-unit intent.

100. [easing-functions](../lessons/v1/easing-functions.md) — **missing-function** — `easing.plato`: `ClassicEasing` does not implement `EasingFunction`, so there is no declared `Eval(ClassicEasing, Number)`. The catalog sum is useless for sampling until a library function (or concept implementation) maps each `Eased(family, phase)` case to a curve. Teaching forces this gap into the open.

101. [easing-functions](../lessons/v1/easing-functions.md) — **wrong-shape** — `easing.plato`: `ElasticParameters`, `BackParameters`, and `BounceParameters` are orphaned records — `ClassicEasing.Eased` carries only family and phase, with no slot for the classic amplitude/period/overshoot knobs. Either add parameterized sum cases (`ElasticEased(Phase, ElasticParameters)`, …) or document that those records are only for a future `EvalClassic(…, params)` overload.

102. [easing-functions](../lessons/v1/easing-functions.md) — **missing-concept** — `easing.plato`: `SpringParameters` sits in the easing file but implements neither `EasingFunction` nor `TimeVarying`. A spring is not an $e(t)$ map; it needs state (position, velocity). Either move it beside motion-integration types or declare a `SpringMotion` / `TimeVarying` wrapper so the file's role is clear.

103. [easing-functions](../lessons/v1/easing-functions.md) — **doc-comment** — `easing.plato`: `SmoothstepEasing.Order` should state the closed form for orders 0–2 explicitly (`t`, `3t²−2t³`, `6t⁵−15t⁴+10t³`) so implementers and teachers share one reference without hunting external sources.

104. [engineering-tolerance-fits](../lessons/v1/engineering-tolerance-fits.md) — **wrong-shape** — `uncertainty.plato`: `Tolerance` forces non-negative `Plus`/`Minus` about `Nominal`, but ISO shaft/hole limits often lie entirely above or below the basic size. Teaching fits immediately wants either signed deviations (`UpperDeviation`/`LowerDeviation`) or an explicit `Limits(Lower, Upper)` alternate form; the current shape pushes authors to lie about `Nominal`.

105. [engineering-tolerance-fits](../lessons/v1/engineering-tolerance-fits.md) — **missing-function** — `engineering.plato`: `ShaftHoleFit` stores `Engagement` but nothing computes `FitClass` from the two `Tolerance` bands (min/max clearance). A pure function `ClassifyFit(hole: Tolerance, shaft: Tolerance): FitClass` would make the lesson’s table executable and keep `Engagement` from drifting out of sync.

106. [engineering-tolerance-fits](../lessons/v1/engineering-tolerance-fits.md) — **naming** — `uncertainty.plato`: the type name `Tolerance` collides cognitively with floating-point epsilons and with fields like `PathSimplifyParameters.Tolerance: Number`. Prefer `EngineeringTolerance` or `DimensionalTolerance` so applied-engineering lessons can say the word “tolerance” without disambiguation paragraphs.

107. [engineering-tolerance-fits](../lessons/v1/engineering-tolerance-fits.md) — **missing-type** — `engineering.plato`: no ISO fit designation record (hole basis letter/grade + shaft letter/grade). Lessons and CAD imports routinely start from “H7/g6”; mapping tables have nowhere typed to land.

108. [engineering-tolerance-fits](../lessons/v1/engineering-tolerance-fits.md) — **doc-comment** — `uncertainty.plato`: `Tolerance` should state the unit convention when paired with `Length` (must match `NominalDiameter.Meters` for `ShaftHoleFit`) and warn that measurement types (`UncertainNumber`, `ExpandedUncertainty`) are not substitutes for manufacturing zones.

109. [euler-angles-and-gimbal-lock](../lessons/v1/euler-angles-and-gimbal-lock.md) — **wrong-shape** — `rotations.plato` / `transforms.plato`: `EulerAngles(q: Quaternion)` always returns `RotationOrder.ZXY`, discarding any preferred order the caller might want. A second overload `EulerAngles(q, order: RotationOrder)` (with a documented singularity policy per order) is what interop and teaching both need.

110. [euler-angles-and-gimbal-lock](../lessons/v1/euler-angles-and-gimbal-lock.md) — **doc-comment** — `rotations.plato`: `EulerAngles` fields document names but not the axis mapping (Yaw→Y, Pitch→X, Roll→Z). That mapping currently lives only in `transforms.plato` library banners; it should sit on the type so the declaration file teaches alone.

111. [euler-angles-and-gimbal-lock](../lessons/v1/euler-angles-and-gimbal-lock.md) — **missing-function** — `rotations.plato`: no `IsNearGimbalLock(e: EulerAngles, tolerance: Angle): Boolean` (or on the quaternion before decompose). Authoring tools and this lesson's warnings need a shared predicate rather than re-deriving pole tests.

112. [euler-angles-and-gimbal-lock](../lessons/v1/euler-angles-and-gimbal-lock.md) — **naming** — `intrinsics.plato`: `CreateFromYawPitchRoll` hides the fixed $ZXY$ convention in the name. Aligning the doc comment with `EulerAngles.Order` (or renaming toward `CreateFromEulerZXY`) would reduce the "which yaw-pitch-roll?" confusion this lesson exists to prevent.

113. [euler-angles-and-gimbal-lock](../lessons/v1/euler-angles-and-gimbal-lock.md) — **missing-function** — `intrinsics.plato` / `quantities.plato`: only `Angle(x: Number)` (radians payload) is intrinsic. Authoring examples want `Degrees(x: Number): Angle` (and maybe `Turns`) so UI-shaped snippets are not forced to write raw radian literals for every yaw/pitch/roll dial.

114. [extrusion-along-path](../lessons/v1/extrusion-along-path.md) — **missing-function** — `paths.plato` → `surfaces.plato`: no `ToCurve(path: Path2D, flatten: PathFlattenParameters): Curve2D` (or `Polyline2D`) conversion. Extrusion-along-path teaching always starts from SVG-like outlines and then needs a sweep profile.

115. [extrusion-along-path](../lessons/v1/extrusion-along-path.md) — **naming** — `surfaces.plato`: `ExtrudedSurface` vs `SweptSurface` is correct CAD usage, but many users search for “extrude along path” and will miss `SweptSurface`. A doc-comment alias note (“also called extrude-along-path”) on `SweptSurface` would save a lot of wrong type choices.

116. [extrusion-along-path](../lessons/v1/extrusion-along-path.md) — **wrong-shape** — `surfaces.plato`: `ExtrudedSurface.Profile: Curve3D` vs `SweptSurface.Profile: Curve2D` is teachable but surprising. Consider a shared `Profile2D` + explicit `Plane`/`Frame3D` for linear extrude so both generators consume the same outline type.

117. [extrusion-along-path](../lessons/v1/extrusion-along-path.md) — **missing-type** — `surfaces.plato`: no sweep with a radius/scale law along $V$ (tapered pipes, draft). `TubeSurface` is constant `Radius` only; terrain-style lessons invent ad-hoc workarounds.

118. [extrusion-along-path](../lessons/v1/extrusion-along-path.md) — **missing-concept** — `curves-surfaces.concepts.plato` / `surfaces.plato`: `FramedCurve3D.FrameAt` is the right primitive under sweeps, but `SweptSurface` does not require `Path` to implement `FramedCurve3D` in the type declaration — only in the doc comment’s RMF promise. Encoding that as a concept constraint would make the lesson’s frame discussion type-checkable.

119. [fbm-terrain-intuition](../lessons/v1/fbm-terrain-intuition.md) — **missing-concept** — `noise.plato`: `FbmNoise2D/3D` (and ridged / turbulence) should implement `DifferentiableScalarField2D/3D` or document that gradients are finite-difference only. Terrain lessons always need slope; `GradientAt` is the natural API already declared in `fields.plato`.

120. [fbm-terrain-intuition](../lessons/v1/fbm-terrain-intuition.md) — **doc-comment** — `noise.plato`: `FbmNoise2D` should recommend default starting knobs (`Lacunarity: 2`, `Gain: 0.5`, `Octaves: 4..8`) and warn that `Basis: White` is legal but unsuitable. Teaching time is wasted rediscovering defaults.

121. [fbm-terrain-intuition](../lessons/v1/fbm-terrain-intuition.md) — **missing-function** — `noise.plato`: no `AmplitudeBound(gain: Number, octaves: Integer): Number` helper for the geometric sum. Every author renormalizes fBM by hand to map into metres.

122. [fbm-terrain-intuition](../lessons/v1/fbm-terrain-intuition.md) — **wrong-shape** — `noise.plato`: `DomainWarpNoise2D` omits `Octaves` / `Lacunarity` / `Gain` — it warps a single basis layer. Terrain coastlines usually want warped *fBM*. Either document composing warp ◦ fBM externally or add an `FbmDomainWarpNoise2D` record.

123. [fbm-terrain-intuition](../lessons/v1/fbm-terrain-intuition.md) — **pedagogy** — `fields.plato`: `ScalarFieldGraph2D` is ideal for “fBM × scale + bias” elevation graphs, but wiring `Source(FieldIndex)` to noise values is only explained indirectly. A doc example naming noise as a typical external source would connect the two files the way this lesson must.

124. [floating-point-tolerance](../lessons/v1/floating-point-tolerance.md) — **missing-function** — `uncertainty.plato`: no `Contains(Tolerance, Number): Boolean`, `AlmostEqual(Number, Number, absolute, relative)`, or `WithinUncertainty(UncertainNumber, Number)`. The lesson's acceptance checks are handwritten every time; the types cry out for these predicates.

125. [floating-point-tolerance](../lessons/v1/floating-point-tolerance.md) — **missing-type** — no `FloatingPointTolerance` / `Epsilon` record with `{Absolute, Relative}` fields for numerical (as opposed to engineering) comparison. `Tolerance` is already claimed for manufacturing semantics; overloading it for ulps-style epsilons would muddle the file's story.

126. [floating-point-tolerance](../lessons/v1/floating-point-tolerance.md) — **doc-comment** — `Tolerance`: emphasize that Plus/Minus are **acceptance allowances**, not 1-sigma uncertainties, and point to `UncertainNumber` for the latter. Authors arriving from float-equality blog posts will otherwise stuff epsilons into `Tolerance`.

127. [floating-point-tolerance](../lessons/v1/floating-point-tolerance.md) — **missing-function** — geometric companions: `AlmostEqual(Point3D, Point3D, …)` and `AlmostParallel(Vector3D, Vector3D, Angle)` are absent. Uncertainty geometry has covariances but no simple deterministic epsilon API for the exact `Point3D` / `Vector3D` world most call sites use.

128. [floating-point-tolerance](../lessons/v1/floating-point-tolerance.md) — **pedagogy** — `ErrorPropagation` declares *how* to push uncertainty through code but nothing connects it to `UncertainNumber` arithmetic. A minimal `Add(UncertainNumber, UncertainNumber)` under linear propagation would make the file teachable end-to-end instead of vocabulary-only.

129. [gaussian-distribution](../lessons/v1/gaussian-distribution.md) — **missing-function** — `random.plato`: `ProbabilityDistribution` has `Pdf`/`Cdf`/ `Mean`/`Variance` but no declared `Sample(dist, rng: RandomState) -> (Number, RandomState)` (or equivalent). The file's own header says sampling pairs distribution with `RandomState` later; teaching the Gaussian immediately needs that pairing signature.

130. [gaussian-distribution](../lessons/v1/gaussian-distribution.md) — **naming** — `random.plato`: `NormalDistribution.Mean` (field) collides conceptually with `ProbabilityDistribution.Mean(Self)`. Prefer renaming the field to `Location` or documenting that concept `Mean` must equal the field for this type, to reduce binder and pedagogy confusion.

131. [gaussian-distribution](../lessons/v1/gaussian-distribution.md) — **missing-function** — `random.plato`: no `Standardize(x, dist) -> Number` or `FromStandard(z, dist) -> Number` helpers for the $z = (x-\mu)/\sigma$ transform that every Gaussian lesson (and every Box–Muller / inverse-CDF sampler) uses.

132. [gaussian-distribution](../lessons/v1/gaussian-distribution.md) — **doc-comment** — `random.plato`: `NormalDistribution` should state the invariant `StandardDeviation > 0` and that `Variance` returns its square, matching the file's sample-statistics conventions elsewhere.

133. [geo-coordinates](../lessons/v1/geo-coordinates.md) — **missing-function** — `geo-spatial.plato`: `GeoSegment` and `GeoCoordinate` have no declared `GeodesicDistance`, `InitialBearing`, or `Destination(distance, bearing)` helpers. Teaching “Earth ruins flat vectors” without a named geodesic distance on the vocabulary leaves the punchline unimplemented at the API surface.

134. [geo-coordinates](../lessons/v1/geo-coordinates.md) — **missing-function** — `points.plato` / `geo-spatial.plato`: no declared conversion trio `GeoCoordinate` ↔ `EcefCoordinate` ↔ `EnuCoordinate` (given a `GeodeticDatum`). Those conversions are the bridge every geospatial pipeline needs; they should be first-class once libraries land.

135. [geo-coordinates](../lessons/v1/geo-coordinates.md) — **doc-comment** — `points.plato`: `GeoCoordinate` does not state latitude/longitude ranges or the longitude wrap convention. A one-line normative range (and “altitude above ellipsoid”) would prevent silent degree-vs-radian and wrap bugs in callers.

136. [geo-coordinates](../lessons/v1/geo-coordinates.md) — **pedagogy** — `geo-spatial.plato`: `GeoBounds.ContainsCoordinate` ignores altitude (documented on `GeoRegion`), which is correct for surface regions but surprising next to `GeoCoordinate.Altitude`. A sibling vertical interval or explicit “2.5D” note on `GeoCircle` would clarify airspace vs map-fence use.

137. [geo-distance-haversine](../lessons/v1/geo-distance-haversine.md) — **missing-function** — `geo-spatial.plato`: no `Distance(a: GeoCoordinate, b: GeoCoordinate, radius: Length): Length` (spherical) or `Distance(a: GeoCoordinate, b: GeoCoordinate, ellipsoid: ReferenceEllipsoid): Length` (geodesic). `GeoSegment`, `GeoPath`, and `GeoCircle` all read as if that operation existed; the haversine lesson cannot name it in Plato today.

138. [geo-distance-haversine](../lessons/v1/geo-distance-haversine.md) — **missing-function** — `geo-spatial.plato`: `ContainsCoordinate` on `GeoCircle` needs the same primitive; consider also `InitialBearing(a: GeoCoordinate, b: GeoCoordinate): CompassBearing` (forward azimuth), which navigation lessons always introduce beside distance.

139. [geo-distance-haversine](../lessons/v1/geo-distance-haversine.md) — **pedagogy** — `points.plato`: `GeoCoordinate` doc comment says “above the reference ellipsoid” but the type does not name which datum. Teaching distance requires saying whether altitudes and lat/lon are WGS84 or something else — a `Datum` field or a parallel `GeodeticPosition { Coordinate; Datum }` would make that explicit.

140. [geo-distance-haversine](../lessons/v1/geo-distance-haversine.md) — **doc-comment** — `geo-spatial.plato`: `GeoSegment` should state whether “shortest arc” means spherical great-circle or ellipsoid geodesic, and point at the (currently missing) distance function. Right now both readings are plausible and conflict at ~0.5% relative error.

141. [gradient-descent-step-size](../lessons/v1/gradient-descent-step-size.md) — **missing-function** — `optimization.plato`: no declared step operator such as `DescentStep(params: GradientDescentParameters, x, grad, velocity) -> (x', velocity', params')`. Teaching step size needs a single pure transition; the file only stores knobs.

142. [gradient-descent-step-size](../lessons/v1/gradient-descent-step-size.md) — **wrong-shape** — `optimization.plato`: `GradientDescentParameters` omits `LineSearch` while `LbfgsParameters` includes it. Either add `LineSearch` to gradient descent (with `FixedStep` as default) or document that GD is intentionally fixed-step-only so callers do not hunt for Wolfe settings in vain.

143. [gradient-descent-step-size](../lessons/v1/gradient-descent-step-size.md) — **missing-type** — `optimization.plato`: `OptimizationResult` lacks `GradientNorm: Number` (and optionally last gradient vector). Step-size debugging and "are we actually at a critical point?" checks need it.

144. [gradient-descent-step-size](../lessons/v1/gradient-descent-step-size.md) — **doc-comment** — `optimization.plato`: clarify whether `Momentum` uses classical heavy-ball or Nesterov-style evaluation order; the update formula is not written, and different libraries disagree.

145. [graph-adjacency-basics](../lessons/v1/graph-adjacency-basics.md) — **missing-function** — `graphs.plato`: no conversions `AdjacencyStructure(g: Graph)` / `AdjacencyMatrix(g: Graph)` (and reverse). Teaching representation trade-offs wants a declared build path; without it every caller hand-rolls CSR offsets.

146. [graph-adjacency-basics](../lessons/v1/graph-adjacency-basics.md) — **missing-function** — `graphs.plato`: no `Neighbors(g: AdjacencyStructure, v: GraphVertexIndex):` slice/view API. The CSR layout is documented, but neighbor iteration is not a named operation on `GraphLike`.

147. [graph-adjacency-basics](../lessons/v1/graph-adjacency-basics.md) — **wrong-shape** — `graphs.plato`: `AdjacencyMatrix` uses zero as "no edge," which collides with zero weights. A dedicated missing sentinel or a parallel `Array<Boolean>` presence mask would make weighted dense graphs honest.

148. [graph-adjacency-basics](../lessons/v1/graph-adjacency-basics.md) — **doc-comment** — `graphs.plato`: `Graph` should state explicitly whether undirected edge lists store one canonical direction or both, so `EdgeCount` interpretation matches CSR expansion expectations.

149. [halfedge-topology](../lessons/v1/halfedge-topology.md) — **doc-comment** — `topology.plato`: `HalfEdge.Twin` says $-1$ when the edge is on a boundary, while `Face` says $-1$ when the half-edge runs along a hole. Teaching needs a single clarified invariant: do boundary edges use twin $-1$, face $-1$ on one side, or both? Ambiguity here is the #1 implementation fork among half-edge libraries.

150. [halfedge-topology](../lessons/v1/halfedge-topology.md) — **missing-function** — `topology.plato`: `HalfEdgeNavigable` has atomic steps but no `DestinationOf`, `OppositeFaceOf`, `CirculateVertex`, or `BoundaryLoops(mesh)`. The lesson's query table is exactly the missing helper layer.

151. [halfedge-topology](../lessons/v1/halfedge-topology.md) — **missing-function** — `topology.plato`: no `BuildHalfEdgeMesh(TriangleMesh3D)` (or from `PolygonMesh3D`) declaration. Topology is useless without a defined construction contract (manifold failure mode, winding, boundary twins).

152. [halfedge-topology](../lessons/v1/halfedge-topology.md) — **pedagogy** — `topology.plato`: `CornerTable`, `EdgeAdjacency`, and `HalfEdgeMesh` coexist without a doc guide for when to choose which. A short file-level comparison (memory vs query set vs manifold requirement) would prevent treating them as interchangeable.

153. [helix](../lessons/v1/helix.md) — **missing-concept** — `curves-3d.plato`: `Helix` implements only `Curve3D`, not `DifferentiableCurve3D` or `FramedCurve3D`, despite being the canonical constant curvature/torsion / easy framing example. Declaring those implementations (once libraries exist) would match the math story.

154. [helix](../lessons/v1/helix.md) — **missing-function** — `curves-3d.plato`: no `TurnCount(Helix): Number`, `ArcLength(Helix)`, or `PitchAngle(Helix)` helpers. Teaching pitch vs slope needs either doc formulas or named projections from `Radius`/`Pitch`.

155. [helix](../lessons/v1/helix.md) — **doc-comment** — `curves-3d.plato`: `Helix` says angles are swept but does not state how canonical $t \in [0,1]$ maps into `Angles` (linear in angle is the obvious choice — write it down). Same gap exists for `ConicalSpiral3D`.

156. [helix](../lessons/v1/helix.md) — **naming** — `curves-3d.plato`: `SphericalSpiral3D` uses `TurnCount: Number` while `Helix` uses `Angles: AngleInterval`. A parallel `TurnCount` view (or a factory from turn count) would make multi-turn springs easier to author without hand-building angle intervals.

157. [higher-dimensional-vectors](../lessons/v1/higher-dimensional-vectors.md) — **missing-function** — `vectors.plato`: no declared `Arity`/`Count` alias on `VectorN` beyond `Indexable`/`Countable` inheritance details. A obvious `Dimension(v: VectorN): Integer` (or documented `Count`) helps N-D tutorials.

158. [higher-dimensional-vectors](../lessons/v1/higher-dimensional-vectors.md) — **missing-function** — no `Normalize(VectorN): VectorN` or `DirectionN` type. Unit normals for `HyperplaneN` need a sanctioned construction path.

159. [higher-dimensional-vectors](../lessons/v1/higher-dimensional-vectors.md) — **doc-comment** — `higher-dimensions.plato` already notes removal of fixed 4D geometry; `vectors.plato` should echo "use Number4/Quaternion/VectorN, not Vector4D" so readers of the vectors file alone get the policy.

160. [higher-dimensional-vectors](../lessons/v1/higher-dimensional-vectors.md) — **pedagogy** — `SimplexN` and `SubspaceN` omit `implements Value` unlike neighbors. Either align them or document why — inconsistency distracts when teaching the N-D kit as one family.

161. [histogram-binning](../lessons/v1/histogram-binning.md) — **missing-function** — `statistics.plato`: `Histogram` has no `BinIndex(h, x)`, `Insert(h, x)`, or `AddSample` declared. Teaching binning requires stating the index formula in prose; the library should own `BinIndex(h: Histogram, value: Number): Integer` (−1 if outside range).

162. [histogram-binning](../lessons/v1/histogram-binning.md) — **missing-function** — `statistics.plato`: no `Density(h: Histogram): Array<Number>` that divides counts by `(sum * binWidth)`. Almost every plotting path wants this view; leaving it unnamed invites inconsistent normalization.

163. [histogram-binning](../lessons/v1/histogram-binning.md) — **doc-comment** — `statistics.plato`: state explicitly that `Range.Start < Range.End` is required and that `Counts.Count >= 1`. Reversed `NumberInterval` is legal elsewhere but poisonous here.

164. [histogram-binning](../lessons/v1/histogram-binning.md) — **wrong-shape** — `statistics.plato`: consider a `HistogramBinning` parameter record `{ Range: NumberInterval; BinCount: Integer }` separate from filled `Counts`, so empty templates and accumulated histograms are distinct types rather than "zeros means empty" convention.

165. [homogeneous-coordinates](../lessons/v1/homogeneous-coordinates.md) — **missing-function** — `points.plato`: no `ToHomogeneous(p: Point3D): HomogeneousPoint3D`, `ToPoint(h: HomogeneousPoint3D): Point3D` (with nonzero-$W$ precondition), or `ToHomogeneous(v: Vector3D): HomogeneousPoint3D` with $W=0$. The types are declared; the lift/project API is missing, so call sites invent `Number4` instead.

166. [homogeneous-coordinates](../lessons/v1/homogeneous-coordinates.md) — **missing-function** — `matrices.plato` / transforms: no `Transform(h: HomogeneousPoint3D, m: Matrix4x4): HomogeneousPoint3D` that stays in homogeneous space without forcing an immediate divide — useful for clipping pipelines that need $w$ before the divide.

167. [homogeneous-coordinates](../lessons/v1/homogeneous-coordinates.md) — **doc-comment** — `HomogeneousPoint3D`: state the $W=1$ point / $W=0$ vector convention explicitly on the type, not only in surrounding prose. That convention is the entire reason the type exists beside `Point3D`.

168. [homogeneous-coordinates](../lessons/v1/homogeneous-coordinates.md) — **naming** — perspective divide is only described inside `Transform(Point3D, ProjectiveTransform3D)`. A named `PerspectiveDivide(h: HomogeneousPoint3D): Point3D` would make the step teachable and reusable.

169. [images-as-functions](../lessons/v1/images-as-functions.md) — **missing-function** — `images.plato`: no `Sample(image, uv: UvCoordinate, filter: ResampleFilter): Color` (or per-concrete-type overloads). The function view is unteachable as an API until sampling exists; resize parameters hint at it but do not provide it.

170. [images-as-functions](../lessons/v1/images-as-functions.md) — **missing-concept** — `images.plato`: consider `concept SampledImage inherits Image, Procedural<UvCoordinate, Color>` for linear working images, so `Eval`/`Sample` is discoverable the same way easings expose `Eval`.

171. [images-as-functions](../lessons/v1/images-as-functions.md) — **doc-comment** — `images.plato`: `Bitmap` says "sRGB-encoded" and `FloatImage` says "linear-light"; add an explicit warning that convolution on `Bitmap` is almost always wrong. Processing docs in `46` assume linear inputs without saying so on each record.

172. [images-as-functions](../lessons/v1/images-as-functions.md) — **missing-type** — `image-processing.plato`: compositing needs a `CompositeOp { Blend: BlendMode; Coverage: PorterDuff; }` (plus optional premultiplication flag). Orthogonal enums without a pairing type invite incomplete parameters in every call site.

173. [indexed-meshes](../lessons/v1/indexed-meshes.md) — **missing-function** — `meshes.plato`: `TriangleMesh3D` has no declared builder from flat arrays (`Array<Point3D>` + `Array<Integer>` triples or `Array<VertexIndex>`). Every lesson example hand-assembles `TriangleFace` records; a `FromIndexed` (or `TriangulatedGeometry3D`-level constructor) would match how importers actually arrive at meshes and give one place to validate bounds and reject `-1`.

174. [indexed-meshes](../lessons/v1/indexed-meshes.md) — **missing-function** — `meshes.plato` / `TriangulatedGeometry3D`: no `TriangleCornersAt(x, face: FaceIndex): (Point3D, Point3D, Point3D)` or equivalent returning the three embedded corners in one call. Readers derive it from `FaceAt` + `PositionAt`; a named helper would document the canonical lookup and keep `.Value` leaks out of user code.

175. [indexed-meshes](../lessons/v1/indexed-meshes.md) — **doc-comment** — `topology.plato`: `CornerIndex` doc ties corners to triangle index arithmetic (`c / 3`, `c mod 3`) while `TriangleFace` uses three explicit `VertexIndex` fields. A one-line cross-note on `TriangleFace` ("corners are not `CornerIndex`; use `CornerTable` when corner-table navigation is required") would reduce confusion at the boundary between explicit face records and corner-table topology.

176. [indexed-meshes](../lessons/v1/indexed-meshes.md) — **missing-concept** — `meshes.plato`: no shared **`IndexedSurfaceMesh<P, F>`** (or similar) abstraction factoring `Positions` + face indexing shared by `TriangleMesh3D`, `QuadMesh3D`, and `PolygonMesh3D`. Teaching the vertex-buffer / index-buffer pattern once is natural; three parallel struct shapes suggest a parametric concept with `PositionAt(vertex: VertexIndex): P` and face-count operations would unify importers and validators.

177. [inertia-tensor-diagonal](../lessons/v1/inertia-tensor-diagonal.md) — **missing-function** — `matrices.plato`: `SymmetricMatrix3x3` has no `Eigenvalues` / `Diagonalize` / `Inverse` / `Multiply(SymmetricMatrix3x3, Vector3D)`. Rigid-body lessons cannot even *state* principal-axis extraction in declared ops.

178. [inertia-tensor-diagonal](../lessons/v1/inertia-tensor-diagonal.md) — **missing-function** — `rigid-dynamics.plato`: no `PrincipalMoments(props: MassProperties3D): Number3` or `AlignToPrincipalAxes` helper that returns a corrected local pose + diagonal tensor.

179. [inertia-tensor-diagonal](../lessons/v1/inertia-tensor-diagonal.md) — **naming** — `matrices.plato`: fields `M11…M33` are generic; when used as inertia, doc comments on `MassProperties3D.InertiaTensor` should restate the $I_{xx}$ mapping (partially present in the mass-properties comment, absent on the matrix type).

180. [inertia-tensor-diagonal](../lessons/v1/inertia-tensor-diagonal.md) — **missing-type** — `rigid-dynamics.plato`: a `PrincipalInertia` record `{ Moments: Number3; Frame: Quaternion }` would make the diagonal form first-class instead of leaving authors to overload `SymmetricMatrix3x3` with near-zero off- diagonals.

181. [interpolating-splines](../lessons/v1/interpolating-splines.md) — **missing-function** — `splines.plato`: `CatmullRomCurve2D/3D` expose `Alpha` but declare no helpers to build tangents explicitly, and no conversion `ToHermiteSpline(catmull)`. Teaching "Catmull-Rom is Hermite with automatic tangents" wants that bridge as a named operation.

182. [interpolating-splines](../lessons/v1/interpolating-splines.md) — **doc-comment** — `splines.plato`: `CatmullRomCurve3D` documents alpha values but not the open-curve endpoint tangent policy. One sentence on phantom points vs one-sided differences would remove a major implementation ambiguity for readers.

183. [interpolating-splines](../lessons/v1/interpolating-splines.md) — **missing-type** — `splines.plato`: there is no shared `InterpolatingSpline3D` concept marking "passes through every point." `Curve3D` alone does not distinguish interpolating from approximating families in the type system.

184. [interpolating-splines](../lessons/v1/interpolating-splines.md) — **naming** — `splines.plato`: `CatmullRomCurve3D` is a multi-span spline, while `HermiteCurve3D` is a single segment and `HermiteSpline3D` is multi-span. The Curve-vs-Spline naming is inconsistent across families (Catmull-Rom has no `Spline` sibling name).

185. [intervals-and-bounds](../lessons/v1/intervals-and-bounds.md) — **missing-function** — `intervals-bounds.plato` / `BoundsLike`: the concept library documents that `Contains`, `Union`, `Intersection`, `Expand`, and corner enumeration cannot be derived against `BoundsLike<TPoint>` as declared (no delta type, points are not a `Lattice`). Teaching AABBs without `Union`/`Contains` forces hand-waving — declare `BoundsLike<TPoint, TDelta>` or add Lattice on points, then mirror the `IntervalLike` surface.

186. [intervals-and-bounds](../lessons/v1/intervals-and-bounds.md) — **missing-type** — `intervals-bounds.plato`: no explicit empty-bounds sentinel or `Optional`-style wrapper. Inverted Min/Max is an implicit encoding; a documented `Empty` factory (or sum type) would make the grow-from-empty story teachable and less error-prone.

187. [intervals-and-bounds](../lessons/v1/intervals-and-bounds.md) — **naming** — `intervals-bounds.plato`: `Rect2D` is center+size while `Bounds2D` is min/max; both are rectangles. A doc-comment cross-link stating when to prefer each (layout vs culling) would reduce “which rectangle type?” confusion.

188. [intervals-and-bounds](../lessons/v1/intervals-and-bounds.md) — **doc-comment** — `intervals-bounds.plato`: `NumberInterval` allows Start > End, but the file banner says bounds are inclusive Min/Max without stating whether inverted `Bounds2D` is a supported empty encoding. Pin the empty-bounds convention in the `Bounds2D`/`Bounds3D` comments.

189. [inverse-transforms](../lessons/v1/inverse-transforms.md) — **missing-function** — `transforms.plato`: 3D path has `Transform(v: Vector3D, a: AffineTransform3D)` (linear part) but no `TransformNormal(n: Vector3D, a: AffineTransform3D)` using $(L^{-1})^{\mathsf{T}}$. The 2D side already uses `TransformNormal` on `Matrix3x2`; 3D lighting needs the sibling.

190. [inverse-transforms](../lessons/v1/inverse-transforms.md) — **missing-function** — `transforms.plato`: no `Inverse(t: Transform3D)` even for the common invertible case (nonzero per-axis scale). Authors must drop to matrices; a documented closed-form TRS inverse would match `Inverse(Pose3D)`.

191. [inverse-transforms](../lessons/v1/inverse-transforms.md) — **missing-function** — `matrices.plato`: `Matrix3x3` has no declared `Invert` / `Transpose` / `CanInvert` beside what `Matrix4x4` gets in intrinsics. Normal transforms are $3\times3$ problems; the square linear block deserves the same verbs.

192. [inverse-transforms](../lessons/v1/inverse-transforms.md) — **doc-comment** — `Inverse(pose: Pose3D)`: excellent formula in the comment; add one line that this is the map world→local when `pose` was local→world, tying frames and inverses together for API readers.

193. [joint-constraints-preview](../lessons/v1/joint-constraints-preview.md) — **wrong-shape** — `joints-constraints.plato`: `JointMotor.TargetVelocity` is an untyped `Number` serving both m/s and rad/s. Prefer a sum `LinearVelocity Target | AngularVelocity Target` (quantity types) so hinges cannot silently take linear units.

194. [joint-constraints-preview](../lessons/v1/joint-constraints-preview.md) — **doc-comment** — `joints-constraints.plato`: `BallSocketJoint` swing/twist limits need a precise cone definition (is `SwingLimit.Max` the half-angle from `TwistAxisA`?). The fields are teachable only after that sentence exists.

195. [joint-constraints-preview](../lessons/v1/joint-constraints-preview.md) — **missing-type** — `joints-constraints.plato`: no position/angle *servo* motor (target angle + gains), only velocity `JointMotor`. Door closers and IK-ish drives need it; the lesson currently tells readers to roll their own outer loop.

196. [joint-constraints-preview](../lessons/v1/joint-constraints-preview.md) — **missing-function** — `joints-constraints.plato`: no query for relative hinge angle or slider offset given two body poses. Authoring limits and debugging motors requires that measurement as a declared helper on `HingeJoint` / `SliderJoint`.

197. [keyframes-and-tracks](../lessons/v1/keyframes-and-tracks.md) — **missing-function** — `keyframes-tracks.plato`: `KeyInterpolation` has a `Spring` case, but there is no link to `SpringParameters` from `easing.plato`. A spring segment needs stiffness/damping/mass (or a half-life); without parameters on the key or track, every player invents its own defaults.

198. [keyframes-and-tracks](../lessons/v1/keyframes-and-tracks.md) — **doc-comment / naming** — `keyframes-tracks.plato`: `TransformTrack3D` documents spherical rotation interpolation, yet `AnimationTrack<Quaternion>` and `KeyInterpolation.Linear` still read as "lerp the value." Add an explicit `KeyInterpolation` case (`Spherical` / `Slerp`) or harden the Quaternion-track doc so `Linear` *means* slerp when `T = Quaternion`.

199. [keyframes-and-tracks](../lessons/v1/keyframes-and-tracks.md) — **wrong-shape** — `keyframes-tracks.plato`: `Keyframe<T>.Easing` is always present, including when `Interpolation` is `Bezier`, `Hermite`, or `Constant`, where easing is irrelevant. A sum (`SimpleKey` vs `EasedKey` vs `TangentKey`) would make illegal combinations unrepresentable.

200. [keyframes-and-tracks](../lessons/v1/keyframes-and-tracks.md) — **missing-function** — `keyframes-tracks.plato`: no declared `Evaluate(clip, time): …` or pose-sampling helper that resolves named tracks together. `TimeVarying.Sample` covers one track; clip-level evaluation is what every runtime actually calls.

201. [lights-and-materials](../lessons/v1/lights-and-materials.md) — **missing-function** — `lights.plato` / `materials.plato`: no declared shading entry point such as `Shade(material, lights, geo, view): Color` or even `Irradiance(light, position, normal)`. Photometric fields cannot be taught to completion without an evaluation API that consumes them.

202. [lights-and-materials](../lessons/v1/lights-and-materials.md) — **wrong-shape** — `materials.plato`: `Brdf` exists as a free enum but `Material` does not select one. Either attach `Brdf` to `Material` / `TexturedMaterial` or document that `GgxMetallicRoughness` is mandatory for `Material` and move other BRDFs to alternate material types only.

203. [lights-and-materials](../lessons/v1/lights-and-materials.md) — **doc-comment** — `lights.plato`: `PointLight.Range` of zero means unlimited, while many engines use zero as "disabled." Bold that sentinel in the field comment; it is an easy interoperability footgun.

204. [lights-and-materials](../lessons/v1/lights-and-materials.md) — **missing-type** — `lights.plato`: a sum `type Light = Directional(DirectionalLight) | Point(PointLight) | …` (or a scene list concept) would let examples and scene graphs refer to "a light" without inventing host-side unions. Right now only the `LightSource` concept (shadows only) unifies them.

205. [line-plane-intersection](../lessons/v1/line-plane-intersection.md) — **missing-function** — `lines.plato`: no `Intersect(Line3D, Plane)`, `Intersect(Ray3D, Plane)`, or `Intersect(LineSegment3D, Plane)` despite these being the most common queries on the file's own types. A sum-typed result (`Miss | Point(Point3D) | Line(Line3D)` for the infinite case) would match v3's sum-type conventions.

206. [line-plane-intersection](../lessons/v1/line-plane-intersection.md) — **missing-function** — `lines.plato`: no `SignedDistance(plane: Plane, point: Point3D)` even though the Hesse form makes it a one-liner and every intersection/side test needs it.

207. [line-plane-intersection](../lessons/v1/line-plane-intersection.md) — **missing-type** — `lines.plato`: no local hit/result type for line–plane queries; authors reach for `RayHit3D` from file 35 or invent ad hoc tuples. A small `PlaneHit3D { Point: Point3D; Parameter: Number }` beside the primitives would keep foundation geometry self-contained.

208. [line-plane-intersection](../lessons/v1/line-plane-intersection.md) — **doc-comment** — `lines.plato`: `Plane.Distance` should state the unit-normal precondition in the field comment (it is implied by `Direction3D` but readers still confuse $d$ with Euclidean distance to an arbitrary point).

209. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **missing-function** — `algebra.concepts.plato`: add `InverseLerp(a: Self, b: Self, value: Self): Number` on `Interpolatable` (or a small companion concept). Given endpoints and a value on the line, return the `t` that produced it. Every "map this sensor reading into `[0,1]`" and "where on the segment is this point?" workflow needs inverse lerp; authors should not re-derive `(value - a) / (b - a)` ad hoc with divide-by-zero guards scattered through call sites.

210. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **missing-function** — `algebra.concepts.plato`: add `Remap(value: Self, fromMin: Self, fromMax: Self, toMin: Self, toMax: Self): Self` (or a `Number`-valued overload when remapping scalars). Remap is two inverse lerps composed — from domain to unitless `t`, then into the target range — and appears in every shader, UI layout, and animation rigging lesson that touches normalized coordinates.

211. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **doc-comment** — `color.plato` `Color`: the doc says interpolation is component-wise (good) but should state explicitly that `Lerp` assumes **unpremultiplied linear** RGBA and that lerping `Color8` or sRGB-encoded bytes without conversion is incorrect. This lesson's main color pitfall is invisible from the type declaration alone.

212. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **missing-concept** — `color.plato`: `ColorHSV` and `ColorHSL` implement `Value` but not `Interpolatable`. Hue is an `Angle`; naive RGB lerp and hue-aware lerp diverge. Either document "convert to `Color` before `Lerp`" prominently or declare a separate `HueInterpolatable` (or `LerpHue` on `ColorHSV`) so hue-wheel blending has typed, reviewable semantics.

213. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **doc-comment** — `vectors.plato` `Direction2D` / `Direction3D`: note that `Vector`/`Numerical` inheritance does **not** apply to `Direction2D`/`Direction3D` (they only implement `Value`), but authors coming from other engines assume direction lerp exists. A one-line comment — "normalize after blending the underlying `Vector`, or use angular interpolation" — would prevent a class of rendering bugs.

214. [linear-interpolation](../lessons/v1/linear-interpolation.md) — **pedagogy** — `algebra.concepts.plato` `Interpolatable`: consider a doc-comment example block showing `t` unclamped with one extrapolation case. The comment already states the rule; a single numeric example (`Lerp(0, 10, 1.5) => 15`) would match how often extrapolation surprises newcomers who expect silent clamping.

215. [linear-vs-gamma](../lessons/v1/linear-vs-gamma.md) — **missing-function** — `color.plato` / `color-spaces.plato`: no declared `ToLinear(ColorSRGB): Color`, `ToSRGB(Color): ColorSRGB`, or `Color8` ↔ `Color` conversions with an explicit `TransferFunction`. The doc comments order the pipeline; the vocabulary never names the functions the lesson must teach.

216. [linear-vs-gamma](../lessons/v1/linear-vs-gamma.md) — **naming** — `color.plato`: `Color` is easy to misread as "any RGB." Renaming to `LinearColor` (keeping `Color` as a deprecated alias) or amplifying the doc banner to say "never store encoded sRGB in this type" would match how often this lesson's bug appears.

217. [linear-vs-gamma](../lessons/v1/linear-vs-gamma.md) — **doc-comment** — `Color8`: "typically sRGB-encoded" is soft. State the default assumption (sRGB transfer + sRGB primaries unless tagged otherwise) or pair `Color8` with an explicit `RgbColorSpace` field — untagged bytes are how encode mistakes travel.

218. [linear-vs-gamma](../lessons/v1/linear-vs-gamma.md) — **missing-type** — no `EncodedColor` wrapper tying `(ColorSRGB values × TransferFunction × RgbPrimaries)`. `ColorSRGB` hard-codes sRGB; HDR and Display P3 encoded buffers need a parallel story or everyone invents one.

219. [linear-vs-gamma](../lessons/v1/linear-vs-gamma.md) — **pedagogy** — `Color.Lerp` is component-wise on linear channels (good for light) but the type also tempts perceptual blends. A sibling `Lerp` on `ColorOkLab` called out in `Color`'s doc — "for perceptual midtones prefer ColorOkLab" — would steer authors without forbidding linear lerp.

220. [lines-rays-segments](../lessons/v1/lines-rays-segments.md) — **missing-function** — `lines.plato`: no declared `Parameter(segment|ray|line, point)` returning the unclamped or clamped $t$, and no `PointAt(t)` evaluator. Closest- point teaching wants both; today only `ClosestPoint` appears via `NearestPoint*`.

221. [lines-rays-segments](../lessons/v1/lines-rays-segments.md) — **missing-function** — `lines.plato`: no `ClosestPoints(Line3D, Line3D)` or segment–segment pair query. Skew-line distance is a standard 3D need and is awkward to invent ad hoc beside the types.

222. [lines-rays-segments](../lessons/v1/lines-rays-segments.md) — **missing-function** — `lines.plato`: no conversion helpers `LineSegment3D → Line3D` / `→ Ray3D` (supporting line / ray from A through B). Almost every mesh algorithm needs “the infinite line of this edge.”

223. [lines-rays-segments](../lessons/v1/lines-rays-segments.md) — **doc-comment** — `lines.plato`: `LineSegment2D` says “degenerate when A equals B” but does not state the $t\in[0,1]$ parameterization explicitly (rays/lines do state theirs). Align the segment comment with the same parametric contract.

224. [look-at-camera-basis](../lessons/v1/look-at-camera-basis.md) — **missing-function** — `cameras.plato` / `transforms.plato`: no `Frame3D(look: LookAtCamera)` or `Pose3D(look: LookAtCamera)` conversion is declared. Authors must hand-roll Cross/Normalize or jump to `Matrix4x4.CreateLookAt`, which skips the typed frame entirely.

225. [look-at-camera-basis](../lessons/v1/look-at-camera-basis.md) — **missing-function** — `transforms.plato`: `Frame3D` has `Matrix4x4(f)` (local-to-world) but no named `ViewMatrix(f)` / `WorldToFrame` inverse helper. Look-at teaching constantly needs both directions.

226. [look-at-camera-basis](../lessons/v1/look-at-camera-basis.md) — **doc-comment** — `cameras.plato`: `LookAtCamera` should state which axis is forward in the resulting pose (local $+Z$, $-Z$, etc.) and the cross-product order used when lowered to `CreateLookAt`, so handedness bugs are not rediscovered per backend.

227. [look-at-camera-basis](../lessons/v1/look-at-camera-basis.md) — **pedagogy** — `transforms.plato`: `Basis3D` docs say "not necessarily orthonormal" while `Frame3D` requires orthonormal `Direction3D` axes. A one-line "use Frame3D for rigid cameras; Basis3D for general linear frames" would steer API choice during look-at construction.

228. [matrices-as-machines](../lessons/v1/matrices-as-machines.md) — **missing-function** — `matrices.plato`: `MatrixLike` exposes `ElementAt` but not `Row(i)` / `Column(i)` returning `Number2/3/4`. Teaching "columns/rows are basis images" wants a first-class column/row accessor on the concept, especially under row-storage where columns are gathered.

229. [matrices-as-machines](../lessons/v1/matrices-as-machines.md) — **missing-function** — `matrices.plato`: no `Determinant`, `Trace`, or `IsOrthogonal` on the matrix types/concept. Those are the natural vocabulary for "is this a rotation?" and "does this machine squash volume?"

230. [matrices-as-machines](../lessons/v1/matrices-as-machines.md) — **missing-concept** — `matrices.plato`: there is no `LinearMap` / `SquareMatrix` concept that requires `Multiplicative` + matching row/column counts. `MatrixLike` alone cannot express invertibility or composition.

231. [matrices-as-machines](../lessons/v1/matrices-as-machines.md) — **doc-comment** — `Matrix3x3` / `Matrix4x4`: state in one line that Plato uses row-vector multiplication ($\mathbf{v} M$) so readers do not assume textbook column convention when interpreting `Row1` as a basis image.

232. [matrix-determinant-intuition](../lessons/v1/matrix-determinant-intuition.md) — **missing-function** — `matrices.plato` / `intrinsics.plato`: `Determinant` exists for `Matrix3x2` and `Matrix4x4` but not for `Matrix2x2` or `Matrix3x3`. The 2×2/3×3 cases are exactly where the geometric "area/volume of basis images" story is easiest to teach; they should be first-class.

233. [matrix-determinant-intuition](../lessons/v1/matrix-determinant-intuition.md) — **missing-concept** — `matrices.plato`: `MatrixLike` exposes `ElementAt` but not `Determinant`. Putting `Determinant(x: Self): Number` on the concept (with size-specific bodies) would unify invertibility teaching across fixed and `MatrixN` shapes.

234. [matrix-determinant-intuition](../lessons/v1/matrix-determinant-intuition.md) — **doc-comment** — `matrices.plato`: the file banner should state explicitly whether geometric "column images of basis vectors" refers to columns of the stored row-major layout, so determinant sign discussions stay consistent with `transforms.plato`'s row-vector convention note.

235. [matrix-determinant-intuition](../lessons/v1/matrix-determinant-intuition.md) — **missing-function** — `matrices.plato`: a `LinearPart(m: Matrix4x4): Matrix3x3` (upper-left block) would make the "volume scale of an affine 4×4" story a one-call extraction instead of manual `ElementAt` scraping.

236. [mesh-normals](../lessons/v1/mesh-normals.md) — **missing-function** — `meshes.plato` / `vectors.plato`: computing a face normal needs a cross product, but `Vector3D` / the `Vector` concept do not declare `Cross`. The mesh-normals lesson cannot show an idiomatic one-liner without noting the gap.

237. [mesh-normals](../lessons/v1/mesh-normals.md) — **missing-function** — `meshes.plato` or `mesh-attributes.plato`: no `FaceNormals(mesh)`, `VertexNormals(mesh, weighting)`, or weighting enum (uniform / area / angle). Every engine reimplements this; the attribute file documents where to *store* normals but not how to *author* them from topology.

238. [mesh-normals](../lessons/v1/mesh-normals.md) — **naming** — `mesh-attributes.plato`: well-known channel `"normal"` is described as `Vector3D` while `TangentBasis` uses `Direction3D` for the same geometric role. Prefer one representation (or an explicit doc rule: store non-unit in channels, normalize at use) so smooth-shading code does not guess.

239. [mesh-normals](../lessons/v1/mesh-normals.md) — **doc-comment** — `mesh-attributes.plato`: `AttributeDomain.PerCorner` should state the indexing rule for triangle meshes (corner $c$ → face $c/3$, slot $c \bmod 3$), matching `CornerIndex` in `topology.plato`. Without that, channel length checks are folklore.

240. [mesh-winding-consistency](../lessons/v1/mesh-winding-consistency.md) — **missing-function** — `meshes.plato`: no `FaceNormal(mesh, face: FaceIndex): Direction3D` or `SignedArea` helper that documents the CCW-front contract in code. Callers re-derive Cross(Between…) and can silently swap argument order.

241. [mesh-winding-consistency](../lessons/v1/mesh-winding-consistency.md) — **missing-function** — `topology.plato`: no `AreConsistentlyWound(a: TriangleFace, b: TriangleFace): Boolean` (or edge- based check against `EdgeAdjacency`). Repair tools need a declared predicate for the opposite-traversal rule.

242. [mesh-winding-consistency](../lessons/v1/mesh-winding-consistency.md) — **wrong-shape** — `meshes.plato`: meshes do not store a `WindingOrder` field; the CCW rule is global documentation only. An optional per-mesh `Winding: WindingOrder` would make imported CW data explicit instead of silently wrong under the default assumption.

243. [mesh-winding-consistency](../lessons/v1/mesh-winding-consistency.md) — **doc-comment** — `topology.plato`: `WindingOrder` should mention the reflection/negative-determinant interaction in one sentence — the most common runtime source of "suddenly inverted" meshes after mirroring.

244. [motors-dual-quaternions](../lessons/v1/motors-dual-quaternions.md) — **missing-function** — `transforms.plato`: no `Slerp(a: Motor3D, b: Motor3D, t: Number): Motor3D` (or `ScLERP`). Skinning and this lesson's motivation need a declared blend; `Pose3D.Lerp` is the only rigid interpolation helper today.

245. [motors-dual-quaternions](../lessons/v1/motors-dual-quaternions.md) — **missing-function** — `transforms.plato`: `Motor3D` has `Multiply` / `Compose` / `Inverse` / `Normalize` but no `Dot` or `Antipodal` helper for the sign-alignment step DQS requires. Without it every skinning implementation reinvents the same check.

246. [motors-dual-quaternions](../lessons/v1/motors-dual-quaternions.md) — **doc-comment** — `transforms.plato`: `type Motor3D` should mention the unit dual-quaternion invariant ($|q_r|=1$ and the dual orthogonality condition $q_r\cdot q_d = 0$) explicitly. The packing formula is there; the invariant pair is what `Normalize` is trying to restore.

247. [motors-dual-quaternions](../lessons/v1/motors-dual-quaternions.md) — **naming** — `transforms.plato`: `CreateTranslation(_: Motor3D, v)` is easy to confuse with `Matrix4x4.CreateTranslation`. A name like `FromTranslation` on the motor static side would mirror `Motor3D(pose)` and read clearer in teaching snippets.

248. [noise](../lessons/v1/noise.md) — **wrong-shape** — `noise.plato`: `FbmNoise2D` / `FbmNoise3D` (and turbulence/ridged/ warp) select `Basis: NoiseBasis`, but `Worley` and `Gabor` require parameters that the fractal types do not store. Either document mandatory defaults for those bases, or replace the enum with a sum that can carry Worley/Gabor payloads (or nest a basis noise value). Teaching fBM+Worley currently forces a silent assumption.

249. [noise](../lessons/v1/noise.md) — **missing-function** — `noise.plato`: there is no declared `NoiseGradientAt` / analytic derivative companion for `PerlinNoise*` / `SimplexNoise*`, even though `DifferentiableScalarField3D.GradientAt` exists in `fields.plato`. Noise-as-terrain needs slopes; without a declared gradient story, implementors invent incompatible numeric schemes.

250. [noise](../lessons/v1/noise.md) — **doc-comment** — `noise.plato`: file banner says scalar noises nominally range over $[-1, 1]$, but `WhiteNoise*` is $[0,1]$ and `Worley*` is non-negative with no stated upper bound. Per-type range lines in the doc comments would prevent the classic remap bugs the lesson has to warn about.

251. [noise](../lessons/v1/noise.md) — **missing-type** — `noise.plato`: `CurlNoise3D` has only `Seed` and `Frequency` — no octave/lacunarity controls and no choice of potential basis. Multi-scale curl (common for smoke) has to be layered by the caller with no vocabulary support.

252. [normalization-pitfalls](../lessons/v1/normalization-pitfalls.md) — **missing-function** — `vectors.plato` / `Vector`: add `TryNormalize(self: Vector, fallback: Vector): Vector` or `NormalizeOr(self: Vector, fallback: Vector): Vector` beside the existing preconditioned `Normalize`. Teaching this lesson makes clear that every call site repeats the same `IsZeroLength` guard; the concept library already has `SafeDivide` on `Real` for the analogous scalar case.

253. [normalization-pitfalls](../lessons/v1/normalization-pitfalls.md) — **missing-function** — `vectors.plato` / `Direction2D`, `Direction3D`: declare validated factories, e.g. `FromVector(v: Vector2D): Direction2D` (precondition: non-zero) and `TryFromVector(v: Vector2D, fallback: Direction2D): Direction2D`, plus `FromVectorUnchecked` explicitly documented as unsafe. Today tuple construction `Direction3D(v)` appears in `transforms.plato` but nothing in `vectors.plato` states when wrapping is legal or enforces `IsUnit`.

254. [normalization-pitfalls](../lessons/v1/normalization-pitfalls.md) — **doc-comment** — `vectors.plato` / `Direction2D`, `Direction3D`: the invariant comment should warn that arbitrary `Vector` values may violate unit length after non-rigid transforms or manual construction, and point callers at `IsUnit` for diagnostics. The type promises intent, not runtime proof.

255. [normalization-pitfalls](../lessons/v1/normalization-pitfalls.md) — **pedagogy** — `concept-library/numeric-structures.library.plato` / `Normalize`: pair `IsZeroLength` guidance with `Normalize` in doc comments, or add a guarded helper, so callers do not rediscover near-zero policy independently.

256. [norms-and-distance](../lessons/v1/norms-and-distance.md) — **missing-function** — `algebra.concepts.plato`: **Normed** declares `Magnitude` and `MagnitudeSquared` but not `Normalize`. Normalization is the third leg of the lesson triad (length, distance, normalize); it currently lives only on concrete types via intrinsics (`intrinsics.plato`) and as a derived helper on `Vector` in concept-library. Adding `Normalize(x: Self): Self` to **Normed** (with a documented zero-length precondition) would make the concept self-contained and let `Direction2D`/`Direction3D` factories read as `Normed`-preserving operations.

257. [norms-and-distance](../lessons/v1/norms-and-distance.md) — **missing-concept** — `vectors.plato`: `Vector2D`/`Vector3D` implement **Normed** but not **MetricSpace**, even though Euclidean vector distance is standard. **`Point2D`/`Point3D`** (file 11) likewise lack **MetricSpace** despite being the primary "how far apart are two positions?" types. Implementing **MetricSpace** on geometric points and vectors — with `Distance(a, b) => a.Between(b).Magnitude` for points and `(a - b).Magnitude` for vectors — would let `IsNear`, `IsNearerThan`, and `Nearest` from CoreAlgebra apply directly without the manual `Between(...).Magnitude` chain.

258. [norms-and-distance](../lessons/v1/norms-and-distance.md) — **missing-function** — `algebra.concepts.plato`: **MetricSpace** exposes only `Distance`, not `DistanceSquared`. The **Normed** doc comment already motivates squared magnitude for comparisons; the metric counterpart (`DistanceSquared(a, b)`) appears in concept-library on **Vector** but not on the concept. Declaring it on **MetricSpace** (defaulting to `Distance(a, b).Square` or, for Euclidean types, `Between`/`Subtract` then `MagnitudeSquared`) would make radius and nearest-neighbor tests discoverable at the concept level.

259. [norms-and-distance](../lessons/v1/norms-and-distance.md) — **doc-comment** — `vectors.plato`: **Direction2D** and **Direction3D** doc comments state the unit-length invariant but do not show how to construct one from a `Vector2D`/`Vector3D` safely. A one-line note ("construct via normalization of a non-zero displacement; zero input is undefined") would close the loop between normalization pitfalls and the direction types.

260. [norms-and-distance](../lessons/v1/norms-and-distance.md) — **naming** — `vectors.plato` vs `algebra.concepts.plato`: **Difference**.`Between(a, b)` (displacement from `a` to `b`) shares the name `Between` with **Orderable** interval membership (`Between(x, lower, upper)`). Teaching distance between points forces both names into one lesson. Consider renaming one operation (e.g. `DisplacementTo` on **Difference**, or `InRange` on **Orderable**) to reduce overload confusion in pedagogical material and in API search.

261. [numerical-integration](../lessons/v1/numerical-integration.md) — **missing-type** — `kinematics.plato` / `particles-simulation.plato`: there is no `Integrator` / `IntegrationScheme` sum (`ExplicitEuler | SemiImplicitEuler | Verlet | ...`). The lesson teaches named methods that have no vocabulary hook; cloth implies Verlet via `PreviousPosition` only by documentation.

262. [numerical-integration](../lessons/v1/numerical-integration.md) — **wrong-shape** — `kinematics.plato`: `Trajectory3D` stores parallel arrays but declares no invariant that `Times`, `Positions`, and `Velocities` lengths match (except the empty- velocities escape hatch). A doc-comment invariant or a dedicated sampled-motion concept would harden the teaching examples.

263. [numerical-integration](../lessons/v1/numerical-integration.md) — **missing-function** — `kinematics.plato`: `BallisticTrajectory` and `SimpleHarmonicMotion` look like `Kinematic3D` implementors, but the file never says `implements Kinematic3D`. Wiring that would let `PositionAt` be the single verb for both closed-form and (later) numerically sampled motion.

264. [numerical-integration](../lessons/v1/numerical-integration.md) — **pedagogy** — `particles-simulation.plato`: `SoftBodySettings` and SPH types sit beside particles without a shared "advance by `Duration`" operation. Teaching integration across particles vs cloth requires inventing a step function the declarations do not name.

265. [nyquist-aliasing-preview](../lessons/v1/nyquist-aliasing-preview.md) — **missing-function** — `signals.plato`: no `NyquistFrequency(s: SampledSignal): Frequency` (or on `Frequency` alone: `Nyquist(fs)`). Every sampling lesson and every safe resampler needs this one-liner as a named operation.

266. [nyquist-aliasing-preview](../lessons/v1/nyquist-aliasing-preview.md) — **missing-type** — `signals.plato`: `SampleRateConversion` names interpolation but not an anti-alias policy for decimation. A field such as `AntiAlias: BiquadFilter | FirFilter | None` (or a dedicated sum) would make the Nyquist requirement visible in the type.

267. [nyquist-aliasing-preview](../lessons/v1/nyquist-aliasing-preview.md) — **doc-comment** — `signals.plato`: `SampledSignal` should mention that representable content lies in $[0, \mathrm{SampleRate}/2)$ and that constructing samples from `WaveformGenerator` above that band aliases.

268. [nyquist-aliasing-preview](../lessons/v1/nyquist-aliasing-preview.md) — **pedagogy** — `signals.plato`: `SignalResampling.WindowedSinc` doc should state whether implementations are expected to low-pass on downsample or only interpolate on upsample — the aliasing preview cannot be taught honestly without that contract.

269. [optimization-basics](../lessons/v1/optimization-basics.md) — **missing-function** — `optimization.plato`: abundant parameter and result types, but no `Minimize`, `FindRoot`, or `Solve(LinearProgram)` declarations. The lesson can teach gradient descent only as prose around `GradientDescentParameters`.

270. [optimization-basics](../lessons/v1/optimization-basics.md) — **missing-type** — `optimization.plato`: no `Objective` / `DifferentiableObjective` concept with `Value(Self, Array<Number>)` and optional `Gradient`. Without that, solver parameters float free of any function they could optimize.

271. [optimization-basics](../lessons/v1/optimization-basics.md) — **missing-type** — `optimization.plato`: no `BoxConstraints` separate from full LP — many geometry problems only need per-variable bounds. Teaching projected gradient needs a lighter type than `LinearProgram`.

272. [optimization-basics](../lessons/v1/optimization-basics.md) — **doc-comment** — `optimization.plato`: `OptimizationResult` has both `Converged: Boolean` and `Reason: TerminationReason`. State the invariant (`Converged` iff `Reason == Converged`) so implementors and callers do not disagree.

273. [parametric-curves](../lessons/v1/parametric-curves.md) — **missing-concept** — `curves-surfaces.concepts.plato`: there is no `UnitSpeedCurve` / marker for "parameter equals arc length," and `DifferentiableCurve3D.TangentAt` returns a raw velocity with no `UnitTangentAt`. Teaching constant-speed motion has to narrate a normalize step that the concept surface does not name.

274. [parametric-curves](../lessons/v1/parametric-curves.md) — **missing-function** — `curves-surfaces.concepts.plato`: `ArcLengthParameterized` has total length and conversions, but no `LengthBetween(x, t0, t1)` for partial spans. Dash patterns and subpath measuring need it constantly.

275. [parametric-curves](../lessons/v1/parametric-curves.md) — **doc-comment** — `curves-surfaces.concepts.plato`: the banner states the canonical domain is $[0,1]$, but `PolarCurve2D` and angle-swept concrete curves (elsewhere) use angle domains. A sentence on when concrete types override the canonical domain would prevent concept/type mismatch in readers' heads.

276. [parametric-curves](../lessons/v1/parametric-curves.md) — **pedagogy** — `FramedCurve3D.FrameAt` returns `Frame3D` with "Z axis is tangent," but the concept does not say whether the frame is Frenet or rotation-minimizing. Sweeps care deeply which; the ambiguity should be documented or split into distinct concept functions.

277. [pbr-roughness-metallic](../lessons/v1/pbr-roughness-metallic.md) — **doc-comment** — `Material`: spell out BaseColor's dielectric-vs-metal dual role on the type itself. The file banner mentions glTF MR; the field docs should repeat the dual meaning where authors look first.

278. [pbr-roughness-metallic](../lessons/v1/pbr-roughness-metallic.md) — **missing-function** — no declared `Lerp`/`Mix` guidance for `Material` (metalness blends are nonlinear in appearance). A documented `BlendMaterials` or a warning comment would reduce naive component lerps in LOD transitions.

279. [pbr-roughness-metallic](../lessons/v1/pbr-roughness-metallic.md) — **pedagogy** — `SpecularGlossinessMaterial` says "prefer Material" but does not point at a conversion sketch (gloss → rough = $1 - g$, metalness heuristics). A short conversion note would help importers.

280. [pbr-roughness-metallic](../lessons/v1/pbr-roughness-metallic.md) — **missing-type** — `Brdf` is declared but `Material` does not carry a `Brdf` field; the link is implicit. Either add an optional model selector or document that `GgxMetallicRoughness` is always assumed for `Material`.

281. [perlin-vs-value-noise](../lessons/v1/perlin-vs-value-noise.md) — **doc-comment** — `noise.plato`: `ValueNoise2D/3D` and `PerlinNoise2D/3D` should state the intended output range and whether values may slightly exceed $[-1,1]$. Compare lessons always hit this ambiguity.

282. [perlin-vs-value-noise](../lessons/v1/perlin-vs-value-noise.md) — **missing-function** — `noise.plato`: no `Fade(t: Number): Number` / documented quintic fade as part of the public contract. Teaching Perlin without a named fade makes “smooth interpolation” hand-wavy relative to the rest of v3’s explicitness.

283. [perlin-vs-value-noise](../lessons/v1/perlin-vs-value-noise.md) — **pedagogy** — `noise.plato`: `NoiseBasis` lists `Value` and `Perlin` as peers (good) but nothing in the type system stops someone from assuming they are interchangeable in an `FbmNoise2D`. A short banner comment that “basis choice changes visual family, not just cost” would match what this lesson teaches.

284. [perlin-vs-value-noise](../lessons/v1/perlin-vs-value-noise.md) — **missing-type** — `noise.plato`: no parameters for lattice period / wrapping (`Period: IntegerVector2`). Tiling textures need seamless noise; authors currently fake it with domain tricks outside the type.

285. [perlin-vs-value-noise](../lessons/v1/perlin-vs-value-noise.md) — **naming** — `noise.plato`: consider documenting “gradient noise” as a synonym in the Perlin doc comment. Learners searching for gradient noise otherwise miss the type they want and reinvent value noise under a new name.

286. [planes-halfspaces](../lessons/v1/planes-halfspaces.md) — **missing-function** — `lines.plato`: `Plane` has field `Distance` but no declared `SignedDistance(plane, point)` or `Side` / `Classify` helper. Every clipping lesson wants that name on the surface; leaving it implicit invites divergent implementations.

287. [planes-halfspaces](../lessons/v1/planes-halfspaces.md) — **missing-function** — `lines.plato`: no `Flip(Plane)` / `Flip(HalfSpace)` that negates normal and distance together. Orientation bugs are common; an explicit flip makes the invariant teachable.

288. [planes-halfspaces](../lessons/v1/planes-halfspaces.md) — **missing-function** — `lines.plato`: no `FromPointNormal(point, normal) → Plane` (sets `Distance = Dot(normal, point)`). Constructing Hesse form from a triangle is the usual path into this type.

289. [planes-halfspaces](../lessons/v1/planes-halfspaces.md) — **wrong-shape** — `lines.plato`: `HalfPlane2D` stores normal+distance inline while `HalfSpace` stores `Boundary: Plane`. A `HalfPlane2D` that wrapped a 2D line/Hesse type (or a shared pattern) would make the 2D/3D story easier to teach in parallel.

290. [planes-halfspaces](../lessons/v1/planes-halfspaces.md) — **doc-comment** — `lines.plato`: `Plane.Distance` is “signed distance from the world origin.” Emphasize that it is not the distance from an arbitrary reference point, and that units match the coordinate frame — easy to misread as a generic offset.

291. [point-clouds-voxels](../lessons/v1/point-clouds-voxels.md) — **missing-function** — `pointclouds-voxels.plato`: grids declare Origin/CellSize but no `WorldToCell`, `CellBounds`, or `SampleTrilinear` helpers. Every lesson example has to restate the half-open mapping; those operations belong on the types.

292. [point-clouds-voxels](../lessons/v1/point-clouds-voxels.md) — **missing-concept** — `pointclouds-voxels.plato`: `LevelSetGrid3D` does not implement `SignedDistanceField3D` / `ScalarField3D`, so it cannot `Eval` like `SampledSdf3D` (file 27). Bridging level-set grids into the field vocabulary would unify sampling.

293. [point-clouds-voxels](../lessons/v1/point-clouds-voxels.md) — **wrong-shape** — `pointclouds-voxels.plato` vs `sampling-grids.plato`: dense volumes use Origin+CellSize here but Bounds+CellCounts in sampled scalar grids. Two parameterizations for "regular 3D lattice" force converters and confuse teaching. Pick one canonical grid header or declare explicit conversions.

294. [point-clouds-voxels](../lessons/v1/point-clouds-voxels.md) — **doc-comment** — `pointclouds-voxels.plato`: `AttributedPointCloud3D` says empty channel arrays mean absent, but does not say whether partially filled (length mismatch) is illegal. State the invariant: each non-empty channel length equals `Positions` length.

295. [points-vs-vectors](../lessons/v1/points-vs-vectors.md) — **missing-concept** — `Point3D` (and `Point2D`, `PointN`) do not implement `MetricSpace` in `points.plato`, even though distance between positions is one of the first questions students ask. Either add `MetricSpace` to point types with `Distance(a, b) => Magnitude(Between(a, b))`, or add a short doc comment on `Coordinate` pointing callers at that idiom.

296. [points-vs-vectors](../lessons/v1/points-vs-vectors.md) — **missing-function** — `vectors.plato`: `Vector3D` implements `Normed` but there is no declared `Normalize(x: Vector3D): Vector3D` (nor a concept method on `Normed`). The lesson needs unit directions for almost every geometry example; today only `Direction3D` encodes normalization as a type invariant, with no declared bridge from an arbitrary `Vector3D`.

297. [points-vs-vectors](../lessons/v1/points-vs-vectors.md) — **pedagogy** — `algebra.concepts.plato`: `Difference` names the delta type parameter `TDelta` but the doc comment never states the standard `Between` convention explicitly (displacement from first argument toward second, i.e. $b - a$). One line in the concept comment would prevent sign flips in every downstream transform and animation snippet.

298. [points-vs-vectors](../lessons/v1/points-vs-vectors.md) — **missing-function** — `points.plato`: no declared `ToPoint` / `PositionVector` pair on the point types themselves (only implied by transform libraries elsewhere). For teaching origin-relative vectors, explicit declared converters on `Point3D` ↔ `Vector3D` would keep the lesson inside the foundation files without referring to transform libraries.

299. [polar-cylindrical-spherical](../lessons/v1/polar-cylindrical-spherical.md) — **missing-function** — `points.plato` / `transforms.plato`: no `Point2D(PolarCoordinate)` / `PolarCoordinate(Point2D)` pair (and the 3D cylindrical/spherical analogs). The types exist; without conversions the chart types cannot participate in the point pipeline this lesson describes.

300. [polar-cylindrical-spherical](../lessons/v1/polar-cylindrical-spherical.md) — **missing-function** — `points.plato`: no `IsSingular(p: PolarCoordinate): Boolean` (radius near 0) or spherical `IsNearPole`. Callers need a shared policy for the undefined-angle cases.

301. [polar-cylindrical-spherical](../lessons/v1/polar-cylindrical-spherical.md) — **doc-comment** — `points.plato`: `SphericalCoordinate.Inclination` should explicitly contrast with geographic latitude / elevation-from-equator. One sentence would prevent the most common convention bug the lesson warns about.

302. [polar-cylindrical-spherical](../lessons/v1/polar-cylindrical-spherical.md) — **naming** — `points.plato`: `PolarCoordinate.Angle` vs `CylindricalCoordinate.Azimuth` vs `SphericalCoordinate.Azimuth` — the 2D field is the odd name. Renaming to `Azimuth` (or documenting synonymy) would make the polar↔cylindrical relationship obvious.

303. [polygons-and-winding](../lessons/v1/polygons-and-winding.md) — **missing-function** — `polygons.plato`: no `SignedArea`, `Winding`, or `Reverse` on `Polygon2D`. Teaching shoelace and fixing CW imports wants those names declared beside `PlanarMeasurable.Area` (which may be absolute — pin the sign in docs).

304. [polygons-and-winding](../lessons/v1/polygons-and-winding.md) — **missing-type / wrong-shape** — `polygons.plato` vs `paths.plato`: `FillRule` lives on `Path2D` only. `PolygonWithHoles2D` / `PolygonSet2D` have no fill-rule field because they assume simple nesting — document that self-intersecting polygons must be promoted to `Path2D`, or allow an optional rule for authoring tools.

305. [polygons-and-winding](../lessons/v1/polygons-and-winding.md) — **doc-comment** — `vector-styling.plato`: `PathOffsetParameters` already states left- of-travel / CCW expansion — excellent. Cross-reference the polygon winding banner in `polygons.plato` so style and geometry docs tell one story.

306. [polygons-and-winding](../lessons/v1/polygons-and-winding.md) — **missing-function** — `polygons.plato`: no `ToPath(Polygon2D|PolygonWithHoles2D) → Path2D` implementing `PathLike`. Bridging filled polygons to `StyledPath2D` is the natural authoring path and is currently only implied by the path concept elsewhere.

307. [polynomial-horner-evaluation](../lessons/v1/polynomial-horner-evaluation.md) — **missing-function** — `polynomials.plato`: `Polynomial` has no `Evaluate(Self, Number)` (Horner) and no `EvaluateDerivative` pair. The entire file is evaluation-shaped vocabulary without an evaluation entry point; this lesson cannot show a legal call.

308. [polynomial-horner-evaluation](../lessons/v1/polynomial-horner-evaluation.md) — **missing-function** — `polynomials.plato`: no conversion helpers `ToPolynomial(QuadraticPolynomial)` / `ToQuadratic(Polynomial)` documenting the ascending ↔ descending map. Pedagogy and CAD interop both need them.

309. [polynomial-horner-evaluation](../lessons/v1/polynomial-horner-evaluation.md) — **naming** — `polynomials.plato`: fixed-degree types use school letters `A..E` while dense uses ascending arrays. A short banner comment at the fixed-degree section ("A is highest power; convert with ToPolynomial") would prevent silent swaps.

310. [polynomial-horner-evaluation](../lessons/v1/polynomial-horner-evaluation.md) — **pedagogy** — `polynomials.plato`: `BernsteinPolynomial` should note that Horner in the monomial basis after conversion is numerically inferior to de Casteljau on the Bernstein coefficients — otherwise callers "optimize" the wrong way.

311. [polynomials-and-roots](../lessons/v1/polynomials-and-roots.md) — **missing-function** — `polynomials.plato`: no `Evaluate`, `Horner`, `Derivative`, `Add`, `Multiply`, or `Roots` declarations on `Polynomial` / `QuadraticPolynomial`. The lesson's core verbs are absent; only data shapes exist.

312. [polynomials-and-roots](../lessons/v1/polynomials-and-roots.md) — **naming** — `polynomials.plato`: dual layouts (ascending dense vs descending `QuadraticPolynomial`) are documented but easy to miss. Aliases like `AscendingPolynomial` or conversion functions `ToPolynomial(QuadraticPolynomial)` would make the bridge explicit.

313. [polynomials-and-roots](../lessons/v1/polynomials-and-roots.md) — **missing-type** — `polynomials.plato`: no `RootMultiplicity` pairing or interval isolation result (`IsolatedRoot` with bracket). `PolynomialRoots` is a flat list; Sturm-based pedagogy wants brackets before refinement.

314. [polynomials-and-roots](../lessons/v1/polynomials-and-roots.md) — **doc-comment** — `polynomials.plato`: `BernsteinPolynomial` mentions Bezier control values but does not point at curve types elsewhere. A note that degree is `count(Coefficients) - 1` would match how Bezier degrees are taught.

315. [pose-vs-transform](../lessons/v1/pose-vs-transform.md) — **missing-function** — `transforms.plato`: `Transform3D` has no `Compose(Transform3D, Transform3D)` and the file banner says to compose through matrix/affine forms, but there is no helper such as `ComposeTrs(first, second): AffineTransform3D` that returns the right closed type. Teaching "TRS is not a group" wants a one-liner that lands in `AffineTransform3D` without forcing callers to remember the path.

316. [pose-vs-transform](../lessons/v1/pose-vs-transform.md) — **naming** — `transforms.plato`: `Pose(t: Transform3D): Pose3D` is easy to misread as a constructor. A name like `RigidPart` or `DiscardScale` would make the lossy step louder at call sites the pose-vs-transform lesson keeps emphasizing.

317. [pose-vs-transform](../lessons/v1/pose-vs-transform.md) — **missing-concept** — `transforms.plato`: `Pose3D` implements `Interpolatable` but `Transform3D` does not, with no concept such as `RigidMotion` marking distance-preserving maps. A small marker concept would let generic code (constraints, IK, skinning) require rigidity without listing `Pose3D | Motor3D` by hand.

318. [pose-vs-transform](../lessons/v1/pose-vs-transform.md) — **doc-comment** — `transforms.plato`: `Transform3D` fields are ordered Translation, Rotation, Scale while application is S-R-T. A field-level note ("storage order ≠ application order") would prevent the confusion this lesson has to spell out in prose.

319. [quaternion-double-cover](../lessons/v1/quaternion-double-cover.md) — **doc-comment** — `rotations.plato`: `Quaternion` should state that `q` and `Negative(q)` encode the same rotation. The unit-length invariant alone is not enough; double cover is the #1 consumer footgun.

320. [quaternion-double-cover](../lessons/v1/quaternion-double-cover.md) — **missing-function** — no declared `ApproximatelySameRotation(a, b, tolerance)` or `SameOrientation(a, b)` using $|a\cdot b|$. Without it every engine reimplements fragile ad-hoc checks.

321. [quaternion-double-cover](../lessons/v1/quaternion-double-cover.md) — **missing-function** — no declared `Canonicalize(q)` (e.g. force $W \ge 0$) for deterministic serialization. Optional, but useful beside `Normalize`.

322. [quaternion-double-cover](../lessons/v1/quaternion-double-cover.md) — **pedagogy** — `Interpolatable` on `Quaternion` does not document the hemisphere flip required for shortest-path blends. Either `Slerp` docs (when bodies exist) or the type comment should mention `Dot < 0` negation.

323. [quaternions-without-tears](../lessons/v1/quaternions-without-tears.md) — **doc-comment** — `rotations.plato`: `Quaternion`'s doc states the unit-length invariant but not the double-cover equivalence (`q` and `Negative(q)` encode the same rotation). That fact is the single most common source of confusion for new users and should live on the type declaration.

324. [quaternions-without-tears](../lessons/v1/quaternions-without-tears.md) — **doc-comment** — `rotations.plato`: `Rotor3D` says it is "structurally a quaternion" but does not note the component permutation `(Scalar, YZ, ZX, XY) ↔ (W, X, Y, Z)` documented only in `transforms.plato`. Hoist the isomorphism into the `Rotor3D` comment so readers of `rotations.plato` alone are not misled about field order.

325. [quaternions-without-tears](../lessons/v1/quaternions-without-tears.md) — **missing-function** — `rotations.plato` / `Quaternion`: no declared `ApproximatelySameRotation(a, b, tolerance)` or `DotAbs` helper for orientation comparison mod sign. Every consumer must rediscover the `Dot(a,b) < 0` negation trick and the fact that component equality is wrong; a named predicate on the rotation types would encode the double cover in the API surface.

326. [quaternions-without-tears](../lessons/v1/quaternions-without-tears.md) — **pedagogy** — `intrinsics.plato` hosts `CreateFromAxisAngle`, `Slerp`, and `Concatenate`, while `rotations.plato` declares the type with no `library` block. For teaching and discoverability, either mirror the key factories on a `library Rotations` beside the types, or cross-reference in the file banner — authors grep `rotations.plato` first and miss the hub conversions living in `transforms.plato`.

327. [random-and-distributions](../lessons/v1/random-and-distributions.md) — **missing-function** — `random.plato`: `RandomState` documents draws that return a new state, but no `NextUnitInterval`, `NextInteger`, or `Sample(distribution, rng)` pair is declared. The entire teaching punchline ("pure draw") has no verb on the surface.

328. [random-and-distributions](../lessons/v1/random-and-distributions.md) — **missing-concept** — `random.plato`: `NormalDistribution2D` / `3D` cannot implement `ProbabilityDistribution` (univariate Pdf). A `MultivariateDistribution` concept with `Pdf(Self, VectorND)` would give the Gaussians a home and clarify why they are split out.

329. [random-and-distributions](../lessons/v1/random-and-distributions.md) — **naming** — `random.plato`: `NormalDistribution` has a field also named `Mean`, while the concept function is `Mean(x: Self)`. Teaching "the mean parameter vs the Mean operation" is fine but easy to confuse in prose and in generated APIs.

330. [random-and-distributions](../lessons/v1/random-and-distributions.md) — **doc-comment** — `random.plato`: `VonMisesDistribution` says Pdf/Cdf take radians and `Mean` reports radians, yet `MeanDirection` is `Angle`. Spell the conversion expectation next to the concept mismatch so implementors and lessons agree.

331. [ray-intersection](../lessons/v1/ray-intersection.md) — **wrong-shape** — `spatial-queries.plato`: `RayHit3D` always carries mesh fields (`Face`, `Barycentric`, `Uv`) even for analytic targets. A sum type (`AnalyticHit | MeshHit(...)`) or optional sentinels documented per target would make the "meaningless on sphere" case explicit instead of relying on `-1` and `(0,0)`.

332. [ray-intersection](../lessons/v1/ray-intersection.md) — **missing-function** — `spatial-queries.plato`: `RayIntersectable3D` declares `Raycast` but there is no companion `RaycastAny` / early-out boolean, and no `RaycastAll` returning multiple hits. Shadow rays and CSG need those shapes.

333. [ray-intersection](../lessons/v1/ray-intersection.md) — **missing-type** — `spatial-queries.plato`: no `RayHit3D` field or adjacent type for "origin inside" / entry-vs-exit. Teaching ray-sphere with an interior start has to leave `RayHit3D` and call `ContainsPoint3D` separately.

334. [ray-intersection](../lessons/v1/ray-intersection.md) — **doc-comment** — `lines.plato`: `Ray3D` states $t \ge 0$ but does not say that `Direction` is unit-length so $t$ equals world distance. One sentence tying `Direction3D`'s invariant to distance parameterization would lock the algebra to the type.

335. [reflection-transforms](../lessons/v1/reflection-transforms.md) — **missing-function** — `matrices.plato` / `intrinsics.plato`: `CreateReflection` exists for `Matrix4x4` but there is no `CreateReflection` for `Matrix3x3` or `Matrix2x2` (origin- centered linear mirrors). Teaching planar reflections forces a jump to homogeneous 4×4 even when the problem is 2D.

336. [reflection-transforms](../lessons/v1/reflection-transforms.md) — **naming** — `intrinsics.plato`: `Reflect(self: Vector3D, normal: Vector3D)` takes a bare `Vector3D` for the normal while `CreateReflection` takes a `Plane` whose normal is already a `Direction3D`. Prefer `Reflect(self: Vector3D, normal: Direction3D)` (or document that the vector must be unit) so the unit invariant is not caller folklore.

337. [reflection-transforms](../lessons/v1/reflection-transforms.md) — **doc-comment** — `transforms.plato`: `Pose3D(m: Matrix4x4)` and `Transform3D(m)` mention that reflection is forbidden, but `AffineTransform3D` never states that its linear block *may* have negative determinant. One sentence on orientation-reversing affine maps would tell readers where mirrors actually live.

338. [reflection-transforms](../lessons/v1/reflection-transforms.md) — **missing-function** — `lines.plato`: no `SignedDistance(plane: Plane, point: Point3D)` helper. Reflection lessons (and almost every plane query) need $\mathbf{n}\cdot p - d$; today callers re-derive it from fields ad hoc.

339. [rigid-bodies](../lessons/v1/rigid-bodies.md) — **missing-function** — `rigid-dynamics.plato`: `ForceModel3D.ForceOn` is declared on the concept, but `UniformGravity3D`, `PointGravity`, `DragModel`, and `BuoyancyModel` do not yet list `implements ForceModel3D`. The lesson wants to say "gravity is a force model"; the implements clauses would make that teachable without a wink.

340. [rigid-bodies](../lessons/v1/rigid-bodies.md) — **naming** — `rigid-dynamics.plato`: `RigidBody3D.AngularVelocity` is a `Vector3D` while `RigidBody2D.AngularVelocity` is the quantity type `AngularVelocity`. The asymmetry is documented, but a `Twist3D`-shaped field (or an explicit world-vs-body doc banner) would reduce confusion when teaching spatial angular rates.

341. [rigid-bodies](../lessons/v1/rigid-bodies.md) — **missing-type** — `rigid-dynamics.plato`: there is no `RigidBodyWorld` / simulation container tying `Array<RigidBody3D>`, materials, and `TimeStepSettings` together. Bodies are free-floating records; pedagogy has to invent the "world array" that `BodyIndex` indexes into.

342. [rigid-bodies](../lessons/v1/rigid-bodies.md) — **pedagogy** — `rigid-dynamics.plato`: `MassProperties3D.InertiaTensor` is a `SymmetricMatrix3x3` with no declared helper for "inertia of a solid box/sphere about COM." Deriving mass properties from shapes has nowhere to land those formulas in the vocabulary.

343. [robust-orientation-test](../lessons/v1/robust-orientation-test.md) — **missing-function** — `planar-shapes.plato` / vectors: no declared `Orient2D(a, b, c): Integer` (or a sum type `Left | Right | Collinear`). The winding docs assume the predicate; the API never names it. This is the highest-value add for computational geometry consumers.

344. [robust-orientation-test](../lessons/v1/robust-orientation-test.md) — **missing-function** — no `SignedArea(Triangle2D)` distinct from absolute `PlanarMeasurable.Area`. Teaching robustness needs the signed quantity explicitly.

345. [robust-orientation-test](../lessons/v1/robust-orientation-test.md) — **doc-comment** — `uncertainty.plato`: `Tolerance` is framed as engineering plus/minus about a nominal, not as a geometric epsilon for predicates. A short note on (non-)use for orientation tests would prevent misuse as a global `eps`.

346. [robust-orientation-test](../lessons/v1/robust-orientation-test.md) — **missing-function** — no bridge from `UncertainPoint2D` triples to an uncertain $\Delta$ (`UncertainNumber`). Without propagation helpers, covariance fields stay decorative in geometry kernels.

347. [rotors-and-bivectors](../lessons/v1/rotors-and-bivectors.md) — **missing-function** — `rotations.plato`: no constructor `Rotor3D(plane: Bivector3D, angle: Angle)` (normalize plane, half-angle formula). The GA teaching path wants plane+angle; today you must go axis-angle or quaternion first.

348. [rotors-and-bivectors](../lessons/v1/rotors-and-bivectors.md) — **missing-function** — `rotations.plato`: `Bivector3D` has no `Dual: Vector3D` / `FromDual(Vector3D)` pair documenting the 3D isomorphism used in the quaternion map. The lesson has to state the duality in prose without a named API.

349. [rotors-and-bivectors](../lessons/v1/rotors-and-bivectors.md) — **doc-comment** — `rotations.plato`: `Rotor3D` says it is "structurally a quaternion" but does not spell the component order `(Scalar, YZ, ZX, XY) ↔ (W, X, Y, Z)` on the type. That map currently lives in the transforms library conversion — it belongs on the type banner for GA readers.

350. [rotors-and-bivectors](../lessons/v1/rotors-and-bivectors.md) — **missing-concept** — `rotations.plato`: no shared `Rotor` concept tying `Rotor2D`/`Rotor3D` (sandwich `Transform`, `Inverse` as reverse, `Normalize`). Generic GA code and this lesson's "same idea in 2D and 3D" claim would use it.

351. [sampling-and-grids](../lessons/v1/sampling-and-grids.md) — **missing-function** — `sampling-grids.plato`: `RegularGrid2D` / `RegularGrid3D` declare layout but no helpers such as `NodeCounts`, `CellSize`, `WorldToGrid`, or `CellAt(point)`. Every consumer re-derives the off-by-one node rule; teaching it in prose is a sign the API should own those operations.

352. [sampling-and-grids](../lessons/v1/sampling-and-grids.md) — **missing-type** — `sampling-grids.plato`: there is `SampledColorGrid2D` but no `SampledColorGrid3D`, and no sampled `DirectionField` grid. Volume color and sampled orientation fields are common; file 33's `VoxelColorGrid3D` is a different parameterization (Origin/CellSize vs Bounds/CellCounts), which fractures the mental model.

353. [sampling-and-grids](../lessons/v1/sampling-and-grids.md) — **naming** — `sampling-grids.plato` vs `images.plato`: `SampledColorGrid2D` and `FloatImage` / `GrayscaleImage` both store dense 2D samples with different metadata (`RegularGrid2D` vs `IntegerSize2D`). A doc-comment bridge ("image = grid in pixel index space") or a conversion concept would reduce the dual vocabulary the lesson must explain.

354. [sampling-and-grids](../lessons/v1/sampling-and-grids.md) — **doc-comment** — `images.plato`: `Image` deliberately omits pixel accessors, which is fine, but nothing states how `GrayscaleImage` relates to `ScalarField2D`. Declaring an adapter or noting that images are not `Procedural` over `Point2D` would clarify why `Eval` works on grids but not on `Bitmap`.

355. [scalar-vector-fields](../lessons/v1/scalar-vector-fields.md) — **missing-concept** — `fields.plato`: scalar expression graphs exist (`ScalarFieldGraph2D/3D`) but there is no parallel `VectorFieldGraph3D` / node sum for composing vector fields (add flows, scale, project). Teaching advection pipelines hits this gap immediately after curl and divergence.

356. [scalar-vector-fields](../lessons/v1/scalar-vector-fields.md) — **missing-function** — `fields.plato`: `ScalarFieldGraph3D` has no `Eval` / `implements ScalarField3D`. The graph is inert data until some undeclared interpreter exists. Declaring `ScalarFieldGraph3D implements ScalarField3D` (or a concept `FieldGraph`) would make graphs first-class fields like `ConstantScalarField3D`.

357. [scalar-vector-fields](../lessons/v1/scalar-vector-fields.md) — **missing-function** — `fields.plato`: no `GradientField` wrapper that turns a `DifferentiableScalarField3D` into a `VectorField3D`. The lesson wants to say "the gradient *is* a vector field"; v3 only offers pointwise `GradientAt`, not a reified field value.

358. [scalar-vector-fields](../lessons/v1/scalar-vector-fields.md) — **missing-function** — `vectors.plato` / `algebra.concepts.plato`: `Normed` declares `Magnitude` / `MagnitudeSquared` but there is no `Normalize` (or `Direction3D` factory from `Vector3D`). Gradient-as-normal teaching needs an explicit unitize step on the vocabulary surface.

359. [scalar-vector-fields](../lessons/v1/scalar-vector-fields.md) — **pedagogy** — `fields.plato`: `TensorField2D/3D` and `ComplexField2D` are declared with no differentiable refinements and no constant/graph companions. They are hard to teach alongside the scalar/vector story until Jacobian-level operations or examples appear in doc comments.

360. [scene-graph-hierarchy](../lessons/v1/scene-graph-hierarchy.md) — **missing-function** — `scene3d.plato`: no declared `WorldTransform(scene, node)` or `Children(scene, parent)`. Every host reimplements the walk; naming it locks composition order against `Transform3D` conventions.

361. [scene-graph-hierarchy](../lessons/v1/scene-graph-hierarchy.md) — **missing-function** — no cycle check or `ValidateHierarchy(scene)`. Flat arrays make cycles easy to author by mistake; a pure validator belongs beside the types.

362. [scene-graph-hierarchy](../lessons/v1/scene-graph-hierarchy.md) — **doc-comment** — `SceneNode3D` should restate that `Transform` is parent-relative and that `InstanceSet` transforms are world-space, in one place — the distinction is easy to miss across sections.

363. [scene-graph-hierarchy](../lessons/v1/scene-graph-hierarchy.md) — **pedagogy** — 2D and 3D share the parent-index pattern but diverge on content (`NodeContent2D` sum vs parallel resource slots). A banner cross-note in both files ("same hierarchy mechanics") would help authors port mental models without implying type compatibility.

364. [sdf-operations](../lessons/v1/sdf-operations.md) — **missing-function** — `implicit-sdf.plato`: `SdfTree2D` / `SdfTree3D` declare the tree shape but there is no concept function such as `EvalTree(tree, primitives, point)` on the tree types. Teaching CSG evaluation has to invent the walk; a declared evaluator (even without a body) would pin the contract for leaf resolution and combine semantics.

365. [sdf-operations](../lessons/v1/sdf-operations.md) — **missing-function** — `implicit-sdf.plato`: modifiers (`SdfRoundingModifier`, `SdfShellModifier`, …) are parameter records with no concept tying them to `SignedDistanceField3D`. A `ModifiedSdf3D { Source: ItemIndex; Modifier: ... }` sum type — or concept methods `Round`, `Shell`, `Onion` — would make the apply-step teachable instead of "evaluation context supplies the source."

366. [sdf-operations](../lessons/v1/sdf-operations.md) — **doc-comment** — `implicit-sdf.plato`: `SdfCombine.Blend` should state explicitly that it is linear interpolation of distances, not a smooth Boolean, and that the zero set of a blend is not the blend of the zero sets. The pedagogy gap between Blend and SmoothUnion is the #1 confusion when reading the sum type.

367. [sdf-operations](../lessons/v1/sdf-operations.md) — **wrong-shape** — `implicit-sdf.plato`: `MetaBallSystem3D` implements `ScalarField3D` but not `SignedDistanceField3D`, which is correct numerically, yet nothing in the file offers a conversion or a warning type. A doc note on `SdfNode3D.Leaf` that primitives must be distance-like (not arbitrary scalar fields) would prevent treating metaballs as CSG leaves by accident.

368. [shear-transforms](../lessons/v1/shear-transforms.md) — **missing-function** — `intrinsics.plato`: no `CreateShear` / `CreateSkew` on `Matrix3x2`, `Matrix4x4`, or `Matrix2x2`, despite `CreateScale`, `CreateRotation`, and `CreateTranslation`. The `Matrix2x2` doc comment names shear as a primary use case, but authors must hand-fill rows.

369. [shear-transforms](../lessons/v1/shear-transforms.md) — **missing-function** — `matrices.plato` / `intrinsics.plato`: `Matrix2x2` and `Matrix3x3` declare `Multiplicative` but have no intrinsic `Multiply`, `Determinant`, or `Invert` in `intrinsics.plato` (unlike `Matrix3x2` / `Matrix4x4`). Shear lessons need those operations on the exact types that store linear shears.

370. [shear-transforms](../lessons/v1/shear-transforms.md) — **doc-comment** — `transforms.plato`: `Decompose` / `Transform3D(Matrix4x4)` mention shear as unsupported; add the same note to `Pose3D(Matrix4x4)` (already says no shear) and to a short banner on `Transform3D` itself so readers learn the limitation before hitting the conversion.

371. [shear-transforms](../lessons/v1/shear-transforms.md) — **pedagogy** — `matrices.plato`: `SymmetricMatrix3x3` is documented for inertia and strain, but there is no link from shear-as-transform (this lesson) to shear-as-strain in `engineering.plato`. A cross-file doc pointer would help — without requiring lesson cross-links.

372. [signals-and-sampling](../lessons/v1/signals-and-sampling.md) — **missing-function** — `signals.plato`: `SampledSignal`, `Spectrum`, and `WaveformGenerator` have no `Render`, `Fft`, `Apply(BiquadFilter)`, or `Resample` operations. Teaching Nyquist needs those verbs; only parameter records exist.

373. [signals-and-sampling](../lessons/v1/signals-and-sampling.md) — **missing-type** — `signals.plato`: no explicit `NyquistLimit` helper or `AliasingRisk` doc-tied type linking `Frequency` sample rates to valid tone frequencies. Pedagogy invents the inequality $f_s > 2 f_{\max}$ with nowhere to hang it.

374. [signals-and-sampling](../lessons/v1/signals-and-sampling.md) — **wrong-shape** — `signals.plato`: `Spectrogram.Magnitudes` uses column = time, row = frequency — the opposite of some image conventions (row-major time). A louder banner comment would prevent transposed visualizations.

375. [signals-and-sampling](../lessons/v1/signals-and-sampling.md) — **doc-comment** — `signals.plato`: `SampledSignal` states sample $i$ at $i/f_s$ but does not state whether the first sample is at $t=0$ inclusive for duration `Count/SampleRate` vs `(Count-1)/SampleRate`. Fencepost ambiguity shows up immediately when teaching duration.

376. [signed-distance-fields](../lessons/v1/signed-distance-fields.md) — **missing-type** — `implicit-sdf.plato` declares `SignedDistanceField2D` / `SignedDistanceField3D` concepts and CSG trees, but no closed-form primitive types (e.g. a `Circle`-backed planar SDF or `Sphere`-backed spatial SDF) with the standard $\|p - c\| - r$ implementation. The hand-derived circle formula has no named home in v3.

377. [signed-distance-fields](../lessons/v1/signed-distance-fields.md) — **missing-function** — `SignedDistanceField2D` / `SignedDistanceField3D` carry the sign convention only in doc comments. Pedagogically central queries — `IsInside`, `ClearanceAt` (unsigned distance outside), `IsOnBoundary` with tolerance — are not declared on the concepts. Every snippet re-implements `Eval(sdf, p) < 0` by hand.

378. [signed-distance-fields](../lessons/v1/signed-distance-fields.md) — **missing-function** — `DifferentiableScalarField2D.GradientAt` returns `Vector2D`, but shading and solvers want a unit `Direction2D` on the zero level set. A `NormalAt` returning `Direction2D` / `Direction3D` would match `vectors.plato` and avoid normalize guards at every call site.

379. [signed-distance-fields](../lessons/v1/signed-distance-fields.md) — **doc-comment** — `ScalarField2D` / `ScalarField3D` doc comments list "signed distances" as a use case, but only `implicit-sdf.plato` defines `SignedDistanceField2D` / `SignedDistanceField3D`. A cross-reference in `fields.plato` would clarify that SDFs refine scalar fields rather than introducing a separate evaluation mechanism.

380. [signed-distance-fields](../lessons/v1/signed-distance-fields.md) — **missing-type** — `Circle` and `Sphere` implement `ContainsPoint2D` / `ContainsPoint3D` and `NearestPoint2D` / `NearestPoint3D`, but v3 declares no bridge from those geometry types to `SignedDistanceField2D` / `SignedDistanceField3D`. The geometric and implicit representations of the same solid remain disconnected in the type graph.

381. [skeletal-animation](../lessons/v1/skeletal-animation.md) — **missing-function** — `skeletal-animation.plato`: no declared `ModelPoses(skeleton, localPose) → Array<Pose3D>` or `SkinMatrices(binding, modelPoses) → Array<AffineTransform3D>`. The lesson's core formulas are universal; without named operations every consumer reimplements the forward kinematics loop.

382. [skeletal-animation](../lessons/v1/skeletal-animation.md) — **missing-type** — `skeletal-animation.plato`: `SkinBinding` hard-wires LBS via `AffineTransform3D` inverse binds. A `SkinningMethod = LinearBlend | DualQuaternion` (and optional `Array<Motor3D>` inverse binds) would let the vocabulary express the rigidity-preserving path the dual-quaternion/`Motor3D` story prepares.

383. [skeletal-animation](../lessons/v1/skeletal-animation.md) — **missing-function** — `skeletal-animation.plato`: `SkeletonPose` has no `Blend(a, b, t)` / `ApplyMask(pose, BoneMask)` declarations. Layered animation and upper-body masks are described by `BoneMask` but not operable as typed functions.

384. [skeletal-animation](../lessons/v1/skeletal-animation.md) — **doc-comment** — `skeletal-animation.plato`: `Bone.BindPose` should state explicitly that it is *local* (parent-relative), matching `SkeletonPose`, and that model-space bind lives only as the inverse cache on `SkinBinding`. New readers routinely assume `BindPose` is already model-space.

385. [slerp](../lessons/v1/slerp.md) — **missing-concept** — `algebra.concepts.plato`: `Interpolatable` only has `Lerp`. A sibling concept such as `SphericallyInterpolatable` with `Slerp(a, b, t)` (implemented by `Quaternion`, maybe `Rotor3D`) would make the pose/animation choice discoverable from concepts instead of tribal knowledge that "quaternions use Slerp."

386. [slerp](../lessons/v1/slerp.md) — **doc-comment** — `intrinsics.plato`: `Slerp` should state whether it performs the shortest-path sign flip and what it does for near-parallel inputs. This lesson cannot teach the contract from the declaration alone.

387. [slerp](../lessons/v1/slerp.md) — **missing-function** — `rotations.plato`: `Rotor3D` and `Rotor2D` have `Multiply` / `Normalize` but no `Slerp`. They are isomorphic to unit complex / quaternion forms; animation code that prefers GA naming currently must convert to `Quaternion`, slerp, and convert back.

388. [slerp](../lessons/v1/slerp.md) — **naming** — `intrinsics.plato`: `Lerp` on `Quaternion` is easy to grab by autocomplete when `Slerp` was intended. A doc comment on `Lerp` saying "chord blend; prefer Slerp for orientations" would match the teaching moral.

389. [solid-primitives](../lessons/v1/solid-primitives.md) — **missing-function** — `spatial-primitives.plato`: primitives implement `SpatialMeasurable` (`Volume`, `SurfaceArea`) but v3 declares no evaluation helpers such as `LateralSurfaceArea` vs total area for `Cylinder`/`Cone`, and no `SlantHeight(Cone)` — teaching surface-area breakdowns has to invent those pieces.

390. [solid-primitives](../lessons/v1/solid-primitives.md) — **doc-comment** — `spatial-primitives.plato`: `Sphere` correctly says it is the ball, but the type name remains `Sphere`. A one-line note in the banner that "the boundary sphere proper has no separate type; use `Radius` equality tests or an SDF" would prevent readers from hunting for a hollow-sphere primitive.

391. [solid-primitives](../lessons/v1/solid-primitives.md) — **missing-type** — `spatial-primitives.plato` / `solids.plato`: there is no thin-shell or surface-only counterpart to `Cylinder`/`Cone` (lateral surface without caps). Profile-generated `RevolvedSolid` can approximate them, but a named `CylindricalShell` would match `SphericalShell`'s role for pipes and ducts.

392. [solid-primitives](../lessons/v1/solid-primitives.md) — **naming** — `solids.plato`: `CsgOperation.Difference` is set-difference (A minus B), which is correct, but easy to confuse with the algebraic `Difference` concept on points. A doc-comment cross-warning — or renaming to `Subtract` — would reduce collisions when both appear in one module.

393. [spatial-acceleration](../lessons/v1/spatial-acceleration.md) — **missing-function** — `spatial-structures.plato`: structures implement `SpatialIndex3D` / `RayIntersectable3D` but there are no declared builders (`BuildBvh`, `BuildKdTree`, …). Teaching acceleration without a build contract leaves the cost model and invalidation rules underspecified.

394. [spatial-acceleration](../lessons/v1/spatial-acceleration.md) — **missing-function** — `spatial-queries.plato`: `RadiusQuery3D` exists as a request type, but no concept method `FindInRadius` parallels `FindNearest` on `NearestNeighborQueryable3D`. The query record is stranded without a capability.

395. [spatial-acceleration](../lessons/v1/spatial-acceleration.md) — **wrong-shape** — `spatial-queries.plato`: `RayHit3D` always carries `Face`, `Barycentric`, and `Uv`, with sentinels when inapplicable. For BVH hits against non-mesh primitives those fields are noise. A sum type (`Miss | MeshHit(...) | PrimHit(...)`) would match the tagged-variant preference in the v3 README.

396. [spatial-acceleration](../lessons/v1/spatial-acceleration.md) — **doc-comment** — `spatial-structures.plato`: `BinningGrid3D` cell linearization (X fastest, then Y, then Z) should be repeated on `SpatialHashGrid3D` consumer notes — hash grids use integer coordinates, not linearized dense buckets, and readers conflate the two CSR layouts.

397. [sphere-sphere-intersection](../lessons/v1/sphere-sphere-intersection.md) — **missing-function** — `spatial-primitives.plato`: no `Intersect(Sphere, Sphere)` (or Boolean `Overlaps`) despite both `ContainsPoint3D` and `SupportMappable3D` being present. A sum result `Separate | ExternalTouch | OverlapCircle(Disk3D) | InternalTouch | Nested` would encode the classification table.

398. [sphere-sphere-intersection](../lessons/v1/sphere-sphere-intersection.md) — **missing-type** — `spatial-primitives.plato`: `Disk3D` is a filled patch; there is no `Circle3D` (curve only). Surface–surface intersection naturally returns a circle, not a disk — the vocabulary nudges authors to over-report a filled region.

399. [sphere-sphere-intersection](../lessons/v1/sphere-sphere-intersection.md) — **missing-function** — `collision.plato`: no declared helper to build a `ContactManifold3D` from two world-space `Sphere` values (or `SphereCollider` + poses). Every engine reimplements the same normal/penetration formulas.

400. [sphere-sphere-intersection](../lessons/v1/sphere-sphere-intersection.md) — **doc-comment** — `spatial-primitives.plato`: the `Sphere` comment carefully says "ball" vs "sphere proper"; `SphericalShell` could cross-reference that the region between radii is *not* what `Sphere`–`Sphere` overlap means, reducing solid/surface confusion.

401. [spring-damping-critical](../lessons/v1/spring-damping-critical.md) — **missing-function** — `easing.plato`: no declared `CriticalDamping(stiffness, mass): Number` or `SpringParameters.Critical(stiffness, mass)` factory. The doc states the formula; the API should compute it so callers do not mistype the 2.

402. [spring-damping-critical](../lessons/v1/spring-damping-critical.md) — **missing-function** — no `DampingRatio(params): Number` helper. Debugging feel is much easier in $\zeta$ space than raw $c$.

403. [spring-damping-critical](../lessons/v1/spring-damping-critical.md) — **doc-comment** — `CameraShake.Damping` and `SpringParameters.Damping` share an English name with different meanings. Cross-file disambiguation in both comments would cut confusion (without requiring readers to open motion-graphics when tuning springs).

404. [spring-damping-critical](../lessons/v1/spring-damping-critical.md) — **pedagogy** — `SpringParameters` does not implement `EasingFunction` and has no `Eval` story in the easing file. A short banner note — "integrate against a target; not a normalized ease" — would stop authors from plugging it into `Tween.Easing`.

405. [springs-and-procedural-motion](../lessons/v1/springs-and-procedural-motion.md) — **missing-type** — `easing.plato`: declare something like `SpringState<T> { Value: T; Velocity: T; }` (or a non-generic scalar/vector pair) plus `Step(params, state, target, dt)`. Parameters without state cannot teach or implement reactive springs.

406. [springs-and-procedural-motion](../lessons/v1/springs-and-procedural-motion.md) — **missing-function** — `keyframes-tracks.plato` / `easing.plato`: `KeyInterpolation.Spring` has no typed association with `SpringParameters`. Add parameters on the key/track or a documented default constructor so players share one feel.

407. [springs-and-procedural-motion](../lessons/v1/springs-and-procedural-motion.md) — **naming / doc-comment** — `motion-graphics.plato`: `CameraShake.Damping` vs `SpringParameters.Damping` share a name with different meanings (exponential decay rate vs viscous coefficient). Disambiguate in comments (`DecayRatePerSecond`) or rename one field to prevent copy-paste tuning bugs.

408. [springs-and-procedural-motion](../lessons/v1/springs-and-procedural-motion.md) — **pedagogy** — `motion-graphics.plato`: `Oscillator` implements `TimeVarying<Number>`, but `WiggleMotion` and `CameraShake` do not. Either give them `Sample` via `TimeVarying` (possibly vector-valued) or document why they are inert parameter bags — the asymmetry confuses readers comparing procedural tools.

409. [statistics-of-points](../lessons/v1/statistics-of-points.md) — **missing-function** — `statistics.plato`: rich result types (`SummaryStatistics`, `Histogram`, `Covariance3D`) exist, but no `Summarize(Array<Number>)`, `Centroid(Array<Point3D>)`, or `SampleCovariance(Array<Point3D>)` constructors. Teaching point-set statistics has to invent the verbs that build these records.

410. [statistics-of-points](../lessons/v1/statistics-of-points.md) — **missing-type** — `statistics.plato`: there is no `Medoid` / `GeometricMedian` result, and no point-cloud summary bundling centroid + `Covariance3D`. The lesson's "statistics of points" framing outruns the univariate-first vocabulary.

411. [statistics-of-points](../lessons/v1/statistics-of-points.md) — **doc-comment** — `statistics.plato`: `PolynomialFit.Coefficients` ascending-power convention matches `Polynomial` elsewhere, but the banner does not cross-cite that type. A one-liner would prevent silent disagreement with descending school-form polynomials.

412. [statistics-of-points](../lessons/v1/statistics-of-points.md) — **pedagogy** — `statistics.plato`: `OutlierDetection` and `MovingAverage` are parameter sums without an associated "apply to sample" result type. They read as intent records; lessons cannot show outputs without inventing parallel result shapes.

413. [surface-normal-consistency](../lessons/v1/surface-normal-consistency.md) — **missing-function** — `mesh-attributes.plato`: no `ComputeFaceNormals(mesh: TriangleMesh3D): MeshAttribute<Vector3D>` or `OrientConsistent(mesh): TriangleMesh3D` utilities. The lesson’s whole point is an operation the vocabulary never names.

414. [surface-normal-consistency](../lessons/v1/surface-normal-consistency.md) — **wrong-shape** — `mesh-attributes.plato`: well-known `"normal"` channels are documented as `Vector3D`, but normals are unit directions. Prefer `MeshAttribute<Direction3D>` (new channel group) or document a hard invariant that normal channels must be unit length.

415. [surface-normal-consistency](../lessons/v1/surface-normal-consistency.md) — **missing-concept** — `surfaces.plato`: `OffsetSurface` should implement or require `DifferentiableSurface` on `Base`, and declare how `NormalAt` transforms (same as base for pure normal offset). As written it is only `ParametricSurface`, so the consistency story for offsets is doc-comment folklore.

416. [surface-normal-consistency](../lessons/v1/surface-normal-consistency.md) — **doc-comment** — `mesh-attributes.plato`: `AttributeDomain` should spell out length rules for `"normal"` (`PerFace` → face count, `PerCorner` → $3\times$ triangle count for triangle meshes). Authors guess wrong and desynchronize channels from `Mesh`.

417. [surface-normal-consistency](../lessons/v1/surface-normal-consistency.md) — **missing-function** — `mesh-attributes.plato`: no `FlipNormals(attribute)` / `AlignHandedness` helpers; mirrored UV workflows always reinvent them beside `TangentBasis.Handedness`.

418. [surfaces-of-revolution](../lessons/v1/surfaces-of-revolution.md) — **doc-comment** — `surfaces.plato`: `SurfaceOfRevolution` states the profile X/Y convention but not which UV map to angle vs profile parameter. Spell out "U ↔ angle within `Angles`, V ↔ profile parameter in $[0,1]$" (or the actual choice) so `ClosedU`/`ClosedV` are predictable.

419. [surfaces-of-revolution](../lessons/v1/surfaces-of-revolution.md) — **missing-function** — `surfaces.plato`: no helpers to build the classic profiles (`Cylinder` as revolve of a line, `Sphere` as revolve of a semicircle, `Torus` as revolve of a circle). Teaching the table of special cases wants `AsSurfaceOfRevolution(sphere)`-style bridges — or factories on the primitive types.

420. [surfaces-of-revolution](../lessons/v1/surfaces-of-revolution.md) — **wrong-shape** — `surfaces.plato`: `ExtrudedSurface.Profile` is `Curve3D` while revolve/sweep profiles are `Curve2D`. The asymmetry is defensible but undocumented as a design rule; a banner comment in the generated-surfaces section would prevent "why can't I extrude a Curve2D?" confusion.

421. [surfaces-of-revolution](../lessons/v1/surfaces-of-revolution.md) — **missing-type** — `surfaces.plato`: partial revolves often need end-cap disks as part of a solid workflow; the surface type has no `Capped` flag. Caps today require a separate solid or mesh step — worth a documented companion or flag if lathe UX matters.

422. [texture-filtering-modes](../lessons/v1/texture-filtering-modes.md) — **doc-comment** — `texturing.plato`: `FilterMode.Anisotropic` should state required interaction with `TextureSampler.Anisotropy` (minimum ratio, and whether underlying min/mag is implied to be linear-mip-linear). The lesson cannot specify a portable contract from the declarations alone.

423. [texture-filtering-modes](../lessons/v1/texture-filtering-modes.md) — **naming** — `texturing.plato`: `Nearest` / `Linear` omit "mip" and double as mag filters, while other cases encode both. Consider documenting that bare `Nearest`/ `Linear` mean "no mip selection" (level 0 only) vs "undefined mip," which backends treat differently.

424. [texture-filtering-modes](../lessons/v1/texture-filtering-modes.md) — **missing-function** — `texturing.plato`: no `Sample(binding, uv: UvCoordinate): Color` on `TextureBinding` / `ProceduralTexture` parallelism beyond `ColorAt`. Teaching filtering needs a single sampling entry point that *uses* `TextureSampler`.

425. [texture-filtering-modes](../lessons/v1/texture-filtering-modes.md) — **wrong-shape** — `texturing.plato`: consider splitting mag filter, min filter, and mip mode (as GPU APIs do) instead of one flat `FilterMode` sum — anisotropy and mip bias sit awkwardly beside a single enum that already tried to encode combinations.

426. [time-is-not-a-number](../lessons/v1/time-is-not-a-number.md) — **missing-function** — `time.plato`: `Instant` implements `Difference<Duration>`, but unlike points in the `Transforms` library there is no local library block here showing `Add` / `Between` / `Lerp` bodies for instants. Declaring and documenting those operations next to the types (or a `library Time`) would make the affine story as concrete as `Point3D`.

427. [time-is-not-a-number](../lessons/v1/time-is-not-a-number.md) — **missing-function** — `time.plato`: no conversions `ToDuration(frame: FrameTime): Duration`, `ToInstant(...)`, or `ToDuration(beats: BeatTime, tempo: Tempo): Duration`. The types exist; the bridges into continuous seconds are what every animation/audio lesson needs.

428. [time-is-not-a-number](../lessons/v1/time-is-not-a-number.md) — **missing-function** — `time.plato`: `TimeInterval` has no `Duration(interval)`, `Contains(instant)`, or `Overlaps` helpers. Half-open semantics are stated in a comment but not operable.

429. [time-is-not-a-number](../lessons/v1/time-is-not-a-number.md) — **doc-comment** — `Instant`: state the affine analogy in one line ("time-line point; delta type is Duration, cf. Point/Vector") so the parallel to `points.plato` is discoverable from the declaration alone.

430. [torus-parametric-surface](../lessons/v1/torus-parametric-surface.md) — **missing-concept** — `spatial-primitives.plato`: `Torus` is a solid only. There is no declared `ParametricSurface` (or boundary-surface) view, so UV evaluation and `ClosedU`/`ClosedV` live only as folklore. A `TorusSurface` type or `implements ParametricSurface` on a boundary companion would make the UV map first-class.

431. [torus-parametric-surface](../lessons/v1/torus-parametric-surface.md) — **missing-function** — no declared `Eval(Torus, UvCoordinate): Point3D`, `CenterlinePoint`, or `DistanceToCenterline`. Every mesher reinvents the standard formulas; naming them on the solid would lock conventions (angle zero, normal sense).

432. [torus-parametric-surface](../lessons/v1/torus-parametric-surface.md) — **doc-comment** — the self-intersection note is good, but the comment does not state the volume/surface-area formulas for the $r \le R$ case. Adding them would match how `SpatialMeasurable` is taught elsewhere.

433. [torus-parametric-surface](../lessons/v1/torus-parametric-surface.md) — **pedagogy** — `Direction3D` for `Axis` is correct, yet authors often pass a non-unit `Vector3D`. A factory `Torus.Create(center, axisVector, R, r)` that normalizes (or refuses) would reduce silent tilt errors when the axis is built from two points.

434. [triangle-barycentric-area](../lessons/v1/triangle-barycentric-area.md) — **missing-function** — `points.plato` / `planar-shapes.plato`: no `Barycentric(triangle: Triangle2D, point: Point2D): BarycentricCoordinate` or the inverse `Point(triangle, bary)`. The type exists; the maps that give it meaning do not.

435. [triangle-barycentric-area](../lessons/v1/triangle-barycentric-area.md) — **missing-function** — `planar-shapes.plato`: `PlanarMeasurable.Area` does not specify signed vs absolute in the concept (`geometry.concepts.plato`). Triangles need `SignedArea` explicitly for barycentrics and winding; document or split the API.

436. [triangle-barycentric-area](../lessons/v1/triangle-barycentric-area.md) — **doc-comment** — `points.plato`: `BarycentricCoordinate` should state the vertex binding ($U\to$ first vertex of the triangle, etc.) and clarify behavior when $U+V+W\neq 1$ (off-plane / degenerate input).

437. [triangle-barycentric-area](../lessons/v1/triangle-barycentric-area.md) — **missing-function** — `planar-shapes.plato`: no `IsInside` spelled in terms of barycentrics alongside `Contains`. Teaching materials re-derive the weight test every time; a single documented implementation would lock the epsilon policy.

438. [triangle-geometry](../lessons/v1/triangle-geometry.md) — **missing-function** — `planar-shapes.plato` / `spatial-primitives.plato`: no declared `Normal(Triangle3D)`, `SignedArea(Triangle2D)`, `Circumcenter`, `Incenter`, or `Barycentric(triangle, point)`. The lesson’s toolkit is classical; only `Area` / `Centroid` / `Contains` appear via concepts. Name the rest on the geometry library surface.

439. [triangle-geometry](../lessons/v1/triangle-geometry.md) — **missing-function** — `spatial-primitives.plato`: `Triangle3D` has `NearestPoint3D` but no `Plane` extraction (`FromTriangle → Plane`). Clipping and BSP authors need that one-liner as a declared conversion.

440. [triangle-geometry](../lessons/v1/triangle-geometry.md) — **naming** — `planar-shapes.plato`: `Triangle2D` documents CCW positive area; `Triangle3D` documents right-hand normals. A shared one-line “ordering convention” pointer between the two types would reduce import-time winding mistakes.

441. [triangle-geometry](../lessons/v1/triangle-geometry.md) — **doc-comment** — `spatial-primitives.plato`: state explicitly that `Triangle3D` is a zero-thickness patch (no `ContainsPoint3D`) so learners do not expect solid containment.

442. [trs-transforms](../lessons/v1/trs-transforms.md) — **missing-function** — `transforms.plato`: no `Compose` / `Inverse` on `Transform3D` (documented as intentional), but also no `TryCompose(a, b): Optional<Transform3D>` that succeeds when the product stays in-family (uniform scales, or compatible axes). Authors currently only see the negative space.

443. [trs-transforms](../lessons/v1/trs-transforms.md) — **missing-function** — `transforms.plato`: `Transform3D(m: Matrix4x4)` assumes `Decompose` succeeded and unpacks `Tuple4` as `(X2,X1,X0)` without checking the Boolean. A `TryTransform3D(m): Optional<Transform3D>` matching the precondition comment would make failure teachable.

444. [trs-transforms](../lessons/v1/trs-transforms.md) — **naming** — `Scale: Number3` is correct per the vector naming rule, but newcomers look for `Vector3D`. A doc comment on the field ("per-axis scale factors; not a geometric displacement") would prevent that wrong turn.

445. [trs-transforms](../lessons/v1/trs-transforms.md) — **doc-comment** — `Transform(v: Vector3D, t: Transform3D)`: mention that normals are not displacements under non-uniform scale, so this is the wrong helper for lighting normals.

446. [tuples-vs-vectors](../lessons/v1/tuples-vs-vectors.md) — **missing-function** — `vectors.plato`: no explicit `Vector3D(Number3)` / `Number3(Vector3D)` conversion constructors. The lesson needs a named, visible cast at the semantic boundary; without it, hosts invent ad hoc reinterpret casts that erase the rule.

447. [tuples-vs-vectors](../lessons/v1/tuples-vs-vectors.md) — **doc-comment** — `Number3`: say aloud that it is **not** a geometric vector and list primary roles (scales, homogeneous pre-geometry, channel triples). The file banner states the rule; the type doc should repeat it where grep lands.

448. [tuples-vs-vectors](../lessons/v1/tuples-vs-vectors.md) — **naming** — `IntegerVector2/3/4` vs geometric naming: consider documenting a forbidden list (`Vector3`, `Vec3`, `float3` as Plato source names) in the file banner so codegen authors do not reintroduce them as aliases in Plato text.

449. [tuples-vs-vectors](../lessons/v1/tuples-vs-vectors.md) — **wrong-shape** — `Direction3D` wraps `Vector3D` but `Number3` has no unit-channel counterpart. That asymmetry is fine; a doc note under `Direction3D` ("normalize geometric vectors, not arbitrary Number3 channel triples") would stop RGB-normalization antipatterns.

450. [tuples-vs-vectors](../lessons/v1/tuples-vs-vectors.md) — **pedagogy** — `Vector` concept name collides with everyday "vector" meaning `Vector3D`. A remark on the concept — "algebraic vector family; prefer concrete Vector3D/Number3 at APIs" — would reduce over-abstract call sites that accept any `Vector` and accidentally take `Number8`.

451. [units-in-types](../lessons/v1/units-in-types.md) — **missing-function** — `quantities.plato`: no cross-quantity operators such as `Multiply(a: Length, b: Length): Area`, `Divide(a: Length, b: Length): /* ratio */`, or `Divide(distance: Length, time: /* Duration */): Speed`. The file's own banner says multiplication yields a different type, but nothing declares those maps — the lesson cannot show typed dimensional arithmetic end-to-end.

452. [units-in-types](../lessons/v1/units-in-types.md) — **missing-type** — `quantities.plato`: there is `Speed` (scalar) but no quantity-level companion for "duration as a quantity product partner" inside this file (`Duration` lives in `time.plato`). A documented `Divide(Length, Duration): Speed` (wherever it lives) would make the Length/Speed story teachable without dropping to raw `Number`.

453. [units-in-types](../lessons/v1/units-in-types.md) — **naming** — `quantities.plato`: `Amount(x: Self): Number` on `Quantity` vs per-type fields (`Meters`, `Kilograms`). Callers will wonder whether to read `.Meters` or call `.Amount`. A doc comment stating they are equivalent accessors for the canonical SI value would remove the ambiguity.

454. [units-in-types](../lessons/v1/units-in-types.md) — **doc-comment** — `Temperature` / `TemperatureDelta`: the split is correct and subtle; the comments should state explicitly that you must not add two `Temperature` values as if they were deltas, and that `UnitOfMeasure.OffsetToSI` exists primarily for temperature scales.

455. [vertex-index-safety](../lessons/v1/vertex-index-safety.md) — **missing-function** — `topology.plato`: no `IsNone(i: VertexIndex): Boolean` (or shared `Index` helper) for the $-1$ sentinel. Every safe walk re-implements `Value < 0` by hand; a named predicate would document the convention at the call site.

456. [vertex-index-safety](../lessons/v1/vertex-index-safety.md) — **missing-function** — `meshes.plato`: `TriangulatedGeometry3D` gives `PositionAt` / `FaceAt` but no `TryPositionAt` / bounds-checked variant returning an optional or sentinel. Teaching safety currently stops at "caller must validate."

457. [vertex-index-safety](../lessons/v1/vertex-index-safety.md) — **doc-comment** — `collections.concepts.plato` / `topology.plato`: the `Index` concept should restate the global $-1$ means none rule in one place, since every typed index repeats it in a one-liner that readers may skim past.

458. [vertex-index-safety](../lessons/v1/vertex-index-safety.md) — **naming** — `meshes.plato`: `SlotIndex` is another typed index for materials/batches. A short cross-reference in the topology file's index section would show that the pattern extends beyond mesh elements, reducing "why not just int?" pushback.

459. [voxel-grid-sampling](../lessons/v1/voxel-grid-sampling.md) — **missing-function** — `pointclouds-voxels.plato`: no `WorldToCell(grid, point): IntegerVector3` / `CellBounds(grid, cell): Bounds3D` helpers, even though the file banner defines the mapping in prose. Every consumer re-implements floor division and half-open edge cases.

460. [voxel-grid-sampling](../lessons/v1/voxel-grid-sampling.md) — **missing-function** — `sampling-grids.plato` / `pointclouds-voxels.plato`: no conversion between cell-centred `DensityGrid3D` and node-centred `SampledScalarGrid3D` (offset by half `CellSize`, or box-filter resample). The sampling lesson’s central pitfall has no typed operation.

461. [voxel-grid-sampling](../lessons/v1/voxel-grid-sampling.md) — **wrong-shape** — `sampling-grids.plato`: `InterpolationScheme` mixes 2D and 3D constructors in one sum (`Bilinear` beside `Trilinear`). Prefer separate 2D/3D scheme types so `GridSamplingScheme` for 3D cannot name bilinear.

462. [voxel-grid-sampling](../lessons/v1/voxel-grid-sampling.md) — **missing-type** — `pointclouds-voxels.plato`: only isotropic `CellSize`. Medical volumes need `CellSize: Vector3D` (or `Number3`); teaching anisotropic sampling currently requires a disclaimer that v3 cannot represent it.

463. [voxel-grid-sampling](../lessons/v1/voxel-grid-sampling.md) — **doc-comment** — `pointclouds-voxels.plato`: `DensityGrid3D` should cross-reference `SampledScalarGrid3D` and state “cell-centred, not a `ScalarField3D` by itself” so authors do not assume `Eval` exists on it.
