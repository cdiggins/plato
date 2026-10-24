---
id: compiler-495
title: Document optimizer components with Greenhouse and freshness gate
type: feature
status: done
priority: p2
effort: M
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: []
---

Produce a source-linked optimizer handbook using dotnet-greenhouse discovery. Keep agent maintenance guidance separate and enforce documentation freshness when implementation changes.

## Done means

- [x] Provide overview and component guides covering algorithms, motivation, references, usage, tests and effectiveness boundaries.
- [x] Implement reproducible source evidence generation and a CI check requiring review after dependency changes.
- [x] Verify generation, stale-input detection and links; document maintenance commands separately.

Delivered [the handbook](../../docs/optimizer/README.md), Greenhouse-generated symbol evidence and source map,
and [Optimizer Documentation Stewardship](../../tools/optimizer-docs/MAINTENANCE.md) outside the reader docs.
CI checks source hashes and requires explicit prose-review attestation; deterministic rendering does not bless stale evidence.

Verification: Greenhouse documentation tests passed and the partial-class exporter fix is committed in that repository as `0a4e8f5`.
The documentation maintenance tests and link/freshness check pass; focused optimizer tests pass, and the recorded full Plato suite passes.
The integration battery's external SDK tripwire remains red in the category already tracked by repo-473.
[Dated evidence](../../docs/reports/optimizer-handbook-validation-2026-10-24.md) records commands, allocation results and gate timings.
Root PROGRESS.md was occupied by another mission and was preserved. Remote publication remains pending approval after an automatic review rejection.
