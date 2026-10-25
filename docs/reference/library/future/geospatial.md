# Geospatial and higher-dimensional vocabulary

Read [geospatial types](../../api/future/geo-spatial.types.md), [region interface](../../api/future/geo-spatial.concepts.md), [geospatial bodies](../../api/future/geo-spatial.library.md), [higher-dimensional types](../../api/future/higher-dimensions.types.md), and the [foundation point declarations](../../api/foundation/points.types.md) that include geodetic coordinates.

## A planetary coordinate is not a Cartesian point

Latitude, longitude and altitude need a reference ellipsoid/datum and angular/length units. ECEF coordinates are Earth-centered Cartesian values; ENU coordinates are a local tangent-frame representation; projected map coordinates depend on a projection and zone. Changing a datum or projecting onto a plane is a geometric operation, not copying three fields into a differently named record.

`IGeoRegion.ContainsCoordinate` ignores altitude. Bounds, paths, polygons and circles therefore describe surface regions, not arbitrary 3D volumes. A west bound greater than east has a special antimeridian-crossing meaning in the bounds body; it is not the usual empty Cartesian-interval encoding.

## Documented limitations

The polygon containment implementation uses straight segments in longitude/latitude coordinates. Its declaration describes geodesic edges, which are a different geometry. The difference matters for long edges, polar regions and dateline crossings. Ring containment is not antimeridian-aware; split wrapped rings before applying it. The circle distance helper uses a mean spherical Earth model, so do not interpret it as a precision ellipsoidal geodesic solution.

Reference ellipsoid, datum, projection, UTM zone, elevation-grid and map-tile records retain parameters. A declaration is not evidence of a complete coordinate-transform system, grid-shift loader or map-tile service. Tile indices belong to their declared zoom/layout scheme.

## Higher-dimensional objects and fractals

`NSphere`, `HyperplaneN`, `SimplexN` and `SubspaceN` use runtime-dimensional components. Establish matching dimensions, valid normals/bases and any independence conditions before applying mathematical formulas. Julia/Mandelbrot/quaternion-fractal parameter records describe iteration inputs; iteration budgets, escape tests and rendering are separate operations. Finite iteration is an approximation to an infinite-set definition, not an automatic membership proof.

## Related work

[GeoJSON RFC 7946](https://datatracker.ietf.org/doc/html/rfc7946#section-3.1.1) defines segment interpolation in longitude/latitude coordinates, which differs from a geodesic on an ellipsoid. That is a useful comparison for Plato's implemented polygon test, but it does not resolve the contradictory geodesic wording in its declaration. [glTF's coordinate-system section](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) illustrates a Cartesian scene convention; it does not solve planetary datum/projection semantics. Plato's geographic records must preserve those extra meanings at a scene boundary.
