---
date: 2026-07-29
title: Keep the `_` receiver convention; emit it as C# static abstract
status: superseded
superseded-by: 2026-07-29-static-interface-members-corrected.md
links: [tracker/issues/plato-306.md, tracker/issues/plato-307.md, tracker/issues/plato-308.md, submodules/Plato/Plato.CSharpWriter/CSharpFunctionInfo.cs, submodules/Plato/stdlib/algebra-operations.concepts.plato]
---

## Context

Plato spells a type-level (constructor-shaped) function as one taking a receiver it
ignores: `Zero(_: Color): Color`, `Pi(_: Number): Number`, `FromAmount(_: Self, x: Number): Self`.
The underscore means "I need the receiver's TYPE to dispatch, but not its value".
`CSharpFunctionInfo.IsStatic` reads this purely syntactically
(`ParameterNames.Count == 0 || ParameterNames[0] == "_"`) and emits a C# `static` method.

Two things collided while landing plato-306/307.

First, `Additive` declares `Zero(x: Self): Self` — a *named* receiver, so the generated
interface member is an ordinary instance method — while the implementations spell it
`Zero(_: Color)`, so the writer emits `public static Color Zero()`. A static method cannot
implement an instance interface member: 40 × CS0736 across `Zero`/`One`/`MinValue`/`MaxValue`
on `Color`, `Complex`, `Proportion`, `Percent`, `Probability`. Nothing caught the
declaration/implementation disagreement, because it is carried in a *parameter name*, and
parameter names are semantically inert everywhere else in the language. It surfaced only as a
C# compile error 1232 generated files downstream.

Second, this raised whether interfaces should lower to C# interfaces at all, since (pre-C# 11)
interfaces could not carry static members.

## Decision

1. **Keep the `_` convention. Do not add a `static` keyword to Plato.**
2. **Promote `_` from a naming convention to a checked signature element**: an implementation's
   receiver-usage marker must match the obligation it discharges, enforced by the checker.
3. **Emit `_`-receiver interface members as C# `static abstract` interface members**, with the
   implementing type supplying a plain `static` method — the `System.Numerics.INumber<T>` shape.
4. **Additionally emit a UFCS extension method** (`static Color Zero(this Color _) => Color.Zero();`)
   so Plato's `x.Zero` call syntax survives into C#.
5. Consequently the interface declarations change to `Zero(_: Self): Self` and the twenty
   instance-ified bodies from Plato `b055944` are reverted.

## Rationale

The `_` convention is information-equivalent to a `static` keyword — the writer already reads
it correctly, which proves the information is fully present and machine-readable. It also buys
real economy: Plato keeps ONE dispatch rule (first parameter is the dispatch position) instead
of gaining a second kind of function with its own resolution, overload, and `Self`-binding
story. It costs no grammar, no keyword, and no new binding form, and `_: Self` keeps the
receiver type participating in monomorphization exactly like any other member.

The genuine weakness is not the spelling but that **modifiers are part of a signature and names
are not**. `static` would sit where the checker already validates compatibility; a parameter
name sits where nothing looks. That is the specific mechanism that would have caught all twenty
bodies at lint time. Decision 2 closes exactly that gap without paying for the keyword.

`static abstract` interface members shipped in C# 11 and are available on `net8.0` at the
default language version, so they do not violate the "no new C# language features" hard rule.
They express the actual obligation ("every implementor supplies a zero") rather than encoding
it as "a function from Self to Self".

Decision 4 was **verified empirically before adopting**, not assumed. A static member does not
block an instance-style extension call of the same name: C# 7.3's improved-overload-candidates
rule discards static members from the candidate set when the method group is invoked with an
instance receiver, leaving nothing applicable, so normal processing falls back to extension
invocation. CS0176 never fires. Confirmed on net8.0/default LangVersion for the nullary case
(`c.Zero()`), the n-arg case (`c.FromAmount(5f)`), static-through-the-type (`Color.Zero()`),
and generic use under a constraint (`T.Zero()` with no instance in hand, including on an empty
array).

## Alternatives rejected

- **Add a `static` keyword to Plato.** Rejected: buys only what decision 2 buys, at the cost of
  a second dispatch rule, new grammar, and a separate account of how `Self` binds in a static
  interface member.
- **Keep the instance-ified fix from `b055944`.** Rejected as the permanent answer. It works and
  it unblocked 40 errors, but it was chosen because it required no emitter work, not because it
  is right: it makes `Color.Zero` need a `Color` in hand to ask what the zero `Color` is, and it
  forced signatures like `Zero(x: Color) => (0,0,0,0)` where `x` is named but never used — the
  convention has no way to say "ignores its receiver but must be an instance member", so the
  workaround lies in the opposite direction.
- **Stop emitting C# interfaces for interfaces entirely; emit concrete types only.** Rejected, but
  not by much, and the reasoning is worth preserving. Plato monomorphizes before emission, so
  the interfaces carry no weight in the generated code — they exist only so C# consumers can
  write generic code and see the contracts. Dropping them would delete the ~300 CS0315/CS0305
  errors of plato-308 outright. Retained because losing them removes the only mechanism for
  generic programming over the vocabulary from C#. Revisit if a survey of Studio call sites
  shows nobody constrains on them.

## Consequences

- The emitter gains a `static abstract` path; `CSharpFunctionInfo.IsStatic` stays the source of
  truth for which members take it.
- Every `_`-receiver member now emits **two** C# surfaces (static + extension). Slightly larger
  generated output; call-site ergonomics preserved in both directions.
- Interface declarations must be audited for the `x` vs `_` split — the stdlib is currently
  inconsistent, which is what produced the bug.
- The checker rule in decision 2 will surface further existing disagreements; expect a burn-down
  rather than a clean first run.
- This does NOT address plato-308's CS0315/CS0305 cluster, which is the separate question of
  interfaces used in *value* position (a field typed `Curve3D`) versus *constraint* position.
  Interfaces handle the latter and cannot express the former under an F-bounded encoding.
