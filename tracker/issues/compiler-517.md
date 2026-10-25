---
id: compiler-517
title: Bound the compiled arm of differential witnesses
type: bug
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [plato-brep TKT-1025]
---

## Problem

The differential backend keeps interpreter work serial, but its cooperative deadline does not bound the following compiled invocation, host-value conversion or structural comparison. A fast interpreter witness can therefore exceed its witness cap inside the compiled arm. This predates compiler-515.

## Evidence

The unchanged compiler36850a37 Release binaries reproduce two controlled failures through the real DifferentialBackend: blocked invocation and blocked conversion miss the cap assertion; the fast-arm control passes. All three cases run. Finite release gates, cleanup joins and an external30second process watchdog avoid runaway test workers. The elevated same-binary retry completed naturally; an earlier sandbox launch timeout is preserved without attributing its cause.

Evidence and regression source are committed in plato-brep21f03c4e under staging/differential-witness-cap/regression. TKT-1025 tracks consumer integration. Recursive comparison coverage is part of the fix, not claimed by the original red gate.

## Acceptance

- [ ] A configured deadline bounds compiled invocation, conversion and comparison without running the shared interpreter on an abandoned worker.
- [ ] Timed-out workers cannot publish late mismatches, and the next law can execute with the deadline cleared.
- [ ] Cold and cached witness requests, genuine mismatches, fault classification and unsupported conversion retain their intended behavior.
- [ ] Focused tests pass against the changed assembly; the committed production compiler and pinned plato-brep consumer are verified separately.

## Ownership

The existing compiler wave explicitly released DifferentialRun.cs and LawRunner.cs to this track. Candidate edits live in plato-brep staging; upstream production source is unchanged. Preserve unrelated shared status-report, experiment and scratch files.
