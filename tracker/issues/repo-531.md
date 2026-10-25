---
id: repo-531
title: Test fixture discovery mistakes isolated output for source tree
type: bug
status: idea
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-25
closed:
links: [tests/PlatoTests/CheckerTestSupport.cs]
---

Reproduced during docs-523 verification with UseArtifactsOutput=true and an isolated
ArtifactsPath. CheckerTestSupport.FindFolder searches AppContext.BaseDirectory before the
CallerFilePath source fallback. Its upward walk accepts the first existing directory named
PlatoTests. The isolated output tree contains bin/PlatoTests, so this output folder wins and
the source fallback is never reached.

ExactOutputTests, FlatMapFilterTests, MultiConsumerFusionTests and StatementInliningTests then
append Optimization fixture paths to the wrong folder. The retained TRX for
`.temp/gate-logs/20261025-114436-991105` reports 51 failures, all DirectoryNotFoundException
for these fixture groups; this is not evidence of optimizer result failures.

Representative message: Could not find a part of the path
`.temp/reference-gate-artifacts/bin/PlatoTests/Optimization/ExactOutputFixtures/exact-output.plato`.
Use the already implemented source fallback first, or verify candidates contain the required
source fixtures. The helper's comment already promises support for private artifacts paths.

## Done means

- [ ] Add a regression where an output folder named PlatoTests coexists with source fixtures.
- [ ] Find source fixtures correctly with both normal and isolated artifact layouts.
- [ ] Run affected optimizer tests and reconcile the reference's verification-layout caveat.
