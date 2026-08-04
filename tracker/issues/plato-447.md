---
id: plato-447
title: "VertexNormalVectors scans every face per vertex: 152s for a 7680-vertex mesh, unusable at demo scale"
type: bug
status: ready
priority: p1
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439, plato-446]
---

## Problem

`VertexNormalVectors` (`stdlib/geometry/remeshing.library.plato`) is a MapRange over
vertices whose body is a Reduce over **every face**, testing `HasVertex`:

```
VertexNormalVectors(self: TriangleMesh3D): Array<Vector3D>
    => self.Positions.Count.MapRange(v => NormalizedOrZero(
        self.Faces.Count.MapRange(f => f).Reduce(Vector3D(0,0,0), (acc, f) =>
            self.Faces[f].HasVertex(VertexIndex(v)) ? acc + … : acc)));
```

That is vertices × faces. Smooth-shading the geometry-samples parametric surface —
a 160 x 48 torus tessellation, 7680 vertices and 7680 quads — took **152 seconds** in
generated TypeScript, which is what made the demo's browser page appear to hang: the
adapter asks for vertex normals on the first sample it draws.

Unlike plato-446, this quadratic is not documented and not intended: the function
computes a per-face quantity and scatters it, so the natural form is linear.

Measured with (from `demos/typescript/geometry-samples`, after `npm run build:node`):

```js
const [mesh] = samples[0].build();
const t = Date.now(); mesh.mesh.VertexNormalVectors(); Date.now() - t;
```

## Approach

Accumulate per FACE instead of per vertex: each triangle contributes its
`TwiceVectorArea` to its three corners, then normalize. In the expression vocabulary
that is a scatter, which `Reduce` cannot express directly — but the same shape already
appears in `VertexNeighborTable` and `BoundaryNeighborSum`, so whatever those do is
the precedent to follow. If the scatter genuinely needs a builder, `Buffer<Vector3D>`
sized by vertex count is the affine tool for it.

Worth checking the C# backend's timing on the same mesh: the complexity is in the
body, so it is quadratic there too, just with a smaller constant.

## Workaround in place

`demos/typescript/geometry-samples/src/adapters/three.ts` asks the stdlib for normals
only below a vertex-count cap and falls back to `BufferGeometry.computeVertexNormals`
above it, with a comment pointing here. Remove the cap when this closes.

## Done means

- [ ] `VertexNormalVectors` is linear in face count.
- [ ] Timing for the 7680-vertex torus recorded here, before and after.
- [ ] The geometry-samples adapter drops its cap and always uses the stdlib normals.
