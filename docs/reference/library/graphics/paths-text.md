# Paths, paint, strokes and text

`Path2D` stores contours and a fill rule. A contour is a connected sequence of segment verbs. The segment sum carries only the controls that case needs: move, line, quadratic, cubic, elliptical arc, or close. An endpoint is relative to the current contour. `Close` returns to the subpath start. A closed-contour flag also means an implicit closing edge.

## Endpoint arcs and fill rules

The arc case is SVG's endpoint elliptical arc: radii, axis rotation, large-arc and sweep flags, and an endpoint. That is not a center plus an angle interval. Convert the geometry. Relabeling the fields is not that conversion.

`NonZero` fills where the winding count is nonzero. `EvenOdd` fills where the crossing count is odd. The same self-intersecting outline can have different interiors under the two rules. Path emptiness helpers count contours. A nonempty contour list is not a positive painted area.

## Paint and stroke are separate

`Paint` selects a solid color, a gradient, or a texture. `FillStyle` adds opacity. Stroke records specify width, joins, caps, alignment, and dashes. A miter limit, an offset tolerance, or a flattening limit is a geometric decision. It is not an implicit conversion. Geometry bounds and painted bounds differ when a thick stroke or a shadow extends past the path.

Flattening a curved path needs an error policy. A parameter record that documents a tolerance does not establish a flattener that achieves it, especially when a segment cap can stop refinement early.

## A character is not a glyph

Font records identify a face and a style. Glyph placements and runs describe a shaping result. One character is not necessarily one glyph, and a glyph run's positions are not recovered from the string length. Direction, baseline, paragraph layout, OpenType features, and text on a path all move the glyphs. The font helper file orders font weights. It does not load a font, shape text, or lay out a paragraph.

## Where to look

- [Path records](../../api/graphics/paths.types.md), [path interface](../../api/graphics/paths.concepts.md), and [path helpers](../../api/graphics/paths.library.md)
- [Vector styling](../../api/graphics/vector-styling.types.md)
- [Text records](../../api/graphics/text.types.md) and [font bodies](../../api/graphics/text-fonts.library.md)

[SVG 2 paths](https://www.w3.org/TR/SVG2/paths.html) define the path verbs and the elliptical-arc parameters, and [CSS Fonts](https://www.w3.org/TR/css-fonts-4/) defines related weight, style, and feature concepts. These records are Plato's own descriptions. They are not an SVG parser or a browser text-layout engine.
