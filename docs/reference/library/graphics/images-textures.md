# Images, textures and color spaces

`IImage` requires pixel dimensions and deliberately has no generic pixel access. Byte, floating-point, grayscale, bit, and palette-index images store different representations. Unless a concrete type says otherwise, pixels are row-major from the top-left. For width `W`, column and row `(column, row)` flatten to `row*W + column`. Establish a nonnegative in-range coordinate first. Aspect ratio needs a nonzero height.

## Where a pixel sits

UV `(0, 0)` is the top-left, and V increases downward. A bottom-left origin is an explicit choice. A UV coordinate is a normalized texture position. A pixel coordinate is an integer address. Converting one to the other needs a sampling convention, because a pixel corner and a pixel center are different positions.

An encoded image stores codec-oriented data, not pixels you can read directly. Mip chains and tiles describe resolution and storage. Declaring them does not install a decoder, a streamer, or automatic mip generation.

## A texture sample is not a decoded image

A procedural texture answers `ColorAt(uv)`. Opacity helpers read alpha. A checker texture uses floor before the parity test, so a negative coordinate keeps the same cell width. Truncation toward zero would change the cell size across the origin. Gradient and noise textures have their own bodies, with their own interpolation and domain choices.

Wrap, filtering, projection, and UV-transform records tell a consumer how to sample. A `TextureBinding` connects texture data with a material meaning. Using an encoded color texture as linear numeric data changes the analysis. The [color guide](../foundation/color.md) is the computation contract.

Color-space declarations name primaries, white points, transfer functions, and conversion intentions. Only a concrete conversion body is an implemented path. Declaring Lab, XYZ, spectral, or CMYK records does not implement every pair of conversions. Convolution, blur, morphology, resampling, blend, and dithering parameter records are likewise not executable image-processing algorithms.

## Where to look

- [Image types](../../api/graphics/images.types.md), [image interface](../../api/graphics/images.concepts.md), and [image bodies](../../api/graphics/images.library.md)
- [Texture types](../../api/graphics/texturing.types.md), [texture interface](../../api/graphics/texturing.concepts.md), and [texture bodies](../../api/graphics/texturing.library.md)
- [Image-processing records](../../api/graphics/image-processing.types.md)
- [Color-space records](../../api/graphics/color-spaces.types.md) and [bodies](../../api/graphics/color-spaces.library.md)
- [Color](../foundation/color.md)

[CSS Color 4](https://www.w3.org/TR/css-color-4/) separates color spaces from transfer functions. The linear-light `Color` contract here is separate from those declarations. [Compositing and Blending](https://www.w3.org/TR/compositing-1/) specifies operator mathematics. A Plato selector does not run those formulas until a consumer implements them.
