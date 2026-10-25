# Fields, signed distance and deformations

Read [field interfaces](../../api/geometry/fields.concepts.md), [field records](../../api/geometry/fields.types.md), [field algebra](../../api/geometry/fields-implicits.library.md), [graph evaluation](../../api/geometry/fields-graphs.library.md), [distance interfaces](../../api/geometry/implicit-sdf.concepts.md), [distance records](../../api/geometry/implicit-sdf.types.md), [distance bodies](../../api/geometry/implicit-sdf.library.md), [deformation types](../../api/geometry/deformations.types.md) and [deformation bodies](../../api/geometry/deformations.library.md).

## Functions over space

A scalar field maps a point to a number; a vector field returns a displacement-like vector; a deformation maps a point to a new point. Those result types convey different meanings even when coordinates look similar. Constant and closure-backed fields are ordinary values. Explicit expression graphs store operator nodes; source nodes use a caller-supplied field list.

Graph operands must reference lower-indexed nodes. Evaluation then terminates by descent on the nonnegative node index. This is a mathematical well-foundedness argument conditional on valid references and terminating leaf evaluation; an arbitrary malformed graph does not satisfy it.

## Sign, distance and safety are different contracts

An SDF convention is negative inside, zero at the boundary, positive outside. The name does not prove exact distance. The source distinguishes exact distance, conservative exterior lower bound and a field that may overestimate. Sphere tracing is safe only with an appropriate step bound along the ray, not merely a correct sign.

For an exact signed distance `d`, the intended eikonal property is `|gradient d|=1` wherever differentiable. At edges, medial axes or min/max seams, a unique gradient may not exist. A lower-bound field may share the same zero set without preserving distance or unit gradients. Offsetting it by a numeric value can therefore produce a different geometric offset.

If a nonnegative exterior function `f` vanishes on the surface and is known to be K-Lipschitz, then `f(p)/K` does not exceed distance to the zero set: for every zero `q`, `f(p)=|f(p)-f(q)| <= K*|p-q|`; take the infimum over `q`. This is a conditional mathematical proof, not a verified K bound for every library warp.

Bounds on a bounded SDF are conservative culling hints; evaluation still delegates to the underlying field. False bounds can wrongly cull real geometry. Converting a distance field to a Boolean region retains membership and loses metric information.

A placed SDF uses a rigid pose and a positive uniform scale: transform the query back to source coordinates and scale the returned distance into world units. Nonuniform scaling or shear does not generally preserve exact distances; an arbitrary affine pullback needs a justified bound correction before sphere tracing.

## Applying a warp

Twist, bend, taper, shear and mapping deformations move points. Weights come from separate scalar fields. After changing mesh positions, recompute normals or apply a justified Jacobian-based transformation. Finite-difference derivatives require a step and domain access; a generic field's `Eval` alone does not magically supply an analytic gradient.

## Related work

[PBRT's geometry chapters](https://pbr-book.org/4ed/contents) distinguish geometric normals and transformations. [DLMF's mathematical methods](https://dlmf.nist.gov/) supply differential foundations. Plato's distance fidelity is a property of construction history, not an automatically proved refinement carried by `FunctionSdf3D`.
