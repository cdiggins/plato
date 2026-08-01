---
date: 2026-08-01
title: Declared type-parameter bounds are verified and emitted
status: accepted
superseded-by:
links: [../issues/plato-382.md, ../issues/plato-393.md, ../issues/plato-394.md, ../issues/plato-395.md, ../issues/plato-079.md, ../issues/stdlib-377.md]
---

## Context

Plato had no way to say *"this parameter must support these operations"* anywhere it mattered.
`concept` declarations took a `where` clause; `type` declarations did not, and library functions
did not. The clause a concept could write was decorative: nothing in checking read
`TypeParameterDef.Constraints`, the only readers were two lint rules over the AST, and the resolver
dropped it.

The consequence was that a generic concrete type could declare a concept it could not possibly
implement, and the failure surfaced three layers downstream, in the C# writer, as a throwing stub.
`Tween<T> implements TimeVarying<T>` was the shipping instance: any honest `Sample` must `Lerp`
between `From: T` and `To: T`, an operation ON a bare `T`, and nothing in the language let the
declaration require it. Its two siblings had already been moved to `stdlib/future` (stdlib-377)
for exactly this reason.

The cost on the other side of the same gap was duplication. `DeCasteljau` in
`stdlib/geometry/splines-bezier.library.plato` was written five times — Point2D, Point3D,
Vector2D, Vector3D, Number — with bodies identical modulo the type name, because its only
constructed parameter type is `Array<T>`, a `primitive` that is unbounded and must stay unbounded,
so the requirement on the element could not be spelled at all.

One collision was checked and is not in play: the 2026-08-01 decision to keep scalars as wrapper
structs. Under scalar erasure `T` could be `float`, which satisfies no interface bound, and the
F-bounded scheme below would need a parallel story. With wrapper structs every scalar is a struct
implementing the generated interface, so every bound is expressible. Reopening scalar erasure
would reopen this interaction.

## Decision

**Type declarations and library functions take `where`-clause bounds, and those bounds are both
VERIFIED and EMITTED.** Candidate answer 1 of plato-382, and candidate answer 1 of plato-393. A
bound is not a comment with syntax: it restricts what may be written, it licenses what a body may
do, and it reaches the generated C# as a real constraint.

Surface syntax is one clause shape in all three positions — `type`, `concept`, and a library
function — sitting after the parameter list (or, on a function, after the return type, which is the
last slot before the body). The target is named exactly as its declaration spells it: bare `T` on a
declaration's parameter, `$T` on a function's own signature variable.

```plato
type Tween<T> where T: Interpolatable implements TimeVarying<T> { ... }
DeCasteljau(xs: Array<$T>, t: Number): $T where $T: Interpolatable => ...;
```

### Verified

Four diagnostics, all errors, in `Checking/`:

- **CHK309** — a type argument at a CONSTRUCTION SITE does not satisfy the bound its parameter
  declares. `TypeConstraintChecker` walks every type expression a declaration writes: its
  implements/inherits list, its field types, a sum type's per-case field types, and every
  signature of every method it declares. `Tween<String>` is rejected where it is written.
- **CHK206** — a CALL SITE binds a function's bounded signature variable to a type that does not
  satisfy the declared bound. This is where a function bound differs from a type bound: a type is
  written, a function is called. Enforced in `Solver` as a candidate-viability rule, and reported
  as its own code rather than the misleading CHK201 "no overload matches", because the signature
  did match and only the bound failed.
- **CHK310** — a bound that does not name a concept. `where T: Number` promises something the
  language cannot check and C# cannot express as a constraint. Same rule for both places a bound
  can be written.
- **CHK205** — a member call on a bare bounded parameter that no declared bound supplies. The
  solver trials candidates bound-licensed first; if nothing is licensed it retries with licensing
  off, and a call that resolves only that way is using an operation the declaration never
  promised. It still resolves — elaboration and emission are unchanged — because a diagnostic is
  more useful than a cascade of "no overload matches" beneath it.

Satisfaction has ONE reading, in `Checking/TypeConstraints.cs`, shared by the construction-site
check, the solver's licence, and the emitter's licence, so those three can never disagree. It is
concept membership as `ConceptClosure` defines it: the same transitive, per-level-substituted walk
the solver already used for concept parameters. Two properties of that reading are deliberate:

- **An UNBOUNDED parameter stays permissive.** Plato does not require bounds and the whole forward
  vocabulary is written without them; rejecting the unbounded case would be a language change, not
  a constraint check. Bounds restrict where they are declared and change nothing where they are
  not.
- **A function's signature variable INHERITS the bounds of the constructed types its signature
  mentions.** `Sample(x: Tween<$T>, ...)` learns `$T: Interpolatable` from `Tween`'s own clause.
  A bound the function DECLARES joins the inherited ones as a second source of the same shape,
  through the single channel every consumer already read, so it needs no new plumbing in the
  solver, the emitter, or the writer.

### Emitted

The F-bounded shape the writer already produced for function type variables: a concept emits as
`interface C<Self, ...> where Self : C<Self, ...>`, so a bound `T: C<A>` reads `where T : C<T, A>`
— the bounded parameter itself occupies Self. One renderer, `CSharpBoundWriter`, serves both
surfaces (the generated struct's own parameters and a generic function's signature variables), so
the clause a caller must discharge and the clause a callee declares are spelled identically.

**Concept interfaces are excluded**, by the `TypeConstraints.EmittedToCSharp` policy: only bounds
declared on a CONCRETE type are carried into generated code. A concept's own `where` clause
predates bound checking, several shipping concepts carry one, and putting those on the generated
interfaces would propagate a constraint to every mention of the interface at once — a library-wide
change, not this one. The widening path is deliberate and narrow: relax `EmittedToCSharp` to admit
interfaces, fix up the resulting constraint obligations across the shipping vocabulary in the same
change, and re-run the generated build. Because `EmittedToCSharp` is a single predicate read by
both the emitter and `TirEmitSource.IsOpenGenericEmittable`, widening it moves the emission licence
and the body licence together; they cannot drift apart.

**Bound-licensed bodies emit real code.** `IsOpenGenericEmittable` used to refuse any body with a
call dispatching on a bare abstract receiver, because an unconstrained C# type parameter cannot
satisfy it. It now permits such a call when a bound the receiver is KNOWN to carry supplies the
member — the same licence the solver used to resolve it, and the emitted signature carries the
matching `where` clause, so the body is valid C#. Without a licensing bound the refusal stands.

### What it bought

`Tween<T> where T: Interpolatable` has a real `Sample` body, and the five `DeCasteljau` overloads
are one function. The forward lint ceiling (`ForwardStdLibLintTests.MaxLintRatchet`) came down in
the commit that earned it, and LINT001 — the undischargeable-obligation rule this feature exists to
answer — is now absent from the shipping tiers.

## Consequences

### Sum types are indifferent to all of this, by construction

The constraint and emission sides were built without a single test of `TypeDef.IsSum`. The
construction-site walk is over `TypeDef`, and a sum type's per-case field types are visited in the
same loop as a record's fields; the emission path renders the `where` clause through the sum
writer as it does through the record writer. Both are covered by tests
(`TypeConstraintCheckingTests.BoundsOnASumTypeParameter_UseTheSameCodePath`,
`TypeConstraintCodegenTests.ABoundedGenericSum_EmitsTheWhereClauseThroughTheSumPath`), which pass
today only because they construct their own bounded generic sum — the language still rejects one.

That is the groundwork, and it is worth stating precisely what it does and does not settle. A
generic sum is rejected by **CHK306**, and lifting that restriction is `plato-079`, not this
decision. What this decision removes from plato-079's cost is only the constraint half: bounds on a
sum's parameters need no new machinery. What remains for plato-079 is the part CHK306 actually
guards — generalizing the checker over a bare `T` in sum position, and auditing the generated
case factories, the case predicates, and the `match` lowering (which lowers to conditionals with no
new TIR node) for the same generalization. `stdlib/CONVENTIONS.md`'s no-generic-`Optional<T>` rule
stands until that work is done.

### Open edges

- **plato-394** — a function bound whose variable the writer folds into the RECEIVER type's own
  parameter loses its emitted clause, and would produce C# that does not compile. Checked
  correctly, emitted wrongly, with nothing between the author and the compiler error. Not
  reachable from any shipping tier today: the forward stdlib's only function bound is over the
  `primitive` `Array`, in the non-emitted `geometry` tier.
- **CHK206 reports the first failure only.** A call whose arguments violate several declared
  bounds names one of them. Correct but terse; fixing it is a message change, not a semantics one.
- **plato-395** — `TimeVarying<TValue>` still carries no bound, so its derived `Change` (which
  subtracts two bare values) remains a throwing stub. The blocker is scope, not machinery: the
  concept's implementors span `graphics` and the non-shipping `future` tier, so the bound cannot be
  filled in one package.
- **The legacy checker ceiling went UP by one, and that is the feature working.** Promoting CHK205
  to an error surfaced `IInterval.Size` in `stdlib-legacy`, whose only diagnostic had been the
  warning form. `interface IInterval<T> where T: IVectorLike` then uses `Add`/`Subtract` on a bare
  `T`, which `IVectorLike` does not supply — a real under-promise the bound now names. It is not
  fixable by strengthening the clause (`IInterval<Point2D>` would stop being writable), so
  `MaxFunctionsWithDiagnostics` in `tests/PlatoTests/CheckerCompletenessTests.cs` was re-pinned
  with the reason recorded beside it. The fix is the `IInterval`/`IBounds` redesign already named
  there, not a change to bounds.
