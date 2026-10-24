# Proposal: remove recursive numerical-kernel temporaries

Reviewed proposal for compiler-481. The [combined plan](../brep-compiler-optimization-plan.md) governs implementation order and the released first slice. This document states design and proof obligations; measurements belong in validation logs.

## Incorporated critical review

CR-05/06/07 govern this proposal. Tail conversion is deferred work under compiler-484, after F1 and restricted scoped lowering. Require exact self-call identity and a complete return tree; reject executable result conversions and callback-return traversal. Test iteration-local captures through the complete later optimizer/capture pipeline, not only the new pass. Removing stack/allocation exhaustion is a narrowly stated resource boundary; ordinary domain faults, guards and evaluation remain preserved. Both normal and diagnostic optimizer paths must apply the same pass.

Storage reuse is a separate milestone. Its initial proof accepts one private non-growing recurrence with scalar result and closed numeric value elements; it rejects aliases, escaping state and unknown calls. Alternating buffers precede any in-place reuse; capacity never substitutes for logical bounds. Use overflow-aware proofs or equivalent logical checks. No stack allocation or pool is released by this design. A later repeated-value specialization retains count conversion, empty behavior and indexed faults. The first implementation contract is in the combined plan, not this future sequence.

## Recommendation

Start with a small generic conversion of direct self tail calls into loops, preserving evaluation scopes and argument snapshots. Treat that as an enabling change, not an allocation optimization. After scoped combinator lowering is available, prove private ownership for one non-escaping, shrinking loop-carried array and reuse its storage. Start with two alternating heap buffers; add in-place updates only when their dependence proof is explicit. Repeated-value parameter elimination and stack allocation are subsequent, independent optimizations.

Do not recognize spline function names. Match resolved calls, types, data flow, and index relations. Keep public signatures, wrapper scalar types, and source purity intact.

## Evidence checked

* `plato-brep/library/01-geometry/bspline-evaluation.library.plato:135`: `DeBoorLevels` returns `d[0]` at the base case, otherwise calls itself with a newly materialized `(d.Count - 1).MapRange(...)`. Each output reads `d[m]` and `d[m+1]`. Other arguments mostly remain fixed; `r` increases.
* Same source at line 150: `LocalDerivative` tail-recurses over a fresh `p.MapRange(...)`, reducing degree and derivative order. Its zero-order exit calls `DeBoorLevels` with `p.MapRange(i => t)`. That last repeated-value array is a separate allocation opportunity.
* Same source at line 250: `HomogeneousPartial` builds each local control column inside a `MapRange` callback, then evaluates the column and finally the resulting column array. These nested scopes matter for scratch lifetime.
* `plato-brep/generated/Ara3D.StepBrep/Generated/Extensions.g.cs:1962–2037` retains both recursive calls and eager `MapRange` callbacks. `BsplineEvaluation.g.cs:137` has an outer emitted loop whose callback path still constructs a blossom parameter array. This is inspection of existing cached generated output, not regeneration against the current compiler.
* `writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs:139–160` refuses `TirBlock`, `TirIf`, `TirLoop`, `TirLambda`, and `TirConditional` at its current descent boundaries. The crucial recursive branches therefore remain opaque.
* `writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs:124–149` allocates a fresh `T[]` for each lowered map. Lowering alone does not reuse storage. Array results are exposed as `IReadOnlyList<T>` to retain compatibility with reductions and conditional joins.
* `src/Plato.Intrinsics/ArrayIntrinsics.cs:23–36` specifies eager index-order mapper evaluation, once per element, including the first faulting element. `Integer.MapRange` performs checked conversion to native count (`Integer.cs:88`). Storage optimizations must retain checked conversion and the nonpositive-count empty case.
* `src/Plato.Compiler/Checking/Tir.cs` already provides locals, assignments, blocks, conditionals, returns, and loops. It has no ordinary `continue` node. A tail-call transform can avoid introducing one by requiring every loop-body path to end at a return or at the argument-state update.
* `TirCSharpBodyWriter.cs:165` applies `TirLambdaCaptureRewriter` after optimizer passes. That rewriter snapshots captured function parameters at function entry, but leaves captured locals in place. Converting recursion by assigning original parameters is therefore unsafe: captured values can become stale. Use fresh per-iteration local snapshots and preserve their lexical scope.
* `CSharpWriter.RunOptimizerPasses` runs inline, fusion, component expansion, materialization, then loop lowering. Any new pass must be represented in both the ordinary and diagnostic pipeline paths, and must not invalidate later walkers' handling of emission-only nodes.

The sibling investigation `plato-brep/docs/plans/bspline-performance-2026-10-24.md` records repeated small-array allocation during surface evaluation and substantial attribution to rational evaluation/de Boor. Those are existing investigation results, not newly measured results. Its face-worker percentages are not elapsed-time speedup predictions; its successful cache benchmark combines caching and candidate-loop changes and cannot estimate this compiler pass's benefit.

The accepted wrapper-scalar decision (`tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md`) rules out reviving scalar erasure as an expedient. `docs/SEMANTICS.md` and the affine-builder contract in `stdlib/foundation/primitives.plato` preserve immutable arrays; compiler-private mutation is legal only when unobservable. Do not add a spline-specific intrinsic or expand the irreducible host kernel.

## Transformation A: direct self tail calls

Illustrative source, omitting wrapper syntax:

```text
F(d, r, other) = terminal(d, r)
    ? result(d, r)
    : F(map(d, r), r + 1, other)
```

First generated shape:

```csharp
var stateD = d;
var stateR = r;
var stateOther = other;
while (true)
{
    // Fresh lexical bindings each iteration, also safe for retained closures.
    var currentD = stateD;
    var currentR = stateR;
    var currentOther = stateOther;
    if (terminal(currentD, currentR))
        return result(currentD, currentR);
    var nextD = map(currentD, currentR);
    var nextR = currentR + 1;
    var nextOther = currentOther;
    stateD = nextD;
    stateR = nextR;
    stateOther = nextOther;
}
```

Evaluate every recursive argument left-to-right into a temporary before any state write. This is parallel assignment, not sequential substitution: `F(a,b) => F(b,a)` must swap. Do not eliminate unused argument computations unless independently proved total and discardable. The original terminal condition remains inside the loop and base expressions execute only on their original branches.

Initial eligibility: exact resolved self-call identity and same reified signature; expression bodies whose terminal branches are direct self calls or ordinary results, followed by straightforward return/if/block bodies. Reject mutual recursion, recursive calls inside lambdas, non-tail calls, return coercions with executable conversions, incomplete terminal paths, and unknown control-flow shapes. Do not make traversal under a lambda imply that its returns belong to the containing function. Keep fallbacks ordinary recursion.

Prefer a writer-side `TirTailCallLowerer` using existing TIR nodes. Use identity-based substitutions and distinct local definitions for state, snapshots, and next arguments. If implementation cannot represent the phase safely with existing nodes, introduce one narrowly specified writer-only marker; do not introduce shared-core ownership types merely to ease emission. Gate initially under existing `LowerLoops`, which already requests generated loops, with explicit refusal diagnostics in test assertions and TIR dumps.

Benefit: removes managed recursive frames and exposes recurrence structure. It does not remove one array by itself. For typical low degrees, standalone time savings may be negligible. Acceptance should be semantic correctness and an enabled later optimization, not an unsupported spline speed claim.

## Transformation B: private recurrence buffers

Recognize an eligible tail loop's array state and prove all its uses. A first implementation should accept a non-increasing, known count expression, read-only element operations, and a scalar terminal result. Permit only `Count`, bounded indexed loads, and the recognized map as uses of the candidate array. Reject handing the entire array to an unknown call, retaining it in another loop-carried variable, embedding it in a returned structure, capturing it in a surviving/escaping delegate, or returning it. Other input arrays remain borrowed and immutable.

Initially consume the original input directly. The first replacement map produces private storage. On the second replacement allocate a second private buffer; later replacements alternate them. Allocate lazily in the execution region where a map would have allocated, with capacity sufficient for the proven non-increasing sequence. Keep logical count separate from capacity. Compute all next values before publishing the new logical array state. Never infer that an `IReadOnlyList<T>` argument is writable or uniquely owned merely because the current function does not return it.

This removes allocation growth with recurrence depth while preserving the complete old level until the next is computed. It does not require caller ownership or arbitrary dependence analysis. The first two stages may allocate as before, so shallow degree cases may show no allocation reduction. Dynamic growth, multiple intertwined array states, externally visible aliases, and arbitrary call graphs fall back.

Logical bounds are essential: if capacity exceeds live length, reading `scratch[liveLength]` must not expose a stale value that the original array would reject. Prove the index is in the logical range or emit a logical-range check that retains the original checked-index conversion/fault behavior. An invalid `p`/local-size relationship in a public derivative function cannot be silently repaired. `Count` must observe logical count, never capacity.

Arrays can contain references even though the outer value is immutable. For the first slice restrict element types to closed numeric value types, or separately prove no retained element/callback contains the scratch reference. Managed-reference slots beyond live length also retain objects longer; general element support needs a clearing/liveness policy and measurements. No process-global, thread-static, shared pool, or cross-call scratch in the first implementation.

## Transformation C: one in-place buffer, with dependence proof

For shrinking stencils that read only old indices at or above the current destination, ascending traversal is safe. De Boor's `out[m] = blend(old[m], old[m+1])` and the derivative's adjacent difference have this shape. Evaluate the complete right-hand side before writing `scratch[m]`. Earlier writes affect only indices lower than `m`, which no later right-hand side reads.

The first map still creates private storage from the borrowed input; subsequent maps reuse that storage in place. For a recurrence with many shrinking stages this reduces map storage to one allocation without mutating caller data. A reverse-direction loop is not automatically allowed: changing index evaluation order can change the first fault and any relevant callback behavior. If a dependence proof cannot preserve original ascending order, use alternating buffers.

Require explicit affine index proof over loop index plus nonnegative constants, logical-range preservation, and no hidden array access through callees. Unknown accesses refuse in-place reuse. Preserve expression trees exactly: do not replace `a.Lerp(b,t)` with a mathematically equivalent formula, reassociate arithmetic, introduce fused multiply-add, or reorder reductions.

## Transformation D: repeated-value array replacement

`n.MapRange(i => t)` may be represented internally by `(logicalCount, scalarValue)` only where all uses are compiler-visible observations and no materialization boundary escapes. Constant parameter loads are safe to repeat; arbitrary loop-invariant expressions are not necessarily safe to hoist: they might fault on an empty map, allocate, or invoke unknown code.

Replacing `At(repeat(n,t), j)` by `t` alone is wrong for invalid `j`; retain count conversion and logical bounds checks. Preserve the evaluation of the count and original argument order even when the array never allocates. Do not turn an eager map of an expensive/faulting expression into a lazy value just because the language is pure.

The relevant BREP `ts` array crosses a function boundary into a recursive helper and blossom callers also pass nonconstant sequences. Therefore local scalar replacement alone will not remove it. A future specialization pass can clone the callee for a proven repeated producer, pass scalar plus logical length internally, and retain the general public helper. Whole-program replacement of `Array<T>` with a repeat-view runtime class would add allocations/dispatch and recreate lazy-runtime hazards; do not start there.

## Ordered implementable chunks and ownership

1. **Direct self tail-call conversion, independently shippable prerequisite.** Add `writers/Plato.CSharpWriter/Optimization/TirTailCallLowerer.cs`, integrate centrally in `CSharpWriter.cs`, and add `tests/PlatoTests/Optimization/TailCallLowererTests.cs`. Prefer inline test fixtures until reusable fixtures are justified. Tests must compile and execute generated C# against the real runtime, not only count emitted loops. Update optimizer documentation after verified behavior. This is the smallest implementation agent task.
2. **Scoped lowering dependency.** Integrate the separate nested-scope lowering work before promising de Boor allocation improvements. Coordinate ownership of `CSharpWriter.cs`, `TirLoopLowerer.cs`, and `TirCSharpBodyWriter.Loops.cs`; this track cannot independently modify those concurrently. Verify recurrence maps are now explicit loops inside their original branches/iterations.
3. **Ownership/lifetime analysis plus alternating buffers.** Add a narrow writer-side `TirScratchStorageAnalysis.cs` and `TirScratchStorageLowerer.cs`, and isolated `ScratchStorageTests.cs`. Give the emitter explicit storage identity, logical length, and lifetime metadata, rather than inferring ownership from generated variable names. Integrate with lowerer markers after scoped lowering. Initial candidate shape is one non-escaping shrinking recurrence, scalar result, numeric value elements.
4. **Ascending adjacent-read in-place reuse.** Extend analysis with a small index-dependence proof and add negative/refusal cases for backwards dependencies and hidden aliasing. Only land when independent generated-runtime differential tests pass.
5. **Repeated-value specialization.** Hand off to the specialization track's stable internal-call convention; add separate `RepeatedArrayTests.cs`. Keep general-array entry points and bounds checks. Do not require this to ship earlier chunks.
6. **Stack/local allocation experiment, deferred.** Only after escape proofs and concrete storage are stable, consider bounded `Span<T>` or fixed local fields. Require compile-time capacity/element-layout proof or guarded heap fallback. Avoid `stackalloc` per enclosing loop iteration; establish lifetime and maximum stack budget. Net8/default-language restrictions, ref-struct capture rules, and public `IReadOnlyList<T>` boundaries make this a separate emitter design, not a trivial allocation substitution. Avoid pooling until concurrency, clearing, return-on-fault, and retention costs have a measured reason.

Pass-order proposal to review: specialization/inlining exposes recurrence; tail conversion occurs before scoped loop lowering; ownership/storage analysis consumes structured loops afterward. Earlier fusion can erase useful producer shapes, so analyze the exact pipeline representations and keep a conservative fallback. No pass should depend on textual C# names or re-run prior passes without a bounded convergence rule.

## Acceptance and falsifying experiments

For tail calls, compare unoptimized compiled C#, optimized compiled C#, and interpreter values on terminating cases. Test zero/one/many steps; argument swaps and dependencies; multiple terminal branches; ordinary early returns; overload identity; reified generic identities; numeric receiver emitted as `this`; recursion nested in a lambda (must remain untouched); non-tail recursion refusal; and parameter captures whose values change each iteration. A long-depth case belongs only on the transformed path because overflowing a reference process stack is not a safe differential test. Test finite depth and output shape separately.

For scratch, test aliased caller input twice and verify it is unchanged; alias stored in a record; input reused in another callback; returned arrays; zero/negative count; overflow on native count conversion; malformed bounds; a mapper fault at a known index; high-water capacity versus logical length; degree-zero/one cases; alternating buffer parity; nonnumeric element rejection; high-degree fallback; and concurrent independent evaluations. Use a host instrumented mapper/indexer for trace assertions where possible, clearly labeled as a test oracle for scheduling rather than new language effects.

Run code-generation fixtures against default LangVersion on net8. Use the actual runtime wrappers. Existing `tests/PlatoTests/Emission/BuilderReceiverEmissionTests.cs` demonstrates Roslyn compile-and-execute testing, but do not copy its method-extraction shortcut blindly for multi-line loop bodies. Existing loop/fusion tests mostly inspect text and are insufficient as this pass's semantic gate.

Measure allocated bytes with a warmed, single-thread repeated evaluator call and `GC.GetAllocatedBytesForCurrentThread`, no reflection or fixture construction in the measured loop. Pin compiler revision, generated-source hashes, flags, .NET runtime/architecture, input fixtures, degree/order, and iteration count. Report bytes/evaluation and median time separately with alternating baseline/variant batches. Add stage counts proving allocations stop scaling with recurrence depth. Falsify the benefit if array allocations remain in the emitted inner recurrence, aliases force fallback on the real cases, two-buffer overhead dominates common low degrees, or the JIT fails to optimize resulting index/dispatch overhead.

After compiler-unit verification, regenerate BREP output under a pinned toolchain in a separately owned workspace. Run `Law_BSplineLocalDerivativeAgrees`, `Law_BSplineLocalDerivativeAgreesOnFamily`, `Law_BSplineSurfaceFirstJetAgrees`, and `Law_StepsFromJetAgree` families from `bspline-local.laws.plato` / `bspline-polish.laws.plato` on interpreter and C# backends. Add exact component-bit comparisons for optimized versus unoptimized C# where laws use tolerances. Cover repeated knots, rational weights, boundaries, periodic seams, collapsed directions, and degree/order extremes. Full mesh digests and existing contract outcomes are integration acceptance, not replaced by a microbenchmark.

Supervisor owns required recorded builds/gates and stable-input coordination. Relevant developer loop: the new optimization test fixtures in `tests/PlatoTests/PlatoTests.csproj`; integration then records compiler builds via `tools/dotnet-build-record.ps1`, runs `tools/record-gates.py`, and runs the required studio `check-all.ps1` once at mission end. Exact BREP corpus commands/paths must come from its pinned status manifest and README; this track did not run them.

## Risks and unresolved decisions for the reviewer

* Tail recursion removes stack-overflow behavior and storage reuse removes some allocation failures. `docs/SEMANTICS.md:30–31` excludes exceptions and observable evaluation order, while the runtime explicitly promises eager evaluation and the same first faulting element on both backends. That contract boundary needs an explicit decision; existing fusion behavior is not authorization to relax it. This proposal conservatively preserves ordinary arithmetic/index/callback faults, producer completion, and index order. Document whether resource-exhaustion timing and stack traces are outside optimization equivalence before relying on that distinction. This is a design ambiguity, not a newly reproduced compiler defect.
* Capture rewriting currently assumes captured locals need no snapshot because source values are stable. Compiler-generated mutable state must not accidentally exploit that assumption. Fresh iteration snapshots are mandatory and must survive all later substitutions.
* General escape analysis is much larger than a handful of liveness checks. A deliberately small proof that refuses unrecognized uses is preferable; report why actual BREP kernels were refused instead of weakening it.
* Integer arithmetic uses wrappers and profile-specific representation. Capacity arithmetic and bound proofs must model overflow, not ordinary mathematical integers. Derivative input invariants are not established by their names or intended use.
* Two-buffer reuse benefits deeper recurrence more than low degree. In-place recurrence is more promising for cubic inputs but has a stronger proof obligation. Claiming all temporary arrays disappear after tail lowering would be incorrect.
* Caller-to-callee ownership transfer is needed to eliminate local-column allocations, beyond this intraprocedural proposal. Cloning an internal helper for an owned array is a later specialization feature with a public borrowed-input fallback.
* Reusing scratch across independent column evaluations needs a region whose buffers cannot escape each iteration. Per-call scratch alone does not accomplish that. Cross-worker/global reuse is excluded.

No new confirmed out-of-scope defect was established. The tracker already records builder-receiver emission history in `compiler-416`; it is not a new blocker and should not be refiled. Initial `git status` could not inspect the parakeet submodule due access permissions; `git status --ignore-submodules=all` showed unrelated dirty report/tracker files and `PROGRESS.md`, all left untouched.
