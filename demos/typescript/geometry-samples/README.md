# Geometry Samples (TypeScript)

A browser for geometry algorithms and data structures built on the **Plato**
standard library, in the spirit of the Three.js example browser: sample list
on the left, 3D viewer and syntax-colored source on the right (with tabs for
the sample driver, the Plato library files it leans on, and the generated
TypeScript).

The samples are drivers, not implementations: the geometry belongs to the
stdlib, and each sample says which part of it it is showing.

```
npm install
npm run dev        # open http://localhost:5173
npm test           # run all invariants under Node (no browser needed)
npm run gen:plato  # regenerate src/plato/plato.g.ts from the forward stdlib
```

## The Plato pipeline

Geometry comes from the full forward Plato stdlib
([`../../../stdlib`](../../../stdlib): `foundation`, `geometry`, and `graphics`
tiers; `future` and `tests` are excluded), generated to TypeScript by
[`Plato.TypeScriptWriter`](../../../writers/Plato.TypeScriptWriter). The same
source also targets C# (`generated/`). The generated library is designed so
TypeScript reads like the C# equivalent:

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
../../../stdlib/   Forward Plato stdlib — the source of all geometry types
src/plato/         plato.g.ts — GENERATED TypeScript (do not edit; gen:plato)
src/core/          Scene description + IArray interop. No geometry of its own.
src/samples/       One module per algorithm; pure build() → Drawable[]; runs in Node.
src/adapters/      three.ts — only Three.js boundary module
src/app/           Browser shell: viewer, sample list, tabbed code panel
tests/             node:test conformance, sample invariants, adapter round-trips
```

A `Drawable` carries stdlib geometry — a `TriangleMesh3D`, an array of `Line3D`,
an array of `Point3D` — not flat number arrays. Flattening happens once, in the
Three.js adapter.

## Samples

Each row names the stdlib entry point the sample is built on.

| Sample | Stdlib it exercises |
|---|---|
| Parametric Surface | `Torus` / `Supertoroid` (IParametricSurface) → `ToQuadMesh` |
| Icosphere Subdivision | `PolygonMesh3D.Icosahedron`, `TriangleMesh3D.SplitEdges` |
| Value-Noise Terrain | `ValueNoise2D` in a `ScalarFunctionField2D` → `ToTriangleMesh` |
| Delaunay Triangulation | `Triangle2D.Circumcenter`, `Bounds2D.HaltonPoints2D` |
| Convex Hull | `Point2D.TwiceSignedArea`, fills a `ConvexHull2D` |
| Spline + Tube Sweep | `CatmullRomCurve3D` wrapped in a `TubeSurface` |
| Octree | `Bounds3D` split / containment |
| BVH (AABB Tree) | `Bounds3D.UnionOfBounds`, `Triangle3D.Centroid` |
| Connectivity + Smoothing | `TriangleMesh3D.TopologyOf`, `VertexNeighborTable` |
| Raycasting | `Triangle3D.Raycast` (Möller–Trumbore) over `Primitives` |
| Poisson Disk Sampling | `Bounds2D.PoissonDiskPoints2D` |
| Marching Squares | `MetaBallSystem2D`, `IScalarField2D.IsoContour` |

### What is deliberately still TypeScript

The insertion loops — Bowyer-Watson, monotone chain, octree and BVH
construction — stay in the samples. They need a sort and a growable tree, and
the forward vocabulary has neither yet; `tracker/issues/plato-442.md` records
the decision and what it would take. Those samples still use stdlib types and
predicates throughout.

A few calls are avoided with a comment naming the issue: `LaplacianSmoothed`
(sum-typed parameter, plato-440), `Triangle3D.Bounds` and
`UniformLaplacianField` (overload collapse returns 2D results, plato-441),
`LoopSubdivided` (returns NaN, plato-444). The adapter falls back to Three.js
for vertex normals above a size cap (plato-447).

## Adding a sample

1. If new geometry is needed, add it to the forward stdlib (`../../../stdlib`,
   gated by `Plato.CLI lint`) and run `npm run gen:plato`.
2. Create `src/samples/mySample.ts` exporting the algorithm plus a `Sample`
   object whose `build()` returns `Drawable[]`.
3. Register it in `src/samples/index.ts` and add a `?raw` import in
   `src/app/sources.ts`.
4. Add an invariant test in `tests/`.
