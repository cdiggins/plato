---
id: compiler-542
title: Align native text encoding and sum JSON defaults with C#
type: bug
status: ready
priority: p1
effort: M
risk: med
area: compiler
sprint: 
created: 2026-11-01
closed:
links: []
---

Reproduction (2026-11-01): consumer plato-brep commit 412760eae, tools/cpp-validation/test-stages.ps1, frozen compiler f6823eb799399f8d0e832a15063eaa79ca36d104 and library 414a183e845bdfe5de441488726d59dbe8fffc6b. Evidence: artifacts/cpp-validation/candidate-f6823eb7/stage-gate-complete-reports/result.json in the consumer checkout. Seven fixtures, 21 raw structural comparisons: 4 pass, 17 fail, with a saved report for every comparison.

- All seven parsed JSON outputs contain invalid UTF-8: the escaped STEP header author Andre-with-acute produces native byte E9 instead of managed UTF-8 C3 A9.
- Ten decoded/prepared comparisons differ on inactive sum array fields: managed null versus native []. No-shape and truncated decoded/prepared cases pass.
- Diagnostic Latin-1 decoding found only null/empty-array differences beyond the encoding failure. This is diagnostic evidence, not a passing gate or permission to normalize output.

Acceptance: define and document the String/Character encoding and typed sum-field wire/default contract; preserve valid non-Latin-1 text and Bytes round trips; add focused cross-backend regressions; regenerate from committed sources and make all 21 strict fixture comparisons pass without global null/empty normalization, whole-file transcoding, or generated patches. Keep shipping pins unchanged until the aligned laws, decode and mesh gates also pass. Bytes already has its own UTF-8 JSON encoding and must not be double-encoded.
