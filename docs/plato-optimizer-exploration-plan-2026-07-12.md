# Optimizer exploration plan — hypotheses, benchmarks, decisions (2026-07-12)

> Plan 2 of 2 (companion: `plato-consolidation-plan-2026-07-12.md`). This is a MEASUREMENT-driven
> plan: every exploration is a falsifiable hypothesis with an experiment and a decision rule.
> Nothing gets built into the emitter until its hand-written prototype earns its keep on the
> benchmark suite. Prior data: `optimizer-smoke/Bench` (M2) and the `--optimize-arrays` probe
> (`docs/optimizer-stage2-plan.md`, 1.37×).

## 0. What we have already measured (facts, not assumptions)

| # | Finding | Source |
|---|---|---|
| F1 | Delegate removal on scalar paths is ~neutral (`Triangle3D.Transform` ×50M: ≈1.0×). The JIT devirtualizes a monomorphic `Func`. Inlining's value there is *enabling* + code shape, not raw speed. | Bench, 2026-07-12 |
| F2 | Eager materialization **loses** on single-consumption stored arrays (`TriangleMesh3D.Transform` 200k×200: 0.77×) — the lazy `Map` defers the allocation the eager loop pays up front. | Bench |
| F3 | Eager materialization **wins** on multi-consumption (4-pass over a stored 1M-point map: 1.37×). | stage2 probe |
| F4 | The stdlib's fuseable (transient) arrays are small (`Bounds3D` corners = 8); its big arrays are *stored* in results (meshes), so their allocation is irreducible. | golden inspection |
| F5 | `Vector3/Vector4/Matrix4x4/Quaternion` intrinsics already wrap `System.Numerics` types — per-element SIMD exists today; the gap is *batch* (array-level) SIMD. | `Plato.Intrinsics.V2` |

F2+F3 together say the materialization decision is **consumption-dependent** — a static policy is
wrong in one direction or the other. That tension shapes several hypotheses below.

## 1. Hypotheses (falsifiable, ranked by expected payoff)

- **H1 — Element access dominates array paths.** Generated struct fields hold
  `IReadOnlyList<Point3D>`; every element read is an interface (virtual) dispatch. Replacing
  interface-typed storage/iteration with concrete arrays/spans in hot loops beats everything else
  on the list. *(Expected: large. This is the cheapest big win and gates H4's usefulness.)*
- **H2 — Fusion pays only at cache-relevant scale.** Multi-pass pipelines (`Map→Map→Reduce`) over
  arrays ≫ L2 re-stream memory per pass; a fused single pass is bounded by one stream. At 8–1k
  elements it's noise (M2 confirmed); at 256k–4M it MAY be 1.3×+. Author's "maybe" — test it.
- **H3 — Hidden allocations poison scalar paths.** e.g. `Number.Components() =>
  Intrinsics.MakeArray<float>(Value)` allocates an array to view one float; any inlined path that
  ends in `Components()` turns a register op into a GC object. Auditing and replacing/bypassing
  allocating intrinsics is a real win wherever the optimizer currently *exposes* them.
- **H4 — Batch SIMD needs layout, not cleverness.** Because of F5, scalar-at-a-time code already
  reaches SSE via System.Numerics. Array-level speedups (4–8 lanes) require contiguous primitive
  layout (SoA or `Vector3[]` reinterpreted as `float`/`Vector128` spans) — blocked by H1's
  interface storage today. SIMD is therefore *downstream of H1*, not an independent track.
- **H5 — Codegen hygiene is cheap, small, and worth batching.** `[module: SkipLocalsInit]`,
  `readonly partial struct`, `in` parameters for >16-byte structs, and an *audit* (not blanket
  application) of `AggressiveInlining` — each plausibly 0–5%; together maybe measurable; all
  low-risk emitter/csproj switches.
- **H6 — Consumption-aware materialization beats any static policy.** The optimizer should emit
  eager only when the plan can see multi-consumption or a size-N construction (the current
  materializer's positions), and *stay lazy* for single-pass stored results — F2's regression case
  should be detected and avoided, not accepted.

## 2. Prerequisite — the benchmark suite (build FIRST; nothing lands without it)

`Plato.Benchmarks/` on **BenchmarkDotNet** (`MemoryDiagnoser` always; `DisassemblyDiagnoser` for
JIT-asm inspection — roadmap 3.6 says *inspect, don't assume*). The extern-alias A/B trick from
`optimizer-smoke/Bench` carries over (Optimized vs Unoptimized golden in one process).

Matrix (kept deliberately small; ~20 benchmarks, not 200):

| Dimension | Values |
|---|---|
| Operation | `Triangle3D.Transform` (scalar) · `TriangleMesh3D.Transform` (stored array) · `Bounds3D.Transform` (transient array) · curve sampling (`Circle.Eval` sweep) · `Magnitude`-sum reduction (`Map→Reduce`) |
| Size | 8 · 1k · 64k · 1M |
| Consumption | single-pass · 4-pass |
| Variant | Unoptimized golden · Optimized golden · hand-written "ideal" (the ceiling) |

The **hand-written ideal** column is the key methodological addition: for each benchmark, a
straight C# loop over `float`/`Vector3[]` arrays written the way a performance-minded human would.
It bounds what any optimizer work can possibly earn, and immediately shows *where the gap is*
(dispatch? allocation? layout?) via the diagnosers.

Results are committed as a markdown table in `docs/plato-benchmark-results.md` with machine/JIT
noted; every subsequent optimizer PR cites its row deltas.

## 3. Explorations

Each = hypothesis → experiment → decision rule. Prototype by HAND first; build compiler machinery
only for confirmed wins.

### E1 — Storage devirtualization (H1)
Experiment: benchmark `IReadOnlyList<Point3D>` vs `Point3D[]` vs `ReadOnlySpan<Point3D>` iteration
at 64k/1M; then a hand-modified golden where mesh fields are concrete arrays.
Decision: if ≥1.5× on array paths (expected), plan the type-strategy change — generated struct
fields and combinator signatures use a concrete array/`Buffer` type (ties into roadmap P6);
`IReadOnlyList` remains an accepted *input* surface, not the storage.

### E2 — Fusion at scale (H2)
Experiment: hand-write fused vs staged `Map→Map→Reduce` and `Map→Bounds` at 1k/64k/256k/1M/4M,
single pass, with `MemoryDiagnoser`. Explicitly measure the cache story (times per element vs
size).
Decision: fused ≥1.25× at any realistic size → build `TirLoopFuser` at L10 for
recognized-combinator chains (`loop:X -> _lp; loop:Y(_lp)` pairs, which the lowerer already makes
visible). Below that → close H2 permanently with the data in the doc. Note: fusion into an opaque
consumer (`.Bounds()`) additionally needs statement-body inlining — only justified if the
recognized-chain fusion already proved the win.

### E3 — Allocating-intrinsics audit (H3)
Experiment: grep+review every intrinsic that allocates (`MakeArray`, `Components()`,
`ToIArray`, LINQ-ish helpers); count how often optimized-golden hot paths reach one (the
`--inline-report` + golden inspection make this mechanical). Micro-benchmark the top three.
Decision: per-intrinsic — replace with non-allocating equivalents (fixed-arity overloads, spans,
direct field fan-out — the component unroller already does this at *call sites*; the fix is making
sure inlining exposes those sites rather than the boxed `Components()` fallback).

### E4 — Codegen hygiene batch (H5)
Experiment: one branch flipping all of: `SkipLocalsInit` (csproj/module attribute on generated
projects), `readonly` on generated structs, `in` params for >16B parameter types, and an
`AggressiveInlining` audit (it is currently on *everything*; measure whether pruning it off large
bodies helps JIT time/code size). Run the full suite; inspect asm for 2–3 hot bodies.
Decision: keep what is ≥ neutral and simplifies; document each as a one-line finding.

### E5 — SIMD pathways (H4, after E1)
Experiment A (cheap, now): verify via `DisassemblyDiagnoser` that inlined chains like
`new Vector3(...).Transform(m)` actually JIT to SN SIMD instructions post-collapse (this validates
the inliner's value in a way wall-time didn't in F1).
Experiment B (after E1): batch transform over `Vector3[]`/SoA `float[]` with explicit
`Vector128/256` vs scalar loop vs `System.Numerics` loop.
Decision: if B shows ≥2× at 64k+, add a *layout-aware* lowering target (SoA buffers or
span-reinterpretation) to the roadmap as its own increment; do NOT attempt auto-vectorization
inside the current emitter before the storage story (E1) lands.

### E6 — Annotated output (author request; independent, cheap)
`--annotate`: the emitter writes `// plato:` comments at transform sites — `inlined from
Deform@Bounds3D (β-reduced f; tuple→ctor Triangle3D)`, `loop lowered from Map`, `eager: stored
into ctor arg`. Emit only where a transform *fired* (principle: notes where a human would ask
"why does this look like this?"). Off by default; `regen-generated.ps1 -Annotate` produces an
annotated variant for review, the committed golden stays clean (or the author may choose to commit
annotated goldens — decide after seeing one). Implementation is small: the passes already know
what they did; thread a note sink through `RunOptimizerPasses` beside the existing `--dump-tir`.

## 4. Sequencing

1. **Benchmark suite** (§2) — everything depends on it. (~1 day)
2. **E6 annotations** — cheap, improves every subsequent golden review. (~½ day)
3. **E1 storage** — the expected big win and the gate for E5-B. (~1 day to measure; the
   type-strategy change it may trigger is its own planned increment.)
4. **E2 fusion + E3 intrinsics audit** — in either order, both prototype-first. (~1 day each)
5. **E4 hygiene batch** — anytime; good gap-filler. (~½ day)
6. **E5 SIMD** — A anytime; B only after E1's decision.

## 5. Assumptions to keep visible (and revisit)

- Benchmarks on one dev machine are directional, not publishable; deltas <10% are noise unless
  BenchmarkDotNet says otherwise.
- The conformance suites remain the semantic oracle for every experiment that changes output —
  optimization never trades correctness, and the pure language means the oracle is cheap.
- The consolidation plan (Plan 1) is a *multiplier* for this plan: post-C4, emitter experiments
  are snapshot-tested and single-recipe, so each exploration here gets cheaper to land. Where
  effort must be traded, C0–C4 first is the better investment.
