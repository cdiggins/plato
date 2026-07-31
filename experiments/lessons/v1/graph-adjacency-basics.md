---
lesson: graph-adjacency-basics
title: Graph Adjacency Basics
domain: Advanced & applied
v3-files: [65-graphs.plato]
audience: Basic discrete math or programming with linked structures
status: draft-v1
---

# Graph Adjacency Basics

A road map, a mesh's dual, a dependency list, and a social network are the
same abstract object: **vertices** joined by **edges**. The interesting
choice is not "what is a graph?" — it is how you store the joins so that
the queries you care about ("who are my neighbors?", "is there an edge?")
are cheap. Edge lists, adjacency matrices, and CSR adjacency structures
answer those queries at very different prices.

## The idea

A graph $G = (V, E)$ has a finite vertex set $V$ and an edge set $E$ of
pairs from $V$. Edges may be **directed** (one-way streets) or
**undirected** (two-way). Weights on edges turn "is connected" into "how
costly is the connection?"

Three classic representations:

**Edge list.** Store every edge as a `(source, target)` pair (plus weight if
needed). Cheap to append. Expensive to answer "neighbors of $v$" (scan all
edges) or "is $(u,v)$ present?" without an auxiliary index.

**Adjacency matrix.** A $|V| \times |V|$ table; entry $(r,c)$ is the weight
of the edge from $r$ to $c$, or zero for "no edge." $O(1)$ edge existence.
$O(|V|)$ to list neighbors (scan a row). Memory $O(|V|^2)$ — painful for
sparse graphs.

**Adjacency structure (CSR / adjacency arrays).** For each vertex, store a
contiguous run of neighbor indices. An offset array marks where each run
starts. Neighbor iteration is optimal for sparse graphs; edge existence is
a search within one run (or a set beside the CSR).

```
Vertices:  0   1   2   3
Edges:     0-1, 0-2, 1-2, 2-3

CSR offsets:  [0, 2, 3, 5, 6]
neighbors:    [1, 2, 2, 0, 3, 2]
               ^--v0--^  ^v1^ ^--v2--^ ^v3^
```

(Undirected graphs usually store both directions in the neighbor lists, or
document that edges are canonicalized and queries must check both orders.)

Pick the representation for the dominant query. Analytics on dense graphs
like matrices; mesh duals and road networks like CSR; serialization and
simple construction like edge lists.

## In Plato

`65-graphs.plato` declares the family and the three storage forms.

```plato
concept GraphLike
    inherits Value
{
    VertexCount(x: Self): Integer;
    EdgeCount(x: Self): Integer;
    IsDirected(x: Self): Boolean;
}

// Vertices are the integers 0 through VertexCount - 1.
type GraphVertexIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type GraphEdge
    implements Value, Hashable
{
    Source: GraphVertexIndex;
    Target: GraphVertexIndex;
}

type Graph
    implements GraphLike
{
    VertexCount: Integer;
    Edges: Array<GraphEdge>;
    Directed: Boolean;
}
```

Weighted edges share the same index types:

```plato
type WeightedEdge
    implements Value
{
    Source: GraphVertexIndex;
    Target: GraphVertexIndex;
    Weight: Number;
}

type WeightedGraph
    implements GraphLike
{
    VertexCount: Integer;
    Edges: Array<WeightedEdge>;
    Directed: Boolean;
}
```

Dense and sparse alternatives:

```plato
type AdjacencyMatrix
    implements Value
{
    VertexCount: Integer;
    Weights: Array<Number>;
}

type AdjacencyStructure
    implements Value
{
    VertexCount: Integer;
    NeighborOffsets: Array<Integer>;
    NeighborIndices: Array<GraphVertexIndex>;
}
```

`AdjacencyMatrix.Weights` is row-major: index $r \cdot n + c$ holds the
weight from $r$ to $c$; **zero means no edge** (use weight $1$ for
unweighted edges).

`AdjacencyStructure` is CSR: `NeighborOffsets` has $n+1$ entries; neighbors
of $v$ occupy the half-open range
`[NeighborOffsets[v], NeighborOffsets[v+1])` inside `NeighborIndices`.

Usage-shaped sketches:

```plato
let g = Graph {
    VertexCount: 4,
    Edges: [
        GraphEdge { Source: GraphVertexIndex { Value: 0 },
                    Target: GraphVertexIndex { Value: 1 } },
        GraphEdge { Source: GraphVertexIndex { Value: 0 },
                    Target: GraphVertexIndex { Value: 2 } }
    ],
    Directed: false
};

// Matrix: edge 0 -> 1 with weight 1 at index 0*4 + 1
let m = AdjacencyMatrix {
    VertexCount: 4,
    Weights: [ /* 16 numbers, zeros elsewhere */ ]
};

// CSR neighbor walk for vertex v
let v = GraphVertexIndex { Value: 0 };
let start = offsets[v.Value];
let end = offsets[v.Value + 1];
// neighbors = NeighborIndices[start .. end)
```

Algorithm result types reuse the same indices — shortest paths as parent
pointers, components as labels:

```plato
type ShortestPathTree
    implements Value
{
    SourceIndex: GraphVertexIndex;
    ParentIndices: Array<GraphVertexIndex>;
    Distances: Array<Number>;
}

type ConnectedComponents
    implements Value
{
    ComponentLabels: Array<Integer>;
    ComponentCount: Integer;
}
```

Parent $-1$ means "source or unreachable," matching the global typed-index
sentinel convention.

## Pitfalls / fine print

**Implicit vertices.** `Graph.VertexCount` can exceed the vertices that
appear in `Edges`. Isolated vertices exist by count alone — do not infer
$|V|$ from the edge list.

**Undirected storage.** When `Directed` is false, each `GraphEdge` is an
unordered pair, but CSR builders usually emit two directed neighbor entries.
Know which layer you are on.

**Zero weight vs missing edge.** In `AdjacencyMatrix`, zero is absence. You
cannot store a legitimate zero-cost edge without a different sentinel
scheme — a real limitation for some algorithms.

**Offset arrays are `Integer`, not `GraphVertexIndex`.** Same CSR rule as
meshes: boundaries are not element references.

**Hypergraphs.** `Hypergraph` allows edges with any number of vertices. Do
not force them into `GraphEdge` pairs; incidence is a different CSR
(`HyperedgeOffsets` / `HyperedgeVertices`).

**Index spaces.** `GraphVertexIndex` is not a mesh `VertexIndex`. Mixing
them across a mesh-dual graph requires an explicit mapping array.

## Try it

1. Undirected triangle on vertices $0,1,2$. Write the three `GraphEdge`
   values for an edge-list `Graph` with `Directed: false`.
2. For that graph, if CSR stores both directions, how many entries are in
   `NeighborIndices`?
3. Why is edge existence $O(1)$ in `AdjacencyMatrix` but not in `Graph`?

<details>
<summary>Answers</summary>

1. Pairs $(0,1)$, $(1,2)$, $(2,0)$ (order within each pair arbitrary when
   undirected).
2. Six — each of three undirected edges contributes two directed neighbor
   slots.
3. Matrix indexes weight at $r\cdot n + c$ directly; an edge list must scan
   (or build a side index) to find a matching pair.

</details>

## Library recommendations

- **missing-function** — `65-graphs.plato`: no conversions
  `AdjacencyStructure(g: Graph)` / `AdjacencyMatrix(g: Graph)` (and reverse).
  Teaching representation trade-offs wants a declared build path; without it
  every caller hand-rolls CSR offsets.

- **missing-function** — `65-graphs.plato`: no
  `Neighbors(g: AdjacencyStructure, v: GraphVertexIndex):` slice/view API.
  The CSR layout is documented, but neighbor iteration is not a named
  operation on `GraphLike`.

- **wrong-shape** — `65-graphs.plato`: `AdjacencyMatrix` uses zero as
  "no edge," which collides with zero weights. A dedicated missing sentinel
  or a parallel `Array<Boolean>` presence mask would make weighted dense
  graphs honest.

- **doc-comment** — `65-graphs.plato`: `Graph` should state explicitly
  whether undirected edge lists store one canonical direction or both, so
  `EdgeCount` interpretation matches CSR expansion expectations.
