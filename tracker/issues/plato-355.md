---
id: plato-355
title: Construct Point3D and peers from lower-D plus scalar
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-346, plato-349]
---

## Idea
`Point3D` (and similar vector-like types) should be constructible from a lower-dimensional sibling plus a scalar — e.g. Point2D + Z → Point3D. The pattern repeats across Vector/Number/Integer bundles and UV/Point families.

## Assumptions
- This is a frequent authoring pattern (promote 2D drawings into 3D).
- Explicit constructors/factories beat abusing implicits for dimension changes (clarity).
- Should be consistent across Point/Vector/NumberN/IntegerVector families.

## Design decisions
- **API** — `Point3D(p: Point2D, z: Number)` vs `WithZ` extension vs implicit (probably not).
- **Which dimension is filled** — always trailing component vs axis parameter.
- **Family coverage** — Points only vs all *2→*3/*3→*4 promotions.

## Related
- Point2D/Point3D in points stdlib; Vector2D/3D; Number2/3.
- [plato-346](plato-346.md) — implicits (likely explicit here).
- [plato-349](plato-349.md) — IntegerVector naming when adding Integer2→Integer3.

## Approaches
Short term: Point3D(Point2D, Number) + Vector3D(Vector2D, Number) factories.
Long term: systematic *N from *(N-1)+scalar for Number/Integer bundles; optional axis-labeled overloads.
Adjacent: Point2D from Number + Number already exists via fields — ensure symmetry.

## Bedrock
Establishes a **dimension-promotion convention** across vector-like types. Verdict: **simplest**. Must NOT make dimension promotion implicit by default.

## Done means
- [ ] Point3D from Point2D+scalar
- [ ] Same pattern on Vector3D (and documented for Number3 if applicable)
- [ ] CONVENTIONS note for the pattern

## Simplest possible implementation
Two factory functions in points/vectors libraries.
- Pros: tiny; high usability.
- Cons: incomplete family until sweep.

## Case against
- `Point3D(x.XY, z)` field construction is already clear enough.
- Proliferation of overloads.
- Verdict: **pursue** — low cost, matches muscle memory from other APIs.
