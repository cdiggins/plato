---
id: repo-489
title: Consolidate codegen verification and isolate CLI responsibilities
type: debt
status: in-progress
priority: p2
effort: M
risk: low
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

## Done means

- [ ] Generated-C# regression fixtures share compilation/reference policy and preserve executable assertions.
- [ ] CLI source loading, linting and generated-file persistence have separate reusable responsibilities, with equivalent command results and generated output.
- [ ] Focused and integrated verification is recorded, each milestone committed, and remaining repo-wide debt identified through existing tracker items.

## Scope

Apply the Platonic Coder priorities to the next incremental pass after repo-468. Preserve public CLI entry points, backend selection, input ordering, diagnostics and exit codes. Do not modify language semantics or generated runtime representations. Existing correctness and architecture backlog remains explicit; this issue does not certify the entire repository free of debt.

Root PROGRESS.md belongs to plato-448; this issue and milestone commits hold this mission's checkpoint instead.
