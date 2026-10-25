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

- [x] A configured deadline bounds compiled invocation, conversion and comparison without running the shared interpreter on an abandoned worker.
- [x] Timed-out workers cannot publish late mismatches, and the next law can execute with the deadline cleared.
- [x] Cold and cached witness requests, genuine mismatches, fault classification and unsupported conversion retain their intended behavior.
- [ ] Focused tests pass against the changed assembly; the committed production compiler and pinned plato-brep consumer are verified separately.

## Ownership

The existing compiler wave explicitly released DifferentialRun.cs and LawRunner.cs to this track. The verified candidate and three regression fixtures are promoted together; LawRunner.cs remains unchanged. Preserve unrelated shared status-report, experiment and scratch files.

## Verified implementation

The absolute witness deadline now bounds compiled invocation, conversion and
recursive comparison. Interpreter work stays on its serial caller. Only the
caller publishes a completed mismatch; late worker results are discarded. A
background worker may outlive the deadline, so this bounds caller latency rather
than forcibly terminating uncooperative compiled code. Null deadlines preserve
synchronous behavior. Active deadlines require a parallel-safe compiled backend.

Private verification passed71 distinct cases:15 focused deadline regressions,
3 existing differential semantics cases,47 exact budget/selection controls and
6 fault-classification cases. Both private builds had zero warnings/errors.
All cases execute without skips against candidate Plato.Laws SHA256
1c7503653fd7cdc0f9a87a62d3acd7d5800e1a536a2d9f36f952f17db615d1e3;
input inventories, embedded resources and actual runtime identities are unchanged.
The original invocation/conversion failures remain preserved as red evidence.

Source, drivers, complete case roster and receipt hashes are committed in
plato-brep d52002b3, staging/differential-witness-cap/fix/unit-gate-summary.json.
Two earlier selector attempts reached the unchanged watchdog because selection
included extra cases; they are retained as incomplete, not passing validation.
The accepted native NUnit selector requires exactly one named case per child.

The final acceptance box remains open: build the actual committed production
compiler, verify its consumers, then move the plato-brep pin. The private assembly
is not a substitute for that final committed-build evidence.
