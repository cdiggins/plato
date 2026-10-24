# Plato optimizer handbook

Plato expresses geometry through small functions, fixed-component values and array combinators.
The C# optimizer removes some of that abstraction overhead before C# compilation: it exposes
callee bodies, combines producers, expands fixed shapes and emits explicit loops. It operates
on typed intermediate representation (TIR), after the compiler has resolved and specialized
functions. It is not the .NET JIT and does not perform general machine-code optimization.

The suite follows the C# pipeline, where these passes are scheduled together. The
[C++/CUDA writer](../../writers/Plato.CppWriter/README.md) also consumes the shared inliner
through `ITirInlineHost`; that does not imply it runs the remaining C# passes.

Start with the [pipeline reference](../csharp-optimizer-pipeline.md) for exact scheduling,
flags, runtime contracts and a worked example. This handbook explains the individual components
and the evidence behind their intended benefits. Implementation and executable tests take
precedence over both documents.

## Components and their relationships

| Component | Core idea | Activation |
|---|---|---|
| [Inlining](inlining.md) | Substitute a resolved function body to expose hidden operations | `--inline` |
| [Fusion](fusion.md) | Compose adjacent producers or put a producer inside its consumer | `--fuse` |
| [Literal expansion](literal-expansion.md) | Evaluate the structure of a small, statically bounded traversal at compile time | Part of `--fuse` |
| [Component expansion](component-expansion.md) | Replace fixed-size collection plumbing with field expressions | `--optimize` |
| [Materialization](materialization.md) | Mark selected array construction boundaries with eager helper names | `--optimize-arrays`; runtime incompatibility documented |
| [Loop lowering and emission](loops.md) | Turn recognized dynamic combinators into local loops | `--loops` |
| [Rewrite infrastructure](rewrite-infrastructure.md) | Rebuild typed trees and substitute parameters consistently | Used by passes |

`CSharpWriter.RunOptimizerPasses` schedules inlining, fusion, component expansion,
materialization and loop lowering in that order. Literal expansion belongs to fusion, rather
than being an independently scheduled pass. The [generated source map](source-map.md) gives
Greenhouse-extracted public symbols and source links, including marker nodes and host contracts.

Before these optional passes, the compiler's [short-circuit normalization](../../src/Plato.Compiler/Checking/TirShortCircuit.cs)
expresses Boolean short-circuiting as conditionals. It implements language semantics even when
optimization is disabled. After the passes, [TirCSharpBodyWriter](../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs)
renders their trees and marker nodes. The [compiler's TIR provider](../../src/Plato.Compiler/Checking/TirEmitSource.cs)
supplies specialized bodies; the optimizer does not reparse Plato or introduce another resolver.

## What effectiveness means

Removing an intermediate array is a structural benefit; it is not by itself a measured speedup.
The guides distinguish assertions about emitted C#, compiled behavior, and performance. Durable
pages intentionally omit timing and allocation measurements: use the [evaluation procedure](evaluation.md)
and retain dated results with their input, flags and environment.

For example, inlining can expose a `MapRange(...).Map(...).Reduce(...)` chain. Loop lowering
can then evaluate its stages in one index loop, with no intermediate producer arrays. Fusion
can simplify that chain first, but is not required for every producer-chain lowering. Small
literal traversals can instead become straight-line expressions inside lambdas, where the
general loop lowerer refuses to hoist loops.

These are bounded transformations, not a blanket claim of semantic equivalence for every input.
[compiler-483](../../tracker/issues/compiler-483.md) tracks investigation of sequencing,
fault/termination behavior, name-based recognition, captures and generated names. Its hypotheses
are not all confirmed bugs. Purity alone does not establish that eager evaluation can be skipped.

## Reading and using the suite

Read [evaluation](evaluation.md) before choosing flags for a workload. Follow each component's
source and test links to see its accepted and refused cases. [Related work](references.md)
connects the implementation to established compiler ideas without claiming that Plato implements
those systems in full.

Maintenance is described separately in the [Optimizer Documentation Stewardship runbook](../../tools/optimizer-docs/MAINTENANCE.md).
