# Surfaces and differential geometry

A parametric surface maps a UV coordinate to a 3D point. The preferred domain is usually the unit square. In the surface evaluators, U is the first direction, also called the profile or column direction, and V is the second, also called the sweep or row direction. `ClosedU` and `ClosedV` say whether that parameter seams shut. A mesh sampler has to honor them, or it leaves a gap or writes the seam twice.

## A surface takes two parameters

These UV coordinates parameterize the geometry. Texture UVs also follow the top-left image origin, where V increases downward. A parameterized surface need not be a single-valued height field. A closed seam does not make a watertight volume.

Where the derivatives exist, `Su` and `Sv` are the partial derivatives of position. The cross product `Su x Sv` is an oriented normal when it is nonzero. Its magnitude is the local area scale. Normalize it only when it is nondegenerate. A singular parameterization can have a zero cross product on a surface that is still geometrically fine.

## Frames, curvature and jets

A jet stores position and derivatives at one parameter value. Fundamental forms, a shape operator, and principal curvatures describe the surface locally. Sampled positions do not replace them. Curvature uses reciprocal world-length units. A Frenet frame needs derivative and curvature assumptions, and it is undefined on some straight or stationary configurations. A rotation-minimizing frame carries orientation along a curve, and that orientation depends on the path.

## Sweeps and offsets approximate

`SweptSurface` and `TubeSurface` use a deterministic fixed-up frame. Their type descriptions call for a rotation-minimizing frame. The evaluated surface can twist relative to that intended frame along the path.

`OffsetSurface` estimates the base normal with central differences, because the base interface does not require analytic derivatives. The header calls that difference exact when the base is differentiable. A finite difference is not an analytic derivative. Near the boundary the samples are clamped, so the difference becomes one-sided. The step, the boundary, and the stability still need numerical evaluation.

A trimmed surface, a subdivision scheme, or a named analytic surface follows the contract in its own source. A type that stores a subdivision or a trim is not a tessellator that handles every singularity and self-intersection.

## Where to look

- [Surface and solid interfaces](../../api/geometry/surfaces-solids.concepts.md)
- [Surface records](../../api/geometry/surfaces.types.md) and [surface bodies](../../api/geometry/surfaces.library.md)
- [Analytic surface records](../../api/geometry/surfaces-shapes.types.md) and [bodies](../../api/geometry/surfaces-shapes.library.md)
- [Differential-geometry records](../../api/geometry/differential-geometry.types.md)
- [UV origin convention](../../../../stdlib/CONVENTIONS.md#uv-origin--top-left-v-increases-downward)

[CGAL's surface and geometry packages](https://doc.cgal.org/latest/Manual/packages.html) separate smooth geometry from meshes, and [PBRT's shape chapters](https://pbr-book.org/4ed/contents) cover parameter derivatives and normals in rendering. A stored differential record here is not automatic differentiation, and it does not carry either library's robustness.
