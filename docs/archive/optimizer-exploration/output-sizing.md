> **EXECUTED 2026-10-25** — see [compiler-509](../../../tracker/issues/compiler-509.md). Historical record; do not execute.
> The bounded implementation and [geometry qualification](../../reports/optimizer-geometry-exact-output-2026-10-25.md) are complete. Unexecuted broader comparisons below remain proposals, not approved follow-up work.

# Exact destination allocation for fixed-output FlatMap

Work under [compiler-509](../../../tracker/issues/compiler-509.md), governed by
[the coordinated wave](../../design/optimizer-exploration/wave.md). The standalone experiment and real-TIR preflight led to
the bounded implementation described here. Measurements and verification status live in the
[dated experiment report](../../reports/optimizer-output-sizing-2026-10-25.md).

## Opportunity and source evidence

`src/Plato.Intrinsics/ArrayIntrinsics.cs`, `FlatMap`, eagerly invokes each callback and
copies its complete result to a growing `List<TResult>`. The FlatMap case in
`writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs` also grows a List. It already calls
`TryWriteFlatMapConditional`; the older [BREP fusion proposal](../../design/brep-compiler-optimization/fusion-filter.md)
describes conditional singleton elimination, which should not be claimed as new work here.

For a callback returning an exact-typed raw array literal of invariant width, the final
length is source length multiplied by literal width. Allocate that final destination once
and write consecutive elements into it. The initial version retains callback arrays;
this isolates result storage savings from a later literal-element append optimization.
The C# boundary remains `IReadOnlyList<T>` and production scalar wrappers remain intact.

## Eligibility and legality

`TirExactOutput.TryGetWidth` matches only an already lowered, trusted intrinsic FlatMap.
`TirLoweredLoop.SourceFunction` retains resolved callee identity across rebuilding;
`TirIntrinsicIdentity` checks reference membership in the compilation's intrinsic library.
The default-off `--exact-output` switch requires `--loops`; library callers use
`CSharpWriter.ExactOutputAllocation` together with loop lowering. Require an
expression-bodied lambda whose body is a raw `TirArray`, with exact concrete element and
result types. Width comes from `TirArray.Elements.Count`, never from evaluating a callback
or inspecting a sample result. Refuse wrapped callback/array shapes, unknown callbacks,
branches with varying width and fused producer chains. Element conversions remain in their
original literal context. `MaximumCallbackNodes` bounds duplicated callback code because
the exact and fallback branches both render the body. Do not use a
name-only operation match or strip conversions to make a pattern fit.

The emitter selects exact storage at runtime only when the already evaluated source is a
concrete element array. Arbitrary host lists retain the grow-list path. The array guard
proves stable length, not immutability: callbacks may mutate later source values, and
the same ordered reads observe those mutations. No ownership proof is needed because
the transformation neither stores into nor reuses the source storage.

Preserve source evaluation and callback binding order. Evaluate each callback once per
source element, including when the proved width is zero; preserve literal element order,
the first fault and termination behavior. Empty output does not prove an empty computation.
Resource-exhaustion timing changes when allocating the destination early; this needs the
explicit allocation-optimization exception described in the [BREP contract](../../design/brep-compiler-optimization-plan.md).
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
2. Completed real-TIR preflight: [probe.py](../../../tools/experiments/optimizer-exploration/output-sizing/probe.py)
   compiles a Plato fixture against foundation and inspects raw and lowered bodies using
   existing compiled assemblies. Literal arrays and canonical provenance survive capture
   rewriting. Representative geometry coverage remains a future measurement.
3. Implemented bounded matcher and emitter in `Optimization/TirExactOutput.cs` and
   `TirCSharpBodyWriter.ExactOutput.cs`. `ExactOutputTests` compile flag-off/on generated
   code against the actual wrapper runtime, cover ordering and fallback, and measure
   allocation. The dated report records which checks passed, rather than this design.
4. Compare exact arrays, reserved List capacity and the existing grow-list emitter in
   warmed paired generated-code runs. Check allocation trends and realistic callback cost;
   retain the implementation only if eligible workload benefit warrants code complexity.
5. Follow [optimizer stewardship](../../../tools/optimizer-docs/MAINTENANCE.md), refresh
   affected source evidence and guides, run freshness and relevant optimizer regressions,
   then supervisor-owned final gates. Broadening to equal-width branches, fused producers
   or callback-array elimination requires separate proof and tests.
