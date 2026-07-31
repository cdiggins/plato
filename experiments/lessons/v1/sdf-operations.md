---
lesson: sdf-operations
title: SDF Operations
domain: Fields, implicits & noise
v3-files: [27-implicit-sdf.plato]
audience: High-school math and general programming background; familiarity with the idea of a signed distance field
status: draft-v1
---

# SDF Operations

Boolean modeling on triangle meshes is expensive and fragile: union two solids and you
must clip faces, stitch edges, and repair non-manifold wreckage. On a signed distance
field the same operations collapse to a few comparisons. If $d_A$ and $d_B$ are the
signed distances to two shapes, then $\min(d_A, d_B)$ is the distance to their union,
$\max(d_A, d_B)$ is the intersection, and $\max(d_A, -d_B)$ is $A$ with $B$ carved out.
That algebraic simplicity is why SDFs dominate real-time raymarching and procedural
solid modeling — and why Plato's v3 vocabulary treats combination as a first-class tree
of named operations, not an afterthought.

## The idea

A signed distance field reports how far a point is from a surface: negative inside,
zero on the boundary, positive outside. Exact distances are ideal; many practical SDFs
only guarantee a **lower bound** (still enough for conservative raymarching).

Constructive solid geometry (CSG) builds complex shapes from primitives. For exact SDFs
the Boolean operations are:

| Operation | Formula | Intuition |
|-----------|---------|-----------|
| Union | $\min(d_A, d_B)$ | Closest surface wins |
| Intersection | $\max(d_A, d_B)$ | Farthest (most restrictive) wins |
| Difference $A \setminus B$ | $\max(d_A, -d_B)$ | Invert $B$, then intersect |
| Exclusive-or | $\max(\min(d_A,d_B), -\max(d_A,d_B))$ | In one but not both |

```
  d_A = -2   d_B =  1     min = -2  (inside A → inside union)
  d_A =  3   d_B =  1     min =  1  (outside both → dist to nearer)
  d_A = -2   d_B = -1     max = -1  (inside both → dist to nearer wall)
  d_A = -2   d_B =  1     max(d_A,-d_B) = max(-2,-1) = -1  (A minus B)
```

Hard min/max produce sharp creases where the two surfaces meet. **Smooth** variants blend
over a radius $k$: near the interface they soften the corner instead of leaving a
dihedral edge. Polynomial smooth-min (often called `smin`) is one common blend; the
details vary, but the geometric contract is the same — a $C^1$ transition whose blend
width is controlled by $k$.

Linear **blend** is different again: $$(1-w)\,d_A + w\,d_B$$ interpolates the fields
themselves. That is useful for morphing shapes, but it is not a Boolean and the zero
level set of the blend is not the blend of the level sets.

Metaballs take another route: sum radial falloff kernels and take a level set. The
result looks organic and "blobby" without an explicit CSG tree.

## In Plato

v3 encodes SDFs as concepts that refine scalar fields, then stores CSG as flat trees of
combination nodes. From `27-implicit-sdf.plato`:

```plato
concept SignedDistanceField3D
    inherits ScalarField3D
{ }

type SdfCombine
    = Union
    | Intersection
    | Difference
    | ExclusiveOr
    | SmoothUnion(Radius: Number)
    | SmoothIntersection(Radius: Number)
    | SmoothDifference(Radius: Number)
    | Blend(Weight: Proportion);
```

A tree never embeds child nodes recursively. Leaves point at an external primitive list
by `ItemIndex`; interior nodes point at earlier nodes by `SdfNodeIndex`. Node 0 can be
anything; `Root` names the result. Operands always reference **lower** indices — the
array is already in topological order.

```plato
type SdfNode3D
    implements Value
    = Leaf(Primitive: ItemIndex)
    | Interior(Combine: SdfCombine, Left: SdfNodeIndex, Right: SdfNodeIndex);

type SdfTree3D
    implements Value
{
    Nodes: Array<SdfNode3D>;
    Root: SdfNodeIndex;
}
```

Usage-shaped evaluation (illustrative — bodies are not in v3 yet):

```plato
// primitives[0] = sphere SDF, primitives[1] = box SDF
tree.Nodes[0] = Leaf(0)
tree.Nodes[1] = Leaf(1)
tree.Nodes[2] = Interior(SmoothUnion(0.2), 0, 1)
tree.Root = 2

d = Eval(primitives[0], p)   // via ScalarField3D / Procedural
// combined distance at p walks the tree with min/max/smooth per SdfCombine
```

Modifiers are separate parameter records applied to a source SDF at evaluation time.
They reshape distance without changing the combination tree:

```plato
type SdfRoundingModifier { Radius: Number; }      // inflate / fillet
type SdfShellModifier    { Thickness: Number; }   // hollow shell
type SdfOnionModifier    { Thickness: Number; Count: Integer; }
type SdfTwistModifier3D  { AnglePerUnit: Angle; }
type SdfBendModifier3D   { Curvature: Number; }
type SdfDisplacementModifier3D
{
    Source: ItemIndex;   // external ScalarField3D
    Amplitude: Number;   // result is a bound, not exact distance
}
```

Metaballs implement `ScalarField3D` directly (not necessarily a true SDF):

```plato
type MetaBallSystem3D
    implements Value, ScalarField3D
{
    Balls: Array<MetaBall3D>;
    Threshold: Number;
}
```

Sampled grids store distances on a lattice when you need to bake an analytic field:

```plato
type SampledSdf3D
    implements Value, SignedDistanceField3D
{
    Values: Array3D<Number>;
    Bounds: Bounds3D;
}
```

## Pitfalls / fine print

**Lipschitz / bound vs exact.** After displacement, elongation, or some smooth blends,
the field may only be a **bound** on true distance. Raymarchers that step by `d` then
overshoot. Prefer exact primitives near the camera; treat displaced results conservatively.

**Order of difference.** `Difference` is left minus right: $A \setminus B$, not
symmetric. Swapping children inverts which shape is the cutter.

**Smooth radius units.** `SmoothUnion(Radius)`'s radius is in the same world units as
the distances. Too large and the blend swallows whole features; too small and you
might as well use hard `Union`.

**Blend is not smooth-union.** `Blend(Weight: Proportion)` lerps distances. A weight of
$1/2$ does not mean "halfway Boolean." Use it for morphs; use `SmoothUnion` for soft CSG.

**ExclusiveOr.** XOR of SDFs is less common in modeling UIs. Check that your renderer
and your mental model agree on the resulting topology (often thin shells and cusps).

**Repetition and domain ops.** `SdfRepetitionModifier3D` tiles by folding space. Counts
that are non-positive mean unbounded repetition on that axis — easy to accidentally
create infinite copies that break bounds queries.

**Metaball ≠ SDF.** `MetaBallSystem3D` is a scalar field whose zero set is the blob.
The numeric value is not generally Euclidean distance. Do not plug it into a CSG tree
expecting `min`/`max` distance semantics unless you convert or accept the error.

**2D and 3D trees are parallel.** `SdfTree2D` / `SdfNode2D` mirror the 3D forms. Mixing
a planar leaf into a spatial tree is a type error by construction — keep domains separate.

## Try it

1. Sphere $d_s = \|p - c\| - r$ and half-space $d_h = p\cdot n - b$. Write the SDF for
   the sphere with the half-space cut away (sphere difference half-space).
2. At a point where $d_A = 0.4$ and $d_B = 0.1$, what is hard union? Hard intersection?
3. Why does `SmoothDifference` need a radius while hard `Difference` does not?

<details>
<summary>Answers</summary>

1. $\max(d_s, -d_h)$.
2. Union $\min(0.4, 0.1) = 0.1$; intersection $\max(0.4, 0.1) = 0.4$.
3. Hard difference is a pointwise max with a sign flip — no free parameter. Smooth
   difference blends across a band of width related to the radius, so the radius is
   part of the operation's meaning.

</details>

## Library recommendations

- **missing-function** — `27-implicit-sdf.plato`: `SdfTree2D` / `SdfTree3D` declare the
  tree shape but there is no concept function such as `EvalTree(tree, primitives, point)`
  on the tree types. Teaching CSG evaluation has to invent the walk; a declared evaluator
  (even without a body) would pin the contract for leaf resolution and combine semantics.

- **missing-function** — `27-implicit-sdf.plato`: modifiers (`SdfRoundingModifier`,
  `SdfShellModifier`, …) are parameter records with no concept tying them to
  `SignedDistanceField3D`. A `ModifiedSdf3D { Source: ItemIndex; Modifier: ... }` sum
  type — or concept methods `Round`, `Shell`, `Onion` — would make the apply-step teachable
  instead of "evaluation context supplies the source."

- **doc-comment** — `27-implicit-sdf.plato`: `SdfCombine.Blend` should state explicitly
  that it is linear interpolation of distances, not a smooth Boolean, and that the zero
  set of a blend is not the blend of the zero sets. The pedagogy gap between Blend and
  SmoothUnion is the #1 confusion when reading the sum type.

- **wrong-shape** — `27-implicit-sdf.plato`: `MetaBallSystem3D` implements `ScalarField3D`
  but not `SignedDistanceField3D`, which is correct numerically, yet nothing in the file
  offers a conversion or a warning type. A doc note on `SdfNode3D.Leaf` that primitives
  must be distance-like (not arbitrary scalar fields) would prevent treating metaballs as
  CSG leaves by accident.
