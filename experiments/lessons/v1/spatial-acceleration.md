---
lesson: spatial-acceleration
title: Spatial Acceleration
domain: Meshes & spatial structures
v3-files: [34-spatial-structures.plato, 35-spatial-queries.plato]
audience: High-school math and general programming background; comfort with trees and arrays
status: draft-v1
---

# Spatial Acceleration

Check every triangle against every ray and your frame time dies with the square of scene
size. The fix is always the same idea in different costumes: group nearby primitives under
nested bounds, reject whole groups with one cheap test, and only then run the expensive
exact test on survivors. Bounding volume hierarchies, octrees, kd-trees, and uniform
grids are those costumes. Plato's v3 spatial layer stores them as flat node arrays plus
permutation indices into *your* primitive list, and pairs them with explicit query and
hit records so raycasts and neighbor searches are ordinary immutable data.

## The idea

**Brute force** for $n$ objects and $q$ queries costs about $O(nq)$ exact tests.
**Acceleration** aims for something closer to $O(n\log n)$ build and $O(q\log n)$ query
(with many caveats).

Common structures:

**BVH (bounding volume hierarchy)** — binary tree of axis-aligned boxes. Internal nodes
bound two children; leaves hold a small set of primitives. Excellent for ray tracing
static geometry.

**Quadtree / octree** — regularly subdivide a square/cube into 4 or 8 equal children until
depth or occupancy limits. Simple, good for points and dynamic insertions (especially
**loose** octrees with enlarged node bounds).

**Kd-tree** — axis-aligned splitting planes chosen from the data (often median). Classic
for nearest-neighbor in point sets.

**Uniform / hash grids** — hash world position into cells; only search nearby buckets.
Superb when objects are similarly sized and density is moderate; painful when sizes vary
wildly.

```
  BVH idea:                         Grid idea:
      [ root bounds ]                 +---+---+---+
       /           \                  | a |   | b |
    [L]           [R]                 +---+---+---+
    / \           / \                 |   |a,c|   |
  leaves hold primitive ranges        +---+---+---+
```

Shared implementation conventions in Plato:

1. Nodes live in one array; children are `SpatialNodeIndex` ($-1$ = none). Node 0 is root.
2. The structure does **not** own primitives — it stores `Array<ItemIndex>` permutations
   into the caller's list.
3. Leaves reference half-open `IntegerInterval` ranges of that permutation.
4. Broad-phase queries may return **false positives**; the caller runs exact tests.

Queries are first-class values: raycast, sphere sweep, $k$-nearest, radius search,
overlap, closest point, frustum cull.

## In Plato

Structures (`34-spatial-structures.plato`):

```plato
type BvhNode3D
{
    Bounds: Bounds3D;
    LeftChild: SpatialNodeIndex;
    RightChild: SpatialNodeIndex;
    Primitives: IntegerInterval;   // empty for internal nodes
}

type Bvh3D
    implements Value, SpatialIndex3D, RayIntersectable3D
{
    Nodes: Array<BvhNode3D>;
    PrimitiveIndices: Array<ItemIndex>;
}

type Octree3D
    implements Value, SpatialIndex3D
{
    Bounds: Bounds3D;
    Nodes: Array<OctreeNode>;
    ItemIndices: Array<ItemIndex>;
    MaxDepth: Integer;
}

type LooseOctree3D
{
    Octree: Octree3D;
    Looseness: Number;   // 1 = tight; 2 common for movers
}

type KdTree3D
    implements Value, SpatialIndex3D, NearestNeighborQueryable3D
{
    Points: Array<Point3D>;
    Nodes: Array<KdTreeNode3D>;
    PointIndices: Array<ItemIndex>;
}

type SpatialHashGrid3D
    implements Value, SpatialIndex3D
{
    CellSize: Number;
    CellCoordinates: Array<IntegerVector3>;
    BucketOffsets: Array<Integer>;   // CSR
    ItemIndices: Array<ItemIndex>;
}
```

Index concept (candidate generation):

```plato
concept SpatialIndex3D
{
    ItemCount(x: Self): Integer;
    CandidatesInBounds(x: Self, bounds: Bounds3D): Array<ItemIndex>;
}
```

Morton codes support linear BVH builds and cache-friendly sorts:

```plato
type MortonCode3D
{
    Value: Integer;   // interleaved X,Y,Z bits
}
```

Queries and results (`35-spatial-queries.plato`):

```plato
type RayQuery3D
{
    Ray: Ray3D;
    MaxDistance: Number;   // <= 0 means unbounded
    FilterMask: Integer;   // -1 accepts all categories
}

type RayHit3D
{
    Hit: Boolean;
    Distance: Number;
    Position: Point3D;
    Normal: Direction3D;
    Face: FaceIndex;
    Barycentric: BarycentricCoordinate;
    Uv: UvCoordinate;
}

type KNearestQuery3D
{
    Center: Point3D;
    NeighborCount: Integer;
    MaxDistance: Number;
}

type NearestNeighbors
{
    Indices: Array<ItemIndex>;
    Distances: Array<Number>;   // ascending
}

concept RayIntersectable3D
{
    Raycast(x: Self, query: RayQuery3D): RayHit3D;
}

concept NearestNeighborQueryable3D
{
    FindNearest(x: Self, query: KNearestQuery3D): NearestNeighbors;
}
```

Usage-shaped snippets:

```plato
hit = Raycast(bvh, RayQuery3D(ray, 1000, -1))
if hit.Hit
    p = hit.Position
    n = hit.Normal

candidates = CandidatesInBounds(grid, queryBox)
// exact overlap test per candidates[i] against caller's primitives

nn = FindNearest(kd, KNearestQuery3D(p, 8, 0))
// nn.Indices[0] is nearest; Distances parallel
```

Closest-point and overlap results round out the toolkit (`ClosestPointResult3D`,
`OverlapQuery3D`, `Containment`, `FrustumCullResult`).

## Pitfalls / fine print

**False positives are normal.** `CandidatesInBounds` may return items whose exact geometry
misses the box. Always narrow-phase. Designing APIs that imply exactness here causes
subtle bugs.

**Empty interval.** `Start == End` means no items. Do not special-case only "zero count"
without checking the interval encoding.

**MaxDistance ≤ 0 means unbounded** for ray and neighbor queries. Passing $0$ thinking
"zero length ray" silently means "infinite."

**FilterMask.** Combined with candidate category bits by bitwise AND; non-zero passes.
$-1$ (all bits set) accepts everything. A mask of $0$ rejects everything — another silent
footgun.

**BVH leaf vs internal.** Internal nodes use empty `Primitives` ranges and real children;
leaves use $-1$ children and a non-empty range. Walking the tree must branch on that
convention.

**Octree child packing.** `FirstChild` indexes four (quad) or eight (oct) *consecutive*
nodes. The ordering is documented (2D: $-X-Y$, $+X-Y$, $-X+Y$, $+X+Y$; 3D: X fastest,
then Y, then Z). Wrong octant math sends queries into empty space.

**Kd-tree owns points.** `KdTree3D` stores a copy of `Points`. Updating positions means
rebuilding (or accepting stale queries). BVHs over mesh triangles usually share the mesh
and only permute indices.

**Grid cell size.** Too large → many objects per bucket (slow). Too small → many buckets
touched per query (slow). Match cell size to typical object diameter.

**Hit fields when miss.** If `RayHit3D.Hit` is false, remaining fields are meaningless —
do not read `Distance` as infinity unless your code sets that convention itself.

**Build quality.** A BVH is only as good as its split heuristic (SAH, etc.). v3 declares
the *data shape*, not the builder; two BVHs with identical leaf sets can differ wildly in
trace cost.

## Try it

1. A BVH leaf has `Primitives = [4, 7)` into `PrimitiveIndices`. How many primitives does
   it reference, and how do you get the caller's index of the first one?
2. Why might a loose octree with `Looseness = 2` help moving objects more than a tight
   octree?
3. `KNearestQuery3D` asks for 5 neighbors with `MaxDistance = 10`. You get 3 results.
   What happened?

<details>
<summary>Answers</summary>

1. Three primitives (indices 4, 5, 6). First caller's index is `PrimitiveIndices[4]`.
2. Enlarged node bounds let each object sit entirely inside one node despite motion,
   reducing expensive reinsertions across boundaries; tight octrees churn as objects
   cross faces.
3. Only three points existed within distance 10; `NeighborCount` is a maximum, not a
   guarantee.

</details>

## Library recommendations

- **missing-function** — `34-spatial-structures.plato`: structures implement
  `SpatialIndex3D` / `RayIntersectable3D` but there are no declared builders
  (`BuildBvh`, `BuildKdTree`, …). Teaching acceleration without a build contract leaves
  the cost model and invalidation rules underspecified.

- **missing-function** — `35-spatial-queries.plato`: `RadiusQuery3D` exists as a request
  type, but no concept method `FindInRadius` parallels `FindNearest` on
  `NearestNeighborQueryable3D`. The query record is stranded without a capability.

- **wrong-shape** — `35-spatial-queries.plato`: `RayHit3D` always carries `Face`,
  `Barycentric`, and `Uv`, with sentinels when inapplicable. For BVH hits against non-mesh
  primitives those fields are noise. A sum type (`Miss | MeshHit(...) | PrimHit(...)`)
  would match the tagged-variant preference in the v3 README.

- **doc-comment** — `34-spatial-structures.plato`: `BinningGrid3D` cell linearization
  (X fastest, then Y, then Z) should be repeated on `SpatialHashGrid3D` consumer notes —
  hash grids use integer coordinates, not linearized dense buckets, and readers conflate
  the two CSR layouts.
