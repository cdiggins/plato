# Sampling, point clouds and voxels

Read [sample records](../../api/geometry/sampling.types.md), [sampling bodies](../../api/geometry/sampling.library.md), [point-cloud/voxel interfaces](../../api/geometry/pointclouds-voxels.concepts.md), [cloud records](../../api/geometry/pointclouds.types.md), [cloud bodies](../../api/geometry/pointclouds.library.md), [voxel records](../../api/geometry/voxels.types.md) and [voxel bodies](../../api/geometry/voxels.library.md).

## Nodes and cells are distinct

A sampled field stores values at grid nodes: `n` cells along an axis require `n+1` node samples. A voxel grid stores cell values: `n` cells have `n` values. Mixing these conventions shifts samples and changes edge counts.

For a voxel grid with origin `O` and positive cell size `s`, cell `(i,j,k)` occupies the half-open box from `O+s*(i,j,k)` to `O+s*(i+1,j+1,k+1)`. The origin is the minimum corner, not the center of the first cell. Cell centers require adding half a cell. Occupancy is Boolean; density values need the producer's interpretation; a level-set grid stores signed distances with its own narrow-band/background convention.

Sampled scalar fields evaluate bilinearly or trilinearly between nodes. Their reference bodies clamp out-of-range node accesses at the edge. Other interpolation/boundary descriptors are vocabulary, not a reason to assume the default evaluator changed. Empty extents and inconsistent value counts violate construction assumptions.

## Sampling changes the represented object

Sampling a curve or surface chooses finite parameters; interpolation between them approximates the original function. Sample records preserve parameter arrays, whose lengths must agree with the point arrays and whose ordering matters. More samples do not prove an error bound near a discontinuity, sharp bend or high-frequency feature.

Halton, Hammersley, Sobol, stratified, jittered, Poisson-disk and blue-noise names refer to different distribution/spacing goals. Use the concrete pattern generator and its seed contract. Reproducibility within a library version does not imply that outputs remain identical after implementation changes, or that deterministic samples are statistically independent.

## Point attributes can become stale

Enriched clouds store parallel positions and normals/colors/intensities. `ToPointCloud` deliberately discards those channels. The deformation bodies map positions and leave normals unchanged. **Known conflict:** the header claims rigid mappings preserve those unchanged normals, but a rigid rotation must rotate normals too. Translation is safe; a rotation of a +X normal around +Z should change its direction. Consumers must correct or recompute normals before treating a transformed oriented cloud as valid.

## Related work

[NumPy's array reference](https://numpy.org/doc/stable/reference/routines.array-manipulation.html) helps compare layout, but Plato adds explicit spatial origin/cell semantics. [PBRT's sampling chapters](https://pbr-book.org/4ed/contents) explain why sampling patterns differ; a familiar pattern name does not establish the discrepancy, blue-noise quality, or integration error of a particular generated set.
