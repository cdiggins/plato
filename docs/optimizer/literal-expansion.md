# Literal expansion: specialize a bounded traversal

[Overview](README.md) · [Fusion](fusion.md) · [Source map](source-map.md)

## Idea and motivation

When a range has a small literal length, its traversal structure is known during compilation.
For example, `3.MapRange(f)` can become `[f(0), f(1), f(2)]`; a subsequent `Any(p)` can become
a Boolean chain. This is a bounded form of partial evaluation, related to
[loop unrolling](references.md#fixed-shapes-and-loops), although Plato rewrites combinator trees
rather than machine-level loops.

The motivating site recorded in the source is the STEP mesher's triangle refinement predicate:
a small range inside a lambda. General loop lowering cannot hoist a loop out of that lambda
without changing its evaluation scope. Expression expansion gives that site another route.

## Implementation and boundaries

[TirLiteralUnroller](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs) is called
by fusion. It expands `MapRange`, maps literal arrays, reduces arrays to a left-associated fold,
and converts `All`/`Any` into `TirBooleanChain` nodes. Empty Boolean traversals use their identity
values. `TirFusion.UnrollLimit` bounds the accepted literal size; dynamic and larger inputs
remain available for loop lowering or ordinary runtime execution.

Callbacks must support substitution or an eligible delegate invocation. The helper refuses
substitution that repeats a non-leaf argument. This controls duplicated work; it is not a complete
analysis of eager element evaluation. Short-circuiting after expansion can matter when an array
element faults or fails to terminate; [compiler-483](../../tracker/issues/compiler-483.md) tracks
the broader semantic investigation.

## Usage, tests and effectiveness

There is no independent flag: use `--fuse`. [LiteralUnrollingTests](../../tests/PlatoTests/Optimization/LiteralUnrollingTests.cs)
checks literal limits, dynamic refusal, Boolean/fold forms and expression scopes. See
[evaluation](evaluation.md) for running that class and inspecting the TIR dump.

Expansion can eliminate traversal overhead and, when the producer is consumed by another
rewrite, temporary arrays. A standalone expanded `MapRange` still returns an array literal;
expansion alone does not imply zero allocation. Larger emitted bodies can increase compilation
and JIT costs, which motivates the explicit bound. No universal speedup is claimed.
