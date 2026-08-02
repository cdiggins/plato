---
id: plato-348
title: Make Transform3D/Pose3D implement transform interfaces; add RigidTransform
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-259, plato-260, plato-229, stdlib/transforms-trs.plato]
---

## Idea
`Transform3D` / `Transform2D` (`transforms-trs.plato`) and related pose/rigid types implement only `Value` (Pose3D also `Interpolatable`). There is no `RigidTransform3D` type/interface in stdlib (MCP empty). Transforms should participate in an interface lattice (compose, invert, apply-to-point) instead of being inert records.

## Assumptions
- Authors need generic `Transform`/`RigidTransform` bounds for deformers and scene graphs.
- TRS is not closed under composition (file header already warns) — rigid vs affine vs TRS must be distinct interfaces.
- Pose3D / Motor3D / Matrix4x4 already express overlapping rigid motion (see plato-259/260).

## Design decisions
- **Interface split** — RigidTransform (isometry) vs AffineTransform vs TRS authoring type.
- **Canonical rigid** — Pose3D vs Motor3D vs new RigidTransform3D record (plato-259 Motor preference).
- **Obligations** — Invert, Compose, TransformPoint / TransformVector / TransformNormal.

## Related
- `stdlib/transforms-trs.plato` — Transform2D/3D implements Value only.
- `stdlib/transforms-pose.plato` — Pose3D implements Value, Interpolatable.
- [plato-259](plato-259.md) — Motor as canonical rigid motion.
- [plato-260](plato-260.md) — Rotor vs Quaternion.
- [plato-229](plato-229.md) — interface lattice completion.

## Approaches
Short term: add `interface RigidTransform3D` (or dimension-generic) with Invert/Compose/Apply; implement on Pose3D and/or Motor3D; leave TRS as authoring type converting to affine/matrix.
Long term: Transform3D implements a weaker Transform interface (not rigid); shared deformer APIs.
Adjacent: 2D siblings; deprecate duplicate Apply helpers.

## Bedrock
Strengthens the **rigid vs TRS vs matrix** seam already documented in transforms-trs. Verdict: **right**. Simple version must NOT claim TRS is closed under Compose.

## Done means
- [ ] Rigid (and optionally general Transform) interface(s) declared with Apply/Invert/Compose
- [ ] Pose3D and/or Motor3D implement rigid
- [ ] Transform3D either implements a non-rigid Transform interface or documents why not
- [ ] ADR or links clarify relationship to plato-259

## Simplest possible implementation
Interface + implements on Pose3D wrapping existing library ops; TRS stays Value-only with ToMatrix/ToPose.
- Pros: generic bounds appear; respects TRS limits.
- Cons: naming confusion until Motor story settles.

## Case against
- Matrix4x4 / Pose3D methods may be enough without interfaces.
- Premature lattice work before Motor/Quaternion ADR (plato-259/260).
- Verdict: **pursue** interface sketch now; **park** heavy implements until rigid canonical is chosen.
