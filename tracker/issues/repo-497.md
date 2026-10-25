---
id: repo-497
title: Parameterize array receiver emission test cases
type: debt
status: ready
priority: p3
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

## Scope

Deferred from repo-493 because the user selected only shared writer selection and generated-code helper consolidation. ArrayReceiverLibraryEmissionTests has three similar cases for concrete receivers, generic receivers and declared-only stubs; their different expectations must remain visible.

## Done means

- [ ] Parameterize the three cases with descriptive names or share their assertion helper, preserving every expected signature and stub assertion.
- [ ] Run the affected fixture and measure before claiming any speed benefit; avoid sharing mutable compiler/writer state.
