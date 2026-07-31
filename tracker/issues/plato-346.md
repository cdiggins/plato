---
id: plato-346
title: Adopt implicit converters more widely in stdlib
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-345, plato-323]
---

## Idea
There seem to be many places where an existing implicit converter could be used but call sites still write explicit casts or parallel overloads. This is adoption debt on the implicit-conversion surface, not inventing Convertible (plato-345).

## Assumptions
- Implicit converters already exist for some pairs (stdlib + writer); gaps are unused opportunities, not total absence.
- Over-eager implicits hide cost and create ambiguity; adoption should be selective.
- Forward-stdlib C# errors sometimes show conversion friction (see plato-323 notes on IReadOnlyList→Array).

## Design decisions
- **Inventory first** — audit declared implicits vs call sites that reimplement conversion.
- **Policy** — when is explicit preferred (precision, allocation) vs implicit OK?
- **Tooling** — lint "could use implicit" vs manual sweep.

## Related
- [plato-345](plato-345.md) — Convertible<T> concept (may underpin implicits).
- [plato-323](plato-323.md) — body-level C# errors; conversion friction mentioned.
- Writer/intrinsics implicit conversion tables (host side).

## Approaches
Short term: inventory implicits; fix highest-churn call sites (geometry scalars, mesh collections).
Long term: lint + Convertible-backed implicits.
Adjacent: remove redundant overloads once implicits cover them.

## Bedrock
Strengthens the **one conversion path** invariant: declare once, use everywhere. Verdict: **simplest**. Must NOT blanket-enable all numeric widenings.

## Done means
- [ ] Written inventory of current implicits and top unused opportunities
- [ ] N high-traffic sites switched (N agreed at triage)
- [ ] No new ambiguous-overload regressions in fast gate

## Simplest possible implementation
Manual audit doc in `.temp/` then PR the top 5 call-site cleanups.
- Pros: immediate readability wins; proves value.
- Cons: no systemic fix; inventory rots.

## Case against
- Explicit conversions document intent; "use implicits more" can reduce clarity.
- Ambiguity bugs are costly to diagnose.
- Verdict: **pursue** as a bounded adoption sweep with an allowlist of converter pairs.
