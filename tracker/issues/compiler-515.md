---
id: compiler-515
title: Separate law execution budgets from witness preparation
type: bug
status: done
priority: p1
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed: 2026-10-25
links: [C:/Users/cdigg/git/plato-brep/tickets/TKT-1006-law-witness-budget-accounting.md]
---

## Problem

Consumer plato-brep TKT-1006: cold DomainOf witness construction is charged to both the law stopwatch and compiled abandonment timer, while parallel RunAll prewarms it. Identical laws therefore receive different budget verdicts depending on cache/order. Witness construction already has its own cap and timing record.

## Done means

- [x] Domain preparation is excluded consistently from law elapsed and abandonment; case generation, premises and law execution stay charged.
- [x] Witness caps, missing/faulted witness results and the warm whole-request RunLimit remain effective.
- [x] Bounded real-backend tests cover cold/cached, direct/serial/parallel, actual law exhaustion, compiled abandonment, generated-case time and warm requests.
- [x] Consumer focused laws pass unchanged budget2 on a pinned immutable compiler/source snapshot, with zero differential mismatch and unchanged inputs.

## Ownership and verification

Backlog supervisor for plato-brep owns only src/Plato.Interpreter/Verification/LawRunner.cs and new tests/Plato.Interpreter.Tests/LawBudgetTests.cs, tests/Plato.Navigation.Tests/WarmLawBudgetTests.cs. Current prototype is staged at C:/Users/cdigg/git/plato-brep/staging/law-budget-fix; not promoted or tested. Other compiler/test-tagging changes are excluded. Compiler-504 confirms no source overlap and released its timing window. An immutable private unit-test snapshot is prepared; execution remains pending the consumer correctness gates. Build only immutable private inputs with recorded-build wrapper; upstream Git turn coordinated before commit.

## Verified implementation milestone

The three owned source files are promoted from the immutable plato-brep candidate
on upstream81d2437fbdb5cd56a4cb131389773f0e361c34db. Runner tests40/40 and warm/request
limit tests7/7 passed, including19 new runner and6 new warm cases, with no skips.
Source and host compiler fingerprints stayed unchanged. Candidate manifest SHA256:
4b27ddf20cf70b8393c4062773013f30550509f0bdf6333ffea700734253c4c5.
Recorded build evidence and the exact verification drivers are committed in
plato-brep7d4bb199; private logs are under
artifacts/continuous-backlog/law-budget-fix/candidate-01 there.

Consumer verification remains outstanding. Its first Debug CLI attempt overflowed
the Parakeet CST construction stack before any law ran (exit3221225725); inputs
were unchanged and the failed attempt is retained. The next gate uses the normal
Release compiler closure from this promoted commit, through the existing consumer
regen-compiler policy, with pinned private project references. No law budget or
witness cap is relaxed. This issue remains in-progress until the final checkbox
is verified; the consumer dependency pin has not moved.

## Final Release consumer verification

The normal Release cache built from fix commit
36850a37a0f4ad44fea51adff00a52621985e085 passed all fifteen focused baseline
consumer runs and all five TKT-424 runs at the unchanged two-second law budget.
Cold, combined, differential, compiled and compiled parallel-two cases retained
exact discovery and case counts, zero differential mismatches, and unchanged
source/compiler/runtime fingerprints. The focused consumers cover grid corners,
projection, spline records, region winding and proofs, and segment-floor production.
Evidence and reproducible driver are committed in plato-brep293c9389;
its final-consumer-summary.json records the individual log and verdict hashes.

The wider 29-run attempt is explicitly incomplete. An unpromoted TKT-457 catalog
constructed twelve of thirteen witnesses: the torus reached the unchanged
120-second witness cap. Its law took 0.01 seconds with zero mismatches; the strict
coverage driver correctly rejected the missing witness. Seven later catalog runs
were not attempted. This is retained construction-cap evidence, not a passing
catalog or a waiver. TKT-457 remains open, and its source is excluded from the
consumer pin integration. The preceding Debug parser stack overflow remains
compiler-516. Consumer default-pin regeneration and host/corpus integration are
tracked separately in TKT-1006; this compiler issue's focused acceptance is met.
