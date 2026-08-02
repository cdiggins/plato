---
id: plato-345
title: Add Convertible<T> conversion interface
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-346, plato-306]
---

## Idea
There is no `Convertible<T>` (or similarly named) interface in Plato today (MCP search empty). The ask is an interface that says "Self can be converted to T" so generic code and implicit-conversion sites share one obligation instead of ad-hoc `ToX` methods.

## Assumptions
- Many pairwise conversions already exist as library methods; discovery and generic constraints are weak.
- An interface is only worth it if the checker/codegen can use it for implicit conversion or generic bounds.
- Overlaps with whatever implicit-converter mechanism already exists (see plato-346).

## Design decisions
- **Direction** — `Convertible<T>` (Self→T) vs bidirectional `Conversion<A,B>` vs marker-only.
- **Implicit vs explicit** — interface enables implicit conversion vs only documents `ToT` / `FromT`.
- **Overlap with existing implicits** — unify with current implicit converter tables vs parallel surface.

## Related
- [plato-346](plato-346.md) — adoption of existing implicit converters.
- [plato-306](plato-306.md) — Difference defaults via optional delta conversion interface (related pattern).
- [plato-308](plato-308.md) — CS0315 / missing boxing conversions for interface Self.

## Approaches
Short term: introduce `interface Convertible<T> { To(self: Self): T }` (name TBD) and implement on a few high-traffic pairs (Integer→Number, Color8→Color).
Long term: checker uses Convertible for implicit slots; lint missing conversions between sibling types.
Adjacent: FromConvertible / bidirectional pair.

## Bedrock
Gives a **named conversion obligation seam** the implicit-converter machinery and generics can share. Verdict: **simplest-along-the-grain**. Simple version must NOT auto-enable every ToX as implicit.

## Done means
- [ ] Interface declared and documented
- [ ] ≥3 real implementers in stdlib
- [ ] At least one generic API constrained by Convertible<T>

## Simplest possible implementation
Marker/obligation interface + manual implements on a handful of pairs; no checker implicits yet.
- Pros: vocabulary exists; cheap to try.
- Cons: without checker use it is documentation theater.

## Case against
- Ad-hoc `ToX` methods are clearer and avoid diamond conversion graphs.
- Implicit conversion + Convertible together can create ambiguity explosions.
- Verdict: **pursue** only if tied to plato-346's implicit story; otherwise **park**.
