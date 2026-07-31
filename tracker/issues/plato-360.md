---
id: plato-360
title: Lint Reduce that should be Sum
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-277, plato-359]
---

## Idea
Many `Reduce(0, (a,x) => a + x)` (and kin) are actually `Sum`. A lint rule (or codemod) could suggest/simplify to Sum — reducing noise and pointing authors at the named aggregate.

## Assumptions
- `Sum` exists or should exist on Indexable<Number> (plato-277 notes P2 collections package / inline Reduce spelling).
- Lint is valuable only if Sum is total and equivalent for the matched patterns.
- False positives on non-associative "+" overloads would be bad.

## Design decisions
- **Lint vs library only** — LINT rule in Plato linter vs doc convention.
- **Pattern match** — literal Zero seed + add lambda vs any Additive.
- **Auto-fix** — suggest only vs autofix.

## Related
- [plato-277](plato-277.md) — Sum vs Reduce(0.0, …).
- [plato-359](plato-359.md) — seed inference ( complementary).
- Existing LINT00x infrastructure (plato-321/325).

## Approaches
Short term: ensure Sum(Indexable<Number>) (and Integer) exists; replace obvious stdlib sites.
Long term: LINT "Reduce that is Sum".
Adjacent: Product, All, Any similar lints.

## Bedrock
Dogfoods named aggregates on the **Indexable reductions** seam. Verdict: **simplest**. Must NOT delete seeded Reduce.

## Done means
- [ ] Sum available for the element types we care about
- [ ] Stdlib call sites cleaned or inventory filed
- [ ] Optional LINT filed/implemented with low false-positive evidence

## Simplest possible implementation
Add/confirm Sum; manually replace ten call sites; defer lint.
- Pros: payoff without tooling.
- Cons: no ongoing enforcement.

## Case against
- Reduce is the general form; teaching Sum+Reduce is fine without lint noise.
- Pattern matching lambdas in a lint is fragile.
- Verdict: **pursue** Sum adoption; **park** lint until Sum is ubiquitous.
