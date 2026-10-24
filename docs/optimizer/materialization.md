# Materialization: an eager-name rewrite with a runtime mismatch

[Overview](README.md) · [Pipeline runtime contract](../csharp-optimizer-pipeline.md) · [Loop lowering](loops.md)

## Idea and original motivation

Materializing a lazy sequence once can avoid repeated evaluation when a collection is stored or
reused. [TirArrayMaterializer](../../writers/Plato.CSharpWriter/Optimization/TirArrayMaterializer.cs)
was designed around that idea: recognize materialization positions and rename `Map`/`MapRange`
to eager variants. [LINQ materialization](references.md#materialization) is a related concept,
not evidence that Plato's arrays are lazy.

The live [ArrayIntrinsics](../../src/Plato.Intrinsics/ArrayIntrinsics.cs) implementation already
makes `MapRange` eager. The eager helper names emitted by this pass are absent from that runtime.
Consequently this pass does not establish an additional caching benefit for those calls and can
produce C# that fails to compile. The [pipeline reference](../csharp-optimizer-pipeline.md)
documents the runtime mismatch in detail.

## Implementation and boundaries

The rewrite recognizes direct constructor or tuple arguments, looking through coercions, and
selected `let` values that are reused or referenced beneath a lambda/loop. It changes call names,
not the array representation or the surrounding algorithm. A producer buried inside an ordinary
function argument is not a direct materialization position.

The subsequent loop lowerer accepts eager spellings at supported sites and may eliminate them.
Its refusals mean that adding `--loops` cannot guarantee removal of every renamed call.

## Usage, tests and effectiveness

`--optimize-arrays` enables the pass, but it should not be selected as an independently useful
optimization against the live runtime. [ArrayMaterializerTests](../../tests/PlatoTests/Optimization/ArrayMaterializerTests.cs)
checks the rename and its positions. Passing a text-shape test does not establish that the
renamed calls link to runtime methods.

Any experiment involving this flag must compile the emitted artifact as well as inspect it.
There is no supported performance claim here. Retirement or redesign is a separate behavior
change; this handbook records the mismatch instead of silently changing the compiler.
