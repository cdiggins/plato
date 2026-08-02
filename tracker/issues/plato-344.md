---
id: plato-344
title: Add OrientedBox3D to Plato stdlib
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-343, stdlib/planar-boxes.plato, ara3d-sdk/src/Ara3D.Geometry/Primitives/OrientedBox3D.cs, plato-270]
---

## Idea
Plato has `OrientedBox2D` (`planar-boxes.plato`) but no `OrientedBox3D`. Ara3D.Geometry already ships `OrientedBox3D(Frame3D Frame, Vector3 Size)` with intersection/corner helpers — port or re-express that type in stdlib.

## Assumptions
- AABB is insufficient when orientation matters (IFC compare, tight fit, collision).
- Frame3D + size (or half-extents) is the right storage, matching the C# record.
- PCA (plato-343) is a common constructor but not required for the type itself.

## Design decisions
- **Representation** — Frame+Size vs center+rotation+extents vs 8 corners.
- **Interface obligations** — Bounds-like? Solid? Meshable3D?
- **Naming** — OrientedBox3D (match C#) vs OBB3D vs OrientedBounds3D.

## Related
- `stdlib/planar-boxes.plato` — OrientedBox2D.
- `ara3d-sdk/src/Ara3D.Geometry/Primitives/OrientedBox3D.cs` — prior art.
- [plato-343](plato-343.md) — PCA as FromPoints constructor.
- [plato-270](plato-270.md) — Ara3D.Geometry vs Plato differential.

## Approaches
Short term: declare OrientedBox3D + Corners/Contains/ToBounds/ToMesh helpers mirroring 2D and C#.
Long term: intersection (SAT), union approximations, PCA-from-points factory.
Adjacent: OrientedBox as interface parameterized by dimension.

## Bedrock
Completes the **2D/3D oriented box symmetry** already started in planar-boxes. Verdict: **simplest**. Must NOT wait on PCA to ship the type.

## Done means
- [ ] `OrientedBox3D` type in stdlib with Frame (or equivalent) + size
- [ ] Corners / axis-aligned Bounds conversion
- [ ] Parity note vs Ara3D.Geometry OrientedBox3D

## Simplest possible implementation
Port the C# record fields + GetCorners; skip SAT until needed.
- Pros: unlocks typed OBB in Plato immediately.
- Cons: thin until algorithms catch up.

## Case against
- Bounds3D + explicit Frame transforms may already cover call sites without a new type.
- Divergence from C# if Plato picks a different field layout.
- Verdict: **pursue** — 2D already exists; 3D gap is accidental.
