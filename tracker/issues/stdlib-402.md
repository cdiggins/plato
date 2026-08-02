---
id: stdlib-402
title: Sum-kind types have no Equals, so laws comparing two sum values cannot type-check
type: bug
status: done
priority: p2
effort: M
risk: med
area: stdlib
sprint: 
created: 2026-08-02
closed: 2026-08-02
links: [tracker/issues/stdlib-398.md, tracker/decisions/2026-07-29-polyhedra-dual-kind-map.md]
---

## Symptom

Three laws in `stdlib/tests/polyhedra.laws.plato` do not type-check:

```
CHK201  Law_PlatonicDualIsInvolution     No overload of 'Equals' matches (PlatonicSolidKind, PlatonicSolidKind)
CHK201  Law_ArchimedeanDualIsInvolution  No overload of 'Equals' matches (ArchimedeanSolidKind, ArchimedeanSolidKind)
CHK201  Law_CatalanDualIsInvolution      No overload of 'Equals' matches (CatalanSolidKind, CatalanSolidKind)
```

They are visible to `plato_check` (whose corpus is all of `stdlib/`) and to the merged program the
forward conformance run builds. They are NOT visible to the checker ratchet: `stdlib-398` scoped that
gate to the tier folders, which is why the ratchet is green while these three are still broken.

## Cause

`PlatonicSolidKind` / `ArchimedeanSolidKind` / `CatalanSolidKind` are sum types, and no `Equals`
overload exists for a sum. The gap is general: any law or body that compares two values of a sum type
hits it. An involution law (`Dual(Dual(x)) == x`) is the natural shape for these, so the vocabulary
will keep producing them.

## Fix approaches

1. Give sums a structural `Equals` in the checker/binder, the way constructors and case factories are
   already synthesized. Widest fix; needs a decision on what equality means for a case carrying
   fields.
2. Declare `Equals` by hand on each sum-kind type in the library. Cheap, does not scale, and the
   pattern would have to be repeated for every future sum.
3. Rewrite the three laws to compare a projection (a name or index) instead of the sum values. Makes
   the laws pass without answering the question, and weakens what they assert.

Approach 1 is the one worth costing; the other two are workarounds recorded so the trade-off is
visible.

## Resolution

**Approach 1** — the binder synthesizes structural `Equals` and `NotEquals` on every sum type,
next to the per-case factories (`SymbolFactory`, new `FunctionType.SumEquality` in
`IFunction`/`Definitions`). They are bodyless, like the factories: the tagged struct the writer
emits already defines both members (design doc §6), so the declarations only exist for the
checker to resolve `a == b` between two sum values. The "what does equality mean for a case
carrying fields" question was already answered by the design doc: factories zero the inactive
fields, so whole-struct structural equality is exact.

The same pass fixed the sibling gap the checker exposed once the laws compiled: a match whose
scrutinee is call-shaped (`u.Orientation`, `self.Position`, `p.RelationTo(pl, tol)`) had no
subject type at resolve time and died with a location-less CHK304. `ResolveMatch` now reads the
declared field type off the receiver (or the agreeing sum return of the arity-matching group
overloads), and registers the `MatchExpression` in `SymbolsToNodes` so any CHK3xx on a match
carries a source location.

## Done means

- [x] Two values of a sum type can be compared for equality from Plato source, or a decision record
      says why they cannot and what to write instead
- [x] The three polyhedra involution laws type-check
