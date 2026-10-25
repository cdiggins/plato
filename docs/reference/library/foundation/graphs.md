# Graph data and graph summaries

[Graph types](../../api/foundation/graphs.types.md), [the graph interface](../../api/foundation/graphs.concepts.md), and [graph bodies](../../api/foundation/graphs.library.md) describe vertices, edges and result records. Graph vertices are implicit positions in `[0,VertexCount)`; isolated vertices therefore remain present even if no edge names them.

## Choose the storage contract

An edge list stores source/target pairs. For an undirected graph their order has no meaning; store each logical edge according to the representation's convention rather than automatically adding both orientations. A `WeightedGraph` adds a unitless numeric weight whose interpretation is set by the application, such as distance or cost.

`AdjacencyMatrix` uses a flat row-major store: entry `r*V+c` is the weight from `r` to `c`. Zero denotes no edge, so this representation cannot distinguish a zero-weight edge from absence without a separate encoding. CSR adjacency uses an offset array and neighbor references; vertex `v` has neighbors in the half-open span from offset `v` to offset `v+1`. A hypergraph uses the same packing idea for edges that join arbitrary numbers of vertices.

Typed `GraphVertexIndex` values must be checked against the owning graph. Negative indices encode absence for parent pointers and unmatched results. A shortest-path parent of “none” can mean source or unreachable, so inspect the accompanying source and reachability convention before reading a distance.

## What the small interface can prove

`IGraph` exposes vertex count, edge count and directedness. Its generic helpers can compute summaries; they cannot traverse adjacency that the interface does not expose. For a finite undirected graph, summing degrees counts each edge twice, yielding `2*EdgeCount`. For a directed graph the sum of in- plus out-degrees has the same value. This is a combinatorial argument, not evidence of a concrete traversal implementation.

The “maximum possible edges” helper assumes a simple graph: no self-loops and no parallel edges. Completeness and density derived from that maximum have that precondition. Storing a traversal, matching, flow or layout result type does not supply an algorithm to produce it; inspect the bodies before expecting BFS, Dijkstra or a max-flow solver.

## Related work

[NetworkX's graph types](https://networkx.org/documentation/stable/reference/classes/index.html) explicitly distinguish directedness and parallel-edge support. Plato's `IGraph` exposes directedness and counts, while its simple-graph density formulas add assumptions beyond that interface. Do not copy a multigraph or self-loop-containing graph into those formulas without reconciling the model.
