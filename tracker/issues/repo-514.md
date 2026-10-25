---
id: repo-514
title: Optimizer fixtures assume the default test output directory
type: bug
status: idea
priority: p3
effort: S
risk: low
area: repo
sprint: 
created: 2026-10-25
closed:
links: []
---

Private `dotnet test --artifacts-path .temp/test-speed/artifacts` runs fail in FlatMapFilterTests because fixture discovery derives the source folder from the output directory. Sixteen cases report DirectoryNotFoundException for `.temp/test-speed/artifacts/bin/PlatoTests/Optimization/FlatMapFilterFixtures/filter.plato` instead of using the repository fixtures. This blocks isolated test builds in shared checkouts. Default-layout runs are unaffected. Use source-relative lookup or copied content, following CheckerTestSupport. Small test-infrastructure fix; do not change optimizer behavior.

Evidence: `.temp/test-speed/compiler-default.trx` and `.temp/test-speed-compiler-default.log` from repo-513.
