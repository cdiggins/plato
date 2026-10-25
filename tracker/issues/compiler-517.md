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
- [x] Focused tests pass against the changed assembly; the committed production compiler and pinned plato-brep consumer are verified separately.

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

## Pinned consumer integration verified

The actual committed fc4b5 production Release compiler and its consumer are
verified separately from the private unit assembly. Plato-brep 0d316ed1 moves
both compiler pin locations and promotes the regenerated output. TKT-1025 closes
in that same scoped commit. Evidence is committed under
staging/differential-witness-cap/fix in plato-brep.

Production03 has zero errors; its warnings match the prior normal build. All
25 strict consumer commands passed with the original law/witness limits. Two
fresh normal regenerations match exactly and emit no degraded code. The original
Git-archive comparison stopped on LF versus CRLF and remains failed; a separate
strict proof binds prior raw output to committed Git blobs and proves every fresh
raw body unchanged after the compiler provenance header. No other normalization
is accepted. All 54 mesher, 33 decoder and one AP214 host tests passed without
skips; fresh host builds have zero warnings/errors. Actual runtime and input
closures remain unchanged.

Host receipt: 796ea54ea33b840e427d372a99af6b1c80fefc25403380525fafe5fe19472458.
Supplemental regeneration receipt: 3a5e3cc442ae14f46bbe3ae097f5faaa35f2e3ebceef5d8f1bae97b225837bea.
Consumer receipt: 529be8ee1734f38bbdc89e49a55847962097779999a09fcb6cf9a70cc3707784.
The previous corpus evidence is carried forward by exact generated/runtime body
equality and fresh host gates; no fresh corpus or full law sweep is claimed.
