---
id: plato-361
title: Resolve HalfEdge vs DirectedEdge vocabulary overlap
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-302, plato-314, plato-316, stdlib/topology-half-edges.plato]
---

## Idea
Confusion between `HalfEdge` and `DirectedEdge` vocabulary — which to keep? Today: `HalfEdge` / `HalfEdgeMesh` / `HalfEdgeNavigable` are first-class (`topology-half-edges.plato`, `topology.concepts.plato`). `DirectedEdge` appears as methods returning `Array<VertexPair>` (`DirectedEdges` on triangle/quad face arrays in `meshes-faces.library.plato`), not as a dual topology IR. The confusion is naming overlap between half-edge mesh structure and "oriented edge as vertex pair."

## Assumptions
- Half-edge remains the navigable mesh IR (plato-302 prefers edge-uses for BREP, half-edges for meshes).
- DirectedEdges-as-VertexPair is useful for algorithms that do not need twin/next pointers.
- Renaming or documenting the distinction beats deleting one blindly.

## Design decisions
- **Keep both roles** — HalfEdge (structured) vs DirectedEdge/VertexPair (ephemeral orientation) with clearer names.
- **Rename candidates** — DirectedEdges → OrientedVertexPairs / FaceSideEdges; or introduce type DirectedEdge = VertexPair alias.
- **Docs** — CONVENTIONS / topology README table.

## Related
- `stdlib/topology-half-edges.plato` — HalfEdge, HalfEdgeMesh.
- `stdlib/meshes-faces.library.plato` — DirectedEdges → VertexPair.
- [plato-302](plato-302.md) — BREP edge-uses vs HalfEdgeMesh.
- [plato-314](plato-314.md) / [plato-316](plato-316.md) — UndirectedEdge* incidence naming.

## Approaches
Short term: glossary clarifying HalfEdge vs directed vertex-pair vs UndirectedEdge index; rename DirectedEdges if still ambiguous.
Long term: optional `type DirectedEdge = VertexPair` only if it earns its keep; never two mesh IRs.
Adjacent: retire unused synonyms.

## Bedrock
Protects the **one navigable mesh IR (HalfEdgeMesh)** invariant while allowing ephemeral oriented pairs. Verdict: **right**. Simple version must NOT invent a second half-edge-like structure named DirectedEdge.

## Done means
- [ ] Written glossary: HalfEdge vs DirectedEdges vs UndirectedEdge*
- [ ] Rename or type-alias decision recorded
- [ ] Call sites use the chosen names consistently in one topology-facing library

## Simplest possible implementation
Docs + rename `DirectedEdges` → `OrientedVertexPairs` (or similar); keep HalfEdge.
- Pros: clears confusion fast.
- Cons: churn on method name.

## Case against
- "Directed edge" is standard graph language; renaming away may hurt.
- Problem is education, not code — a paragraph may suffice.
- Verdict: **pursue** glossary; rename only if agents keep conflating after docs.
