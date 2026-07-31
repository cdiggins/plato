---
id: plato-353
title: Add QuadGrid3D type
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-302, plato-326, plato-318]
---

## Idea
Need a `QuadGrid3D` — a regular quad lattice in 3D (control net / subdivided surface grid), distinct from `QuadMesh3D` (indexed arbitrary quads). MCP search finds no QuadGrid type today; BREP discussion (plato-302) already names QuadGrid as a possible BrepSurface case.

## Assumptions
- Grid topology (Nu x Nv samples) is a common authoring/tessellation intermediate.
- Grid2D/3D collection types exist or are adjacent (plato-326 NumRows/NumColumns mismatch) — QuadGrid3D is geometric points on a grid, not raw Grid3D<T>.
- Used by NURBS/subdiv/Coons and loft sampling.

## Design decisions
- **Storage** — Grid2D<Point3D> wrapper vs explicit RowCount/ColumnCount + Array.
- **Concepts** — ParametricSurface sample? Meshable3D via quad faces?
- **Naming** — QuadGrid3D vs ControlGrid3D vs PointGrid3D.

## Related
- [plato-302](plato-302.md) — BrepSurface may include QuadGrid case.
- [plato-326](plato-326.md) — Grid2D/3D RowCount vs NumRows.
- QuadMesh3D in meshes.plato — irregular cousin.
- [plato-318](plato-318.md) — builders may emit grids then meshes.

## Approaches
Short term: `type QuadGrid3D { Points: Grid2D<Point3D> }` + ToQuadMesh / ToTriangleMesh.
Long term: BREP case; subdiv operators; normal estimation.
Adjacent: QuadGrid2D for planar domains.

## Bedrock
Fills the **regular quad lattice** slot between parametric surfaces and QuadMesh3D. Verdict: **simplest**. Must NOT duplicate Grid2D APIs — wrap/compose them.

## Done means
- [ ] QuadGrid3D type in stdlib
- [ ] Conversion to QuadMesh3D (and/or triangle mesh)
- [ ] At least one producer (sample surface / loft)

## Simplest possible implementation
Thin wrapper over Grid2D<Point3D> + face indexing helpers.
- Pros: tiny; composes existing grid.
- Cons: easy to confuse with QuadMesh3D / raw Grid.

## Case against
- Grid2D<Point3D> alone may suffice without a new type name.
- Naming collision with BREP "QuadGrid" payload.
- Verdict: **pursue** if BREP/surface sampling needs a named home; else **park** behind Grid2D<Point3D> helpers.
