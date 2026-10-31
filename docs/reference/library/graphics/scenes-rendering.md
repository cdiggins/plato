# Scenes, materials and render settings

A scene record stores what a host should draw. It does not issue drawing commands.

## Hierarchies and resource pools

A 3D scene has a flat node array and flat resource pools. A typed parent reference builds the hierarchy. A negative parent means a root. A node transform is relative to its parent. Material, mesh, light, and camera indices each belong to their own pool. They are not one shared integer namespace. A light or camera binding sum selects a typed subpool, and the index is read there.

A world placement composes the node's local transform with its ancestors, in the order the row-vector transform convention uses. A cycle, or a parent index past the end of the array, breaks the tree. The record constructor does not detect cycles. A layer mask is a bitmask, not a typed element index.

2D scene content selects paths, text, image references, or groups. A group affects isolation and compositing. A clip can use path coverage, node alpha, or luminance. A negative clip reference means there is no clip. Multiplying opacities is not the same operation as rendering an isolated group and then compositing it.

## Materials and render settings are data

Metallic-roughness, specular-glossiness, unlit, toon, and layered descriptions are different appearance models. Their texture channels need the declared interpretation and color encoding. Alpha mode, cutoffs, transmission, clearcoat, sheen, and subsurface data need host behavior. Writing a field does not implement a BRDF or a transport model.

Render settings select tone mapping, anti-aliasing, shadows, fog, bloom, and path-tracing controls. They are not a software renderer, a GPU pipeline, or a benchmark. A path-tracing sample count does not prove convergence. A frame-statistics record stores an observation. It does not guarantee a frame time.

## Where to look

- [2D scenes](../../api/graphics/scene2d.types.md) and [3D scenes](../../api/graphics/scene3d.types.md)
- [Materials](../../api/graphics/materials.types.md)
- [Render settings](../../api/graphics/render.types.md)
- [Texture bindings](../../api/graphics/texturing.types.md)
- [Row-vector transforms](../foundation/matrices-transforms.md)

[glTF 2.0](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) also organizes scenes as nodes, resource references, transforms, and material data. These records use their own indices, frames, and selectors, so an interchange mapping has to be written, including the features a target format cannot store. [Compositing and Blending](https://www.w3.org/TR/compositing-1/) separates blend operations from isolated compositing groups.
