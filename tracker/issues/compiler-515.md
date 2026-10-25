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

- [ ] Domain preparation is excluded consistently from law elapsed and abandonment; case generation, premises and law execution stay charged.
- [ ] Witness caps, missing/faulted witness results and the warm whole-request RunLimit remain effective.
- [ ] Bounded real-backend tests cover cold/cached, direct/serial/parallel, actual law exhaustion, compiled abandonment, generated-case time and warm requests.
- [ ] Consumer focused laws pass unchanged budget2 on a pinned immutable compiler/source snapshot, with zero differential mismatch and unchanged inputs.

## Ownership and verification

Backlog supervisor for plato-brep owns only src/Plato.Interpreter/Verification/LawRunner.cs and new tests/Plato.Interpreter.Tests/LawBudgetTests.cs, tests/Plato.Navigation.Tests/WarmLawBudgetTests.cs. Current prototype is staged at C:/Users/cdigg/git/plato-brep/staging/law-budget-fix; not promoted or tested. Other compiler/test-tagging changes are excluded. Compiler-504 confirms no source overlap and released its timing window. An immutable private unit-test snapshot is prepared; execution remains pending the consumer correctness gates. Build only immutable private inputs with recorded-build wrapper; upstream Git turn coordinated before commit.
