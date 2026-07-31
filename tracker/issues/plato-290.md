---
id: plato-290
title: Duplicate Range(Integer) declaration in stdlib-legacy intrinsics blocks lint --strict
type: bug
status: done
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [submodules/Plato/stdlib-legacy/intrinsics.plato, submodules/Plato/PlatoCompiler/Analysis/Linter.cs]
---

## Issue
`stdlib-legacy/intrinsics.plato` declares `Range(Integer): IArray<Integer>` twice in the same
library block, differing only in parameter name:

```
402:    Range(self: Integer): IArray<Integer>;
474:    Range(n: Integer): IArray<Integer>;
```

Plato overload identity is by name + parameter types, so these are the same signature. LINT004
(duplicate function signatures within a library) flags it. Confirmed by reading the file, not
inferred.

## Impact
Low blast radius but it is the **only** Error-severity lint finding in either stdlib, so it
single-handedly makes `Plato.CLI lint submodules/Plato/stdlib-legacy --strict` exit 1. With the
severity axis added 2026-07-28, `--strict` became usable as a gate for the first time; this one
declaration is what stands between the shipping stdlib and a green strict gate.

Runtime effect unverified. Which of the two the resolver picks is unclear; since both have the
same return type and `Range` is an intrinsic backed by the runtime, behavior is probably
identical either way. Worth confirming before assuming it is cosmetic.

## Affected code
- `submodules/Plato/stdlib-legacy/intrinsics.plato:402` — `Range(self: Integer)`, receiver-style naming, sits in the intrinsics block near the other `self:` members.
- `submodules/Plato/stdlib-legacy/intrinsics.plato:474` — `Range(n: Integer)`, alphabetically placed between `PrependAndAppend` and `Reduce`.
- `submodules/Plato/stdlib-legacy/intrinsics.plato:469` — `MapRange(n: Integer, f: ...)` directly above `:474`, using the same `n:` convention. Suggests `:474` came in with the MapRange group.
- `submodules/Plato/PlatoCompiler/Analysis/Linter.cs` — LINT004, classified Error.
- `submodules/Plato/PlatoCompiler/Analysis/Linter.cs:106` — `Range` is in `MembersImplementedByWriter`, so the writer supplies it and neither declaration carries a body.

## Cause / analysis
Speculation: two independent additions. The `self:` form at `:402` follows the receiver
convention used for method-style calls (`n.Range()`); the `n:` form at `:474` was likely added
alongside `MapRange` by someone working alphabetically who did not see the earlier entry 72
lines up. The file is long and the two blocks use different naming conventions, which is exactly
the condition that hides a duplicate from review.

No ADR in `tracker/decisions/` covers a deliberate double declaration.

## Priority
Recommend **p2**. Severity is low — probably no runtime effect — but the fix is trivial and the
payoff is disproportionate: it converts `--strict` on the shipping stdlib from red to green,
which is what lets `--strict` be wired into `check-all.ps1` as a real gate. Cheap now, and it
blocks a gate rather than a feature. Not urgent; safe to defer without compounding.

## Dependencies
- Blocks: adopting `lint --strict` as a CI/check-all gate for `stdlib-legacy`.
- Touches: `stdlib-legacy` is WRITABLE but every change requires `regen-generated.ps1 -Apply` + `check-all.ps1` green per `submodules/Plato/CLAUDE.md`. Deleting a declaration may shift emitted output.

## Fix approaches
1. **Delete `:474`.** Keep the `self:` form, which matches the receiver convention and the surrounding intrinsics style. Regen and confirm byte-identical output.
2. **Delete `:402`.** Keep `n:`, matching `MapRange` directly above it. Equivalent unless something relies on `self:` for method-form emission — check `--methods` / `ExtensionStyleWriter` before choosing.
3. **Keep both, exempt intrinsics from LINT004.** Rejected on its face; it hides real duplicates in the one file most likely to accumulate them.

## Bedrock
The invariant is *one signature, one declaration per library* — LINT004 already encodes it, and
this is the rule catching its first real violation on shipping code. The leverage is not in the
deletion but in wiring `--strict` into `check-all.ps1` afterward, so the rule guards the file
from here on instead of reporting into a log nobody gates on.

Verdict: **simplest-along-the-grain**. What the simple fix must NOT do: delete the line and stop
there. Without `--strict` in the gate battery the next duplicate lands exactly the same way, and
this issue gets refiled in six months.

## Done means
- [x] one of the two declarations removed; `lint stdlib-legacy --strict` exits 0
- [x] `regen-generated.ps1 -Apply` run and the diff reviewed (expected: no change, since the writer supplies `Range`)
- [x] `check-all.ps1` green
- [x] `--strict` wired into the gate battery so LINT004 regressions fail loudly

## Resolution (2026-07-28)
Deleted the `Range(n: Integer)` declaration (option 1), keeping `Range(self: Integer)` in the
Integer section. Evidence for the choice: no emitter keys off the parameter *name* `self` —
receiver selection is positional/type-based everywhere (`CSharpWriter.cs:591`,
`CSharpFunctionInfo.FirstParameterName`, `ExtensionStyleWriter`), and the literal `"self"` in C#
appears only where the compiler *synthesizes* a parameter (`SymbolFactory.cs:675`,
`Definitions.cs:350`), never matched against source. The name being emission-irrelevant, the tie
broke on placement: `:402` sits among the Integer members (its receiver type), while `:474` sat in
the IArray block where every neighbour takes an `IArray<$T>` receiver.

Lint stdlib-legacy --strict: exit 1 / 221 findings (LINT004 x1, Error x1) → exit 0 / 220 findings
(LINT004 gone, every other count unchanged). Regen diff: 0 differing across both variants, as
predicted — `Range` is in `CSharpWriter.IgnoredFunctions`, so neither declaration was ever emitted.
`check-all.ps1`: 7/7 PASS. The old non-strict `lint (stdlib-legacy)` gate became
`lint --strict (stdlib-legacy)`, plus a new `lint --strict (stdlib forward)` gate.

## Simplest fix
Delete line 474. Pro — one line, keeps the receiver-style declaration consistent with the
surrounding block, immediately greens `--strict`. Con — requires the regen + check-all cycle to
confirm emitted output is unchanged, so the verification costs more than the edit. Leaves the
gate unwired unless the last checkbox is also done.

## Prevention
- LINT004 already prevents recurrence *if* it is gated. It is currently reported, not enforced — that gap is the real prevention item and is covered by the last "Done means" box.
- Contributing factor worth its own idea: `intrinsics.plato` mixes `self:` and `n:` parameter-naming conventions, which is what let a same-signature duplicate read as a different function. A convention check (or a documented rule in the stdlib CONVENTIONS.md) would make duplicates visually obvious. Candidate for /track-idea.
