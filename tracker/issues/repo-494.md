---
id: repo-494
title: Give Greenhouse audits explicit source scope and useful ranking
type: idea
status: idea
priority: p2
effort: M
risk: low
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

## Observed problem

Greenhouse's directory-mode ProjectPaths enumerates all descendant C# files and filters generated paths, but does not exclude `.temp`, nested worktrees, vendor submodules or archived `bonepile` source. Running `analyze . --format duplicates` in Plato on 2026-10-24 returned 2,999 clusters, mostly inflated by backup copies. Filtering to current `src`, `writers` and `tests` and removing `bonepile` left 84 candidate clusters. These are syntax matches, not equivalent-behavior proofs.

## Proposed improvement in dotnet-greenhouse

Add explicit repeatable source-root/exclusion or tracked-file selection to the CLI, with an included/excluded file summary. Preserve a way to scan intentionally untracked fixtures. Rank exact matches by repeated nontrivial tokens and provide structured output without invoking the expensive similarity pipeline. Verify exclusions with temporary fixtures, including nested backups, generated files and ordinary untracked code. Avoid hard-coding Plato-specific directories into Roslyn's generic generated-file classifier.

The CLI's documented one-line suppression does not apply to its exact-body index: MethodBodyIndex indexes every nonempty normalized method body. Correct that documentation or expose an explicit threshold with tests. Bounded MCP duplicate discovery already exists; reuse its policies where applicable rather than building a second analyzer.

This is a medium tool usability improvement, not a detected Plato correctness bug. Scoped CLI runs and filtering were sufficient for repo-492, so no Greenhouse source changes were necessary to complete that audit. Implementation belongs in the sibling dotnet-greenhouse repository.
