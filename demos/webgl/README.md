# Plato WebGL demos (TypeScript / Three.js)

Browser demos over one generated library: **polyhedra**, **polygons**, **CSG**,
**deformers**, **parametric curves**, **parametric surfaces**, **noise**,
**colour spaces**, **transforms**, **marching cubes**, **voxels**,
**lattices**, **blue-noise sampling**, **remeshing**, **finite elements**,
**rigid bodies** and **cloth**. Every solid, ring, boolean, warp, curve,
surface, field, colour, isosurface, unit cell, point set, remesh, stress value
and simulation step is computed by members that `Plato.TypeScriptWriter` emitted
from the Plato sources in [`stdlib/`](../../stdlib) — the demo code builds
inputs, reads results, and repacks them into Three.js buffers. It does not
re-derive any geometry.

The last six pages are also the demonstration that the library reaches past
static geometry: `lattices`, `sampling` and `remeshing` extend `stdlib/geometry`,
while `fea`, `rigidbody` and `cloth` drive `stdlib/future` — the tier that is
neither linted nor converted to C#, and which joined this app's codegen recipe
so those pages would have something to run.

Where a generated member is missing, broken or absent from the library, the page
says so in its status line by name — `UNAVAILABLE (…)` — rather than substituting
an answer of its own. Those readings are live: a page starts printing values the
day the member starts working, with no edit. See
[`tracker/issues/plato-419.md`](../../tracker/issues/plato-419.md) (writer
defects) and [`plato-420.md`](../../tracker/issues/plato-420.md) (library tiers
that declare a vocabulary and implement nothing over it).

```bat
npm install
npm run smoke     # generated members evaluate to the values Plato pins down
npm run scenes    # every scene of every page builds, off the page
npm run probe     # does a member evaluate, throw, or return NaN?
npm run dev       # http://localhost:5175
npm run typecheck
npm run build
```

## Layout

```
index.html            Landing page linking every demo
polyhedra.html        One page per demo, one entry each
polygons.html         (csg, deformers, curves, surfaces, noise, colors,
                       transforms, marching, voxels, lattices, sampling,
                       remeshing, fea, rigidbody, cloth)
src/
  plato/
    plato.g.ts        GENERATED — do not edit; npm run gen:plato
    array-ext.ts      Hand-written prelude; see "The prelude" below
  shared/
    demo.ts           Scene / Control / Demo contract
    ui.ts             Sidebar, scene list, and the Gratify control panel
    widgets.ts        The Gratify parts: slider, toggle, segment, colour, button
    viewer.ts         Three.js stage: orbit camera, lights, grid
    mesh.ts           Plato geometry -> THREE.BufferGeometry
    raster.ts         Plato fields -> a textured quad, for the field pages
    style.css
  demos/
    polyhedra.ts      One scene catalog per page
    …
scripts/
  smoke.mts           Value gate over the generated members
  scenes.mts          Builds every scene of every page, headless
  probe.mts           Triage: ok / FAIL / NaN, per member
vendor/
  gratify/            The Gratify UI framework, vendored ESM + a typing shim
```

Adding a page or working on one: [`COORDINATION.md`](COORDINATION.md) has the
file-ownership table, the control contract and the gates.

## The controls

The sidebar is a [Gratify](https://github.com/ara3d/gratify) app on a canvas —
an Elm-shaped `(init, update, view)` triple whose `update` hands the shell a new
parameter bag. A demo never touches Gratify: it declares `Control` values and
reads numbers back out of `params`. Four kinds — `slider`, `toggle`, `select`
(segmented) and `color` (a saturation/value pad over a hue strip, which writes
back `<key>H`, `<key>S`, `<key>V`) — plus a free **Reset parameters** button.
A drag emits an intent per pointer event; the shell coalesces them so `build`
runs at most once per animation frame.

## The gates

| Command | What it proves |
|---|---|
| `npm run typecheck` | the app compiles |
| `npm run smoke` | the generated members still return the values the Plato source pins down |
| `npm run scenes` | every scene on every page builds, off the page — it imports each demo module, calls each scene's `build` at its defaults, and fails on a throw or an empty result. Prints vertices, milliseconds and the status line per scene. A simulation scene (one declaring `tick`) is also stepped for a short run, and fails if any position went non-finite — a solver that diverges throws nothing and draws nothing, so nothing else would catch it |
| `npm run probe` | not a gate: the triage tool. Calls each candidate member and reports `ok`, `FAIL <message>` or `NaN` |

`scenes` is the one that catches what typecheck cannot — a member that throws
only under real inputs, a scene that quietly builds nothing, a build that got
slow. `probe`'s NaN column is the one that matters most, because the writer
defects that survive longest are the silent ones.

## The prelude

`src/plato/array-ext.ts` is hand-written support code, not generated. The
TypeScript writer emits calls to Plato library functions in extension-method
position but does not always emit the functions themselves, so the mesh,
polygon, curve, noise, transform and voxel paths reach for members that are not
there. The prelude fills exactly those holes, each body mirroring the `.plato`
source named above it:

| Gap | What the prelude supplies |
|---|---|
| `Array<T>` libraries are never emitted (`IArray<T>` declares only At/Count/Map/Reduce) | `Concatenate`, `SubArray`, `FlatMap`, `Append`, `AtModulo`, `First`/`Last`, aggregates, `DeCasteljau`, `AtWrapped` |
| `meshes-polygon` / `geometry` / `polygons` helpers used as array extensions | `PolygonMeshOfFaces`, `PolygonMeshOfVertexNumbers`, `FromRows`, `ShoelaceArea`, `ChainLength`, `PolygonAreaCentroid`, `PolygonContainsPoint`, `VectorArea`, `PlanarPolygonCentroid`, `PlanarPolygonContains` |
| The ear-clipping kernel is `Array`-first and wholly unemitted | `TriangulateRings` and the passes under it, so every `Triangulate` obligation works |
| The marching-cubes kernel is `Array`-first in the same way | `MarchingCubesCell`, `MarchingCubesTriangleCount` |
| `Array3D<T>` has no runtime at all (`new` on a type-only interface) | The 3D array the voxel and sampled-field grids are built from |
| Overloads are dropped ("Skipped: overload or duplicate member") | Runtime dispatch for `Transform(Quaternion)`, `Transform(Rotation2D)`, the scalar `NumberN.Multiply`, and the commuted `Multiply(Number, IScalable)` |
| Extra arguments are dropped silently at the call site | The three-index `LatticeHash`, without which spatial noise is flat in z |
| `Integer` division emitted as float division | Truncating `FaceCorner`, the marching-cubes corner offsets, `MakeArray3D`, `WorleyNeighbour` |
| Sum types are C#-only in v1 (CHK320) | Tagged `PlaneRelation3D`, `NoiseBasis`, `WorleyDistance`, `WorleyFeature`, and the library functions that dispatch on them |
| Record returns written as tuple literals | Slot names `Hit` / `Point` / `Parameter` on `Tuple3`, for `PlaneHit3D` |
| `IArithmetic` obligations on the native number mapping | `Zero`, `One`, `Half`, and the `Number.Pi` / `Epsilon` / `MinValue` / `MaxValue` constants |

Each entry is a writer gap, not a design choice — the prelude shrinks as the
writer grows, and `tracker/issues/plato-419.md` is where they are catalogued.
`npm run smoke` is what tells you which side changed: it checks values the Plato
source fixes (a truncated icosahedron has 32 faces, a unit square has area 4, a
de Casteljau evaluation at t = 0 is the first control point, a marched unit
sphere puts every vertex on the unit radius), so a break points at the writer or
the prelude rather than at the demo.

## Regenerating

```bat
npm run gen:plato
```

Runs `Plato.CLI --typescript` over `stdlib/foundation`, `stdlib/geometry`,
`stdlib/graphics` and `stdlib/future`, then stamps `@ts-nocheck` on the output.
Re-run `npm run smoke` and `npm run scenes` afterwards.

`future` is in the recipe because the simulation and analysis pages have nowhere
else to get a library from. It is the tier the repo does not lint and does not
convert to C# (see [`AGENTS.md`](../../AGENTS.md)), so it is held only to parsing
and type-checking — treat a member out of it as less proven than one out of
`geometry`, and keep the `UNAVAILABLE (…)` reporting honest on those pages.

## See also

- [SDF demo](../typescript/sdf/README.md) — the same generated library, used for
  signed distance fields
- [GLSL demo studio](../glsl/index.html) — Plato compiled to GLSL, with a
  Gratify panel over the shader uniforms
- [TypeScript geometry samples](../typescript/geometry-samples/README.md) —
  curated demo subset rather than the full stdlib
- [Plato.TypeScriptWriter](../../writers/Plato.TypeScriptWriter/README.md)
