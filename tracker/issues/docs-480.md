---
id: docs-480
title: Correct optimizer documentation for implemented fusion
type: debt
status: in-progress
priority: p2
effort: S
risk: low
area: docs
sprint: 
created: 2026-10-24
closed:
links: [docs/csharp-optimizer-pipeline.md]
---

The optimizer reference still describes fusion and consumer lowering as future work even though the writer implements both. This can send performance investigations toward already shipped transformations.

## Done means

- [ ] Document the source-verified pass order, flags, fusion, literal unrolling, and FlatMap lowering.
- [ ] Correct the worked example and remaining-gap list, including the TKT-507 filter-idiom reference.
- [ ] Refresh directly linked current optimizer summaries, preserve historical plan content, and verify documentation references.
