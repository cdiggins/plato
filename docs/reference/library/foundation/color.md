# Color and alpha

[Color types](../../api/foundation/color.types.md) and [color bodies](../../api/foundation/color.library.md) define the foundation representation. [Graphics color spaces](../../api/graphics/color-spaces.types.md) and [conversion bodies](../../api/graphics/color-spaces.library.md) add color-science vocabulary.

## Compute in linear light

`Color` is linear-light RGBA with straight, unpremultiplied alpha. Components are nominally in `[0,1]`; HDR or wide-gamut workflows may use values outside that range. An arbitrary constructor does not establish a gamut or clamp a channel.

`Color8` is the byte-oriented interchange form, typically sRGB encoded. “Typically” is not enough metadata to decode every file: establish the encoding at the boundary. Decode sRGB channels before arithmetic or interpolation in `Color`; encode when writing a display-oriented format. A byte value of 128 does not denote half of full linear intensity.

## Interpolation is not compositing

Component-wise linear interpolation blends stored channels. Compositing answers how one partly transparent layer covers another. With straight colors `Cs,Cd` and alphas `as,ad`, source-over has alpha `ao=as+ad*(1-as)`. Its premultiplied RGB is `Cs*as + Cd*ad*(1-as)`; returning straight RGB additionally divides by `ao` when positive. This mathematical example explains the representation; it does not assert every compositing operator has a library body.

Do not store premultiplied RGB in a `Color` and then treat it as straight. Transparent RGB may contain meaningful data even when alpha is zero; later interpolation can expose it.

The graphics HSV conversion bodies operate on linear RGB and use `Angle` for hue. HSV chosen from a conventional encoded-sRGB picker therefore cannot be copied blindly. Conversion back from `ColorHSV` constructs opaque color; alpha does not survive as part of HSV because the triple has no alpha channel.

## Related work

[CSS Color 4](https://www.w3.org/TR/css-color-4/) separates encoded sRGB, linear sRGB and other spaces and specifies interpolation behavior. Plato shares the need to distinguish encoding from color coordinates, but its linear HSV convention and straight component interpolation are specific API choices, not a claim of CSS interpolation compatibility.
