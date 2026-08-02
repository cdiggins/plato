---
lesson: mesh-normals
title: Mesh Normals
domain: Meshes & spatial structures
v3-files: [31-meshes.plato, 32-mesh-attributes.plato]
audience: High-school math and general programming background; vectors and cross products helpful
status: draft-v1
---

# Mesh Normals

Flat-shaded faceted look versus smooth plastic look is rarely a different mesh. It is
almost always a different **normal** channel: one normal per face, or averaged normals at
shared vertices, or unique normals at corners so a crease can stay sharp. Lighting
equations care about the normal at the shaded point, not about the geometric plane of a
triangle alone. Plato separates the indexed mesh (`TriangleMesh3D`) from attribute
channels (`RichMesh3D`, `AttributeDomain`) so you can store whichever normal layout the
asset needs without changing connectivity.

## The idea

A triangle $A,B,C$ (counter-clockwise from the front) has a geometric **face normal**:

$$
N_{\text{face}} = (B-A) \times (C-A)
$$

Normalize for lighting. That normal is constant across the face — flat shading.

**Vertex normals** for smooth shading assign a direction to each shared vertex, usually
by averaging the normals of incident faces. Common weightings:

- **Uniform** — average unit face normals (sensitive to tessellation density).
- **Area-weighted** — weight by triangle area (large faces pull harder).
- **Angle-weighted** — weight by the corner angle at that vertex (better on irregular
  meshes).

At shade time you interpolate vertex normals across the triangle (barycentric), then
renormalize. The interpolated direction is not the normal of any single face — that is
what makes Gouraud/Phong-style shading look smooth.

**Hard edges** need a seam in the normal field. If two faces share a vertex index but
should not share a normal, you either:

- split the vertex (duplicate position, different attributes), or
- store normals **per corner** (one value per face-vertex slot) so the same `VertexIndex`
  can carry different normals in different faces.

```
  Smooth: one normal at v          Hard edge: two corner normals at v
        n_avg                            n1 ≠ n2
         ↑                                ↑   ↑
      /  |  \                          /  |  \
     /   v   \                        /   v   \
```

Tangent frames for normal mapping extend the story: at each shading point you need an
orthonormal basis (tangent, bitangent, normal) plus a handedness sign for mirrored UVs.

## In Plato

The mesh itself (`31-meshes.plato`) stores only positions and faces:

```plato
type TriangleFace
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

Winding is counter-clockwise when viewed from the front (`WindingOrder.CounterClockwise`
is the library default). Face records never contain $-1$.

Attributes live next door (`32-mesh-attributes.plato`):

```plato
type AttributeDomain = PerVertex | PerFace | PerCorner | PerEdge | Uniform;

type MeshAttribute<T>
{
    Name: String;
    Domain: AttributeDomain;
    Values: Array<T>;
}

type RichMesh3D
    implements Value, Meshable3D
{
    Mesh: TriangleMesh3D;
    NumberChannels: Array<MeshAttribute<Number>>;
    IntegerChannels: Array<MeshAttribute<Integer>>;
    VectorChannels: Array<MeshAttribute<Vector3D>>;
    UvChannels: Array<MeshAttribute<UvCoordinate>>;
    ColorChannels: Array<MeshAttribute<Color>>;
    TangentChannels: Array<MeshAttribute<TangentBasis>>;
}
```

Well-known channel name `"normal"` is documented as `PerVertex` or `PerCorner`
`Vector3D` (often treated as directions after normalize). Face normals naturally use
`PerFace`.

Tangent space:

```plato
type TangentBasis
{
    Tangent: Direction3D;
    Bitangent: Direction3D;
    Normal: Direction3D;
    Handedness: Number;   // +1 or -1
}
```

Usage-shaped snippets:

```plato
mesh: TriangleMesh3D
face = FaceAt(mesh, FaceIndex(i))
pA = PositionAt(mesh, face.A)
pB = PositionAt(mesh, face.B)
pC = PositionAt(mesh, face.C)
// geometric face normal ~ (pB - pA) × (pC - pA)  — Cross not yet on Vector interface

rich.VectorChannels[0] = MeshAttribute(
    "normal",
    PerVertex,
    vertexNormals)      // length == vertex count

rich.VectorChannels[1] = MeshAttribute(
    "normal",
    PerCorner,
    cornerNormals)      // length == 3 * triangle count
```

`UvSeam` / `UvAtlas` bookkeeping marks where UV discontinuities force attribute splits —
the same edges often want hard geometric normals too.

## Pitfalls / fine print

**Winding and back faces.** If winding is wrong, face normals point inward and lighting
inverts. Plato's contract is CCW from the outside; importers that use CW must reverse
or flip.

**Normalize after interpolate.** Lerping unit normals does not preserve length. Always
normalize the interpolated result before lighting (unless using a technique that
explicitly works with non-unit vectors).

**Area vs angle weights.** Uniform averages bias toward finely tessellated regions.
Area weights bias toward large triangles. Angle weights are usually the best default for
organic meshes; none of these are declared as library functions yet — see recommendations.

**Per-vertex cannot encode a crease.** A single normal at a shared vertex cannot be both
the left-face and right-face direction. Creases require `PerCorner` normals or split
vertices. Splitting breaks position sharing and can hurt memory; corner attributes keep
topology shared.

**Channel length contracts.** `PerVertex` ⇒ `Values.Count == Positions.Count`.
`PerFace` ⇒ face count. `PerCorner` ⇒ for triangles, $3 \times$ face count (corner $c$
belongs to triangle $c/3$). `Uniform` ⇒ exactly one value. Mismatches are silent data bugs
until something crashes later.

**Normal vs `Direction3D`.** The well-known `"normal"` channel is typed as `Vector3D` in
the doc comment, while `TangentBasis.Normal` is `Direction3D`. Be consistent about
normalization when converting between them.

**Degenerate faces.** Zero-area triangles have undefined face normals (cross product
vanishes). Skip or repair before averaging into vertex normals.

**Handedness.** Mirrored UV islands flip the bitangent relative to $N \times T$.
`Handedness` of $-1$ records that; ignoring it mirrors normal maps.

## Try it

1. Triangle with vertices $(0,0,0)$, $(1,0,0)$, $(0,1,0)$. What is the unit face normal
   under CCW winding?
2. Two coplanar triangles share an edge. Should their area-weighted vertex normals on
   that edge match the face normal?
3. You need a sharp cube edge. Do you store `"normal"` as `PerVertex` or `PerCorner`?

<details>
<summary>Answers</summary>

1. $(B-A)\times(C-A) = (1,0,0)\times(0,1,0) = (0,0,1)$ — already unit.
2. Yes — both faces share the same normal direction, so any average of that direction
   (with positive weights) recovers the same unit normal.
3. `PerCorner` (or split vertices). `PerVertex` would average the two face normals and
   round the edge.

</details>

## Library recommendations

- **missing-function** — `31-meshes.plato` / `08-vectors.plato`: computing a face normal
  needs a cross product, but `Vector3D` / the `Vector` interface do not declare `Cross`.
  The mesh-normals lesson cannot show an idiomatic one-liner without noting the gap.

- **missing-function** — `31-meshes.plato` or `32-mesh-attributes.plato`: no
  `FaceNormals(mesh)`, `VertexNormals(mesh, weighting)`, or weighting enum
  (uniform / area / angle). Every engine reimplements this; the attribute file documents
  where to *store* normals but not how to *author* them from topology.

- **naming** — `32-mesh-attributes.plato`: well-known channel `"normal"` is described as
  `Vector3D` while `TangentBasis` uses `Direction3D` for the same geometric role. Prefer
  one representation (or an explicit doc rule: store non-unit in channels, normalize at
  use) so smooth-shading code does not guess.

- **doc-comment** — `32-mesh-attributes.plato`: `AttributeDomain.PerCorner` should state
  the indexing rule for triangle meshes (corner $c$ → face $c/3$, slot $c \bmod 3$),
  matching `CornerIndex` in `30-topology.plato`. Without that, channel length checks are
  folklore.

> Resolved 2026-07-28 (vector part): `Cross(Vector3D,Vector3D)` exists in intrinsics.plato and is now advertised by the discoverability banner in vectors.plato, so the idiomatic face-normal one-liner has a discoverable home (item 236 vector part, stdlib commit pending).
