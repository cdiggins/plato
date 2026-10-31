# Fields, signed distance and deformations

A scalar field maps a point to a number. A vector field returns a vector. A deformation maps a point to another point. The coordinates can look similar. The result types do not mean the same thing.

Constant fields and closure-backed fields are ordinary values. An explicit expression graph stores operator nodes, and source nodes read a caller-supplied field list. Each operand has to reference a lower-indexed node. Evaluation then descends on a nonnegative node index and stops. That argument holds when the references are valid and the leaves terminate. A malformed graph does not satisfy it.

## A name does not prove a distance

The signed-distance convention is negative inside, zero on the boundary, and positive outside. The name does not mean the value is the exact distance. The source separates an exact distance, a conservative lower bound on the exterior, and a field that may overestimate. Sphere tracing needs a valid step bound along the ray. A correct sign is not that bound.

For an exact signed distance `d`, the intended eikonal property is `|gradient d| = 1` where the gradient exists. At an edge, a medial axis, or a min or max seam, a unique gradient may not exist. A lower-bound field can share the zero set and still fail to preserve distance or unit gradients. Adding a number to that field can move the boundary by a different geometric offset.

If a nonnegative exterior function `f` vanishes on the surface and is K-Lipschitz, then `f(p)/K` does not exceed the distance to the zero set. For every zero `q`, `f(p) = |f(p) - f(q)| <= K*|p - q|`, and the infimum over `q` is the bound. That is a conditional proof. It is not a checked K for every warp in the library.

Bounds stored on a bounded signed distance are conservative culling hints. Evaluation still calls the underlying field. A false bound can hide real geometry. Converting a distance field to a Boolean region keeps membership and drops the metric.

A placed signed distance stores a rigid pose and a positive uniform scale. The query goes back to source coordinates, and the returned distance is scaled into world units. Nonuniform scale or shear does not preserve exact distances. An arbitrary affine pullback needs its own bound correction before sphere tracing.

## A warp moves points

Twist, bend, taper, shear, and mapping deformations move points. Weights come from separate scalar fields. After mesh positions change, recompute normals, or apply a Jacobian transformation you can justify. A finite-difference derivative needs a step and access to the domain. `Eval` on a generic field does not return an analytic gradient. Finite differences are not analytic derivatives.

## Where to look

- [Field interfaces](../../api/geometry/fields.concepts.md), [field records](../../api/geometry/fields.types.md), [field algebra](../../api/geometry/fields-implicits.library.md), and [graph evaluation](../../api/geometry/fields-graphs.library.md)
- [Distance interfaces](../../api/geometry/implicit-sdf.concepts.md), [distance records](../../api/geometry/implicit-sdf.types.md), and [distance bodies](../../api/geometry/implicit-sdf.library.md)
- [Deformation types](../../api/geometry/deformations.types.md) and [deformation bodies](../../api/geometry/deformations.library.md)

[PBRT's geometry chapters](https://pbr-book.org/4ed/contents) separate geometric normals from transformations, and [DLMF](https://dlmf.nist.gov/) is the differential background. Distance fidelity here follows from how the field was built. `FunctionSdf3D` does not carry a proof that a later refinement stayed exact.
