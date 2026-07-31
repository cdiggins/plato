---
id: plato-359
title: Infer Reduce seed/default from accumulator type
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-277, plato-360]
---

## Idea
`Reduce` sometimes takes an explicit default/seed in the first position (`Reduce(0.0, (total, x) => total + x)` per plato-277 notes). Can the seed be inferred from the accumulator/element type (Zero, empty monoid, etc.)?

## Assumptions
- Many reductions are monoidal (Sum, And, Or) with an obvious identity.
- Inferring the wrong seed is a silent correctness bug (non-monoid ops).
- Numerical / generic Zero concepts may already exist for some types.

## Design decisions
- **Inference rule** — require Zero/Identity concept vs special-case Number/Boolean vs overload Reduce without seed.
- **Partiality** — empty collection: return identity vs error.
- **Overlap with Sum** — plato-360 lint toward Sum when seed is Zero and op is +.

## Related
- [plato-277](plato-277.md) — Reduce on Indexable; Sum spelled as Reduce(0.0, …).
- [plato-360](plato-360.md) — Reduce that should be Sum.
- Algebra concepts for Zero/Additive.

## Approaches
Short term: `Reduce(xs, f)` overload that requires `T: HasZero` (name TBD) or uses Zero(T).
Long term: monoid-based Reduce; empty-collection laws in tests.
Adjacent: Scan with same seed rules.

## Bedrock
Ties Reduce to the **identity element** seam in the numeric/algebra lattice. Verdict: **simplest-along-the-grain**. Simple version must NOT infer seeds for arbitrary lambdas without a Zero constraint.

## Done means
- [ ] Seedless Reduce overload with explicit concept constraint
- [ ] Empty-collection behavior documented
- [ ] Existing seeded Reduce remains for non-monoid cases

## Simplest possible implementation
Add Reduce(xs, f) where element type has Zero; desugar to Reduce(Zero, f).
- Pros: deletes noise at Sum-like sites.
- Cons: only helps Zero-able types.

## Case against
- Hidden seeds hurt readability for non-associative ops.
- Zero concept may not fit all useful reductions (concatenation seed "").
- Verdict: **pursue** behind a clear HasZero/Identity constraint; keep seeded form.
