---
id: repo-492
title: Audit duplication with dotnet-greenhouse
type: problem
status: in-progress
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

## Done means

- [x] Run Greenhouse duplicate analysis and distinguish current sources from backups and generated artifacts.
- [x] Manually inspect ranked code/test candidates and document semantic risks and reproducible commands.
- [x] Capture implementation work and tooling gaps as separate follow-ups.

## Outcome

Report: `docs/reports/duplication-audit-2026-10-24.md`. Exact scan completed in 14.23 seconds; scoped similarity completed in 0.95 seconds. The best opportunities are shared writer function selection and remaining constraint-codegen fixture machinery. Identical memoization/value-equality loops are deliberately rejected as a mechanical merge. No runtime or test code changed; build and integration gates were not needed for this analysis-only milestone.

repo-493 tracks consolidation; repo-494 captures optional Greenhouse scope/ranking improvements. Root PROGRESS.md belongs to another mission and was left untouched.
