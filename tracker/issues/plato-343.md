---
id: plato-343
title: Add PCA (principal component analysis) to stdlib
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-344, stdlib/statistics-correlation.plato]
---

## Idea
No PCA (principal component analysis) types or libraries appear in Plato stdlib (MCP search for PCA/PrincipalComponent empty). Covariance2D/3D exist in `statistics-correlation.plato` and multivariate normals carry Covariance fields — the linear-algebra inputs for PCA exist, but eigendecomposition / principal axes / explained-variance outputs do not.

## Assumptions
- Consumers want OBB axes, uncertainty ellipsoids, or dimensionality reduction from point clouds / covariance.
- Covariance2D/3D (and MatrixN) are the natural inputs.
- Numeric eigendecomp may need an intrinsic or a small numerical library; not pure Plato for general N today.

## Design decisions
- **API shape** — `Pca2D`/`Pca3D` result records (axes + variances) vs generic `PrincipalComponents(MatrixN)`.
- **Input** — from Covariance vs from point samples (compute covariance inside).
- **Numeric backend** — Plato library vs host intrinsic; 2x2/3x3 closed forms vs general solver.

## Related
- `stdlib/statistics-correlation.plato` — Covariance2D/3D.
- `stdlib/random-multivariate.plato` / `uncertainty-estimation.plato` — Covariance fields.
- [plato-344](plato-344.md) — OrientedBox3D often built from PCA axes of a point set.
- Ara3D.Geometry already has OrientedBox3D consumers that invent axes ad hoc.

## Approaches
Short term: closed-form 2x2/3x3 symmetric eigendecomp on Covariance2D/3D → PrincipalAxes2D/3D.
Long term: sample PCA, SVD for NxN, link to OBB and uncertainty ellipses.
Adjacent: generic Matrix eigendecomp (larger numerical story).

## Bedrock
Closes the **covariance → oriented frame** seam already implied by statistics/uncertainty types. Verdict: **simplest-along-the-grain**. Simple version must NOT invent a full BLAS; ship 2D/3D only.

## Done means
- [ ] From Covariance2D/3D (or equivalent), callers get ordered unit axes + eigenvalues/variances
- [ ] Documented numerical caveats (degenerate / near-equal eigenvalues)
- [ ] At least one dogfood path (e.g. OBB-from-points or uncertainty ellipse)

## Simplest possible implementation
Hard-code 2x2 and 3x3 symmetric eigenroutines in a statistics library; return axes + values.
- Pros: unblocks OBB/PCA UX; no MatrixN solver.
- Cons: does not scale past 3D; numeric edge cases need tests.

## Case against
- Covariance without robust eigen is already useful; PCA may be premature until OrientedBox3D and mesh bounds need it.
- Intrinsic dependency for general N is a larger commitment than a geometry feature.
- Verdict: **pursue** for 2D/3D closed form if OBB lands; otherwise **park**.
