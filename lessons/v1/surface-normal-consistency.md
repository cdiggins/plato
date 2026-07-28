---
lesson: surface-normal-consistency
title: Surface Normal Consistency
domain: Meshes & spatial structures
v3-files: [32-mesh-attributes.plato, 24-surfaces.plato, 20-concepts-curves-surfaces.plato]
audience: Comfortable with triangle meshes and the cross product as a way to get a perpendicular; no rendering experience required
status: draft-v1
---

# Surface Normal Consistency

Flip one triangle’s winding in a mesh and suddenly a smooth statue grows a
black fleck — a normal pointing the wrong way. Offset a surface “outward” and
half of it caves inward instead. **Normal consistency** means every local
orientation agrees with a single global choice of “outside” (or with the
parametric right-hand rule), so lighting, culling, and offsets all tell the
same story.

## The idea

### What a normal is

At a point on an oriented surface, a **unit normal** is a direction
perpendicular to the tangent plane. Two choices exist: $n$ and $-n$. Picking
one consistently is an orientation.

For a parametric patch $\mathbf{r}(u,v)$,

$$
n = \mathrm{normalize}\bigl(\mathbf{r}_u \times \mathbf{r}_v\bigr)
$$

— the right-hand rule applied to the parameter axes. Swap $u$ and $v$, or
negate one partial, and $n$ flips.

For a triangle $(A,B,C)$ with the same rule:

$$
n = \mathrm{normalize}\bigl((B-A)\times(C-A)\bigr)
$$

Winding $A\to B\to C$ vs $A\to C\to B$ is exactly the $n$ vs $-n$ choice.

```
      C                         C
     / \                       / \
    / n \   winding ABC       /(-n)\  winding ACB
   A-----B                   A-----B
```

### Face normals vs vertex normals

- **Per-face:** one $n$ for the whole triangle — faceted look; hard edges
  everywhere.
- **Per-vertex:** average (often area- or angle-weighted) of adjacent face
  normals — smooth shading; washes out intended creases.
- **Per-corner (face-vertex):** different normals at the same geometric vertex
  for different faces — the representation that can be smooth on one side of
  an edge and hard on the other.

### Consistency checks

1. **Adjacent faces:** shared edge should see opposite windings if both faces
   use outward normals on a closed shell (manifold check).
2. **Parametric seams:** when $u=0$ and $u=1$ identify on a closed cylinder,
   normals must meet — `ClosedU`/`ClosedV` surfaces still need matching
   orientation across the seam.
3. **Offsets:** moving along $+n$ by distance $d$ is only “outward” if $n$ was
   outward everywhere; mixed signs produce self-intersections.

Tangent frames for normal mapping add a further consistency demand: tangent,
bitangent, and normal must form a predictable handedness, recorded explicitly
when UV mirrors flip the frame.

## In Plato

Parametric surfaces declare analytic normals on `DifferentiableSurface`.
Meshes store normals as attribute channels with an explicit domain.

From `20-concepts-curves-surfaces.plato` (surface concepts used by file 24):

```plato
concept DifferentiableSurface
    inherits ParametricSurface
{
    TangentUAt(x: Self, uv: UvCoordinate): Vector3D;
    TangentVAt(x: Self, uv: UvCoordinate): Vector3D;
    NormalAt(x: Self, uv: UvCoordinate): Direction3D;
}
```

From `24-surfaces.plato`:

```plato
type OffsetSurface
    implements ParametricSurface
{
    Base: ParametricSurface;
    Distance: Number;
}

type ExtrudedSurface
    implements ParametricSurface
{
    Profile: Curve3D;
    Direction: Direction3D;
    Distance: Number;
}

type HeightField
    implements HeightFieldSurface
{
    Heights: Array2D<Number>;
    Domain: Bounds2D;
}
```

From `32-mesh-attributes.plato`:

```plato
type AttributeDomain = PerVertex | PerFace | PerCorner | PerEdge | Uniform;

type MeshAttribute<T>
{
    Name: String;
    Domain: AttributeDomain;
    Values: Array<T>;
}

type TangentBasis
    implements Value
{
    Tangent: Direction3D;
    Bitangent: Direction3D;
    Normal: Direction3D;
    Handedness: Number;
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

Usage-shaped sketches:

```plato
// Analytic normal from a differentiable patch
n = patch.NormalAt(UvCoordinate { U: 0.3; V: 0.7 });

// Shell expanded "outward" — only correct if Base normals are consistent
shell = OffsetSurface {
    Base: patch;
    Distance: 0.05;    // positive follows base orientation
};

// Faceted face normals as a channel
faceNormals = MeshAttribute<Vector3D> {
    Name: "normal";
    Domain: PerFace;
    Values: onePerTriangle;
};

// Smooth shading with hard-edge seams: PerCorner, not PerVertex
cornerNormals = MeshAttribute<Vector3D> {
    Name: "normal";
    Domain: PerCorner;
    Values: onePerCorner;
};

rich = RichMesh3D {
    Mesh: mesh;
    NumberChannels: [];
    IntegerChannels: [];
    VectorChannels: [cornerNormals];
    UvChannels: [uv0];
    ColorChannels: [];
    TangentChannels: [tangents];
};

// Mirrored UV island: Handedness = -1
basis = TangentBasis {
    Tangent: t;
    Bitangent: b;
    Normal: n;
    Handedness: -1.0;
};
```

Doc comments on `RichMesh3D` recommend the well-known channel name `"normal"`
as `PerVertex` or `PerCorner` `Vector3D` values. Prefer `Direction3D` when you
mean unit length; the channel type is `Vector3D` today, so normalization is a
consumer responsibility.

`OffsetSurface` implements `ParametricSurface` but not necessarily
`DifferentiableSurface` in the declaration — its normal is implied to follow
the base orientation for positive `Distance`, which only helps if that base
orientation is already coherent.

## Pitfalls / fine print

- **Averaging unit normals ≠ normalizing the average.** Weight, sum, then
  normalize once. Averaging already-normalized vectors with equal weight
  under-represents large faces unless you area-weight.
- **Zero area faces.** Degenerate triangles produce undefined face normals;
  they poison vertex averages if not excluded.
- **`PerVertex` hides hard edges.** A cube with per-vertex normals looks
  spherical-ish at the corners. Use `PerCorner` (or split vertices) for boxes.
- **Two-sided materials mask bugs.** A flipped island may still draw with
  two-sided lighting while offsets and ray hits fail.
- **Height fields.** Upward normal consistency is usually “$+Z$ component
  positive” for graphs $z=h(x,y)$; flipping the domain parameterization can
  still invert $n$.
- **Handedness vs winding.** `TangentBasis.Handedness` records UV mirror
  flips; it does not repair inconsistent mesh winding by itself.

## Try it

1. Triangle $(0,0,0)$, $(1,0,0)$, $(0,1,0)$. What is the unit face normal with
   ABC winding? What if you swap B and C?
2. Why does `AttributeDomain.PerCorner` exist if `PerVertex` is smaller?
3. `OffsetSurface` with `Distance: -0.05` on a consistently oriented sphere —
   does the result grow or shrink?

<details>
<summary>Answers</summary>

1. $(B-A)\times(C-A) = (1,0,0)\times(0,1,0) = (0,0,1)$. Swapping B and C
   yields $(0,0,-1)$.
2. So one geometric vertex can carry different normals on different faces —
   hard edges / UV seams — without duplicating positions in the base mesh
   connectivity (values live in the attribute channel).
3. Shrinks (moves opposite the base outward orientation), remaining a sphere
   of smaller radius if $|d|$ is below the original radius.

</details>

## Library recommendations

- **missing-function** — `32-mesh-attributes.plato`: no
  `ComputeFaceNormals(mesh: TriangleMesh3D): MeshAttribute<Vector3D>` or
  `OrientConsistent(mesh): TriangleMesh3D` utilities. The lesson’s whole
  point is an operation the vocabulary never names.
- **wrong-shape** — `32-mesh-attributes.plato`: well-known `"normal"` channels
  are documented as `Vector3D`, but normals are unit directions. Prefer
  `MeshAttribute<Direction3D>` (new channel group) or document a hard
  invariant that normal channels must be unit length.
- **missing-concept** — `24-surfaces.plato`: `OffsetSurface` should implement
  or require `DifferentiableSurface` on `Base`, and declare how
  `NormalAt` transforms (same as base for pure normal offset). As written it
  is only `ParametricSurface`, so the consistency story for offsets is
  doc-comment folklore.
- **doc-comment** — `32-mesh-attributes.plato`: `AttributeDomain` should spell
  out length rules for `"normal"` (`PerFace` → face count, `PerCorner` →
  $3\times$ triangle count for triangle meshes). Authors guess wrong and
  desynchronize channels from `Mesh`.
- **missing-function** — `32-mesh-attributes.plato`: no
  `FlipNormals(attribute)` / `AlignHandedness` helpers; mirrored UV workflows
  always reinvent them beside `TangentBasis.Handedness`.
