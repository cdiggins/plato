---
id: compiler-534
title: Define outcome semantics and optimization-friendly contracts
type: idea
status: in-progress
priority: p1
effort: L
risk: high
area: compiler
sprint:
created: 2026-10-25
closed:
links: [tracker/decisions/2026-10-25-mixed-evaluation-preserves-required-results.md, docs/design/checked-optimizer-pipeline.md]
---

The user approved recording the recommendation that Plato may mix eager, on-demand, streaming and deferred/cached evaluation for pure computation, and may skip faults confined to unnecessary work. Defined successful results and work required by the requested result or explicit validation must be preserved. The [accepted decision](../decisions/2026-10-25-mixed-evaluation-preserves-required-results.md) is the authority for this design direction. This is not a claim of implemented compiler behavior.

## Done means

- [x] Record the approved mixed-evaluation direction and its boundaries in an architecture decision.
- [x] Update the semantic reference and checked-pipeline plan, distinguishing intended policy from current behavior.
- [ ] Resolve fault identity, competing required faults, nontermination and forcing/validation boundaries with small semantic examples.
- [ ] Specify checkable dependency/effect/ownership contracts and select the first bounded rule migration.
- [ ] Implement and independently validate that migration against the accepted contract, preserving successful outputs and rejecting suppression of required failures.

## Constraints and priorities

Pure computation does not imply totality, determinism of arbitrary host delegates, or safe affine-builder duplication. Preserve numerical-profile semantics and conversions; no floating-point reassociation permission is granted. Omission of a nonterminating computation and arbitrary selection among competing required faults remain unresolved.

Prioritize verified pure host boundaries and affine ownership, precise effects, and bounds/output-shape facts consumed by an actual rule. No new lazy type or fully lazy runtime is required by the decision. Eager materialization remains useful for repeated traversal; streaming and deferred caching are implementation choices with different cost tradeoffs.

Compiler-483 continues to characterize old-pass assumptions; compiler-521 owns the checked transformation architecture. Unmigrated rules retain their existing conservative guards. Differential evidence must distinguish permitted unnecessary-fault suppression from an unexplained fault-to-success discrepancy.

## Recording milestone

The accepted recommendation, documentation links and V2 planning contract are recorded. Runtime behavior and optimizer defaults are unchanged. Remaining checkboxes are semantic resolution and implementation work, not missing documentation.
