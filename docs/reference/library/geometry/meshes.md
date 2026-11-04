# Meshes, attributes and topology

A triangle mesh stores positions and faces that name those positions by index. A triangle soup can repeat the same coordinates on separate vertices. Matching coordinates do not create shared topology.

## An index is not a copy of the point

The generic mesh layers separate access to points, access to primitives, and indexed storage. Bounds and area often need only the primitives. Welding, adjacency, and subdivision need to know which vertices are shared. `IFace` is an ordered loop of vertex references. Its index is a corner of that face, not a global vertex index.

`ToTriangleMesh` is a separate operation because it can choose diagonals, drop attributes, or keep only the boundary of a volume mesh. Triangle, quad, and polygon meshes are not interchangeable inputs for every operation. Tetrahedral and hexahedral meshes describe cells in a volume, not only boundary faces.

## One vertex can carry several corner values

A rich mesh can attach values to vertices, corners, faces, or other domains. At a UV seam, one geometric vertex can have different corner UVs or normals. Putting those channels in one per-vertex array drops that split. The attribute's domain decides its length and its index.

Counterclockwise order is the front, or the outside. The right-hand rule gives the normal. Mirroring reverses the winding. A texture chart uses UV coordinates, and images use a separate top-left origin.

## Counts are not adjacency

`IMeshElementCounts` returns a vertex count, a count of deduplicated undirected edges, and a face count. `IMeshIncidence` says which elements touch which. It does not promise constant-time access or a particular order. `IHalfEdge` gives constant-time local navigation, and it does not inherit the incidence interface that materializes those sets. A boundary twin may be absent. A negative result is not a twin to follow.

For a finite cell decomposition, `chi = V - E + F` is the Euler characteristic. Reading a genus off that number also needs the documented assumptions: closed, connected, manifold, and orientable. Counting directed corners as undirected edges gives the wrong characteristic even when the arithmetic on those counts is right.

## Where to look

- [Mesh types](../../api/geometry/meshes.types.md), [mesh interfaces](../../api/geometry/meshes.concepts.md), [mesh bodies](../../api/geometry/meshes.library.md), and [polygon-mesh bodies](../../api/geometry/meshes-polygon.library.md)
- [Topology types](../../api/geometry/topology.types.md), [interfaces](../../api/geometry/topology.concepts.md), and [topology bodies](../../api/geometry/topology.library.md)

[CGAL's packages](https://doc.cgal.org/latest/Manual/packages.html) separate polygon meshes, half-edge structures, and mesh algorithms. An interface of the same name here does not include CGAL's validity checks, exact predicates, or complexity bounds.
