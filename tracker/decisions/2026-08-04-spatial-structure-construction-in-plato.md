---
date: 2026-08-04
title: Spatial-structure construction belongs in Plato
status: accepted
superseded-by:
links: [plato-442, plato-439, plato-378, plato-368]
---

## Context

The spatial structures ship as declared types plus full query surfaces —
`Bvh3D`, `Octree3D`, `KdTree2D/3D`, `ConvexHull2D/3D` — with nothing in the
tree that constructs one (plato-442). Rebuilding the geometry-samples demo
(plato-439) forced octree, BVH, hull and Delaunay construction to stay in
TypeScript. plato-442 asked for a recorded decision: are these builders
expressible in the pure vocabulary at all, or are the flat-array structures
meant to be built by a host and only queried in Plato?

## Decision

Construction belongs in Plato, as ordinary `*.library.plato` bodies. Nothing
about these types is host-only. Two of the issue's three "missing
prerequisites" already exist; the third (ordering) is expressible today and
lands as a library function, not an intrinsic.

Specifically:

1. **Ordering is a library function.** `SortedIndices` / `Sort` are written
   with the existing procedural vocabulary (bottom-up merge passes over
   `Buffer`, no recursion), so under the plato-378 admission rule they must
   NOT join `intrinsics.library.plato`. Backends recover O(n log n) native
   speed through the override table (plato-368), the same route as every
   other derived function.
2. **Accumulate-and-patch already exists.** The issue's second prerequisite
   ("no way to grow a tree of variable arity") is false as stated: bodies may
   use `var`, `while`, `if`, `new List<T>()` / `new Buffer<T>(n)`, `Add` and
   `Set` — the ear-clipping triangulator (`triangulation.library.plato`) is
   exactly an accumulate-and-patch loop over a node pool. Tree builders use
   an explicit work stack (a `List` of pending ranges) instead of recursion,
   which also keeps them inside the no-recursion contract GLSL requires.
3. **Delaunay stays deferred.** Bowyer–Watson needs multiset/cancellation
   vocabulary that has no owner yet; it is out of scope for plato-442 and
   waits for a concrete consumer.

## Rationale

- The corpus already crossed this bridge: ear clipping is a more intricate
  pointer-patching pass than a median-split BVH build, and it lives in the
  stdlib with laws over it.
- The types themselves anticipate in-Plato construction: the Morton-code doc
  comment calls the codes "the basis of linear BVH builds", and the flat
  node-and-permutation encoding is precisely the shape a builder emits.
- A host-builds-only rule would make every backend (C#, C++, TypeScript,
  GLSL...) reimplement each structure, which is the situation plato-439
  demonstrated and the stdlib exists to end.

## Alternatives rejected

- **Host constructs, Plato queries** — multiplies per-backend work, leaves
  the demo split (TS builders against Plato types), and contradicts the
  reference-body doctrine (plato-378) that keeps the intrinsic surface
  minimal.
- **Sort as a bodiless intrinsic** — violates the plato-378 admission rule,
  since a portable reference body exists; the override table already exists
  to recover native speed.

## Consequences

- `SortedIndices(xs, ...)` / `Sort` land in the foundation tier; the hull,
  BVH and kd builders consume the permutation form (they must map back to
  source indices).
- `ConvexHull2D` gets a monotone-chain constructor; `Bvh3D` a median-split
  builder; `Octree3D` a point-set builder — all pure Plato, work-stack style,
  documented as O(n log n)-shaped reference bodies with speed recovered per
  backend via plato-368.
- Query-only types that still lack a builder (`KdTree2D/3D`, `ConvexHull3D`,
  Delaunay) say so in their doc comments: instances come from a host or a
  future builder, with this decision as the reference.
- The O(n²) insertion alternative for sorting was rejected inside the
  reference body too: merge passes are while-expressible, stable, and honest
  about cost.
