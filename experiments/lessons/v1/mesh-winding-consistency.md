---
lesson: mesh-winding-consistency
title: Mesh Winding Consistency
domain: Meshes & spatial structures
v3-files: [31-meshes.plato, 30-topology.plato]
audience: Basic mesh / triangle experience; right-hand rule helpful
status: draft-v1
---

# Mesh Winding Consistency

Two triangles share an edge. One lists the edge as $(A,B)$; the neighbor
lists it as $(A,B)$ too. Lighting flips, back-face culling eats half the
model, and a "watertight" export opens in another tool as a sieve. The
geometry is fine; the **winding** is inconsistent. Winding is the
orientation of a face loop — the order of its vertices — and consistent
winding is what makes "front" mean the same thing everywhere on a surface.

## The idea

Order three vertices $A, B, C$. By the right-hand rule, that order defines a
normal direction: fingers curl $A \to B \to C$, thumb points toward the
**front**.

```
        C
        ●
       / \
      /   \     A -> B -> C is counter-clockwise
     /     \    from the front (normal toward you)
    ●-------●
    A       B
```

Reverse to $A, C, B$ and the normal flips. Nothing about the positions
changed — only the combinatorial orientation.

On a shared interior edge, two faces should traverse the edge in **opposite**
directions. If face 0 walks $A \to B$ along the edge, face 1 should walk
$B \to A$. That is the manifold orientation condition. If both walk
$A \to B$, one face's idea of "front" points into the volume of the other.

```
   Face 0 walks A->B          Face 1 should walk B->A
        ● C                         D ●
       / \                         / \
      /   \                       /   \
     ●-----●                     ●-----●
     A  ->  B                    A  <-  B
```

Closed surfaces that are orientable admit a global choice: pick an outward
normal and wind every face so its front faces outward. Non-orientable
surfaces (Möbius bands) cannot; real-time meshes usually assume orientable
manifold-with-boundary data.

## In Plato

The library default is explicit in both topology and mesh files.

From `30-topology.plato`:

```plato
// The rotational direction of a face loop as seen from the front side.
// CounterClockwise is the library default.
type WindingOrder = CounterClockwise | Clockwise;
```

From `31-meshes.plato`:

```plato
// All faces wind counter-clockwise when viewed from the front
// (outside), matching WindingOrder.CounterClockwise.

type TriangleFace
    implements Value, Hashable
{
    A: VertexIndex;
    B: VertexIndex;
    C: VertexIndex;
}
```

Half-edges encode the same convention operationally: `Next` / `Previous`
walk the bordered face counter-clockwise, and twins oppose each other:

```plato
type HalfEdge
    implements Value
{
    Origin: VertexIndex;
    Twin: HalfEdgeIndex;
    Next: HalfEdgeIndex;
    Previous: HalfEdgeIndex;
    Face: FaceIndex;
}
```

Edge adjacency records which face sits on the "left" when walking $A \to B$:

```plato
type EdgeAdjacency
    implements Value
{
    Edges: Array<VertexPair>;
    LeftFaces: Array<FaceIndex>;
    RightFaces: Array<FaceIndex>;
}
```

Doc comment: `LeftFaces[i]` is the face where traversal $A \to B$ runs
counter-clockwise along the face loop. That is winding, stored as sidedness.

Usage-shaped normal from a consistently wound triangle:

```plato
let tri = FaceAt(mesh, face);
let pa = PositionAt(mesh, tri.A);
let pb = PositionAt(mesh, tri.B);
let pc = PositionAt(mesh, tri.C);
let n = Normalize(Cross(Between(pa, pb), Between(pa, pc)));
// n points toward the front if A,B,C are counter-clockwise from that side
```

Flipping one face to repair winding:

```plato
// Reverse orientation: A,C,B instead of A,B,C
let flipped = TriangleFace { A: tri.A, B: tri.C, C: tri.B };
```

Tetrahedral cells extend the rule to volumes: `TetrahedronCell` orders $D$
on the counter-clockwise side of triangle $A$-$B$-$C$ so signed volume is
positive — the 3D analogue of consistent winding.

## Pitfalls / fine print

**Importer disagreement.** Some formats are clockwise-by-default. Always
know the author's `WindingOrder` before trusting normals or culling.

**Transforms with negative determinant.** A reflection reverses winding in
world space. If you transform positions but not orientation policy, fronts
become backs. Detect $\det < 0$ and flip face order or invert normals.

**Duplicate vertices along a seam.** Two faces may look adjacent in space
but reference different `VertexIndex` values. Topological adjacency checks
(shared indices) then miss the edge; visual "neighbors" can disagree on
winding without ever sharing a `VertexPair`.

**Non-manifold edges.** An edge with three faces has no consistent left/right
pair. `Manifoldness` in `TopologySummary` flags this; winding repair
algorithms assume manifold-with-boundary.

**2D vs 3D screens.** "Counter-clockwise as seen from the front" needs a
viewpoint. In 2D (`TriangleMesh2D`), front usually means the plane's
positive normal (toward $+Z$ if embedded in XY). In 3D, front means outside
for closed meshes.

**UV and attribute islands.** Flipping a face in position space may require
flipping corresponding corner attributes so textures do not mirror
unintentionally.

## Try it

1. Triangle $(0,0,0)$, $(1,0,0)$, $(0,1,0)$. Is $A,B,C$ in that order
   counter-clockwise when viewed from $+Z$? Which way does the right-hand
   normal point?
2. Neighboring faces share vertices $0,1$. Face P lists edge as $0,1$; face
   Q lists $0,1$ as well. Are they consistently oriented?
3. Why does `HalfEdge.Twin` matter for checking winding consistency?

<details>
<summary>Answers</summary>

1. Yes from $+Z$; normal points toward $+Z$.
2. No — both traverse the shared edge the same way; one should be $1,0$.
3. On a consistently oriented manifold, a half-edge and its twin run opposite
   directions on the same undirected edge; twin links are the local witness
   of opposite traversal.

</details>

## Library recommendations

- **missing-function** — `31-meshes.plato`: no `FaceNormal(mesh,
  face: FaceIndex): Direction3D` or `SignedArea` helper that documents the
  CCW-front contract in code. Callers re-derive Cross(Between…) and can
  silently swap argument order.

- **missing-function** — `30-topology.plato`: no
  `AreConsistentlyWound(a: TriangleFace, b: TriangleFace): Boolean` (or edge-
  based check against `EdgeAdjacency`). Repair tools need a declared
  predicate for the opposite-traversal rule.

- **wrong-shape** — `31-meshes.plato`: meshes do not store a
  `WindingOrder` field; the CCW rule is global documentation only. An
  optional per-mesh `Winding: WindingOrder` would make imported CW data
  explicit instead of silently wrong under the default assumption.

- **doc-comment** — `30-topology.plato`: `WindingOrder` should mention the
  reflection/negative-determinant interaction in one sentence — the most
  common runtime source of "suddenly inverted" meshes after mirroring.
