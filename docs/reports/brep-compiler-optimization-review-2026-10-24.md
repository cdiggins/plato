# Independent critical review of the BREP compiler proposals, 2026-10-24

Review state: complete, source inspection only. All five finalized proposals and the supervisor brief were read. Platonic Coder and Parallel Wave were applied. No compiler changes, builds, benchmarks, tracker changes or Git mutations were performed. Existing dirty status outputs and root PROGRESS.md were preserved. The existing test results in the brief are inherited baseline evidence, not verification performed by this reviewer.

## Verdict

Proceed with **one bounded conditional-singleton append optimization in the existing FlatMap emitter**, after the typing and workload preflight below. Do not approve the whole roadmap for implementation. This slice has a direct, explainable allocation benefit, does not require moving work across scopes or iterations, and has recognizable instances in cached BREP output. It can retain every unmatched branch unchanged.

Scoped lowering is the best next enabling investment, but even its first proposed scope expansion can expose the existing argument-hoisting problem. Tail conversion is reasonable later infrastructure; it removes no arrays and its capture/control-flow obligations are larger than its immediate benefit for low-degree splines. Concrete-storage propagation has uncertain independent benefit and a mismatch between its proposed scope and test example. Constant specialization merits investigation after scope handling, with an explicit comparison against the existing `--inline` flag and JIT behavior.

Severity: P1 means a correctness prerequisite for the affected transformation; P2 means an implementation or evidence gap that must be resolved before claiming that slice complete. Findings do not imply every deferred prerequisite must be fixed before the selected filter slice. Each stable ID below needs an explicit disposition in the revised plan.

## Source facts checked independently

* `TirCSharpBodyWriter.Loops.cs`, FlatMap case, evaluates the callback result into `_p`, then copies its elements into one List. `ArrayIntrinsics.FlatMap` has the same per-element scheduling. `Intrinsics.MakeArray<T>` returns its `params T[]` argument. Removing a selected one-element temporary while retaining the single Add therefore has a narrow equivalence argument.
* Cached `plato-brep/generated/Ara3D.StepBrep/Generated/BsplineInverse.g.cs` contains both a Number singleton/empty filter near line 32 and a polished UvCoordinate singleton versus dummy-array Take(0) near line 205. Current source also has the latter idiom in `projection-grid.library.plato:57`. The initially attempted ProjectionGrid.g.cs path does not exist in that cached output. These are evidence of relevant shapes, not a fresh code-generation result.
* `TirLoopLowerer.Lower` appends hoists from all call arguments to one list, skips ternary/lambda boundaries, and shares one hoist list across Boolean-chain terms. `TryLower` and producer recognition use names and shapes. The lowerer can fuse direct producer chains even when the separate fusion pass is disabled.
* `TirFusion.CanSubstitute` accepts an unused parameter and a single use under a conditional; `TakeToMapRange` can discard the earlier producer count. These are actual code paths. Their user-visible fault/termination consequences require focused reproducers and semantic triage; existing success tests do not settle them.
* `TirCSharpBodyWriter` emits `TirCoerce.Inner` and relies on the surrounding C# target to perform implicit conversions. `TirArray` deliberately emits an explicit `MakeArray<T>` to preserve element conversions. Changing the surrounding context is consequently a semantic operation, even if the TIR wrapper itself emits no cast.
* `TypeExpression.Equals` compares definition name, definition ID and recursive type arguments. It is a better starting point for same-type checks than Name or ToString. Exact emitted context still matters for implicit conversions.
* Capture rewriting runs after optimizer passes and traverses marker children. It copies captured function parameters but leaves captured locals in place; the interpreter's `MakeClosure` uses an environment snapshot. These facts make capture behavior a real prerequisite for tail-state and callback-specialization work, not a solved consequence of source purity.
* The wrapper-scalar and property-free ADRs remain compatible with all proposed narrow slices. No new scalar erasure, public representation change, or intrinsic is needed. The existing generic-callee binding issue is already tracked as plato-374.

## Actionable findings and required revisions

### CR-01 — P1: keep filter conversion and target typing intact

Applies to the first implementation. Confirmed emitter behavior: implicit conversions are supplied by context, and array literals pin their element type. Replacing `MakeArray<A>(value)` plus an append into `List<B>` with `List<B>.Add(value)` can eliminate an intermediate conversion when A and B differ. Moving a conditional arm to an independently inferred `var` can likewise lose its original conditional target type.

Require a raw expression-bodied lambda, a raw conditional body, one raw singleton `TirArray` arm, a concrete one-dimensional result, and exact agreement among the singleton array element type, conditional result element type, callback return element type and authoritative lowered result element type. Preserve value expressions and their conversions in place. For the first implementation, refuse coercion-wrapped callback/results and any unknown or disagreeing type; do not peel `StripCoerce` to make a pattern fit. If the other branch requires contextual conversion, decline the whole optimization. Prefer a fallback local explicitly typed as the same `IReadOnlyList<T>` used by the original conditional result when that equivalence is established.

Acceptance must include a conversion-bearing singleton and a mismatched/ambiguous type shape that falls back. A raw-type predicate alone is not a test that generated C# still performs the same conversion.

### CR-02 — P2: prove the conservative filter pattern reaches current BREP code

Applies before expanding implementation scope. Cached C# is suggestive, but cannot show whether current post-capture TIR exposes the raw nodes required by CR-01. Require a minimized source fixture matching the current projection candidate expression, using real primitive/runtime declarations and the consumer's existing `--loops --fuse` recipe. Inspect the actual selected TIR or emitted shape and confirm that the proposed predicate fires without enabling `--inline`, stripping conversions or adding Take recognition.

If it does not fire, report the exact wrapper/type barrier and return for a revised contract. Do not weaken the predicate silently. If only accepted-arm arrays disappear, report exactly that: rejected dummy-array/Take allocations and the result List remain. Raw empty MakeArray already may use a cached empty params array; do not count it as a removed per-element allocation without measurement.

### CR-03 — P1: nested statement traversal must not broaden unsafe hoisting

Applies to scoped lowering, not the selected filter emitter change. The S0 proposal says to reuse lowering recursively inside existing blocks. Reusing the current general `Lower` on newly admitted statements also admits expressions such as `F(FaultA(), MapRange(...FaultB...))`, whose loop prelude can run before the earlier ordinary argument. Scope preservation alone does not preserve sequencing.

Revise S0 to admit only direct whole-statement/local-initializer/return combinators whose arguments do not themselves introduce a reordered prelude, or first implement ordered expression prerequisites that bind earlier expressions before a later prelude. Keep short-circuit operands and loop conditions refused. Establish this rule before S1/S2. A later callback-prelude representation needs a single documented binder/children contract plus scoped restoration of `_loopParamNames`; do not let separate implementers choose competing marker formats.

Tests must distinguish sibling faults, selected versus unselected branches, zero-trip loops, and inner range counts changing on successive outer iterations. The unoptimized compiled backend is the scheduling baseline, with interpreter comparison where its fault model is comparable.

### CR-04 — P1: local-binding fusion is deferred until a termination/eagerness policy exists

Applies to fusion across locals. One use and adjacency do not make eager stage fusion equivalent. Producer completion originally precedes the consumer and its seed; fusion interleaves them. Early-exit consumers can omit producer work. A later fault or nontermination can disappear even though no source I/O exists. Array reads through caller-supplied interfaces add further behavior outside the pure source model.

The proposal correctly identifies this issue but its required totality proof remains an unbuilt compiler facility. Do not describe local fusion as ready after only a use-count pass. Defer arbitrary callees, indexing, conversions, recursion, unknown collections and early-exit consumers. Before any narrow total-expression slice, define its admissible input domain and what observations it preserves. Preserve original producer/seed ordering; a flag is not a correctness proof. New tests must include a producer fault after an early consumer result and competing producer/consumer faults. Nontermination tests require a bounded host harness, never an unbounded in-process hang.

### CR-05 — P1 for new semantic rewrites: call identity needs a precise contract

Applies to Take/MapRange recognition extensions, constant folding, specialization, tail self-call matching, and any newly exposed operation eligibility. Source confirms matching by spelling today, and fusion synthesizes null-Callee calls. A same-name user body must not be treated as the standard operation solely because its signature fits. Preserving an outer callee does not retroactively prove synthesized producers are trusted.

For these future passes, define the actual symbol/provenance rule, including monomorphized definitions and synthesized operations, before implementation. Null or ambiguous identity must fall back. This need not block the first **structural-only** append slice: its input is already an existing FlatMap marker, and it changes only the consumption of a raw array literal without adding call recognition. Keep that distinction explicit. Do not require the filter implementer to redesign the existing matcher as an incidental prerequisite. Triage its baseline weakness separately as H-03 below; never claim the emitter patch fixes it.

### CR-06 — P1: tail conversion needs a complete control/capture contract before buffer work

Applies to scratch-storage transformation A. Fresh per-iteration snapshots and evaluate-all-next-arguments-before-state-writes are necessary. They are not sufficient if a later pass hoists a captured snapshot outside its defining iteration, if terminal paths fall through, or if a recursive result conversion remains after the apparent self call.

Start with exact self identity and an expression/return tree whose leaves are either ordinary returns or direct self calls, with no executable result conversion and no callback-return traversal. Keep state mutable but snapshot definitions fresh within each iteration. Explicitly test swap/dependent arguments, selected terminal guards, callback values retained across iterations, and rejected non-tail/result-converted calls. Preserve integer wrapper operations and checked native-count conversions; do not treat recurrence arithmetic as unbounded mathematical arithmetic. The pass must be wired in both normal and diagnostic optimizer paths.

The proposed exemption for stack exhaustion and allocation exhaustion should be written as the new transformation's conservative optimization boundary. It should not silently rewrite the entire language's evaluation semantics. Tail conversion alone is enabling work, with no array-allocation success criterion.

### CR-07 — P1: scratch reuse requires logical bounds and explicit refusal, not just liveness

Applies to scratch-storage transformations B–D. Restrict initial reuse to one private, non-growing recurrence with a scalar terminal result and closed numeric value elements. Reject aliases stored in other state, records, arrays, surviving closures, unknown calls or returned values. Borrowed input stays untouched. Prove the whole old level remains available before publishing the next level.

Two lazily allocated alternating arrays are a defensible first storage algorithm; capacity and logical count must be distinct. Every indexed read must either be proved within the logical count with overflow-aware arithmetic or retain an equivalent logical bound check. Otherwise larger retained capacity silently changes invalid accesses into stale-value reads. In-place ascending updates require a separate dependence proof and must not change traversal order to make an unsafe stencil fit. Repeated-value array replacement must retain count conversion, zero-count behavior and index faults and needs a specialized call boundary for the actual BREP use.

Required tests include borrowed aliases, retained closures, stale slots above live length, alternating parity, zero/negative/overflow counts, malformed derivative sizes and later-index faults. Measure allocation versus recurrence depth and common low degrees separately; two buffers may save nothing in shallow cases. No stackalloc or pool in this milestone.

### CR-08 — P1: specialization must preserve call identity, eager actuals and result context

Applies to statement-aware inlining and constant propagation. The proposed narrow straight-line slot restriction is sensible. Require all nontrivial actuals, including unused ones and implicit conversions, to be evaluated once in original order before the callee statements. Clone local definition identities as well as names. Preserve the resolved result conversion at the original call boundary; inserting a `var` result must not defer or remove it.

Do not reuse existing expression substitution as proof of eager equivalence: its source explicitly permits fewer evaluations. Keep plato-374's ambiguous generic dispatch cases ineligible until separately resolved. Integer/Boolean folding must identify actual operations and model the selected profile; refuse unproved arithmetic rather than evaluating host defaults. Enforce a cumulative expansion budget, not only per-callee size.

RationalPartial(0,0) already skips expensive derivative calls. Its specialization is therefore mainly branch/scalar simplification and exposure for later passes. Compare baseline recipe, old compiler with `--inline`, and new compiler with `--inline`; only the last comparison isolates this feature. Measure generated/native code size and startup as well as warmed execution. Do not claim allocation removal merely from less generated text.

### CR-09 — P2: concrete-storage slice A's test example is outside its proposed eligibility

Confirmed plan mismatch. Slice A accepts a producer's direct `TirTempRef` in the same block, with alias propagation deferred. Its suggested multi-consumed local is instead represented as `TirVariable`, so the proof cannot reach that producer under the stated contract. Direct Map/Reduce chains may disappear into the current lowerer's source-stage fusion even with `--fuse` disabled.

Before implementing this slice, perform a static eligibility census and select a surviving shape, for example separately lowered MapRange sources feeding Zip, if confirmed by emitted TIR. Alternatively, explicitly include single-assignment alias propagation in a revised scope with its own invalidation tests. Do not add unused provenance infrastructure based on a fixture it cannot optimize.

Keep ordinary interface locals and all public/generic call boundaries unchanged; use the backing array only for the compiler-generated source length/index pair. Require exact physical element type, lexical dominance, and preservation of facts through capture rebuilding. This changes no allocations. If JIT inspection or a warmed benchmark shows the same code or no useful improvement, defer it rather than widening the public ABI.

### CR-10 — P1 for capturing callbacks, P2 for the first callback prototype: share specialization and settle capture behavior

Applies to proposal 5's callback work. This depends on the later clone registry in proposal 3, not merely on its first statement-inlining slice. State that dependency explicitly. Do not build a second registry or duplicate the inliner's dispatch/conversion policy. Begin with a known noncapturing callable whose every use is understood. Pass runtime captures as ordinary parameters only after capture timing and assignment behavior have a tested contract.

The interpreter snapshot and C# local-capture assumptions need a reproducer before choosing an oracle for assignable captures. Until then reject them; do not make a value snapshot on the strength of purity prose. Struct functors remain deferred: they add generic code size, copies, ABI/boxing hazards and a second callable representation. Existing literal callbacks already inline in lowered loops, so profile surviving callbacks before choosing a target.

### CR-11 — P1 for acceptance: compile and execute the actual optimized output

Applies to every implementation slice. Existing loop/fusion tests primarily inspect text. The ShortCircuitTests differential example uses default backend options and would miss optimized output if copied unchanged. Require explicit flag combinations and generated C# compilation using actual wrappers and the target language version; Roslyn's installed default parse version is not, by itself, proof of the net8 SDK's default language version.

For filter allocation evidence, compare the new branch path with an equivalent retained ordinary callback/fallback or controlled pre-change emitter on the **same source materialization and flags**. Comparing only loops-off to loops-on confounds callback and producer-lowering savings. Warm invocation delegates before measuring and keep reflection, fixture creation, formatting and assertions outside the allocation loop. Consume outputs. Separate result-list storage from per-element singleton arrays. Require measured allocation reduction on a recognized case; throughput and whole-mesher speed remain unknown until measured.

### CR-12 — P2: sequence shared infrastructure rather than launching all implementations together

All five plans eventually touch `TirLoopLowerer.cs`, `TirCSharpBodyWriter.Loops.cs`, the optimizer pipeline or capture handling. Parallel planning was useful; unrestricted parallel implementation would create incompatible assumptions. Give the first implementer only the loop emitter, its dedicated test file(s), and private evidence paths. Supervisor owns optimizer documentation, tracker and build-record outputs. Subsequent scopes need a shared expression-prerequisite/binder contract before independent helpers are assigned. Re-run affected gates when those contracts change; do not treat an agent's earlier check as integrated verification.

## Baseline hazards to triage separately

These are source-inspected concerns, not new executable reproductions. They are not automatic blockers for the structural append slice. Record them as investigations and promote them to bugs only with a reproducer and an agreed expectation. Do not fix them incidentally in this optimization.

* **H-01 (P1 hypothesis): argument/Boolean hoisting.** `Lower(TirCall)` and `Lower(TirBooleanChain)` can collect a later computation before an earlier fault or short-circuit guard. Create minimized TIR/source tests demonstrating reachable forms. Newly admitted scopes must refuse these until sequencing is proven (CR-03).
* **H-02 (P1 hypothesis): fusion changes completion/fault/termination behavior.** Map composition can discard unused or conditionally used producer expressions; Take replacement can discard the producer count; consumer fusion can stop early. Compare eager unfused C# with optimized output, including checked conversion overflow in a discarded count, then decide which differences violate the intended contract (CR-04).
* **H-03 (P1 hypothesis): same-name combinator misrecognition and coercion stripping.** Existing name/shape matching and unrestricted StripCoerce lack an explicit standard-operation provenance check. A custom same-name body or executable conversion is the required reproducer. Do not force a new filter acceptance test to pass by silently fixing this entire baseline class (CR-05).
* **H-04 (P1 hypothesis): assigned-local closure mismatch.** Confirm whether legal source can retain a closure across local reassignment and whether interpreter snapshots differ from emitted C# local capture. Reuse an existing issue if found. This blocks admitting assignable captures in new transforms, not immutable captures in the filter fixture (CR-06/CR-10).
* **H-05 (P2 hypothesis): generated-name collisions.** Loop IDs avoid other generated IDs but not demonstrably user names. Compile parameters/locals named `_lp0`, `_i0`, and the relevant suffixed callback spelling; only file a confirmed bug if reachable. No new generated-name scheme is needed for the proposed first slice; branch blocks can reuse existing fallback names in separate lexical scopes.

## Revised first implementation contract: F1

**Behavior:** within an already selected lowered FlatMap, emit `if (condition) list.Add(value); else { original fallback evaluation and copy; }` when the raw, typed conditional/singleton pattern passes CR-01. Support either branch orientation. Evaluate the predicate once, only evaluate the selected value/branch, preserve source order and duplicates, and retain the result List/interface boundary. Keep the current allocation capacity policy. No new call matching, Take/empty-call simplification, multi-element append expansion, local fusion, scoped lowering, recursion work or public API changes.

**Files:** `writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs` plus a focused new partial helper only if it improves clarity; `tests/PlatoTests/Optimization/FlatMapFilterTests.cs` and narrowly owned fixtures if necessary. Do not edit lowerer/fusion/capture core as an opportunistic fix. Supervisor updates `docs/csharp-optimizer-pipeline.md` once the actual supported shape is verified. Build snapshot and logs remain supervisor-owned.

**Preflight:** inspect post-capture typed shapes from a minimized current BREP candidate expression. Prove the exact matched array/result types. Build one compiled baseline with real runtime wrappers, including the target's default net8 language version. If raw shape/typing or test-harness assumptions fail, stop the expansion and report the blocker before changing the contract.

**Acceptance:**

1. Compile and run all-kept, all-rejected, mixed, empty, duplicates and reversed-arm inputs with loops off, loops on/fuse off and loops on/fuse on. A fused producer test is separate from semantic fault tests, which use a materialized input.
2. Use throwing/counting host callbacks or equivalent distinct invalid-access fixtures to show predicate once, accepted value once, rejected value never, fallback evaluation exactly when selected, and zero source elements invoking neither branch. Retain `[Fault()].Take(0)` behavior without recognizing Take.
3. Cover outer immutable captures, a predicate containing a nested Boolean conditional, typed wrapper/record elements, conversion-bearing elements and conservative fallback for wrapped/mismatched/generic-unknown result types. Existing unsupported delegate callbacks and multi-element arrays keep their ordinary path.
4. Assert the recognized branch no longer calls MakeArray and the opposite branch remains present. Compile/run rather than extracting only a fragile method substring for execution. Retain existing CombinatorFusionTests, LoopLowererArrayTypeTests and relevant conditional/lambda emission checks.
5. Demonstrate a warmed per-call allocation decrease versus the same lowered FlatMap's ordinary path, with all other optimizations constant and generated-source evidence. Verify output equality. Record actual bytes and timing protocol in evidence, not normative design prose. A no-regression timing sample is useful; a guaranteed mesher speedup is not an acceptance promise.
6. Run repository-required integrated gates once after stable-input checks. Record unrelated baseline failures explicitly. A minimized real candidate fixture establishes applicability; a full regenerated BREP law/digest/throughput run is an integration follow-up unless the supervisor assigns that separate checkout and resource ownership now. Do not label whole-BREP behavior verified without it.

**Fallback:** if a raw conditional pattern is not reached safely, narrow to a directly returned raw singleton callback body if an actual relevant compiled shape exists, or return to planning. Do not turn the failure into broad Take recognition or coercion stripping. Scoped statement lowering is the next separately reviewed candidate, with CR-03 refusal rules incorporated first.

## Per-idea disposition and later order

| Idea | Verdict and strongest credible benefit | Remaining uncertainty / dependency |
| --- | --- | --- |
| Scratch storage | Keep as staged research and later implementation: tail frames first, explicit private alternating buffers next, dependence-proved in-place reuse later. The allocation payoff is the strongest long-term candidate. | Ownership, logical bounds, overflow, captures and low-degree benefit remain unproved. Requires scoped recurrence visibility; CR-06/07. |
| Scoped lowering | Prioritize immediately after F1. Existing-scope traversal, then tail-return ternaries, then expression callback preludes. It exposes the actual spline kernel and removes some callback overhead. | S0 is enabling only. Ordered siblings and binder/capture contracts must be solved or explicitly refused; CR-03. Arrays remain allocated. |
| Constant specialization | Keep narrow statement inlining and integer/Boolean propagation as separate milestones; measure before adding clones/CSE/invariant motion. | Existing JIT and already guarded derivative paths limit standalone claims; known generic binding and result-context risks; CR-08. |
| Fusion/filter | Ship F1 if preflight passes. Defer local-binding fusion and Take simplification. | Accepted singleton allocation removal is concrete; total filter allocation removal is not delivered. CR-01/02/04/05. |
| Concrete storage/callbacks | Census and benchmark local storage opportunities before implementation; known noncapturing callback specialization only after shared clone infrastructure. | First test shape is currently outside storage slice A, JIT may already devirtualize, dominant arrays cross calls. CR-09/10. |

The review does not recommend implementing global caches, scalar erasure, general mutable array escape analysis, arbitrary local fusion, public Span/functor representations, stack allocation, arithmetic reassociation or blanket optimization of statement callbacks. The proposals correctly identify many of these as later work; the revised plan must retain those deferrals rather than treating this review as authorization to implement all stages.
