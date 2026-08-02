---
id: stdlib-402
title: Sum-kind types have no Equals, so laws comparing two sum values cannot type-check
type: bug
status: ready
priority: p2
effort: M
risk: med
area: stdlib
sprint: 
created: 2026-08-02
closed:
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

## Done means

- [ ] Two values of a sum type can be compared for equality from Plato source, or a decision record
      says why they cannot and what to write instead
- [ ] The three polyhedra involution laws type-check
