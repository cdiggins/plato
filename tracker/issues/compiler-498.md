---
id: compiler-498
title: Measure aggressive inlining on plato-brep meshing
type: problem
status: in-progress
priority: p2
effort: M
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

Compare increasingly aggressive Plato source inlining on the plato-brep STEP
mesher, keeping the library, runtime, loop lowering, and fusion fixed. Work in
isolated committed snapshots because both checkouts have concurrent sessions.

## Done means

- [x] A reproducible isolated runner records compiler/library revisions, policies, build outcomes, and code size.
- [x] Measure buildable variants on STEP inputs with repeated warm runs, allocations, and mesh/diagnostic equivalence checks.
- [x] Publish the results and limits, with compile failures retained as evidence and follow-up work tracked.

## Progress

Pinned Plato 763034a67612fb1ada95891434cba8fc2ecf5707 and plato-brep
99c8fdee6be8d3fc92f45401321d442e15375316. Experiment files and logs live under
`.temp/inline-brep`; scripts under `tools/experiments`. The root PROGRESS.md is
occupied by another mission and is untouched.

The five policies have generated and passed the stub gate. All four full inline
policies hit the same compilation failures, captured in compiler-499. Matched
partial builds retain the same 51 baseline files and compile successfully. The
480/16 and 2000/32 outputs are byte-identical. All 27 controlled measurements completed with matching meshes and diagnostics;
the frozen host check permits only the generated B-rep DLL to differ.


## Outcome

Report and raw evidence: docs/reports/brep-inlining-2026-10-24.md and its data JSON.
Current versus aggressive partial inlining shows no demonstrated warm meshing win
against baseline. Larger budgets reach a code-expansion plateau; allocation growth
on two models is tracked by compiler-500. Full-inline emission failures remain in
compiler-499. The initial sleep-interrupted timings are retained but excluded from
conclusions. No production optimizer defaults changed.

Final checks: CLI build passed (132s), shipping lint passed (18s), all 361 PlatoTests
passed (537s). Studio lints, SDK build, and all 15 geometry tests passed; its existing
external frozen-artifact failure remains repo-473. Stale documentation evidence for
an independently changed concrete writer is captured by compiler-501. Runner Python
and PowerShell syntax checks passed. Root status snapshot/HTML and other sessions'
scratch files remain outside this commit.
