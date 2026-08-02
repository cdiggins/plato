---
id: plato-307
title: Additive declares no identity element (Zero)
type: debt
status: done
priority: p1
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/algebra-operations.concepts.plato, submodules/Plato/stdlib/algebra-numeric.concepts.plato, submodules/Plato/stdlib/quantities.concepts.plato, submodules/Plato/stdlib/matrices.concepts.plato, tracker/issues/plato-306.md]
---

## Issue

`interface Additive` is documented as "An additive group" but declares only
`Add` / `Subtract` / `Negative` — no identity element. A group without its
identity is not a group; the doc comment states a contract the interface does not
carry.

The identity `Zero(x: Self): Self` lives instead on `NumericalLimits`, bundled
with `One` / `MinValue` / `MaxValue`. That placement is wrong on two counts:
`Zero` is an *algebraic* property (the additive identity) while `One` /
`MinValue` / `MaxValue` are *representational* (what the number tier can hold),
and the bundling means a type can only obtain `Zero` by also claiming
representable extremes it may have no meaning for.

Unverified as a runtime defect — this is a vocabulary/modelling defect, found by
inspection while working plato-306.

## Impact

Any type that is `Additive` but not `Numerical` has no zero:

- **All 50 `Quantity` types.** `Quantity inherits Value, Comparable, Hashable, Additive, Scalable, Interpolatable` — no `NumericalLimits`. `Duration.Zero`, `Length.Zero`, `Mass.Zero` do not exist.
- **All `MatrixLike` types.** `inherits Value, Additive, Scalable` — no zero matrix.
- **Every `where T: Additive` bound.** `IntervalLike<T>` today, `Difference<TDelta>` under plato-306. A generic body under such a bound cannot name the identity, so no fold-with-identity, no empty-sum, no accumulator seed, and no "distance from zero" is expressible generically.

Directly blocks the `Origin` derivation in plato-306 (`Origin(x) =>
x.FromOffset(x.Offset.Zero)`) for `Instant`, whose delta is `Duration`.
`Point2D`/`Point3D`/`PointN` are unaffected only by luck — their deltas are
`Vector`, which reaches `Zero` via `Numerical`.

Cost of doing nothing: every future generic derivation that needs an identity
gets written concretely per type, or silently skipped with a TODO, the way
`algebra-metric.library.plato` already skips `MidPoint`.

## Affected code

- [algebra-operations.concepts.plato:8](../../submodules/Plato/stdlib/algebra-operations.concepts.plato:8) — the "An additive group" comment.
- [algebra-operations.concepts.plato:9](../../submodules/Plato/stdlib/algebra-operations.concepts.plato:9) — `interface Additive`, missing `Zero`.
- [algebra-numeric.concepts.plato:11](../../submodules/Plato/stdlib/algebra-numeric.concepts.plato:11) — `Zero(x: Self): Self` on `NumericalLimits`, where it does not belong.
- [quantities.concepts.plato:15](../../submodules/Plato/stdlib/quantities.concepts.plato:15) — `Quantity` inherits `Additive` without `NumericalLimits`: 50 types with no zero.
- [matrices.concepts.plato:12](../../submodules/Plato/stdlib/matrices.concepts.plato:12) — `MatrixLike` likewise.
- [numeric-structures-components.library.plato:113](../../submodules/Plato/stdlib/numeric-structures-components.library.plato:113) — the generic `Zero(self: Vector) => self.Broadcast(0.0)`; stays as-is, satisfies the relocated obligation unchanged.
- Concrete `Zero` bodies that also stay unchanged: `color.library.plato:53`, `numbers.library.plato:57` / `:137` / `:176` / `:215`.

## Cause / analysis

`NumericalLimits` was assembled as "the distinguished-elements-and-extremes
surface of the number tower", which is a coherent grouping if every additive
type is a number. It stopped being true when `Quantity` and `MatrixLike` were
introduced as `Additive` without joining the numeric tower — deliberately, since
neither has meaningful `MinValue`/`MaxValue` semantics tied to representation.
`Zero` went along for the ride and was never re-homed.

The `Additive` doc comment was written from the mathematics (an additive group)
rather than from the declaration, so the gap never showed up in review.

## Priority

**p1.** Small, low-risk, and actively gating: plato-306 Rec 2 cannot derive
`Origin` for `Instant` without it, and that work is in flight now. It also grows
quietly — every generic derivation deferred for want of an identity is a
concrete body someone writes instead, and those are the bodies plato-306 is
trying to delete.

## Dependencies

- Blocked by: nothing.
- Blocks: [plato-306](plato-306.md) Rec 2 (`Origin` on `OriginBased<TDelta>`).
- Touches: `algebra-*.concepts.plato` and `quantities.library.plato` — the same files plato-306 edits. Do not run concurrently with a plato-306 agent.

## Fix approaches

1. **Move `Zero` from `NumericalLimits` to `Additive`** (recommended). One generic fill covers all 50 quantity types via `FromAmount`. `Numerical` still reaches `Zero` transitively (it inherits both), so no existing implementor loses anything and no concrete `Zero` body changes.
2. **Add `Zero` to `Additive` and leave the `NumericalLimits` copy.** Avoids touching the numeric tower, but a type inheriting both then owes the same member twice; duplicate-obligation behaviour in the checker is unverified and the redundancy invites drift.
3. **Derive it in a library: `Zero(x: Additive) => x.Subtract(x)`.** No interface change at all, but it overlaps the existing `NumericalLimits.Zero` obligation and six concrete `Zero` bodies — overload ambiguity for a line that would be deleted by approach 1 anyway. Also silently wrong for NaN carriers.

## Bedrock

Fixes the invariant that the *interface comment is the contract*: `Additive` says
"additive group" and will then carry the group's four pieces. It also draws the
seam `NumericalLimits` was blurring — algebraic identity (`Zero`) on the
algebra interface, representational limits (`One`, `MinValue`, `MaxValue`) on the
number-tower interface. That split is what makes `Quantity` and `MatrixLike` able
to be additive groups without pretending to be numbers.

Downstream it makes every identity-seeded generic derivation reachable from a
plain `where T: Additive` bound: fold-with-identity, empty-sum, accumulator
seeds, and the `Origin` of plato-306.

**Verdict: simplest-along-the-grain.** The simple fix must NOT take approach 3
(a library-derived `x.Subtract(x)`): it leaves the interface still lying about
being a group, keeps `Zero` on `NumericalLimits`, and adds an overload that
collides with the six concrete bodies — closing the symptom while making the
real move harder.

## Done means

- [x] `Zero(x: Self): Self` declared on `interface Additive`; removed from `NumericalLimits`; both doc comments updated to match what each now carries.
- [x] `Zero(x: Quantity) => x.FromAmount(0.0)` added to `quantities.library.plato`, discharging the new obligation for all 50 quantity types.
- [x] No existing `Zero` body changed or duplicated (`Vector`, `Color`, `Complex`, `Proportion`, `Percent`, `Probability`).
- [x] `lint submodules/Plato/stdlib` reports 0 parse / 0 resolution findings, no worse than the pre-change run.

Landed in Plato `45dbd6e`. Lint measured on a pristine-compiler copy (a concurrent
session had an uncommitted `Compilation.cs` change in the shared tree that turns a
pre-existing `Twist2D`/`Twist3D` duplicate between `kinematics.plato` and
`deformations.plato` into a fatal resolution error; that session is fixing it):
0 parse / 0 resolution both sides, LINT001 flat at 278, LINT003 2302 -> 2293.

The `MatrixLike` cost predicted under *Simplest fix* materialised as expected and
was paid in the same change: 6 concrete `Zero` bodies in
`numeric-structures-matrix.library.plato`, since `MatrixLike` exposes no
construction primitive (the same gap that blocks a generic `Transpose`).

## Simplest fix

Move the one line. `Zero(x: Self): Self` leaves `NumericalLimits`, joins
`Additive`; add the single `Quantity` fill.

Pros: `Additive` becomes the group it claims to be; 50 quantity types and every
`MatrixLike` gain an identity; `where T: Additive` bounds become useful for
identity-seeded generics; no concrete body changes.

Cons: `MatrixLike` now owes `Zero` and has no fill — it gains a throwing stub
where it previously had no member at all. That is a truthful signal (a matrix
type genuinely should have a zero) but it is a new stub, and filling it needs a
per-shape constructor this vocabulary may not have yet. Verify at lint time and
file separately if the fill is not cheap.

## Prevention

- The `Additive` case is a doc-vs-declaration mismatch that no gate checks. A linter rule of the form "interface comment cites a named algebraic structure ⇒ the declared members cover that structure's operations" is not realistically automatable, but a **review checklist line in `LIBRARIES.md`** — *if the comment names a structure, list its laws and check each has a member* — is. Offer to file.
- Law coverage: `stdlib-legacy-tests` has no `Law_AdditiveIdentity` (`x.Add(x.Zero) == x`, `x.Zero.Add(x) == x`). Adding it would have surfaced the gap on the first type that could not express it. Worth its own issue at the class level.
