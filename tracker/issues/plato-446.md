---
id: plato-446
title: "CornerTwinTable is quadratic in corner count: TopologyOf costs ~1s for a 1280-face mesh"
type: debt
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439]
---

## Problem

`TwinCorner` searches every corner of the mesh for the one whose directed edge is the
reverse of the query's. `CornerTwinTable` runs it once per corner, so building the
twin table is quadratic in corner count, and `TopologyOf` — which every remeshing
pass starts from — inherits that. `remeshing.library.plato` says so at the section
head ("Cost: quadratic in the corner count, and it dominates every pass built on
it"), so this is a known property rather than a surprise; what is new is a
measurement of what it costs a consumer.

Measured from generated TypeScript while rebuilding geometry-samples (plato-439) on a
level-3 icosphere — 642 vertices, 1280 faces, 3840 corners. Reproduce with:

```js
const m = noisySphere();            // src/samples/halfEdge.ts
const t = Date.now(); m.TopologyOf(); Date.now() - t;
```

At that size a single `TopologyOf` is the dominant cost of the whole sample, and a
smoothing loop that rebuilt it per iteration took two minutes for twelve iterations.
The sample now builds the rings once and passes them down, which is what the library
comment advises, and that is the correct workaround — but it means every consumer has
to know.

## Approach

Group directed edges by their unordered vertex pair and pair up within each group.
`SortedFaceKey` already exists for the analogous face problem. Without a sort or a
hash primitive (plato-442 covers the missing ordering vocabulary), the grouping needs
either a bucket over vertex index — an `Array<List<CornerIndex>>` sized by vertex
count, which the affine builders can express — or the hash structure that arrives with
whatever plato-442 decides.

Worth measuring the C# backend separately before assuming the cost is uniform: the
quadratic is in the Plato body, but the constant factor is the backend's.

## Done means

- [ ] `CornerTwinTable` is linear or near-linear in corner count.
- [ ] A timing recorded here for the same level-3 icosphere, before and after.
- [ ] The "build it once and pass it down" advice in `remeshing.library.plato` kept,
      since it stays good practice, but no longer load-bearing.

## Update 2026-08-04 (plato-442 closed — the sort exists)

The Approach section above is stale where it says "Without a sort or a hash
primitive (plato-442 covers the missing ordering vocabulary)". plato-442 closed
the same day and ordering landed as a library reference body:
`SortedIndices(xs, lessOrEqual): Array<Integer>` and `Sort(xs, lessOrEqual)` in
`stdlib/foundation/sorting.library.plato` (bottom-up merge over an index
`Buffer`, stable, recursion-free).

So the grouping no longer waits on a decision. `SortedIndices` over the directed
edges keyed by their unordered vertex pair gives the grouping directly in
O(n log n), and the permutation form is the one that maps back to corner
indices. The bucket-over-vertex-index alternative sketched above is still a valid
choice and may be the cheaper one, but it is now a choice between two available
routes rather than a workaround for a missing primitive.

Unchanged: the measurement, the quadratic itself, and every `Done means` item.
There is still no hash primitive, so if a design wants hashing rather than
ordering, that gap is real.
