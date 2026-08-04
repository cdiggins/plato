---
id: plato-444
title: "TriangleMesh3D.LoopSubdivided returns NaN for every original vertex"
type: bug
status: ready
priority: p1
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439, plato-441]
---

## Symptom

Subdividing the stdlib icosahedron once produces the right SHAPE — 42 vertices, 80
faces, correct Euler characteristic — but the twelve vertices carried over from the
input come back with `Z = NaN`. X and Y are finite and plausible. The new edge
vertices (indices 12..41) are all finite, so `LoopEdgePoint` is fine and
`LoopVertexPoint` is not.

Reproduced in generated TypeScript (`demos/typescript/geometry-samples`):

```js
const ico = PolygonMesh3D.Icosahedron().ToTriangleMesh();
const sub = ico.LoopSubdivided();
sub.Positions.At(0);   // Point3D { X: -0.4035…, Y: 0.6529…, Z: NaN }
```

`ButterflySubdivided` and `SplitEdges` over the same mesh are clean, which is why the
geometry-samples icosphere uses `SplitEdges`.

## Where to look

X and Y surviving while only Z is lost is the signature of a 2D/3D confusion rather
than an arithmetic one — the same shape as plato-441, where
`UniformLaplacianField` on a `TriangleMesh3D` resolves to the `Vector2D` overload of
`UniformLaplacian` and returns 2D vectors. `LoopVertexPoint` sums the one-ring through
`BoundaryNeighborSum` / `LoopBeta`; check whether any of those resolve to a 2D
overload in the generated output before looking for a formula error.

If it IS overload resolution, this is a symptom of plato-441 and closes with it. If
the C# output is also wrong, the bug is in the body and this is the primary issue.

## Done means

- [ ] `LoopSubdivided` over a closed mesh returns finite positions for every vertex.
- [ ] Checked whether the C# backend has the same defect, and said so here.
- [ ] A law or test covering subdivision output finiteness.
