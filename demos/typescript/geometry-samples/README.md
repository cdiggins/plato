# Geometry Samples (TypeScript)

A browser for geometry algorithms and data structures built on the **Plato**
geometry library, in the spirit of the Three.js example browser: sample list
on the left, 3D viewer and syntax-colored source on the right (with tabs for
the sample driver, the Plato source, and the generated TypeScript).

```
npm install
npm run dev        # open http://localhost:5173
npm test           # run all invariants under Node (no browser needed)
npm run gen:plato  # regenerate src/plato/plato.g.ts from demos/plato-src/
```

## The Plato pipeline

Algorithms and types are written once in Plato
([`../../plato-src/geometry.plato`](../../plato-src/geometry.plato)) and
generated to TypeScript by [`Plato.TypeScriptWriter`](../../../Plato.TypeScriptWriter).
The same source also targets Rust (see
[`../../rust/geometry-samples`](../../rust/geometry-samples)) and C#. The
generated library is designed so TypeScript reads like the C# equivalent:

- **Fluent syntax on plain numbers.** Plato's `Number`/`Integer`/`Boolean`/
  `String` map to native `number`/`boolean`/`string`; their functions are
  installed on the native prototypes (non-enumerable) with `declare global`
  typings. So `(0.5).Turns().Cos()`, `x.Sqrt()`, `t.Clamp(0, 1)` all work on
  plain values, and native arithmetic operators still apply.
- **No property getters.** Only declared fields are properties (`v.X`);
  everything else is a method (`v.Length()`, `a.Dot(b)`), matching the
  extension-method convention on the C# side.
- Concrete types are immutable classes with `With` functions, `Create`,
  `Default`, and structural `Equals`.

## Architecture

```
../../plato-src/   geometry.plato — shared Plato source for TS and Rust demos
src/plato/         plato.g.ts — GENERATED TypeScript (do not edit; gen:plato)
src/core/          Scene-description types and mesh-building helpers. No Three.js.
src/samples/       One module per algorithm; pure build() → Drawable[]; runs in Node.
src/adapters/      three.ts — only Three.js boundary module
src/app/           Browser shell: viewer, sample list, tabbed code panel
tests/             node:test conformance, sample invariants, adapter round-trips
```

## Samples

| Sample | What it demonstrates |
|---|---|
| Parametric Surface | Tessellating f(u,v); fluent `u.Turns().Cos()` angles |
| Icosphere Subdivision | Recursive refinement; `a.MidPoint(b).Normalize()` |
| Value-Noise Terrain | fBm heightfield; `v00.Lerp(v10, fx)` |
| Delaunay Triangulation | Bowyer-Watson; `p.DistanceSquared(center)` |
| Convex Hull | Monotone chain; the turn test is `(a-o).Cross(b-o)` |
| Spline + Tube Sweep | Catmull-Rom + parallel transport, fully fluent |
| Octree | Adaptive subdivision; `min.MidPoint(max)` |
| BVH (AABB Tree) | Median split; `min.Min(v)` / `max.Max(v)` bounds |
| Half-Edge + Smoothing | One-ring traversal; `p.Lerp(ringAverage, lambda)` |
| Raycasting | Moller-Trumbore, reads identically to the C# version |
| Poisson Disk Sampling | Bridson blue noise; `(gy - 2).Max(0)` grid clamps |
| Marching Squares | Iso-contours of a metaball field |

## Adding a sample

1. If new geometry is needed, add it to `../../plato-src/geometry.plato` and run
   `npm run gen:plato` (and regenerate Rust with `../../rust/geometry-samples/gen-plato.ps1`).
2. Create `src/samples/mySample.ts` exporting the algorithm plus a `Sample`
   object whose `build()` returns `Drawable[]`.
3. Register it in `src/samples/index.ts` and add a `?raw` import in
   `src/app/sources.ts`.
4. Add an invariant test in `tests/`.
