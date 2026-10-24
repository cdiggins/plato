# Writer optimization

These passes rewrite typed intermediate representation (typed IR) before a backend renders source. `CSharpWriter.RunOptimizerPasses` schedules the C# pipeline; the [pipeline reference](../../../docs/csharp-optimizer-pipeline.md) defines pass order, flags, and runtime contracts.

`TirInliner` substitutes resolved callee bodies through the `ITirInlineHost` contract, with refusals recorded by `InlineReport`. `TirComponentUnroller` uses the operation patterns in `ComponentUnroller` to replace fixed-component combinators. `TirArrayMaterializer` handles materialization boundaries. `TirFusion` and `TirLiteralUnroller` simplify compatible chains and literal inputs; `TirLoopLowerer` creates markers rendered by `TirCSharpBodyWriter.Loops.cs`. `TirRewrite` supplies shared tree rebuilding and substitution.

These files retain the `Ara3D.Geometry.CSharpWriter` namespace because other backends consume the shared inliner. Emission syntax belongs in the body writers, while changes to pass semantics require the existing optimizer tests under [PlatoTests](../../../tests/PlatoTests/README.md).
