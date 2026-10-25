# Multiple-consumer fusion

[Overview](README.md) · [Pipeline](../csharp-optimizer-pipeline.md) · [Loops](loops.md)

`--loops --fuse-consumers` combines adjacent independent reductions over the same local
Number array. It is disabled by default. The producer still finishes before either fold;
each accumulator retains its original left-to-right arithmetic order. This saves a source
traversal without reassociating floating-point operations or recomputing the producer.

[TirMultiConsumerFusion](../../writers/Plato.CSharpWriter/Optimization/TirMultiConsumerFusion.cs)
requires a top-level contiguous sequence: an array binding, then two reduction bindings.
The source must be a raw array literal or the canonical intrinsic MapRange call. Both folds
must have Number literal seeds and raw lambdas made only from Number literals, immutable
parameters, and canonical Number Add/Multiply calls. Those operations cannot throw ordinary
arithmetic exceptions, so interleaving them preserves observable behavior under the runtime
contract. Resource exhaustion is not an equivalence guarantee.

Resolved intrinsic identity and exact instantiated types are checked, rather than trusting
method spelling. Assignments, foreign collections, intervening statements, dependent folds,
unknown calls, conversions and broader numerical functions are refused. Purity alone does
not justify changing fault order. These restrictions deliberately limit the first production
slice; the broader candidate census does not authorize widening them.

The pass runs after component expansion and before materialization and loop lowering.
[TirMultiReduction](../../writers/Plato.CSharpWriter/Optimization/TirMultiReduction.cs) carries
the original bindings through capture rewriting. Its children remain traversable by the
shared rewrite infrastructure. A dedicated emitter writes both accumulators in one loop,
reserving generated names against declarations, including unused lambda parameters.

[MultiConsumerFusionTests](../../tests/PlatoTests/Optimization/MultiConsumerFusionTests.cs)
compile actual Plato fixtures and emitted C#, compare the separate and combined folds
bitwise, exercise producer faults, refusals, generated names and optimizer interactions.
The [dated report](../reports/optimizer-multi-consumer-2026-10-25.md) separates prototype
measurements from production validation. Fewer loops alone do not establish a runtime speedup.
