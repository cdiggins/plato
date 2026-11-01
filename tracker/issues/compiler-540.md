---
id: compiler-540
title: Destroy native scratch values on rollback and reset
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

Canonical runtime now registers nontrivial elements, destroys in reverse order on rollback/reset/thread exit, frees thread-owned blocks, and checks allocation-size arithmetic. Native mark API carries owner/reset generation; expired marks fail closed. Header synchronized into CppCpuPrelude.cs. Recorded Release build zero errors; ScratchArenaTests passed 2/2, including existing runtime values, nested marks across blocks, retained strings, 100 rollbacks, thread exit and abort cases for byte overflow/reset-expired marks. Remaining: consumer host integration and freshly regenerated BREP validation; do not promote this runtime into a host that rewinds block offsets directly.

Consumer host now uses the destructor-aware mark/rollback API when available, retaining the historical adapter only for old runtimes. Frozen f6823eb7 regenerated build passed; SAM_AP203 strict mesh parity passed with one and two workers, and c211 passed with row rollback enabled. Integrated native regression selection passed 9/9. Broader ownership/storage and aggregate worker-memory validation remain open; strict front-end wire blockers are tracked in compiler-542.
