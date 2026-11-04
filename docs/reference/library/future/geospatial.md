# Geospatial and higher-dimensional vocabulary

This vocabulary is in the `future` tier, which is not part of the default library. Latitude, longitude, and altitude need a reference ellipsoid or datum, and they need angular and length units. ECEF coordinates are Earth-centered Cartesian values. ENU coordinates are a local tangent frame. Projected map coordinates depend on a projection and a zone. Changing a datum, or projecting onto a plane, is a geometric operation. Copying three fields into a differently named record is not that operation.

## A planetary coordinate is not a Cartesian point

`IGeoRegion.ContainsCoordinate` ignores altitude. Bounds, paths, polygons, and circles therefore describe surface regions, not arbitrary 3D volumes. In the bounds body, a west bound greater than the east bound means a band that crosses the antimeridian. That is not the empty-interval encoding used for Cartesian intervals.

## The polygon test is not geodesic

The polygon containment implementation uses straight segments in longitude and latitude. Its declaration describes geodesic edges, which are a different geometry. The difference matters for long edges, polar regions, and dateline crossings. Ring containment is not antimeridian-aware. Split a wrapped ring before applying it. The circle distance helper uses a mean spherical Earth model. It is not a precision ellipsoidal geodesic.

Reference ellipsoid, datum, projection, UTM zone, elevation-grid, and map-tile records store parameters. A declaration is not a complete coordinate-transform system, a grid-shift loader, or a map-tile service. A tile index belongs to the zoom and layout scheme that declared it.

## Higher-dimensional objects and fractals

`NSphere`, `HyperplaneN`, `SimplexN`, and `SubspaceN` use components whose dimension is known at runtime. Match the dimensions, and establish valid normals, bases, and any independence conditions, before applying a formula. Julia, Mandelbrot, and quaternion-fractal parameter records describe iteration inputs. The iteration budget, the escape test, and the rendering are separate operations. A finite iteration approximates an infinite-set definition. It is not an automatic membership proof.

## Where to look

- [Geospatial types](../../api/future/geo-spatial.types.md), [region interface](../../api/future/geo-spatial.concepts.md), and [geospatial bodies](../../api/future/geo-spatial.library.md)
- [Higher-dimensional types](../../api/future/higher-dimensions.types.md)
- [Foundation point declarations](../../api/foundation/points.types.md), including geodetic coordinates
- [Aspirational library domains](domains.md)

[GeoJSON RFC 7946](https://datatracker.ietf.org/doc/html/rfc7946#section-3.1.1) defines segment interpolation in longitude and latitude, which differs from a geodesic on an ellipsoid. That comparison fits the implemented polygon test, and it does not resolve the geodesic wording in the declaration. [glTF's coordinate-system section](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) is a Cartesian scene convention. It does not supply planetary datum or projection semantics. Geographic records have to keep those meanings at a scene boundary.
