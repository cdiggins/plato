---
id: compiler-475
title: Review drift in implicit and emitted conversion inventories
type: debt
status: idea
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

`ImplicitCastsMatchBaseline` and `EmittedConversionsMatchBaseline` fail against their committed inventories. Replaying both tests with the original `876fe36b` Plato assemblies and with the repo-468 organization changes produces identical failure messages. The organization work preserved the inventory test files beside their source-relative baselines.

The tests intentionally require semantic review before accepting new coercions. Inspect the actual conversion additions/removals against `stdlib/CONVENTIONS.md` and the single-field wrapping contract, then correct generation or refresh the reviewed inventories. Do not refresh them merely to turn the organization gate green. Both failures obstruct use of the full compiler suite as a clean regression signal.

## Done means

- [ ] Explain and approve or fix each conversion difference against the language contracts.
- [ ] Both inventory tests pass with reviewed baselines.
