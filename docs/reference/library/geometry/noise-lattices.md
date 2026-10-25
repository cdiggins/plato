# Procedural noise and cellular lattices

Read [noise types](../../api/geometry/noise.types.md), [noise bodies](../../api/geometry/noise.library.md), [lattice types](../../api/geometry/lattices.types.md) and [lattice bodies](../../api/geometry/lattices.library.md). Both families build large spatial structure from compact parameters, but their meanings differ.

## Noise is a deterministic field

A noise value is evaluated at a point. The reference lattice hashing combines integer coordinates with a seed; no mutable generator state advances during evaluation. Equal inputs within one implementation select equal results. The source explicitly excludes exact hash constants from its cross-version contract.

Frequency scales input coordinates: increasing it makes features smaller in world space. Basis families differ: value noise blends values, gradient families blend directional contributions, Worley forms measure nearby feature points, and Gabor forms use localized oscillatory kernels. Fractal combinations layer octaves with changing frequency and amplitude; turbulence or ridged transformations change the resulting distribution. Do not infer one common exact value range for every modifier merely from a base-family range.

Domain warping changes the point where another field is sampled. Curl noise derives a vector-valued flow interpretation; its numerical derivative construction and step are part of the implementation, not an automatic proof of zero divergence in floating point. Noise gradients and warped distance values do not automatically retain signed-distance bounds.

## Lattices store connectivity and placement

A unit cell contains normalized node coordinates and struts naming node pairs. Tiling supplies world placement and repeated connectivity; shared boundary struts need a consistent emitting-cell rule so the same physical edge is not duplicated. Cell size, repeat extents and strut thickness must be meaningful for the constructor's contract.

`StrutSdf3D` describes distance-like geometry around a strut list. Graded structures vary radius through a field. TPMS families use periodic implicit equations; a named minimal-surface family and a finite trigonometric field do not prove that an arbitrary thickened, sampled or graded construction is an exact minimal surface or exact SDF. A network and a sheet choose different regions around the field.

Example: doubling the world cell size doubles strut spacing. Doubling only a noise frequency halves its feature scale. These parameters act on opposite sides of a domain mapping; do not treat both as a generic “detail” knob.

## Related work

[PBRT's textures and sampling chapters](https://pbr-book.org/4ed/contents) offer related procedural-field ideas. [CGAL's geometry packages](https://doc.cgal.org/latest/Manual/packages.html) provide a contrast between implicit geometry and explicit connectivity. Plato's field construction is not a claim of manufacturing validation, structural strength or certified minimal-surface accuracy.
