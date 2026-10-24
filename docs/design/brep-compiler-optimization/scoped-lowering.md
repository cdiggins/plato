# Scoped combinator lowering proposal

Reviewed proposal for compiler-481. The [combined plan](../brep-compiler-optimization-plan.md) governs implementation order and the released first slice. This document states design and proof obligations; measurements belong in validation logs.

## Incorporated critical review

CR-03 narrows S0: new statement scopes admit only a combinator that occupies an entire local initializer or return and whose arguments introduce no nested reordered prelude. If this eligibility cannot be proved, retain the original expression. Do not recursively call the current general hoister on arbitrary newly entered expressions. Broader forms require ordered prerequisites that bind earlier siblings before a later prelude. Conditions, Boolean-chain operands and loop conditions remain outside S0. Test competing sibling faults, unselected branches, zero-trip loops and inner counts that change per outer iteration.

Before S1/S2, document one binder/Children/WithChildren contract and scoped restoration of loop-parameter names. No implementation agent may independently invent a competing callback marker. CR-11 requires explicit optimization flags, compiled execution with actual wrappers and C# 12 for net8, not only string checks or default-backend laws. These are release gates for compiler-485; F1 ships independently first. Existing unexecuted sequencing/capture/name hypotheses are tracked by compiler-483.

## Recommendation

After the combined plan's F1 slice, implement restricted recursive lowering in **existing statement scopes**: nested blocks, the bodies of `if` arms, and the bodies of `while` loops. Give every original statement its own prelude, inserted immediately before that statement in its original enclosing block. Keep condition expressions, ternary expressions, and lambdas unchanged in this first chunk. This is the smallest implementation because existing TIR and C# emission already represent every necessary statement. It establishes the scope discipline without inventing a new expression representation.

Next lower ternaries that are the direct value of a function return into `if` statements with returns in each arm. This is the smallest extension with a direct B-spline benefit: `DeBoorLevels` and `LocalDerivative` put their allocating kernels in tail-return ternaries. It still needs no value-producing block or callback return transformation. Then add callback-local preludes to remove the nested callback invocation in `HomogeneousPartial`. Arbitrary expression-position ternaries, loop conditions, and arbitrary statement-bodied callbacks should follow separately.

The first chunk is enabling work, not a claim of a material mesher speedup. The next two chunks expose the actual B-spline hot paths. Scoped lowering by itself retains the result arrays; eliminating those arrays requires the separately proposed ownership/buffer optimization.

## Source evidence

Links below are local source evidence, not benchmark claims.

* [TirLoopLowerer.Rewrite and LowerStatement](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs) process top-level statements, but return `TirBlock`, `TirIf`, and `TirLoop` unchanged. [Lower](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs) refuses both lambdas and conditional expressions.
* [FnOk](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs) requires an expression-bodied, non-eta lambda or a correctly typed delegate reference. Simply descending into lambdas and replacing their expressions with blocks makes the enclosing combinator fail that predicate.
* [WriteLoweredLoop](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs) inserts a callback by writing `lam.Body` as an expression after an assignment or condition has already started. A block inserted there is invalid. Fused source stages have the same limitation.
* [TirBlock/TirReturn/TirIf/TirLoop](../../../src/Plato.Compiler/Checking/Tir.cs) already provide statement scopes. [WriteStatement](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) emits them. `TirBlock` is statement-only and has no value type; it must not be used as a pretend C# expression.
* [Ordinary lambda emission](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) already supports brace-delimited lambda bodies, but that is distinct from the callback-inlining path above.
* [Capture rewriting](../../../src/Plato.Compiler/Checking/TirLambdaCaptureRewriter.cs) runs after optimizer lowering and again for ordinary lambda bodies. It enumerates lambda-bound definitions and captured references, and structurally rebuilds marker nodes through `Children` and `WithChildren`. Any new writer marker must implement both correctly.
* [Pass ordering](../../../writers/Plato.CSharpWriter/CSharpWriter.cs) is inline, fusion, component unrolling, materialization, loop lowering; loop lowering is C#-specific and late. This proposal preserves that order and the existing `--loops` flag.
* B-spline source (`../plato-brep/library/01-geometry/bspline-evaluation.library.plato`, sibling repository) places `MapRange` in recursive conditional branches, and `HomogeneousPartial` nests one `MapRange` in another callback. Generated HomogeneousPartial (`../plato-brep/generated/Ara3D.StepBrep/Generated/BsplineEvaluation.g.cs`, sibling repository) contains the outer `for` but leaves the inner `.MapRange((j) => ...)` call. Generated recursive kernels (`../plato-brep/generated/Ara3D.StepBrep/Generated/Extensions.g.cs`, sibling repository) retain `MapRange` in ternaries. This is inspection of cached output, not fresh generation.
* [ArrayIntrinsics.MapRange](../../../src/Plato.Intrinsics/ArrayIntrinsics.cs) explicitly constructs eagerly, computes each index once in order, and describes agreement with interpreter faults. [Language semantics](../../../docs/SEMANTICS.md) instead describes pure values and no observable evaluation order. Preserve existing operational behavior conservatively; do not use purity to justify moving a faulting expression out of a guard. [ShortCircuitTests](../../../tests/Plato.Interpreter.Tests/ShortCircuitTests.cs) verifies guards against invalid indexing.
* Read the [wrapper scalar ADR](../../../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md) and [method-only emission ADR](../../../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md). Neither needs superseding. Keep wrapper-typed indices and public list signatures; no scalar erasure, API spelling change, or new language feature is needed.

## How the transformation works

### Existing statement scopes

Factor a `LowerBlock`/`LowerStatementInScope` operation that returns a statement or a block of statements. For an existing block, append each original statement's local prelude followed by its rewritten statement to the **same** block list. Never wrap a `let` and its prelude in a new child block: that would hide the declared local from later statements.

For `TirIf`, keep its condition untouched initially and recurse independently into each arm. An arm originally represented by a single statement may need a block around its prelude and rewritten statement. Preserve absent `else` arms. For `TirLoop`, keep its condition untouched and recurse into its body. The nested lowered loop therefore runs on each original iteration, and never when the outer loop has zero iterations. Preserve `TirReturn` as a return from the function; there is no new callback boundary here.

Illustrative source:

```plato
if (enabled) {
    var values = count.MapRange(i => Eval(i));
    return Consume(values);
}
```

Illustrative C# after lowering, with descriptive names rather than actual generated suffixes:

```csharp
if (enabled) {
    var sourceCount = count;
    var length = System.Math.Max(0, (int)sourceCount);
    var valuesArray = new Value[length];
    for (var index = 0; index < length; index++) {
        var i = (Integer)index;
        valuesArray[index] = Eval(i);
    }
    IReadOnlyList<Value> loopResult = valuesArray;
    var values = loopResult;
    return Consume(values);
}
```

No computation from the selected statement runs before `enabled` is tested. The array remains eager and remains a list at its consumer boundary.

### Tail-return branches

Normalize only `return cond ? yes : no`, including an expression-bodied function treated as an implicit return, into `if (cond) { return yes; } else { return no; }`. Apply statement-scope lowering recursively to each return. Preserve every explicit coercion around the conditional: distributing a conversion into both arms must retain the conditional's unified result type. The initial version can decline coercion-wrapped forms it cannot prove equivalent.

This handles the following real shape without allocating an IIFE delegate:

```plato
DeBoorLevels(d, ...) => d.Count <= 1 ? d[0] : DeBoorLevels((d.Count - 1).MapRange(...), ...);
```

Afterwards, the count guard still runs first; the base arm still indexes `d[0]` without constructing the recurrence array; only the recursive arm creates and fills the next array, then calls the recursion. This does **not** remove recursion or reuse array storage.

### Inline callback bodies

Represent a prepared callback body as a writer-local plan `(prelude statements, result expression)` rather than changing `TirLambda.Body` prematurely into a general statement tree. Initially accept callbacks that were expression-bodied before preparation; full early-return callback handling is not needed.

Prepare each callback using a fresh scope after the outer call has passed `FnOk`. Keep the lambda binder and original parameter identities visible to capture analysis. Options are an emission-only scoped-value marker inside the lambda body, or explicit prepared-body metadata on a loop marker that preserves the binder when enumerating children. The marker-inside-lambda option is less error-prone for `lambdaBound` enumeration. It must be consumed only by statement-aware emission, never by ordinary `WriteNode` in an unfinished expression. Decide its precise contract centrally before parallel implementation.

Refactor loop emission to run in this order on every iteration: bind source-stage parameter, emit its local prelude, bind stage result; bind final callback parameters, emit callback prelude, then emit final assignment/fold/predicate/append using its result expression. A source stage's prelude runs before the next stage. `Reduce` must bind the old accumulator before executing the step body and update it only after the body produces the new value.

For `HomogeneousPartial`, after binding the outer `i`, emit the inner column array and its `j` loop, then call `LocalDerivative(innerResult, ...)` and assign the resulting value to the outer array. The inner array is still allocated once per outer iteration, but there is no inner `Func` invocation or captured inner lambda. All accesses to the outer `i` refer to that iteration's stable wrapper local.

For ordinary surviving lambdas, use the existing block-lambda emission path: prelude then `return result;`. For a callback already containing arbitrary statements or early returns, retain the existing refusal in the first callback chunk. Supporting them requires a separate transformation of callback returns to a local result and exit target; raw callback `return` cannot be spliced into the enclosing function.

## Semantic invariants and explicit non-goals

1. **Guard and evaluation count.** No prelude crosses a conditional arm, Boolean short-circuit operand, lambda invocation, or loop iteration boundary. Keep loop conditions intact until lowering can rerun their preludes before every condition test, including the final false test.
2. **Eager inputs and faults.** Receiver/count, seed, and arguments are evaluated once, with already-evaluated preceding siblings preserved before a later sibling's prelude. An operation that could throw must not be moved earlier just because it is pure. For general expression lowering, normalize children left-to-right: if a later child contributes a prelude, bind preceding nontrivial children before that prelude. Preserve callback-count and same-first-fault behavior for each individual eager `MapRange`.
3. **Array contract.** Negative and zero `MapRange` counts invoke no callbacks. Keep the current checked `Integer` to `int` conversion behavior; `Integer.MapRange` and the conversion operator supply it. Zip truncates to the shortest input, fixed-width group maps truncate incomplete groups, and `WithNext` must not execute modulo zero. Do not change these existing lowering contracts.
4. **Boolean guards.** `TirConditional` and `TirBooleanChain` are control flow, even when they appear in expression positions. A blanket `Children.Select(Lower)` is wrong. Preserve language-defined short circuit. For the first implementation decline newly exposed expression forms whose correct local sequencing is not established.
5. **Mutation and assignment.** Local rebinding exists despite the language's pure public model. Keep assignment order. Add explicit `TirAssign` handling only for a local/parameter lvalue whose evaluation is trivial; more complex lvalues need ordered address/value evaluation. The current lowerer does not descend into `TirAssign`, so body traversal alone will leave assignment RHS combinators unchanged unless this is separately implemented.
6. **Captures.** Function parameters used in surviving closures may need the existing copy; outer lambda parameters may never be hoisted above the loop that binds them. Keep nested lambda binders visible in marker children. Do not capture the mutable raw C# loop index; retain a wrapper local declared inside each iteration. Do not silently change snapshot-versus-reference behavior when local variables are reassigned.
7. **Names and lifetime.** Preserve identity-based binding rather than matching names. Each new emitted local must avoid source parameters, source locals, surviving lambda parameters, and other generated locals. `NextId` is thread-local and reset per generation, which separates generated loops but does not itself prove avoidance of user-chosen `_lpN` or `x_N` names. Nested `_loopParamNames` changes need push/restore semantics, not unconditional removal of an outer binding that might be reused in a shared tree.
8. **Types.** Preserve the conditional's joined type, wrapper indices, `IReadOnlyList<T>` result boundary, and explicit coercions. Generic or unresolved element types must use the existing renderability refusal. Do not infer a narrower array type merely from one branch.
9. **Result order.** Do not reassociate floating-point arithmetic, reorder accumulation, lift allocations across iterations, fuse an additional stage, or introduce buffer reuse as an accidental part of scope lowering. Those are separate reviewed passes.
10. **Target constraints.** Generated C# must compile on net8.0 with its default language version. Keep this writer-local until another backend has a concrete need; shared core TIR changes widen the testing burden substantially.

## Ordered implementation chunks and anticipated files

Each chunk has its own tracker acceptance criteria, targeted tests and commit. The supervisor owns tracker mutations, commit scheduling, shared documentation and final gates; implementation agents receive exact fences.

### S0: nested statement bodies (bounded first implementation)

Files: `writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs`; new `tests/PlatoTests/Optimization/ScopedLoopLowererTests.cs`; new `tests/Plato.Interpreter.Tests/ScopedLoopLoweringTests.cs`; supervisor update to `docs/csharp-optimizer-pipeline.md`.

Factor existing top-level block traversal into recursion. In newly admitted scopes, lower only a direct combinator occupying the complete `TirLet` initializer or `TirReturn` value, with no nested argument prelude that can reorder earlier evaluations. Leave other expression forms unchanged until ordered prerequisites are implemented. Conditions, ternaries, callbacks, and assignment expressions retain current refusals. Retain original nodes when unchanged so flag-off and no-op behavior are straightforward to check. Start with unfused `MapRange` fixtures, so success does not accidentally depend on existing fusion behavior.

Acceptance: for an existing nested scope, the supported direct combinator disappears and a loop is emitted inside that exact scope; flag-off retains the call; code compiles and executes identically to interpreter and unoptimized C#; branch guards, zero outer iterations, per-iteration counts, local visibility, and nested name binding are exercised. No claim of B-spline allocation removal.

### S1: tail-return conditional lowering

Files: lowerer above, optionally a focused `Optimization/TirScopedLowering.cs` helper if complexity warrants it; same owned test fixtures. Existing writer should be enough because it emits `TirIf` and returns.

Support direct ternary return trees and expression-bodied functions containing them. Keep nested ordinary expression conditionals refused. Add minimal recursive de Boor-shaped fixtures using integer arrays, plus real B-spline law/mesh acceptance outside this repository when the supervisor schedules that consumer check. Ensure recursion and base cases remain unchanged; there should be loops only inside selected recursive arms.

### S2: callback-local preludes

Files: lowerer; new `Optimization/TirScopedValue.cs` only if the reviewed representation requires it; `TirCSharpBodyWriter.Loops.cs`; `TirCSharpBodyWriter.cs` for statement-aware callback emission and scoped name restoration; same targeted test fixtures. Capture rewriter should work structurally; modify it only if a reproducer proves the binder contract insufficient.

Support expression-bodied lambdas for all currently lowered combinators and fused source stages. Prepare callbacks after outer eligibility is determined. Add nested `MapRange`/`Reduce`, outer-variable capture, same-spelling parameters at multiple depths, surviving inner lambdas, callback delegates as parameters, and a nested callback in a producer stage. Do not admit arbitrary statement-bodied callbacks in this chunk.

### S3: general expression scopes and control conditions

Files: scoped lowering helper/markers plus body writer and fixtures. Introduce typed conditional-result locals and a systematic ordered expression-to-statements lowering result. A conditional with branch preludes becomes a typed local assigned in each branch. Reject unresolved joined types. While-condition lowering needs an explicit loop-with-condition-prelude form or `while (true)` plus condition test and break; do not just hoist the test. General statement-body callbacks require callback-local exit handling. This is a larger project, not part of the bounded first implementation.

## Tests and verification

Existing [LoopLowererArrayTypeTests](../../../tests/PlatoTests/Optimization/LoopLowererArrayTypeTests.cs) and [CombinatorFusionTests](../../../tests/PlatoTests/Optimization/CombinatorFusionTests.cs) mostly inspect emitted text. Keep useful shape checks, but emitted C# compilation and execution are required for this work. Existing [CSharpBackend.Build](../../../src/Plato.Laws/CSharpBackend.cs) already accepts `lowerLoops` and `fuse`; use the [ShortCircuitTests](../../../tests/Plato.Interpreter.Tests/ShortCircuitTests.cs) differential pattern. Run each applicable fixture with loops off, loops on/fuse off, and loops on/fuse on; do not use the default backend configuration and accidentally test only unoptimized output.

Required fixture groups, added as the relevant slice is implemented:

* Both arms of an `if`, an absent `else`, early returns, a nested block whose local is used later in the same block, and a false branch containing an invalid index in a callback.
* While loops with zero, one, and several iterations; an inner range size depending on the current outer counter; a condition that changes each iteration and remains evaluated at its original point; an assignment RHS that must stay unchanged while unsupported.
* Negative and zero range sizes with a faulting callback; one element; valid and invalid array access; distinguishable faulting sibling arguments to detect changed evaluation order.
* De Boor-shaped tail-return recursion with base case, multiple recursive steps, and an invalid discarded arm; nested ternaries representing derivative orders.
* Nested callbacks capturing function parameters, previous locals, outer lambda parameters and mutable loop-body locals; same spellings with distinct bindings; names resembling `_lp0`, `_i0`, and suffixed lambda names; a surviving nested closure invoked after its creator iteration.
* Nested producers before Reduce/All/Any/FlatMap, verifying any observable early termination and fault behavior against the **unfused** baseline, rather than assuming existing fusion is an oracle. Keep arithmetic association intact.
* Net8.0 compilation of emitted C# and no degraded body stubs. Compare actual values/faults in addition to textual loop placement. A host-only counting or throwing callback fixture can instrument invocation count without changing Plato's public purity contract.

Proposed scoped commands (not run): `dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter "FullyQualifiedName~ScopedLoopLowererTests|FullyQualifiedName~LoopLowererArrayTypeTests|FullyQualifiedName~CombinatorFusionTests"`; `dotnet test tests/Plato.Interpreter.Tests/Plato.Interpreter.Tests.csproj -c Release --filter "FullyQualifiedName~ScopedLoopLoweringTests|FullyQualifiedName~ShortCircuitTests"`. Builds whose output is reported use `tools/dotnet-build-record.ps1` with supervisor-owned snapshot/log outputs. The supervisor runs the required repository recorder and studio full gate once after writers stop; record input revisions, actual durations and any baseline failures.

## Benefits, measurement, and dependencies

Proven source fact: the present generated B-spline kernel leaves eager callbacks in the described scopes. Expected benefit: after S1/S2, their bodies become ordinary C# loop statements and local computations, avoiding delegate invocation and some captured-closure allocations, and exposing inner accesses to the JIT. No speedup magnitude is established. Small arrays still allocate, so do not report every eliminated callback as an eliminated array.

Measure isolated `HomogeneousPartial`, `LocalDerivative`, and a de Boor-shaped fixture with runtime rather than literal dimensions, to avoid measuring existing literal unrolling. Record warmed throughput, allocated bytes per evaluation, GC counts, generated code size and generation/compile time. Use fixed deterministic inputs and consume results. Then measure whole BREP meshing with identical model input, degree distribution, options, result checks and warm/cold protocol. Review allocation and speed separately: extra C# locals can increase code size while reducing delegate overhead. Existing profile percentages from another investigation are not a measured improvement.

Buffer reuse depends on explicit local loop scope and liveness; scope lowering does not establish unique ownership. Specialization may expose more constants and branches, but must run before this writer-local lowering unless it understands the new markers. Fusion may reduce inner intermediate arrays once nested callbacks become visible, but eager-fault equivalence needs an explicit decision. Concrete storage can optimize the new arrays later; keep list boundaries for now. These tracks touch the same lowerer/emitter files, so their implementation waves are sequential unless extracted helpers have agreed contracts.

## Alternatives and tradeoffs

* **Immediate general ANF/block-expression IR:** uniform handling of branches, loops and inlining, but much larger surface and more consumers. Prefer a late writer-local representation initially; do not add a shared-language node merely to save one emitter helper.
* **Immediately invoked block lambda (`Func<T>(() => { ... })()`):** easy C# syntax and preserves local guards, but can allocate/capture/invoke a delegate exactly where performance work seeks to remove one. Useful only as a correctness prototype, not the intended optimized output.
* **Generated local helper function for each scope:** avoids some delegate allocation, but requires capture/signature/type handling, may inhibit inlining and gives buffer analyses extra call boundaries. Not necessary for S0/S1.
* **Callback-body lowering first:** gives a direct `HomogeneousPartial` benefit, but current `FnOk`, inline expression emission, capture rewriting and parameter naming must change together. Risk is materially higher than recursive existing statement scopes.
* **Tail-return ternaries first:** attractive direct B-spline payoff with existing TIR. It still needs the same scope-recursive statement mechanism, so make that mechanism its own small verified chunk before adding normalization.

## Risks, unknowns, and findings for supervisor triage

1. **Existing argument-hoisting order risk (code inspected, no reproducer run).** `Lower(TirCall)` visits arguments and appends nested loop preludes immediately, without binding earlier ordinary argument expressions. `F(FaultA(), MapRange(...FaultB...))` can therefore fault in B before A. Scope expansion could expose this behavior in new places. File or locate a focused correctness issue; either repair ordered sequencing before broad expression support or conservatively refuse the problematic forms in newly entered scopes. Do not claim full eager-fault preservation solely from branch-local insertion.
2. **Existing Boolean-chain guard risk (code inspected, no reproducer run).** `Lower(TirBooleanChain)` uses one shared hoist list for every term, whereas its emitter prints `&&`/`||`. A nested loop in a later term can run before its guard. Ordinary Boolean operators commonly elaborate as `TirConditional`, so reachability through actual component-unroller output needs a reproducer. Treat this as an investigation finding, not a proven current user-facing defect.
3. **Existing producer-fusion fault risk (code inspected, no reproducer run).** A fused eager `MapRange` feeding `Any`/`All` can skip later producer elements that unoptimized eager code evaluates. More general Map→Map fusion changes stage ordering. This is a prerequisite semantic question for the fusion proposal, not permission for scoped lowering to weaken behavior.
4. **Closure capture semantics need an adversarial test.** The interpreter snapshots environments in [MakeClosure](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs), while capture-rewriter commentary treats locals as single-assignment and leaves local captures to C#. Source language now supports local reassignment. Determine whether a closure retained across reassignment is legal and differs today; do not silently widen this mismatch by moving closure creation.
5. **Identifier collisions are not proven absent.** Generated loop ids are unique among themselves, but source names can resemble emitted names unless front-end restrictions prohibit them. Check with a fixture before deciding whether a shared fresh-name allocator is a prerequisite or follow-up.
6. **Performance uncertainty.** The JIT may already remove some delegate overhead; additional local declarations alone do not guarantee throughput gains. Code size or register pressure may offset wins. Benchmark only after correctness gates pass.
7. **Shared-checkout risk.** Lowerer, loop emitter and optimizer documentation are likely shared touch points for all proposals. Adopt one file owner and a revisioned scope contract before writers begin. This proposal has made no code/tracker/git mutations and claims no completed implementation.
