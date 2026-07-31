---
id: plato-314
title: Split MeshTopology into MeshElementCounts + MeshIncidence; qualify unqualified Edge names
type: debt
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: []
---

## Problem

`concept MeshTopology` (`stdlib/topology.concepts.plato`) declares only
`VertexCount` / `EdgeCount` / `FaceCount`. Those three counts are enough for the
Euler characteristic and the genus of a closed manifold, so the content is real —
but the *name* promises connectedness and delivers a census. Nothing in the
concept says which elements touch which.

Two consequences:

1. **Missing middle rung.** The ladder jumps from three counts straight to
   `HalfEdgeNavigable` (constant-time half-edge walks). The representations that
   actually dominate — indexed triangle soup, face-vertex lists — sit between the
   two: they have incidence but no O(1) adjacency, so today they can implement
   nothing beyond the counts. Connected components, boundary loops, valence,
   manifoldness and orientability all need that missing rung.
2. **`EdgeCount` is a trap at the counts tier.** An undirected, deduplicated edge
   count is not derivable from a triangle-index buffer without building
   incidence. An implementer who returns `3 * FaceCount` or a half-edge count
   makes `EulerCharacteristic` silently wrong, and no law rejects it.
   `PolygonMesh3D.EdgeCount` already ships with a "closed manifold only"
   precondition for exactly this reason.

Separately, the stdlib README already requires `Edge` to be domain-qualified
wherever declared (the generic-noun rule), yet the mesh topology files declare
bare `EdgeIndex`, `EdgeList`, `EdgeAdjacency`, `EdgeCount`, `HasEdges`,
`CornerEdge`, `EdgeMidpoints`, `PerEdge`. `HalfEdge*` is the only family that
says which kind of edge it means.

## Done means

- [x] `MeshTopology` renamed `MeshElementCounts`; `EdgeCount` renamed
      `UndirectedEdgeCount`; all implementers and library bodies follow.
- [x] New `concept MeshIncidence inherits MeshElementCounts` covering the six
      incidence queries (V-F, F-V, V-E, E-V, E-F, F-E), with derived members
      (valence, vertex neighbours, boundary/wire/manifold edge tests) in
      `meshes-topology.library.plato`.
- [x] Unqualified mesh-domain `Edge` names qualified `UndirectedEdge*` (indices,
      lists, adjacency, counts, corner-edge numbering, attribute domain).
      Graph-theory `EdgeCount`/`GraphEdge`, image "edge" (edge detection,
      `ClampToEdge`) and polygon-segment helpers are a different sense and stay.
- [x] `tools/check-stdlib-fast.ps1` both gates PASS.
- [x] `stdlib/README.md` registry + `CONVENTIONS.md` owner list updated.

## Notes

`HalfEdgeNavigable` deliberately does NOT inherit `MeshIncidence`: walking a face
loop or a vertex fan to answer an incidence query is a build step, not a
constant-time read, so forcing the obligation on every half-edge type would push
allocation into the concept. A half-edge type may implement both.

`GenusIfClosed` kept its name: the `IfClosed` suffix already states the
precondition the counts tier cannot check.

Follow-ups filed: plato-316 (no concrete type implements `MeshIncidence` yet, so
`lint` reports LINT008 for it and the derived helpers are unreachable),
plato-317 (`docs/stdlib-ai-summary.txt` still carries the old names, among other
staleness). `Plato.Generated.V3/` also still contains `MeshTopology`; it is a
checked-in generated artifact and codegen ships from `stdlib-legacy`, so it was
left alone.
