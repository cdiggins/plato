# Spatial search and mesh reconstruction

Read [spatial interfaces](../../api/geometry/spatial.concepts.md), [spatial records](../../api/geometry/spatial-structures.types.md), [spatial bodies](../../api/geometry/spatial-structures.library.md), [remeshing records](../../api/geometry/remeshing.types.md) and [remeshing bodies](../../api/geometry/remeshing.library.md). These are two algorithm families: locating candidates and rebuilding geometry.

## Broad phase selects candidates

BVHs, kd-trees, quadtrees, octrees and spatial hashes partition or enclose data for faster rejection. A BVH stores bounding nodes and a permutation of primitive indices; it need not store the primitives themselves. `CandidatesInBounds` can return false positives because an intersecting leaf is reported as a whole. Test the actual primitive in a narrow phase.

The BVH ray query reports entry into a leaf box, **not a confirmed surface intersection**. Its surface-related fields use empty/synthetic values. Do not display that position or normal as a geometric hit without a primitive test. A nearest-neighbor result similarly needs the declared metric and search contract; a result record alone does not establish an optimal search implementation.

Stored indices are meaningful relative to the source collection used to build the structure. Reordering or replacing geometry without rebuilding or updating bounds can invalidate queries. Empty trees and negative child references need explicit handling before node access.

## Remeshing rebuilds a value

The reference remeshing design makes decisions against one unchanged input mesh and produces a new mesh. Splitting, collapsing, flipping, welding, subdivision, smoothing and decimation therefore must coordinate index remaps and conflict choices; they are not arbitrary in-place edits.

An edge collapse needs topology assumptions such as a link condition. Smoothing can shrink geometry or move a boundary. Subdivision refines a control mesh; it does not automatically preserve an analytic surface. Quadric error measures are local approximation criteria, not a certified maximum Hausdorff-distance bound.

The source explicitly builds topology by scanning corners against corners because no concrete keyed-container path is available there. That preprocessing is quadratic in corner count and can dominate every pass. Do not promise interactive processing of large scientific meshes based solely on the existence of a remeshing function.

## Related work

[CGAL's spatial-searching, acceleration and mesh-processing packages](https://doc.cgal.org/latest/Manual/packages.html) distinguish queries, predicates and reconstruction algorithms. Plato has related vocabulary, but performance and robustness must come from its own bodies and tests. [PBRT's acceleration structures](https://pbr-book.org/4ed/contents) illustrate the same separation between a box candidate and a true shape hit.
