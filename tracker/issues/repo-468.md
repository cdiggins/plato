---
id: repo-468
title: Organize source by responsibility and document repository navigation
type: debt
status: done
priority: p2
effort: L
risk: med
area: repo
sprint: 
created: 2026-10-23
closed: 2026-10-23
links: []
---

Make the repository easier to navigate and divide among agents, following the Platonic Coder documentation and architecture guidance. Preserve language behavior, public APIs and build entry points.

## Done means

- [x] Compiler sources have responsibility-based groups, useful file headers and folder guides.
- [x] Writer sources have responsibility-based groups, useful file headers and folder guides.
- [x] Runtime and navigation sources have responsibility-based groups, useful file headers and folder guides.
- [x] The repository documentation audit is addressed, with format and ownership exceptions explained and remaining improvements tracked.
- [x] Integrated checks are recorded and each independent chunk has a reversible commit.
- [x] Repeated original-versus-reorganized compiler measurements show no significant regression on the same input, with equivalent generated output.

## Plan

See [the organization plan](../../docs/archive/repository-organization-plan-2026-10-23.md).

## Validation outcome

The [dated results](../../docs/reports/repository-organization-results-2026-10-23.md) and its evidence JSON record builds, tests, source/documentation audits, output equivalence and timing samples. CLI and affected project builds passed; interpreter, exporter, intrinsic runtime and policy regression suites passed. Compiler and navigation failures were reproduced with original assemblies; the external studio frozen-file failure is separately tracked. No thresholds, accepted inventories or frozen manifests were relaxed. The gate battery remains red on those existing failures, which are not claimed as resolved here.

The repeated comparison found equivalent generated C# and no material median compile-time regression for the tested foundation recipes. Shared-machine variation limits the conclusion to this workload; no algorithmic speedup or universal performance guarantee is claimed. All discovered behavioral fixes beyond safe organization have separate tracker items.
