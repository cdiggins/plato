# Scenes, materials and render settings

Read [2D scenes](../../api/graphics/scene2d.types.md), [3D scenes](../../api/graphics/scene3d.types.md), [materials](../../api/graphics/materials.types.md), [render settings](../../api/graphics/render.types.md), and [texture bindings](../../api/graphics/texturing.types.md). These are retained descriptions: they store what a host should draw rather than issuing drawing commands.

## Hierarchies and resource pools

A 3D scene has a flat node array and flat resource pools. Typed parent references define a hierarchy; a negative parent denotes a root. Node transforms are relative to parents. Material, mesh, light and camera indices each belong to their own pool, not a universal integer namespace. A light/camera binding sum selects a typed subpool, whose index is interpreted there.

To compute a world placement, compose the node's local transform with its ancestors in the order consistent with [row-vector transforms](../foundation/matrices-transforms.md). A cycle or an out-of-range parent breaks the tree model; the record constructor is not an automatic cycle detector. Layer masks are bitmasks rather than typed element indices.

2D scene content selects paths, text, image references or groups. Groups affect isolation/compositing; clips can use path coverage, node alpha or luminance. A negative clip reference means absence. Avoid assuming an opacity multiply is the same as rendering an isolated group then compositing the result.

## Materials are models with parameters

Metallic/roughness, specular/glossiness, unlit, toon and layered descriptions encode different appearance models. Their texture channels need the declared interpretation and color encoding. Alpha mode, cutoffs, transmission, clearcoat, sheen and subsurface data need host behavior; assigning a field does not implement a BRDF or transport model.

Render settings select tone mapping, anti-aliasing, shadows, fog, bloom and path-tracing controls. They are not a software renderer, GPU pipeline or benchmark guarantee. A path-tracing sample count does not prove convergence, and a frame-statistics record is an observation container rather than a guarantee of frame time.

## Related work

[glTF 2.0](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) also organizes scenes around nodes, resource references, transforms and material data. Plato's records have their own indices, frame conventions and richer selector families; interoperability needs an explicit mapping, including loss when a target format cannot represent a feature. [Compositing and Blending](https://www.w3.org/TR/compositing-1/) explains the distinction between blend operations and isolated compositing groups.
