---
id: plato-347
title: Adopt Constants library at more call sites
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-283, tracker/DONE.md]
---

## Idea
`Constants` library shipped (DONE plato-272: Pi helpers, UnitX/Y/Z, etc.) but many call sites still hard-code literals or re-derive values. Adoption debt: use Constants where they already exist.

## Assumptions
- Constants file is the sanctioned home for shared numeric/geometric literals.
- Some "missing constants" may actually be missing *entries*, not just unused ones — separate follow-ups.
- Zero/One/Pi left as intrinsics/concept members by design (plato-272 outcome).

## Design decisions
- **Scope** — only replace duplicates of existing Constants vs also add new named constants when patterns repeat.
- **Lint** — magic-number lint against known Constants values vs manual sweep.
- **Domain folders** — color constants already have plato-283.

## Related
- DONE plato-272 — Constants library shipped.
- [plato-283](plato-283.md) — color constants folder.
- `stdlib` call sites still using raw `3.14159…` / unit axes (audit needed).

## Approaches
Short term: grep for UnitX/Y/Z, TwoPi, degree/radian factors; replace with Constants.
Long term: lint + expand Constants for repeated domain literals.
Adjacent: per-domain constant modules (colors, units).

## Bedrock
No new architecture — dogfoods the Constants seam plato-272 added. Verdict: **simplest**.

## Done means
- [ ] Audit list of remaining duplicate literals for existing Constants members
- [ ] High-traffic files switched
- [ ] Any truly missing repeated literals filed as follow-ups (not silently invented in this issue)

## Simplest possible implementation
Targeted replace in geometry/transform libraries that already import Constants neighbors.
- Pros: small diffs; reinforces habit.
- Cons: incomplete; no enforcement.

## Case against
- Local literals can be clearer than hunting a Constants name.
- Over-centralizing one-off numbers creates noise in Constants.
- Verdict: **pursue** for duplicates of *existing* members; **park** open-ended "add more constants" without evidence.
