# WebGL demo studio — agent coordination

Every demo shares one Vite app. The shell, the generated library, the prelude
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
| lattices | `src/demos/lattices.ts` | anything else |
| sampling | `src/demos/sampling.ts` | anything else |
| remeshing | `src/demos/remeshing.ts` | anything else |
| fea | `src/demos/fea.ts` | anything else |
| rigidbody | `src/demos/rigidbody.ts` | anything else |
| cloth | `src/demos/cloth.ts` | anything else |
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

## Simulation scenes

A scene that also declares `tick` is a simulation. The shell drives it once per
animation frame with the elapsed seconds (clamped to 1/20 s, so a backgrounded
tab does not resume with one enormous step), the current parameters, and the
object `build` returned:

```ts
{
  id: 'drape',
  viewer: { spin: false },        // the idle rotation reads as motion otherwise
  build: params => makeClothScene(params),   // also the RESET
  tick: (seconds, params, object) => {
    state = step(state, seconds);            // Plato does the stepping
    writePositions(object, state);           // demo work: repack into buffers
    return `frame ${n}  max stretch ${...}`; // replaces the status line
  },
}
```

Rules that are easy to get wrong:

- **`build` is the reset.** A parameter change rebuilds, so the simulation
  restarts from its initial state. Keep the mutable state in a closure that the
  `build` call creates — module-scope state is shared between scenes and
  survives the reset that was supposed to clear it.
- **`tick` mutates in place**; only `build` may allocate a new object. Update
  the `position` attribute and set `needsUpdate`, do not rebuild geometry per
  frame.
- **A throw in `tick` stops the driver** and leaves the message on the status
  line — it does not repeat sixty times a second.
- `npm run scenes` steps every ticking scene 30 frames at 1/60 s, prints the
  per-frame cost, marks anything over 16 ms `OVER BUDGET`, and **fails the gate
  if any position went non-finite** — a diverged solver throws nothing and draws
  nothing, so that check is the only thing standing between it and a green gate.

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

- `src/plato/plato.g.ts` is generated from **four** tiers — `stdlib/foundation`,
  `stdlib/geometry`, `stdlib/graphics` and `stdlib/future`. `future` joined the
  recipe so the finite-element, rigid-body and cloth pages have a library to
  drive; it is the tier that is not linted and not converted to C#, so a member
  from it is more likely to be rough than one from `geometry`. That makes the
  `UNAVAILABLE (…)` discipline below more important on those pages, not less.
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
- End the file with `export { demo };` — that is what lets `npm run scenes`
  reach your catalog.

## Gates

All three must pass before you report done:

| Command | What it proves |
|---|---|
| `npm run typecheck` | the app compiles |
| `npm run smoke` | the generated members the demos rely on still return the values the Plato source pins down |
| `npm run scenes` | **every scene on every page builds**, off the page: it imports each demo module, calls each scene's `build` at its default parameters, and fails on a throw or an empty result. It prints each scene's vertex count, build time and status line, and marks anything over 400 ms `SLOW`. A scene with a `tick` is also stepped — see "Simulation scenes" above |

`npm run probe` is not a gate — it is the quick way to find out whether a member
evaluates at all, before you build a scene around it.

**Several agents share this working tree.** `npm run typecheck` and
`npm run scenes` both see every page, so a failure in a file you do not own is
another agent mid-edit: re-run, and if it persists, say so in your report and
judge yourself on your own page's scenes. Never "fix" someone else's file.

Do not commit; report what you changed and let the coordinating session commit.
Do not start a dev server — the gates above are the verification, and the port
is shared.

## Ports

Dev server: **5175** (5173 = geometry-samples, 5174 = SDF demo). If it is taken,
`npx vite --port 5185 --strictPort` is the spare the launch config knows about.
