---
id: plato-334
title: "Query/solve result types share no interface: success flag spelled four ways"
type: debt
status: idea
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-30
closed:
links: [submodules/Plato/stdlib/spatial-queries-proximity.plato, submodules/Plato/stdlib/spatial-queries-overlap.plato, submodules/Plato/stdlib/optimization.plato, submodules/Plato/stdlib/collision-contacts.plato, submodules/Plato/stdlib/statistics-correlation.plato, tracker/issues/plato-079.md]
---

## Issue
The stdlib has thirteen "outcome of an algorithm" record types across five files
(three each in spatial-queries-proximity, spatial-queries-overlap, optimization,
and collision-contacts, plus `MaxFlowResult`), and a record-less fourteenth shape
in statistics-correlation. Every one of them `implements Value` and nothing else — there is no interface
covering the shape they share. As a result the same four facts are spelled
differently in each: **did it succeed** is `Intersects`, `Hit`, `Converged`, or an
out-of-band `-1` index sentinel; **how well** is `Distance`, `Residual`,
`ResidualNorm`, or absent; **where** is `Position`, `Root`, or `Points`; **how much
work** is `Iterations` or `CulledCount` or absent. Nothing is broken today — this is
vocabulary debt, not a defect.

## Impact
Three concrete costs. (1) No generic code can be written over results: a caller
cannot write one `IsHit`/`OrElse`/`Require` helper, so each call site re-reads the
specific field name. (2) Fields that ought to travel together drift apart —
`OptimizationResult` carries `Reason: TerminationReason`, but `RootFindResult` and
`LeastSquaresResult`, which end for exactly the same reasons, carry only a bare
`Converged: Boolean` and cannot say *why* they stopped. (3) New result types are
authored by copying whichever neighbour is closest, so the divergence compounds:
every new solver or query adds another spelling. Cost of doing nothing is slow
and steady rather than sharp.

## Affected code
- `submodules/Plato/stdlib/spatial-queries-proximity.plato:65,77,90` —
  `ClosestPointResult2D`/`3D`, `ClosestPairResult`. Success is implicit; missing
  target is the `-1` sentinel in `PrimitiveIndex`/`Face`.
- `submodules/Plato/stdlib/spatial-queries-overlap.plato:34,44,55` —
  `IntersectionResult2D`/`3D` (`Intersects: Boolean`), `FrustumCullResult`.
- `submodules/Plato/stdlib/optimization.plato:29,33,70,82` — `TerminationReason`
  sum type, `OptimizationResult` (has `Reason`), `RootFindResult` and
  `LeastSquaresResult` (both `Converged: Boolean`, no `Reason`).
- `submodules/Plato/stdlib/collision-contacts.plato:106,120,132` —
  `ShapeCastResult2D`/`3D` (`Hit: Boolean`, `-1` body sentinel), `OverlapResult`.
- `submodules/Plato/stdlib/graphs-algorithms.plato:109` — `MaxFlowResult`.
- `submodules/Plato/stdlib/statistics-correlation.library.plato:49-67` —
  `LinearFit`/`PolynomialFit` carry fit quality as loose methods
  (`Residual`, `CorrelationMagnitude`) with no result record at all, a fourth
  shape again.

## Cause / analysis
These types were authored file-by-file as each domain landed, and each one is
locally reasonable — `Hit` really is the natural word for a shape cast, `Intersects`
for an intersection. The debt is that no one owned the cross-domain vocabulary, and
Plato has no forcing function here: `implements Value` is satisfied by any record,
so nothing prompts an author to ask whether an interface already describes what they
are writing. `TerminationReason` shows the repo already knows the better pattern —
it just was not lifted out of `optimization.plato`. Not speculation: the sum type
exists at `optimization.plato:29` and is used by exactly one of the three types in
its own file that could use it.

## Priority
Recommend **p3**. Nothing is incorrect and no user-visible behavior depends on it,
so severity is low. But frequency is high and rising — this surface grows with every
new query or solver — and unification gets strictly more expensive per type added.
Right shape for a content-wave increment rather than an urgent fix. Safe to defer;
the cost of deferral is linear, not compounding, as long as new result types are not
being added in bulk right now.

## Dependencies
- Blocked by: nothing.
- Blocks: nothing filed. Would be a natural prerequisite for any generic
  "run a query, handle failure uniformly" helper library.
- Touches: five files across spatial, collision, optimization, graphs, and
  statistics areas — exactly the kind of broad rename that collides with a
  parallel stdlib content wave. Best done alone, or first in a wave.

## Fix approaches
1. **Interface-only, no type changes** — declare `QueryResult` (a success predicate),
   `PointResult<TPoint>`, `DistanceResult`, `ParameterizedResult`, and have the
   existing types implement them, keeping their current field names and adding the
   interface members as one-line derived bodies. Non-breaking, incremental,
   per-file. Leaves the inconsistent field names in place.
2. **Unify field names as well** — additionally rename to one spelling of success
   and one of quality. Cleanest end state; breaks every call site; the least
   attractive per unit of value since the names are individually good.
3. **Add `FitQuality` only** — a shared
   `{ RootMeanSquareError, MaximumError, SampleCount }` record for the fit-shaped
   results (`LinearFit`, `PolynomialFit`, `LeastSquaresResult`), and stop there.
   Narrowest useful slice; does nothing for the query-shaped half.

## Bedrock
The seam is the boundary between *algorithms* and *their callers* — currently there
is no vocabulary there at all, so every crossing is bespoke. The invariant worth
establishing: *an algorithm reports its outcome through an interface, not through a
record whose field names the caller must memorize.* Strengthening it makes three
future things cheap that are currently impossible: generic failure handling, uniform
diagnostics (every solver able to say *why*, not just *whether* — the
`TerminationReason` generalization), and result types that compose (a query feeding
a solver without an adapter). This is the idea worth taking from `plato-src-v2`,
which had exactly these interfaces sketched (`IQueryResult`, `IPointResult`,
`IDistanceResult`, `IFitResult`, `FitQuality`) and nothing else the stdlib lacks.

Verdict: **right** — the value here *is* the shared interface; a narrower fix
(option 3) delivers a record with no interface behind it and re-creates the same debt
one level down. If capacity forces option 3 first, it must NOT introduce `FitQuality`
as a bare struct — it should land as the payload of a `FitResult` interface, so the
interface layer stays reachable.

## Done means
- [ ] result interfaces declared in one place with doc comments explaining the split
- [ ] all thirteen existing result types implement the applicable interfaces
- [ ] `RootFindResult` and `LeastSquaresResult` carry `TerminationReason`, not a
      bare `Converged` flag
- [ ] at least one generic helper written against the interface, proving it is usable
- [ ] ForwardStdLib test green

## Simplest fix
Option 1 restricted to the two clearest interfaces: a success predicate and a
distance/quality accessor, implemented as derived bodies over the existing fields.
Gain: unblocks generic helpers immediately, no renames, no call-site churn, can land
file-by-file so it interleaves with other stdlib work. Give up: the field-name
inconsistency survives, so readers still see `Hit` next to `Intersects` next to
`Converged` — the interface hides it from generic code but not from humans.

## Prevention
- **Convention**: CONVENTIONS.md has no entry for result/outcome types. One naming
  the required interface and the standard spelling of success + reason would stop the
  next variant. Cheapest single preventive step; worth doing even if the
  refactor is deferred.
- **Check**: a CHK rule — a type whose name ends in `Result` must implement a result
  interface — is mechanically checkable and fits the existing stdlib conformance
  suite.
- **Related**: `-1`-as-missing-index appears in at least four of these types
  (`PrimitiveIndex`, `Face`, `Body`, `MatchedRightIndices`). That is a separate
  smell — an optional index wants a sum type, not a sentinel — and is already
  covered by [plato-079](plato-079.md) (Option/Result partiality cleanup); fold the
  sentinel sites into that sweep rather than filing anew.
