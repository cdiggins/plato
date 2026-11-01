---
id: compiler-539
title: Isolate native optimization context per writer
type: bug
status: done
priority: p1
effort: S
risk: low
area: compiler
sprint: 
created: 2026-11-01
closed: 2026-11-01
links: []
---

Each CppWriter now owns its optimization host. Native function transformation saves, resets and restores the shared passes' thread-local rename/fusion/loop counters. The regression initially failed on accumulated loop names, then passed after counter scoping: two independent compilations with different flags, three concurrent repeats, exact generated-output comparisons. Recorded Release build: zero errors. Shared transformation algorithms and order are unchanged; the broader backend-neutral context remains a later chunk.
