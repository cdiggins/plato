---
id: compiler-521
title: Plan declarative checked optimizer transformations
type: idea
status: in-progress
priority: p1
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-25
closed:
links: [docs/design/checked-optimizer-pipeline.md]
---

User requests an understandable, extensible architecture for independently switchable optimizations, declarative rules where useful, explicit safety assurance and parallel execution. This item owns the shared architecture; existing optimization items keep their benefit and legality obligations.

## Done means

- [x] Review existing TIR architecture, semantic contracts and prior decisions.
- [x] Record the declarative rule contract, trusted boundary and validation strategy.
- [x] Specify parallel ownership, dependencies, scoped gates and migration/rollback sequence for all six optimizations.
- [ ] Implement and independently review the first checked transformation slice under the plan, without widening current optimization eligibility.

The current request authorizes planning. The implementation criterion remains pending; planning does not establish a safety guarantee for unimplemented checkers.

## Planning review

Architecture, safety and delivery subagents reviewed the existing pipeline and the proposed plan. Independent review clarified that exact-output changes emitted C#, so the first slice trusts its renderer and tests actual generated syntax and behavior; it does not claim per-compilation emitted-code equivalence. Verifier scope starts with the relevant exact-output forms and expands with fusion. Implementation is not started.

## Semantic prerequisites

The [semantic audit](../../docs/reports/optimizer-semantics-audit-2026-10-25.md) adds explicit language/intrinsic/backend premise sources, affine effects and executable coercions to the plan. Characterize older passes under compiler-483 before claiming whole-pipeline equivalence; the exact-output pilot remains a bounded adjacent-stage migration. Implementation is still pending.
