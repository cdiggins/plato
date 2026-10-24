---
id: compiler-481
title: Plan and critically review BREP compiler optimizations
type: problem
status: done
priority: p1
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: [docs/design/brep-compiler-optimization-plan.md]
---

Investigate the compiler opportunities behind allocation-heavy BREP meshing: private scratch storage, scoped combinator lowering, constant specialization, broader fusion/filter lowering, and concrete storage/callback specialization. The user requests independent parallel proposals, a fresh critical review, revision incorporating that review, and an implementation-agent handoff. This item delivers the reviewed design; implementation chunks receive their own acceptance criteria and issues.

## Done means

- [x] Preserve all five source-grounded proposals, including mechanism, implementation chunks, benefits, risks, unknowns, and verification.
- [x] Obtain an independent critical review and record the disposition of every actionable finding.
- [x] Commit the revised plan with explicit dependencies and a bounded first implementation contract.
- [x] Start an implementation agent against a ready or in-progress implementation issue and the reviewed contract.

The independent planning tracks and critical review are saved under docs/design/brep-compiler-optimization/ and docs/reports/brep-compiler-optimization-review-2026-10-24.md. Review findings CR-01 through CR-12 have explicit dispositions in the combined plan. Compiler-482 releases only F1; compiler-483 records unexecuted correctness hypotheses and compiler-484 through compiler-488 retain deferred opportunities. The implementation agent implement_filter was dispatched against F1 revision 1 after design commit 3cbe460c. The combined design remains live through its implementation issues; completing this planning item does not mark the optimization roadmap executed.
