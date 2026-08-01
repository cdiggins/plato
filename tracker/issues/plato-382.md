---
id: plato-382
title: Constrained type parameters on concrete types, and constraint-carrying C# emission
type: problem
status: idea
priority: p3
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/graphics/motion-graphics.types.plato, stdlib/graphics/time-varying.concepts.plato, src/Plato.Compiler/Checking/TirEmitSource.cs, parakeet/Parakeet.Grammars/PlatoGrammar.cs, src/Plato.AST/AstNodeFactory.cs, plato-376, stdlib-377]
---

## Issue

Plato has no way to say *"this concrete type's parameter must support these
operations"*. `concept` declarations take a `where` clause; `type` declarations
do not. The consequence is that a generic concrete type can declare a concept it
cannot possibly implement, and the failure surfaces only at C# emission time, as
a throwing stub.

`Tween<T>` (`stdlib/graphics/motion-graphics.types.plato:32`) is the last
shipping instance. It `implements TimeVarying<T>`
(`stdlib/graphics/time-varying.concepts.plato:11`), whose obligation is
`Sample(x: Self, time: Duration): TValue`. Any honest `Sample` for a tween must
`Lerp` between `From: T` and `To: T` — an operation ON a bare `T` — and nothing
in the language lets the declaration require `T: Interpolatable`
(`stdlib/foundation/algebra-operations.concepts.plato:64`). So the obligation is
undischargeable, LINT001 reports it, and the writer emits a throw.

This is a **problem**, not a bug: closing it should produce an ADR (does Plato
grow declared bounds on concrete types, and do those bounds become *verified* or
stay decorative?) plus follow-up issues, not a one-line fix.

## Impact

Narrow today, structural tomorrow.

- Exactly one shipping-tier symptom: `Sample(Tween<T>, Duration):T` is the single
  remaining LINT001 finding across the three shipping tiers, and the sole
  genericity-related degraded body left (see plato-376's Resolution). Its
  siblings `AnimationTrack<T>` / `TangentTrack<T>` were moved to `stdlib/future`
  under stdlib-377 precisely because of this blocker; `Tween` stayed behind
  because `TimeVarying` is a shipping concept.
- `Tween<T>` has **zero references** anywhere in the corpus — no library body, no
  test, no other type mentions it. Nothing is broken for a user today.
- The real cost is that the ceiling is invisible: the next author who writes a
  generic type over an operated-on parameter re-discovers this the same way, at
  emission time. The workaround that exists — one spelling per value type — is
  visible as the five near-identical `DeCasteljau` overloads in
  `stdlib/geometry/splines-bezier.library.plato:32-55` (Point2D, Point3D,
  Vector2D, Vector3D, Number), whose bodies are identical modulo the type name.

## Affected code

- `stdlib/graphics/motion-graphics.types.plato:32` — `Tween<T>`, the symptom.
- `stdlib/graphics/time-varying.concepts.plato:11` — the `TimeVarying<TValue>`
  obligation being violated.
- `src/Plato.Compiler/Checking/TirEmitSource.cs:75` — `IsOpenGenericEmittable`,
  the open-generic emission fallback (landed 2026-08-01, `91bd6ac`) that
  deliberately *refuses* these bodies. Its rule: a call that dispatches on a bare
  abstract receiver stays a throwing stub, because a C# type parameter with no
  `where` clause cannot satisfy it. `Tween.Change` — which subtracts two bare
  `$TValue` — is the named example in that method's own doc comment.
- `parakeet/Parakeet.Grammars/PlatoGrammar.cs:194` — the `Type` rule:
  `Identifier + TypeParameterList + ImplementsList + ...`, with no
  `ConstraintList`. The `Concept` rule at line 198 has it.
- `src/Plato.AST/AstNodeFactory.cs:472` — hardcodes `Array.Empty<AstConstraint>()`
  for concrete types; line 501 reads the real list for concepts.
- `src/Plato.Compiler/Symbols/SymbolFactory.cs:513` — already populates
  `TypeParameterDef.Constraints` from `astTypeDeclaration.Constraints`
  uniformly, so the symbol layer needs no change at all: it is faithfully reading
  an always-empty list.
- `writers/Plato.CSharpWriter/CSharpFunctionInfo.cs:124,146` — `Constraints` /
  `ConstraintString`, which already emit F-bounded `where T : IConcept<T>` for
  FUNCTION type variables in wrapper mode. Nothing equivalent runs for a type
  declaration's own parameters.
- `src/Plato.Compiler/Analysis/Linter.cs:279` (LINT002) and `:562` — the only two
  readers of declared constraints today. Both read them **off the AST**; the
  comment at 562 states why: "'where' bounds are dropped by the resolver (see
  LINT002)".

## What makes this hard

The front end is small and mechanical; the semantics are the actual question.

**Front end (small).** Add `ConstraintList` to the `Type` grammar rule, stop
hardcoding the empty array in `AstNodeFactory`, and `SymbolFactory` starts
producing populated `TypeParameterDef.Constraints` with no further change.

**Back end (the design work).** Three open questions, none of them answered by
the front end landing:

1. **Are the bounds verified or decorative?** Today **nothing in checking
   consumes `TypeParameterDef.Constraints`** — the only readers are two lint
   rules over the AST, and the resolver drops them. If declared bounds land
   without a checking rule, `type Tween<T> where T: Interpolatable` is a comment
   with syntax. The valuable version rejects `Tween<String>` at the use site.
2. **How does the bound reach C#?** `where T : IInterpolatable<T>` on the emitted
   struct, presumably — the same F-bounded shape the writer already produces for
   functions. Then `IsOpenGenericEmittable` can be relaxed: a call dispatching on
   a bare `T` is fine *when `T` carries the bound that supplies the member*, and
   the throwing stub becomes a real body. That relaxation is the payoff and
   belongs in the same design.
3. **Do constraints propagate?** A library function over `Tween<$T>` must inherit
   `$T`'s bound, or the emitted C# will not compile.

One collision that is NOT in play: the 2026-08-01 decision to keep scalars as
wrapper structs (`decc091`, foundation tier emitted with wrapper scalars — no
`--scalar=float` erasure in the shipping recipe). Under erasure `T` could be
`float`, which satisfies no interface bound, and the whole F-bounded scheme would
need a parallel story. With wrapper structs every scalar is a struct that
implements the generated interface, so the bound is expressible. Reopening scalar
erasure (plato-370 does this for `double`) would reopen this interaction.

## Priority

p3. Severity is low and frequency is zero — one lint finding on a type with no
references, and no consumer is blocked. But it is the standing ceiling on generic
concrete types, and every deferral pays for it in hand-duplicated bodies
(`DeCasteljau` x5 today). It is safe to defer; it is not safe to forget, which is
why the interim options below are recorded rather than taken silently.

## Dependencies

- Blocked by: nothing. The pairing half (plato-376) already landed; this is the
  second, independent blocker its Resolution section calls out.
- Blocks: plato-376's last unticked box ("The remaining track blocker is either
  fixed or split into its own issue") — filing this satisfies the split.
- Touches: `parakeet/Parakeet.Grammars/PlatoGrammar.cs` and
  `src/Plato.AST/AstNodeFactory.cs` (grammar/AST — collides with any concurrent
  syntax work); `TirEmitSource.cs` (fresh code as of `91bd6ac`).

## Candidate answers

1. **Full design: declared bounds, verified, emitted.** Grammar + AST + a checking
   rule that rejects an unsatisfied type argument + writer emission of the `where`
   clause on the type + a relaxation of `IsOpenGenericEmittable` for bounded
   variables. Removes the last degraded body, and lifts the
   `DeCasteljau`-style duplication ceiling for everyone. Largest change, and the
   only one that makes the constraint mean anything.
2. **Front end only, bounds decorative.** Cheap, and actively harmful: it lets
   authors write a constraint the compiler ignores, which is exactly the trap
   LINT002 exists to catch for concepts.
3. **Interim, no language change — move `Tween` to `stdlib/future`.** Follows its
   two siblings under stdlib-377; `future` is neither linted nor emitted, so
   LINT001 goes to 0 and the genericity-related degraded body count to 0. Costs
   nothing (zero references) and hides nothing, provided this issue stays open as
   the reason.
4. **Interim, no language change — drop `implements TimeVarying<T>` and supply
   per-type `Sample` overloads.** The `DeCasteljau` pattern
   (`splines-bezier.library.plato:32`). Keeps `Tween` shipping and usable at
   concrete value types, at the cost of one body per type and the loss of the
   abstraction.

Options 3 and 4 are recorded so the shipping tiers can go clean before the design
lands; neither closes this issue.

## Bedrock

The invariant worth establishing: **a declaration may not promise what the type
system cannot check.** `Tween<T> implements TimeVarying<T>` is a promise with no
backing, and the current architecture discovers that three layers downstream, in
the C# writer, as a runtime throw. The seam is `TypeParameterDef.Constraints`
(`src/Plato.Compiler/Symbols/Definitions.cs:325`) — it exists, it is populated,
and it is read by nobody. Making it load-bearing turns an emission-time throw
into a declaration-time error, and gives `IsOpenGenericEmittable`
(`TirEmitSource.cs:75`) the information it is currently missing, which is the
difference between "emit a stub" and "emit the body".

Verdict: **right**. The interim options are the simple ones and both are honest,
but neither strengthens anything — they relocate or duplicate the symptom. If an
interim is taken, it must NOT delete `TimeVarying<T>` or fold `Sample` into
concrete types in a way that erases the record of what the abstraction was, or
the design work loses its motivating example.

## Done means

- [ ] An ADR in `tracker/decisions/` answers: do `type` declarations take `where`
      bounds, are those bounds checked, and how do they reach C#.
- [ ] Follow-up issues filed for whichever of grammar/AST, checking, and writer
      emission the ADR calls for.
- [ ] `Tween<T>` either compiles with a real `Sample` body, or has an
      issue-linked disposition recorded here (moved to `future`, or per-type
      overloads).
- [ ] LINT001 across the three shipping tiers is 0, and no genericity-related
      degraded body remains.
