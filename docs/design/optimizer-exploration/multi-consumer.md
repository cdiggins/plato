# Fuse independent reductions over the same materialized source

Exploration and ordered implementation plan for [compiler-506](../../../tracker/issues/compiler-506.md),
under [contract O1](wave.md). This proposes a compiler transformation; the executable prototype is
handwritten candidate C# using the real scalar runtime. It does not implement a Plato pass.
Measurements belong in the [dated report](../../reports/optimizer-multi-consumer-2026-10-25.md).

## Opportunity and existing boundaries

Compute independent folds in one traversal while keeping a separate accumulator for each fold.
For example, sum and sum of squares read the same materialized samples. A combined loop loads
each sample once and updates both accumulators in their original index order. Bounds and centroid
are potential callers, not specially recognized operations. This removes traversal and repeated
loads; it does not remove the input allocation or permit reassociation of floating arithmetic.

[TirFusion](../../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs) composes expression
chains; [TirLoopLowerer](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs)
lowers individual statements and constructs separate loop markers. Neither structure alone
provides the cross-statement dependence and source-provenance proof required here. The current
[loop emitter](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs) supplies the
closest target shape. The distinction from the existing
[fusion/filter proposal](../brep-compiler-optimization/fusion-filter.md) is combining sibling
consumers, not pulling a producer into a single consumer.

A concrete source candidate is `Eval(Clothoid2D, Number)` in
[curves-shapes.library.plato](../../../stdlib/geometry/curves-shapes.library.plato): adjacent
reductions calculate x and y from the same samples. Its calls, conversions and arithmetic must
be inspected before accepting it. Reusing its repeated tangent/weight computations is a separate
common-subexpression optimization and must not be credited to traversal fusion.

## Initial acceptance contract

Limit the first production slice to adjacent local bindings of independent full reductions in
the same block, over exactly the same resolved source binding. Require a previously materialized
compiler-owned array with stable length and contents and no escaping mutable alias. An
`IReadOnlyList<T>` static type is not sufficient: a host implementation can change its result
on each indexed read. Do not adopt a producer or fuse across intervening statements.

Require exact resolved `Reduce` identity, closed types, raw eligible callbacks, literal seeds
and callbacks whose arithmetic and conversions are independently proven total and terminating.
The candidate subset is wrapper `Number` addition and multiplication plus immutable scalar
reads. Unknown calls, executable coercions, indexing, division in other numeric domains,
mutable captures, recursion, foreign lists, early-exit consumers and cross-accumulator dependencies
refuse the rewrite. The second callback must not capture the first reduction's result.
Permit only seeds whose movement before the first loop cannot introduce observable work.

Keep the original source evaluation and every producer evaluation before the combined loop.
Preserve each accumulator's initialization, update grouping and final result conversion.
Both reductions traverse the complete identical index domain. Empty sources still return both
seeds. No vectorization, parallel fold, tree reduction or arithmetic commoning is implied.
The first implementation should cap grouping at a pair until code-size and register-pressure
measurements justify expansion. This is an implementation policy, not a language limitation.

## Critical review

Purity is insufficient: a failing second callback can overtake a later failure in the first
callback. Nontermination creates the analogous ordering problem. The executable counterexample
uses explicit exceptions as an observable stand-in for distinct domain faults; it is not itself
a Plato source program. Requiring total callbacks avoids this reorder rather than explaining it
away. Array provenance is also essential even when callbacks are total.

The scalar kernel is deliberately favorable. Many geometry kernels spend most of their time
in transcendental functions, so removing source reads may have little effect. Combining large
accumulators can increase register pressure or hinder independent vectorization by the JIT.
The input is already materialized in both variants; claim no allocation saving unless a later
separate transformation actually removes an allocation. Whole-application speed remains unknown.

## Execution plan and release gates

1. Execute the isolated runtime experiment, retain exact inputs and fingerprints, compare bitwise
   results including exceptional floating values, and reproduce unsafe counterexamples. This is
   the bounded step executed under O1, not approval of a production transform.
2. With compiler-file ownership assigned, add a read-only candidate census over resolved TIR.
   Identify adjacent reductions, reject dependency/capture/provenance gaps, and report reasons.
   Inspect actual generated C# for the Clothoid candidate. Do not alter emission in this milestone.
3. Review the narrow total-expression classifier and source-provenance proof. If the classifier
   accepts no real workload, retain the census and stop rather than widening eligibility implicitly.
4. Add a combined reduction marker and emitter under an explicit opt-in experiment flag. Keep the
   unfused tree as fallback and preserve binding identity. Schedule after recognition has enough
   provenance, before ordinary loop emission; do not infer semantics from printed function names.
5. Compile and execute actual Plato fixtures with the flag off/on, runtime wrappers and default
   net8 language level. Cover refusal shapes, empty inputs, conversions, NaNs/infinities/signed zero,
   captures, dependent seeds and callbacks, name shadowing and deterministic generated names.
6. Compare against already lowered separate loops, benchmark representative callers, and inspect
   generated/native code if results suggest JIT equivalence or regression. Refresh optimizer
   source evidence and guides via [stewardship](../../../tools/optimizer-docs/MAINTENANCE.md),
   run focused regressions and supervisor-owned integration gates before promotion.

Run the isolated experiment with
`python tools/experiments/optimizer-exploration/multi-consumer/run.py`.
The runner records builds through the repository error parser with private output paths because
the shared status reports belong to another session. Production integration remains outstanding.
