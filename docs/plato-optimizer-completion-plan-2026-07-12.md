# Optimizer completion plan — delegate inlining and beyond (2026-07-12)

> Handoff plan for the next agent. Prior art: `docs/plato-inlining-beta-reduction-plan-2026-07-12.md`
> (the landed increment and its history), `docs/optimizer-stage2-plan.md` (array materializer),
> `docs/plato-emitter-phases.md` (pipeline layers). Read those before starting; this doc is the
> forward plan only.

## 0. Where things stand (verified 2026-07-12)

The optimizer pipeline is: **inline (L7) → component-unroll (L8) → array-materialize (L9) →
loop-lower (L10)**, run per function body (`CSharpWriter.RunOptimizerPasses`). All landed and
pushed on `plato-generated-projects`:

- `TirRewrite` — shared post-order rewrite + `Substitute` + `TryBetaReduce` (+ 12 unit tests).
- Delegate-parameter inlining + β-reduction in `TirInliner` (38f59c7): a lambda argument
  substitutes into a `Function{N}`-typed parameter when every use is consuming; applications
  β-reduce away; **gated on `writer.NoProperties`** (the V2 recipe) so property-ful V1 recipes are
  byte-for-byte unaffected.
- §5c receiver resolution: `TrustedTypeName` prefers a CONCRETE param-0 receiver type (node's own
  monomorphized type) over an interface-typed signature — this is what makes `Bounds3D.Transform`
  etc. actually fire.
- Post-substitution alpha-rename over the final inlined tree (CS0136 shadowing).
- All-extension-methods runtime, scalar-erased half: `Number`/`Integer`/`Boolean` intrinsics moved
  to `<Type>Intrinsics` extension classes in `Plato.Intrinsics.V2`.
- **`Ara3D.SDK.ConformanceTests.V2Runtime`** — the semantic gate for all of this: V2 intrinsics +
  full V2 recipe, 204/204. Regen: `tools/regen-conformance-v2runtime.ps1 -Test`.
- `Plato.Generated.Optimized` golden refreshed (86c5a58); the diff *is* the feature —
  `Point3D.Transform` is now `this.Vector3().Transform(t.Matrix)`, `Bounds3D.Transform` a bare loop.

**What does NOT collapse yet** (the remaining work):

| Family | Example | Blocker |
|---|---|---|
| Tuple-returning Deforms | `Triangle3D`, `Quad*`, `Line*`, `Ray*`, all meshes | §5b: tuple tail-lift is gated OFF under `ScalarErase`, and the only recipe running `--inline` has `--scalar=float` — so `InlineTails` is currently **dead code in production** |
| Nested-lambda Deforms | `Lines3D/Triangles3D/Quads3D`: `Map(xs, l => Deform(l, f))` | `f` used under a lambda → refused |
| Multi-use lambda args | any callee applying `f` from two non-tuple positions | single-use v1 gate |
| Call sites inside lambdas | `xs.Map(p => b.Deform(...))` | `insideLambda` cheap-args-only rule |
| Loop-bodied callees | `xs.Map(f).Bounds()` post-lowering | inliner is expression-only; no statement splicing |
| Non-erased intrinsics as extensions | `Angle/Vector*/Matrix*/Quaternion/Plane` | emitter emits a colliding forwarder extension (task #7; off inlining critical path) |

## 1. Design principles that govern everything below

1. **Purity makes substitution sound.** The only real constraints are evaluation count (never run
   an expression more times than the original call would) and C# target-typing (delegates, tuples).
2. **Position-suppressed coercions are the enemy.** The emitter renders `TirCoerce` by *omitting*
   it and letting C# convert at the position boundary. Every position-dependent hack (the
   `InlineTails` tail-spine walk, the `ScalarErase` gate) exists because of this. **Prefer
   rewriting to position-independent forms (explicit constructor calls) over tracking positions.**
   This principle drives M1.
3. **The monomorphized node type is the truth; signatures can lie.** (§5c lesson: a receiver
   declared `IDeformable3D` is solved to `Bounds3D` on the node.) When resolving specializations,
   trust concrete node types over declared/zonked signature types.
4. **V2-first.** New optimization behavior is gated on `NoProperties`; V1 recipes get the frozen
   baseline. Never spend effort making new optimizations work under the property-ful surface.
5. **Byte-identity gates are the regression oracle.** `regen-plato.ps1` (off-flag), Scalar V1
   204/204 (gate-off behavior), V2Runtime 204/204 (gate-on semantics), `regen-generated.ps1`
   (golden diff = intended change review). **A landed emitter change must refresh the golden in the
   same change** — 38f59c7 initially forgot this; 86c5a58 fixed it.

## 2. Milestones

### M0 — Instrumentation + fast tests (½ day; do this first)

Twice this effort stalled on "why didn't X inline?", answered both times by ad-hoc env-var
`Console.Error` hacks that were then deleted. Make the question answerable permanently:

- **`InlineReport`**: `TryInlineCall` returns (or records) a refusal reason from a small enum
  (`NoCalleeBody`, `NoGroundTir(recv)`, `StatementBody`, `ReturnTypeMismatch`, `NotSelfContained`,
  `LambdaArgRefused(why)`, `BudgetExceeded`, …). Aggregate per generation into a table the CLI can
  print under `--inline-report`: function × callee × reason, plus totals (calls inlined,
  β-reductions, tuple lifts). Trivial to implement; transforms debugging.
- **TIR-shape test helpers** in `PlatoTests`: `AssertNoDelegateInvoke(tir)`,
  `AssertCollapsed(tir)` (no `TirInvoke` whose target is a parameter; no residual combinator
  `TirCall` in materialization positions), `CountNodes<T>(tir)`.
- **`InlinerTests` fixture**: compile the stdlib **once** (`CheckerTestSupport.CompileStdLib`,
  already cached by the existing suites), run `RunOptimizerPasses` in-proc, and assert TIR shapes
  for a pinned list of flagship functions (`Bounds3D.Transform`, `Point3D.Transform`, …). This
  runs in **seconds**, vs. the ~60s regen+build+test conformance cycle. The conformance suites
  remain the milestone-end gate; the TIR tests are the inner development loop.

### M1 — Tuple bodies via constructor-call rewrite (≈1 day; the simplest thing that works)

**Replace the tuple tail-lift with a position-independent rewrite**: when inlining a callee whose
root is `Tuple_N(...)` and whose return type is a struct `T`, emit **`TirConstructorCall(T, args)`**
(the marker node already exists — `TirComponentUnroller` uses it and `TirCSharpBodyWriter` prints
it) instead of the bare tuple.

- **Why this is simplest**: `new Triangle3D(a, b, c)` is valid in *any* expression position, under
  *any* erasure — no tail-spine tracking, no `ScalarErase` gate, no reliance on C#'s
  position-sensitive tuple conversion. The entire `InlineTails` walk and the `tailPosition`
  parameter likely **delete** (the general bottom-up pass handles the rewritten bodies).
- **The gate** (verified empirically): rewrite only when the struct's regular constructor arity
  equals the tuple arity — true for the whole hot family (`Triangle3D(Point3D,Point3D,Point3D)`,
  `TriangleMesh3D(points, faceIndices)`), **false for `Translation3D`** (1-arg `Vector3` ctor vs a
  3-element tuple via nested conversions) — that shape simply stays uninlined, as today.
- Also handles nested tuples inside bodies in principle, but keep scope to the ROOT tuple first.
- Gates: V2Runtime 204/204, Scalar V1 unaffected, golden refresh showing the mesh/Triangle/Quad
  `Transform` family collapsing, `InlinerTests` pins.

### M2 — Measure (½ day; before deciding M3)

The `optimizer-smoke/` harness (see `optimizer-smoke/README.md`) already builds min/full,
opt/non-opt project pairs. Add micro-benchmarks for the collapsed family: `TriangleMesh3D.Transform`
over 1M points, `Bounds3D.Transform`, a curve-sampling loop. Record before/after (golden@8c0bd6d vs
current) numbers into the doc. The array materializer earned 1.37× on its probe; this work should
be held to the same standard. **M3's shape is chosen from this data, not from aesthetics.**

### M3 — One of: loop fusion, or statement-body inlining (2–3 days; conditional)

Two strongly viable alternatives for the next deep transform — pick ONE after M2:

- **(a) Loop fusion / deforestation at L10.** `Bounds3D.Transform` currently materializes a
  `Point3D[8]` and then calls `.Bounds()` (itself a loop) on it. Fusing producer/consumer
  combinator chains (`Map→Reduce`, `Map→Map`, `Map→ctor-arg`) into single loops eliminates the
  intermediate arrays entirely. Implemented in `TirLoopLowerer` where both sides are already
  visible as `TirLoweredLoop`/recognized calls. Likely the **bigger perf win** (allocation removal)
  and narrower blast radius (one pass, V2-only).
- **(b) Statement-body inlining in `TirInliner`.** Generalize the inliner to splice statement/loop
  bodies (fresh result temp + hoisted statements). More general — it would inline *any* lowered
  callee (e.g. `.Bounds()` post-lowering) — but it is heavier machinery (statement hoisting into
  expression contexts, interaction with L8–L10 ordering, likely a second inline pass after L10).

Recommendation: **(a) first** — it directly consumes what M1 exposes, and (b) can come later if the
report shows significant non-combinator call overhead remaining. Do not build (b) speculatively.

### M4 — Gate relaxations, in payoff order (≈1 day)

Use the M0 `InlineReport` refusal histogram to rank; each item is a small gate change + the fast
TIR tests + one conformance run:

1. **Nested-lambda family**: `Deform(x: Lines3D, f) => Map(xs, l => Deform(l, f))` — `f` occurs
   under a lambda but the *use* is consuming and the lambda is itself a combinator argument that
   L10 will inline. Relax `UnderLambda` refusal to allow substitution into a lambda that is itself
   in a consuming position (the capture-hoist concern doesn't apply to a β-reducible/loop-lowered
   lambda).
2. **Multi-use lambda args** beyond tuple bodies (bounded by the existing
   `uses × lambdaSize ≤ MaxBodyNodes` budget — already implemented, just currently moot).
3. **`insideLambda` call sites**: allow inlining lambda-free callee bodies with a substituted
   lambda arg when the site's enclosing lambda is a combinator argument (same reasoning as 1).

### M5 — (parallel / optional) Non-erased runtime port + emitter forwarders (task #7)

Structural completeness for the all-extension-methods runtime; **off the inlining critical path**
(instance-vs-extension does not affect emission shape). Requires the emitter to stop generating the
colliding wrapper-type forwarder (`AngleExtensions.Cos(this Angle) => self.Cos()`) — note the
forwarder also performs `Number→float` return erasure, which the direct-call replacement must
preserve. Then port `Angle/Vector2-8/Matrix3x2/4x4/Quaternion/Plane` intrinsics to
`<Type>Intrinsics` classes (pattern in `Number.cs`/`Integer.cs`; interface obligations stay
instance). Gate: byte-identity + all suites. Schedule whenever convenient; nothing in M1–M4
depends on it.

## 3. The eight questions, answered directly

**Viable alternatives?** (i) M3(a) fusion vs M3(b) statement inlining — genuinely both viable,
resolved by measurement, see M2/M3. (ii) For M1, the alternative to the ctor rewrite is keeping the
tail-position analysis and adding wrapper-aware tuple *emission* under erasure — rejected: it
preserves position-dependence (principle 2) and more code for less generality. (iii) A more radical
alternative to all of it: make the elaborator emit explicit constructor calls for tuple-to-struct
conversions *at elaboration time* (no bare tuples in TIR at all) — attractive long-term, but it
changes byte-identity for every recipe including default; out of scope while V1 gates are live.

**Reordering?** The one hard order is M0 → everything (instrumentation pays for itself
immediately) and M2 → M3 (measure before building). M1 ↔ M2 could swap, but M1 is cheap and
enlarges what M2 can measure, so M1 first. M5 floats freely. Avoid the temptation to do M3 before
M1: fusion wants the mesh family already collapsed to see the full chains.

**Design principles/decisions?** §1 above. The binding ones: position-independent rewrites over
position tracking; node types over signature types; V2-only gating; golden refresh in the same
change as any emitter-behavior change.

**Simplest thing that could possibly work?** M1's ctor rewrite *is* it, and we are doing it. The
plan deliberately does NOT do statement-body inlining first (the "complete" solution) because the
flagship payoff is reachable without it and M2 may show fusion dominates it anyway.

**Technical debt being introduced (and standing)?**
- The `NoProperties` behavior fork in the inliner — intentional; retire when V1 recipes retire.
- `InlineTails`/`tailPosition` becomes dead after M1 → **delete it in M1**, don't leave it.
- `Inline`'s `int[1]` counter and the loose `ownerTypeName` threading → fold into a small
  `InlineContext` struct (see refactoring below).
- Name-based heuristics: `IsCallableOnReceiver`/`ScaffoldingNames` (emittability by name) and
  `IsEtaShaped` (`_eta` prefix) — fragile but load-bearing; M0's report will show how often they
  fire, informing whether to replace them with plan-derived data.
- `TirLoopLowerer` and `TirLambdaCaptureRewriter` still carry private `StripCoerce`/`ReplaceNode`
  copies that `TirRewrite` supersedes.

**Trivially-easy functions/data structures?** `InlineReport` + refusal-reason enum (M0);
`AssertNoDelegateInvoke`/`AssertCollapsed` TIR matchers (M0); a cached `TirNode.DescendantCount`
(budget checks currently re-enumerate `Descendants()`); `CSharpWriter.IsV2Recipe` property
(readable gate instead of raw `NoProperties` tests); a `--dump-tir` golden-pin test (dump the TIR
for ~5 flagship functions and string-compare — cheapest possible behavior lock).

**Refactoring first?** Small and targeted, all low-risk: (1) `InlineContext` bundling
`writer/callerTir/ownerTypeName/callerNames/count` — removes 6-arg parameter trains before M4 adds
more state; (2) point `TirLoopLowerer.StripCoerce` and `TirLambdaCaptureRewriter.ReplaceNode` at
`TirRewrite`; (3) extract the `IsStatementNode` copies (inliner, capture-rewriter, body-writer) to
one place. Do these inside M0/M1 commits, not as a separate mission.

**Test-framework optimization?** M0's in-proc `InlinerTests` is the big one: seconds-fast TIR
assertions for the inner loop, conformance for milestone gates. Second: the golden-TIR-dump pin
(locks optimizer behavior without a C# build). Third: when running conformance repeatedly, skip
regen when only the runtime changed (the suites rebuild generated code unconditionally; a
`-SkipRegen` flag on the regen scripts is a 5-line change that halves the cycle). Keep
`check-all.ps1` as the end-of-mission battery only.

## 4. Rerun crib (from `C:\Users\cdigg\git\studio`)

```powershell
.\tools\regen-plato.ps1                        # off-flag byte identity (must stay green)
.\tools\regen-conformance-scalar.ps1 -Test     # V1 gate-off behavior, 204/204
.\tools\regen-conformance-v2runtime.ps1 -Test  # V2 gate-on semantics, 204/204
.\tools\regen-generated.ps1                    # golden diff (refresh with -Apply + commit)
dotnet test submodules\Plato\PlatoTests ...    # TirRewriteTests + (M0) InlinerTests
# TIR inspection: Plato.CLI ... --dump-tir=<dir>  (per-phase dumps, one file per type)
```
