# Reuse neighboring grid samples

Exploration tracked by [compiler-511](../../../tracker/issues/compiler-511.md), under
[contract O1](wave.md). This is a proposal and executable model, not an enabled compiler
optimization. Measurements live in the [dated report](../../reports/optimizer-grid-reuse-2026-10-25.md).

## Opportunity and existing work

Adjacent cells ask for overlapping lattice nodes. A compiler could retain samples that
will be used by the next cells, avoiding repeated expensive procedural evaluations while
keeping less storage than a fully sampled grid. This is useful only when the sampling
cost outweighs the cache lookup, validity tracking and storage cost.

[MarchingCubesCornerValue and MarchingCubesLattice](../../../stdlib/geometry/voxels.library.plato)
expose this pattern through a callback and integer lattice coordinates. The existing
[plato-432](../../../tracker/issues/plato-432.md) already owns changing procedural marching
cubes to sample a grid first. That library algorithm change is independent and likely
easier to deliver. This proposal neither implements nor closes that issue.

[TirLoopLowerer](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs)
represents supported array loops with sources, callbacks and fused stages. It does not
provide a general affine grid schedule or a proof of callback totality. Its documented
one-dimensional lowering boundary is a reason to begin with explicit rectangular
fixtures rather than assume current marching cubes can be recognized automatically.
[Materialization](../../optimizer/materialization.md) shares selected array values;
sample reuse instead shares equal callback applications across loop iterations. Existing
fusion and the host JIT must be included in future baselines.

## Legality contract

Recognize resolved callback identity, complete types and immutable captures, not a name
such as `Eval`. Cache lifetime belongs to one invocation with one captured environment.
Require a proved total deterministic callback on the complete accessed domain. Purity
alone supplies neither termination nor domain safety. The initial slice refuses unknown
callbacks, mutable external arrays and uncertain conversions.

Prove exact equality of repeated sample arguments and their evaluation. Integer lattice
identity is insufficient if the original expressions construct different floating-point
coordinates: reassociating origin and step arithmetic can change the sampled value.
Retain the original arithmetic and wrapper conversions. Any checked overflow or bounds
calculation remains in its original guard and order.

The initial schedule is a rectangular lexicographic cell traversal with a fixed corner
stencil. Retain the current and next sample rows, populate entries at first demand, and
evict a row only when the schedule proves it dead. No eager prefill, border expansion,
cross-call cache, reordered reduction, or speculative sampling is permitted. Empty
dimensions evaluate no sample. Keys require validity state separate from sample values,
so zero, NaN and every other valid value remain cacheable.

The reference model uses dictionary entries to make membership explicit. A generated
implementation should use private fixed row buffers with explicit validity state after
its size and overflow checks are proven. Extending this to spatial volumes requires a
separate slab-lifetime proof; it is not established by the planar model.

## Critical review

The strongest practical objection is proof availability. Proving totality for arbitrary
procedural geometry is much harder than recognizing nearby indices. A narrow accepted
set may miss the expensive callbacks that motivate this pass. If so, prefer an explicit
library sampling algorithm with its own contract over unverifiable compiler annotations.

Demand-driven caching preserves first-use order, but removes repeated calls. It cannot
preserve observations of stateful host callbacks; admitting one through an incorrectly
trusted certificate is a correctness bug. The model deliberately demonstrates this.
Even eager prefill of exactly the required nodes can change which fault is observed first.

Saving calls is not a runtime speedup measurement. Cheap backing-array reads are likely
poor candidates, and row storage can increase allocation and memory traffic. A dictionary
prototype establishes value reuse and liveness, not the cost of generated row buffers.
The decision should therefore remain opt-in until compiled workload evidence exists.

## Ordered execution plan

1. Implement a dependency-free model of demand-driven row reuse, compare values and first
   demands against repeated sampling, and test empty/boundary/refusal cases and unsafe
   counterexamples. This bounded step is executed in
   [prototype.py](../../../tools/experiments/optimizer-exploration/grid-reuse/prototype.py).
2. Capture actual typed and emitted source for a small rectangular Plato stencil under
   explicit baseline optimizer flags. Record whether the callback identity and schedule
   survive existing passes. Stop if they cannot be recovered without broadening semantics.
3. Specify a conservative proof producer for exact arguments, immutable capture identity,
   totality and index bounds. Add accepted/refused typed fixtures before any emitter edits.
   Coordinate with array-facts work; do not depend on its unreviewed prototype.
4. Obtain ownership of the relevant compiler files. Implement a bounded planar matcher and
   private row-buffer emission. Preserve fallback and guards. Follow
   [optimizer stewardship](../../../tools/optimizer-docs/MAINTENANCE.md), refresh evidence,
   review affected guides and run documentation freshness checks.
5. Compile and execute real generated C# against baseline for wrappers, conversions,
   signed zero/NaN, empty shapes, boundary overflow, captures and rejected callbacks.
   Run warmed paired workloads with outputs consumed, allocations measured and runtime
   identity recorded. Compare expensive procedural samples and cheap array reads.
6. Decide whether benefits justify shipping the narrow slice. Only then consider spatial
   slabs or marching-cubes adoption, coordinated with plato-432, followed by required
   integrated gates. Do not enable a broader rewrite on call-count evidence alone.
