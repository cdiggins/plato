# Rewrite infrastructure: preserve the tree contract

[Overview](README.md) · [Inlining](inlining.md) · [Source map](source-map.md)

## Idea and motivation

Optimization rules need a consistent way to visit children, rebuild nodes and substitute resolved
parameters. Centralizing these operations prevents each pass from implementing its own incomplete
tree traversal. [TirRewrite](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs) is the
shared implementation. [MLIR's pattern rewriter](references.md#rewriting-infrastructure) addresses
the same separation between traversal and rules, with a much broader infrastructure.

## Contracts

`Rewrite` visits each node through its `Children` and `WithChildren` contract, then applies the
supplied rule. It preserves the original node when its children and the rule leave it unchanged,
so identity can signal structural progress without printing the tree. New marker types supply
their own rebuilding behavior rather than requiring another visitor switch. Enter/exit hooks
let a caller track scope such as lambda depth. Marker nodes introduced by earlier optimizations
must remain traversable by later passes. `Substitute` replaces references keyed by resolved
`ParameterDef` objects, rather than performing text replacement. `TryBetaReduce` applies a lambda
when its argument-use checks allow substitution.

Substitution itself does not alpha-rename every binding. The caller owns capture and naming
constraints; the inliner supplies its own lambda-parameter renaming. `IsCheap`, coercion handling,
list recognition and type helpers encode assumptions used by several passes. Changes to these
helpers can affect more than the pass that motivated them.

[ITirInlineHost](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs) is the body lookup
and policy contract for the shared inliner. [InlineReport](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs)
turns its refusals into diagnostics. [CSharpWriter](../../writers/Plato.CSharpWriter/CSharpWriter.cs)
owns scheduling, dump output and per-generation counter resets. These are supporting components,
not additional independently enabled optimization passes.

`TirMultiReduction` participates in child rebuilding and statement recognition, preserving
its source and original fold bindings across capture rewriting. `TirLoweredLoop.SourceFunction`
retains the resolved call identity through `WithChildren`; `TirIntrinsicIdentity` checks that
identity against the compilation's designated intrinsic library. The new consumer and exact
allocation passes use it without changing the recognition policy of older passes.

## Usage, tests and effectiveness

There is no user-facing rewrite flag. [TirRewriteTests](../../tests/PlatoTests/Optimization/TirRewriteTests.cs)
checks tree rebuilding and substitution, while pass tests exercise the resulting interactions.
Use the [evaluation commands](evaluation.md) with that class filter after changes to traversal.

The benefit is consistency and composability rather than a direct runtime speedup. A traversal
that loses type/origin information or misses a marker can silently disable a later optimization
or generate incorrect code. Scope, conversions and callback identity need explicit scrutiny;
the [sequencing investigation](../../tracker/issues/compiler-483.md) records unresolved hypotheses.
