# Images, textures and color spaces

Read [image types](../../api/graphics/images.types.md), [image interface](../../api/graphics/images.concepts.md), [image bodies](../../api/graphics/images.library.md), [texture types](../../api/graphics/texturing.types.md), [texture interface](../../api/graphics/texturing.concepts.md), [texture bodies](../../api/graphics/texturing.library.md), [image-processing records](../../api/graphics/image-processing.types.md), and [color-space records](../../api/graphics/color-spaces.types.md) with [bodies](../../api/graphics/color-spaces.library.md).

## Storage and interpretation

`IImage` requires only pixel dimensions. It deliberately excludes generic pixel access: byte, floating, grayscale, bit and palette-index images have different representations. Unless a concrete type says otherwise, pixel storage is row-major from the top-left. For width `W`, `(column,row)` flattens to `row*W+column`; establish nonnegative in-range coordinates first. Aspect ratio needs nonzero height.

UV `(0,0)` is top-left and V increases downward. Bottom-left origin is explicit. A UV coordinate describes normalized texture space; a pixel coordinate is an integer address. Conversion needs a sampling convention: pixel corners and centers are different positions.

An encoded image stores codec-oriented data rather than directly readable pixels. Mip chains and tiles describe resolution/storage organization; their declarations do not install decoders, streamers or automatic mip generation.

## Textures are sampled appearance

A procedural texture answers `ColorAt(uv)`; opacity helpers read alpha. A checker texture uses floor before parity selection so negative coordinates keep consistent cell widths. Truncating toward zero would create a discontinuity in cell size across the origin. Gradient and noise textures have concrete bodies with their own interpolation/domain choices.

Wrap, filtering, projection and UV-transform records control how a consumer samples. A `TextureBinding` connects texture data with material meaning; using an encoded color texture as linear numeric data can silently change an analysis. See [color](../foundation/color.md).

Color-space declarations name primaries, white points, transfer functions and conversion intentions. Only concrete conversion bodies establish implemented paths; declaring Lab, XYZ, spectral or CMYK records does not establish all pairwise conversions. Similarly, convolution, blur, morphology, resampling, blend and dithering parameter records are not evidence of executable image-processing algorithms.

## Related work

[CSS Color 4](https://www.w3.org/TR/css-color-4/) distinguishes color spaces and transfer functions. Plato's declarations provide related vocabulary, with a separate linear-light `Color` computation contract. [Compositing and Blending](https://www.w3.org/TR/compositing-1/) specifies operator mathematics; corresponding Plato selectors need a consumer implementation before those formulas execute.
