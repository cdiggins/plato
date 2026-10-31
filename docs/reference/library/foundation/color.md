# Color and alpha

`Color` is linear-light RGBA with straight, unpremultiplied alpha.

## Compute in linear light

The [global conventions](../../../../stdlib/CONVENTIONS.md) are the authority for that representation. Components are nominally in `[0,1]`. HDR or wide-gamut workflows may use values outside that range. An arbitrary constructor does not establish a gamut or clamp a channel.

`Color8` is the byte-oriented interchange form, typically sRGB encoded. "Typically" is not enough metadata to decode every file. Establish the encoding at the boundary. Decode sRGB channels before arithmetic or interpolation in `Color`, and encode when writing a display-oriented format. A byte value of 128 does not denote half of full linear intensity.

## Interpolation and compositing

Component-wise linear interpolation blends stored channels. Compositing answers how one partly transparent layer covers another. With straight colors `Cs,Cd` and alphas `as,ad`, source-over has alpha `ao=as+ad*(1-as)`. Its premultiplied RGB is `Cs*as + Cd*ad*(1-as)`. Returning straight RGB additionally divides by `ao` when positive. This mathematical example explains the representation. It does not assert that every compositing operator has a library body.

Do not store premultiplied RGB in a `Color` and then treat it as straight. Transparent RGB may contain meaningful data even when alpha is zero. Later interpolation can expose it.

The graphics HSV conversion bodies operate on linear RGB and use `Angle` for hue. HSV chosen from a conventional encoded-sRGB picker therefore cannot be copied blindly. Conversion back from `ColorHSV` constructs opaque color. Alpha does not survive as part of HSV, because the triple has no alpha channel.

[CSS Color 4](https://www.w3.org/TR/css-color-4/) separates encoded sRGB from linear sRGB. Plato's linear HSV convention and straight component interpolation are its own API choices, not a claim of CSS interpolation compatibility.

## Where to look

- [Color types](../../api/foundation/color.types.md) and [color bodies](../../api/foundation/color.library.md)
- [Graphics color spaces](../../api/graphics/color-spaces.types.md) and [conversion bodies](../../api/graphics/color-spaces.library.md)
