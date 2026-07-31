---
id: plato-320
title: PolygonMesh3D.UndirectedEdgeCount assumes a closed manifold: silently wrong Euler characteristic and genus on open meshes
type: bug
status: in-progress
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/stdlib/meshes-polygon.library.plato, submodules/Plato/stdlib/meshes-polygon-corners.library.plato, submodules/Plato/stdlib/topology.concepts.plato, submodules/Plato/stdlib/meshes-topology.library.plato, tracker/issues/plato-315.md, tracker/issues/plato-308.md]
---

## Issue

`PolygonMesh3D` satisfies its `MeshElementCounts.UndirectedEdgeCount` obligation by dividing the
corner count by two (`meshes-polygon.library.plato:34-35`):

```
UndirectedEdgeCount(self: PolygonMesh3D): Integer
    => self.CornerCount / 2;
```

That derives the topological edge count from the face-corner census by assuming every undirected
edge carries exactly two corners — true only on a closed mesh. `topology.concepts.plato:23-27`
explicitly forbids exactly this: a representation that cannot dedupe undirected edges without
building incidence "should implement `MeshIncidence` and derive the count from it rather than
guess: an over-count silently corrupts every Euler-characteristic result." `PolygonMesh3D`
guesses, and does not implement `MeshIncidence`.

Two secondary defects sit in the same family:

- **The in-source comment states the error backwards.** `meshes-polygon.library.plato:33` says
  "an open mesh over-counts." It **under**-counts. With C corners, U_i interior and U_b boundary
  edges: C = 2·U_i + U_b, so C/2 = U_i + U_b/2 < U_i + U_b = the true U. Under-counting E
  *inflates* χ = V − E + F. Single triangle: V=3, F=1, C=3, so the formula yields E=1 (integer
  truncation) and χ = 3, where the true disk values are E=3 and χ=1.
- **Boundary corners break the dense edge numbering.** `TwinCorner`
  (`meshes-polygon-corners.library.plato:59-69`) returns the "none" index −1 for a boundary
  corner, so `IsCanonicalCorner` (:127-128, `corner.Value < TwinCorner(corner).Value`) is false
  for every boundary corner. No boundary edge therefore has a canonical corner, so
  `CornerUndirectedEdge` (:133-139) aliases boundary corners onto some other edge's index and its
  documented guarantee — "the results cover [0, UndirectedEdgeCount)" (:132) — does not hold on
  an open mesh.

## Impact

Wrong topology answers with no error signal, on the load-bearing member of the concept. Every
`MeshElementCounts` consumer inherits the fault for any `PolygonMesh3D` with a boundary:
`EulerCharacteristic` (`meshes-topology.library.plato:30-31`), `GenusIfClosed` (:37-38, already
precondition-guarded on closedness but reads the corrupt count), and `HasUndirectedEdges`
(:49-50). A half-open mesh returns a plausible-looking integer, so nothing surfaces the problem;
this is a correctness bug, not a missing feature.

Blast radius is bounded today by the fact that codegen still ships from `stdlib-legacy`, and by
`polyhedra-conway.library.plato` (:82, :87, :93) consuming `IsCanonicalCorner` /
`CornerUndirectedEdge` on Conway operands that are closed polyhedra in practice. Neither the
concept nor the corner library states that closedness precondition, so the guard is accidental.

## Affected code

- `submodules/Plato/stdlib/meshes-polygon.library.plato:32-35` — the guessing implementation and
  its backwards precondition comment.
- `submodules/Plato/stdlib/meshes-polygon-corners.library.plato:59-69` — `TwinCorner` returns −1
  on a boundary corner; :125-139 — `IsCanonicalCorner` / `CornerUndirectedEdge` mishandle that −1.
- `submodules/Plato/stdlib/topology.concepts.plato:23-33` — the obligation and the prohibition
  being violated.
- `submodules/Plato/stdlib/meshes-topology.library.plato:30-50` — the derived consumers.
- `submodules/Plato/stdlib/meshes.plato:81-82` — `PolygonMesh3D implements Value, Meshable3D,
  MeshElementCounts` (no `MeshIncidence`).
- `submodules/Plato/stdlib/polyhedra-conway.library.plato:76-93` — the only external consumer of
  the canonical-corner numbering.

## Cause / analysis

`PolygonMesh3D` stores faces as a jagged corner table with no side table
(`meshes-polygon.library.plato:14-15`), so it genuinely cannot dedupe edges by a direct read. The
concept anticipated that case and prescribed the answer — implement the `MeshIncidence` rung and
derive U from it — but the implementation took the cheap closed-manifold shortcut instead and
recorded the shortcut as a precondition comment rather than a defect.

The machinery for the correct answer already exists in the corner library. An undirected edge can
be counted exactly once by naming it at its lower corner when it has a twin, and at its only
corner when it does not:

```
U = count of corners c where TwinCorner(c) == none OR c < TwinCorner(c)
```

That is `IsCanonicalCorner` widened to treat "no twin" as canonical — which simultaneously fixes
the dense numbering, since every boundary edge then gets its own canonical corner and the
`[0, U)` coverage guarantee holds again. Cost is another O(C²) scan on top of an already O(C²)
`TwinCorner`, consistent with the scan-based style of the whole file; correctness first,
performance is a separate concern (no benchmark exists for these bodies).

**Related dead-concept finding (belongs to plato-315, recorded here because it is this bug's
prescribed fix path):** `MeshIncidence` has **zero implementors** tree-wide, while
`meshes-topology.library.plato:66-118` ships **eleven** derived bodies on it — a larger
unreachable API surface than the `Dimensioned` case plato-315 already flags. The middle rung of
the topology ladder is currently decoration. Fixing this bug via option 2 below is also what
makes that rung real.

## Fix approaches

1. **Widen the canonical-corner rule (preferred; effort S).** Treat a twin-less corner as
   canonical in `IsCanonicalCorner`, then implement `UndirectedEdgeCount` as the count of
   canonical corners rather than `CornerCount / 2`. Fixes the count, χ, and the
   `CornerUndirectedEdge` dense numbering in one change, adds no new concept obligations, and
   leaves Conway behaviour on closed input bit-identical. Correct the backwards comment.
2. **DONE SEPARATELY (0aa7331), though not as this bug's fix.** `PolygonMesh3D` now implements
   `MeshIncidence` via `meshes-polygon-incidence.library.plato`, landed under plato-315's
   dead-concept decision. `UndirectedEdgeCount` still uses the option-1 canonical-corner count
   rather than being re-derived from incidence: the canonical rule is already correct and
   cheaper, so re-routing it through incidence would add cost without adding correctness. The
   value of this option was always the eleven unreachable derived bodies, and those are now
   reachable. Original text: **Make `PolygonMesh3D` implement `MeshIncidence` and derive U from it (effort M).** The
   concept's own prescription, and it lights up eleven unreachable derived bodies. Strictly more
   work than option 1 for the same correctness result, and it obliges the full six-query
   incidence surface. Best done as its own change once option 1 has stopped the bleeding.
3. **Document-only: state the closed-manifold precondition on the concept.** Rejected — it would
   make the concept's central member conditionally meaningless and contradicts
   `topology.concepts.plato:23-27`, which already ruled the guess out.

## Priority

p2, not p1: real silent-wrongness, but no shipping consumer today (codegen is still on
`stdlib-legacy`) and the one in-tree consumer is accidentally safe on closed input. Option 1 is
small and self-contained, so this is cheap to clear before the forward stdlib starts feeding
codegen — at which point it becomes p1.

## Dependencies

- Same files as any P8 mesh-library work; land before or after that wave, not concurrently.
- Gate: `Plato.CLI lint stdlib` zero errors + `tools/check-stdlib-fast.ps1`.
- **BLOCKED ON plato-308** for the one outstanding Done-means box. Numeric chi
  verification needs the forward conformance suite to execute, which needs the generated
  C# to compile. Nothing in this issue can close it; do not re-investigate.
- Verification needs an open-mesh case: assert χ = 1 for a single triangle and for a quad strip
  (a disk), and χ = 2 for a closed tetrahedron/cube, before and after.

## Done means

- [x] `UndirectedEdgeCount(PolygonMesh3D)` returns the true undirected edge count for open and
      closed meshes alike (no `CornerCount / 2`).
- [x] `IsCanonicalCorner` treats a twin-less (boundary) corner as canonical, so
      `CornerUndirectedEdge` results again cover `[0, UndirectedEdgeCount)` on open meshes.
      `CornerUndirectedEdge` needed the same `IsNone` guard: its `canonical` binding was
      `corner.Value.Min(twin.Value)`, which is `-1` for a boundary corner and made the
      preceding-canonical-corner scan empty.
- [ ] **OUTSTANDING — χ verified = 1 for a single triangle and for an open quad strip, = 2 for a
      closed cube.** Cannot be executed today: forward-stdlib conformance does not run. Codegen
      now succeeds but the generated C# does not compile
      (`conformance/Plato.ForwardConformanceTests/README.md`, status 2026-07-29), and
      `stdlib-tests/` laws are type-checked only. Hand-trace of the new formula, recorded so the
      expected numbers are not re-derived later: single triangle — 3 corners, every corner
      twin-less, so all 3 are canonical, E=3 and χ = 3 − 3 + 1 = 1 (disk, correct; the old
      formula gave E=1, χ=3). Closed cube — 24 corners, every corner twinned, 12 canonical,
      E=12 and χ = 8 − 12 + 6 = 2 (sphere, correct; unchanged from the old formula). Tick this
      box when the conformance suite executes.
- [x] The backwards "an open mesh over-counts" comment corrected to under-counts.
- [x] Conway operators (`polyhedra-conway.library.plato`) unchanged on closed input; lint +
      ForwardStdLib gate pass. Closed-input equivalence holds by construction, not by test: on a
      closed mesh every corner has a twin, so `twin.IsNone` is false and both edited predicates
      reduce to their previous expressions. `tools/check-stdlib-fast.ps1` PASS (lint --strict 0
      errors, checker ratchet no regression).

## Simplest fix

Option 1, two edits: widen `IsCanonicalCorner` to accept a twin-less corner, and replace
`CornerCount / 2` with a canonical-corner count.

## Prevention

- The plato-315 prevention idea — lint flagging concepts with zero implementors but derived
  library bodies — would have surfaced the dead `MeshIncidence` rung, which is what let the
  shortcut look acceptable.
- Sharper rule this bug suggests: a derived body whose comment states a precondition its own
  concept forbids guessing at is a defect, not documentation. Worth a `/track-idea` for a
  convention: preconditions belong on the concept, not on an implementor working around it.
