---
id: plato-354
title: Port BlockMesh builder into Plato
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-318, plato-273]
---

## Idea
Port a BlockMesh-style builder into Plato: construct meshes from axis-aligned (or framed) box "blocks" / voxels-as-boxes, as in historical Studio Block Mesh tooling (superseded in UI by Box Frame — studio-216 — but the geometric builder remains useful).

## Assumptions
- Authors still want CSG-ish union of boxes → mesh without full CSG.
- plato-318 covers general Triangle/Quad mesh builders; BlockMesh is a *domain* builder on top.
- May compose List/Buffer builders rather than a new unique type.

## Design decisions
- **API** — AddBlock(box)/AddBlock(frame,size) then Freeze→TriangleMesh3D vs immediate mesh concat.
- **Merging** — weld coincident faces vs keep internal walls.
- **Relation to voxels** — BlockMesh vs VoxelGrid mesher.

## Related
- [plato-318](plato-318.md) — general mesh builders (do not duplicate).
- DONE studio-216 — Box Frame replaced Block Mesh in Studio UI.
- Voxel types in `stdlib/voxels.plato`.
- [plato-273](plato-273.md) — geometry libraries into stdlib.

## Approaches
Short term: Plato library that appends box quads/tris into List builders from plato-318 path.
Long term: optional face welding; Studio script thin wrapper.
Adjacent: Cylinder/Sphere block primitives.

## Bedrock
Domain sugar over the **mesh builder seam** (plato-318), not a parallel construction stack. Verdict: **simplest-along-the-grain**. Must NOT invent a third unique builder type before plato-318 lands.

## Done means
- [ ] AddBlock → triangle or quad mesh path works in Plato
- [ ] Documented merge/weld behavior
- [ ] Links to plato-318 without duplicating general builder scope

## Simplest possible implementation
Functions taking List<Point3D>+List<Face> (or Mesh builder) and appending a box; no unique type.
- Pros: unblocked by unique-type allow-list.
- Cons: less ergonomic than C# BlockMesh class.

## Case against
- Studio moved off Block Mesh UI — demand may be nostalgia.
- Voxel meshing + CSG may supersede blocks.
- Verdict: **park** until plato-318 exists; then **pursue** as a thin library if a consumer asks.
