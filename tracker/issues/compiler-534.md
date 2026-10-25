---
id: compiler-534
title: Define outcome semantics and optimization-friendly contracts
type: idea
status: idea
priority: p1
effort: L
risk: high
area: compiler
sprint:
created: 2026-10-25
closed:
links: [docs/design/checked-optimizer-pipeline.md]
---

User proposes that fault timing should not constrain optimization provided the result is the same answer or the same fault. Capture as a language-design proposal, not an implemented semantic change.

## Design direction

Define observational equivalence for pure Plato computation by returned value or a structured fault, separately addressing divergence. Allocation timing, callback invocation counts, intermediate traversal order and stack traces need not be observations for verified pure deterministic computations. Fault identity and competing-fault selection must be explicit: allowing either of several faults is weaker than requiring the same fault and must not be silently assumed. Removing a fault, returning a different fault, changing successful values or turning divergence into termination is not licensed merely by ignoring fault timing.

Prioritize statically enforced affine ownership and pure host boundaries; checked effects distinguishing purity, determinism, partiality and termination; refinement/shape information for bounds and exact output cardinality; total Option/Result-style APIs for expected failure; and explicit ordered versus associative reductions with profile-correct numeric laws. Inference/verification should justify contracts rather than trusting arbitrary optimization annotations. A total callable can still have non-total argument expressions.

Immutable completed arrays can be streamed when outcome equivalence is established. An explicit lazy sequence abstraction would allow intentionally demand-driven behavior without silently changing eager Array construction. Arbitrary host IReadOnlyList/delegate effects must stay outside the pure guarantee unless wrapped/validated against a documented boundary contract.

## Decisions and acceptance before implementation

- Define fault identity, competing faults and termination requirements with small counterexamples. Decide whether ordinary calls remain strict independently of unobservable evaluation order.
- Preserve short-circuit selection of branches and numeric-profile results. Do not infer floating-point associativity from finite values or a real-number abstraction.
- Specify compiler-verifiable effect/ownership/shape contracts and their host trust boundary, prioritized by the existing optimization candidates.
- Review which compiler-483 obligations become unnecessary versus still necessary under the proposed semantics; retain existing behavior until the language decision and migration are explicit.

Related: compiler-483 semantic audit and compiler-521 checked transformation architecture. No fault-policy, compiler, interpreter or runtime change is made by this capture.
