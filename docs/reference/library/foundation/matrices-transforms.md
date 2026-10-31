# Matrices, rotations and transforms

Plato uses row vectors: `v' = v M`.

## Rows and application order

The element at zero-based `(r,c)` is component `c` of stored row `r`. Named `Row1` is row index zero. Matrix composition applies left to right: `v(A B)=(v A) B`. An affine homogeneous matrix represents translation in addition to a linear transformation. A vector and a point differ in how translation applies. Layout and the world frame are specified by the [global conventions](../../../../stdlib/CONVENTIONS.md). [System.Numerics.Matrix4x4](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4) is the intended matrix-layout and row-vector interoperation model.

Quaternions introduce an intentional order difference. Their Hamilton product `a.Multiply(b)` applies `b` first. `Concatenate(a,b)` applies `a` first. `Compose(first,second)` is the library's representation-independent "first, then second" spelling. Never transfer multiplication order solely from matrices to quaternions.

The transform interfaces define a hierarchy. A rotation is rigid. A rigid map preserves distances and angles. An affine map preserves lines and parallelism but may shear or scale. Converting a general matrix to a pose requires the stated rigidity precondition. A singular matrix has no inverse. A negative determinant reverses orientation and face winding. Formulas written for column vectors, including [PBRT's transformation chapter](https://www.pbr-book.org/4ed/Geometry_and_Transformations/Transformations), must be transposed, and the composition order reconsidered, before use in Plato.

## Euler angles and placement

World space is right-handed and Z-up. Euler names are separately specified: yaw is about Y, pitch about X, and roll about Z. `RotationOrder` lists first-applied fixed-world axes first. Do not reinterpret yaw as "rotation around the world up axis" merely because another engine does so.

`IAffine2D/3D`, `IRigid2D/3D`, and `IRotational3D` identify faithful conversions to common representations. They allow composition across representations without inventing a sampling resolution. Arbitrary point deformations are broader. See [fields and deformations](../geometry/fields.md).

## Numerical and interface limits

Dense, sparse, symmetric, and banded records have distinct layout contracts. Read the relevant declaration before constructing storage. The generic numeric helpers explicitly note a generic transpose construction gap: element access alone cannot manufacture an arbitrary implementing matrix. Concrete overloads must be inspected independently.

## Where to look

- [Matrix types](../../api/foundation/matrices.types.md), [matrix interfaces](../../api/foundation/matrices.concepts.md), [matrix bodies](../../api/foundation/matrices.library.md), and [matrix operations](../../api/foundation/matrices-ops.library.md)
- [Rotation types](../../api/foundation/rotations.types.md) and [rotation operations](../../api/foundation/rotations-ops.library.md)
- [Transform types](../../api/foundation/transforms.types.md), [transform interfaces](../../api/foundation/transforms.concepts.md), and [transform bodies](../../api/foundation/transforms.library.md)
- [Generic numeric helpers](../../api/foundation/numeric-structures.library.md)
