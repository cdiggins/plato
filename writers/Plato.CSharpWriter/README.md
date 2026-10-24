# C# source generation

This project emits C# libraries from a Plato compilation. Developers changing generated signatures, extension methods, or function-body syntax should begin here. The project references the compiler; generated runtime behavior belongs to [Plato.Intrinsics](../../src/Plato.Intrinsics/README.md).

| Entry | Responsibility |
|---|---|
| `CSharpWriter.cs` | Coordinates output files, extension plans, profiles, and optimizer scheduling. |
| `CSharpTypeWriter.cs`, `CSharpConcreteTypeWriter.cs` | Render type names and concrete declarations. |
| `CSharpFunctionInfo.cs`, `CSharpBoundWriter.cs` | Determine member signatures and generic constraints. |
| `ExtensionStyleWriter.cs` | Partitions members between structs and extension libraries. |
| `TirCSharpBodyWriter.cs` | Renders typed intermediate representation (typed IR) statements, expressions, and calls. |
| `TirCSharpBodyWriter.Loops.cs` | Renders the loop markers produced by optimization. |
| `CSharpSerializationWriter.cs` | Adds serialization, formatting, and parsing members. |
| [Optimization](Optimization/README.md) | Rewrites typed IR before emission and records inlining decisions. |
| `DocWriter.cs`, `Analzyer.cs` | Produce reference HTML and analysis reports. |

`CSharpWriterExtensions.ToCSharp` connects a compilation to generation; the [CLI](../../src/Plato.CLI/README.md) supplies command-line access. The [optimizer pipeline](../../docs/csharp-optimizer-pipeline.md) owns pass order, flags, and array contracts. Keep namespace and project identities stable when grouping implementation files: other backends consume optimizer types from this assembly.

The SDK project includes source files recursively. Structural refactoring must preserve generated output; existing tests under [PlatoTests](../../tests/PlatoTests/README.md) cover writer behavior. Consult the repository gate log for execution results rather than treating this guide as a test report.
