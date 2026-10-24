---
id: compiler-481
title: Plan and critically review BREP compiler optimizations
type: problem
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization-plan.md]
---

Investigate the compiler opportunities behind allocation-heavy BREP meshing: private scratch storage, scoped combinator lowering, constant specialization, broader fusion/filter lowering, and concrete storage/callback specialization. The user requests independent parallel proposals, a fresh critical review, revision incorporating that review, and an implementation-agent handoff. This item delivers the reviewed design; implementation chunks receive their own acceptance criteria and issues.

## Done means

- [ ] Preserve all five source-grounded proposals, including mechanism, implementation chunks, benefits, risks, unknowns, and verification.
- [ ] Obtain an independent critical review and record the disposition of every actionable finding.
- [ ] Commit the revised plan with explicit dependencies and a bounded first implementation contract.
- [ ] Start an implementation agent against a ready or in-progress implementation issue and the reviewed contract.
