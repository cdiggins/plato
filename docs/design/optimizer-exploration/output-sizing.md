# Exact destination allocation for fixed-output FlatMap

Exploration under [compiler-509](../../../tracker/issues/compiler-509.md), governed by
[contract O1](wave.md). The bounded experiment has been executed; production integration
is a separate, unimplemented step. Measurements live in the
[dated experiment report](../../reports/optimizer-output-sizing-2026-10-25.md).

## Opportunity and source evidence

`src/Plato.Intrinsics/ArrayIntrinsics.cs`, `FlatMap`, eagerly invokes each callback and
copies its complete result to a growing `List<TResult>`. The FlatMap case in
`writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs` also grows a List. It already calls
`TryWriteFlatMapConditional`; the older [BREP fusion proposal](../brep-compiler-optimization/fusion-filter.md)
describes conditional singleton elimination, which should not be claimed as new work here.

For a callback returning an exact-typed raw array literal of invariant width, the final
length is source length multiplied by literal width. Allocate that final destination once
and write consecutive elements into it. The initial version should retain callback arrays;
this isolates result storage savings from a later literal-element append optimization.
The C# boundary remains `IReadOnlyList<T>` and production scalar wrappers remain intact.

## Eligibility and legality

Match only an already lowered, trusted intrinsic FlatMap with a materialized, immutable,
compiler-owned source whose Count and indexing have their expected semantics. Require an
expression-bodied lambda whose body is a raw `TirArray`, with exact concrete element and
result types. Width comes from `TirArray.Elements.Count`, never from evaluating a callback
or inspecting a sample result. Refuse coercions, unknown callbacks, branches with varying
width, arbitrary host lists, and fused producer chains in the first slice. Do not use a
name-only operation match or strip conversions to make a pattern fit.

Preserve source evaluation and callback binding order. Evaluate each callback once per
source element, including when the proved width is zero; preserve literal element order,
the first fault and termination behavior. Empty output does not prove an empty computation.
Resource-exhaustion timing changes when allocating the destination early; this needs the
explicit allocation-optimization exception described in the [BREP contract](../brep-compiler-optimization-plan.md).
No general permission to reorder defined faults follows from that exception.

Compute capacity in widened host arithmetic. If it exceeds the supported array limit,
retain the original grow-list path. A checked multiplication that throws before the first
callback is incorrect when that callback would otherwise fault first. The runtime guard
must choose the fallback without repeating source/callback argument evaluation. Do not
add speculative callback count checks that change production behavior: eligibility must
establish width before emission.

## Critical review

This is narrower than general output-size analysis. Conditional filtering has unknown
output size; a counting pass can duplicate expensive or faulting callbacks. Equal-width
conditional arms could be a later extension only after preserving branch evaluation.
Removing callback arrays is also separate: retaining eager temporary construction makes
the first change easier to review and measure.

Exact storage trades List growth and slack for one array, but keeps all callback work and
callback allocations. Expensive geometry callbacks may dominate. The JIT already optimizes
some loop/delegate work, and capacity reservation on a List is a useful competing design
to benchmark before choosing production output shape. Host clients that cast results to
List are outside the declared `IReadOnlyList<T>` contract; verify no supported consumer
depends on that concrete type. Claims of end-to-end speedup require real generated code.

## Ordered execution plan

1. Completed bounded step: run the [standalone experiment](../../../tools/experiments/optimizer-exploration/output-sizing/run.py).
   It extracts the actual runtime FlatMap body, compares exact storage while keeping callback
   arrays, and exercises order, faults, empty cases, unsafe width assumptions and overflow fallback.
2. Inspect dumped TIR from a real fixed-output Plato fixture and representative geometry
   workloads. Record whether raw arrays survive inlining/coercion and whether operation
   provenance and source ownership are sufficient. Stop integration if these proofs are absent.
3. After shared-file ownership transfer, implement a focused literal-width matcher and
   exact destination branch in the loop emitter; keep fallback and runtime unchanged.
   Add optimized/unoptimized compiled tests with actual wrapper types, conversions,
   direct and refused shapes, and same-name non-intrinsic calls.
4. Compare exact arrays, reserved List capacity and the existing grow-list emitter in
   warmed paired generated-code runs. Check allocation trends and realistic callback cost;
   retain the implementation only if eligible workload benefit warrants code complexity.
5. Follow [optimizer stewardship](../../../tools/optimizer-docs/MAINTENANCE.md), refresh
   affected source evidence and guides, run freshness and relevant optimizer regressions,
   then supervisor-owned final gates. Production integration remains unstarted until this
   execution plan receives its own owned implementation chunk.
