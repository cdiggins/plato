---
id: plato-294
title: Forward-stdlib generated C# does not compile: 332 structural errors (F-bounded Self, type-arg mismatches)
type: bug
status: dropped
priority: p2
effort: M
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/Plato.CSharpWriter, submodules/Plato/stdlib, tools/regen-forward-conformance.ps1, tracker/issues/plato-291.md]
---

> **DUPLICATE — merged into [plato-308](plato-308.md) on 2026-07-29.** Same bug, filed
> independently the same day; this issue's 332 errors are plato-308's 324 plus the 8 CS0736 that
> had already been fixed there. Everything unique here has been folded into plato-308: the
> plato-291 provenance, the degraded-body regression box (44 at filing), the Stage-2 `-Test`
> flip, the "compare per-shape counts, never the total" discipline, and the writer-collision
> warning. **Do not work this issue — work plato-308.** Kept for the record only.

## Issue
With the plato-291 ground-TIR abort fixed (Plato 61ad4a3), forward-stdlib codegen completes:
1209 .g.cs files from the full merged recipe (stdlib + stdlib-tests laws). But the generated
C# does not build: **332 structural errors**, by shape --
- 166 CS0315: a concrete type used as a self-constrained argument (`Curve3D<Self>` F-bounded
  emission) with no implicit conversion satisfying the constraint.
- 134 CS0305: generic type-argument count mismatches.
- 14 CS0535: unimplemented interface members.
- 8 CS0736 / 6 CS0557 / 4 CS0562: duplicate/partial member clashes.

These are pervasive young-vocabulary type-modeling defects across the whole forward stdlib +
writer, independent of the ground-TIR blocker. Measured 2026-07-29.

## Impact
THE blocker for executing forward-stdlib bodies. Ready and waiting behind it (inherited from
plato-291): `stdlib-tests/foundation.laws.plato` (15 Law_* functions),
`conformance/Plato.ForwardConformanceTests` (reflection law runner + KnownFailures +
BlockerGuardTests), and Stage 2 of `tools/regen-forward-conformance.ps1`, which stays
diagnostic (exit-code report, NOT -Test gating) until the generated C# compiles.

## Affected code
- Plato.CSharpWriter -- the F-bounded `Curve3D<Self>` emission shape (166 errors, one cause).
- stdlib/ -- interface/type declarations whose generic arities disagree with what the writer
  emits (134 CS0305), unimplemented obligations (14 CS0535), duplicate members.
- tools/regen-forward-conformance.ps1 Stage 2 -- flips to -Test when this closes.

## Cause / analysis
Not yet root-caused per shape. The CS0315 wall (50% of all errors) has ONE probable cause in
the writer's F-bounded-generics emission for self-referential interfaces -- fix that first and
remeasure; the rest may shrink substantially. CS0305 suggests interface type-parameter counts
diverging between declaration and use sites (the forward vocabulary's bare-name interfaces carry
different arities than legacy's I-prefixed ones). Speculation until measured per-shape.

## Priority
p2, same as its parent -- it IS the continuation of plato-291's payoff path. Effort M-L:
one writer fix likely collapses the CS0315 half; the CS0305/CS0535 tail is library repair.

## Dependencies
- Blocked by: nothing; plato-291's fix landed (Plato 61ad4a3).
- Blocks: forward conformance laws executing; KnownFailures population; Stage 2 -Test gating.
- Touches: Plato.CSharpWriter (writer sessions collide here -- coordinate).

## Fix approaches
1. Fix the F-bounded emission first (one writer change), remeasure, then triage the remainder
   per shape. Recommended: highest error-count-per-change.
2. Library-first: repair stdlib declarations until arities agree, then revisit the writer.
   Slower feedback; the writer defect would still stand.

## Done means
- [ ] generated forward-stdlib C# compiles (0 errors)
- [ ] foundation.laws.plato executes via ForwardConformanceTests; KnownFailures populated honestly
- [ ] regen-forward-conformance.ps1 Stage 2 flipped to -Test gating
- [ ] degraded-body count (44 at filing) unchanged or lower

## Prevention
The per-shape error inventory is the regression baseline: re-run Stage 2 after each fix and
compare shape counts, never just the total.
