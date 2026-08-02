---
lesson: vertex-index-safety
title: VertexIndex and Typed Mesh Indices
domain: Meshes & spatial structures
v3-files: [30-topology.plato, 31-meshes.plato]
audience: Programmers who have used integer index buffers in mesh code
status: draft-v1
---

# VertexIndex and Typed Mesh Indices

An index buffer full of `int` values is a crime scene waiting to happen. Slot
`17` might mean "vertex 17," "triangle 17," or "material 17" depending on
which array you forgot you were reading. The bug compiles, runs, and draws
fireworks. Typed indices exist to make that confusion a type error instead of
a weekend.

## The idea

Meshes separate **geometry** (positions, normals, UVs) from **topology**
(which vertices form which faces). Topology is almost entirely references:
integers that name slots in other arrays. The mathematical content is trivial
— zero-based offsets — but the *roles* are not interchangeable.

A **vertex index** names a position (and any per-vertex attributes). A
**face index** names a triangle or polygon. A **corner index** names one
vertex slot inside one face. Passing a face index into a position array is
meaningless even when the integer happens to be in range.

```
 Positions:  [P0] [P1] [P2] [P3] ...
                ^         ^
                |         |
 Faces[0]:   A=0 ------ C=2
                \       /
                 \     /
                  B=1
```

The type system's job is to keep "0 meaning vertex 0" distinct from "0
meaning face 0." Sentinel values matter too: many adjacency tables use $-1$
for "no neighbor." That $-1$ must not be confused with a valid slot.

## In Plato

`30-topology.plato` declares a family of index wrappers. All implement
`Index`, store a zero-based `Value`, and reserve $-1$ for "none":

```plato
// A zero-based index of a vertex within a mesh or point set. -1 means "none".
type VertexIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type EdgeIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type FaceIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type CornerIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}

type HalfEdgeIndex
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}
```

Edges as pairs of vertices stay typed end-to-end:

```plato
type VertexPair
    implements Value, Hashable
{
    A: VertexIndex;
    B: VertexIndex;
}
```

Meshes in `31-meshes.plato` consume those types. Face records may not store
$-1$; every corner must name a real vertex:

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

type TriangleMesh3D
    implements Value, TriangulatedGeometry3D, MeshTopology
{
    Positions: Array<Point3D>;
    Faces: Array<TriangleFace>;
}
```

Random access on triangulated geometry is also typed:

```plato
interface TriangulatedGeometry3D
    inherits Meshable3D
{
    TriangleCount(x: Self): Integer;
    FaceAt(x: Self, face: FaceIndex): TriangleFace;
    PositionAt(x: Self, vertex: VertexIndex): Point3D;
}
```

Usage-shaped sketch:

```plato
let mesh: TriangleMesh3D = ...;
let f = FaceIndex { Value: 0 };
let tri = FaceAt(mesh, f);

let pa = PositionAt(mesh, tri.A);
let pb = PositionAt(mesh, tri.B);
let pc = PositionAt(mesh, tri.C);

// Illegal at the type level: FaceIndex where VertexIndex is required
// let oops = PositionAt(mesh, f);
```

CSR adjacency keeps offsets as raw `Integer` (they are boundaries, not
element references) but packs typed payloads:

```plato
type VertexAdjacency
    implements Value
{
    Offsets: Array<Integer>;
    Neighbors: Array<VertexIndex>;
}
```

That split matches the vocabulary README: CSR offset arrays stay
`Array<Integer>`; cross-references use typed indices.

Half-edge navigation refuses to take the sentinel as input — callers must
not pass $-1$; results may still return $-1$ for boundary twins:

```plato
interface HalfEdgeNavigable
{
    OriginOf(x: Self, halfEdge: HalfEdgeIndex): VertexIndex;
    TwinOf(x: Self, halfEdge: HalfEdgeIndex): HalfEdgeIndex;
    // ...
}
```

## Pitfalls / fine print

**Raw `Integer` still appears.** Counts, CSR offsets, bitmasks, and host
handles remain plain integers by design. Do not wrap those as `VertexIndex`.
The rule is about *references to elements*, not every whole number.

**`-1` is data, not a crash.** Adjacency and opposites use $-1$ for absence.
Always branch before calling `PositionAt` or `FaceAt` with a possibly-absent
index. Face records themselves must not contain $-1`.

**Corner vs vertex.** `CornerIndex` $c$ on a triangle mesh maps to face
$c/3$ and slot $c \bmod 3$. The vertex at that corner is
`Corners[c]` in a `CornerTable` — a `VertexIndex`. Conflating corner numbers
with vertex numbers duplicates or drops attributes at seams.

**Sharing vs splitting.** Typed indices encourage vertex sharing (one
`VertexIndex`, many incident faces). Smoothing groups and UV islands
sometimes require duplicate positions with different attribute indices —
that is a second index channel problem, not a reason to drop typing.

**Comparable does not mean interchangeable.** `VertexIndex` and `FaceIndex`
both implement `Comparable`, but comparing across roles is still nonsense.
Equality is within one index space.

## Try it

1. A mesh has 8 positions and 12 triangles. What is the valid range of
   `VertexIndex.Value`? Of `FaceIndex.Value`?
2. Why are `VertexAdjacency.Offsets` typed as `Array<Integer>` while
   `Neighbors` is `Array<VertexIndex>`?
3. `TriangleFace` forbids $-1` in $A,B,C$, but `EdgeAdjacency.RightFaces`
   allows $-1`. Why the difference?

<details>
<summary>Answers</summary>

1. Vertices: $0..7$. Faces: $0..11$.
2. Offsets are one-past-end boundaries into the packed array, not names of
   mesh elements; neighbors *are* vertex references.
3. Every triangle corner must reference a real vertex. A boundary edge
   genuinely has no right face, so $-1$ is a meaningful adjacency sentinel.

</details>

## Library recommendations

- **missing-function** — `30-topology.plato`: no `IsNone(i: VertexIndex):
  Boolean` (or shared `Index` helper) for the $-1$ sentinel. Every safe
  walk re-implements `Value < 0` by hand; a named predicate would document
  the convention at the call site.

- **missing-function** — `31-meshes.plato`: `TriangulatedGeometry3D` gives
  `PositionAt` / `FaceAt` but no `TryPositionAt` / bounds-checked variant
  returning an optional or sentinel. Teaching safety currently stops at
  "caller must validate."

- **doc-comment** — `03-interfaces-collections.plato` / `30-topology.plato`:
  the `Index` interface should restate the global $-1$ means none rule in one
  place, since every typed index repeats it in a one-liner that readers may
  skim past.

- **naming** — `31-meshes.plato`: `SlotIndex` is another typed index for
  materials/batches. A short cross-reference in the topology file's index
  section would show that the pattern extends beyond mesh elements, reducing
  "why not just int?" pushback.
