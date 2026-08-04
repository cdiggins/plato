---
id: plato-439
title: "geometry-samples: rebuild the TS samples on stdlib types, port missing builders back to stdlib"
type: feature
status: done
priority: p2
effort: L
risk: low
area: plato
sprint: 
created: 2026-08-04
closed: 2026-08-04
links: []
---

## Problem

`demos/typescript/geometry-samples` now *generates* the whole forward stdlib
(`9655ffe2`) but consumes almost none of it. Every sample open-codes its geometry in
TypeScript — its own mesh record, its own vertex normals, its own icosahedron, its
own Laplacian smoothing — while `stdlib/geometry` already ships
`PolygonMesh3D.Icosahedron`, `TriangleMesh3D.LoopSubdivided`, `LaplacianSmoothed`,
`VertexNormalVectors` and `Bounds2D.PoissonDiskPoints2D`. The demo therefore
demonstrates the TypeScript writer, not the library.

Second-order problem: writing the samples against the library is the only thing that
has exercised it from a consumer's seat, and it immediately turned up gaps where a
type exists with no way to build it.

## Done means

- [x] Samples consume stdlib geometry types; the scene record carries
      `TriangleMesh3D` / `Point3D` / `Line3D` rather than flat number arrays, and the
      Three.js adapter is the only place that flattens.
- [x] Hand-written geometry that duplicates shipping stdlib is deleted, not wrapped.
- [x] Genuine gaps found from the consumer seat are either filled in
      `stdlib/geometry` or written up here as follow-ups with the reason they were
      not filled.
- [x] `npm run typecheck`, `npm test` and `npm run build` green; every sample renders
      in the browser with a clean console.
- [x] stdlib edits gated: `.\tools\check-stdlib-fast.ps1` green (lint --strict,
      checker ratchet, index freshness).

## Gaps found from the consumer seat

| Gap | Outcome |
|---|---|
| No way to tessellate an `IParametricSurface` into a mesh. `AllQuadFaceIndices` and `ClosedU`/`ClosedV` existed; nothing joined them. | **Filled** — `GridParameter`, `GridPositions`, `ToQuadMesh`, `ToTriangleMesh` in `meshes.library.plato` |
| No way to draw a 2D scalar field as a surface. | **Filled** — `GraphPoint`, `GraphPositions`, `ToQuadMesh`, `ToTriangleMesh` over `IScalarField2D`, same file |
| No ray/triangle intersection; only ray/plane. | **Filled** — `MollerTrumbore` + `Raycast(Triangle3D, Ray3D)` in `lines.library.plato` |
| 2D iso-contour extraction. `MarchingCubes*` shipped for 3D; the 2D sibling was absent. | **Filled** — marching squares in `fields-implicits.library.plato`, numbered to match the cubes |
| `ConvexHull2D` / `Octree3D` / `Bvh3D` / `KdTree3D` ship with query bodies and no builders; Delaunay absent. | **Not filled** — needs an ordering primitive and a growable tree. plato-442 |

## Writer defects this surfaced

Six, all found by using the generated library rather than reading it. Fixed here:
`FlatMap` declared and never implemented; `MakeArray2D` on the ignore list and never
installed; `Array2D` members emitted as methods where bodies read them as fields;
free array functions emitted as module functions but called in receiver position;
interface functions Array implements never reaching `Arr`; `Number.Pi`/`MinValue`/
`MaxValue`/`Epsilon` throwing; constant-idiom calls in static position; integer
division collapsing to float because Number and Integer share one prototype; tuple
literals returning a `TupleN` carrier instead of the declared type.

Filed rather than fixed: plato-440 (sum-typed parameters), plato-441 (overload
collapse, raised to p1 — it returns wrong types silently), plato-443 (interface-typed
fields bind Self to the owner), plato-444 (`LoopSubdivided` NaN), plato-446
(quadratic `CornerTwinTable`), plato-447 (quadratic `VertexNormalVectors`).
