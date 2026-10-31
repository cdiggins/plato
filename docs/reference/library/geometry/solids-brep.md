# Solids, constructive geometry and boundary representation

A solid can be a membership test, a signed distance, an oriented boundary mesh, a parametric construction, or an edge and face shell. Those records can describe the same set of points and still keep different facts: distance, construction history, exact face geometry, or discrete connectivity. Converting one into another samples, drops information, or picks a choice the source did not store.

## Several records can name one solid

Extrusion, revolution, loft, and sweep records store construction parameters. Named regular solids name a geometric family. A stored construction is not a closed, validated boundary. Read the mesh conversion and the evaluation body for the type you are calling.

## Polygon-soup Boolean operations

Union, intersection, and difference cut faces against planes and keep fragments by membership. The implementation requires closed solids whose faces are convex, counterclockwise, and outward. Difference reverses the winding of the retained cut boundary. The reference algorithm favors a direct construction over an accelerated one.

Exactly coplanar shared faces can be misclassified, because fragment centroids lie on the other boundary. These operations are not a robust Boolean kernel for arbitrary coincident CAD faces. The named plane tolerance belongs to this algorithm. It is not a proof of exact predicates.

## A BREP shell cites oriented edges

`Brep3D` stores vertices, undirected edges, and faces. An edge owns one curve. A face loop cites those edges forward or reversed. A watertight shell cites every edge twice, in opposite orientations. A half-edge mesh is a different structure: it stores directed sides for walking a discrete mesh.

The curve sum in this representation contains line segments. Face geometry contains planar and bilinear patches. A more general curved boundary representation is future vocabulary, not an arbitrary CAD kernel.

`Validate` returns separate answers for range, connectivity, endpoint agreement, and watertightness. Range flags qualify the connectivity answers that follow them. These checks do not establish that the solid is free of geometric self-intersections, or that every solid-validity condition holds.

Deforming a BREP maps control points and rebuilds the faceted cases. A nonlinear warp of a straight segment is generally a curve. This representation keeps the straight segment between the mapped endpoints, so the result is not the image of the original segment. Tessellation also chooses a sampling.

## Where to look

- [Solid records](../../api/geometry/solids.types.md) and [solid bodies](../../api/geometry/solids.library.md)
- [Polyhedra operations](../../api/geometry/polyhedra.library.md)
- [Polygon-soup CSG](../../api/geometry/solids-csg.library.md)
- [BREP interfaces](../../api/geometry/brep.concepts.md), [records](../../api/geometry/brep.types.md), and [bodies](../../api/geometry/brep.library.md)

[CGAL's solid-modeling packages](https://doc.cgal.org/latest/Manual/packages.html) distinguish boundary meshes from robust Boolean models, and [PBRT's shapes](https://pbr-book.org/4ed/contents) are built for ray queries. Neither guarantee transfers to this faceted BREP or to centroid-classified polygon-soup CSG.
