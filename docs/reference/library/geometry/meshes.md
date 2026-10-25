# Meshes, attributes and topology

[Mesh types](../../api/geometry/meshes.types.md), [mesh interfaces](../../api/geometry/meshes.concepts.md), [mesh bodies](../../api/geometry/meshes.library.md), [polygon-mesh bodies](../../api/geometry/meshes-polygon.library.md), [topology types](../../api/geometry/topology.types.md), [interfaces](../../api/geometry/topology.concepts.md), and [topology bodies](../../api/geometry/topology.library.md) distinguish geometry from connectivity.

## Positions, primitives and shared storage

A triangle mesh combines positions with face records naming vertex indices. A triangle soup may repeat identical positions independently; an indexed mesh shares positions deliberately. Coincident coordinates do not automatically establish shared topology.

The generic mesh layers separate access to points, access to primitives, and indexed storage. Bounds or area calculations often need only primitives. Welding, adjacency and subdivision need knowledge of shared vertex identity. `IFace` is an ordered loop of vertex references; its index is a corner within the face, not an arbitrary global vertex index.

`ToTriangleMesh` is explicit because it can choose diagonals, discard attributes, or retain only a volume mesh's boundary. Triangle, quad and polygon meshes therefore are not interchangeable encodings in every operation. Tetrahedral and hexahedral meshes describe cells in a volume rather than only boundary faces.

## Attribute domains prevent accidental alignment

A rich mesh may attach values to vertices, corners, faces or other domains. At a UV seam one geometric vertex may have different corner UVs or normals; forcing those channels into a single per-vertex array loses information. The attribute's declared domain determines its expected length and indexing.

CCW order is viewed from the front/outside; the right-hand rule gives the normal. Mirroring reverses winding. A texture chart uses UV coordinates with the separate top-left image convention.

## Counts are not adjacency

`IMeshElementCounts` supplies vertex, deduplicated undirected-edge and face counts. `IMeshIncidence` adds which elements touch which; it promises no constant-time access or result ordering. `IHalfEdge` supplies constant-time local navigation but deliberately does not inherit the materializing incidence interface. A boundary twin may be absent; never dereference a negative result.

For a finite cell decomposition, `chi = V-E+F` is the Euler characteristic. Inferring genus from it additionally requires the documented closedness, connectedness, manifoldness and orientability assumptions. Counting directed corners as undirected edges produces a wrong result even if the arithmetic is correct.

## Related work

[CGAL's packages](https://doc.cgal.org/latest/Manual/packages.html) separate polygon meshes, half-edge structures and mesh algorithms. Plato similarly separates geometric access from topology capabilities; an interface name does not inherit CGAL's validity checks, exact predicates or complexity guarantees.
