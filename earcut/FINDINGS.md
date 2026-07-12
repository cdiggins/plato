# Earcut in Plato — findings

*2026-07-11. Companion to [`earcut.plato`](earcut.plato) and
[`Ara3D.Earcut.Tests/`](Ara3D.Earcut.Tests/).*

## What this is

A functional re-expression of [mapbox/earcut](https://github.com/mapbox/earcut)
(polygon-with-holes triangulation by ear clipping) as a Plato library, compiled
together with the full production standard library (`plato-src`) into a
standalone C# test library, with NUnit tests asserting the invariant the
original earcut suite uses: the output triangles exactly tile the polygon.

```
earcut/
  earcut.plato          the library (129 code lines incl. two types; 256 with comments)
  regen-earcut.ps1      merge plato-src + earcut.plato -> Generated\  (-Test runs suite)
  Ara3D.Earcut.Tests/   NUnit project; Generated\ is emitted, not committed
    EarcutTests.cs      20 tests
    EarcutSupport.cs    one shim for a missing array intrinsic (below)
  FINDINGS.md           this file
```

Recipe: `--csharp-style=extensions --scalar=float --optimize --optimize-arrays
--methods` — the golden conformance recipe minus `--inline` (see bugs below).
Result: **20/20 tests pass** — convex, concave, star, both windings, one/two
holes, hole near a concavity, collinear vertices, duplicate vertices,
zero-width spikes, self-intersecting inputs (graceful degradation), degenerate
inputs, the flat earcut coordinate API, and randomized star-shaped polygons.

## The hypothesis, assessed

> The traditional approach to polygons and triangulation obscures algorithms
> and intent behind micro-optimizations.

**Largely supported, with an honest caveat.** The original earcut is ~700
lines of JavaScript built on a hand-rolled doubly-linked vertex list mutated
in place, z-order-curve spatial hashing, and inlined predicates. The algorithm
in it is genuinely hard to see. The same semantics fit in three pure folds,
each of which states its own meaning:

1. **Orient + filter** — each ring rewound so the outer boundary is CCW and
   holes CW (`OrientedRing`: one shoelace fold and a conditional `Reverse`),
   then stripped of zero-turn corners — duplicates, collinear midpoints, and
   spikes, all caught by the single predicate `CornerTurn == 0`
   (`FilterRing`, earcut's `filterPoints`).
2. **Bridge** — `MergedRing` is literally `holeRings.Reduce(outerRing, MergeHole)`;
   a bridge is valid iff `CanSee`: it properly crosses no edge, passes through
   no vertex, and its midpoint is interior (even-odd test).
3. **Clip** — a ring of n vertices takes exactly n − 2 clips, so ear clipping
   is a fold of `ClipOne` over n − 2 steps; an ear is a CCW corner whose
   triangle contains no other vertex.

The caveat: not all of earcut's complexity is obscuring micro-optimization.
It divides into three kinds, and only two could be discarded:

- **Speed** (z-order hashing, the linked list, reflex-only containment scans):
  discarded. This costs us O(n²) clipping and O(n³)-per-hole bridging vs
  earcut's near O(n log n) — the explicit price of legibility, acceptable here.
- **Data-structure bookkeeping** (node recycling, index arithmetic on the
  flattened coordinate array): dissolved for free. Immutable index arrays +
  `RemoveAt`/`Splice` express the same thing declaratively.
- **Robustness repair** (`splitEarcut`, `cureLocalIntersections`, the sector
  tests in `locallyInside`): *not* mere optimization. This version replaces
  them with a stronger up-front bridge validity test plus a fallback clip that
  guarantees termination and well-formed (if imperfect) output on
  self-intersecting input. Genuinely degenerate real-world data is where the
  remaining risk lives; exact float predicates (`Turn == 0`, `SamePoint`) are
  the same robustness posture as earcut itself.

A bug found during development illustrates the verification payoff: with two
holes, the first-valid bridge for hole 2 ran from (0,0) to (5,5) — exactly
through both corners of hole 1, invisible to a proper-crossing test. Because
`MergedRing` is a pure function of the input, dumping it in a unit test made
the defect obvious in one line, and the fix (`CanSee` also rejects bridges
passing through any vertex) is a single readable predicate. In the mutating
linked-list formulation the same diagnosis requires stepping a debugger
through splice states.

A second bug made the case again, and taught something subtler: `filterPoints`
is *not* dispensable bookkeeping. The ear test must skip vertices whose
coordinates coincide with an ear corner (hole bridges duplicate their
endpoints by construction), but that same skip makes a zero-width spike
invisible — a triangle spanning the spike as a chord passes the ear test, and
the tiling sum overshoots. First assumed to be an optimization and omitted,
filtering turned out to be a correctness precondition: clipping assumes every
zero-turn corner is already gone. The area invariant caught it (`ZeroWidthSpike`
overshot by exactly the chord triangle), and the functional fix is one fold:
remove the first `CornerTurn == 0` vertex, n times (`FilterRing`).

### Algorithmic simplifications relative to earcut

| earcut | this version |
|---|---|
| leftmost-hole sort + x-ray visibility walk + sector checks | first `CanSee`-valid (ring, hole) vertex pair, brute force |
| ear test vs z-order-hashed neighborhood, reflex vertices only | ear test vs every remaining vertex |
| while-loop until done + repair passes on failure | exactly n − 2 fold steps; fallback = clip first convex corner |
| in-place splice of linked nodes | `Splice`: four array concatenations |
| output: flat index list | output: `IArray<Integer3>` (flat API provided for parity) |

## Toolchain findings (things this port surfaced)

Filed here because earcut.plato is the first consumer of several paths.

1. **Heterogeneous `Reduce` mis-typing (emitter bug, the big one).** When the
   fold's accumulator type differs from the element type, the TIR body writer
   casts the accumulator lambda parameter to the *element* type
   (`((int)acc).Add(...)` for a `Number` accumulator over `Indices`), producing
   non-compiling C#. Workaround: only homogeneous folds. Two of the three
   rewrites arguably improved the code (`MergedRing` folding over hole *rings*;
   crossing *counts* instead of boolean parity), but `ClipAll` had to fold over
   n − 2 dummy copies of the start state, which is a wart. Beyond the bug fix,
   the missing primitive is `Iterate(n, seed, f)` — "apply f n times" is the
   natural shape of ear clipping and of most sequential geometry algorithms.
2. **`--inline` emits non-compiling lambdas** whose inlined bodies mix
   `IReadOnlyList<T>` and `ReadOnlyList<T>` returns (CS1662). Dropped from
   this recipe; the other five golden flags work.
3. **`Concatenate` has no C# implementation.** Declared in `intrinsics.plato`,
   absent from `Ara3D.Collections.LinqArray` (which has `Concat`). Shimmed in
   `EarcutSupport.cs`; a one-line `LinqArray` addition would retire the shim.
4. **Scalar-erasure leaks on `Integer` arrays.** `Plato.Intrinsics.
   ArrayExtensions.Range(this int) : ReadOnlyList<Integer>` shadows the
   erased `LinqArray.Range`, so expressions like `0.Range` or
   `points.Indices.Take(0)` type as wrapper-`Integer` lists and fail where
   `IReadOnlyList<int>` is expected. Same family: `0.MapRange(i => i)` emitted
   `MapRangeEager` returning `IReadOnlyList<Integer>`. Workaround:
   `points.Map(x => 0).Take(0)` (generic inference erases correctly).
5. **Static-vs-instance receiver mismatch:** a function with an `_`-named
   receiver (`NoTriangles(_: PolygonWithHoles)`) emits as static C#, but a
   `p.NoTriangles` call site still emits instance-style (CS0176). Renaming the
   receiver parameter sufficed.
6. **Stdlib gaps this algorithm wanted:** a first-index-where / find operation
   (every search here is a fold carrying a −1 sentinel — an `Option` type per
   `docs/additions.plato` §1 would name this); array `Sum`; and the planar
   predicates (`Turn`/orient2d, point-in-triangle, proper segment crossing,
   on-segment, ray parity) which belong in a stdlib `Planar` library — they are
   the foundation of *every* 2D computational-geometry algorithm.
7. **Worked without friction:** statement bodies with `var`, lambdas capturing
   enclosing *parameters*, generic `$T` library functions, struct accumulators
   in folds, nested `IArray<IArray<Integer>>`, operator overloading on
   `Point2D`/`Vector2`, and the two new record types. Lambda capture of
   *locals* is unexercised in plato-src, so this port avoids it — worth a
   conformance law either way.

## Verification approach

One invariant does most of the work, borrowed from earcut's own "deviation"
metric: **Σ signed triangle areas = |outer ring area| − Σ |hole areas|**, with
every triangle CCW, indices in range and distinct, and count ≤ n − 2. Any
dropped ear, filled hole, flipped triangle, or overlap moves the sum. The
suite runs the invariant over hand-picked shapes (each chosen to hit one code
path: winding normalization, bridging, collinear skipping, degenerate input)
plus seeded random star polygons.

## Follow-ups

- Fix the heterogeneous-fold emitter bug (item 1); then restore `ClipAll` to a
  plain `Range` fold — the diff is three lines and makes a good regression test.
- Add `Concatenate` to `LinqArray`; delete `EarcutSupport.cs`.
- Consider promoting `PolygonWithHoles`, the planar predicates, and
  `Triangulate` into `plato-src` (requires the usual `regen-plato.ps1 -Apply`
  + conformance gate cycle; this folder deliberately stayed outside it).
- If performance ever matters: reflex-vertex caching and earcut's leftmost-hole
  ordering both fit the functional formulation without changing its shape;
  z-order acceleration and Delaunay edge refinement would be further,
  independent layers on top — neither changes the pipeline.
