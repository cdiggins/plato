# Array length and index range analysis

Proposal under [compiler-508](../../../tracker/issues/compiler-508.md), governed by
[exploration contract O1](wave.md). The first execution step is an isolated arithmetic
prototype, not a production pass. Measurements are recorded in the
[dated report](../../reports/optimizer-array-facts-2026-10-25.md), not this design.

## Opportunity and existing work

Length facts can establish that an index lies within a particular logical array, simplify
already-evaluated length comparisons, and make safe iteration relationships visible to
later optimization. For example, a completed MapRange of a checked collection count has
length `max(0, count)`. A loop over `i < length - 1` may read `i + 1` if both arithmetic
and logical bounds are proved. Knowing an array's allocated capacity is insufficient.

The [pipeline reference](../../csharp-optimizer-pipeline.md) already describes loop-local
count caching, literal unrolling and producer fusion. This proposal adds reusable analysis
facts; it does not claim those existing optimizations as new. The
[concrete-storage proposal](../brep-compiler-optimization/concrete-storage.md) already
plans provenance for compiler-created backing arrays. Share its producer identity and
scope machinery when integrating; do not introduce a competing storage registry.

Source grounding:

- [Integer.cs](../../../src/Plato.Intrinsics/Integer.cs), `MapRange` and implicit conversion
  to `int`, check collection-width conversion before array construction. In cpu64, a very
  negative count outside the int domain faults before negative-count normalization.
- [ArrayIntrinsics.cs](../../../src/Plato.Intrinsics/ArrayIntrinsics.cs), `MapRange`, invokes
  callbacks eagerly. A length fact describes the result after successful evaluation; it
  does not authorize replacing a producer with its count and dropping callbacks.
- [TirCSharpBodyWriter.Loops.cs](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs),
  `WriteLoweredLoop`, already emits bounded `int` induction variables and normalized range
  counts. Straightforward loop bounds may already be eliminated by the JIT.
- [Tir.cs](../../../src/Plato.Compiler/Checking/Tir.cs) supplies binding identity and explicit
  coercion nodes. Facts must attach to identities and evaluation scopes, not source names.

## Proof contract

Represent integer values using closed intervals or unknown. Integer width comes from the
selected profile; collection counts and index conversions use the runtime collection width.
An addition transfer returns unknown if either operand or any possible result lies outside
the selected signed domain. Supporting modular interval arithmetic is a separate decision.
The first prototype only models addition, conversion, length normalization and access proofs.

An array fact records logical length, completed materialization, and stable compiler-owned
provenance. These are prerequisites supplied by a future TIR adapter, not properties that
the arithmetic model can infer. Do not grant stability to arbitrary IReadOnlyList inputs,
escaped mutable lists, unknown calls, or a recognizable method spelling. Refuse unknown
coercions; preserve every source evaluation and conversion in its original scope.

For independent length/index intervals, certify an access only when the index's lower bound
is nonnegative and its upper bound is below the length's lower bound and fits the collection
index domain. This is deliberately incomplete: correlated variable lengths require a future
symbolic relation such as `i < count(xs)` tied to the same stable producer. Empty loops
produce no access certificate; no vacuous proof may escape and validate an access elsewhere.

Facts never authorize unsafe memory primitives. Initial consumers should only report proven
sites. A later transformation must keep ordinary managed array indexing and demonstrate
that its emitted shape helps the JIT; a proof alone does not remove generated checks.

## Critical review

**Likely benefit is narrower than the original idea suggests.** Existing lowering already
has simple loop limits. A general analysis may add compiler cost without changing machine
code. First seek surviving stencil offsets, repeated bound guards and shape arithmetic;
require a workload census before implementing a general dataflow solver.

**Intervals lose relationships.** A dynamic loop index and dynamic length may each have
wide intervals while being perfectly safe together. An affine or symbolic domain could
recover that relation, but brings joins, dominance and loop reasoning. The initial model
does not pretend to solve this.

**Purity is not totality or host immutability.** Producers may fault or fail to terminate;
interface getters may represent mutable host data. Count simplification must retain eager
materialization. Stable storage facts need scope and escape analysis shared with compiler-487.

**Arithmetic is a semantic boundary.** Checked index narrowing differs from wrapper arithmetic.
Never normalize a negative wide count before the conversion that originally faults. Reject
unsupported operators, arithmetic overflow, target-typed conversion uncertainty and facts
crossing assignment, branch merges, captures or unknown calls until separately proven.

## Ordered execution plan

1. Implement the isolated interval model and exhaustive small-domain checks, including
   empty arrays, signed overflow, narrowing conversion order, logical bounds and eager faults.
   This is the released bounded step; its code is in
   [the experiment directory](../../../tools/experiments/optimizer-exploration/array-facts/analysis.py).
2. With shared-source ownership, implement a report-only adapter for already lowered,
   completed exact-array producers in a lexical block. Reuse concrete-storage provenance.
   Pin resolved operation identity and type, invalidate unknown escapes, and report refused
   sites. Test actual TIR identities and both numeric profiles without changing emitted C#.
3. Run a source census and inspect generated C# for selected consumer kernels. Measure whether
   reported facts expose work that existing lowering and JIT optimizations leave behind.
   Stop or deprioritize if no surviving opportunity is demonstrated.
4. Release one transformation only after review: preserve existing materialization, cache a
   proven stable logical count, and retain managed indexing. Compile and execute baseline and
   transformed output; compare values, callback traces, selected faults and empty behavior.
5. Measure warmed kernels and representative consumer workloads, including compile cost and
   generated-code size. Refresh Greenhouse evidence and optimizer documentation through
   [stewardship](../../../tools/optimizer-docs/MAINTENANCE.md), then run integration gates.

Steps after the prototype remain unimplemented. Promotion requires production ownership,
compiled differential tests and evidence of useful emitted changes; no speedup is inferred
from the arithmetic tests.
