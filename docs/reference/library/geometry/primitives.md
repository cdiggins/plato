# Geometric capabilities, primitives and bounds

Begin with [geometry interfaces](../../api/geometry/geometry.concepts.md), [geometry bodies](../../api/geometry/geometry.library.md), [fit records](../../api/geometry/geometry.types.md), [planar shapes](../../api/geometry/planar.types.md) and [operations](../../api/geometry/planar.library.md), [spatial shapes](../../api/geometry/spatial-primitives.types.md) and [operations](../../api/geometry/spatial-primitives.library.md), and [lines and planes](../../api/geometry/lines.types.md) with [line operations](../../api/geometry/lines.library.md).

## Ask for the capability you need

`IGeometry2D` or `IGeometry3D` identifies an embedding space. A curve in 3D still has one intrinsic parameter. `IBounded3D` supplies bounds; `IContainsPoint3D` supplies membership; `INearestPoint3D` supplies a nearest-point query; `ISupport3D` supplies a farthest point in a direction. None implies all the others.

Convexity, connectedness, manifoldness and closure are promises of a type or value's interpretation. They are not universally checked refinements. `IFitted` and fit-result records describe a fitted shape and residual quality, rather than proving that every estimator exists or that a fit was globally optimal.

A line segment, ray and infinite line have different parameter domains. Do not clamp an infinite-line query into a segment silently. A plane-hit record has a validity flag: check it before using the point or parameter. Parallel, coincident and zero-direction inputs need the operation's exact contract.

## Bounds are enclosures

[Interval and bounds declarations](../../api/foundation/intervals.types.md), [interfaces](../../api/foundation/intervals-bounds.concepts.md), and [bodies](../../api/foundation/intervals.library.md) define inclusive geometric intervals and axis-aligned boxes. An inverted interval encodes empty space; it contains nothing and is the union identity. Empty and zero-size are different: an inclusive `[a,a]` contains one coordinate.

A bounds query may be conservative rather than tight. The source explicitly leaves tighter bounds/support work for circular sectors and segments, superellipses, spherical caps and bilinear patches. An overestimate can be valid for broad-phase rejection while unsuitable for exact area, support or closest-point claims. Read each concrete body and its TODO.

Example: in the world frame, the triangle `(0,0,0),(1,0,0),(0,1,0)` has a right-hand normal along +Z. Reversing the last two vertices reverses that normal. The [frame and winding conventions](../../../../stdlib/CONVENTIONS.md) settle orientation across all shape families.

## Related work

[CGAL's geometry packages](https://doc.cgal.org/latest/Manual/packages.html) distinguish primitives, kernels and geometric algorithms. Plato's analogous vocabulary does not imply CGAL's exact predicates or robustness guarantees. [PBRT's bounding-box chapter](https://pbr-book.org/4ed/contents) explains the broad-phase role of conservative enclosures; exact geometry still requires a separate query.
