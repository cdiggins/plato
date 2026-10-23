---
date: 2026-10-23
title: Generic sums are supported, instantiated at each use like generic products
status: accepted
superseded-by:
links: [../../docs/SEMANTICS.md, ../../docs/design/plato-sum-types-design-2026-07-27.md, ../../stdlib/CONVENTIONS.md]
---

## Context

The sum-type design (`docs/design/plato-sum-types-design-2026-07-27.md`, section 3.5) allowed
generic sums if the monomorphizer specialized them with no new unification machinery, and fell
back to rejecting them (CHK306) when the v1 front end hit a snag. The snag the checker's comment
named was a library consumer writing bare `T` (`GetOrElse(o: Option<T>, fallback: T)`), which does
not resolve. Without generic sums the STEP mesher in bim-open-toolkit hand-copied 39 `Decided*`
sums and used -1 as "none" in 89 places; its types review (TKT-435) counted sentinels and defaults
as the largest class of its recorded defects. Ticket TKT-443 asked for `type Maybe<T> = Just(Value:
T) | Nothing;` to type-check, match exhaustively, run in the interpreter, compile in the C# writer,
and be drawn by the law runner.

## Decision

Generic sums are supported by default; `SumTypeChecker`'s restriction (CHK306) is off unless a host
asks for it.

- **Spelling.** The declaration names its parameter (`T`); a library signature spells the free type
  variable `$T`, as every generic signature in the stdlib already does. Bare `T` outside the
  declaration stays a resolution error, and its message now names the `$T` spelling. The "gap" was
  a convention, not missing machinery: no auto-generalization of bare names was added.
- **Instantiation.** A payload case (`Maybe.Just(n)`) instantiates through its synthesized factory,
  whose signature mentions the sum's type parameter, like any generic call. A nullary case
  (`Maybe.Nothing`) now takes fresh type variables at each use, so the context grounds it; before,
  it carried the declaration's own `Maybe<T>`.
- **C#.** One generic struct per sum (`Maybe<T>`), as generic products emit today. A case built at a
  use names the instance (`Maybe<Integer>.Just(n)`). A library function over a closed instance
  (`F(m: Maybe<Number>)`) cannot be a member of `Maybe<T>`, so it is always an extension method in
  its library's class; until now such a function, for a generic product too, was silently left out
  of the output.
- **Law runner.** A generic instance is drawn with its type arguments substituted into its case and
  field types, recursively (`Maybe<Array<Number>>`, `Maybe<Maybe<Integer>>`). A type argument is
  drawn by its own rules: its `Generate_` function (TKT-446), or its sealed constructors (TKT-444).
  A `Generate_` function for a generic sum is matched against the whole instance
  (`Generate_X(seed): Maybe<EdgeIndex>` serves only `Maybe<EdgeIndex>`). The interpreter records
  the instance a case factory builds from the call site's type when that type is ground, and the
  runner stamps a drawn value whose type a generic body left open with the instance it drew, so a
  value handed to the compiled backend (a sealed constructor's result, a generator's) names
  `Box<Number>`, not `Box<T>`.

## Rationale

Every piece reuses machinery that already handled generic products: the solver treats a type
parameter in a callee's signature as a hole, the monomorphizer specializes bodies per receiver, and
the C# writer already emits generic structs. The work was in the four places where sums took a
path products did not (the nullary case reference, the factory's `new`, the factory call's
receiver, and the generator's sum branch), plus the closed-instance hole products shared.

## Alternatives rejected

- **Auto-generalizing bare type parameter names in library signatures.** It would make `T` mean
  "free variable" in a signature only when some generic type happens to declare a `T`, a rule no
  other generic code follows. The stdlib convention (`$T`) already covers it.
- **Emitting one C# struct per instantiation (`Maybe_Number`).** The design doc's first sketch. C#
  generics already give one struct per instantiation at run time, products emit that way, and the
  law backend's reflection finds generic instances by their open definition.
- **Keeping CHK306 on and shipping a hand-written `Maybe` per payload type.** That is the mesher's
  present state, and the defect record it produced is the reason for this ticket.

## Consequences

- `stdlib/CONVENTIONS.md` sanctions a generic sum as a fourth style for partial operations; the
  shipping tiers declare no `Maybe<T>` yet. Converting the mesher's sentinels and `Decided*` sums
  is bim-open-toolkit's TKT-450.
- Not covered, each failing loudly rather than with a wrong answer: a generic sum nested inside a
  value that a generator or sealed constructor built through a generic body (a `Pair<Maybe<Number>>`
  whose inner `Maybe` a `$T` helper made) keeps the open inner type in the interpreter, and the
  compiled backend refuses it ("no C# type for Maybe<T>"), because the runner stamps only the
  outer value with the instance it drew; a library function over `Maybe<$T>` named like a case predicate (`IsJust`) is a
  second member of the generic struct and the C# does not compile (CS0111), since a generic
  type keeps its functions as members; a library function over a closed instance whose name is a
  no-arg property elsewhere keeps the name-global call syntax (LINT014's existing rule); the
  TypeScript and Rust writers still reject every sum (CHK320).
- A function whose first parameter is a bare type variable (`Wrap(x: $T)`) is still rejected by the
  writer, as for any generic code; it is not a sum restriction.
