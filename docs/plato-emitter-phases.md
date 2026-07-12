# Plato emitter phases — the intermediate layers

Two pipelines produce the generated C#. Only the **last** layer reaches disk today; every TIR
layer is an in-memory `TirFunction` with a `ToString()`, dumpable via `--dump-tir=<dir>`.

## Front-end: source → TIR (per function, `TirEmitSource`)

The checker run `Normalize → Constrain → Solve → Elaborate → Monomorphize`.

| # | Layer | Type | Kind | Generic? |
|---|-------|------|------|----------|
| 1 | AST | `FunctionDef.Body` (`Expression` tree) | tree | yes |
| 2 | Normalized AST | `FunctionDef` (`Compilation.GetNormalizedFunction`) | tree (desugared) | yes |
| 3 | Constraints | `ConstraintSystem` (`ExprTypes` + tyvars) | side-table over #2 | — |
| 4 | Solution | `Solver` (`ResolvedCalls`, zonk map) | side-table over #2 | — |
| 5 | Elaborated TIR | `TirFunction` (`Monomorphizer.ElaborateAll`) | typed tree IR | yes |
| 6 | Monomorphized TIR | `TirFunction` per `(FunctionDef, concreteType)` | typed tree IR | ground |

Layer 5 is the first real IR tree: `TirCall` carries an `EmissionKind`, conversions are explicit
`TirCoerce` nodes. **Static bodies** (constants, the `IArray` library functions in
`Extensions.g.cs`) emit from layer 5, unspecialized. **Member-instance bodies** emit from layer 6.

## Back-end: TIR → TIR → C# (per body; `CSharpWriter.RunOptimizerPasses`)

Each pass is a pure `TirFunction → TirFunction` rebuild (returns the input unchanged if it did
nothing), applied in this fixed order after layer 5/6:

| # | Layer | Pass | Flag | Introduces |
|---|-------|------|------|-----------|
| 7 | Inlined | `TirInliner.Inline` | `--inline` | callee bodies substituted (multi-level, fixpoint); scalar `TirCoerce` tags; alpha-renamed lambda params |
| 8 | Unrolled | `TirComponentUnroller.UnrollFunction` | `--optimize` | `TirComponentAccess`, `TirConstructorCall`, `TirBooleanChain` |
| 9 | Materialized | `TirArrayMaterializer.Rewrite` | `--optimize-arrays` | `Map`/`MapRange` → `MapEager`/`MapRangeEager` |
| 10 | Loop-lowered | `TirLoopLowerer.Rewrite` | `--loops` | `TirLoweredLoop` + `TirTempRef` |
| 11 | C# text | `TirCSharpBodyWriter` | — | the `.g.cs` body string |

`--inline`=7, `--optimize`=8, `--optimize-arrays`=9, `--loops`=10 are independent of the shape
axes (`--csharp-style`, `--scalar`, `--methods`). "Non-optimized" emits straight from 5/6;
"optimized" runs all four. All four passes are applied at every body-emit site through the single
`CSharpWriter.RunOptimizerPasses(tir, fi)` helper — the one place `--dump-tir` records phases.
