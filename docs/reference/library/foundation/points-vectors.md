# Points, vectors, directions and axes

Use [point types](../../api/foundation/points.types.md), [point interfaces](../../api/foundation/points.concepts.md), [vector types](../../api/foundation/vectors.types.md), [vector interfaces](../../api/foundation/vectors.concepts.md), [vector bodies](../../api/foundation/vectors.library.md), [numeric helpers](../../api/foundation/numeric-structures.library.md), and [typed axes](../../api/foundation/axes.types.md) with [axis operations](../../api/foundation/axes.library.md).

## Positions and displacements

A point identifies a position; a vector identifies a displacement. They can share coordinate fields while supporting different operations. If `p=(1,2,0)` and `q=(4,6,0)`, the displacement from `p` to `q` is `(3,4,0)`, whose Euclidean length is 5. Translation changes positions but leaves displacements unchanged.

The [affine interface](../../api/foundation/algebra.concepts.md) uses `Between(a,b)=b-a` and `Add(a,d)=a+d`. In exact arithmetic, with `d=b-a`, substitution proves `Add(a,Between(a,b))=a+(b-a)=b`. This is a mathematical derivation under vector-space laws, not a machine-checked proof about floating-point execution.

`IOriginBased` additionally chooses an origin and converts between positions and their offsets. `PositionVector` and `ToPoint` expose a change in interpretation; a displacement is not silently a position.

## Normalization and direction promises

A `Direction3D` wraps a vector intended to have unit length. Construct through the documented `FromVector`, `TryFromVector` or fallback normalization helper as appropriate; inspect the exact signature for failure reporting. `FromVectorUnchecked` explicitly trusts the caller. Normalizing zero has no unique mathematical answer. Overflow and very small magnitudes also require numerical care.

`NormalizeOr(v,fallback)` makes the fallback visible. A manually constructed direction can violate the unit invariant. Nonuniform scaling can invalidate unit length; a surface normal under such a transform needs the inverse-transpose linear map and renormalization, not ordinary point transformation.

`VectorN` has runtime arity; compatible component counts remain a caller obligation wherever operations combine arrays. Numeric tuples such as `Number3` communicate components without necessarily giving the same geometric meaning as `Vector3D`.

`Axis2D`, `Axis3D` and `SignedAxis3D` express cardinal choices. Their `Ordinal` yields an array component index when needed; an arbitrary integer is not a valid axis selector.

## Related work

[PBRT's vectors, points and normals chapters](https://pbr-book.org/4ed/contents) make the same essential distinction between positions, displacements and normal covectors. Plato's concrete wrappers and capability interfaces differ from PBRT's C++ classes; use Plato overloads and its Z-up convention rather than copying a host API call.
