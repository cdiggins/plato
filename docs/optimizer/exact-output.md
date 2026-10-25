# Exact allocation for fixed-width FlatMap

[Overview](README.md) · [Pipeline](../csharp-optimizer-pipeline.md) · [Loops](loops.md)

`--loops --exact-output` enables exact result allocation for a FlatMap callback that returns
a raw array literal of statically known width. It is disabled by default. The optimization
replaces result-list growth with one result array; callback arrays are still evaluated fully,
in order, and copied. It does not remove callback calls, conversions or faults.

[TirExactOutput](../../writers/Plato.CSharpWriter/Optimization/TirExactOutput.cs) checks the
lowered marker's resolved canonical FlatMap provenance, concrete instantiated input/output
types and raw callback shape. Producer stages, coercions, mismatched element types and
nonliteral callback results are refused. `MaximumCallbackNodes` bounds the generated code
duplication required by the exact and fallback branches.

The [emitter](../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.ExactOutput.cs) takes the
exact path only when the runtime source is an array and the widened count-times-width
calculation fits `System.Array.MaxLength`. Otherwise it retains the ordinary growing-list
path. Array length stability permits exact sizing even if callbacks mutate source elements:
all source reads remain at their original positions. Custom IReadOnlyList implementations
stay on the fallback path. Empty callback arrays still evaluate once per source element.

The loop marker retains `SourceFunction` through rebuilding, so later emission can verify
identity without resolving names again. The original loop emitter remains the default;
the specialization owns only its additional output-allocation choice. Resource exhaustion
and host observation of the concrete result collection type are not equivalence guarantees;
Plato observes the result through its array operations.

[ExactOutputTests](../../tests/PlatoTests/Optimization/ExactOutputTests.cs) use actual Plato
TIR and compile emitted C# to compare values, callback/read traces, faults, mutation,
fallback behavior, flag interactions and allocation. The
[dated report](../reports/optimizer-output-sizing-2026-10-25.md) records measurements and
their limits; the runtime capacity fallback is guarded even where practical test arrays
cannot exercise the maximum capacity boundary.
