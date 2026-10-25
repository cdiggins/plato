# Paths, paint, strokes and text

Read [path records](../../api/graphics/paths.types.md), [path interface](../../api/graphics/paths.concepts.md), [path helpers](../../api/graphics/paths.library.md), [vector styling](../../api/graphics/vector-styling.types.md), [text records](../../api/graphics/text.types.md), and [font bodies](../../api/graphics/text-fonts.library.md).

## A path is geometry plus an interior rule

`Path2D` stores contours and a fill rule. A contour is a connected sequence of segment verbs. The segment sum carries only the controls appropriate to its case: move, line, quadratic, cubic, elliptical arc, or close. Endpoints are interpreted relative to the current contour state; `Close` returns to the subpath start. A closed-contour flag also denotes an implicit closing edge.

The arc case follows SVG's endpoint-style elliptical arc: radii, axis rotation, large-arc and sweep flags, and endpoint. This differs from a center-plus-angle interval arc. A caller must perform a geometric conversion rather than relabel fields.

`NonZero` fills where winding count is nonzero; `EvenOdd` fills where crossing count is odd. These rules can assign different interiors to the same self-intersecting outline. Path emptiness helpers count contours; a nonempty contour list is not proof of positive painted area.

## Appearance is separate from the outline

`Paint` selects solid, gradient or texture appearance. `FillStyle` adds opacity; stroke records specify width, joins, caps, alignment and dashes. A miter limit, offset tolerance or flattening limit is a geometric decision, not an implicit conversion. Geometry bounds and painted bounds differ when a thick stroke or shadow extends outside the path.

Flattening a curved path requires an error policy. A parameter record documenting tolerance does not establish a flattening implementation that achieves it, particularly when a segment cap can stop refinement early.

## Text needs shaping, not just characters

Font records identify a face and style; glyph placements/runs describe a shaping result. A character is not necessarily one glyph, and a glyph run's positions are not recovered simply from string length. Direction, baseline, paragraph layout, OpenType features and text-on-path all affect placement. The concrete font helper file orders font weights; it does not implement a font loader, shaper or layout engine.

## Related work

[SVG 2 paths](https://www.w3.org/TR/SVG2/paths.html) directly explain the path-verb and elliptical-arc correspondence. [CSS Fonts](https://www.w3.org/TR/css-fonts-4/) supplies related weight/style/feature concepts. Plato stores its own typed descriptions and result records; these mappings do not claim a full SVG parser or browser-compatible text layout engine.
