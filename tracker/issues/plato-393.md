---
id: plato-393
title: Where-clause bounds on library function declarations
type: problem
status: done
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: [stdlib/geometry/splines-bezier.library.plato, parakeet/Parakeet.Grammars/PlatoGrammar.cs, plato-382]
---

## Issue

plato-382 gave `type` and `concept` declarations a `where` clause, verified it, and
emits it as a C# constraint. A **library function** declaration still has none. A
function's type variables can only INHERIT a bound, from a constructed type that
appears in its own signature (`Sample(x: Tween<$T>, ...)` learns `$T: Interpolatable`
from `type Tween<T> where T: Interpolatable`). When no such type is in the signature,
the requirement cannot be spelled at all.

The standing example is the de Casteljau reduction
(`stdlib/geometry/splines-bezier.library.plato`). Its body needs `Lerp` on a bare
element, and its only constructed parameter type is `Array<T>` — a primitive that is
unbounded and must stay unbounded. So the function is written **five times**, once per
control-value type (Point2D, Point3D, Vector2D, Vector3D, Number), with bodies
identical modulo the type name.

## Impact

- Five hand-duplicated bodies today, and one more per control-value type anyone adds.
  `plato-382` phase D attempted the collapse and could not make it honest.
- The dishonest collapse is available and is the trap: `DeCasteljau(xs: Array<$T>, ...)`
  RESOLVES, because bounds restrict only where they are declared and an unbounded
  parameter stays permissive. But the declaration then promises nothing, and
  `TirEmitSource.IsOpenGenericEmittable` correctly refuses the body — a call
  dispatching on an unbounded bare parameter emits as a throwing stub, not as C#.
  The geometry tier is not emitted today, so that cost is currently invisible.
- Not urgent: the five overloads are correct and tested; only their duplication hurts.

## Affected code

- `parakeet/Parakeet.Grammars/PlatoGrammar.cs` — `MethodDeclaration` has no
  `ConstraintList`; `Type` and `Concept` both do.
- `src/Plato.Compiler/Checking/TypeConstraints.cs` — `InheritedBounds` is the only
  source of a function variable's bounds; a DECLARED source would join it there.
- `writers/Plato.CSharpWriter/CSharpFunctionInfo.cs` — already emits the per-function
  `where` clause from inherited bounds, so the writer likely needs no change.
- `stdlib/geometry/splines-bezier.library.plato` — the five overloads and the comment
  that records why.

## What makes this hard

Mostly a question of where the bound belongs, not of machinery. A library function is
not a declaration a caller names; a bound on it is a *precondition on inference*, and
the checker would have to decide what happens when an inherited bound and a declared
one disagree. The C# side is settled — a function-level clause is exactly what
`CSharpFunctionInfo.ConstraintString` already writes.

## Candidate answers

1. **`where` on a method declaration**, checked exactly like a type's, unioned with the
   inherited bounds. Smallest surface, matches C#.
2. **Bound the element position instead** — a constrained collection concept
   (`Interpolatable`-bounded array view) that the signature mentions, so the existing
   inheritance path carries it. No language change; new vocabulary, and it changes
   every call site.
3. **Leave it.** The duplication is bounded and visible; revisit if a third example
   appears.

## Resolution

**Yes — library functions take `where` bounds, and they are verified.** Candidate answer 1.

Surface syntax: the clause sits AFTER the return type and before the body, and names the variable
exactly as the signature spells it, with the `$`:

```plato
DeCasteljau(xs: Array<$T>, t: Number): $T where $T: Interpolatable => ...;
```

It is the last thing in the signature, which is the same slot the clause occupies on `type` and
`concept` (`<params> where <bounds> <base-list> { ... }`); everything after a function's return type
IS its body, so no other position keeps the reading "signature, then bounds on that signature, then
body". The one `Constraint` grammar rule now accepts either spelling of the target (bare `T` for a
declaration parameter, `$T` for a function's signature variable), so all three declaration kinds
share it.

Semantics: a declared function bound joins the INHERITED ones as a second source of the same shape,
unioned in `TypeConstraints.InheritedBounds`, which is the single channel every consumer already
read. So it licenses calls in the body (`Solver.BoundsPermit`), emits as the C# `where` clause on
the generated method (`CSharpFunctionInfo.InheritedConstraints`), and licenses the open-generic body
(`TirEmitSource.IsOpenGenericEmittable`) with no new plumbing in any of the three.

Its one NEW obligation is at the call site, which is where a function bound differs from a type
bound: a type is written (checked at its construction site, CHK309), a function is called. Argument
satisfaction is enforced in the solver as a candidate-viability rule — whatever the arguments bind
the variable to must satisfy the bound, or the overload does not match. Reported `CHK206` rather
than the misleading `CHK201` "no overload matches", because the signature DID match and only the
bound failed. `CHK310` (a bound that is not a concept) and `LINT002` (a bound naming a variable the
signature never mentions) were extended to cover function bounds too.

Follow-up filed: `plato-394` — a function bound on a variable that the writer folds into the
RECEIVER type's own parameter loses its emitted clause and would produce uncompilable C#. Not
reachable from any shipping tier today (the stdlib's only function bound is over the `primitive`
`Array`, in the non-emitted `geometry` tier).

## Done means

- [x] A decision recorded on whether library functions take `where` bounds. — yes, verified, with
      the syntax and semantics above; written up in `docs/plato-language-semantics.md` (§5) and
      `docs/compiler-pipeline.md` (diagnostic table + the declared-bounds section).
- [x] If yes: grammar + AST + checker reading + one codegen test. — `PlatoGrammar.MethodDeclaration`
      + `Constraint`, `AstMethodDeclaration.Constraints`, `FunctionDef.DeclaredBounds`,
      `TypeConstraints.InheritedBounds`; tests in `tests/PlatoTests/FunctionConstraintTests.cs` and
      `FunctionConstraintCodegenTests.cs`.
- [x] `DeCasteljau` is one function, and its emitted C# is a real body, not a stub. — one function
      in `stdlib/geometry/splines-bezier.library.plato`, five overloads gone, every caller still
      resolving (the forward-stdlib checker ratchet stays at 0 diagnostics across all four tiers).
      The "real body, not a stub" half is proved by an equivalent in-test fixture
      (`FunctionConstraintCodegenTests.TheEmittedBoundedFunctionCompilesAndRuns`, Roslyn-compiled
      and executed), because `geometry` is not an emitted tier and so has no generated C# of its own.
