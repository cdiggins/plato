---
lesson: indexed-meshes
title: Indexed Meshes — Shared Vertices and Typed Indices
domain: Meshes & spatial structures
v3-files: [30-topology.plato, 31-meshes.plato]
audience: Comfortable with 3D points and triangles; no prior mesh-format experience required
status: draft-v1
---

# Indexed Meshes — Shared Vertices and Typed Indices

A solid model, a game character, a terrain tile: most real 3D surfaces arrive as **many
triangles**. The naïve encoding stores three full `(x, y, z)` triples per triangle — nine
numbers per face, with no connection between corners that sit at the same physical location.
That works for a one-off diagram, but it wastes memory, hides adjacency, and makes smooth
shading impossible: each corner is an island.

**Indexed meshes** split the problem in two. A **vertex buffer** holds each distinct spatial
position once. An **index buffer** (or face list) says, for each triangle, *which three
entries in the vertex buffer to use*. Two triangles that meet along an edge refer to the
**same** vertex index at the shared corner. Memory drops; neighborhood queries become
possible; averaged normals have a well-defined anchor.

This lesson is about that split — why it matters geometrically, how Plato names the pieces,
and why `VertexIndex` is not the same thing as a bare `Integer`.

---

## Triangle soup vs indexed storage

Consider two triangles sharing one edge, forming a quadrilateral bent slightly out of the
plane:

```
        v2 -------- v3
       / \         /
      /   \   T1  /
     /     \     /
    v0 ----- v1
         T0
```

**Triangle soup** stores six independent corners:

| Triangle | corner 0 | corner 1 | corner 2 |
|----------|----------|----------|----------|
| T0       | (0,0,0)  | (1,0,0)  | (0,1,0)  |
| T1       | (1,0,0)  | (1,1,0)  | (0,1,0)  |

The point `(1,0,0)` appears twice. Nothing in the data says those two copies are the same
vertex. A renderer that averages normals per spatial location must rediscover equality by
comparing coordinates — slow, fragile under floating-point noise.

**Indexed storage** uses four positions and two triples of indices:

```
Positions[0] = (0,0,0)   // v0
Positions[1] = (1,0,0)   // v1
Positions[2] = (0,1,0)   // v2
Positions[3] = (1,1,0)   // v3

Face T0: indices (0, 1, 2)
Face T1: indices (1, 3, 2)
```

Six index integers plus four positions replace six full points. More importantly, index `1`
means *the same vertex* in both faces. Connectivity is explicit.

---

## The index buffer pattern

General pattern for a triangle mesh:

1. **`Positions`** — `Array<Point3D>`, length $V$. Slot $i$ is the embedded location of
   vertex $i$.
2. **`Faces`** — one record per triangle. Each record holds three **vertex indices** into
   `Positions`, not coordinates.

Lookup is always a two-step read:

$$
\text{corner position} = \text{Positions}[\text{face corner index}]
$$

The face record is topology (who connects to whom). The position array is geometry (where
they sit in space). File `30-topology.plato` owns connectivity vocabulary; file
`31-meshes.plato` binds it to `Point3D` coordinates.

Other surface encodings in v3 follow the same philosophy:

| Type | Vertex buffer | Index side |
|------|---------------|------------|
| `TriangleMesh3D` | `Array<Point3D>` | `Array<TriangleFace>` — fixed triples |
| `QuadMesh3D` | `Array<Point3D>` | `Array<QuadFace>` — fixed quadruples |
| `PolygonMesh3D` | `Array<Point3D>` | CSR: `FaceOffsets` + `FaceVertices` |
| `LineSet3D` | `Array<Point3D>` | `Array<VertexPair>` — segment endpoints |
| `PointCloud3D` | `Array<Point3D>` | *(none — no connectivity)* |

`PointCloud3D` is the contrast case: scan points, particles, samples. Positions only; no
faces. Useful, but not a surface you can consistently shade or walk across edges.

---

## In Plato: `TriangleMesh3D`

The workhorse declaration:

```plato
// An indexed triangle mesh in 3D: the workhorse interchange representation
// for surface geometry. Faces index into Positions.
type TriangleMesh3D
    implements Value, TriangulatedGeometry3D, MeshTopology
{
    Positions: Array<Point3D>;
    Faces: Array<TriangleFace>;
}
```

Each triangle is a **`TriangleFace`** — three typed vertex slots, not three points:

```plato
// The three vertex indices of one triangle, wound counter-clockwise when
// viewed from the front side.
type TriangleFace
    implements Value, Hashable
{
    A: VertexIndex;
    B: VertexIndex;
    C: VertexIndex;
}
```

**Winding** matters. v3 fixes the front side convention: counter-clockwise when viewed
from outside, matching `WindingOrder.CounterClockwise`. Swap two corners and you flip the
face normal — lighting, culling, and signed volume tests all depend on consistent winding.

Building the shared-edge example:

```plato
let v0 = Point3D { X: 0, Y: 0, Z: 0 };
let v1 = Point3D { X: 1, Y: 0, Z: 0 };
let v2 = Point3D { X: 0, Y: 1, Z: 0 };
let v3 = Point3D { X: 1, Y: 1, Z: 0 };

let mesh = TriangleMesh3D {
    Positions: [v0, v1, v2, v3],
    Faces: [
        TriangleFace {
            A: VertexIndex { Value: 0 },
            B: VertexIndex { Value: 1 },
            C: VertexIndex { Value: 2 }
        },
        TriangleFace {
            A: VertexIndex { Value: 1 },
            B: VertexIndex { Value: 3 },
            C: VertexIndex { Value: 2 }
        }
    ]
};
```

Reading a corner back:

```plato
let face = mesh.Faces.At(0);                    // first triangle
let pa = mesh.Positions.At(face.A.Value);       // Point3D at vertex A
let pb = mesh.Positions.At(face.B.Value);
let pc = mesh.Positions.At(face.C.Value);
```

The **`TriangulatedGeometry3D`** interface names the intended access pattern at the mesh
boundary:

```plato
interface TriangulatedGeometry3D
    inherits Meshable3D
{
    TriangleCount(x: Self): Integer;
    FaceAt(x: Self, face: FaceIndex): TriangleFace;
    PositionAt(x: Self, vertex: VertexIndex): Point3D;
}
```

So idiomatic code prefers `mesh.PositionAt(face.A)` over manual `.Value` indexing — same
data, clearer roles. **`MeshTopology`** adds counts: `VertexCount`, `EdgeCount`,
`FaceCount` — connectivity tallies without embedding coordinates.

---

## Typed indices: `VertexIndex` vs raw `Integer`

v3 forbids using bare `Integer` wherever an array slot refers to **another element** in a
domain-specific collection. Mesh corners use **`VertexIndex`**:

```plato
// A zero-based index of a vertex within a mesh or point set. -1 means "none".
type VertexIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}
```

Related index types from the same file keep roles separate:

| Type | Indexes |
|------|---------|
| `VertexIndex` | A slot in `Positions` |
| `FaceIndex` | A slot in `Faces` (or polygon face $f$) |
| `EdgeIndex` | A slot in an `EdgeList` |
| `CornerIndex` | One corner of one triangle in a corner table |
| `HalfEdgeIndex` | A directed half-edge in `HalfEdgeMesh` |

Each implements the **`Index`** interface — a typed wrapper whose `Value` is the underlying
zero-based integer:

```plato
interface Index
{
    Value(x: Self): Integer;
}
```

**Why bother wrapping?** In large codebases, integers are ambiguous: is `3` a vertex, a
face, a bone, a material slot? Passing a `FaceIndex` where a `VertexIndex` is expected is
a bug the type system can catch once libraries enforce the distinction. The shared sentinel
**`-1` means "none"** across these types — boundary half-edges, missing adjacency, optional
links — but **face corners must never hold `-1`**; every triangle corner must reference a
valid position.

**When raw `Integer` is correct:** offsets and sizes that are not element references.
`PolygonMesh3D` stores variable-length faces in CSR form:

```plato
type PolygonMesh3D
    implements Value, Meshable3D, MeshTopology
{
    Positions: Array<Point3D>;
    FaceOffsets: Array<Integer>;      // one-past-end boundaries — not VertexIndex
    FaceVertices: Array<VertexIndex>; // the actual vertex references
}
```

Face $f$'s vertices lie in the half-open range
`FaceVertices[FaceOffsets[f] .. FaceOffsets[f+1])`. The offsets array holds **boundaries**,
not mesh elements; it stays `Array<Integer>` per v3 conventions. The payload array holds
`VertexIndex`.

For segments, endpoints pair up as **`VertexPair`** (canonical undirected edge naming with
`A.Value <= B.Value` unless direction is authored, as in `LineSet3D`):

```plato
type VertexPair
    implements Value, Hashable
{
    A: VertexIndex;
    B: VertexIndex;
}
```

---

## What sharing buys you

**Memory.** A dense scanned surface with $F$ triangles and roughly $F/2$ interior edges
needs about $F$ distinct vertices (Euler-style hand-waving for closed shapes), not $3F$.
Index buffers are compact; position buffers dominate for large, smooth models anyway, but
deduplication still matters.

**Smooth shading.** A vertex normal averaged from all incident face normals requires
knowing which face corners meet at one index. Shared indices make that star of faces
 enumerable without coordinate hashing.

**Topology.** Edge lists, adjacency tables, half-edge structures (`HalfEdgeMesh`,
`CornerTable`, CSR `VertexAdjacency`) all assume one canonical vertex id per mesh corner
position. They live in `30-topology.plato` precisely because indexed meshes separate
connectivity from coordinates.

**Algorithms.** Subdivision, decimation, remeshing, geodesics, seam detection — all need
adjacency. Indexed form is the on-ramp.

---

## When you deliberately *don't* share

Indexed sharing identifies corners by **index equality**, not by **coordinate equality**.
Sometimes you want the same `(x,y,z)` twice:

- **Hard edges** — two faces meet at a crease; each side needs its own shading normal even
  though positions coincide. Duplicate the position (or split the vertex in the buffer) and
  use different indices.
- **UV seams** — texture coordinates differ while the 3D point does not. Classic meshes
  carry separate attribute buffers (UVs, colors) with their own index streams; v3's
  `TriangleMesh3D` declares positions and face indices only — richer attribute layouts are
  out of scope here but the duplication rule is the same.
- **Per-corner data** — sharp rendering often stores normals or colors per **corner**
  (`CornerIndex` territory) rather than per `VertexIndex`.

If every triangle soup corner becomes a unique vertex index, you recover soup semantics —
valid as a fallback, expensive as a default.

---

## Pitfalls and fine print

**Bounds.** Every index in a face must satisfy `0 <= Value < Positions.Count`. Out-of-range
indices are undefined behavior in consumers; validators belong in importers and editors.

**Winding and normals.** Given CCW corners `A, B, C` when viewed from the front, the
unnormalized face normal follows the right-hand rule on edge vectors
`(PositionAt(B) - PositionAt(A)) × (PositionAt(C) - PositionAt(A))`. Inconsistent winding
across the mesh produces holes in back-face culling and wrong signed volumes.

**`-1` is not a vertex.** Reserve it for optional graph edges and adjacency slots. Triangle
faces require three real vertices.

**Do not mix index kinds.** `FaceOffsets[i]` is an integer boundary; it is not a
`FaceIndex`. `CornerIndex` maps to triangle corners in corner-table algorithms
(`corner c` belongs to triangle `c / 3`, slot `c mod 3`) — a different indexing scheme
than `TriangleFace`'s three `VertexIndex` fields.

**Manifoldness is not guaranteed.** `TriangleMesh3D` can represent non-manifold joins,
T-junctions, or open borders. `TopologySummary` and `Manifoldness` classify what you have;
they do not fix it.

**2D is parallel.** `TriangleMesh2D` uses `Array<Point2D>` with the same `TriangleFace`
records — triangulated planar regions, FEM meshes in the plane.

---

## Try it

<details>
<summary>Exercise 1 — Counts</summary>

Two triangles share exactly one edge (like the quad example above). How many entries does
`Positions` need at minimum? How many `TriangleFace` records? How many total vertex index
slots appear across all faces (counting repeats)?

**Answer.** Minimum **4** positions, **2** faces, **6** index slots (each face references
three corners; the shared edge contributes two indices that repeat the same vertex id).
</details>

<details>
<summary>Exercise 2 — Winding flip</summary>

A front-facing triangle has corners `(A,B,C) = (0,1,2)` CCW when viewed from +Z. You swap
`A` and `B` to `(1,0,2)`. What happens to the geometric normal direction?

**Answer.** The normal flips sign (points the opposite way). Front/back classification
 reverses for a fixed camera.
</details>

<details>
<summary>Exercise 3 — CSR read</summary>

A `PolygonMesh3D` has `FaceOffsets = [0, 4, 7]` and nine entries in `FaceVertices`. How
many faces? How many vertices does face 0 have? Face 1?

**Answer.** **2** faces (`FaceOffsets.Count - 1`). Face 0 spans indices `[0,4)` → **4**
vertices. Face 1 spans `[4,7)` → **3** vertices.
</details>

---

## Library recommendations

- **missing-function** — `31-meshes.plato`: `TriangleMesh3D` has no declared builder from
  flat arrays (`Array<Point3D>` + `Array<Integer>` triples or `Array<VertexIndex>`). Every
  lesson example hand-assembles `TriangleFace` records; a `FromIndexed` (or
  `TriangulatedGeometry3D`-level constructor) would match how importers actually arrive at
  meshes and give one place to validate bounds and reject `-1`.

- **missing-function** — `31-meshes.plato` / `TriangulatedGeometry3D`: no
  `TriangleCornersAt(x, face: FaceIndex): (Point3D, Point3D, Point3D)` or equivalent
  returning the three embedded corners in one call. Readers derive it from `FaceAt` +
  `PositionAt`; a named helper would document the canonical lookup and keep `.Value` leaks
  out of user code.

- **doc-comment** — `30-topology.plato`: `CornerIndex` doc ties corners to triangle index
  arithmetic (`c / 3`, `c mod 3`) while `TriangleFace` uses three explicit `VertexIndex`
  fields. A one-line cross-note on `TriangleFace` ("corners are not `CornerIndex`; use
  `CornerTable` when corner-table navigation is required") would reduce confusion at the
  boundary between explicit face records and corner-table topology.

- **missing-interface** — `31-meshes.plato`: no shared **`IndexedSurfaceMesh<P, F>`** (or
  similar) abstraction factoring `Positions` + face indexing shared by `TriangleMesh3D`,
  `QuadMesh3D`, and `PolygonMesh3D`. Teaching the vertex-buffer / index-buffer pattern once
  is natural; three parallel struct shapes suggest a parametric interface with
  `PositionAt(vertex: VertexIndex): P` and face-count operations would unify importers and
  validators.
