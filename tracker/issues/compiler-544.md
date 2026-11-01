---
id: compiler-544
title: Borrow large readonly native record parameters
type: feature
status: in-progress
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-11-01
closed:
links: []
---

## Done means

- [x] Borrow large closed readonly record parameters using native layout; preserve writable parameters, small values and owned return signatures.
- [x] Native execution tests cover exact signatures, temporary arguments, copied string payloads and parameter assignment.
- [ ] Frozen BREP candidate passes aligned stage, law, decode and mesh gates.
- [ ] Alternating native measurements record performance and correctness on every sample.

Focused verification: cpp-large-parameters recorded build has zero errors; LargeParameterTests, StringBorrowingTests, ScratchArenaTests and SharedCallsParityTests pass 9/9 (.temp-brep-large-parameters-tests.log). Full BREP and performance gates are pending; no speedup claimed.
