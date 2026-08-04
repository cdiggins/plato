---
id: plato-442
title: "Builders for the spatial structures, hulls and Delaunay: the types and queries ship, nothing constructs them"
type: problem
status: ready
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439]
---

## Problem

Several structures ship as a declared type plus a full set of query bodies, with no
way to build one from data:

| Type | Queries that ship | Missing |
|---|---|---|
| `Bvh3D` / `BvhNode3D` | `Raycast`, `CandidatesInBounds`, `BvhNodeCandidates`, `NearestLeafEntry` | construction from primitives |
| `Octree3D` / `LooseOctree3D` | `OctreeCandidates`, `ItemCount` | construction from points |
| `KdTree2D` / `KdTree3D` | `KdCandidates`, `KdNodeCandidates` | construction |
| `ConvexHull2D` / `ConvexHull3D` | `HullVertexCount`, `SourceOf`, `SourcePoints`, `IsDegenerate`, `HullEulerCharacteristic` | the hull itself |
| Delaunay | — | `triangulation.library.plato` is ear-clipping only |

A consumer can therefore describe one of these structures, and read one that arrives
from elsewhere, but cannot produce one. Found while rebuilding
`demos/typescript/geometry-samples` on the stdlib (plato-439): the octree, BVH,
convex-hull and Delaunay samples all had to keep their construction in TypeScript,
against stdlib types and predicates.

## Why they are not written yet

Two prerequisites are missing, and both are language-level rather than
library-level:

1. **No ordering primitive.** `plato_search_symbols` for `Sort` finds one unrelated
   `SortedFaceKey`. Monotone-chain hull, median-split BVH and kd-tree construction all
   begin by sorting. There is no `Sort(xs, comparer)` and no sorted-permutation
   function anywhere in the tree.
2. **No way to grow a tree of variable arity.** `List<T>` / `Buffer<T>` are affine
   builders over a flat sequence. The flat node-and-index representation these types
   already use (`SpatialNodeIndex` into an `Array<BvhNode3D>`) is the right target,
   but filling it means appending nodes whose children are indices not yet assigned —
   an accumulate-and-patch loop with no expression form today.

Delaunay adds a third: Bowyer-Watson rewrites a triangle SET per insertion, and the
cavity boundary is found by cancelling shared edges — a multiset difference with no
vocabulary here.

## Approach

Ordering first — it is independently useful and unblocks the hull and the median
split. A `Sort` on `Array<T>` with a comparer, or a `SortedIndices` returning the
permutation (which is what the hull actually wants, since it must map back to source
indices), lands as an intrinsic with a reference body.

Tree construction wants a decision recorded before anyone writes code: whether these
builders are expressible in the pure vocabulary at all, or whether the flat-array
structures are meant to be built by a host and only queried in Plato. The types were
declared query-first, which suggests the second, but nothing says so.

## Done means

- [ ] A decision recorded in `tracker/decisions/` on whether spatial-structure
      construction belongs in Plato or in the host.
- [ ] If in Plato: an ordering primitive, then `Bvh3D` and `Octree3D` builders.
- [ ] If in Plato: `ConvexHull2D` gets a monotone-chain constructor, and the
      geometry-samples convex-hull sample calls it.
- [ ] Either way, the query-only types say in their doc comments where instances are
      expected to come from.
