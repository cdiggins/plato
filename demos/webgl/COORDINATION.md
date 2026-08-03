# WebGL demo studio — agent coordination

Eleven demos share one Vite app. The shell, the generated library, the prelude
and the Gratify control panel are done; each demo is one file.

## File ownership

| Agent | May write | Must not touch |
|---|---|---|
| polyhedra | `src/demos/polyhedra.ts` | anything else |
| polygons | `src/demos/polygons.ts` | anything else |
| csg | `src/demos/csg.ts` | anything else |
| deformers | `src/demos/deformers.ts` | anything else |
| curves | `src/demos/curves.ts` | anything else |
| surfaces | `src/demos/surfaces.ts` | anything else |
| noise | `src/demos/noise.ts` | anything else |
| colors | `src/demos/colors.ts` | anything else |
| transforms | `src/demos/transforms.ts` | anything else |
| marching | `src/demos/marching.ts` | anything else |
| voxels | `src/demos/voxels.ts` | anything else |
| prelude | `src/plato/array-ext.ts`, `scripts/smoke.mts`, `scripts/probe.mts` | the demo files |

`src/shared/**`, `src/plato/plato.g.ts`, the HTML pages, `vite.config.ts`,
`package.json` and `README.md` are shared: if one of them is genuinely missing
something you need, say so in your report rather than editing it.
`demos/typescript/sdf/**`, `demos/glsl/**` and `writers/Plato.TypeScriptWriter/**`
belong to other tracks — hands off.

## What a demo file does

Export nothing but `demo`; call `mountDemo(demo)` at the end. The contract is in
[`src/shared/demo.ts`](src/shared/demo.ts):

```ts
const demo: Demo = {
  title: 'Polyhedra',
  subtitle: 'polyhedra.library.plato',
  scenes: [
    {
      id: 'truncated-icosahedron',
      title: 'Truncated icosahedron',
      description: 'One sentence on what the Plato source does.',
      plato: ['PolygonMesh3D.Icosahedron', 'PolygonMesh3D.Truncate'],
      controls: [{ key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 }],
      build: params => /* THREE.Object3D */,
      status: params => 'a line of live numbers',
    },
  ],
};
mountDemo(demo);
```

`build` is called on selection and on every parameter change and must be pure —
the viewer disposes the previous object. `status` runs right after it.

## Controls

The sidebar is a [Gratify](https://github.com/ara3d/gratify) app on a canvas
(`src/shared/widgets.ts`), but a demo never touches Gratify: it declares
`Control` values and reads numbers back out of `params`.

| `kind` | Reads back as | Notes |
|---|---|---|
| `slider` | `params[key]` | `min`, `max`, `step`, `def`; the panel snaps to `step` |
| `toggle` | `params[key]`, 0 or 1 | `def` |
| `select` | `params[key]`, the chosen index | `options: string[]`, `def`; cells wrap 2- or 3-across, so keep labels short |
| `color` | `params[key + 'H' \| 'S' \| 'V']`, each 0..1 | `colorDef: [h, s, v]`; a saturation/value pad over a hue strip. `def` is ignored |

Every scene gets a **Reset parameters** button for free. A drag emits many
events; the shell coalesces them, so `build` runs at most once per frame.

## Rules

- Geometry, colour and field values come from the **generated members** in
  `src/plato/plato.g.ts`. Building inputs and repacking outputs is demo work;
  recomputing a formula that the stdlib already defines is not. **If a member
  you need throws, report it — do not silently hand-roll it.** The house pattern
  is `reading()` in [`src/demos/polygons.ts`](src/demos/polygons.ts): the status
  line keeps the member's name and says `UNAVAILABLE (…)`.
  - `X is not a function` usually means the prelude is missing an `Array<T>` or
    overload body, which is the **prelude agent's** file, not yours. Report it
    with the member name and the failing call.
  - A member that returns `NaN` is the same kind of finding — say so, do not
    paper over it.
- Use the helpers in [`src/shared/mesh.ts`](src/shared/mesh.ts) —
  `polygonMeshGeometry`, `polygonSoupGeometry`, `polygonMeshEdges`,
  `triangleArrayGeometry`, `triangleMeshGeometry`, `polylineGeometry`,
  `parametricGeometry`, `meshToSoup`, `toArray`, `fromArray` — and
  [`src/shared/raster.ts`](src/shared/raster.ts) (`rasterPlane`, `rasterStrip`)
  for the demos whose subject is a field rather than a solid. Colours and the
  shared materials are in [`src/shared/viewer.ts`](src/shared/viewer.ts).
- Import `../plato/array-ext.js` transitively via `../shared/mesh.js`; do not
  re-import it.
- Keep a scene's cost sane. `build` runs on every parameter tick: cap lattice
  and octave counts, and memoize anything expensive that the sliders do not
  change (see the `truncateCache` in `src/demos/deformers.ts`). CSG is O(n²) per
  boolean; marching cubes is O(n³) in the node count.
- A page whose scenes mix planar and spatial work sets `Scene.viewer` per scene
  rather than settling for one camera.
- `npm run typecheck` and `npm run smoke` must both pass before you report done.
  `npx tsx scripts/probe.mts` is the quick way to find out whether a member
  evaluates at all before you build a scene around it.

## Ports

Dev server: **5175** (5173 = geometry-samples, 5174 = SDF demo). If it is taken,
`npx vite --port 5185 --strictPort` is the spare the launch config knows about.
