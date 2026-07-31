# Plato — Execution Plan (content-leads)

*Written 2026-07-09. Turns the reassessment (`plato-reassessment-2026-07-09.md`)
into an ordered, gated execution plan, incorporating the author's five decisions.
This plan is authoritative over the older handoff sequence where they conflict.*

## The author's decisions (2026-07-09) — now locked

1. **Lerp parameter type = `Fraction`** (not `Amount` — `Amount` is already the
   field name on the Scale types). See `naming-fraction-and-rational-types.md`.
2. **"New library"** = snapshot the current library as **`stdlib-snapshot-2026-07-09`**
   (frozen reference), then **keep refactoring `stdlib-legacy` directly** (the freeze
   is lifted). Generate a **new `Plato.Geometry` C# library, created inside the
   Plato module**, inspired by `Ara3D.Geometry` but with **no hand-written C#** —
   it consumes the generated shared project (extension-method + scalar shape) and
   is usable by demo apps. `Ara3D.Geometry` (in ara3d-sdk) is untouched and keeps
   consuming V1 output.
3. **Double precision is a near-term goal.**
4. **Unblock function-valued fields now** (the `procedurals` layer).
5. **Content leads** — library content is the priority, not the GLSL PoC.

## Consequences of the decisions

- `stdlib-legacy` is **writable** now. The old "frozen until Phase 4" rule is retired;
  `stdlib-snapshot-2026-07-09/` is the frozen reference instead. The regen byte-identity
  gate now tracks *intended* changes: when `stdlib-legacy` changes, regen `-Apply`
  and the new `Plato.Generated` is the new baseline. (The gate still protects
  against *unintended* emitter drift — off-flag output must only change when
  source changed.)
- The C# associativity fix is authorized and is the **first** code change, because
  it poisons any `+`/`−` chain — no new content is trustworthy until it lands.
- The new `Plato.Geometry` consumes the **extension-method + `--scalar=float`**
  output shape (today's `golden/Plato.Generated.V2`); double precision adds a
  parallel `--scalar=double` variant.

## Gate discipline (unchanged, non-negotiable)

- `.\tools\check-all.ps1` from studio root after each milestone; keep it green.
- Commit locally per repo in dependency order (Plato → ara3d-sdk → studio bump);
  never push; stage files explicitly (never `git add -A`).
- The Plato CLI exits 0 on compile failure — always verify output file count / build.
- `PROGRESS.md` in the Plato repo kept ≤10 lines, updated as work proceeds.

---

## Phase ordering (top-down execution)

### Track 0 — the universal prerequisite
- [ ] **0.1 Compiler associativity fix** (`Plato.AST/AstNodeFactory.cs`):
  - Line 24: `binRight.Precedence < precedence` → `<=` (left-assoc rebalance for
    equal precedence).
  - Prefix operators applied *before* folding the first `BinaryOperation`/
    `TernaryOperation` postfix (unary minus binds tighter than binary ops).
  - Remove the 5 assoc entries from `KnownFailures.json`; regen; expect ~16 files
    / 93 members changed (32 behavior-changing per the diagnosis doc).
  - Gate: the 5 assoc witnesses flip green; `Witness_CatmullRomStartsAtP1` stays
    green (over-correction guard); check-all green.

### Track A — correct & clean the base (Phase 4)
- [ ] **A.1 `MagnitudeSquared` fix** (`core.library.plato:38` → `v.SumSqrComponents`)
  — unblocks ~10 manifest entries (Magnitude/Length, Normalize, Triangle3DArea…).
- [ ] **A.2 Constants** (`constants.plato`): MinNumber/MaxNumber signs, GoldenRatio
  precedence, full-precision Pi/SqrtTwo/Ln10/… (matters once double lands).
- [ ] **A.3 Algebra** (`algebra.plato`): Barycentric, SmootherStep.
- [ ] **A.4 Curves** (`curves.plato`): SineWave, Lissajous, Epicycloid/Hypocycloid/
  Epitrochoid/Hypotrochoid, Rose, FermatsSpiral.
- [ ] **A.5 Geometry library** (`geometry.library.plato`): Bounds2D.Corners (4 not 8),
  Triangle2D signed-area sign, duplicate `Points(Triangle2D)`.
- [ ] **A.6 Angles** (`angles.plato`): ArcMinutes/ArcSeconds inverted factors.
- [ ] **A.7 Time IMeasure obligations** — implement LessThanOrEquals/Multiply/
  Divide/Modulo for `Time` (mirror other measures), killing the throwing stubs.
- [ ] **A.8 `Point2D.Subtract(Vector2)`** stub → real (mirror the Point3D fix).
- [ ] **A.9 Intrinsics signature fixes** (`intrinsics.plato`): `Repeat` generic,
  `WithNext` return type + rename `first`→`wrapAround`, dedupe `Indices`/`MapIndices`.
- [ ] **A.10 Type-surface defects** (recommendation2 §1–2, linter-confirmed):
  `IBounds<TValue,TDelta>` and `IPrimitiveGeometry3D<PrimitiveT>` broken `where`;
  `IDistanceField` domain (`Point`, not `Vector`); `IMeasure inherits IAdditive`;
  drop lossy `IWholeNumber` Lerp; wire or delete dead `IPolyLine`/`ICurve1D`.
  - Manifest should be empty (or only ADR-blocked) after A. Flip lint toward strict.
- [ ] **A.11 ADR-gated decisions** — write a short ADR + proceed per author:
  TRS/Pose composition order; remove implicit `Number→Angle` (+ add `Radians(Number)`);
  Magnitude/Length unification.

### Track B — foundational concepts (enable generic content)
- [ ] **B.1 `Fraction`** semantic type; thread through `IInterpolatable.Lerp`
  (`t: Number` → `t: Fraction`) and document `[0,1]`-ish / extrapolation semantics.
- [ ] **B.2 `Option<T>` / `Result<T,E>`** (pure value types + functions) — honest
  return types for intersection / inversion / closest-point; retire `-1.0`
  sentinels and success-flag tuples over the portable surface.
- [ ] **B.3 `Tolerance` + `IApproximate` / `IFinite`** — promote the harness's
  mixed abs+rel comparison into the library; unify the scattered epsilons.
- [ ] **B.4 `IInnerProduct` / norm concept** (`Dot`, derived `Length`/`Normalize`)
  — packaged with A.1; unblocks generic projection / Gram–Schmidt / closest-point.
- [ ] **B.5 Function-valued fields** (compiler) — unblock `procedurals.plato`:
  either function-typed fields (`Function1<TIn,TOut>` as a field type) or the
  minimum compiler support the commented-out library needs. Un-comment
  `Combine/Map/MapDomain/Compose/Union/Intersection/Difference`. Gate with new
  witnesses. **This is the keystone for the SDF/effector/deformer layer.**
- [ ] **B.6 `NAMING.md` + C#/Plato split policy**, linter-enforced where cheap.

### Track C — content (the heart of goal 1; `plato-library-roadmap-ideas.md` = spec)
- [ ] **C.1 Port stranded pure C#** (review §4.4): angle utils (`Normalize`,
  `AngularDistance`, `AngularLerp`), axis machinery, point/line queries
  (`Distance(Point,Line)`, `ProjectOntoLine`, `Reject`), bounds ops
  (`Intersects`, `Expand`, `FastTransform`), `RotateTo`/`AlignZAxisWith`,
  bilinear quad eval, `SafeNormalize`. Each becomes multi-target.
- [ ] **C.2 Surfaces library** — `Eval(solid, uv)` for Sphere/Cylinder/Cone/
  ConeSegment/Torus/Capsule/Box/Ellipsoid/Tube/NPrism (review §4.1): fulfils
  `ISolid`, deletes the ChatGPT-drafted `SurfaceFunctions.cs`, resolves
  Sphere/Cylinder duplication.
- [ ] **C.3 SDF 2D+3D catalog + operators** (ideas §1) — IQ primitives + booleans/
  smooth-min/round/onion + domain ops + ray-march + normal-from-gradient. Highest
  content-per-line; this is also the content the GLSL PoC will ride on.
- [ ] **C.4 PRNG + noise** (ideas §9) — hash-PRNG, low-discrepancy sequences,
  value/Perlin/Worley noise, fbm/turbulence, IQ cosine palettes. Unblocks
  scatter/jitter/displacement.
- [ ] **C.5 Surface constructors + frames** (ideas §2.2–2.3): Extrude/Revolve/Ruled/
  Loft/Sweep/Tube, Frenet + rotation-minimizing frames.
- [ ] **C.6 Space-warp deformers + falloff fields** (ideas §3.1, §0.1): the Barr
  trio (Twist/Taper/Bend) + Skew/Bulge/Wave/Spherify/Cubify, all `IDeformable3D`,
  all falloff-modulated by scalar fields (the effector layer). This is the
  author's Deformers/Sample-generator family, in Plato.
- [ ] **C.7 Curve machinery** (ideas §7): arc-length reparam, curve queries,
  Catmull-Rom/B-spline curve *types*, Chaikin/Douglas-Peucker.
- [ ] **C.8 Cloners/distributors** (ideas §5): linear/grid/radial/curve/surface
  distribute, Fibonacci sphere, effector-modulated pose arrays.
- [ ] **C.9 (as appetite) interval arithmetic (§11), mass properties + curvature
  (§8), mesh ops (§3.2), Conway/icosphere (§4.2).**

### Track D — targets, precision, packaging (goal 2 + portability)
- [ ] **D.1 New `Plato.Geometry` C# project** (in the Plato module) consuming the
  extension-method + `--scalar=float` generated shared project; no hand-written
  algorithms. Wire a small demo/console that exercises it.
- [ ] **D.2 Double precision**: `--scalar=double` emit variant +
  `Plato.Intrinsics.Double` (Vector/Matrix/Quaternion double impls); namespace
  `Plato.Geometry.Double`. Cross-precision differential conformance (double as the
  oracle for float). Serves BIM/large-coordinate correctness.
- [ ] **D.3 GLSL PoC (Phase 7)** — now rides on the real C.3 SDF catalog:
  `Plato.GlslWriter` over the array-free subset + CPU/GPU conformance.
- [ ] **D.4 Optimizer 3.2–3.6** (beta reduction, const-fold, CSE/SinCos fusion,
  IArray devirt, hygiene) — output performance (goal 2).
- [ ] **D.5 (bigger bet) native type checker** — accelerates all sustained Plato
  authoring; the linter is the down payment.

---

## Execution notes for a successor agent

- Work top-down. Track 0 first, then A, then B, then C (parallelizable per family),
  then D. Content (C) is the goal-1 payload; do not let D block it.
- Each `stdlib-legacy` edit: `regen-plato.ps1 -Apply` → `check-all.ps1` → commit.
- New content lands as **additive new files** where possible (lower risk), edits to
  existing files where a fix/refactor requires it (now allowed).
- `stdlib-snapshot-2026-07-09/` is the frozen 2026-07-09 reference — never edit it; diff
  against it to see how far `stdlib-legacy` has moved.
- Keep `PROGRESS.md` current. Status log lives in `plato-roadmap.md` (append DONE
  notes there as milestones land).
