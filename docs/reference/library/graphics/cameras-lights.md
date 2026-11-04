# Cameras and lights

World coordinates are right-handed, with +Z up. Camera-local view coordinates look down -Z, with +Y up and +X right. A camera pose places that local frame in the world. The global conventions state the relationship. A textbook camera that uses +Y as world up is a different frame, and adopting it changes the model.

## World up is +Z

`ICamera` returns a pose, positive near and far distances with `Near < Far`, and a width-over-height aspect ratio. `ContainsDepth` tests a positive scene distance against that range. It is not a test of an arbitrary signed view-space Z, and it is not a full frustum test. `EyePosition` reads the pose translation. A large far-to-near ratio is a depth-precision concern. On its own it does not prove z-fighting.

Perspective, orthographic, physical, fisheye, equirectangular, and off-axis records describe different projections. A field of view is an `Angle`. Aspect ratio is unitless. Physical optics carry lengths, and the declared units matter. Those fields do not mean every projection matrix, distortion model, or ray generator has a body in the camera helper file.

## A light record stores emission data

Directional, point, spot, area, and environment lights are different spatial emission models. Read each record for units and attenuation. `ILightSource` exposes the shadow-casting flag, and its generic helpers query that flag. It has no shared radiance evaluation and no shadow algorithm.

Shadow settings, IES references, probes, masks, and volumetric controls are data for a consumer. A file reference does not load a file. A flag does not allocate a shadow map. A scientific use needs a renderer that interprets these fields the same way on every light.

## Where to look

- [Camera interface](../../api/graphics/cameras.concepts.md), [camera records](../../api/graphics/cameras.types.md), and [camera bodies](../../api/graphics/cameras.library.md)
- [Light interface](../../api/graphics/lights.concepts.md), [light records](../../api/graphics/lights.types.md), and [light bodies](../../api/graphics/lights.library.md)
- [View-space convention](../../../../stdlib/CONVENTIONS.md#view-space--camera-local-forward-is--z)

[PBRT's camera and light chapters](https://pbr-book.org/4ed/contents) separate camera sampling, emission, and light transport. The camera and light helpers here are not that optical or transport implementation. [glTF 2.0](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) is a scene-interchange comparison. Import and export have to reconcile frames, projection, and units on purpose.
