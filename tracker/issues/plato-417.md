---
id: plato-417
title: "Ear-clipping pass is O(n^2); a sound ear cache must invalidate on the reflex/convex flip"
type: debt
status: idea
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: [stdlib/geometry/triangulation.library.plato, stdlib/geometry/triangulation.types.plato, experiments/earcut/earcut-fast.plato, plato-415, compiler-416]
---

## Issue

`ClipEars` in `stdlib/geometry/triangulation.library.plato` picks the next ear by scanning
candidates from a cursor and running the full ear test on each. That test (`IsEar` ->
`HasNoBlocker`) walks the remaining ring, so selecting one ear costs O(n) candidates x an O(n)
test in the worst case and the pass is O(n^2). mapbox/earcut reaches near O(n log n) by hashing
vertices onto a z-order curve and testing only nearby ones.

This is a recorded trade, not an oversight: the file carries a comment saying so and citing
`STYLE_GUIDE.md`'s ordering (correct first; a faster variant is separate, later work). The issue
exists to capture the requirement a faster version must meet, because the obvious version is
subtly wrong and has already been written wrong once.

**The trap.** The natural speed-up caches `Convex` and a `Blockers` count on each node, refreshes
the clipped ear's two neighbours, and decrements every other candidate's count when the clipped
vertex was blocking it. That is what `experiments/earcut/earcut-fast.plato` does, and it is
unsound. Clipping changes the ring at the ear's two neighbours, and a neighbour can flip from
reflex to CONVEX. A vertex that stops being reflex stops blocking every candidate whose triangle
it sits in — an unbounded set, not the two nodes the clip is local to. Counts go stale, genuine
ears stop being recognised, `FindEar` falls through to its degenerate-input path ("first convex
corner"), and the pass emits OVERLAPPING TRIANGLES.

Observed while porting: a 4x4 square with two 1x1 holes returned 13 faces totalling 20 units of
area over a region of 14. The port therefore carries no ear state at all, and `EarClipNode`'s
declaration says why.

## Impact

Triangulation is the supply of triangles for everything mesh-shaped downstream (plato-415 items
9 and 13, and any consumer of `TriangleMesh2D`). Quadratic behaviour is invisible at the ring
sizes the current tests use and becomes the dominant cost on real boundary data — contours,
font outlines, GIS polygons, tessellated curves — where rings of thousands of vertices are
ordinary.

Nothing is blocked today, and the cost of deferring is bounded: the interface does not change,
only the body.

## Affected code

- `stdlib/geometry/triangulation.library.plato` — the ear-clipping section: `HasNoBlocker`,
  `IsEar`, `FindEar`, `ClipEars`, and the comment stating the invalidation requirement.
- `stdlib/geometry/triangulation.types.plato` — `EarClipNode`, whose fields are exactly the
  linked-list state, with a comment recording why no ear state is cached.
- `experiments/earcut/earcut-fast.plato` — the unsound version, kept as an experiment. Its
  `PROGRESS.md` shows step 4 (generate C#, add equivalence/invariant/performance tests) was never
  completed, which is why the defect survived to be ported.

## Cause / analysis

Not accumulated debt — a trade recorded at the point of the trade. The underlying difficulty is
real: the ear predicate is not local. Convexity is local to a corner's two neighbours, but
BLOCKING is a relation between a reflex vertex and every candidate triangle containing it, so any
incremental scheme has to propagate on the reflex/convex transition. A correct cache needs either

- a reverse index from each vertex to the candidates it currently blocks, maintained on the flip;
  or
- spatial pruning (earcut's z-order hash, or a grid over the ring) making the test cheap enough
  that no cache is wanted — which is how mapbox actually gets its speed. That route sidesteps the
  invalidation problem rather than solving it.

The second is the better bet, and the one earcut validated over years of adversarial inputs.

## Priority

p3. Correctness is settled and the interface is stable, so this is pure headroom, and no consumer
in the tree today has ring sizes that make it hurt. It rises as soon as something real feeds it
boundary data. It must not be attempted without a benchmark and without the tiling invariant
running as a gate, or it will reintroduce exactly the defect above — silently, because
overlapping triangles still look like a triangulation.

## Dependencies

- Blocked by: a running behavioural gate for triangulation (the harness item under plato-415).
  Optimising a geometry kernel with no executable invariant is how the unsound cache was written
  in the first place.
- Touches: `stdlib/geometry/triangulation.library.plato` only. No declaration changes, so
  concurrent work elsewhere in the polygon backlog will not collide.

## Fix approaches

1. **Spatial pruning (earcut's route).** Hash ring vertices onto a z-order curve, keep them in a
   sorted list, and test only vertices whose z-interval overlaps the candidate triangle's box.
   Near O(n log n), no invalidation contract, most field evidence. Largest change: the sorted
   structure has to survive clipping.
2. **Cached ear state with correct invalidation.** Keep `Convex` + `Blockers` plus a reverse index
   from each reflex vertex to the candidates it blocks; on a reflex->convex flip, decrement every
   candidate in that set. Correct and incremental, but the index is more state than the ring
   itself and every operation must maintain it.
3. **Cheap prefilter only.** Keep the scan; skip the blocker walk for candidates whose triangle
   box contains no reflex vertex, using a coarse grid rebuilt every k clips. Small, local, no
   invalidation contract, captures much of the win on real data without claiming an asymptotic
   improvement.

## Bedrock

The seam is the one the file already draws: `IsEar` is a PREDICATE over the ring and `FindEar` is
a SEARCH over candidates; nothing else in the pass knows how an ear is recognised. Any of the
three approaches should land entirely behind those two names with `ClipEars` untouched. That is
what makes a wrong optimisation revertible in one commit, and what lets the naive predicate stay
in the tree as the oracle a fast one is differentially tested against.

The invariant it protects is the one the unsound cache violated: **an ear is a corner no other
ring vertex lies inside, and any structure claiming to answer that faster must answer it
identically.** Keeping the naive predicate callable is what makes that checkable rather than
asserted.

Verdict: **simplest-along-the-grain**. The simple fix must NOT thread cache state through
`ClipEars` or add fields to `EarClipNode` that the clip loop has to maintain — once invalidation
becomes the caller's job the oracle is gone, and the failure mode is silent overlapping triangles
again.

## Done means

- [ ] A benchmark over rings of increasing size exists, and the current pass's growth is recorded
      before any change.
- [ ] The tiling invariant (face areas sum to the polygon's area; every face counter-clockwise)
      runs as a gate over the fast path, including the randomized star-shaped-polygon fuzz.
- [ ] The fast path and the naive predicate are differentially tested — same tiling on every fuzz
      case.
- [ ] Measured improvement on large rings, stated against the recorded baseline.

## Simplest fix

Approach 3: a coarse uniform grid over the ring's bounding box, rebuilt every k clips, used only
to skip the blocker walk when a candidate's triangle box holds no reflex vertex.

- **What you get** — a large constant-factor win on the common case (mostly-convex boundary data
  with sparse reflex vertices), no invalidation contract to get wrong, and it lives entirely
  inside `HasNoBlocker`.
- **What you give up** — still O(n^2) in the worst case, so it does not answer the asymptotic
  complaint; and a rebuilt-every-k-clips structure has its own staleness question — milder than
  the blocker-count one, but not free.

## Prevention

- The unsound cache survived because the experiment was never executed (`PROGRESS.md` step 4 is
  unticked). An experiment carrying an unvalidated algorithm is a trap for whoever ports it next;
  such files should say so in their header.
- The regression test is the tiling invariant itself, which belongs to plato-415's harness item
  rather than to this issue.
- Worth its own idea: a differential-testing helper for the geometry tree, so any "fast path" is
  checked against the obvious implementation over generated inputs as a matter of course.
