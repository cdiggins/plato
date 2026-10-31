# Geometric capabilities, primitives and bounds

`IGeometry2D` or `IGeometry3D` says which space the object lives in. A curve in 3D still has one intrinsic parameter. `IBounded3D` returns bounds. `IContainsPoint3D` returns membership. `INearestPoint3D` returns a nearest point. `ISupport3D` returns a farthest point in a direction. None of those interfaces supplies the others.

## One interface is one query

Convexity, connectedness, manifoldness, and closure are promises about how a type or a value is meant to be read. They are not checks the library runs on every value. `IFitted` and the fit-result records describe a fitted shape and a residual. They do not mean every estimator exists, or that a fit was the global optimum.

A line segment, a ray, and an infinite line have different parameter domains. An infinite-line hit stays unclamped unless that operation's contract clamps it. A plane-hit record has a validity flag. Read it before using the point or the parameter. Parallel, coincident, and zero-direction inputs follow the contract of the operation you called.

## Bounds enclose

Intervals and bounds are inclusive. An axis-aligned box is the enclosure those intervals describe. An inverted interval is empty: it contains nothing, and it is the identity for union. Empty and zero-size are different. An inclusive `[a, a]` contains one coordinate.

A bounds query may be conservative rather than tight. The source leaves tighter bounds and support queries for circular sectors and segments, superellipses, spherical caps, and bilinear patches. An overestimate can reject objects in a broad phase and still be the wrong input for exact area, support, or closest point. Read the concrete body and its TODO.

In the world frame, the triangle `(0,0,0), (1,0,0), (0,1,0)` has a right-hand normal along +Z. Reversing the last two vertices reverses that normal. The frame and winding conventions settle orientation for the shape families.

## Where to look

- [Geometry interfaces](../../api/geometry/geometry.concepts.md), [geometry bodies](../../api/geometry/geometry.library.md), and [fit records](../../api/geometry/geometry.types.md)
- [Planar shapes](../../api/geometry/planar.types.md) and [operations](../../api/geometry/planar.library.md)
- [Spatial shapes](../../api/geometry/spatial-primitives.types.md) and [operations](../../api/geometry/spatial-primitives.library.md)
- [Lines and planes](../../api/geometry/lines.types.md) and [line operations](../../api/geometry/lines.library.md)
- [Interval and bounds declarations](../../api/foundation/intervals.types.md), [interfaces](../../api/foundation/intervals-bounds.concepts.md), and [bodies](../../api/foundation/intervals.library.md)
- [Frame and winding conventions](../../../../stdlib/CONVENTIONS.md)

[CGAL's geometry packages](https://doc.cgal.org/latest/Manual/packages.html) separate primitives, kernels, and geometric algorithms. The same words here do not include CGAL's exact predicates. [PBRT's bounding-box chapter](https://pbr-book.org/4ed/contents) is the broad-phase role of a conservative enclosure. Exact geometry still needs its own query.
