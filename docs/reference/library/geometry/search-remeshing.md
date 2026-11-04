# Spatial search and mesh reconstruction

Spatial search and remeshing are different jobs. Search returns candidate indices. Remeshing returns a new mesh.

## A leaf box is not a surface hit

A BVH, kd-tree, quadtree, octree, or spatial hash partitions or encloses data so queries can reject most of it. A BVH stores bounding nodes and a permutation of primitive indices. It does not have to store the primitives. `CandidatesInBounds` can return false positives, because an intersecting leaf is reported whole. Test the primitive itself in a narrow phase.

The BVH ray query reports where the ray enters a leaf box. That is not a confirmed surface intersection. The surface fields on that hit are empty or synthetic. The position and the normal are not a geometric surface hit until a primitive test says so. A nearest-neighbor result likewise depends on the metric and the search contract the operation declares. The result record does not mean the search was an optimal implementation.

Stored indices refer to the collection used to build the structure. Reordering or replacing that geometry without rebuilding, or without updating bounds, invalidates later queries. An empty tree, or a negative child reference, has to be handled before you index a node.

## Remeshing returns a new mesh

The reference remeshing design reads one unchanged input mesh and produces a new mesh. Splitting, collapsing, flipping, welding, subdivision, smoothing, and decimation have to agree on index remaps and on which change wins. They are not free in-place edits of the input.

An edge collapse needs topology assumptions such as a link condition. Smoothing can shrink the shape or move a boundary. Subdivision refines a control mesh. It does not automatically stay on an analytic surface. A quadric error is a local approximation criterion. It is not a certified maximum Hausdorff distance.

The source builds topology by scanning corners against corners, because no concrete keyed container is available there. That preprocessing is quadratic in the corner count and can dominate every pass. A remeshing function on its own is not a reason to expect interactive processing of a large mesh.

## Where to look

- [Spatial interfaces](../../api/geometry/spatial.concepts.md), [spatial records](../../api/geometry/spatial-structures.types.md), and [spatial bodies](../../api/geometry/spatial-structures.library.md)
- [Remeshing records](../../api/geometry/remeshing.types.md) and [remeshing bodies](../../api/geometry/remeshing.library.md)

[CGAL's spatial-searching, acceleration, and mesh-processing packages](https://doc.cgal.org/latest/Manual/packages.html) separate queries, predicates, and reconstruction. Performance and robustness here have to come from these bodies and their tests. [PBRT's acceleration structures](https://pbr-book.org/4ed/contents) make the same split between a box candidate and a hit on the shape.
