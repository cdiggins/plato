---
id: plato-312
title: Emit _-receiver interface members as C# static abstract
type: debt
status: done
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [tracker/decisions/2026-07-29-static-interface-members.md, submodules/Plato/Plato.CSharpWriter/CSharpFunctionInfo.cs, submodules/Plato/stdlib/algebra-operations.concepts.plato, tracker/issues/plato-308.md]
---

## Issue

Implements [ADR 2026-07-29 static interface members](../decisions/2026-07-29-static-interface-members.md).

Plato spells a type-level function as one with an ignored receiver — `Zero(_: Color): Color`.
`CSharpFunctionInfo.IsStatic` reads that syntactically and emits a C# `static` method. But the
interface declares `Zero(x: Self): Self` with a *named* receiver, so the generated interface
member is an ordinary instance method, and a static method cannot implement it: 40 × CS0736.

Plato `b055944` unblocked this by renaming the receiver in twenty implementation bodies so they
emit as instance members. That was chosen because it needed no emitter work, not because it is
right — it makes `Color.Zero` require a `Color` in hand, and it forces signatures like
`Zero(x: Color) => (0.0, 0.0, 0.0, 0.0)` where `x` is named but never used. The ADR decides the
proper route: emit these as C# `static abstract` interface members, and revert the rename.

## Impact

Low urgency, real cleanliness cost. Today `Color.Zero`, `Complex.One`, `Percent.MinValue` and
the rest read as instance members in the generated C# API, which is backwards for a constant.
Generic C# consumers also cannot reach them: `T.Zero()` under `where T : Additive<T>` is only
available if the interface member is `static abstract`.

The underlying trap has no gate at all, which is the part that actually bites: an obligation
fill whose staticness disagrees with its obligation is silent in Plato and surfaces as a C#
compile error more than a thousand generated files later. That is how twenty bodies drifted.

## Affected code

- [CSharpFunctionInfo.cs:60](../../submodules/Plato/Plato.CSharpWriter/CSharpFunctionInfo.cs:60) — `IsStatic => ParameterNames.Count == 0 || ParameterNames[0] == "_"`; stays the source of truth, gains a `static abstract` consumer.
- [algebra-operations.concepts.plato](../../submodules/Plato/stdlib/algebra-operations.concepts.plato) — `Additive.Zero(x: Self)` should become `Zero(_: Self)`.
- [algebra-numeric.concepts.plato](../../submodules/Plato/stdlib/algebra-numeric.concepts.plato) — `NumericalLimits.One`/`MinValue`/`MaxValue`, same.
- [quantities.concepts.plato](../../submodules/Plato/stdlib/quantities.concepts.plato) — `FromAmount(_: Self, x: Number)` already uses `_`; the n-arg case.
- [algebra-metric.concepts.plato](../../submodules/Plato/stdlib/algebra-metric.concepts.plato) — `OriginBased.FromOffset(_: Self, d: TDelta)`, same shape.
- `stdlib/color.library.plato`, `stdlib/numbers.library.plato` — the twenty bodies renamed in `b055944`, to be reverted.
- Wherever the interface member is written in `Plato.CSharpWriter` (the `Interfaces.g.cs` emission path).

## Cause / analysis

The `_` marker is information-equivalent to a `static` keyword, and the writer already reads it
correctly. The weakness is that **modifiers are part of a signature and parameter names are
not**: nothing in the checker compares an implementation's receiver-usage marker against the
obligation's, because names are semantically inert everywhere else in Plato. So the two halves
drifted apart silently.

## Dependencies

- Blocked by: nothing.
- Related: [plato-308](plato-308.md) — the forward suite cannot build yet, so the CS0736 class cannot be re-verified end to end there until it does. The change is independently verifiable through `regen-generated.ps1` and the legacy conformance suite.
- Touches: `Plato.CSharpWriter` (emitter behaviour ⇒ golden refresh, hard rule 2) and the `algebra-*`/`quantities` interface files.

## Fix approaches

1. **Full ADR route** (recommended): emit `static abstract` for `_`-receiver interface members, emit the paired UFCS extension method, flip the interface declarations to `_`, revert the twenty renames, add the checker rule.
2. **Emitter only, leave declarations alone.** Rejected by the ADR — it leaves the interface still declaring an instance member, so the mismatch remains, just relocated.
3. **Keep `b055944` as the permanent answer.** The zero-work option. Costs the backwards API shape and blocks `T.Zero()` for generic consumers.

## Bedrock

Makes the `_` marker a *contract* rather than a *convention*: it becomes part of what the
checker validates, so an obligation fill can no longer silently disagree with the obligation it
discharges. That is the invariant the 40 CS0736 errors violated, and the checker rule is what
stops the whole class recurring — in any backend, not just C#.

**Verdict: simplest-along-the-grain.** The simple fix must NOT be "keep the rename and move on":
that leaves the marker unchecked, so the next drift is silent again and lands just as far
downstream.

## Done means

Revised against [the corrected ADR](../decisions/2026-07-29-static-interface-members-corrected.md) —
the original boxes 2 and 3 were written from an assumption that measurement disproved.

- [x] `_`-receiver interface members emit as `static abstract` interface members; implementors emit plain `static`. — behind opt-in `--static-abstract`; emits exactly `Quantity.FromAmount`, `Vector.FromComponents`, `OriginBased.FromOffset`.
- [x] ~~A paired UFCS extension method~~ — **not needed**. Generated code monomorphizes before emission, so no call site requires it. The mechanism was verified to work on net8.0 (nullary, n-arg, static-via-type, generic `T.Zero()`) and stays available if a C# consumer scenario ever needs it.
- [x] ~~Interface declarations use `_`; the twenty `b055944` renames are reverted.~~ — **withdrawn.** `Zero`/`One`/`MinValue`/`MaxValue` stay instance obligations: `VectorN.Zero` and `MatrixN.Zero` need the receiver for arity, and `Self.` does not exist in the forward stdlib. `b055944` was right. Instead the genuine drift was fixed in the other direction: `Vector.Broadcast` is now an instance obligation, with its six fixed-arity implementations renamed to match.
- [x] Checker rule: an implementation's receiver-usage marker must match the obligation's; existing disagreements burned down or listed. — **LINT012**, Plato `8d488a8`.
- [x] `regen-generated.ps1` goldens byte-identical (368 files, 0 differing) — no refresh needed, the flag is opt-in and default-false.

### LINT012 (Plato `8d488a8`)

Pairs each obligation with the implementation discharging it by substituted signature — the same
pairing the writer and LINT001 use — and reports disagreement on the `_` marker. Anchored at the
implementation, so a finding lands once on the edit site rather than once per implementing type.
Skips `MembersImplementedByWriter` (synthesized members have no authored receiver) and leaves
unimplemented obligations to LINT001.

Findings: **stdlib-legacy 0** — internally consistent, so no burn-down and good evidence the rule
is not noisy. **stdlib 2**, listed below.

### Burn-down: the two open LINT012 findings

`intrinsics-scalars.library.plato:85-86` — the `Number` intrinsics declare
`MinValue(_: Number)` / `MaxValue(_: Number)` (static) while `NumericalLimits` declares
`MinValue(x: Self)` / `MaxValue(x: Self)` (instance).

This is a **genuine design conflict, not a typo**, which is why it is listed rather than forced:

- The obligation must be instance, because `Vector.MinValue => self.Broadcast(Number.MinValue)` needs the receiver for arity, and an obligation has to be shaped for its hardest implementor.
- `Number.MinValue` is genuinely a constant and is *called* as `Number.MinValue` (e.g. `numeric-structures-components.library.plato:122`, `intervals-transforms-bounds.library.plato:60`). Making it instance breaks every one of those call sites.

The C# build masks it: `Number` is scalar-erased to `float`, so no interface implementation is
emitted and no CS0736 appears. LINT012 is the only thing that sees it.

Resolving it needs a real decision — most likely allowing an interface to declare a member as
type-level *and* letting an implementor that needs the receiver opt out, which is a language
question, not a rename. Worth its own issue if it starts to matter; harmless as two warnings today.

Nothing else outstanding.

### Landed

Plato `0600e5a` — `--static-abstract` flag (`Program.cs`, `CSharpWriterExtensions.cs`,
`CSharpWriter.cs`), `StaticAbstractInterface` (`CSharpFunctionInfo.cs`), emission in
`CSharpTypeWriter.WriteInterfaceFunctions`, `Vector.Broadcast` instance fix, and
`tools/regen-forward-conformance.ps1` opting in.

Gates: CS0736 = 0; forward compile errors 364 with the flag and 364 without (net-neutral, identical
distribution — the rise from the earlier 324 is unrelated stdlib churn from a concurrent session);
`regen-generated.ps1` 368/368 identical; `ForwardStdLibCheckerTests` 0/2133 diagnostics, 5/5 pass.

## Simplest fix

Emit `static abstract` in the interface writer keyed off the existing `IsStatic`, emit the
extension alongside, flip the interface declarations, revert the renames.

Pros: the generated API says what it means; generic C# consumers gain `T.Zero()`; no new Plato
grammar; no new C# language version.

Cons: every `_`-receiver member now emits two C# surfaces, so generated output grows slightly.
The checker rule will surface further pre-existing disagreements, so expect a burn-down rather
than a clean first run. Emitter change means a deliberate golden refresh under hard rule 2.

## Prevention

The checker rule in `Done means` IS the prevention — it is the gate whose absence caused this.
Worth stating in `LIBRARIES.md` as well: **an obligation fill must match its obligation's
receiver-usage marker**, since the same trap exists for any future backend that maps `_` to a
target-language static.
