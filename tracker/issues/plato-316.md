---
id: plato-316
title: Implement MeshIncidence on PolygonMesh3D and TriangleMesh3D
type: feature
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: []
---

## Motivation

plato-314 added `concept MeshIncidence` (`stdlib/topology.concepts.plato`) — the
middle rung between `MeshElementCounts` and `HalfEdgeNavigable` — plus its
derived library members (valence, vertex neighbours, boundary/wire/non-manifold
edge tests). No concrete type implements it yet, so `lint` reports:

```
topology.concepts.plato(45): LINT008: concept 'MeshIncidence' has no concrete
implementer; it is either dead vocabulary or a missing 'implements' clause
```

The derived helpers are therefore unreachable from any mesh in the tree.

## Approach

`PolygonMesh3D` is the natural first implementer: the corner-as-half-edge
navigation in `meshes-polygon-corners.library.plato` can already answer all six
queries with no side table.

- `VerticesOfFace` — `Faces.Row(face)`.
- `UndirectedEdgesOfFace` — `FaceArity` slots mapped through `FaceCorner` +
  `CornerUndirectedEdge`.
- `FacesOfVertex` / `UndirectedEdgesOfVertex` — `VertexCornerRing` mapped
  through `CornerFace` / `CornerUndirectedEdge`.
- `VerticesOfUndirectedEdge` / `FacesOfUndirectedEdge` — need the inverse of
  `CornerUndirectedEdge` (find the canonical corner of rank e), which is the one
  genuinely new search.

Two things to settle while writing it:

1. **Cost.** The corner searches are linear, so these land quadratic like the
   rest of that file. Acceptable for catalog solids; the stored half-edge builder
   is plato-298.
2. **Signature collisions.** `FaceArity(PolygonMesh3D, Integer)` and
   `VertexDegree(PolygonMesh3D, VertexIndex)` already exist and overlap in
   meaning with the concept's derived `FaceVertexCount` / `VertexValence`. Decide
   whether the concrete pair is retired in favour of the concept-level names
   before adding the `implements` clause, or the ambiguity lands at the call
   site.

`TriangleMesh3D` is the second implementer and needs an actual edge list — it has
only `Positions` + `Faces`, so an `UndirectedEdgeList` must be built or stored.
That is the case that motivated the concept in the first place.

## Done means

- [ ] `PolygonMesh3D implements MeshIncidence` with bodies in their own
      `*.library.plato` file; LINT008 for `MeshIncidence` is gone.
- [ ] The `FaceArity` / `VertexDegree` overlap is resolved, not shadowed.
- [ ] Round-trip check on a catalog solid: every undirected edge is interior,
      `VertexValence` matches `VertexDegree`, Euler characteristic is 2.
- [ ] `tools/check-stdlib-fast.ps1` both gates PASS.
