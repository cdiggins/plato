# Solids, constructive geometry and boundary representation

Read [solid records](../../api/geometry/solids.types.md), [solid bodies](../../api/geometry/solids.library.md), [polyhedra operations](../../api/geometry/polyhedra.library.md), [polygon-soup CSG](../../api/geometry/solids-csg.library.md), [BREP interfaces](../../api/geometry/brep.concepts.md), [BREP records](../../api/geometry/brep.types.md) and [BREP bodies](../../api/geometry/brep.library.md).

## Different representations retain different information

A solid can be represented by a membership function, a signed distance function, an oriented boundary mesh, a parametric construction or an edge/face shell. They may describe the same set of points while retaining different information: distance, construction history, exact face geometry, or discrete connectivity. Converting among them can sample, discard, or invent a choice and is therefore often explicit.

Extrusion, revolution, loft and sweep records describe construction parameters. Named regular solids describe a particular geometric family. Inspect their mesh conversion and evaluation bodies independently; a stored construction is not automatically a closed, validated boundary.

## Polygon-soup Boolean operations

Union, intersection and difference cut faces against planes and keep fragments according to membership. The implementation requires closed solids with convex CCW outward faces. Difference reverses the retained cut boundary's winding. Its reference algorithm favors straightforward construction over acceleration.

**Known unreliable case:** exactly coplanar shared faces can be misclassified because fragment centroids lie on the other boundary. Do not present these operations as robust production CSG for arbitrary coincident CAD faces. The named plane tolerance is algorithm-specific, not a proof of exact predicates.

## BREP uses oriented edge citations

`Brep3D` stores vertices, undirected edges and faces. An edge owns one curve; a face loop contains forward or reversed uses of those edges. A watertight shell should cite every edge twice, in opposite orientations. This differs from a half-edge mesh, which stores directed sides for discrete navigation.

The implemented curve sum contains line segments; face geometry contains planar and bilinear patches. More general curved BREP is future vocabulary, not an existing arbitrary CAD kernel. `Validate` returns separate range, connectivity, endpoint-agreement and watertightness answers. Range flags guard the meaning of subsequent connectivity answers. These structural checks do not establish absence of geometric self-intersections or every solid-validity condition.

BREP deformation maps control points and rebuilds faceted cases. A nonlinear warp of a straight segment is generally curved, yet this representation keeps a straight segment between mapped endpoints. It therefore is not exact pointwise function composition. Tessellation also introduces a sampling choice.

## Related work

[CGAL's solid-modeling packages](https://doc.cgal.org/latest/Manual/packages.html) distinguish boundary meshes and robust Boolean models. Plato's faceted BREP and centroid-classified polygon-soup CSG should not be assigned the guarantees of those kernels. [PBRT's shapes](https://pbr-book.org/4ed/contents) illustrate a different goal: ray-query geometry does not itself prove a watertight solid.
