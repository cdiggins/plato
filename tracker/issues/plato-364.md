---
id: plato-364
title: Navigation index does not model sum types (match arms, case constructors)
type: bug
status: ready
priority: "2"
effort: "M"
risk: "low"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-232, plato-236, plato-237, submodules/Plato/Plato.Navigation/ReferenceExtractor.cs, submodules/Plato/PlatoCompiler/Symbols/SymbolFactory.cs, submodules/Plato/Plato.Navigation.Tests/Corpus.cs]
---

## Symptoms

The navigation index (`Plato.Navigation`, plato-236/237) was built before sum types landed
(plato-232, 2026-07-27) and has never been gated against a corpus that uses them: its test
corpus was `stdlib-legacy`, which has none. Pointing the corpus at the forward `stdlib`
(2026-07-30) surfaced three failures, all one root cause — **nothing about a `match` expression
or a case constructor is indexed**.

**(a) Match-arm identifiers are invisible.** `AstMatchArm.CaseName` and `AstMatchArm.Binders`
produce no definition record, no reference record, and no diagnostic. In
`stdlib/axes-2d.library.plato`:

```
Component(v: Vector2D, axis: Axis2D): Number
    => match (axis) {
        X => v.X;          // 'X' here is unknown to the index
        Y => v.Y;
    };
```

Go-to-definition on the `X` of an arm returns nothing; find-references on the case `Axis2D.X`
misses every arm that matches it. Fails `IdentifierSweepTests.EveryIdentifierIsClassified`.

**(b) Payload binders resolve to nothing.** A binder introduced by a pattern is not a definition,
so its *uses* in the arm body are references with an empty target list. From
`stdlib/fields-graphs.library.plato`:

```
Remap(input, fromLow, fromHigh) =>
    (graph.EvalNodeAt(input, ...) - ...) / (graph.EvalNodeAt(fromHigh, ...) - ...)
```

`input`, `fromLow`, `fromHigh`, `fieldIndex`, `a`, `b`, `combine`, `left`, `level` are all
unresolved names in the current stdlib. Fails
`InvariantTests.UnresolvedReferencesAreOnlyKnownBuiltins`, whose whole point is that only
`$T`-style type variables, `Self` and `default` may lack a source definition.

**(c) Case constructors render as their owning type in target groups.** A sum-case factory is a
compiler-generated function named after the case, so it joins the function group of that name.
`DefResolver.OfFunction` (ReferenceExtractor.cs) maps a function to its declaring `MemberDef`,
and a compiler-generated function has none — so it takes the documented fallback and maps to the
**owning type**. The `-` operator in `stdlib-tests/foundation.laws.plato:26` resolves to:

```
ref 'Subtract' Operator
    -> Type 'ScalarFieldNode2D'   (has a Subtract case)
    -> Type 'ScalarFieldNode3D'   (has a Subtract case)
    -> Type 'BlendMode'           (has a Subtract case)
    -> Method 'Subtract' x 15     (the real overload group)
```

6,144 references in the forward corpus have targets spanning more than one name. Fails
`QueryTests.AFunctionCallOffersEveryOverload`. This is the worst of the three for agents: it
makes go-to-definition on ordinary arithmetic offer unrelated sum types, and it inflates every
`plato_references` result for a colliding name.

## Impact

Every agent editing the forward stdlib navigates it through this index (the `plato-navigation`
MCP server indexes `stdlib` + `stdlib-tests` by default as of 2026-07-30). Sum types are now the
sanctioned encoding for classification (CONVENTIONS.md A1, the kind-pattern retirement), so the
blind spot grows with the vocabulary rather than shrinking. `plato_check` is unaffected — it runs
the real compiler, which understands sums.

## Root cause notes

Read from `SymbolFactory.ResolveMatch` (SymbolFactory.cs:252) and
`ReferenceExtractor.DefResolver`. The three symptoms have two different owners, and neither is a
missing record type — `DefKind.SumCase` / `SumCaseField` already exist and case *declarations*
are indexed.

**(a) and (b) are binder-side, in `PlatoCompiler`.** `ResolveMatch` already does the semantic
work correctly:

```csharp
var caseDef = subjectDef?.Cases?.FirstOrDefault(c => c.Name == caseName);   // case IS resolved
binders.Add(BindValue(bName, new VariableDef(ValueBindingsScope, bName, bType, null)));
```

- The case name is resolved to a `SumCaseDef` but **no `RefSymbol` is created for
  `astArm.CaseName`**, so nothing reaches `factory.SymbolsToNodes` and the index sees no
  occurrence at all. That is symptom (a).
- Each binder becomes a real `VariableDef` bound into scope — so uses in the arm body *do*
  resolve — but it is constructed with a **null AST node** (the last argument). `DefResolver.
  OfSymbol` maps a symbol to a def through `SymbolsToNodes`, so a node-less `VariableDef`
  yields no target. That is symptom (b): the binding is correct, only its provenance is lost.

Both are small, local changes in `ResolveMatch` — pass `astArm.Binders[i]` instead of `null`,
and register a reference for the case name. Neither is a navigation-index change, and
`PlatoCompiler` is owned by another workstream, so this half needs to be handed over.

**(c) is navigation-side and is a rendering choice, not a resolution error.** The group really
does contain the sum-case factories — Plato puts case constructors in the function namespace
under their case name, so `Subtract` legitimately names 15 methods *and* three case factories.
The index is faithful about membership and wrong about identity: `OfFunction`'s documented
fallback ("a compiler-generated function has no syntax of its own and maps to its owning type")
was written when the only such functions were constructors and implicit casts, where pointing at
the type is the right answer. For a sum-case factory it is not — the case has its own syntax and
its own `SumCase` def.

## Fix approaches

1. **Point case factories at their `SumCase` def (fixes c).** In `DefResolver.OfFunction`, resolve
   a sum-case factory to the `SumCase` DefRecord instead of the owning type. More truthful, and
   the mixed-name groups disappear on their own because the `SumCase` def is *named* `Subtract`.
   Navigation-side, small, no design question. Do this first.
2. **Give binders their node (fixes b).** One argument in `ResolveMatch`. Requires a
   `PlatoCompiler` change.
3. **Emit a reference for the arm's case name (fixes a).** Also `ResolveMatch`; the `SumCaseDef`
   is already in hand at that point, so it is a `RefSymbol` construction, not a lookup.

Note that (1) alone does not restore go-to-definition on an arm — it fixes what an operator or
call reference *reports*. (3) is what makes `match` navigable.

## Simplest implementation

(1) on its own is worth landing: it is contained in `ReferenceExtractor`, needs no compiler
change, and un-scopes the `QueryTests` assertion. (2) and (3) should go to whoever owns
`SymbolFactory`.

## Current state

`Plato.Navigation.Tests` was moved to the forward corpus on 2026-07-30. The three assertions
above are scoped to exclude this known gap and name this issue, so a *new* kind of miss still
fails the suite while these three do not. Removing those scopes is the acceptance test for this
issue.
