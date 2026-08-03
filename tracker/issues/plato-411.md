---
id: plato-411
title: Signed distance fields do not say whether their values are exact, a lower bound, or neither
type: problem
status: done
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-02
closed: 2026-08-02
links: [stdlib/geometry/implicit-sdf.concepts.plato, stdlib/geometry/implicit-sdf.library.plato, tracker/issues/plato-409.md, tracker/issues/plato-287.md]
---

## Problem

`implicit-sdf.concepts.plato` says an SDF's values "are exact distances or conservative lower
bounds of them", and then never distinguishes the two again. Nothing in the vocabulary — not
a type, not an interface, not a field — records which one a given field is, so a caller that
needs exactness has no way to ask and no way to be warned.

That would be a documentation nit except that the two-way split is itself wrong. There are
**three** cases, and the library currently names only two of them:

- **Exact** — the value is the true signed distance, so `|∇f| = 1` almost everywhere (the
  eikonal property). Sphere tracing takes maximal steps; `Offset` produces the true offset
  surface; the unit gradient is the true surface normal.
- **Lower bound** — the value never exceeds the true distance. Sphere tracing still
  converges, just with shorter steps; `Offset` and normals are wrong but marching is safe.
- **Not a bound at all** — the value may EXCEED the true distance. A sphere tracer stepping
  by it tunnels straight through surfaces. This is a correctness failure, not a quality one.

The third case exists in the library today and is mislabelled as the second.
`ApplyToDomain(SdfTwistModifier3D)` and `ApplyToDomain(SdfBendModifier3D)` both say "The
result is a bound, not an exact distance" — true for a gentle twist, false for a strong one,
because a domain warp with derivative greater than 1 inflates distances rather than
shrinking them. Same for `ApplyToDistance(SdfDisplacementModifier3D)` once Amplitude exceeds
the displacement field's own Lipschitz constant. The comments are reassuring in exactly the
direction that hurts.

## Assumptions

- Sphere tracing is the consumer that makes this matter. `RayMarch` (landed in plato-409) is
  the first thing in the repo that can be broken by an over-estimating field.
- No caller today needs the distinction resolved at compile time; the failures are runtime
  artefacts (tunnelling, wrong normals), not type errors.

## Design decisions

**Should this be an interface split — `IExactSignedDistanceField*` beside the existing one?
Recommendation: no.**

- Exactness is a property of a VALUE's construction history, not of a type. `FunctionSdf3D`
  holds a lambda; the same type holds an exact sphere and an inexact twisted thing. A marker
  interface would be unenforceable and wrong half the time it was applied.
- The interface lattice cannot afford it. There are already four SDF types crossed with
  bounded and differentiable variants; adding an exactness axis multiplies that again, and
  plato-229 is actively trying to shrink the lattice, not grow it.
- The one place a type COULD carry it honestly is a wrapper that is constructed from known
  parts — which is data, not an interface.

**Recommended encoding, in increasing cost:**

1. **Three-way doc vocabulary, applied uniformly.** Every primitive and every operator states
   which of the three it returns, in the same words each time. Free, and it removes an
   actively misleading comment on the twist and bend modifiers. This is the part worth doing
   regardless of what follows.
2. **`DistanceFidelity` as data on the wrapper types**, following the plato-287 precedent
   where `BoundedSdf` carries its `Bounds` as a field rather than changing what distance
   means. A sum type (`Exact | LowerBound | Unbounded`) that the CSG operators combine:
   union of two exacts is exact, intersection is at best a lower bound, anything smooth is a
   lower bound, twist and bend are unbounded. `RayMarch` can then refuse, or fall back to a
   fixed step, rather than silently tunnelling.
3. **A Lipschitz constant instead of an enum.** Strictly more useful than the three-way tag:
   many inexact fields are k-Lipschitz for a known k, and dividing the reported distance by k
   makes them safe to march again — recovering correctness rather than merely reporting its
   absence. `Exact` is then just k = 1. The cost is that every operator must compute a
   composed k, which is real work and occasionally impossible.

The honest summary: (1) is overdue, (2) is the plato-287-shaped answer, (3) is the right
answer if anyone ever wants guaranteed-correct marching over warped fields.

## Related

- [plato-409](plato-409.md) — landed `RayMarch`, the first consumer that an over-estimating
  field can actually break, and `EikonalResidualAt`, which measures the departure from
  exactness empirically when the label is not trusted.
- [plato-287](plato-287.md) — the precedent for carrying a claim as DATA on a wrapper
  (`Bounds`) rather than as a change to what the field means.
- [plato-229](plato-229.md) — the interface-lattice work this issue must not fight; an
  exactness interface axis is exactly the kind of growth that item is pushing back on.
- [stdlib/geometry/implicit-sdf.library.plato](../../stdlib/geometry/implicit-sdf.library.plato) —
  the twist / bend / displacement comments that are wrong today.

## Approaches

Short term: fix the three misleading comments and write the three-way vocabulary into
`implicit-sdf.concepts.plato` so there is one place that defines the words. Half a day, no
API change, and it stops the library from telling a caller that a twisted field is safe to
march when it is not.

Long term: `DistanceFidelity` on the wrappers, then a Lipschitz constant if guaranteed
marching over warped fields is ever wanted.

## Bedrock

The seam is the **contract of `ISignedDistanceField*` itself** — what a caller may assume
about a returned number. It is currently one sentence in a header comment that covers two of
three cases. Naming the third case is what lets `RayMarch` and any future mesh extractor
state their own preconditions in terms of it, instead of each re-deriving when a field is
safe. Fixing it in the doc layer keeps the type lattice flat, which is the property worth
protecting.

Verdict: **simplest-along-the-grain.** The simple version must NOT introduce an exactness
interface or an exactness type parameter — once the distinction lives in the lattice it
cannot be moved back out to data without breaking every implementor.

## Done means

- [x] `implicit-sdf.concepts.plato` defines exact / lower bound / not-a-bound in one place
- [x] Every distance primitive and every operator in `implicit-sdf.library.plato` states
      which of the three it returns, using those words
- [x] The twist, bend and displacement comments no longer claim to return a bound
- [x] `plato_check` clean

## Simplest possible implementation

Documentation only: the three definitions in the concepts file, and one clause per primitive
and operator. No type, no interface, no field.

**What you get** — a caller can tell whether marching a given composition is safe, and the
one actively wrong claim in the library goes away.

**What you give up / risk** — nothing is enforced, so a future operator can still be
documented wrong; and a caller composing fields at runtime still cannot query fidelity, which
is the case (2) exists to serve.
