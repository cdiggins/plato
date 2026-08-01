---
id: plato-385
title: Boolean comparison operators return raw bool while Number/Integer return the wrapper
type: debt
status: idea
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-01
closed:
links: [src/Plato.Intrinsics/Boolean.cs, src/Plato.Intrinsics/Number.cs, src/Plato.Intrinsics/Integer.cs, src/Plato.Intrinsics/Character.cs, src/Plato.Intrinsics/String.cs, plato-384]
---

## Issue

The wrapper family in `src/Plato.Intrinsics` does not agree on what a comparison
returns. `Number` and `Integer` return the wrapped `Ara3D.Geometry.Boolean`;
`Boolean`, `Character` and `String` return raw C# `bool`.

| type | `<` `<=` `>` `>=` return |
|---|---|
| `Number.cs:70-82` | `Boolean` |
| `Integer.cs:125-137` | `Boolean` |
| `Boolean.cs:85-94` | `bool` |
| `Character.cs:58-67` | `bool` |
| `String.cs:56-65` | `bool` |

Reported as a `Boolean`-only inconsistency; the sweep found it is a 2-vs-3 split
across the whole family. Nothing marks either group as the intended convention —
no comment, no ADR — so the split reads as accretion, not design.

## Impact

No observed miscompilation. `Boolean` declares implicit conversions in both
directions (`Boolean.cs:38,41`), so the generated partials paper over the
difference: `_Character.g.cs:33` and `_String.g.cs:33` both emit
`public Boolean LessThanOrEquals(X b) => this <= b;` and the raw `bool` converts
silently on the way out. That is exactly why this has never surfaced as a bug —
and why it is worth recording, since the inconsistency is currently invisible.

The costs are real but second-order:

- A hand-written consumer of the runtime gets a different static type depending on
  which wrapper it is comparing, so generic or macro-style code over the family
  cannot be written uniformly.
- The two-way implicit conversion that hides the split is itself a hazard: with
  `bool` and `Boolean` interconvertible in both directions, overload resolution
  can pick a different member than the reader expects, and an accidental
  `Boolean`-vs-`bool` mismatch never gets flagged.
- It obscures the ownership question in plato-384: `Equals` returning `Boolean`
  rather than `bool` is what blocks `IEquatable<T>` there. The same convention,
  applied one member over, has a concrete cost.

## Affected code

- `src/Plato.Intrinsics/Boolean.cs:85-94` — the four raw-`bool` comparison
  operators, under a "Comparison operators" banner with no explanation.
- `src/Plato.Intrinsics/Character.cs:58-67`, `String.cs:56-65` — same shape.
- `src/Plato.Intrinsics/Number.cs:70-82`, `Integer.cs:125-137` — the
  `Boolean`-returning group.
- `src/Plato.Intrinsics/Boolean.cs:38,41` — the two implicit conversions that
  make the difference invisible.

## Cause / analysis

Speculation, but well supported by the file layout: the `Boolean`-returning
operators sit next to the arithmetic ones on `Number`/`Integer`, where returning
the wrapper keeps a fluent chain in the wrapper world
(`a.LessThan(b).And(...)`). `Boolean`/`Character`/`String` got their comparisons
written as thin forwards to the underlying `bool`/`char`/`string` comparison,
where raw `bool` is the natural thing to type. Neither author had a stated rule
to follow.

## Priority

Low. Nothing is broken and nothing is blocked; this is a tidiness and
uniformity item that becomes relevant when someone writes code generic over the
wrapper family, or when plato-384 forces the `bool`-vs-`Boolean` question
anyway. Safe to defer indefinitely; best done *with* plato-384, since both
change the same convention in the same files.

## Dependencies

- Touches: `Boolean.cs`, `Character.cs`, `String.cs` — shared with plato-384
  (equality) and plato-383 (`String.cs` null default).
- Best sequenced with plato-384: that issue has to decide `bool` vs `Boolean` for
  `Equals`, and the answer should be the same rule.

## Fix approaches

1. **Standardize on `Boolean`** — change the three raw-`bool` types to match
   `Number`/`Integer`. Consistent with the "everything is a wrapper" direction
   confirmed by the 2026-08-01 scalar decision (`decc091`, wrapper scalars, no
   erasure). Risk: `if (a < b)` on a `Character` now needs the implicit
   conversion to `bool`, which exists, so it keeps compiling — but any `??`,
   pattern match, or overload resolution near the call site can shift.
2. **Standardize on `bool`** — the reverse. Cheaper for interop and for `if`,
   but fights the wrapper convention and would make `Number` comparisons stop
   chaining in the wrapper world.
3. **Write the rule down and leave the code alone.** Zero risk, zero benefit
   beyond documenting an inconsistency as intentional — which it is not.

## Bedrock

The invariant worth stating: **one rule for what a Plato wrapper's operators
return, applied to every wrapper.** The seam is `src/Plato.Intrinsics` as a
family rather than five independent files — the same seam plato-384 identifies
for the object protocol. Fixing this alone strengthens little; fixing it as part
of a written wrapper-surface convention (with plato-384) is what makes the next
wrapper correct by default and lets the two-way implicit conversion on `Boolean`
be reconsidered rather than relied on.

Verdict: **simplest-along-the-grain**. Do approach 1 as a mechanical change, but
it must NOT be landed as a silent type change: the convention has to be written
down in the same commit, or the next wrapper re-introduces the split.

## Done means

- [ ] All five wrappers' `<` `<=` `>` `>=` return the same type.
- [ ] The rule is stated once, in a comment at the top of the wrapper family or
      in the runtime's own docs, so a new wrapper has something to follow.
- [ ] `tests/Plato.Intrinsics.Tests` passes, and forward conformance builds
      (the generated partials call these operators directly).

## Simplest fix

Approach 1: change the twelve operator signatures on `Boolean`, `Character` and
`String` to return `Boolean`, and write the rule down beside them.

- Get: a uniform family surface; the `bool`/`Boolean` question is answered once
  for plato-384 to follow.
- Give up / risk: hand-written consumers relying on the raw `bool` return see a
  type change (source-compatible via the implicit conversion, but not
  binary-compatible), and the change touches files with two other open issues.

## Prevention

- The written convention above is the prevention. A stronger version — a test or
  lint over `src/Plato.Intrinsics` asserting that every wrapper's comparison
  operators return the family type — is worth its own idea, and would also cover
  the plato-384 object-protocol members.
