---
id: plato-328
title: Inferred interface constraints are lost on generic library-function type variables
type: bug
status: ready
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed:
links: [tracker/issues/plato-323.md, submodules/Plato/Plato.CSharpWriter/CSharpFunctionInfo.cs]
---

## Issue

A library function generic in its element type emits as a C# generic method with NO `where`
clause, even when its body calls an interface member on that type variable. The one live instance,
surfaced by plato-323's Array-receiver emission path:

`stdlib/splines-bezier.library.plato:26`

```
DeCasteljau(xs: Array<$T>, t: Number): $T
```

whose body calls `.Lerp`. The forward recipe emits

```csharp
public static _T0 DeCasteljau<_T0>(this IReadOnlyList<_T0> xs, float t) { ... _var52[i].Lerp(...) ... }
```

with no constraint, so csc reports (1 error, `Extensions.g.cs`):

```
CS1929: '_T0' does not contain a definition for 'Lerp' and the best extension method overload
        'AlgebraMetric.Lerp(Instant, Instant, float)' requires a receiver of type 'Ara3D.Geometry.Instant'
```

`CSharpFunctionInfo.Constraints` renders from `FunctionInstance.ConstrainedTypeVariables`, so
either the checker never records the `Interpolatable` obligation the body implies, or the writer
does not consult it for a library function emitted through the static/extension path (the
receiver's own type variable in particular).

Emitting the function is still a large net win — it fixes 8 CS1061 call sites and costs this
1 error — so plato-323 kept it. This is the residual.

## Impact

1 of the errors remaining in the forward conformance build; blocks nothing else. Latent
correctness risk beyond that: any future `Array<$T>` library function whose body uses interface
members hits the same wall, and the failure mode is a compile error in generated code rather than
a checker diagnostic.

## Affected code

- `submodules/Plato/Plato.CSharpWriter/CSharpFunctionInfo.cs` — `Constraints` /
  `ConstraintString`.
- `submodules/Plato/PlatoCompiler/Checking/` — whether the constraint is inferred and recorded at
  all.
- Repro: `.\tools\regen-forward-conformance.ps1 -Codegen` then
  `dotnet build submodules\Plato\conformance\Plato.ForwardConformanceTests -c Release`, and grep
  `Extensions.g.cs` in the output.

## Fix approaches

1. Writer-side: check whether the solved constraint is already on
   `FunctionInstance.ConstrainedTypeVariables` and simply not rendered for this emission kind — if
   so this is a one-line fix.
2. Checker-side: infer the interface obligation from the member calls in the body and record it, the
   same way an interface's own type parameters get their bounds.
3. Author-side workaround (does NOT fix the general case): declare the bound explicitly in
   `stdlib/splines-bezier.library.plato`. Cheap, and worth doing if 1 and 2 are both deep.

## Done means

- [ ] The generated `DeCasteljau<_T0>` carries its `where` clause and the CS1929 is gone.
- [ ] A writer-level test pins a constrained generic library function's emitted signature.
