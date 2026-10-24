---
id: docs-480
title: Correct optimizer documentation for implemented fusion
type: debt
status: done
priority: p2
effort: S
risk: low
area: docs
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: [docs/csharp-optimizer-pipeline.md]
---

The optimizer reference still describes fusion and consumer lowering as future work even though the writer implements both. This can send performance investigations toward already shipped transformations.

## Done means

- [x] Document the source-verified pass order, flags, fusion, literal unrolling, and FlatMap lowering.
- [x] Correct the worked example and remaining-gap list, including the TKT-507 filter-idiom reference.
- [x] Refresh directly linked current optimizer summaries, preserve historical plan content, and verify documentation references.

Validation: checked the documented pass order and flags against the writer and both CLI commands, verified local Markdown links and source-file references, and passed `git diff --check`. Examples describe source-derived transformations rather than a fresh CLI transcript. Full build/regeneration gates were not run for this documentation-only change; their recording scripts would overwrite another session's status artifacts.
