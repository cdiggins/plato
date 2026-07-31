---
id: plato-349
title: Rename IntegerVectorN to IntegerN for naming consistency
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-241, stdlib/vectors-integer.plato]
---

## Idea
Integer vectors are named `IntegerVector2/3/4` (`vectors-integer.plato`) while float companions are `Vector2D/3D/4D` and scalar bundles like `Number3` exist — not `NumberVector3`. Should `IntegerVector3` become `Integer3` (and peers) for consistency?

## Assumptions
- Naming inconsistency slows discovery and codegen porting (plato-241 renamed float vectors toward *D suffix).
- Voxel / grid code already uses IntegerVector3 as coordinates (`VoxelBrick3D.Coordinate`).
- Rename is mechanical but high churn across stdlib + generated C#.

## Design decisions
- **Target names** — Integer2/3/4 vs Integer2D/3D/4D vs keep IntegerVector*.
- **Relation to Number2/3/4** — mirror that pattern (Integer3) vs mirror Vector*D.
- **Index types** — stay distinct from VertexIndex etc.

## Related
- `stdlib/vectors-integer.plato` — IntegerVector2/3/4.
- [plato-241](plato-241.md) — Vector2→Vector2D rename idea.
- Voxels / Grid types using IntegerVector3.

## Approaches
Short term: decide naming ADR; alias old names if language supports.
Long term: mechanical rename + emit snapshot update.
Adjacent: Int2/Int3 C#-style aliases for interop.

## Bedrock
Aligns the **scalar-tuple naming convention** (NumberN / IntegerN) across numeric bundles. Verdict: **simplest-along-the-grain** if aliases exist; else medium churn. Must NOT conflate with index newtypes.

## Done means
- [ ] ADR picks IntegerN vs IntegerND vs keep IntegerVectorN
- [ ] Types renamed or aliased; references updated
- [ ] Fast gate + emit snapshot green

## Simplest possible implementation
Document Integer3 as the preferred name and add type aliases if Plato supports them; defer deleting IntegerVector3.
- Pros: low breakage.
- Cons: dual names linger.

## Case against
- IntegerVector* is explicit ("vector of integers") and already shipped; rename churn > clarity gain.
- Number3 is components-without-vector-algebra; IntegerVector may intentionally signal vector ops.
- Verdict: **park** until a broader numeric naming pass (with plato-241); decide together.
