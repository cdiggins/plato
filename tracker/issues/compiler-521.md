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

- [ ] Review existing TIR architecture, semantic contracts and prior decisions.
- [ ] Record the declarative rule contract, trusted boundary and validation strategy.
- [ ] Specify parallel ownership, dependencies, scoped gates and migration/rollback sequence for all six optimizations.
- [ ] Implement and independently review the first checked transformation slice under the plan, without widening current optimization eligibility.

The current request authorizes planning. The implementation criterion remains pending; planning does not establish a safety guarantee for unimplemented checkers.
