---
id: plato-395
title: TimeVarying<TValue> carries no bound, so Change stays a throwing stub
type: debt
status: idea
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/graphics/time-varying.concepts.plato, stdlib/graphics/time-varying.library.plato, plato-382, plato-393]
---

## Issue

`plato-382` gave `type` and `concept` declarations a verified, emitted `where` clause, and the
payoff was `Tween<T> where T: Interpolatable` — its `Sample` is now a real body. The CONCEPT that
`Tween` implements, `TimeVarying<TValue>`
(`stdlib/graphics/time-varying.concepts.plato`), still carries no bound, and one of its derived
functions needs one:

```plato
Change(self: TimeVarying<$TValue>, earlier: Duration, later: Duration): $TValue
    => self.Sample(later) - self.Sample(earlier);
```

Subtracting two bare `$TValue` needs `Difference` (or `Subtractive`) on the element. Nothing
supplies it, so `TirEmitSource.IsOpenGenericEmittable` correctly refuses the body and
`TimeVaryingValues.Change` emits as a throwing stub — the same shape `Tween.Sample` had before
plato-382, and the standing example in that method's own doc comment.

The reason it was not fixed with `Tween` is recorded in
`stdlib/graphics/time-varying.library.plato` (the comment above `ValueAt`): `TimeVarying`'s
implementors span `motion-graphics.types.plato` AND `future/keyframes-tracks.types.plato`
(`AnimationTrack` / `TangentTrack`), so adding `where TValue: Difference<TValue>` to the concept
obliges every one of them at once, including declarations in the non-shipping `future` tier.

## Impact

Low and not on a shipping path.

- `graphics` is not an emitted tier, so no generated project contains the stub today.
- The lint ratchet is unaffected: `LINT001` is 0 across the shipping tiers, because `Change` is a
  library body over a concept, not an undischargeable obligation on a concrete type.
- The cost is the same one plato-382 named: the ceiling is invisible. A reader who sees
  `Tween.Sample` work and `Change` throw has no way to tell why without reading the emit source.

## Fix approaches

1. **Bound the concept**: `concept TimeVarying<TValue> where TValue: Difference<TValue>`, and
   fix up every implementor in both `graphics` and `future` in the same change. Honest, and the
   thing plato-382's machinery was built for. The work is the tree-wide fix-up, not the bound.
2. **Bound the FUNCTION instead** (plato-393): `Change(...) where $TValue: Difference<$TValue>`.
   Smaller — it obliges only this function's callers, not every implementor of the concept.
   Check it against `plato-394` first: `TimeVarying<$TValue>` is a CONCEPT in receiver position,
   not a concrete struct, so the rebind that issue describes should not apply, but that needs
   verifying before the clause is written.
3. **Drop `Change`.** It has no callers. Cheapest, and loses a reasonable derived operation.

Approach 2 is the likely answer and is the smaller half of approach 1.

## Done means

- [ ] `TimeVaryingValues.Change` either emits a real body, or is removed, or has its stub
      recorded here as deliberate with the bound that would fix it named.
