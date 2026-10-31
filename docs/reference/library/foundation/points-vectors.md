# Points, vectors, directions and axes

A point identifies a position. A vector identifies a displacement. They can share coordinate fields while supporting different operations.

## Positions and displacements

If `p=(1,2,0)` and `q=(4,6,0)`, the displacement from `p` to `q` is `(3,4,0)`, whose Euclidean length is 5. Translation changes positions and leaves displacements unchanged.

The [affine interface](../../api/foundation/algebra.concepts.md) uses `Between(a,b)=b-a` and `Add(a,d)=a+d`. In exact arithmetic, with `d=b-a`, substitution proves `Add(a,Between(a,b))=a+(b-a)=b`. This is a mathematical derivation under vector-space laws, not a machine-checked proof about floating-point execution.

`IOriginBased` additionally chooses an origin and converts between positions and their offsets. `PositionVector` and `ToPoint` expose a change in interpretation. A displacement is not silently a position. World space is right-handed and Z-up under the [global conventions](../../../../stdlib/CONVENTIONS.md). [PBRT's vectors, points and normals chapters](https://pbr-book.org/4ed/contents) draw the same distinction between positions, displacements, and normals, and Plato's wrappers and Z-up frame are its own, so use a Plato overload rather than a host API call.

## Normalization and direction promises

A `Direction3D` wraps a vector intended to have unit length. Construct through the documented `FromVector`, `TryFromVector`, or fallback normalization helper, and inspect the exact signature for failure reporting. `FromVectorUnchecked` explicitly trusts the caller. Normalizing zero has no unique mathematical answer. Overflow and very small magnitudes also require numerical care.

`NormalizeOr(v,fallback)` makes the fallback visible. A manually constructed direction can violate the unit invariant. Nonuniform scaling can invalidate unit length. A surface normal under such a transform needs the inverse-transpose linear map and renormalization, not ordinary point transformation.

`VectorN` has runtime arity. Compatible component counts remain a caller obligation wherever operations combine arrays. Numeric tuples such as `Number3` communicate components without necessarily giving the same geometric meaning as `Vector3D`.

`Axis2D`, `Axis3D`, and `SignedAxis3D` express cardinal choices. Their `Ordinal` yields an array component index when needed. An arbitrary integer is not a valid axis selector.

## Where to look

- [Point types](../../api/foundation/points.types.md) and [point interfaces](../../api/foundation/points.concepts.md)
- [Vector types](../../api/foundation/vectors.types.md), [vector interfaces](../../api/foundation/vectors.concepts.md), and [vector bodies](../../api/foundation/vectors.library.md)
- [Numeric helpers](../../api/foundation/numeric-structures.library.md)
- [Typed axes](../../api/foundation/axes.types.md) and [axis operations](../../api/foundation/axes.library.md)
