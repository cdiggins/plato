# Procedural noise and cellular lattices

Noise and lattices both build a large spatial pattern from a short parameter list. A noise value is a field sample at a point. A lattice stores nodes, struts, and how those cells repeat in space.

## Noise is a deterministic field

Evaluation hashes integer coordinates with a seed. No mutable generator advances between calls. Equal inputs inside one implementation return equal results. The source leaves the exact hash constants out of its cross-version contract.

`Frequency` scales the input coordinates. Raising it makes features smaller in world space. Value noise blends stored values. Gradient families blend directional contributions. Worley noise measures nearby feature points. Gabor noise uses localized oscillatory kernels. Fractal noise layers octaves and changes frequency and amplitude between them. Turbulence and ridged variants change the distribution of the result. A range quoted for a base family does not transfer to every modifier.

Domain warping evaluates another field at a moved point. Curl noise builds a vector field from a numerical derivative, and that step is part of the implementation. It is not a proof that the floating-point result has zero divergence. A noise gradient or a warped distance does not keep signed-distance bounds on its own.

## Lattices store connectivity and placement

A unit cell stores normalized node coordinates and struts that name pairs of nodes. Tiling places that cell in the world and repeats the connectivity. Shared boundary struts need one emitting-cell rule, or the same physical edge is stored twice. Cell size, repeat extents, and strut thickness have to satisfy the constructor you are calling.

`StrutSdf3D` is distance-like geometry around a list of struts. A graded structure varies the radius through a field. A TPMS family uses a periodic implicit equation. Naming a minimal surface, or evaluating a finite trigonometric field, does not make an arbitrary thickened, sampled, or graded solid an exact minimal surface or an exact signed distance. A network and a sheet keep different regions of that field.

Doubling the world cell size doubles strut spacing. Doubling only a noise frequency halves the feature scale. The two parameters sit on opposite sides of the domain mapping. They are not one "detail" control.

## Where to look

- [Noise types](../../api/geometry/noise.types.md) and [noise bodies](../../api/geometry/noise.library.md)
- [Lattice types](../../api/geometry/lattices.types.md) and [lattice bodies](../../api/geometry/lattices.library.md)

[PBRT's textures and sampling chapters](https://pbr-book.org/4ed/contents) discuss procedural fields, and [CGAL's geometry packages](https://doc.cgal.org/latest/Manual/packages.html) separate implicit geometry from explicit connectivity. Building one of these fields is not a manufacturing check, a strength check, or a certificate that a surface is minimal.
