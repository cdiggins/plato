# Matrices, rotations and transforms

Read [matrix types](../../api/foundation/matrices.types.md), [interfaces](../../api/foundation/matrices.concepts.md), [matrix bodies](../../api/foundation/matrices.library.md), [matrix operations](../../api/foundation/matrices-ops.library.md), [rotation types](../../api/foundation/rotations.types.md), [rotation operations](../../api/foundation/rotations-ops.library.md), [transform types](../../api/foundation/transforms.types.md), [interfaces](../../api/foundation/transforms.concepts.md) and [transform bodies](../../api/foundation/transforms.library.md).

## Rows and application order

Plato uses row vectors: `v' = v M`. The element at zero-based `(r,c)` is component `c` of stored row `r`; named `Row1` is row index zero. Matrix composition applies left to right: `v(A B)=(v A) B`. An affine homogeneous matrix represents translation in addition to a linear transformation; a vector and a point differ in how translation applies.

Quaternions introduce an intentional order difference: their Hamilton product `a.Multiply(b)` applies `b` first. `Concatenate(a,b)` applies `a` first. `Compose(first,second)` is the library's representation-independent “first, then second” spelling. Never transfer multiplication order solely from matrices to quaternions.

The transform interfaces define a hierarchy: a rotation is rigid; a rigid map preserves distances and angles; an affine map preserves lines and parallelism but may shear or scale. Converting a general matrix to a pose requires the stated rigidity precondition. A singular matrix has no inverse. A negative determinant reverses orientation and face winding.

## Placement is not Euler naming

World space is right-handed and Z-up. Euler names are separately specified: yaw is about Y, pitch about X, roll about Z; `RotationOrder` lists first-applied fixed-world axes first. Do not reinterpret yaw as “rotation around the world up axis” merely because another engine does so.

`IAffine2D/3D`, `IRigid2D/3D` and `IRotational3D` identify faithful conversions to common representations. They allow composition across representations without inventing a sampling resolution. Arbitrary point deformations are broader; see [fields and deformations](../geometry/fields.md).

## Numerical and interface limits

Dense, sparse, symmetric and banded records have distinct layout contracts. Read the relevant declaration before constructing storage. The [generic numeric helpers](../../api/foundation/numeric-structures.library.md) explicitly note a generic transpose construction gap: element access alone cannot manufacture an arbitrary implementing matrix. Concrete overloads must be inspected independently.

## Related work

[System.Numerics.Matrix4x4](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4) is the intended matrix-layout and row-vector interoperation model. [PBRT's transformation chapter](https://www.pbr-book.org/4ed/Geometry_and_Transformations/Transformations) is useful mathematics, but formulas written for column vectors must be transposed and composition order reconsidered before use in Plato.
