# WebGL demo studio — agent coordination

Four demos share one Vite app. The shell, the generated library and the prelude
are done; each demo is one file.

## File ownership

| Agent | May write | Must not touch |
|---|---|---|
| polyhedra | `src/demos/polyhedra.ts` | anything else |
| polygons | `src/demos/polygons.ts` | anything else |
| csg | `src/demos/csg.ts` | anything else |
| deformers | `src/demos/deformers.ts` | anything else |

`src/shared/**`, `src/plato/**`, the HTML pages, `package.json` and `README.md`
are shared: if one of them is genuinely missing something you need, say so in
your report rather than editing it. `demos/typescript/sdf/**` and
`writers/Plato.TypeScriptWriter/**` belong to other tracks — hands off.

## What a demo file does

Export nothing; call `mountDemo(demo)` at the end. The contract is in
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
    },
  ],
};
mountDemo(demo);
```

`build` is called on every parameter change and must be pure — the viewer
disposes the previous object.

## Rules

- Geometry comes from the **generated members** in `src/plato/plato.g.ts`.
  Building inputs and repacking outputs is demo work; recomputing a formula that
  the stdlib already defines is not. If a member you need throws, report it —
  do not silently hand-roll it.
- Use the helpers in `src/shared/mesh.ts` (`polygonMeshGeometry`,
  `polygonSoupGeometry`, `polygonMeshEdges`, `meshToSoup`, `toArray`) rather
  than reaching into `IArray` yourself.
- Import `../plato/array-ext.js` transitively via `../shared/mesh.js`; do not
  re-import it.
- Keep a scene's cost sane: CSG is O(n²) per boolean, so cap the input sizes.
- `npm run typecheck` and `npm run smoke` must both pass before you report done.

## Ports

Dev server: **5175** (5173 = geometry-samples, 5174 = SDF demo).
