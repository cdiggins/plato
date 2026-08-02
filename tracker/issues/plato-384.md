---
id: plato-384
title: Wrapper structs declare no Equals/GetHashCode/IEquatable: boxing and composition-dependent equality
type: debt
status: idea
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-01
closed:
links: [src/Plato.Intrinsics/Number.cs, src/Plato.Intrinsics/Integer.cs, src/Plato.Intrinsics/Boolean.cs, src/Plato.Intrinsics/Character.cs, src/Plato.Intrinsics/String.cs, tests/Plato.Intrinsics.Tests/NumberTests.cs, plato-383, plato-385]
---

## Issue

No wrapper struct in `src/Plato.Intrinsics` declares `Equals`, `GetHashCode`,
`operator ==`, or `IEquatable<T>`. The handwritten half of `Number`, `Integer`,
`Boolean`, `Character` and `String` has none of them — none in the whole folder.
Three consequences, two of which survive even when the generated partial is
present:

**1. Standalone, the wrappers fall back to `ValueType`.** The handwritten
sources are a *shared project* with no assembly of its own, and
`tests/Plato.Intrinsics.Tests` compiles them alone, with no generated partial
(its `.csproj` says so in a comment). In that composition `Number.Equals` is
`ValueType.Equals` — reflection-driven, boxing both operands on every call — and
`GetHashCode` likewise. Pinned by `StructEqualityIsFieldWiseNotIeee`
(`tests/Plato.Intrinsics.Tests/NumberTests.cs:167`).

**2. Composed with the generated partial, equality exists but still boxes in
generic code.** The generated half does supply the overrides — `_Number.g.cs:22-27`
emits `Equals(Number)`, `Equals(object)`, `GetHashCode`, `operator ==` / `!=`,
and the four other wrappers match — but `Equals(Number)` returns the wrapped
`Ara3D.Geometry.Boolean`, not `bool`, so it does **not** satisfy
`IEquatable<Number>`, and no wrapper declares that interface. So
`EqualityComparer<T>.Default` finds no strongly-typed comparer and falls back to
the object comparer: every `Dictionary<Number, _>` lookup, every
`List<Integer>.Contains`, every `Distinct` boxes. True for all five wrappers, in
every composition.

**3. NaN semantics diverge from IEEE, in both compositions.** `nan.Equals(nan)`
is `true` (field-wise standalone; `float.Equals` in the generated body) while
`nan >= nan` is `false` (`Number.cs:82`, real IEEE comparison). Both readings are
defensible — `Equals` is the reflexive dictionary-key contract, `>=` is IEEE —
but the current state picks one by accident rather than by decision, and nothing
on the type says which.

## Impact

- Anyone compiling `Plato.Intrinsics` without generated output — today, the
  runtime's own test suite — gets reflection-based equality on the most
  frequently compared types in the system.
- Every generic-collection use of a wrapper boxes, in every composition. That is
  the hot path for any code keying a dictionary by `Integer` or deduping
  `Number`s.
- The equality contract lives entirely in the *generated* half, so the
  handwritten wrapper is not self-consistent: whether `Number == Number` even
  compiles depends on what else is in the assembly.

## Affected code

- `src/Plato.Intrinsics/Number.cs`, `Integer.cs`, `Boolean.cs`, `Character.cs`,
  `String.cs` — the five wrappers, none declaring `Equals` / `GetHashCode` /
  `IEquatable<T>` / `==`.
- `tests/Plato.Intrinsics.Tests/NumberTests.cs:167` —
  `StructEqualityIsFieldWiseNotIeee`, pinning the standalone behaviour.
- `tests/Plato.Intrinsics.Tests/Plato.Intrinsics.Tests.csproj` — the composition
  with no generated partial.
- `generated/Plato.Generated.Foundation.Unoptimized/_Number.g.cs:22-27` (and the
  matching `_Integer` / `_Boolean` / `_Character` / `_String`) — where the
  overrides actually come from, and where the `Boolean`-returning `Equals`
  blocks `IEquatable<T>`.

## Cause / analysis

The handwritten/generated split was drawn around *Plato-visible* members: the
writer emits everything an interface obligation names, the handwritten file supplies
what Plato cannot express. `Equals(object)` and `GetHashCode` are C#
object-protocol members with no Plato obligation behind them, so they ended up on
the generated side by default rather than by decision — and `IEquatable<T>`,
which has no Plato analogue at all, ended up nowhere. The `Boolean` return type
on `Equals` is the same wrapper-family convention plato-385 records for the
comparison operators; here it has a measurable cost.

## Priority

Not urgent, compounds slowly. Nothing is observably wrong in shipped generated
code today — the overrides are present. The debt is that correctness depends on
the composition, and that the boxing is invisible until someone profiles.
Deferring is safe; the fix gets marginally harder with each new wrapper.

## Dependencies

- Touches: all five files in `src/Plato.Intrinsics`, plus the writer if the fix
  lands on the generated side. Collides with plato-383 (`String.cs`) and
  plato-385 (`Boolean.cs`).
- Related: plato-383 — `default(String).GetHashCode()` throws today for the same
  root reason its default is unsound; fixing that fixes this instance too.

## Fix approaches

1. **Declare `IEquatable<T>` with a `bool`-returning `Equals(T)` on each
   handwritten wrapper**, plus `override bool Equals(object)`, `GetHashCode`,
   `==`/`!=`. Kills the boxing and makes the wrapper self-consistent standalone.
   Risk: CS0102 duplicate-member collisions with the generated partial — exactly
   the failure mode plato-378 hit with `Zero`/`One` — so each member needs an
   owning half and the writer needs to skip the ones the runtime takes.
2. **Emit `IEquatable<T>` from the writer instead**, leaving the handwritten half
   empty. No collision risk, but the standalone composition (the test suite)
   stays on `ValueType`, so the pinning test stays a pin.
3. **Both, split by member**: writer emits nothing object-protocol; runtime owns
   `Equals` / `GetHashCode` / `IEquatable` / `==`. Cleanest ownership line,
   largest diff.

Whichever is chosen, record the NaN decision explicitly — `Equals` reflexive
(dictionary contract) vs IEEE — as a comment on the member.

## Bedrock

The invariant: **the object protocol on a wrapper is owned by exactly one half,
and the wrapper is correct standalone.** `src/Plato.Intrinsics` is a shared
project that compiles into several different assemblies, and its equality
semantics change depending on which one — that is the real defect, not the
boxing. Drawing the ownership line (approach 3) makes the handwritten runtime a
complete, testable artifact on its own, which is what
`tests/Plato.Intrinsics.Tests` is trying to be, and closes a class of CS0102
partial-collision surprises of the kind plato-378 hit.

Verdict: **simplest-along-the-grain**. The simple version is approach 1 on the
five wrappers. It must NOT be done by adding members while the writer keeps
emitting the same names — that trades a performance bug for a build break. The
writer's skip-list moves in the same change.

## Done means

- [ ] Each of the five wrappers implements `IEquatable<T>` with a
      `bool`-returning `Equals(T)`, and overrides `Equals(object)` /
      `GetHashCode`.
- [ ] No CS0102 in any composition: runtime standalone, and runtime + generated
      partial (forward conformance builds).
- [ ] `StructEqualityIsFieldWiseNotIeee` is rewritten to assert the *decided*
      semantics rather than pin the fallback, with the NaN choice stated in a
      comment on the member.
- [ ] A test asserts `EqualityComparer<Number>.Default` resolves to the
      strongly-typed comparer — the observable form of the fix.

## Simplest fix

Approach 1 on all five wrappers, with the writer's emission of the same members
removed in the same commit.

- Get: no boxing in generic collections, a self-consistent standalone runtime,
  one place to read the equality contract.
- Give up / risk: a partial-class ownership negotiation with the writer, and a
  behaviour change for anyone relying on the current `Boolean`-returning `Equals`
  overload resolution.

## Prevention

- The composition test in the Done-means list (build the runtime standalone AND
  composed) is the general guard; it is the same gap that let the plato-378
  `Zero`/`One` CS0102 cluster through.
- Worth its own idea: a documented ownership table for the handwritten/generated
  split — which members each half may declare. The writer already has a skip-list
  interface (`Linter.MembersImplementedByWriter`); the inverse list does not exist.
