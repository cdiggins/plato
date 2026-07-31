---
id: plato-350
title: Reduce Indexable boilerplate for implicitly indexable types
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-276, plato-304]
---

## Idea
Some types are "obviously" indexable (fixed fields A,B,C,D or a single Array field) but authors still write Count/At machinery by hand. How to reduce that boilerplate without surprising semantics or breaking concept checking?

## Assumptions
- `Face` already inherits `Indexable<VertexIndex>`; TriangleFace/QuadFace gained synthesis via plato-324 work.
- Writers already synthesize At/Count for some single-collection-field shapes (plato-276 notes gaps in Rust/TS).
- Over-synthesis can invent wrong element types or orders.

## Design decisions
- **Trigger** — single Array/List field vs all similarly typed fields vs explicit `derives Indexable`.
- **Order** — declaration order for multi-field faces; document invariant.
- **Opt-out** — attribute/keyword when auto Indexable would be wrong.

## Related
- [plato-276](plato-276.md) — At/Count synthesis ignores some single-collection-field types (writers).
- DONE plato-324 — Face concept + indexed mesh obligations.
- [plato-304](plato-304.md) — IArray capabilities port.
- `stdlib/meshes.concepts.plato` — Face inherits Indexable.

## Approaches
Short term: document current synthesis rules; extend C# writer to cover known gaps (plato-276).
Long term: `derives Indexable` or concept-default for homogeneous field tuples.
Adjacent: same for Hashable/Equatable fieldwise defaults.

## Bedrock
Strengthens the **Indexable synthesis seam** (compiler/writer) so Face-like and wrapper types stop re-hand-rolling At/Count. Verdict: **simplest-along-the-grain**. Simple version must NOT auto-Indexable every multi-field record.

## Done means
- [ ] Written rules for when At/Count are synthesized
- [ ] Known single-collection gaps closed (or filed precisely)
- [ ] Opt-out or non-trigger cases documented with one example each

## Simplest possible implementation
Finish single-field Array synthesis everywhere; leave multi-field faces explicit unless `derives` exists.
- Pros: predictable; matches today.
- Cons: Face-like quad/tet fields still manual.

## Case against
- Hidden synthesis makes reading a type lie about its methods.
- Wrong field order bugs are worse than boilerplate.
- Verdict: **pursue** for single-collection wrappers; **park** multi-field auto-Indexable without an explicit derives keyword.
