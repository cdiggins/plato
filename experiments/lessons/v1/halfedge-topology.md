---
lesson: halfedge-topology
title: Half-Edge Topology
domain: Meshes & spatial structures
v3-files: [30-topology.plato]
audience: High-school math and general programming background; comfort with arrays and indices
status: draft-v1
---

# Half-Edge Topology

A triangle mesh stored as "list of corners into a vertex buffer" answers one question
well: which three positions make this face? It answers neighborhood questions poorly.
What faces touch this edge? What is the next vertex around this face? Which edges bound
this hole? Brute force scans the whole mesh. **Half-edge** structure pays memory to make
those queries $O(1)$ or $O(\text{degree})$. Plato's v3 topology file is the vocabulary
for that structure — typed indices, the `HalfEdge` record, the navigable mesh, and the
CSR adjacency tables you use when a full half-edge mesh is more than you need.

## The idea

Every undirected mesh edge is split into two opposing **half-edges**, each directed.
One half-edge borders a face (or a hole); its **twin** borders the other side.

```
            Next
         -------->
     v0              v1
         <--------
            Twin of the above
```

Each half-edge stores:

- **Origin** — the vertex it leaves
- **Twin** — the opposite half-edge (or "none" on a boundary)
- **Next** / **Previous** — walk around the incident face loop
- **Face** — which face it borders (or "none" if it runs along a hole)

From any half-edge you can:

| Query | How |
|-------|-----|
| Destination vertex | `Origin` of `Twin` (if twin exists) |
| Next edge around face | `Next` |
| Adjacent face across edge | `Face` of `Twin` |
| Circulate around a vertex | leave via a half-edge, jump to twin, take next… |

```
  Face walk (CCW):   h → Next(h) → Next(Next(h)) → …
  Neighbor face:     Face(Twin(h))
  Boundary:          Twin(h) = none, or Face(h) = none for hole loops
```

This only works cleanly on **manifold** surfaces (optionally with boundary): every edge
has one or two incident faces, and the neighborhood of each vertex is a disk (or half-disk
on the boundary). Non-manifold junctions (three faces glued to one edge) break the twin
invariant.

Cheaper alternatives when you do not need full navigation:

- **Edge list + adjacency** — undirected `VertexPair`s with left/right faces
- **Corner table** — triangle corners with opposite-corner links (common in geometry
  processing)
- **CSR adjacency** — packed neighbor arrays for vertex–vertex or face–face queries

## In Plato

Typed indices make roles explicit (`30-topology.plato`):

```plato
type VertexIndex { Value: Integer; }   // -1 = none
type EdgeIndex   { Value: Integer; }
type FaceIndex   { Value: Integer; }
type CornerIndex { Value: Integer; }
type HalfEdgeIndex { Value: Integer; }
```

The half-edge record and mesh:

```plato
type HalfEdge
    implements Value
{
    Origin: VertexIndex;
    Twin: HalfEdgeIndex;
    Next: HalfEdgeIndex;
    Previous: HalfEdgeIndex;
    Face: FaceIndex;
}

type HalfEdgeMesh
    implements Value, MeshTopology, HalfEdgeNavigable
{
    Positions: Array<Point3D>;
    HalfEdges: Array<HalfEdge>;
    FaceHalfEdges: Array<HalfEdgeIndex>;   // one starter per face
    VertexHalfEdges: Array<HalfEdgeIndex>; // one outgoing per vertex (-1 if isolated)
}
```

Navigation is a concept (callers must not pass $-1$; boundaries appear as $-1$ *results*):

```plato
concept HalfEdgeNavigable
{
    OriginOf(x: Self, halfEdge: HalfEdgeIndex): VertexIndex;
    TwinOf(x: Self, halfEdge: HalfEdgeIndex): HalfEdgeIndex;
    NextOf(x: Self, halfEdge: HalfEdgeIndex): HalfEdgeIndex;
    PreviousOf(x: Self, halfEdge: HalfEdgeIndex): HalfEdgeIndex;
    FaceOf(x: Self, halfEdge: HalfEdgeIndex): FaceIndex;
}
```

Boundary loops gather hole half-edges (`Face = -1`) in walk order:

```plato
type BoundaryLoop
{
    HalfEdges: Array<HalfEdgeIndex>;
}
```

Undirected edges and CSR fallbacks:

```plato
type VertexPair { A: VertexIndex; B: VertexIndex; }  // canonical A.Value <= B.Value

type EdgeAdjacency
{
    Edges: Array<VertexPair>;
    LeftFaces: Array<FaceIndex>;
    RightFaces: Array<FaceIndex>;   // -1 on boundary; both -1 for wire edges
}

type VertexAdjacency
{
    Offsets: Array<Integer>;        // CSR: length = vertexCount + 1
    Neighbors: Array<VertexIndex>;
}
```

Topological health summary:

```plato
type TopologySummary
{
    VertexCount: Integer;
    EdgeCount: Integer;
    FaceCount: Integer;
    BoundaryLoopCount: Integer;
    ConnectedComponentCount: Integer;
    EulerCharacteristic: Integer;   // V - E + F; closed genus g → 2 - 2g
    Manifoldness: Manifoldness;
}
```

Usage-shaped snippets:

```plato
h0 = FaceHalfEdges[f]
h1 = NextOf(mesh, h0)
h2 = NextOf(mesh, h1)
// h0,h1,h2 walk face f counter-clockwise

opp = TwinOf(mesh, h0)
adjacentFace = FaceOf(mesh, opp)   // -1 if h0 was a boundary half-edge

vStart = VertexHalfEdges[v]
// circulate around v using Twin/Next until back at vStart
```

Corner tables for triangle-only pipelines:

```plato
type CornerTable
{
    Corners: Array<VertexIndex>;
    Opposites: Array<CornerIndex>;  // -1 on boundary edges
}
```

## Pitfalls / fine print

**Manifold assumption.** `HalfEdgeMesh` requires a manifold (possibly bounded) surface.
Non-manifold meshes need repair, a different structure, or acceptance that twins are
ambiguous.

**Sentinel discipline.** $-1$ means none. Never use it as an ordinary index into
`HalfEdges`. Navigation functions document that callers must not pass $-1$; check twin
before following it.

**Winding.** Face loops are counter-clockwise from the front (`WindingOrder`). Mixing CW
data into a CCW half-edge mesh inverts what "left" and "right" mean for
`EdgeAdjacency`.

**Canonical pairs.** `VertexPair` producers should emit $A.Value \le B.Value` except where
documented (line sets keep authored direction). Sorting matters for hashing edges.

**CSR half-open ranges.** Neighbors of $v$ are `Neighbors[Offsets[v] .. Offsets[v+1])`.
The final offset equals payload length. Off-by-one here corrupts every query.

**Half-edge count.** A closed manifold with $E$ undirected edges has $2E$ half-edges.
Boundary edges still have two half-edges (one with `Face = -1`) in the usual encoding —
twins exist; the hole side is marked on `Face`, not necessarily by missing twins.
Read the twin/face sentinels carefully for the specific builder you use; v3 allows twin
$-1$ on boundary in the `HalfEdge` doc comment.

**Euler characteristic.** For a closed connected orientable surface,
$\chi = 2 - 2g$. A sphere has $\chi = 2$; a torus $\chi = 0$. Boundaries and multiple
components change the formula — trust `TopologySummary` fields together, not
$\chi$ alone.

**Positions are geometry.** Topology is connectivity; `Positions` embed the mesh in
space. Algorithms that only need adjacency can ignore positions — but
`HalfEdgeMesh` still carries them as one value type.

## Try it

1. A tetrahedron has 4 vertices, 6 edges, 4 faces. What is $V - E + F$? What genus does
   that imply for a closed manifold?
2. Given half-edge $h$, how do you get the vertex $h$ points *toward* (its destination)?
3. Why is `FaceHalfEdges` length equal to face count, not half-edge count?

<details>
<summary>Answers</summary>

1. $4 - 6 + 4 = 2$, the sphere ($\chi = 2$, genus 0).
2. `OriginOf(mesh, TwinOf(mesh, h))` when a twin exists. (If twin is $-1$, the destination
   must be recovered another way — e.g. `OriginOf(Next(h))` still works on a face loop.)
3. It is a face → representative half-edge map: one starter per face loop, used to begin
   a walk. Every half-edge still lives in `HalfEdges`.

</details>

## Library recommendations

- **doc-comment** — `30-topology.plato`: `HalfEdge.Twin` says $-1$ when the edge is on a
  boundary, while `Face` says $-1$ when the half-edge runs along a hole. Teaching needs a
  single clarified invariant: do boundary edges use twin $-1$, face $-1$ on one side, or
  both? Ambiguity here is the #1 implementation fork among half-edge libraries.

- **missing-function** — `30-topology.plato`: `HalfEdgeNavigable` has atomic steps but no
  `DestinationOf`, `OppositeFaceOf`, `CirculateVertex`, or `BoundaryLoops(mesh)`. The
  lesson's query table is exactly the missing helper layer.

- **missing-function** — `30-topology.plato`: no `BuildHalfEdgeMesh(TriangleMesh3D)` (or
  from `PolygonMesh3D`) declaration. Topology is useless without a defined construction
  contract (manifold failure mode, winding, boundary twins).

- **pedagogy** — `30-topology.plato`: `CornerTable`, `EdgeAdjacency`, and `HalfEdgeMesh`
  coexist without a doc guide for when to choose which. A short file-level comparison
  (memory vs query set vs manifold requirement) would prevent treating them as interchangeable.
