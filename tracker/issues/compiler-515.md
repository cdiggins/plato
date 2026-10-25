---
id: compiler-515
title: Separate law execution budgets from witness preparation
type: bug
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [C:/Users/cdigg/git/plato-brep/tickets/TKT-1006-law-witness-budget-accounting.md]
---

## Problem

Consumer plato-brep TKT-1006: cold DomainOf witness construction is charged to both the law stopwatch and compiled abandonment timer, while parallel RunAll prewarms it. Identical laws therefore receive different budget verdicts depending on cache/order. Witness construction already has its own cap and timing record.

## Done means

- [x] Domain preparation is excluded consistently from law elapsed and abandonment; case generation, premises and law execution stay charged.
- [x] Witness caps, missing/faulted witness results and the warm whole-request RunLimit remain effective.
- [x] Bounded real-backend tests cover cold/cached, direct/serial/parallel, actual law exhaustion, compiled abandonment, generated-case time and warm requests.
- [ ] Consumer focused laws pass unchanged budget2 on a pinned immutable compiler/source snapshot, with zero differential mismatch and unchanged inputs.

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