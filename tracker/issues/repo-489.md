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

- [x] Generated-C# regression fixtures share compilation/reference policy and preserve executable assertions.
- [x] CLI source loading, linting and generated-file persistence have separate reusable responsibilities, with equivalent command results and generated output.
- [ ] Focused and integrated verification is recorded, each milestone committed, and remaining repo-wide debt identified through existing tracker items.

## Scope

Apply the Platonic Coder priorities to the next incremental pass after repo-468. Preserve public CLI entry points, backend selection, input ordering, diagnostics and exit codes. Do not modify language semantics or generated runtime representations. Existing correctness and architecture backlog remains explicit; this issue does not certify the entire repository free of debt.

Root PROGRESS.md belongs to plato-448; this issue and milestone commits hold this mission's checkpoint instead.

Milestone 1: shared in-memory C# compilation preserves the fixture-specific parse/optimization options. Recorded build passed; focused emission and optimizer checks passed (43 tests, 77 seconds). The reference exclusion retains the existing decision to use embedded runtime wrappers, avoiding duplicate types from Ara3D.Geometry.dll.

Milestone 2: Program retains its public Main/Lint entry points and delegates to focused commands. SourceFiles returns ordered discovery results and GeneratedFiles owns writer persistence. The CLI build passes; tools/check-cli-behavior.py matches the original executable across every command case and emitted file after excluding only existing wall-clock banners.
