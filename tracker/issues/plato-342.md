---
id: plato-342
title: Make implements Value optional or implied for record types
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-334, plato-340]
---

## Idea
Today nearly every record writes `implements Value` (e.g. `Transform3D`, `TimeInterval`). Value is the root marker for concrete records; the question is whether the language should imply it so authors only declare non-obvious interfaces.

## Assumptions
- Most records are Values; explicit `implements Value` is noise when nothing else is declared.
- Interfaces that need stronger obligations (Hashable, Indexable, IntervalLike) must stay explicit.
- Changing defaulting must not break existing implements lines or interface checking.

## Design decisions
- **Implied vs required** — imply Value for every `type` unless it is an interface/sum-only form, vs keep explicit forever for readability/grepability.
- **Opt-out** — is there any concrete type that must *not* be Value? If none, implication is safe.
- **Multi-implements sugar** — `implements Hashable` means `Value, Hashable` vs must list Value always.

## Related
- [plato-334](plato-334.md) — types stuck at bare Value (taxonomy debt, different angle).
- [plato-340](plato-340.md) — colors currently `implements Value` only; implication would not fix missing Color interface.
- stdlib pattern: `Transform3D implements Value`, `NumberInterval implements IntervalLike<Number>` (Value via inheritance).

## Approaches
Short term: document convention that Value is implied and authors only list extra interfaces; optionally warn on redundant sole `implements Value`.
Long term: checker synthesizes Value for concrete types; `inherits Value` on interfaces unchanged.
Adjacent: interface-defaulting for other universal markers (Hashable?).

## Bedrock
Strengthens the **type vs interface declaration seam** in the checker/parser: concrete records always inhabit Value. Verdict: **simplest-along-the-grain**. Simple version must NOT auto-add Hashable/Equatable or silence missing interface obligations.

## Done means
- [ ] Spec/ADR states whether Value is implied for concrete `type` declarations
- [ ] Checker/docs match the ADR; stdlib either keeps or drops redundant `implements Value` consistently
- [ ] Existing interface obligation lint still fires for missing non-Value interfaces

## Simplest possible implementation
Doc + optional LINT for redundant sole `implements Value` without changing semantics.
- Pros: zero language risk; cleans noise gradually.
- Cons: authors still type Value until lint lands; no real implication.

## Case against
- Explicit Value is teachable and greppable; implication hides the root of the lattice.
- Half-migrated stdlib (some implied, some explicit) is worse than consistent verbosity.
- Verdict: **park** until a language pass is already touching implements defaults; doc/lint is enough for now.
