---
id: plato-393
title: Where-clause bounds on library function declarations
type: problem
status: idea
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-01
closed:
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

## Done means

- [ ] A decision recorded on whether library functions take `where` bounds.
- [ ] If yes: grammar + AST + checker reading + one codegen test.
- [ ] `DeCasteljau` is one function, and its emitted C# is a real body, not a stub.
