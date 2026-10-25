---
id: docs-527
title: Clarify implemented status of historical sum-type design
type: debt
status: done
priority: p2
effort: S
risk: low
area: docs
sprint: 
created: 2026-10-25
closed: 2026-10-25
links: [docs/design/plato-sum-types-design-2026-07-27.md]
---

The July proposal still reads as an implementation plan and mixes a generic-sum
status note with superseded emission claims. Clarify its historical role without
changing language behavior or duplicating the normative specification.

## Done means

- [x] Mark the proposal historical and link current semantics and generic-sum decision.
- [x] Correct misleading generics, representation, and recursion statements using source evidence.
- [x] Check edited-document links and whitespace; record the limits of verification.

## Verification

Documentation-only change. Inspect the checker, C# sum writer and sum tests; do not
claim executable coverage from source inspection. Existing root PROGRESS.md belongs
to another mission and is left untouched.

Local Markdown targets resolved; git diff --check passed. Source inspection confirms
generic sums default on, generic C# structs, wrapper payloads, active-case equality
and JSON ToString. Direct recursion remains a documented representation inference;
no recursive program or compiler suite was executed for this documentation edit.
