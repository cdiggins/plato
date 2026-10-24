---
id: compiler-478
title: Navigation corpus rebuild exceeds its timing budget on this host
type: problem
status: in-progress
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

`DegradedModeTests.FullCorpusBuildStaysInsideItsBudget` failed on this Windows host with both original `876fe36b` assemblies and the repo-468 organization changes. For the same 194-file corpus, its 7,760 ms budget was exceeded by the original warm rebuild (21,255 ms) and the reorganized warm rebuild (17,664 ms). These were shared-machine runs, not a controlled performance experiment; the results establish a baseline failure, not a newly introduced slowdown.

The test currently cannot certify navigation latency on this host. Repeat under controlled load and profile parsing, binding and index construction before deciding whether to optimize the rebuild or revise how the test measures its contract. Do not raise the budget merely to turn this run green. Navigation rebuild is distinct from CLI C# generation, whose paired comparison is recorded in repo-468.

Reproduce: `dotnet test tests/Plato.Navigation.Tests/Plato.Navigation.Tests.csproj -c Release --filter Name=FullCorpusBuildStaysInsideItsBudget`.

## Done means

- [x] Reproduce with host/load details and identify the dominant cost or measurement instability.
- [x] Restore a meaningful, repeatable navigation latency check, with any performance change compared against the original behavior.

## Resolution and limits

With other owned timed workloads stopped, fresh rebuilds satisfy the unchanged 40 ms/file ceiling. An explicit phase probe separates parsing, binding and indexing. The reorganized/fixed path measured fresh rebuilds at 4.83–7.39 s and cached updates after priming at 0.77–1.30 s; the preserved original assemblies measured 3.93–6.91 s and 0.58–0.66 s respectively. These sequential phase runs diagnose cost and host variability; they are not an interleaved speedup experiment. Parsing dominates the fresh path.

The existing full-rebuild test and its ceiling remain intact. A separate warm-update test primes the real `IncrementalIndexer`, requires every parse to be reused, checks stable definition/reference totals, and applies the same budget. During the complete navigation suite, fresh rebuild was 4.242 s and cached update 0.841 s against a 7.760 s ceiling. All 73 executed navigation tests passed. The README now names both checks and the explicit phase probe instead of stale measurements and retired gates.

No production performance optimization was needed or claimed. This resolves the investigation through reproducible diagnosis and independent coverage of both paths; it does not promise wall-clock stability under competing workloads or eliminate environmental variation. Original and fixed assemblies differ intentionally by the 161 assignment references restored under compiler-472.
