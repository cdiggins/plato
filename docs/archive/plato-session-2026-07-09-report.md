> **ARCHIVED 2026-07-16** — session report (historical record). See tracker/DONE.md.

# Plato content-leads session — 2026-07-09 (overnight)

*What ran while you slept. Everything below is committed locally (nothing pushed),
every step gated by `check-all.ps1` (8/8), one repo at a time (Plato → ara3d-sdk →
studio bump). Plan: `plato-execution-plan-2026-07-09.md`.*

## Your five decisions, locked in

1. Lerp parameter = **`Fraction`** (not `Amount`).
2. **`plato-src-legacy`** = frozen snapshot; **`plato-src` is writable** (freeze
   retired); a new `Plato.Geometry` C# library (in the Plato module, no hand-written
   algorithms) is the eventual consumer.
3. Double precision = near-term.
4. Unblock function-valued fields.
5. **Content leads.**

## What landed (all gated green)

**Setup** — `plato-src-legacy/` snapshot (25 files, DO-NOT-EDIT); freeze lifted in
`CLAUDE.md` + `plato-for-agents.md`; execution plan written.

**Track 0 — compiler associativity fix** (`AstNodeFactory.cs`). The prerequisite for
all new `+`/`−` content. Two defects: equal-precedence chains weren't rebalanced
(`<`→`<=`) and prefix operators bound looser than binary ops. Blast radius exactly
the diagnosed 16 files/93 members; the 5 associativity witnesses flipped green.

**Track A — the ENTIRE 36-entry bug wave.** `KnownFailures.json` is now **empty**:
- `MagnitudeSquared` no longer divides by component count (fixed Magnitude/Length/
  Dot laws on Vector2/3/4/8, Triangle3D area, Vector3 magnitude).
- Constants: MinNumber/MaxNumber sign swap, GoldenRatio precedence, irrationals to
  full double precision.
- Barycentric (dropped `v1`), SmootherStep (wrong polynomial).
- 8 curve formulas: SineWave, Lissajous, Epi/Hypo-cycloid, Epi/Hypo-trochoid, Rose,
  FermatsSpiral.
- Triangle2D signed area, Bounds2D corners (8→4), duplicate Points, ArcMinutes/Seconds.
- Time IMeasure obligations (new `Measures` library) and Point2D.Subtract(Vector2).
- Dead `where` clauses on IBounds / IPrimitiveGeometry3D.

**Track C content (first pieces):**
- **`Sdf3D`** library — Inigo-Quilez signed-distance primitives (Sphere, Box,
  RoundBox, Torus, VerticalCapsule, CappedCylinder, Plane) + CSG (Union/Intersect/
  Subtract/SmoothUnion). Pure, GLSL-portable — seeds the future GLSL PoC. 10 witnesses.
- **2D parity + interpolation**: Vector2 `Cross`/`Perpendicular`/`Rotate`, and
  `InverseLerp`/`Remap` on Number. 5 witnesses.

Conformance grew 178 → **200 tests, 0 fail** across all four emitter modes
(V1/V2/Opt/Scalar). `plato-src-legacy` lets you diff the whole delta.

## Two things worth your eyes

1. **Compiler papercut (found writing the SDF library):** the scalar-broadcast
   conversion `Vector3(0.0)` lowers to `((Number)0).Vector3()`, which doesn't
   resolve (the extension wants `float`). Workaround in content: use
   `new Vector3(0.0,0.0,0.0)`. Worth fixing so type-named broadcasts work in library
   bodies — it will bite every content author.
2. **Library-ordinal comment churn:** adding a new `library` file shifts a
   `/* Vectors_21.… */`-style ordinal in ~10 generated files (comment-only, benign,
   but noisy in diffs). Adding functions to *existing* libraries avoids it (that's
   why the 2D-parity change touched only 2 files). A one-line emitter change to drop
   the ordinal from the comment would end the churn for the whole content phase.

## What I deliberately did NOT do (and why)

- **`Fraction` thread-through `IInterpolatable.Lerp`.** Introducing the type is easy;
  changing `t: Number` → `t: Fraction` ripples through every implementer and every
  `x.Lerp(y, 0.5)` call site, and forces `Fraction` to behave like `Number` in
  arithmetic (`b * t`). That's a breaking redesign whose resulting API you should see
  before it lands. **Recommend:** decide whether `Fraction` wraps a `Number` with an
  implicit conversion both ways, then thread it in one reviewed pass.
- **Function-valued fields / `procedurals` unblock.** A genuine compiler task
  (generic constraints in function bodies + lambda-valued construction). The SDF
  catalog was written as plain functions over concrete types, so content is *not*
  blocked on it — but the compositional scene-graph layer is. Worth a focused session.
- **Deeper type-surface changes** (IMeasure inherits IAdditive; IDistanceField domain
  Point vs Vector; dead IPolyLine/ICurve1D concepts) and the **ADR-gated** items
  (TRS/Pose order, removing implicit `Number→Angle`) — these change semantics or
  need your ruling. Left for a considered pass.
- **Double precision, GLSL PoC, new `Plato.Geometry` C# library** — Track D, after
  more content.

## Recommended next steps (in order)

1. **Emitter one-liners:** drop the library ordinal from generated comments; make
   `Vector3(0.0)` broadcast lower correctly. Both remove friction for all further content.
2. **`Fraction`** — decide the wrapping/conversion shape, then thread it (one pass).
3. **More content** (proven pattern, additive, witness-verified): finish the SDF
   catalog (2D SDFs, more 3D primitives, domain ops), then surfaces `Eval(solid,uv)`,
   then PRNG/noise, then the deformer/effector family.
4. **Function-valued fields** (compiler) when you want the dynamic composition layer.
5. **Double precision** (`--scalar=double` + `Plato.Intrinsics.Double`).

## Commit trail (local only)

Plato: setup → assoc fix → bug wave (A.1-A.6) → bug wave (A.7/A.8/A.10a) → Sdf3D →
2D parity. ara3d-sdk: matching regen commits. studio: pointer bumps + planning docs.
`git log` in each repo tells the story; `check-all.ps1` is green at HEAD.
