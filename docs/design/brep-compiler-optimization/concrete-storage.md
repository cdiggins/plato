# Proposal 5: concrete local storage and surviving callbacks

Reviewed proposal for compiler-481. The [combined plan](../brep-compiler-optimization-plan.md) governs implementation order and the released first slice. This document states design and proof obligations; measurements belong in validation logs.

## Incorporated critical review

CR-09 corrects slice A's eligibility/example mismatch. Before implementation, count eligible surviving direct producer uses and inspect a fixture such as separately lowered MapRange sources feeding Zip. A multi-consumed TirVariable local requires alias propagation deferred to slice B, so it is not an acceptance example for direct-TirTempRef slice A. If no relevant shape survives existing fusion, stop this experiment or explicitly review alias propagation as a new scope. Keep ordinary interface locals and all call boundaries; only the compiler-generated length/index pair may use proven backing storage. This removes no allocations and needs JIT/benchmark evidence before priority increases.

CR-05/10 makes callback specialization depend on the later shared clone registry from compiler-486, not merely its initial statement-inlining stage. Start known noncapturing callees; reject assignable captures until compiler-483 establishes their operational contract. Do not create a second registry, broaden dispatch by name, or introduce struct functors/spans in the first experiment. CR-11 requires exact target compilation and a controlled comparison. Compiler-487 holds this future work; F1 and restricted scoped lowering take precedence.

## Recommendation

Treat storage and callbacks as separate, measurable steps. The smallest credible storage change preserves every logical `IReadOnlyList<T>` value and signature, but lets a subsequent generated loop read a known, already allocated backing array directly. It neither removes an allocation nor introduces mutable shared storage. Implement callback specialization only after the scoped lowering and bounded specialization work supplies the needed scope and evaluation-order machinery. Prefer private static specializations with explicit capture arguments over a new generic struct-functor runtime.

This is an enabling optimization with unmeasured benefit, not an established major B-spline speedup. The dominant B-spline arrays cross function boundaries, and nested MapRange callbacks remain; merely retaining local array information cannot eliminate those costs.

## Evidence checked in this checkout

- `writers/Plato.CSharpWriter/CSharpTypeWriter.cs:584-614` maps FunctionN to `System.Func` and Array/IArray to `IReadOnlyList`. These are the language value representations throughout fields, parameters, returns, and generic calls. Array2D/Array3D are ordinary generated structs, not equivalent array storage candidates.
- `writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs:18-37` creates source locals with `var`, reads `.Count`, creates a physical `T[]`, and deliberately binds its result to an `IReadOnlyList<T>` local. That upcast copies a reference; it does not allocate a wrapper. FlatMap instead owns a `List<T>` and upcasts it. The loop emitter already inlines eligible lambda literals, so callback removal must target what survives this existing pass.
- The same emitter explains the historical failures explicitly: using a T[] result temporary caused CS0266 and CS8917 in TKT-505; Reduce seeds, FlatMap results, and conditional joins depended on the original interface type. Reduce now declares its accumulator using the call's result type, not inference from the seed.
- The sibling record `../plato-brep/tickets/TKT-505-plato-loop-lowerer-array-type.md` confirms the repair and historical whole-mesher results. Those results also caution that fewer allocated bytes did not guarantee faster whole-mesher execution. This proposal does not rerun or adopt those timings as an expected speedup.
- `TirCSharpBodyWriter.cs:371-490` prints local bindings using `var`, applies target typing for delegates, invokes function values via `.Invoke`, and leaves solved widening coercions largely implicit in C#. Altering a local's physical C# type can therefore change later overload selection even when its Plato type is unchanged.
- `Optimization/TirLoopLowerer.cs:11-66` represents lowered producers as `TirLoweredLoop` and results as `TirTempRef`, making a local producer/result provenance relation feasible. `WithChildren` must preserve any added metadata.
- `src/Plato.Compiler/Checking/TirLambdaCaptureRewriter.cs` hoists enclosing-function parameter references to snapshot locals, by reference-node identity; it deliberately does not hoist local variables or outer-lambda parameters outside their scope. Repeated references may produce repeated copies of large structs. That is evidence of emitted copies, not proof that all copies survive JIT optimization or that collapsing them is semantically valid.
- `Optimization/TirInliner.cs:159-256` already supports some lambda argument substitution and beta reduction, with limits on consuming positions, nested captures, body size, and result conversions. A new callback pass must extend/reuse this logic rather than repeat it under a second inconsistent set of rules.
- Generated BREP `BsplineEvaluation.g.cs:205-244` allocates an outer HomogeneousPoint3D array, calls a capturing MapRange inside each loop iteration, aliases the array as IReadOnlyList, and passes it to LocalDerivative. Concrete local storage alone does not make that callee concrete. `Extensions.g.cs:1962-2037` shows recursive DeBoorLevels/LocalDerivative crossing interface-array parameters. `BrepTopology.g.cs:820-852` shows two lowered FlatMap loops where the second reads the first list through its interface alias; that is relevant evidence for a later List extension.
- Existing executable emission test pattern: `tests/PlatoTests/Emission/LambdaReceiverEmissionTests.cs` compiles generated C# with Roslyn and invokes it; it specifically protects lambda receiver target typing. `Optimization/LoopLowererArrayTypeTests.cs` and `CombinatorFusionTests.cs` currently pin useful output shapes but are not substitutes for compilation and execution tests.
- `tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md` requires Number/Integer/etc. wrappers. This proposal retains them. The `plato-386` issue already tracks FlatMap returning a mutable List through IReadOnlyList; this optimization must not assume that an arbitrary interface array is physically immutable or globally unaliased.

## Storage mechanism

Keep semantic types separate from local storage facts. A storage fact records an exact producer identity, exact C# element type, backing-local identity, storage kind, and valid lexical scope. It is not a new Plato type or a global change to `ToCSharpTypeName`.

Illustrative current code, after existing lowering:

```csharp
var backing = new Number[n];
// Fill backing in the original evaluation order.
IReadOnlyList<Number> value = backing;
var source = value;
var count = source.Count;
for (var i = 0; i < count; i++) Consume(source[i]);
ReturnOrCall(value);
```

Proposed local read path:

```csharp
var backing = new Number[n];
// Identical fill and logical value.
IReadOnlyList<Number> value = backing;
var source = backing; // Only this compiler-generated loop source changes.
var count = source.Length;
for (var i = 0; i < count; i++) Consume(source[i]);
ReturnOrCall(value); // All general language uses retain the semantic type.
```

1. After loop lowering, identify array-producing lowered loops and their corresponding `TirTempRef` uses. Initially accept only exact-element-type results feeding subsequent compiler-generated loops in the same lexical block with no unknown intervening operation. Do not infer provenance from a name prefix, pretty-printed type, or arbitrary caller-supplied array.
2. Record the backing local in writer-local state or an explicit emission annotation. Prefer a small, separately testable provenance component over duplicating recognition in Count, indexing, calls, and declarations. Its facts must remain valid through capture rewriting and `WithChildren` reconstruction. Never keep facts in static mutable state.
3. `WriteLoweredLoop` uses backing storage only for its source count/index expressions, selecting `.Length` for exact arrays. All other expression emission sees the existing interface temporary. No changes to generic inference, constructor fields, returns, Reduce seed typing, lambda target typing, or extension-method receivers.
4. Subsequent slices may propagate facts through direct single-assignment `TirLet` aliases and support compiler-owned List results after construction completes. Invalidate conservatively at assignments, conditional joins, scope exits, unknown calls, and capture escape. These restrictions can later be relaxed by explicit proofs; they are not blanket claims about all arrays.
5. Do not replace arbitrary `At` calls initially. In-bounds reads generated by the loop are straightforward; unchecked accesses require preserving invalid-index behavior as well as values. Do not hoist source evaluation or length observation across a guard, callback, or potentially faulting expression.

This keeps existing allocation and eager enumeration intact. The potential benefit is giving C# and the JIT direct length/index operations and an obvious bounds-check relation. A same-method interface alias may already be devirtualized; in that case the optimization has no runtime benefit. The proposal makes no claim that every interface read dispatches at runtime.

### Why changing the local type globally is unsafe

An interface result from a Reduce step cannot be assigned back to an accumulator inferred as T[]. A conditional may combine an array with another IReadOnlyList implementation. A FlatMap lambda returning arrays can change inferred delegate result types and overload candidates. Existing array wrappers are structs; reference-type covariance does not imply `Number[]` is convertible to an array of another wrapper or to `object[]`. Conversely, reference arrays do have covariant runtime behavior, so physical type checks must not be generalized through a semantic widening. Preserve the exact element type before using a backing reference.

Even when both overloads compile, changing the receiver's static C# type may select a different extension or generic instantiation. Preserving logical values at all ordinary call boundaries avoids that class of miscompilation. Any later private helper specialization must be keyed by the resolved symbol, complete generic substitution, result type, and physical storage signature; name-based overload discovery is insufficient.

## Callback mechanism

### Preferred route: bounded static specialization

A visible call such as `Minimize(t => curve.Eval(g, t), domain)` can become a call to a private specialization whose extra parameters are the captured curve and geometry values. Its loop invokes the resolved Eval body directly. The public/general Minimize(Func<...>, ...) remains available. An entirely noncapturing known callback is the first eligible slice. This can remove an indirect invocation even where a noncapturing delegate is already cached; removing allocation is not its guaranteed benefit.

Implement as a shared specialization facility with proposal 3, not a second clone generator. Clone the selected callee TIR only when every callback use is a known invocation or a further approved specialization; alpha-rename definitions, bind ordinary arguments and captured values once in original evaluation order, replace `TirInvoke` with the typed callback body/call, and preserve parameter/return conversions. Bound per-callee and per-program code growth and recursion. Keep a fallback and an optimization report giving rejection reasons.

Capturing callbacks are a second slice. Capture explicit values at the same lexical point as the original closure construction. A captured array remains the same reference, not a copied array. Do not snapshot an element read or expression earlier than it originally occurs. A lambda created in a loop must receive that iteration's bindings; an escaping lambda must retain the correct lifetime. A closure that captures an assignable generated local may observe later updates, so passing a value snapshot is invalid without proof. Reject builder/unique captures, callbacks stored in structs or returned, unknown callback forwarding, and unresolved generic targets initially. Existing `TirLambdaCaptureRewriter` behavior must be honored; bypassing it demands tests against the actual baseline, not an assumption that purity makes every capture interchangeable.

An example is illustrative only: the exact BREP inverse/minimization call should be selected from a recorded profile after scoped lowering removes callbacks it can already handle. Avoid hand-specializing functions by names such as Minimize or BSpline.

### Alternative: generic struct functors

A readonly generated capture struct with `Invoke`, passed as `TFn where TFn : struct, ICallable<...>`, can let a generic loop invoke it without boxing when the call stays constrained to TFn. It avoids one helper clone per call-site body in source, but generates native generic instantiations, adds interfaces/helper signatures, and copies captured structs by value. Storing it as ICallable or converting it to Func reintroduces boxing or delegate allocation. Large BSplineSurfaceData/geometry captures may be expensive to copy; adding `in`/ref parameters creates further ABI and alias constraints. An escaping language Function value still needs a delegate-compatible representation.

Therefore do not make functors the first implementation. Compare them only if static specialization produces unacceptable code size or cannot share important kernels. Neither approach implies that the JIT will inline a large callback body.

### Span is not the first representation

Span/ReadOnlySpan is not a drop-in substitute for IReadOnlyList and cannot be captured in normal delegate closures or stored in ordinary generated structs under the net8 default language target. Generic T may contain references, so a universal stack allocation is invalid. Returning stack-backed spans is invalid. Retaining exact heap arrays locally gets most of the representation experiment without a new lifetime system. Stack storage belongs to the separate bounded-buffer/escape-analysis proposal, with an explicit unmanaged proof, budget, and fallback.

## Implementation boundaries and dependencies

Storage slice A is independent of semantic fusion: it preserves every materialization and can be evaluated using existing top-level lowered loops. Proposed files: a new `writers/Plato.CSharpWriter/Optimization/TirLocalStorageFacts.cs`, a small change to `TirCSharpBodyWriter.Loops.cs`, and only if needed a narrowly scoped annotation in `Optimization/TirLoopLowerer.cs`. Tests belong in a new `tests/PlatoTests/Optimization/ConcreteStorageCodegenTests.cs`; preserve the existing array/fusion tests. Confirm exact ownership with the supervisor because the scoped-lowering implementer necessarily touches the same emitter.

Storage slice B (aliases and nested scopes) depends on slice A and the lexical-block design in proposal 2. Cross-function concrete-array helpers depend on proposal 3's bounded specialization registry. Storage reuse additionally depends on proposal 1's escape/liveness proof; physical-array knowledge alone never authorizes mutation.

Callback slice A (known noncapturing callable) depends on proposal 3's symbol-identified clone registry, exact conversions, argument bindings, and code-size budget. Callback slice B (captures) also depends on proposal 2's scoped statement lowering and capture-lifetime tests. Proposal 4 fusion may eliminate some candidate producers/callbacks before this pass runs; use the post-fusion tree and measure remaining cases. Do not attempt simultaneous independent edits to the inliner, capture rewriter, and loop emitter without shared contracts.

No runtime API change, scalar erasure, fast-math, cross-query cache, public ABI change, or broad intrinsic-overload addition is proposed.

## Tests and verification plan

For storage A, first confirm a surviving direct-TirTempRef fixture, such as separately lowered MapRange sources feeding Zip, then compile and execute it with Roslyn. Multi-consumed local fixtures belong to storage B, whose alias propagation can reach their producers. Assert identical results with and without the optimization, the presence of a direct backing-array source for eligible loops, and unchanged interface signatures. Include empty and negative MapRange counts, wrapper Integer index behavior, Reduce with an array seed and interface-returning step, conditional array/interface joins, FlatMap returning an interface array, nested element arrays, implicit element conversions, and same-name overloads. These are concrete TKT-505 regression protections, not just textual snapshots.

For fallback, use caller-supplied arrays and custom IReadOnlyList implementations with observable Count/index logs. Ensure the optimization does not replace or hoist those operations. Include nested scopes/shadowed names, a branch whose unselected side would fail, and sequential potentially failing producers to detect changed fault order. Do not broaden eager fusion while testing storage.

For callbacks, compile/run fixtures covering noncapturing callbacks, struct and reference captures, nested lambdas, outer-lambda iteration variables, callback reuse, callbacks retained/returned (must fall back), callback result conversion before member navigation, receiver overloads, and zero-element loops where callbacks must not run. Verify generic constraints and actual generated method selection, not only output text. Compare allocation per invocation only after warmup and account separately for JIT delegate caching.

Focused existing tests: LoopLowererArrayTypeTests, CombinatorFusionTests, LambdaReceiverEmissionTests, FunctionConstraintCodegenTests, TypeConstraintCodegenTests, and TirRewriteTests. Run relevant generated compile-and-execute fixtures and interpreter/C# differential laws. Use the repo's build recording script for reported C# builds; coordinate shared outputs through the supervisor. Once stable, regenerate a pinned BREP recipe, compile, run spline laws plus mesh digests, and run the supervisor's end-of-mission gates once. No such checks were run for this design.

## Benefit measurements and falsification

Use the exact same pinned input, runtime, CPU profile, flags, and harness between arms. Compare `--loops --fuse` baseline against one new storage change with all other proposals disabled, then repeat on the combined implementation. Separate cold/JIT time from warmed kernel time; inspect generated machine code when claiming dispatch/bounds-check elimination. Record code size, generation/build time, allocated bytes, Gen0 collections, and warmed kernel/whole-mesher timing. Keep arithmetic and evaluation order fixed and compare values, law outcomes, and mesh digests.

Storage A should have unchanged allocation volume. If the baseline JIT already removes interface dispatch and produces equivalent loops, or eligible same-scope producer/consumer pairs are rare after fusion, that falsifies its claimed speed benefit; do not inflate it into a broad ABI rewrite. A callback experiment is not a win if allocations fall but code growth/instruction-cache cost or capture copies slow the workload. A microbenchmark gain that disappears in representative BREP cases is insufficient grounds for aggressive default specialization. Re-evaluate the priority after the higher-value allocation and nested-lowering changes land.

## Review questions and unresolved risks

1. Can provenance survive the capture rewrite without relying on global temp-name strings? Prefer identities; verify marker reconstruction and lexical dominance explicitly.
2. Does the smallest storage slice encounter enough surviving direct producer/consumer pairs to matter? A cheap static census can answer this before implementation, but it is not a substitute for measurement.
3. Which callback targets remain hot after scoped lowering and existing inlining? Generated presence alone does not prove runtime cost.
4. Where is evaluation/fault order normative versus merely current runtime behavior? Until resolved, preserve the current eager sequence and do not rely on purity to delete or reorder faulting calls.
5. Does the chosen runtime already devirtualize and inline these calls? This is deliberately a benchmark/JIT-inspection unknown, not an asserted defect in the compiler.
6. Can the specialization registry reuse a clone across capture values without accidentally using value identity as a cache key? It should specialize shape/types/known target, pass runtime capture values as arguments, and cap recursion/code growth.
7. Existing FlatMap mutability leakage is tracked as plato-386 and is out of scope. The issue contains an old MapRange implementation description; notify documentation owner rather than quietly treating it as current evidence.

No new unrelated defect was confirmed. Historical compile failures and the existing mutability issue constrain this proposal; they do not authorize unrelated fixes. The supervisor owns durable tracker creation and final implementation selection after review.
