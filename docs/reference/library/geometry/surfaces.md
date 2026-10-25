# Surfaces and differential geometry

Read [surface and solid interfaces](../../api/geometry/surfaces-solids.concepts.md), [surface records](../../api/geometry/surfaces.types.md), [surface bodies](../../api/geometry/surfaces.library.md), [analytic surface records](../../api/geometry/surfaces-shapes.types.md) with [bodies](../../api/geometry/surfaces-shapes.library.md), and [differential-geometry records](../../api/geometry/differential-geometry.types.md).

## Two parameters create a surface

A parametric surface maps a UV coordinate to a 3D point. The preferred domain is usually the unit square. U is the first/profile/column direction and V the second/sweep/row direction in the surface evaluator convention. `ClosedU` and `ClosedV` state seam connectivity; a mesh sampler must use them to avoid an unintended gap or duplicate seam strip.

The UV labels here parameterize geometry. Texture UVs additionally use the [top-left origin convention](../../../../stdlib/CONVENTIONS.md#uv-origin--top-left-v-increases-downward). A parameterized surface need not be a single-valued height field, and a closed seam does not establish a closed watertight volume.

Where derivatives exist, let `Su` and `Sv` be partial derivatives of position. The cross product `Su x Sv` supplies an oriented normal when nonzero. Its magnitude is the local area scale; normalize only if nondegenerate. A singular parameterization can have zero cross product even when the geometric surface itself is meaningful.

## Frames, curvature and jets

A jet stores position and derivatives at a parameter value. Fundamental forms, a shape operator and principal curvatures describe local differential geometry; they are not interchangeable with sampled positions alone. Curvature uses reciprocal world-length units. A Frenet frame needs derivative and curvature assumptions; it is undefined at some straight or stationary configurations. A rotation-minimizing frame instead transports orientation along a curve and requires path-dependent information.

## Approximation must remain visible

`SweptSurface` and `TubeSurface` currently use a deterministic fixed-up frame, while their intended description calls for a rotation-minimizing frame. The result may twist differently along a path. `OffsetSurface` estimates the base normal with central differences, because the base interface does not require analytic derivatives. A differentiable base does **not** make a finite difference an exact derivative; the source header's “exact” wording is too strong mathematically. Its step, boundary behavior and stability need numerical evaluation.

Trimmed surfaces, subdivision schemes and elaborate named analytic surfaces each require the concrete source contract. A type carrying a subdivision or trim description is not a proof of a robust tessellator for every singularity and self-intersection.

## Related work

[CGAL's surface and geometry packages](https://doc.cgal.org/latest/Manual/packages.html) distinguish smooth geometry from meshes. [PBRT's shape chapters](https://pbr-book.org/4ed/contents) explain parameter derivatives and normals in rendering. Plato's stored differential records do not by themselves provide automatic differentiation or the robustness of either implementation.
