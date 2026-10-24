# Fusion: combine adjacent collection operations

[Overview](README.md) · [Literal expansion](literal-expansion.md) · [Loop lowering](loops.md)

## Idea and motivation

An eager `xs.Map(g).Map(f)` constructs an intermediate array. Composing the callbacks into
`xs.Map(x => f(g(x)))` removes that intermediate collection. Similarly, feeding a mapped value
directly into a fold can avoid producing a collection merely to consume it. This is related to
[deforestation and stream fusion](references.md#fusion-and-lowering); Plato implements local
tree patterns, not a general stream intermediate representation.

Geometry algorithms often express small transformations as chains. The pass keeps that source
vocabulary useful without requiring authors to write an imperative loop for each chain.

## Implementation and boundaries

[TirFusion](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs) traverses children first,
then applies bounded local rewrites. `RangeToMapRange` exposes an index producer;
`TakeToMapRange` exposes a prefix of a suitable source. `ComposeMaps` combines maps, and
`FuseMapIntoConsumer` embeds a map into supported `Reduce`, `All`, `Any` or `FlatMap` callbacks.
`FusionContext` constructs the typed calls and fresh parameters those rules need.

Composition checks callback shape and parameter usage. A nontrivial expression cannot be
substituted where it would be evaluated repeatedly or captured unsafely. Rejected compositions
can still reach [loop lowering](loops.md), which stores stage results in locals. Fusion follows
expression chains, not the definitions of earlier local bindings. Small literal inputs are
handled by the separate [literal-expansion helper](literal-expansion.md).

The rewrites do not hoist statements, so they can run in lambdas and conditional expressions.
That structural property does not prove that fusion preserves all eager fault and termination
behavior: a short-circuit consumer may stop before a producer previously completed. `Take`
also has a source-domain precondition. See the [pipeline runtime contract](../csharp-optimizer-pipeline.md)
and [sequencing investigation](../../tracker/issues/compiler-483.md).

## Usage, tests and effectiveness

Enable `--fuse`, usually with `--inline` when helpers hide chains and `--loops` when an explicit
dynamic loop is wanted. Fusion alone may leave a runtime `MapRange` and its result allocation.
It is not synonymous with loop lowering.

[CombinatorFusionTests](../../tests/PlatoTests/Optimization/CombinatorFusionTests.cs) checks
generated loop/array shapes for normalized ranges and map chains, including a reused mapped
value and the flag-off case; [LiteralUnrollingTests](../../tests/PlatoTests/Optimization/LiteralUnrollingTests.cs)
covers its literal helper. The asserted disappearance of intermediate arrays or combinator calls
establishes a generated-code benefit. For runtime claims, compare identical inputs and outputs
using the [evaluation procedure](evaluation.md); include early exits and fault-sensitive inputs.
