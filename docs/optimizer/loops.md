# Loop lowering and emission: execute a chain in one traversal

[Overview](README.md) · [Fusion](fusion.md) · [Evaluation](evaluation.md)

## Idea and motivation

Dynamic array combinators can be emitted as ordinary loops over indices or elements. A producer
chain can run inside its consumer's loop, binding each produced value to a local rather than
allocating an intermediate array. This is related to [fusion and lowering](references.md#fusion-and-lowering),
but the implementation is a recognized set of combinator shapes, not general loop optimization.

The motivation is predictable generated code: remove traversal/delegate plumbing and producer
allocations while retaining readable Plato functions. A map still needs result storage; a fold
can return only its accumulator. Eliminating intermediates is different from eliminating all
allocations in a callback or result.

## Implementation and boundaries

The scope walker in
[TirLoopLowerer.Scopes.cs](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.Scopes.cs)
uses the same loop recognizer for computed arguments and nested calls. `TirEvaluation` carries
ordered prerequisites; operands finish in source order before a later operand's loop executes.
`TirStatementRewriter` places those prerequisites inside blocks, inline regions, branches and
loop bodies. Conditional expressions become local result regions when either branch has
prerequisites, keeping untaken work behind its guard. `TirGuardedLoop` places condition
prerequisites inside every loop test, including the terminating test. Recognized callbacks are
cloned independently and lowered inside their own iteration scopes; an empty outer loop executes
none of their prerequisites. Generated names avoid existing bindings. The scope traversal is separate
from recognizing the supported array operations.

Known callback propagation exposes proven callable arguments before loop recognition; it does not
rely on a separate list of intrinsic names.
Recognized consumer callbacks accept statement bodies: each iteration declares the parameters,
executes a local result/exit region, then consumes the result. Returns inside nested lambdas keep
their own boundary. Producer-chain stages use the same result emission for expression or statement
bodies, so inlining a callback does not disable chain lowering. No invoked lambda is introduced. Tests in
[StatementInliningTests](../../tests/PlatoTests/Optimization/StatementInliningTests.cs) compile
and execute empty and populated ranges, statement reductions, nested callback returns, guarded
branches and sibling fault order. They assert that computed arguments still lower with every
inlining policy.

[TirLoopLowerer](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs) replaces a
recognized expression with `TirTempRef` and inserts a `TirLoweredLoop` statement for its result.
The [loop emitter](../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs) renders that
marker. Kinds include map, indexed map, map-range, zip, reduce, Boolean consumers, flat-map and
supported adjacent-window operations; the complete mapping is in the [pipeline reference](../csharp-optimizer-pipeline.md).

Supported map producer chains are represented by `SourceStages`; `IndexSource` selects direct
index iteration from a `MapRange`. Each stage is stored in a local so a consumer can use its
result repeatedly without recomputing it. This path works with `--loops` even when `--fuse` is
off. Fusion can expose additional producers such as `Range`.

The lowerer refuses unsupported callback bodies,
unrenderable element types, non-one-dimensional collections and callbacks it cannot render.
It does not trace a local variable back to an earlier producer definition. Those restrictions
matter because hoisting a loop can change evaluation frequency and guards.

The [flat-map emitter](../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.FlatMap.cs) has a
further specialization: eligible raw singleton-array arms of an expression-bodied conditional
callback append directly to the result list. Exact array/element types and raw syntax shape are
required. Other arms retain evaluation and copying in their selected branch. This does not make
general nested-loop lowering available.

The independent default-off [exact output option](exact-output.md) recognizes a raw literal
FlatMap callback and chooses exact result storage only for runtime array sources. The marker's
`SourceFunction` preserves resolved intrinsic provenance through capture rebuilding; emission
does not infer identity from the string `Kind`. [Consumer fusion](consumer-fusion.md) runs
before this lowerer and emits its own traversable paired-fold marker.

Rebuilding constructor markers while lowering their arguments preserves their resolved
type. Losing it changes receiver-aware field emission after inlining. Array count and
index access use the same parenthesized receiver rendering as ordinary member calls.

### `--lower-more`: the grid builders and Filter (lambda removal L3)

`--lower-more` (requires `--loops`, C# only, default off) extends the recognizer to four calls of the
stdlib's `Primitives` library whose callback `--loops` alone cannot reach: `MakeArray2D` and
`MakeArray3D` (their bodies wrap `f` in a second lambda for a `MapRange`, so the plain lowering keeps
the outer delegate), `CartesianProduct` (the same through `MakeArray2D`), and `Filter` (an ordinary
generic call until the inliner expands it). The lowered forms, printed by `WriteLoweredLoop` for the
kinds `Make2D`, `Make3D`, `Cartesian` and `Filter`:

- a grid is one element array `new T[cols * rows]` (times `layers` for a volume) filled row-major by
  nested loops (layer, row, column) with the callback's parameters bound to the loop counters and a
  running element index, then the `Array2D<T>` or `Array3D<T>` record constructed once from it. A
  non-positive extent means no cell; the record keeps the extents as given;
- `Filter` fills an array presized to the source's count, appending each element the predicate
  accepts, and trims it with one `System.Array.Resize` when fewer passed. The receiver may be a
  fused producer chain, as for `Map` and `Reduce`.

The switch keeps every existing refusal: the element type (and, for a grid, the record type) must be
a concrete C# type, the callback a literal lambda or a `Function{N}` reference of the arity, and the
callee the `Primitives` function (a same-named library function is left alone). The hoister sees
the new loops as any lambda-callback loop; its locals sit inside `if (n > 0)` around the loop nest.
`--presize` does not touch them (they allocate once). Off, the output is byte-identical.
[LowerMoreTests](../../tests/PlatoTests/Optimization/LowerMoreTests.cs) compiles and executes each
kind on and off (same values, same callback order, including zero and negative extents), the hoist
interaction, the type-variable refusal and the switch-off text.

## Usage, tests and effectiveness

Enable `--loops`; use `--dump-tir` to distinguish a lowered marker from a surviving runtime call.
[LoopLowererArrayTypeTests](../../tests/PlatoTests/Optimization/LoopLowererArrayTypeTests.cs)
covers array recognition and emitted loops. [CombinatorFusionTests](../../tests/PlatoTests/Optimization/CombinatorFusionTests.cs)
covers direct chains and reused stage values. [FlatMapFilterTests](../../tests/PlatoTests/Optimization/FlatMapFilterTests.cs)
compiles and executes accepted/refused singleton-arm cases.

The [dated validation report](../reports/optimizer-handbook-validation-2026-10-24.md) records the
singleton specialization's allocation savings against the ordinary lowered path. It measures
that rule's allocation effect, not an elapsed-time speedup for the whole pipeline.

One loop without intermediate arrays is useful structural evidence. Actual gains depend on
collection size, callback cost, captures, early exits and the JIT. The sequencing and identity
questions in [compiler-483](../../tracker/issues/compiler-483.md) remain relevant to broader
claims. Use paired correctness and allocation/runtime measurements from [evaluation](evaluation.md).
