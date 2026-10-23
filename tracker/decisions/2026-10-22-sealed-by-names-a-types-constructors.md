---
date: 2026-10-22
title: "sealed by F, G" names the only functions that may construct a type
status: accepted
superseded-by:
links: [../../docs/SEMANTICS.md]
---

## Context

Plato has no visibility modifiers, and every product type gets a public constructor (its fields,
and one tuple of them when it has several), so any function can build any value. A proof type
(a value whose existence says a check passed, such as the STEP mesher's `CheckedMesh`) is then a
convention: nothing stops a function other than the checker from building one. The STEP mesher's
types review in bim-open-toolkit (`docs/plans/step-mesher-types-review-2026-10-22.md`, TKT-435)
traced six recorded defects to a flag where a proof was needed. Ticket TKT-444 asked for a type
to name the functions allowed to construct it, with a construction anywhere else a compile error,
and for the law runner to build such a type only through those functions or its witnesses.

## Decision

A type declaration takes an optional clause after `implements`:

```plato
type CheckedMesh
    sealed by CheckMesh
{
    Mesh: Mesh;
    Tolerance: Number;
}

type Verdict sealed by Judge = Pass | Fail(Reason: Number);
```

- A function may construct the type when its name is in the list and it returns the type; every
  overload of that name returning the type counts. Each name must have at least one such
  function, or compilation halts with a resolution error.
- Anywhere else, the type checker reports **CHK311** for a constructor call, `new`, a tuple
  converted to the type (at a return or as an argument), a sum case, or `default`. It runs in
  `Checking/SealedConstruction.cs`, per function, over the overloads and coercions the solver
  committed, so it sees what the elaborator emits.
- Field reads, `match`, and calls to the named functions stay allowed everywhere. Emission does
  not change: the generated C# constructor stays public.
- The law runner (`ValueGenerator`) draws a sealed type from its generators when it has any, as
  TKT-446 made it do for any type; otherwise from its named functions that have a body, called
  on drawn arguments to the depth limit; its witnesses join the draw as before. It never builds
  the type field by field.

## Rationale

The grammar change is one optional rule, `SealedList`, in parakeet's `PlatoGrammar` (parakeet
`ara3d/parakeet` main), and one child of `Type`. The forms that need no grammar change were
weighed first:

- A `where` bound naming the functions as a type (`type Check where sealed: (CheckWithin, WorseOf)`)
  parses today, but every reader of declared bounds (the solver's inherited bounds, CHK309 and
  CHK310, the writers that emit bounds, the declaration writer) would have to learn to skip it,
  and the function names would sit in a type position that resolves them as types.
- A library function by naming convention, like `Law_`, `Witness_` and `Generate_`, cannot carry
  a list of function names except as a string literal, which navigation cannot follow and a
  rename would not update.
- A comment form is dropped by the parser's whitespace rule, so the compiler never sees it.

The clause sits where `implements` sits, reads as the sandbox proposal spells it
(bim-open-toolkit `sandbox/step-brep-mesh/PLATO-SPEC-EXTENSIONS.md` section 2.2), and a reader
that does not know it ignores one optional child.

The check is a type-checker diagnostic rather than a resolution error because only the solver
knows which overload a call resolved to and where a tuple became a struct; the symbol tables see
a call to the group named `CheckedMesh` and no tuple conversion at all.

## Alternatives rejected

- Private constructors with a visibility modifier — a general access-control feature for one
  need, and SEMANTICS.md excludes visibility on purpose.
- Refusing a tuple conversion to a sealed type inside the solver — a refused conversion surfaces
  as a generic unification or no-match error, not as its own code, and trial matching would see
  the refusal too.
- Halting compilation on CHK311 — every other checker error is reported, not halting, and the
  writers already degrade a body that fails to check.

## Consequences

- A sealed constructor without a body (an intrinsic, or one still to be written) contributes no
  values to the law runner; a law over a type with no drawable constructor and no witness reports
  why.
- A named function that faults on drawn arguments makes the law an error naming the function, as
  a faulting generator does.
- A sealed generic type is checked, but the runner can draw it only from a generator or a
  witness, since its constructors' signature variables cannot be drawn.
- The STEP mesher's proof types are not sealed by this change; TKT-451 seals them.
