---
id: plato-356
title: Make TimeInterval implement IntervalLike
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-333, plato-357, stdlib/time.plato, stdlib/intervals-bounds.concepts.plato]
---

## Idea
`TimeInterval` (`stdlib/time.plato`) implements only `Value` with `Start`/`End: Instant`, while `NumberInterval` implements `IntervalLike<Number>`. TimeInterval should participate in the Interval interface family. Comment on TimeInterval already describes a half-open span.

## Assumptions
- `IntervalLike<T>` requires `T: Additive, Interpolatable, Comparable` (`intervals-bounds.concepts.plato`).
- `Instant` today implements `Value, Comparable, OriginBased<Duration>` — **not** Additive or Interpolatable, so a naive `implements IntervalLike<Instant>` will fail constraints.
- Duration is the natural delta (OriginBased already says so).

## Design decisions
- **Fix Instant first** — add Additive/Interpolatable via Duration, vs loosen IntervalLike constraints, vs TimeInterval-specific interface.
- **Openness** — half-open (comment) vs closed IntervalLike helpers (see plato-333 IntegerInterval tension).
- **Naming** — IntervalLike vs a TimeIntervalLike.

## Related
- `stdlib/time.plato` — TimeInterval, Instant.
- `stdlib/intervals-bounds.concepts.plato` — IntervalLike.
- [plato-333](plato-333.md) — IntegerInterval half-open vs closed IntervalLike bodies.
- [plato-357](plato-357.md) — times as coordinates (broader).

## Approaches
Short term: make Instant satisfy IntervalLike's `where` (Additive/Interpolatable through Duration) then `TimeInterval implements IntervalLike<Instant>`.
Long term: share Span/Contains/Clamp helpers; document half-open semantics.
Adjacent: LabeledTimeInterval alignment.

## Bedrock
Closes the **interval interface gap for time** and forces Instant into the additive story OriginBased already implies. Verdict: **right**. Simple version must NOT pretend Instant+Instant is meaningful — only Instant+Duration.

## Done means
- [ ] TimeInterval implements IntervalLike (or documented specialized sibling)
- [ ] Instant meets required constraints without illegal Instant+Instant
- [ ] Contains/Span behavior documented as half-open if that remains the rule

## Simplest possible implementation
Add Instant interpolatable/additive-via-Duration members; implements line on TimeInterval; reuse IntervalLike library helpers carefully for openness.
- Pros: one interface family.
- Cons: may need helper overrides for half-open (plato-333 lesson).

## Case against
- Time is not a linear Number; forcing IntervalLike may misuse floating SecondsSinceEpoch.
- Half-open vs closed will repeat IntegerInterval bugs.
- Verdict: **pursue** with explicit half-open tests; watch plato-333.
