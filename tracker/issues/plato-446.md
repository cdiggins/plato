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
