---
id: compiler-478
title: Navigation corpus rebuild exceeds its timing budget on this host
type: problem
status: idea
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

- [ ] Reproduce with host/load details and identify the dominant cost or measurement instability.
- [ ] Restore a meaningful, repeatable navigation latency check, with any performance change compared against the original behavior.
