# Component expansion: turn a fixed shape into field operations

[Overview](README.md) · [Rewrite infrastructure](rewrite-infrastructure.md) · [Source map](source-map.md)

## Idea and motivation

A fixed-component geometry value need not be processed as a dynamic collection. A component map
over a vector can become a constructor whose arguments apply the callback to each field. This
removes `Components` wrappers, temporary arrays and reconstruction plumbing while preserving
the scalar formulas. It resembles [scalar replacement and unrolling](references.md#fixed-shapes-and-loops),
but uses Plato's known geometry shapes instead of general memory escape analysis.

The rationale in the source is abstraction overhead in generated component-wise operations,
such as interpolation. Fixed arity and field metadata are already available at C# emission time.
Historical timings in source comments are not measurements of today's wrapper-scalar recipe.

## Implementation and boundaries

[TirComponentUnroller](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs)
rewrites recognized call sites and suitable monomorphized library bodies. The metadata and
operation patterns live in [ComponentUnroller](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs).
That file also contains older expression marker machinery; the active pipeline entry is the
TIR unroller, not the retired legacy body writer mentioned by historical comments.

The output uses `TirComponentAccess`, `TirConstructorCall` and `TirBooleanChain`, rendered by the
C# body writer. Literal callbacks can be beta-reduced; suitable delegate parameters can be invoked
per component. Recognized constructor and Boolean consumers let library bodies avoid routing
fixed components through a temporary dynamic array.

The pass needs a recognized component layout and supported expression/callback shape. Dynamic
arrays are not fixed-component values. Unsupported or statement-shaped bodies remain unchanged;
later passes may still handle eligible dynamic operations. Field casts retain wrapper-type
conversion context. Expansion changes the plumbing, not the underlying numeric formula.

The field-layout table currently recognizes concrete implementations of `IArrayLike`.
Forward vocabulary using other contracts, such as `IVector`, does not automatically
enter that table. Fixed-array rewrites can still run under `--optimize`, so enabling
the flag alone does not establish that vector operations were expanded into fields.
Inspect the generated form when evaluating that benefit; the forward-vocabulary
coverage gap is tracked in [compiler-503](../../tracker/issues/compiler-503.md).

## Usage, tests and effectiveness

Enable `--optimize`; add `--inline` when component calls are hidden behind helpers.
[ComponentUnrollerTests](../../tests/PlatoTests/Optimization/ComponentUnrollerTests.cs) checks
direct fields, constructor/Boolean forms and the boundary where genuinely dynamic arrays still
reach loop lowering. [TirRewriteTests](../../tests/PlatoTests/Optimization/TirRewriteTests.cs)
checks traversal of marker nodes used by later passes.

The intended benefit is removal of per-operation collection and callback plumbing for fixed
shapes. Delegate references can still produce invocations, and the resulting scalar calls still
depend on .NET optimization. Validate generated shape, compile and execute it, then measure the
workload as described in [evaluation](evaluation.md).
