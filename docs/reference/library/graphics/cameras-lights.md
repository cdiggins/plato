# Cameras and lights

Read [camera interface](../../api/graphics/cameras.concepts.md), [camera records](../../api/graphics/cameras.types.md), [camera bodies](../../api/graphics/cameras.library.md), [light interface](../../api/graphics/lights.concepts.md), [light records](../../api/graphics/lights.types.md) and [light bodies](../../api/graphics/lights.library.md).

## Distinguish world and view frames

World coordinates are right-handed with +Z up. Camera-local view coordinates look down -Z, with +Y up and +X right. A camera pose places that local frame in the world. The [global conventions](../../../../stdlib/CONVENTIONS.md#view-space--camera-local-forward-is--z) specify the relationship; choosing a different world up axis because a textbook camera uses +Y would change the model.

`ICamera` supplies pose, positive near/far distances with `Near < Far`, and width/height aspect ratio. `ContainsDepth` tests positive scene distance against that range; it is not a test of arbitrary signed view-space Z or a complete frustum-membership predicate. `EyePosition` reads pose translation. A large far/near ratio is a depth precision concern, not by itself a proof of z-fighting.

Perspective, orthographic, physical, fisheye, equirectangular and off-axis records describe different projections. A field of view is an `Angle`; aspect ratio is unitless. Physical optics carry lengths whose declared units matter. Their presence does not prove every projection matrix, distortion model or ray generator is implemented in the derived camera helper file.

## Light descriptions are not a renderer

Directional, point, spot, area and environment lights represent different spatial emission models. Read each record for parameter units and attenuation. The common `ILightSource` exposes only the shadow-casting flag; its generic helpers are shadow-status queries. It supplies no shared radiance evaluation or shadow algorithm.

Shadow settings, IES references, probes, masks and volumetric controls are data for a consumer. A file reference does not load itself; a flag does not allocate a shadow map. Scientific use requires a renderer that interprets these fields consistently.

## Related work

[PBRT's camera and light chapters](https://pbr-book.org/4ed/contents) separate camera sampling, emission and light transport. Plato provides overlapping vocabulary, but the small camera/light helper libraries are not a full optical or transport implementation. [glTF 2.0](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) is a useful scene interchange comparison; import/export must explicitly reconcile coordinate frames, projection assumptions and units.
