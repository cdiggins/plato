---
id: plato-306
title: Generic Difference defaults via optional delta conversion concept
type: idea
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/stdlib/algebra-metric.concepts.plato, submodules/Plato/stdlib/quantities.concepts.plato, submodules/Plato/stdlib/quantities.library.plato, submodules/Plato/stdlib/transforms-points.library.plato, submodules/Plato/stdlib/time.library.plato, submodules/Plato/stdlib/intervals-bounds.concepts.plato, submodules/Plato/stdlib/points.plato, submodules/Plato/stdlib/time.plato, tracker/issues/plato-277.md]
---

## Idea

`Difference<TDelta>` already models affine positions (Point↔Vector, Instant↔Duration): add/subtract a delta, and take the displacement between two positions (`Between`). Today each implementor hand-writes those members, even when the type is just “same numbers as the delta, different role” — points already expose `PositionVector`/`ToPoint`; Instant/Duration share seconds.

Proposal: keep `Difference` as the public affine API; add an *optional* helper concept for the two conversions (working name `AsDelta`: `ToDelta` / `FromDelta`). When that helper is present, one library fills `Add` / `Subtract` / `Between` (and the `Self−Self→TDelta` sugar for `-`) the way `Quantity` fills arithmetic from `Amount`/`FromAmount`. Types that cannot or should not convert trivially keep hand-written `Difference` members and simply omit the helper.

Also noted in the same discussion: `Between(a,b)` is the same operation as `Subtract(b,a)` when `Subtract(Self,Self): TDelta` means `a - b`; the concept keeps `Between` for a stable orientation law (`a.Add(a.Between(b)) == b`).

## Assumptions

- Enough `Difference` implementors share a trivial conversion-to-delta that a generic fill pays for itself (Point2D/3D, Instant today; possibly more).
- Not every `Difference` has that conversion (homogeneous/curvilinear points, exotic embeddings) — so the helper must stay optional, not required on `Difference`.
- Plato’s existing “library over concept” pattern is enough for defaults; we do not need concept default bodies.
- `TDelta` must support additive ops for the generic bodies (`where TDelta: Additive` is available on concepts today; see `IntervalLike` in `intervals-bounds.concepts.plato`).
- Types must still declare `implements Difference<…>` explicitly; presence of the helper does not auto-claim `Difference`.

## Design decisions

- **Required vs optional conversions** — Fold `ToDelta`/`FromDelta` into `Difference` (Quantity clone) vs keep them on a separate optional concept. Former: one concept, forces conversion on all Difference types. Latter (preferred in discussion): hand-written Difference remains legal.
- **Helper naming** — `AsDelta` / `ToDelta`/`FromDelta` vs reuse `PositionVector`/`ToPoint` (point-specific) vs `Amount`/`FromAmount` (collides with Quantity; Instant’s carrier is seconds, not Duration) vs `OriginDelta`/`AtOrigin` vs a subtype name like `CartesianDifference` / `OriginBasedDifference`.
- **Structural shape** — Freestanding optional helper + library defaults vs `CartesianDifference inherits Difference` (only the subclass gets free fills) vs no shared concept (document the recipe only) vs delta-centric `Apply(delta, position)` API.
- **`Between` vs `Subtract(Self,Self)` as primitive** — Keep `Between` on the concept for law orientation, or make `Subtract(Self,Self): TDelta` the primitive and define `Between(a,b) => Subtract(b,a)` in a library.
- **Implicit type-named conversions** — Rejected for points already (`transforms-points.library.plato` deliberately avoids silent point↔vector coercion); a named concept keeps the conversion explicit while still enabling generics.

## Related

- [algebra-metric.concepts.plato](../../submodules/Plato/stdlib/algebra-metric.concepts.plato) — `Difference<TDelta>` definition and `Between` orientation contract.
- [quantities.concepts.plato](../../submodules/Plato/stdlib/quantities.concepts.plato) / [quantities.library.plato](../../submodules/Plato/stdlib/quantities.library.plato) — precedent: projections + one generic obligation fill.
- [transforms-points.library.plato](../../submodules/Plato/stdlib/transforms-points.library.plato) — Point Difference fills + `PositionVector`/`ToPoint`.
- [time.library.plato](../../submodules/Plato/stdlib/time.library.plato) — Instant `Difference<Duration>` fills (parallel hand-written pattern).
- [intervals-bounds.concepts.plato](../../submodules/Plato/stdlib/intervals-bounds.concepts.plato) — live `where T: …` / `where TPoint: Difference<TDelta>` syntax on concepts.
- [plato-277](plato-277.md) — notes `Difference` does not constrain `TDelta` to `Scalable` (blocks generic MidPoint); related concept-gap burn-down.
- Stale comment in `functional-procedural.library.plato` claims Plato has no `where` syntax — contradicted by `IntervalLike`; fix if touching that file.

## Approaches

Short term:

1. Add optional helper concept + `DifferenceDefaults` library; migrate Point2D/3D and Instant to supply only conversions; keep `Between` orientation tests.
2. Skip a new concept: extract a shared comment + law tests only; leave per-type bodies (lowest risk).
3. Introduce `OriginBasedDifference` (or similar) that *inherits* `Difference` and owns the conversions + defaults — clearer than a freestanding `AsDelta`.

Long term: constrain `TDelta: Additive` (and maybe `Scalable`) on `Difference` or the helper so MidPoint/Lerp-style generics become expressible ([plato-277](plato-277.md)); more affine positions reuse the same fill.

Adjacent ideas (file separately if wanted):

- Promote `Subtract(Self, Self): TDelta` onto `Difference` (or replace `Between` as the primitive).
- Fix stale “no where clause” comment / document concept-`where` in semantics.
- Generic MidPoint once `TDelta` is suitably constrained.

## Approaches compared (from discussion)

| Approach | Idea | Tradeoff |
|---|---|---|
| A. Optional helper + library defaults | `AsDelta` (name TBD) fills Difference when present | Flexible; types still dual-declare implements |
| B. Required conversions on Difference | Quantity clone | Forces conversion on every Difference |
| C. No concept | Keep Point/Instant fills separate | No abstraction; duplication / orientation drift |
| D. Subtype of Difference | e.g. `OriginBasedDifference inherits Difference` | Names which Differences get free fills; Instant/Point both “origin-based” is a mild stretch |
| E. Delta-centric API | `Apply(delta, position)` on the delta type | Unusual for UFCS; still need reverse map for Between |

## Case against

- **Two concepts for one idea.** Callers care about `Difference`; `AsDelta` is machinery. Cognitive load and naming bikesheds may exceed the duplication saved (only a handful of implementors today).
- **False generality.** Point↔vector and Instant↔duration look alike as field copies but may diverge (origins, units, homogeneous divide-by-W). A shared fill can encode the wrong law for a future “almost Cartesian” type.
- **`implements` still duplicated.** Without language support for “AsDelta implies Difference members,” authors must remember both declarations — easy to implement AsDelta and forget Difference (or the reverse).
- **Quantity already covers Duration.** Instant is the interesting half; Point already has named converters. The “generic” story might be optimizing for a pattern that stays rare.
- **`where TDelta: Additive` enforcement depth.** Concept `where` parses, lints, and reaches C# codegen; relying on it for body checking of `$D + $D` may still be softer than authors expect.

**Verdict: pursue** — the Quantity precedent is strong, Point and Instant already duplicate the same recipe, and an *optional* helper (or OriginBased subtype) preserves escape hatches. Park if a census shows no third implementor within a release. Drop only if the team prefers documented recipe + law tests (approach C) and rejects any new concept surface.

## Bedrock

Strengthens the affine seam in `algebra-metric.concepts.plato`: one orientation law for `Between`, one place that derives `+`/`-` from “convert to delta, do additive math, convert back,” matching the Amount/FromAmount seam in `quantities.*.plato`. Makes adding the next affine position type (and fixing orientation bugs) cheaper.

**Verdict: simplest-along-the-grain** — do *not* make conversions required on `Difference`, do *not* add implicit point↔vector conversions, and do *not* invent `where` on library functions for this; optional helper (or Difference subtype) + library over that concept stays within today’s language.

## Done means

- [x] Optional conversion concept (final name) declared; `Difference` unchanged as the affine API (or documented subtype relationship if approach D wins). — **approach D**: `OriginBased<TDelta> inherits Difference<TDelta>`, members `Offset` / `FromOffset`.
- [x] One library fills Add / Subtract(delta) / Between / Subtract(Self,Self) from the conversions, with `TDelta: Additive`. — plus `Lerp` and `Origin`; bound is `TDelta: Additive, TDelta: Scalable`.
- [x] Point2D/3D and Instant migrated to conversion-only obligation fills; behavior matches prior `Between` = b−a law. — and PointN / UvCoordinate / UvwCoordinate, which had never been filled at all.
- [x] At least one Difference implementor remains conversion-free (or a documented hypothetical) proving the helper is optional. — documented hypothetical: `HomogeneousPoint2D`/`3D`, whose delta conversion needs a divide by W. No live conversion-free implementor exists, so this is the weaker half of the criterion; see *Outstanding* below.
- [ ] Law/witness coverage for `a.Add(a.Between(b)) == b` on migrated types.

### Landed

- Plato `d5f2bcb` — Rec 1: `Difference` bound to `TDelta: Additive, Scalable`; `Subtract` (both spellings) and `Lerp` derived; 8 hand-written bodies deleted.
- Plato `bfef281` — Rec 2: `OriginBased<TDelta>`; `Add`, `Between`, `Origin` derived; all six implementors migrated.
- Prerequisite [plato-307](plato-307.md) (Plato `45dbd6e`) moved `Zero` onto `Additive` so `Origin` resolves for `Duration` as well as the vector deltas.

Measured on a pristine-compiler copy with the stdlib baseline pinned at `45dbd6e`
(a concurrent session was editing `PlatoCompiler/Compilation.cs` and
`kinematics.plato` throughout; an unpinned first measurement was contaminated by
their `Wrench` → `SpatialForce`/`SpatialVelocity` rename and had to be redone):

- 0 parse / 0 symbol-resolution errors.
- LINT001 (unfilled obligations that become throwing stubs) **278 → 266, zero regressions**. The twelve fixed are `PointN` / `UvCoordinate` / `UvwCoordinate` × {`Add`, `Between`, `Subtract`, `Lerp`}.
- `ForwardStdLibCheckerTests` 5/5 pass, ratchet `MaxFunctionsWithDiagnostics = 19` holds, sum-type diagnostics 0.

### Outstanding

**Law/witness coverage is not reachable in the forward stdlib today.** The
`Law_*` / `Witness_*` runner works over `stdlib-legacy` + `stdlib-legacy-tests`
through the conformance suite; `stdlib` (the forward vocabulary) is
declarations, and its only gates are `Plato.CLI lint` and the type-checker
ratchet in `ForwardStdLibCheckerTests`, neither of which executes a law.
`Difference` / `OriginBased` do not exist in `stdlib-legacy`, so the orientation
law `a.Add(a.Between(b)) == b` cannot be stated where the runner would see it.
The orientation is currently protected only by inspection and by the derivations
routing through a single `Between`.

Closing this box needs one of: laws for the forward vocabulary (a new harness),
or the concepts reaching `stdlib-legacy`. Worth its own issue rather than
holding this one open indefinitely — but the box stays unticked until then,
because an unverified orientation law on six migrated types is exactly the
regression this refactor could hide.

### Language findings worth keeping

- **Library dispatch is on the first parameter, which must be a concrete type or a concept.** A bare type variable there is rejected (`not a concrete type or concept`). So reversed-operand forms — `Add(delta, position)`, and the delta-centric `Apply(delta, position)` of approach E — can never be derived generically over a concept; they stay concrete per type. This retires approach E on a hard language constraint rather than on taste.
- Concept `where` bounds reach body checking: `Subtract`, `Lerp` and `Origin` all use `$D` arithmetic supplied purely by the bound, and the checker accepts them. The stale comment claiming Plato has no type-parameter constraint syntax has been removed from `algebra-metric.library.plato`.
- Overload resolution prefers the subtype's library body: `Add`/`Between` over `OriginBased<$D>` win over the `Difference<$D>` obligation for the same receiver. The whole approach-D design depends on this, and it holds.

## Simplest possible implementation

Add `AsDelta<TDelta> where TDelta: Additive` with `ToDelta`/`FromDelta`; library of four one-liners; Point2D/3D alias `PositionVector`/`ToPoint` as those members; Instant maps via seconds ↔ Duration; delete duplicated arithmetic bodies in `transforms-points.library.plato` / `time.library.plato`.

Pros:
- Stops Point/Instant drift; orientation fixed once.
- Matches Quantity’s proven pattern; no new language feature.

Cons:
- New concept name to defend; dual `implements` on each type.
- Only two call sites today — ROI uncertain until a third appears.
