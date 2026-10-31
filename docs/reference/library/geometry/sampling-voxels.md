# Sampling, point clouds and voxels

A sampled field stores values on grid nodes. A voxel grid stores values in cells. Along one axis, `n` cells have `n + 1` node samples and `n` cell values. Mixing the two conventions shifts the samples and changes how many edges you count.

## A node grid is not a cell grid

For a voxel grid with origin `O` and positive cell size `s`, cell `(i, j, k)` is the half-open box from `O + s*(i, j, k)` to `O + s*(i + 1, j + 1, k + 1)`. The origin is the minimum corner of the first cell, not its center. A cell center is half a cell in from that corner. Occupancy is Boolean. A density value means whatever the producer stored. A level-set grid stores signed distances under its own narrow-band and background convention.

Sampled scalar fields evaluate with bilinear or trilinear interpolation between nodes. The reference bodies clamp an out-of-range node index to the edge. Other interpolation and boundary names in the vocabulary do not mean the default evaluator changed. An empty extent, or a value count that does not match the grid, breaks the construction assumptions.

## Sampling approximates the original

Sampling a curve or a surface picks finitely many parameters. Interpolation between those samples approximates the original function. Sample records keep the parameter arrays. Those arrays have to match the point arrays in length, and their order matters. Adding samples does not give an error bound near a discontinuity, a sharp bend, or a high-frequency feature.

Halton, Hammersley, Sobol, stratified, jittered, Poisson-disk, and blue-noise names are different distribution and spacing goals. Use the concrete generator and its seed contract. A pattern that repeats inside one library version can change when the implementation changes. Deterministic samples are not statistically independent on that account.

## Normals stay stored when positions move

An enriched cloud stores positions in parallel with normals, colors, or intensities. `ToPointCloud` drops those channels on purpose. The deformation bodies map positions and leave normals unchanged.

The header says those unchanged normals stay exact under a rigid mapping. A translation does. A rotation does not: the stored normal is the old vector. A +X normal rotated with the positions around +Z has to change direction, and this body does not rotate it. Recompute or correct normals before treating the result as an oriented cloud.

## Where to look

- [Sample records](../../api/geometry/sampling.types.md) and [sampling bodies](../../api/geometry/sampling.library.md)
- [Point-cloud and voxel interfaces](../../api/geometry/pointclouds-voxels.concepts.md)
- [Cloud records](../../api/geometry/pointclouds.types.md) and [cloud bodies](../../api/geometry/pointclouds.library.md)
- [Voxel records](../../api/geometry/voxels.types.md) and [voxel bodies](../../api/geometry/voxels.library.md)

[NumPy's array reference](https://numpy.org/doc/stable/reference/routines.array-manipulation.html) is a useful comparison for layout, and these grids add an origin and a cell size. [PBRT's sampling chapters](https://pbr-book.org/4ed/contents) explain why the pattern names differ. A familiar name does not state the discrepancy, the blue-noise quality, or the integration error of one generated set.
