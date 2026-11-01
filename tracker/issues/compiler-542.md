---
id: compiler-542
title: Align native text encoding and sum JSON defaults with C#
type: bug
status: in-progress
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

Repair chunks (2026-11-01): e4225c63 adds explicit null/present array state, preserving nested JSON null versus []; 373ceaae changes native Character/String to UTF-16 code units with explicit UTF-8 boundaries and Latin-1 Bytes unchanged. Native runtime/Unicode checks pass, integrated native regressions pass 10/10, and C++/CUDA dialect isolation passes 2/2. eb508247 centralizes typed defaults for omitted JSON members without weakening explicit Character validation; its generated-record and runtime selection passes 4/4.

Frozen consumer candidate 373ceaae regenerates the same 1771 C# source files byte-for-byte as f6823eb7. Actual provenance matches the prepared manifest. Managed shared laws pass 5/5, isolated decode tests pass 90/90, and the managed host builds with zero warnings/errors. Full native build/stage/mesh gates remain pending. This candidate predates eb508247; the later typed-member follow-up still needs inclusion in the final frozen candidate. Consumer Unicode fixture preflight confirms Greek and a supplementary character are actually decoded; no comparator normalization was introduced.
