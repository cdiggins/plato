# Plato WebGL demos (TypeScript / Three.js)

Four browser demos over one generated library: **polyhedra**, **polygons**,
**CSG** and **deformers**. Every solid, ring, boolean and warp is computed by
members that `Plato.TypeScriptWriter` emitted from the Plato sources in
[`stdlib/`](../../stdlib) — the demo code builds inputs, reads results, and
repacks them into Three.js buffers. It does not re-derive any geometry.

```bat
npm install
npm run smoke     # generated members evaluate to the values Plato pins down
npm run dev       # http://localhost:5175
npm run typecheck
npm run build
```

## Layout

```
index.html            Landing page linking the four demos
polyhedra.html        One page per demo, one entry each
polygons.html
csg.html
deformers.html
src/
  plato/
    plato.g.ts        GENERATED — do not edit; npm run gen:plato
    array-ext.ts      Hand-written prelude; see "The prelude" below
  shared/
    demo.ts           Scene / Control / Demo contract
    ui.ts             Sidebar, scene list, parameter controls
    viewer.ts         Three.js stage: orbit camera, lights, grid
    mesh.ts           Plato geometry -> THREE.BufferGeometry
    style.css
  demos/
    polyhedra.ts      One scene catalog per page
    polygons.ts
    csg.ts
    deformers.ts
scripts/
  smoke.mts           Value gate over the generated members
```

## The prelude

`src/plato/array-ext.ts` is hand-written support code, not generated. The
TypeScript writer emits calls to Plato library functions in extension-method
position but does not always emit the functions themselves, so the mesh,
polygon and CSG paths reach for members that are not there. The prelude fills
exactly those holes, each body mirroring the `.plato` source named above it:

| Gap | What the prelude supplies |
|---|---|
| `Array<T>` libraries are never emitted (`IArray<T>` declares only At/Count/Map/Reduce) | `Concatenate`, `SubArray`, `FlatMap`, `Append`, `AtModulo`, `First`/`Last`, aggregates |
| `meshes-polygon` / `geometry` / `polygons` helpers used as array extensions | `PolygonMeshOfFaces`, `PolygonMeshOfVertexNumbers`, `FromRows`, `ShoelaceArea`, `ChainLength`, `PolygonAreaCentroid`, `PolygonContainsPoint`, `VectorArea`, `PlanarPolygonCentroid`, `PlanarPolygonContains` |
| Overloads are dropped ("Skipped: overload or duplicate member") | Runtime dispatch for `Transform(Quaternion)` on `Vector3D` / `Point3D` |
| `Integer` division emitted as float division | Truncating `PolygonMesh3D.FaceCorner`, which `Truncate` indexes with `k / 2` |
| Sum types are C#-only in v1 (CHK320) | A tagged `PlaneRelation3D` with the `IsFront` / `IsBack` / `IsCoplanar` members the CSG bodies call |
| Record returns written as tuple literals | Slot names `Hit` / `Point` / `Parameter` on `Tuple3`, for `PlaneHit3D` |
| `IArithmetic` obligations on the native number mapping | `Zero`, `One`, `Half`, and the `Number.Pi` / `Epsilon` / `MinValue` / `MaxValue` constants |

Each entry is a writer gap, not a design choice — the prelude shrinks as the
writer grows. `npm run smoke` is what tells you which side changed: it checks
values the Plato source fixes (a truncated icosahedron has 32 faces, a unit
square has area 4, `Taper3D` scales by `1 + rate * t`), so a break points at the
writer or the prelude rather than at the demo.

## Regenerating

```bat
npm run gen:plato
```

Runs `Plato.CLI --typescript` over `stdlib/foundation`, `stdlib/geometry` and
`stdlib/graphics`, then stamps `@ts-nocheck` on the output. Re-run `npm run
smoke` afterwards.

## See also

- [SDF demo](../typescript/sdf/README.md) — the same generated library, used for
  signed distance fields
- [TypeScript geometry samples](../typescript/geometry-samples/README.md) —
  curated demo subset rather than the full stdlib
- [Plato.TypeScriptWriter](../../writers/Plato.TypeScriptWriter/README.md)
