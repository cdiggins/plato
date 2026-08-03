> **SUPERSEDED, ARCHIVED 2026-07-16** — superseded by plato-execution-plan-2026-07-09.md (see in-file note). Historical record; do not execute.

# Plato Roadmap — Implementation Plan

> **SUPERSEDED IN PART (2026-07-09) by the content-leads plan** —
> `plato-execution-plan-2026-07-09.md`. Author decisions: keep the pre-refactor
> library as `plato-src-legacy`, refactor `plato-src` directly (freeze retired),
> lerp parameter = `Fraction`, double precision near-term, unblock function-valued
> fields, **content leads**. Landed under the new plan: the compiler associativity
> fix (Phase 4 opener) and the **entire 36-entry bug wave** (Phase 4 §1/§8) —
> `KnownFailures.json` is empty, `check-all` 8/8. First content library shipped
> (`Sdf3D`). The Phase 4 section below is therefore DONE; Phases 5/7 and the
> optimizer stages remain as written but re-ordered behind content.

*Execution plan for the workstreams discussed in `../reports/plato-library-review.md`,
`../reports/plato-library-roadmap-ideas.md`, and `docs/discussions/plato-uniqueness-types-july-7-2026-14h42.md`.
Scope: Plato compiler (`submodules/Plato`) and the C# targets in `ara3d-sdk`.
Out of scope for now: TypeScript and Rust writers, and any change to the current library's behavior.*

---

## Ground rules (apply to every phase)

1. **The current library does not change.** `plato-src/*.plato` and `Plato.Generated/*` are frozen except
   for: (a) *additive* new files, and (b) the explicitly gated bug-fix wave in Phase 4. Known bugs stay in
   place until then, tracked in a quarantine manifest (Phase 1).
2. **Every codegen change ships as a parallel artifact first.** New emitter output goes to a new shared
   project (`Plato.Generated.V2`), never overwrites `Plato.Generated`. Adoption = flipping a project
   reference, which is a one-commit revert.
3. **Differential testing is the gate.** No optimizer transform, emitter change, or precision variant is
   adopted until it produces results equal (or within declared tolerance) to the baseline on the shared
   test-vector suite.
4. **Compiler work happens in the Plato submodule** (its own commits, bumped into studio); SDK-side
   projects and tests happen in `ara3d-sdk`. Each phase notes which side it touches.

Dependency graph (→ = "needs"):

```
P0 Safety net
 └→ P1 Verification harness
     ├→ P2 Extension-method emitter (V2)
     │    ├→ P3 Optimizer stage 1 (on V2)
     │    └→ P5 Double-precision target
     ├→ P4 Bug-fix wave (first change to the library)
     └→ P6 Affine intrinsics (List/Buffer) ──→ optimizer stage 2 (later)
P7 GLSL proof of concept (needs a small additive SDF library; independent of P2–P6)
```

---

## Phase 0 — Safety net (SDK side, ~days)

Nothing can be verified, refactored, or optimized until the current state is pinned.

- [x] **0.1 Golden baseline.** DONE 2026-07-07: tag `plato-generated-baseline-2026-07` on ara3d-sdk
      (`0a0f619`, clean tree) and `plato-roadmap-baseline-2026-07` on studio (`c4fd81f`).
- [x] **0.2 One-command regeneration.** DONE: `tools/regen-plato.ps1` (studio repo). Mechanism:
      `dotnet run --project submodules\Plato\Plato.CLI -- <inputDir> <outputDir>` (CLI takes positional
      args; defaults in `Plato.CLI\Config.cs`). Default mode diffs against checked-in output ignoring the
      volatile timestamp header, exit 1 on any difference; `-Apply` copies content-changed files only.
      Caveat: the CLI exits 0 even on compile errors — the script sanity-checks output count.
- [ ] **0.3 Regen-diff CI.** Pipeline step: regenerate, `git diff --exit-code` on `Plato.Generated`.
      This immediately surfaces the known source⇄generated drift (`Degrees(Integer)` differs between
      `angles.plato` and `_Integer.g.cs`). **Resolve the drift by editing whichever side is wrong to match
      the *current shipped behavior*** (generated code is correct here, so fix `angles.plato:4` to
      `.Degrees` — this is behavior-preserving, allowed under ground rule 1).
- [x] **0.3 DONE (2026-07-07): drift reconciled, regen-diff GREEN** (163 identical / 0 differing /
      0 missing / 0 extra, modulo documented Sphere/Cylinder exclusions). Drift was broader than
      suspected — three manual correctness fixes lived only in generated code and were mirrored back to
      plato-src preserving shipped behavior exactly: `angles.plato` Degrees(Integer), `geometry.library.
      plato` Area(Triangle3D) `.Half`, and `Subtract(Point3D, Vector3)`. The hand-added
      `Degrees(this Number)` moved to handwritten `Ara3D.Geometry\NumberAngleExtensions.cs`. Applying
      regen surfaced a Dot/Cross ambiguity (newer compiler emits extension wrappers colliding with
      hand-written duplicates in `Plato.Intrinsics\Vector3.cs`) — resolved by adding instance Dot/Cross
      intrinsics and removing the duplicate extensions; behavior-identical, verified by build + 15/15
      GeometryTests + refreshed API snapshot (8,907 lines). Known latent bug recorded for Phase 4:
      `Point2D.Subtract(Vector2)` throws NotImplementedException (matches source; Point3D had the manual
      fix, Point2D never did). CI wiring note: `regen-plato.ps1` exits 1 on any diff — hook it into
      whatever CI runs when one exists for the monorepo.
- [x] **0.4 API-surface snapshot.** DONE: `ara3d-sdk\tools\ApiSnapshot` + `tools\api-snapshot.ps1` →
      `ara3d-sdk\docs\api\plato-generated-api-baseline.txt`. 550 public types, 8,334 member lines,
      deterministic (byte-identical across runs, ordinally sorted).
- [x] **0.5 Benchmark baseline.** DONE: `ara3d-sdk\tests\Ara3D.SDK.Benchmarks` (BenchmarkDotNet 0.14,
      9 benchmarks), results in `baseline-2026-07-07.md`. Headline: generated component-wise ops
      (ZipComponents `Lerp`, MapComponents `ClampZeroOne`) are ~72 ns/vec with 168–304 B/vec allocation
      vs ~1 ns/vec, 0 B on the intrinsic path (~100×); `Bounds3D.Include` allocates 432 B/point
      (432 MB folding 1M points). Confirms P3.1 unrolling as the top optimizer target. Note: `Abs`/
      `Clamp` on Vector3 are already intrinsic-backed; `Lerp`/`ClampZeroOne` are the generated-machinery
      representatives.
- [x] **0.6 Name-collision exclusions documented.** The compiler *does* generate `_Sphere.g.cs` /
      `_Cylinder.g.cs`; they were deleted from the checked-in tree because handwritten `Ara3D.Geometry`
      structs with the same names (different definitions: C# Cylinder = line+radius, Plato = height+
      radius) would collide. Excluded in `regen-plato.ps1`; real resolution lands with P2.6.

**Exit criteria:** CI fails on any unregenerated edit; baseline tag, API snapshot, and benchmark numbers committed.

---

## Phase 1 — Verification harness (both sides, ~1–2 weeks)

The Plato test library, built so it later becomes the cross-target conformance suite.

- [ ] **1.1 Test-function convention.** Decide and document: a `library XxxTests` whose functions take no
      arguments and return `Boolean` (pure, no exceptions — `false` = fail). Compiler flag
      `--emit-tests` generates an NUnit adapter class per test library
      (`[Test] public void Foo() => Assert.IsTrue(Tests.Foo());`). *(Compiler-side; small emitter task.)*
- [ ] **1.2 Law library (new additive file `plato-src/tests/laws.plato` — does not touch existing files).**
      Generic Boolean functions over interfaces: `AddCommutes(a: IAdditive, b: IAdditive)`,
      `LerpEndpoints`, `NormalizeIsUnit(v: IVector)`, `MagnitudeEqualsLength` where both exist,
      `ClosedCurveCloses(c: IClosedCurve2D)`, unit round-trips (`Degrees∘Degrees ≈ id`), analytic
      derivative vs. central difference for the `Algebra` library, `Invert∘Invert ≈ id` for transforms.
      Monomorphization instantiates each law per implementing type = the test matrix.
- [ ] **1.3 Input vectors, host-side for now.** Hand-written C# generators (seeded, deterministic) feeding
      the law tests: edge values (0, ±1, ±ε, ±max), plus N pseudorandom values per type. (Moving
      generation *into* Plato waits for the PRNG library — additive content, can come any time.)
- [ ] **1.4 Tolerance policy.** One place (`TestTolerances`) defining per-type/per-precision epsilons as
      *parameters*. This is a prerequisite for P5 (double) reusing the same suite.
- [ ] **1.5 Known-failures manifest.** The laws *will* fail on the known bugs (`MagnitudeSquared`,
      `Barycentric`, `SmootherStep`, curve formulas, `Bounds2D.Corners`, …). Since the library is frozen,
      record each as `[KnownFailure("../reports/plato-library-review.md §1.x")]` — the suite stays green, and the
      manifest is the work queue for Phase 4.
- [ ] **1.6 Linter (compiler side, `Plato.CLI lint`).** Five checks over the existing AST/symbol tables:
      unfulfilled interface obligations; `where` clauses naming undeclared type variables; declared-but-
      unused fields; duplicate signatures; generic return-type mismatches. Warnings only at first; the
      current library's violations go into the same manifest.
- [ ] **1.7 Witness tests (additive `plato-src/tests/witness.plato`).** Hand-written formula-vs-citation
      checks for everything with a Wikipedia link: `0.25.Turns.UnitCircle ≈ (0,1)`, cardioid at 0/π,
      known Bezier midpoints, Platonic solid vertex counts, etc.

**Exit criteria:** `dotnet test` runs generated law + witness tests; manifest enumerates every known
failure with a review-doc reference; `plato lint` runs in CI (non-blocking).

**Phase 1 status (2026-07-07): DONE — including 1.6.** Linter shipped as `Plato.CLI lint <folder>
[--strict]` (`PlatoCompiler\Analysis\Linter.cs`), all five checks implemented. Findings on plato-src:
246 = LINT001 unfulfilled obligations ×91 (Time arithmetic, Point2D.Subtract, IRotation3D.Quaternion,
every ISolid Eval/ClosedX/ClosedY) + LINT002 undeclared where-vars ×2 + LINT003 unused fields ×150
(mostly the functionless-types class) + LINT004 duplicate signatures ×2 (incl. new find:
`Range(Integer)` ×2 in intrinsics.plato) + LINT005 generic mismatch ×1 (WithNext). plato-test-src: 0.
Review corrections: `Lissajous.A` and `LookAt3D.Origin` are used in current source (fixed since the
review was written) — the linter correctly does not flag them.
**Associativity bug diagnosed** (docs/plato-assoc-bug-diagnosis.md): root cause `Plato.AST\
AstNodeFactory.cs` — strict `<` in the precedence rebalance (line 24) + prefix ops applied after
postfix folding (118–122). Blast radius measured: 16 files / 93 members, 32 behavior-changing —
SmoothStep, Hermite(±Derivative), CatmullRom(±Derivative), Quadratic/CubicBezierSecondDerivative, and
**vector `FromOne` = −(x+1)** (new find, poisons generic Lerp). Fix prototyped, verified, reverted;
lands as the FIRST item of the Phase 4 wave with the 5 assoc witnesses as its gate.

Implementation notes:
- Laws/witnesses live OUTSIDE plato-src in `submodules\Plato\plato-test-src\` (laws must never enter the
  production API); `tools\regen-conformance.ps1` merges plato-src + plato-test-src and generates into
  `ara3d-sdk\tests\Ara3D.SDK.ConformanceTests\Generated\` (gitignored, rebuilt on demand). The test
  project compiles the merged output standalone (no Ara3D.Geometry reference → no Sphere/Cylinder
  collision) + a reflection-driven law runner (seeded, N=25 instances/type). The 1.1 compiler
  `--emit-tests` flag was NOT needed — deferred to the linter/type-checker workstream.
- **Suite: 178 tests = 129 (type×law) + 46 witnesses + 3 harness; 142 pass, 36 ignored-known
  (all manifest-backed), 0 fail.** All 23 laws compiled first try; laws monomorphized onto 15 types.
- `KnownFailures.json` (36 entries) is the Phase 4 work queue, each entry referencing
  plato-library-review.md.
- **NEW FINDINGS beyond the review (see review doc §8 addendum):**
  1. **COMPILER BUG (critical): additive chains emit right-associatively** — `2t³ − 3t² + 1` becomes
     `2t³ − (3t² + 1)`. Shipped `SmoothStep`, `Hermite(+Derivative)`, `CatmullRom(+Derivative)` are
     wrong despite correct source. Must be fixed in Plato.CSharpWriter/compiler BEFORE the Phase 4 wave
     (it invalidates "fix the source, regen" for any mixed +/− chain).
  2. `ArcMinutes`/`ArcSeconds` factors inverted (`angles.plato:12-13`).
  3. `Time` has 4 generated throwing stubs (unfulfilled IMeasure obligations — lint check 1 class).
  4. `Area(Triangle2D)` returns negated signed area for CCW triangles.
- Language-ergonomics feedback for the author: `Number` is not `INumerical` so scalar
  SmoothStep/Bezier/etc. are unavailable on plain numbers; `IAdditive` lacks a zero element; library
  `AlmostEqual` (relative-only) is unusable near 0 — the harness added mixed abs+rel `LawEq`, which is
  the concrete input to the 1.4 tolerance policy.

---

## Phase 2 — Extension-method C# emitter, "V2" (compiler side, ~2–3 weeks)

The requested move from ~340 instance members per partial struct to extension members, done as a parallel
output so the SDK is never broken.

**Design decisions to make first (2.1):**
- [x] **Classic extension methods vs. C# 14 `extension` blocks — DECIDED: C# 14 extension members (spike
      2026-07-07, verdict GO).** Verified on this machine (SDK 10.0.301 installed): `net8.0` target +
      `<LangVersion>14</LangVersion>` compiles and runs on the .NET 8 runtime. Extension instance
      *properties* on structs work (preserves `v.Length` call syntax), as do extension methods, static
      extension members (`Vec3.UnitX`), generic extension blocks with interface constraints, and even
      extension *operators*. Confirmed limits: extension members cannot satisfy C# interfaces (CS0535 —
      those stay in the struct, as planned), and instance members **silently** shadow same-name
      extensions (no warning) — so the 2.5 API-diff gate is the safety net against unintended shadowing.
      Operational requirements: pin the build with `global.json` to the .NET 10 SDK (9.x is also
      installed); consumers on older compilers see extension properties as plain static methods (release-
      notes line). Fallback (classic extension methods, `v.Length()` everywhere) rejected as source-
      breaking unless the SDK pin proves unacceptable.
- [ ] **What must remain in the struct:** fields, constructors, operators, implicit conversions, and any
      member that *implements a C# interface* (extensions cannot satisfy interfaces). Everything else —
      the entire library-function fanout — becomes extensions.
- [ ] **Organization: one static class per Plato `library`** (`Vectors`, `Core`, `Transforms`, …), in one
      file each. This makes generated C# mirror plato-src 1:1 (discoverability, docs, blame), instead of
      today's one-file-per-type with provenance lost.
- [ ] **Namespace strategy.** Option A: same namespace (`Ara3D.Geometry`), types slim down in place.
      Option B: extensions in `Ara3D.Geometry.Library` so consumers opt in per-using. Recommend A for
      drop-in compatibility; the API snapshot (0.4) arbitrates what "compatible" means.

**Implementation:**
- [x] **2.2–2.4 DONE (2026-07-07).** `--csharp-style=extensions` implemented as a parallel plan/emit pass
      (`ExtensionStyleWriter.cs`; default path verified byte-identical via regen-diff). Output: one
      static class per Plato library with C# 14 `extension` blocks; no-arg functions emitted as extension
      *properties* (call syntax preserved). Kept in structs: scaffolding, interface obligations, stubs,
      operators/casts, statics, intrinsics, generic types' members, and any name shared with a kept
      member (silent-shadowing rule). Production V2: 1,503 moved members across 17 library files;
      Vector3 struct 160 → 42 members (−74%). Artifacts: `src/Plato.Generated.V2` (unreferenced shared
      project), `tests/Ara3D.SDK.ConformanceTests.V2` (links V1 test sources + manifest),
      `tools/regen-conformance-v2.ps1`. **Gates: V2 conformance 142/36/0 (identical test-case set), V2
      builds standalone under SDK 10.0.301 (net8.0, LangVersion 14), V1 regen-diff + V1 conformance +
      lint all still green.** Notable emitter work: 4-way bare-name re-qualification inside extension
      blocks (receiver/static/type/intrinsic), library↔type name collisions resolved by `*Library` class
      suffix, `Law_*` functions kept in structs (reflection contract).
- [x] **2.5 DONE (2026-07-07): GO.** Report: `ara3d-sdk\docs\api\v1-v2-api-diff.md`. Of 6,622 in-scope V1
      member lines: 5,122 unchanged, 1,486 moved (verified bijection into the 17 library classes),
      **0 emitter-attributable removals, 0 added**. The 14 "removed" entries are all handwritten members
      that re-merge at 2.6 (partial-struct members; plus a new 2.6 finding: the handwritten `Curves`
      static class collides with the generated `Curves` library class — same bucket as Sphere/Cylinder,
      fix by partial-merge or rename during the trial swap). Call-syntax spot-checks pass. Residual 2.6
      risks: the Curves collision and the .NET 10 SDK / LangVersion 14 pin for consumers using instance
      syntax on moved members.
- ~~**2.6** SDK trial swap~~ / ~~**2.7** Adopt~~ — **CANCELLED (2026-07-08)**: no switchover; a new
      library will be built from scratch later using the V2 shape as its reference. The Sphere/Cylinder/
      Curves collisions and the full intrinsic scalar erasure move to the new-library effort.

**Why this phase is second, not later:** the optimizer (P3), double target (P5), and eventually GLSL all
write through the emitter. Doing the emitter reorganization first means every subsequent workstream is
built once, on the final shape, instead of twice.

**Exit criteria:** SDK builds and all tests pass on V2; V1 retired on a schedule.

---

### Phase 2 revision (2026-07-07, decided with the author after reviewing V2 output)

1. **SUPERSEDED: C# 14 extension blocks → classic extension methods.** The 2.1 spike verdict stands
   technically, but the product decision is to drop the C# 14 dependency ("kooky syntax", .NET 10 SDK
   pin) and gain symmetry with the TypeScript emitter: no-arg Plato library functions emit as extension
   METHODS (`v.Length()`), not extension properties. Struct-kept members (fields, interface obligations,
   scaffolding) remain properties. V2 will be regenerated in this style; the emitter's own expression
   writer must emit `()` at call sites of moved members; API diff redone afterward.
2. **Repo restructuring (do before the emitter revision):** `Plato.Intrinsics` source of truth moves into
   the Plato repository (it is the C# target's runtime companion, not an SDK asset), along with the
   conformance test projects (then fully self-contained in the Plato repo) and `Plato.Generated.V2`
   (a compiler golden artifact). ara3d-sdk keeps a *synced copy* of Plato.Intrinsics + Plato.Generated —
   maintained and diff-gated by the regen script — so the SDK still builds standalone without the Plato
   toolchain (same pattern as the generated code today).
3. **Scalar erasure as an emitter option (`--scalar=float|wrapper`)** — retire the `Number`/`Integer`/
   `Boolean` wrappers from generated signatures in favor of native primitives; library functions on
   scalars become extension methods on the primitive. Lands AFTER the extension-methods revision;
   `Angle` remains a real struct (unit safety is its purpose).
4. **Double precision (P5) explicitly deferred** until after the above; namespace decision recorded:
   same type names under `Ara3D.Geometry.Double`, separate assembly + double intrinsics, `--scalar=double`.
5. Fix V2 emitter indentation inconsistencies (seen in `_Angle.g.cs`) as part of the revision.

**Items 1 + 5 DONE (2026-07-08).** `--csharp-style=extensions` now emits CLASSIC extension methods
(one plain static class per library, `public static R Name(this T recv, ...)`); no C# 14 anywhere,
`<LangVersion>14</LangVersion>` removed from the V2 conformance csproj, golden V2 compiles standalone
on net8.0 with the default LangVersion. No-arg moved functions are METHODS (`v.Magnitude()`); the
writer injects `()` at every call site by a globally consistent name partition (computed in
`CSharpWriter.BuildExtensionPlans` before anything is written): a no-arg name that is a property
ANYWHERE (kept struct member, interface declaration, or the pinned handwritten-intrinsics list
AlmostZero/Pow2/Pow3/ReciprocalSquareRootEstimate) is demoted back into its structs on every type,
so each name is uniformly method or property. Result: 1,284 moved members (424 no-arg → methods)
across 17 library files; ~220 members that were extension properties in the C# 14 variant returned
to their structs as properties (mixed syntax accepted per the decision). Statics never moved (no
constructor-like fallout). Indent fix: `CodeBuilder.Write` leaves `AtNewLine` false after nested-
builder fragments, so the next member lost its indentation (V1 quirk, e.g. `_Angle.g.cs` Zero/One);
fixed via `WriteWithLineStateSync` at all nested-builder junctions in EXTENSIONS MODE ONLY — the V1
fix is deferred to adoption because default-mode byte identity is a hard gate (verified green).
Gates: V2 conformance 142/36/0; V2+`--optimize` 142/36/0; V1 regen-diff byte-identical
(163/0/0 + intrinsics sync); V1 conformance 142/36/0 (and V1+opt 142/36/0); lint OK (246 findings,
unchanged); golden V2 standalone compile 0 errors. API diff (2.5 redo) still pending, as planned.

**Item 3 DONE (2026-07-08): `--scalar=wrapper|float` landed.** Default `wrapper` is byte-identical
(regen-diff green); `float` requires `--csharp-style=extensions` and is rejected otherwise with a
clear error (`--csharp-style=default` + erasure stays unsupported by design). Under `--scalar=float`
every generated signature/field/local/generic argument uses the native primitives (`IArray<Number>`
-> `IReadOnlyList<float>`), literals lose their casts (`((Number)0.5)` -> `0.5f`), and the five
scalar per-type files become extension-method classes over the primitives (Plato bodies emitted in
full; intrinsics as forwarders `((Number)x).Sqrt`; operators/indexers/interface-impls dropped with
`// scalar-erasure drop` comments). Emitter mechanics: bodies normalized to "float-land"
(scalar-parameter casts, scalar-returning call wraps, receiver-aware `()` via a scalar-overload
table + conservative expression-primitive analysis in `CSharpFunctionBodyWriter`), wrapper-receiver
bridge twins, re-homed wrapper-sourced implicit broadcast operators, a 3-property
`partial struct Number` shim (AlmostZero/Pow2/Pow3 - handwritten property-syntax users), hardwired
forwarders for compiler-invisible handwritten members (Cubic/Linear/Quadratic/
ReciprocalSquareRootEstimate/Range/MakeArray2D). Deliberate documented seams (`golden/README.md`):
concept interfaces and struct-kept obligation members of NON-scalar types stay wrapper-typed until
the new-library intrinsic erasure. Fourth conformance suite
`conformance\Ara3D.SDK.ConformanceTests.Scalar` + `tools
egen-conformance-scalar.ps1` (supports
`-Optimize`); the shared law runner gained a static-law discovery pass (extension methods over
primitives, receiver primitive mapped back to the wrapper name) with key-level dedupe, so the
V1/V2/Opt case lists are unchanged. V1-vs-Scalar law/witness case lists verified IDENTICAL (175
keys; all 28 scalar law-by-type pairs present, zero losses; same 36-entry manifest, no key-form
changes). Gates: regen-diff byte-identity + intrinsics sync green; V1/V2/Opt/Scalar conformance all
142/36/0; `--optimize --scalar=float` compiles and passes 142/36/0; erased golden compiles
standalone (net8.0, default LangVersion, 0 errors); lint unchanged.
`golden\Plato.Generated.V2` regenerated in the adoption shape
(`--csharp-style=extensions --scalar=float`); signature-shape summary in `golden\README.md`.

**Direction change (2026-07-08, author):** the 2.6/2.7 switchover is CANCELLED — `Ara3D.Geometry` keeps
V1 indefinitely; a new library will be built from scratch later, with the V2 extension-method +
scalar-erased output serving as its reference shape (and as groundwork for other backends). The
refreshed API diff is therefore unnecessary. Remaining execution order, per the author:
**scalar erasure finishes → Phase 6 affine (re-run) → Phase 7 GLSL PoC → Phase 4 bug wave
(now authorized in that order: compiler associativity fix first, then the 36-entry manifest burn-down).**

## Phase 3 — Optimizer, stage 1 (compiler side, ongoing)

Scope: the mechanical, high-payoff transforms from the review discussion. Each transform is a flag,
adopted only when the differential suite (P1) passes and the benchmark suite (0.5) shows the win.

Ordered by expected payoff:
- [x] **3.1 DONE (2026-07-07): component-op unrolling**, behind `--optimize` (works with both
      `--csharp-style` modes; off-flag output byte-identical, regen-diff green). Emission-time
      specialization in `ComponentUnroller.cs` (beta-reduced field-wise rewrites of MapComponents/
      ZipComponents/All/Any/Reduce; all-or-nothing per body; 398 → 63 surviving HOF call sites, 57/163
      files changed). Gates: Opt conformance 142/36/0 exact (bugs preserved bit-for-bit — verified the
      36 known failures still fail); V2+optimize also 142/36/0. Perf (1M-op probe): Lerp 57.8→1.8 ns
      (31.5×), ClampZeroOne 64.7→5.9 ns (11×), SumComponents 23.1→4.7 ns (5×), all allocations → 0 B.
      Artifacts: `tests/Ara3D.SDK.ConformanceTests.Opt`, `tools/regen-conformance-opt.ps1`. Adoption of
      optimized output into production = same gate as the V2 swap (parallel artifact until approved).
      **Fixed-arity body fan-out DONE 2026-07-13**: `TirComponentUnroller` also accepts delegate-typed
      HOF references (invoked per component via `TirInvoke`) and unrolls the generic ArrayLibrary
      bodies themselves (`CreateFromComponents(Self, Map/Zip/Reverse/Components(...))` → direct
      constructor call; `All`/`Any` → `&&`/`||` chains), so `Integer2.ZipComponents(a, b, f)` emits
      `new Integer2(f.Invoke(a.A, b.A), f.Invoke(a.B, b.B))` instead of a Components()+array+loop
      round-trip; inlined lambda call sites (e.g. `Angle.Lerp`) collapse to one-liners too. Goldens:
      32 Optimized files refreshed, Unoptimized byte-identical; `ComponentUnrollerTests` pin the
      shapes; conformance 205/205.
      **Fixed-size-array unroll + cast cleanup DONE 2026-07-13** (dev loop: the new self-contained
      `plato-src-small` corpus + `Small/` projects, see `submodules/Plato/Small/README.md`):
      (1) the component unroller accepts cheap-projection vector sources (`x.A`, not only bare
      params) → `Line3D.Eval` unrolls to `new Point3D(...)`; (2) the inliner inlines array-LITERAL
      bodies (`Corners`) and the unroller unrolls Map/Zip/Reduce/All/Any/Reverse over a fixed-size
      `TirArray` → `Bounds3D.Deform` drops its loop to
      `MakeArray<Point3D>(f.Invoke(c0)..f.Invoke(c7)).Bounds()` (TirArray now prints a typed
      `MakeArray<T>` so the per-element Vector3→Point3D coercion survives); (3) redundant
      `coerce<float→float>` casts on provably-primitive inners are dropped, clearing the `((float)x)`
      noise from every scalar body. Both goldens refreshed (cast cleanup also touches the unoptimized
      recipe); PlatoTests 116/116; conformance 205/205. Deferred: broad `_varN = p` copy-let removal
      (needs the shared lambda-capture hoist changed, which the out-of-scope legacy TS/Rust writers
      still mirror byte-for-byte).
- [~] **3.2 Beta reduction.** Inline literal lambdas into known callee bodies at the Plato level
      (purity makes it sound). Surviving HOFs on `IArray` get struct-functor emission
      (`TFunc : struct, IFunc<T,R>`) instead of `Func<T,R>` delegates.
      **Delegate inlining + β-reduction DONE** (V2/`--no-properties` recipe): `TirInliner` +
      `TirRewrite`; the `Transform → Deform(lambda)` family collapses to inline loops. **M0+M1 DONE
      2026-07-12** (plan `submodules/Plato/docs/plato-optimizer-completion-plan-2026-07-12.md`):
      `--inline-report` refusal instrumentation (`InlineReport`) + fast in-proc `InlinerTests`;
      the tuple-returning body family now inlines via a position-independent `new T(elem...)`
      constructor rewrite (`TirConstructorCall`), replacing the deleted tail-position tuple lift —
      mesh + tuple-helper Transforms (UnitCircle→Point2D, curve Evals, TriangleMesh/QuadMesh/
      LineMesh) collapse under the scalar recipe. V2Runtime 204/204; golden refreshed. Remaining
      (plan M2-M5): measurement, loop fusion vs statement-body inlining, gate relaxations
      (nested-lambda / multi-use / insideLambda), non-erased runtime port.
- [ ] **3.3 Constant folding + strength reduction.** `Constants.Pi` → literal; `3.0.Sqrt.Half` folded;
      `x.Pow(2.0)` → `x * x`.
- [ ] **3.4 CSE + `SinCos` fusion.** Purity makes CSE side-effect-free; pair `t.Cos`/`t.Sin` occurrences
      into one `MathF.SinCos` call (the curve library is full of them).
- [ ] **3.5 `IArray` devirtualization + fusion.** Concrete-typed loop emission where the container type is
      known; `Map∘Map` fusion; `MapRange` → pre-sized array fill. (Full loop-into-buffer lowering waits
      for P6's `Buffer<T>`.)
- [ ] **3.6 Emitted-code hygiene.** `readonly partial struct`, `in` params for >16-byte types, tuple
      literals lowered to constructors, verify `Number` erases to `float` in hot bodies (inspect JIT asm
      via the benchmark project, don't assume).

**Exit criteria per transform:** differential suite identical (or within declared ULP tolerance, recorded
per transform); benchmark delta committed to the doc.

---

## Phase 4 — Bug-fix wave (first intentional library change; needs explicit go-ahead)

Only after P1 exists, so every fix flips a `KnownFailure` to a passing law/witness test.

- [ ] **4.1** For each manifest entry: un-quarantine the test (now failing), apply the one-line fix from
      `../reports/plato-library-review.md §1`, regenerate, test passes. One commit per fix, review-doc reference in
      the message.
- [ ] **4.2** The behavioral fixes that need a decision first (flagged in the review): TRS/Pose
      composition order; `Angle` implicit-conversion removal (breaking); `Magnitude` vs `Length`
      unification. Each gets a short ADR in `docs/` before the change.
- [ ] **4.3** Linter warnings → errors in CI once the manifest is empty.

---

## Phase 5 — Double-precision C# target (compiler + SDK, after P2)

- [ ] **5.1** Parameterize the emitter on scalar type: `Number → double`, `MathF → Math`, literal suffixes.
      Emit to `Plato.Generated.Double` under namespace `Ara3D.Geometry.Double` (or `Ara3D.GeometryD`).
- [ ] **5.2** `Plato.Intrinsics.Double`: the real work. System.Numerics vectors are float-only, so
      Vector2/3/4, Matrix4x4, Quaternion need double implementations (plain fields first;
      `Vector256<double>` SIMD later). The existing intrinsics seam is exactly the right isolation —
      same file-per-type layout, same API.
- [ ] **5.3** Conformance: run the P1 suite against the double build with its own tolerances;
      **cross-precision differential** — identical seeded inputs through float and double, assert
      agreement within float tolerance. The double build becomes the oracle for the float build (and
      catches float-only issues like the truncated `Pi` constant).
- [ ] **5.4** Consumer story: document when to use which (exact predicates, large-coordinate BIM models →
      double; render-adjacent → float).

---

## Phase 6 — Affine types: `unique type List<T>` / `Buffer<T>` (compiler + intrinsics)

Per the syntax proposal: **one keyword, declaration-site only** (`unique type`), no use-site or
parameter annotations, method effects (observe/mutate/consume) in the compiler's intrinsic table.
Runtime-checked first; the static affine pass rides the future type-checker workstream.

- [x] **6.1 Grammar:** `unique` modifier on `type` in the Parakeet grammar; parse + AST only. Hard-reject
      `unique` on non-intrinsic types for now.
- [x] **6.2 Intrinsic implementations** in `Plato.Intrinsics`: `List<T>` (grow, `Add`, `AddRange`, `Set`,
      `Count`, `At`, `Freeze`) and `Buffer<T>` (fixed-size, write-by-index, `Freeze`), both with a
      `frozen` flag; `Freeze` hands the backing array to an `IArray<T>` wrapper **without copying** and
      invalidates the builder (`ImmutableArray<T>.Builder.MoveToImmutable` semantics). Any post-freeze use
      throws.
- [x] **6.3 Declarations** in a new additive file `plato-src/unique.plato` (`unique type List<T> { }`,
      `unique type Buffer<T> { }`) + intrinsic signatures with effect classification
      (observe: `Count`, `At`; mutate: `Add`, `Set`, returns the builder; consume: `Freeze`).
- [x] **6.4 Validation ports (new additive files, not rewrites):** 2–3 append-heavy algorithms written
      fresh in Plato — ear-clipping triangulation, polyline→mesh extrusion, `Filter` for `IArray`.
      These prove the API shape before any static rule is frozen. They must *not* replace the existing
      C# implementations yet.
- [x] **6.5 Documented conventions** (the affine rules as prose + linter warnings where cheap: builder
      as field type, builder in lambda capture — both detectable structurally today).
- [ ] **6.6 (Later, with the type checker):** the occurrence-counting affine pass; runtime checks remain
      as backstop.

**Exit criteria:** the three ported algorithms pass property tests against their C# counterparts
(differential, seeded inputs); zero-copy freeze verified by benchmark.

**Runtime slice DONE (2026-07-09):** declaration-site `unique type` support, runtime-checked
`List<T>`/`Buffer<T>` intrinsics, additive `plato-src/unique.plato`, and affine validation proofs
landed. The conformance validation uses fresh Plato proof slices for `Filter`, convex-quad
ear clipping, closed-triangle extrusion faces, fixed-size extrusion vertices, and `AddRange`/`Set`
witnesses; it does not replace the existing C# algorithms yet. Gates: regen-diff + intrinsics sync
green; V1/V2/Opt/Scalar conformance all pass with 53 witnesses and 185 total tests; full
`check-all.ps1` passes. The static occurrence-counting affine pass and benchmark-backed zero-copy
proof remain deferred with 6.6.

---

## Phase 7 — GLSL proof of concept (compiler side, independent)

Thin vertical slice; explicitly a PoC, not a backend.

- [ ] **7.1 Content:** a small additive `plato-src/sdf3d.plato` — ~10 IQ primitives (sphere, box,
      round-box, torus, capsule, cylinder), booleans + smooth-min, normal-from-gradient. (First slice of
      the roadmap-ideas SDF catalog; array-free by construction, so it's GLSL-compatible.)
- [ ] **7.2 Writer:** `Plato.GlslWriter` handling the no-`IArray`, no-`String` subset: types → `vec2/vec3/
      mat4/float`, libraries → free functions, no-arg functions → functions (GLSL has no properties),
      tuples → constructors. Reject (with a clear diagnostic) anything outside the subset.
- [ ] **7.3 Demo:** a WebGL/ShaderToy-style raymarcher whose scene SDF is compiled from the same
      `sdf3d.plato` that also runs in C# in Studio — *the* one-source-many-targets demo.
- [ ] **7.4 Conformance:** evaluate the SDF at seeded sample points on CPU (C#) and in the shader
      (render to texture / transform feedback), compare within tolerance. This extends the P1 suite to
      the third target and proves the conformance architecture.

---

## Suggested first two weeks (concrete, in order)

1. Tag the golden baseline; write `tools/regen-plato` script; wire regen-diff CI (P0.1–0.3).
2. Fix the one drift it reveals (`angles.plato:4` to match shipped behavior — behavior-preserving).
3. Commit API snapshot + benchmark project with baseline numbers (P0.4–0.5).
4. Compiler: `--emit-tests` NUnit adapter for `*Tests` libraries returning Boolean (P1.1).
5. Write `laws.plato` + `witness.plato` (additive); run; populate the known-failures manifest (P1.2–1.5).
6. Spike the C# 14 `extension`-block question in a scratch project against the SDK build — it's the
   go/no-go for P2's shape, and the answer is needed before any emitter code is written (P2.1).
7. Start the `lint` command with the two cheapest checks (unused fields, duplicate signatures) (P1.6).

## Explicitly deferred

- TypeScript and Rust writers. **DONE 2026-07-10 (TIR retarget):** both writers now emit bodies from
  the Typed IR (`TirTypeScriptBodyWriter` / `TirRustBodyWriter`, `UseTir` on by default, CLI
  `--no-tir` opts out), byte-identical to the legacy path (flag-on/off differential tests).
- The full type checker (top roadmap item, separate plan; P1's linter and P6's affine pass are designed
  to fold into it). **STATUS 2026-07-10 (2nd update): the TIR is the production emit path for EVERY
  style and writer** — default/extension/scalar/optimize C# (scalar+optimize combined stays legacy)
  plus TS/Rust — each proven byte-identical by a full-library flag-on/off differential
  (`PlatoTests/{Extension,TypeScript,Rust,Optimize,Scalar}EmitFlagOnTests`). The legacy
  `CSharpFunctionBodyWriter` heuristics survive only as the `--no-tir` fallback and the differential
  reference; deleting them is now unblocked. Handwritten intrinsics are DECLARED to the checker
  (`plato-src/intrinsics.plato`: Number MinValue/MaxValue/RSRE/Linear/Quadratic/Cubic, IOrderable
  equality, the Number→Angle cast): stdlib diagnostics 78 → 68 of 823 (CHK201 22 → 13;
  `CheckerDiagnosticsSummaryTests` prints the burn-down worklist). Tightening the permissive solver
  rules (Self-unifies-anything, syntactic emission) remains deferred until the count is lower.
  Handoff: `submodules/Plato/docs/type-checker-handoff.md`.
- Library content expansion (`../reports/plato-library-roadmap-ideas.md`) — resumes after P4, on a tested base.
  **First slice DONE 2026-07-10:** `library Solids` implements the `IProceduralSurface` obligations
  (Eval/ClosedX/ClosedY) for all 11 `ISolid` types; the `_NPrism`-class NotImplementedException
  stubs are gone (numerically probe-verified).
- **Golden V2 is now the optimized adoption shape (2026-07-10, author decision):**
  `golden/Plato.Generated.V2` is generated with `--csharp-style=extensions --scalar=float
  --optimize --optimize-arrays` BY DEFAULT (`tools/regen-golden-v2.ps1`, gated in `check-all.ps1`;
  the Scalar conformance suite runs the same flags). This required enabling the TIR path for the
  scalar+optimize combination (marker nodes now carry `ScalarComponentPrim`, so the scalar
  method-call/cast decisions survive unrolling) — the legacy writer no longer serves ANY
  style combination by default.
- Optimizer stage 2 (loop-into-buffer lowering, functional-in-place) — P6 done, unblocked; plan:
  `submodules/Plato/docs/optimizer-stage2-plan.md`. **Increment 1 DONE 2026-07-10**
  (`--optimize-arrays`, `TirArrayMaterializer`): Map/MapRange results stored into constructed
  structs (Deform/To3D, 10 stdlib sites) or multi-referenced lets are lowered to eager
  `MapEager`/`MapRangeEager` array fills. Opt conformance now runs both optimizer flags (204/204);
  `ArrayMaterializerTests` pins the transform's footprint; probe 1.37× on 4-pass consumption
  (grows with callback depth). Remaining increments: Map∘Map fusion, struct functors,
  consumer-side loop fusion.
- Sum types with exhaustive matching (the partiality gap). **DONE 2026-07-27 (plato-232):**
  `type X = Case(...fields) | Case | ...;` declarations and exhaustive `match` expressions
  (positional binders, no default arm), checked with CHK300–CHK306 and CHK320, lowered during
  elaboration to a tag-conditional chain over existing TIR nodes (no new node), and emitted as a
  C#-only tagged `readonly partial struct` (int `Kind` + flattened `Case_Field` fields + per-case
  factories + structural equality). Generics restricted in v1 (CHK306). Shipped across three
  Plato commits (front-end AST `1d3ed84`, checking/lowering/emission `507de64`, wave-3 flagship
  migrations) plus the parakeet grammar bump; PlatoTests 126 → 142. Wave-3 migrated five
  flagship `plato-src-v3` kind-pattern types to real sums (`PathSegment2D`, `Paint`,
  `MaskSource2D`, `ScalarFieldNode2D/3D`, `WindowFunction`); the ~100 pure-enum `XxxKind` types
  remain as a follow-up sweep. Design: `submodules/Plato/docs/plato-sum-types-design-2026-07-27.md`;
  survey: `../reports/plato-sum-types-v3-survey.md`. Deferred: GLSL/TS/Rust emission, nested patterns, guards,
  default arm, recursive sums, bare constructors, generic sums.
