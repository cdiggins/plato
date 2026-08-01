---
id: plato-383
title: default(String) wraps a null system string; every observation throws
type: bug
status: done
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: [src/Plato.Intrinsics/String.cs, tests/Plato.Intrinsics.Tests/StringTests.cs, plato-384]
---

## Issue

`Ara3D.Geometry.String` is a struct wrapping a `string` reference
(`src/Plato.Intrinsics/String.cs:17`), so `default(String)` wraps `null`. Every
observation on it then throws.

Observed (pinned by `DefaultWrapsANullString` in
`tests/Plato.Intrinsics.Tests/StringTests.cs:83`):

- `default(String).Value` is `null`
- `default(String).Count` throws `NullReferenceException`
- `default(String).At(n)` likewise, and so does every comparison operator, which
  all go through `a.Value.CompareTo(...)` (`String.cs:56` onward)

Expected: the default of a Plato wrapper is a sane zero value. Every other
wrapper in `src/Plato.Intrinsics` satisfies that — `default(Number)` is `0`,
`default(Integer)` is `0`, `default(Boolean)` is `false`, `default(Character)` is
`'\0'`. `String` is the only member of the family whose default is a trap, and
the natural zero — the empty string — exists.

The test currently *pins the broken behaviour* as "current behavior", so the
suite is green today; fixing this means inverting that test.

## Impact

Any generated code path that reaches an uninitialized `String` throws instead of
producing an empty string: a `String` field of a struct constructed via
`default`, a `String` in a zero-initialized array or a `new T[n]`, a
`default(T)` in a generic body. C# offers no way to prevent a struct from being
default-constructed, so this cannot be closed off at the source — the wrapper has
to tolerate it.

Frequency today is low (the shipping stdlib is numeric and geometric; `String`
is barely used in generated bodies), which is why it has gone unnoticed. It
becomes routine the moment string-carrying types enter the stdlib.

## Affected code

- `src/Plato.Intrinsics/String.cs:17` — `public readonly string Value`, the
  nullable field that makes the default unsound.
- `src/Plato.Intrinsics/String.cs:31,36` — `At`, `Count`: the two Plato-visible
  observations that throw.
- `src/Plato.Intrinsics/String.cs:43,56+` — `ToSystem` and the comparison
  operators, which propagate the null.
- `tests/Plato.Intrinsics.Tests/StringTests.cs:83` —
  `DefaultWrapsANullString`, which pins the bug and must be inverted by the fix.

## Cause / analysis

A wrapper over a *reference* type has a default the wrapper author did not
choose. `Number`/`Integer`/`Boolean`/`Character` all wrap value types whose
zero is meaningful, so the same design is sound for them; `String` inherited the
pattern without inheriting the property. Nothing indicates this was deliberate —
there is no comment or ADR defending it.

## Priority

Deferrable. Severity is high per occurrence (a `NullReferenceException` out of
code that reads as total) but frequency is near zero in the shipping corpus.
Cost of deferral does not compound, but the fix is cheap and the failure mode is
the kind that shows up first in someone else's generated code, so it is worth
doing before string-carrying types arrive rather than after.

## Dependencies

- Touches: `src/Plato.Intrinsics/String.cs`, shared with plato-384 (which adds
  `Equals`/`GetHashCode` to the same wrapper family). Land them in either order,
  but expect a textual conflict in `String.cs`.

## Fix approaches

1. **Normalize on read.** `Value` becomes a property returning
   `_value ?? string.Empty`; `Count`, `At`, `ToSystem` and the operators all go
   through it. Total by construction, no allocation, and no caller can observe a
   null. Costs a null check on every access, and `Value` stops being a field
   (check whether `[DataMember]` serialization and any generated code depend on
   field-ness).
2. **Normalize in the constructor.** `String(string value) => Value = value ?? "";`
   Cheaper at read time, but does not help `default(String)`, which never runs a
   constructor. Fixes only half the problem, so at best it is an addition to 1.
3. **Make the observations total individually** — `Count => Value?.Length ?? 0`,
   and so on. Same effect as 1 with more places to forget one.

## Bedrock

The invariant: **a Plato wrapper's `default` is its zero value, on every wrapper,
without exception.** That is what lets generated code use `default(T)` and
zero-initialized arrays without knowing which wrapper it holds. `String` is the
single violation; fixing it at the accessor (approach 1) restores the invariant
for the whole family rather than patching the two members someone happened to
call. It also makes the family rule statable and testable — a test that asserts
"the default of every wrapper is observable without throwing" belongs in
`tests/Plato.Intrinsics.Tests`.

Verdict: **right**, and it is also nearly the simplest thing.

## Done means

- [x] `default(String).Count` is `0` and `default(String).At(...)` behaves as on
      the empty string — no `NullReferenceException` from any Plato-visible member.
- [x] `DefaultWrapsANullString` in `tests/Plato.Intrinsics.Tests/StringTests.cs`
      is inverted into a regression test asserting the empty-string default.
- [x] A family-level test asserts that every wrapper in `src/Plato.Intrinsics`
      survives observation of its `default`.
- [x] `tests/Plato.Intrinsics.Tests` passes.

## Simplest fix

Approach 1: route every observation through a null-normalizing accessor.

- Get: `default(String)` behaves as the empty string everywhere, including in
  code nobody has written yet.
- Give up: `Value` stops being a plain readonly field, so serialization
  (`[DataMember]`) and any generated code that touches the field directly need
  checking; a branch on every access (predictable, effectively free).

## Prevention

- The family-level default test in the Done-means list is the general fix: it
  catches the next wrapper over a reference type.
- Worth its own idea: a rule that a Plato wrapper struct may not have a field of
  reference type unless the wrapper normalizes it. That is a checkable property
  of `src/Plato.Intrinsics` and would have prevented this at authoring time.
