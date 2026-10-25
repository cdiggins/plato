---
id: docs-525
title: Reconcile language reference contradictions with compiler evidence
type: debt
status: ready
priority: p1
effort: M
risk: low
area: docs
sprint: 
created: 2026-10-25
closed:
links: [docs/reference/implementation/discrepancies.md]
---

The reference audit found disagreement between normative prose, contributor instructions,
historical pipeline descriptions and compiler behavior. This can lead an author to reject
valid generic sums, assume a stale codegen path, or rely on an incomplete loop lowering.
The source-evidenced discrepancy register is maintained by docs-523; reconciling the original
authorities and deciding semantic changes is separate work.

## Done means

- [ ] Review each entry in `docs/reference/implementation/discrepancies.md` against compiler tests.
- [ ] Correct obsolete authority and contributor prose, preserving explicit decisions rather than inferring new semantics.
- [ ] Give unresolved compiler defects (including foreach lowering) their own reproduction tests and tracker issues.
- [ ] Update the discrepancy register when each conflict is settled.
