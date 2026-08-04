---
id: plato-439
title: "geometry-samples: rebuild the TS samples on stdlib types, port missing builders back to stdlib"
type: feature
status: in-progress
priority: p2
effort: L
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
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

- [ ] Samples consume stdlib geometry types; the scene record carries
      `TriangleMesh3D` / `Point3D` / `Line3D` rather than flat number arrays, and the
      Three.js adapter is the only place that flattens.
- [ ] Hand-written geometry that duplicates shipping stdlib is deleted, not wrapped.
- [ ] Genuine gaps found from the consumer seat are either filled in
      `stdlib/geometry` or written up here as follow-ups with the reason they were
      not filled.
- [ ] `npm run typecheck`, `npm test` and `npm run build` green; every sample renders
      in the browser with a clean console.
- [ ] stdlib edits gated: `.\tools\check-stdlib-fast.ps1` green (lint --strict,
      checker ratchet, index freshness).

## Gaps found from the consumer seat

| Gap | Verdict |
|---|---|
| No way to tessellate an `IParametricSurface` into a mesh. `AllQuadFaceIndices` and `ClosedU`/`ClosedV` exist; nothing joins them. | Fill — `ToQuadMesh` in `surfaces.library.plato` |
| 2D iso-contour extraction. `MarchingCubes*` ships for 3D; the 2D sibling is absent. | Fill if the per-cell shape carries over |
| `ConvexHull2D` / `ConvexHull3D` types ship with query bodies (`HullVertexCount`, `SourceOf`, …) and no constructor. | Assess — needs a sort primitive |
| `Octree3D` / `Bvh3D` / `KdTree3D` ship with query bodies (`Raycast`, `CandidatesInBounds`) and no builders. | Assess — recursive construction over affine builders |
| Delaunay triangulation. `triangulation.library.plato` is ear-clipping only. | Assess |
