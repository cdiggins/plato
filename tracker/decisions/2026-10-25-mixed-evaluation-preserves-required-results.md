---
date: 2026-10-25
title: Permit mixed evaluation and omission of unnecessary pure work
status: accepted
superseded-by:
links: [../issues/compiler-534.md, ../../docs/SEMANTICS.md, ../../docs/design/checked-optimizer-pipeline.md]
---

## Context

Geometry workloads benefit from different evaluation and storage strategies. A complete mesh may
be reused many times, an intersection query may need only one witness, and adaptive sampling may
request a small part of a large domain. Requiring every optimization to preserve eager evaluation
of unused computations prevents useful transformations even when successful geometry is unchanged.

The user accepts that some faults in unnecessary geometry computations may be skipped and approved
recording the recommendation to allow mixed evaluation. This decision accepts that language-design
direction. It does not claim the compiler, interpreter or runtime already implements it consistently.
Implementation and the unresolved details remain tracked in compiler-534.

## Decision

Plato may use eager computation, on-demand computation, streaming, or deferred computation with
cached results for pure computations. These are implementation choices rather than reasons for
successful geometry to change.

The intended optimization contract is:

> The compiler may avoid unnecessary pure computation, including faults arising only from that
> computation, while preserving every defined successful result and all work required by the
> requested result or explicit validation.

This deliberately goes beyond moving a fault earlier or later: an execution that formerly faulted
may instead return a valid result when the omitted computation was unnecessary. It does not permit
inventing a successful result when required computation fails, dropping requested output, or changing
a previously successful answer. Numerical results remain subject to the selected Number profile;
this decision grants no reassociation or approximate-arithmetic permission.

"Unnecessary" must be established from the requested operation's semantic contract and dependencies,
not from a cost estimate, the absence of observed test failures, or a generic purity label. Fault
suppression is achieved by proving work unnecessary and not evaluating it, not by catching arbitrary
exceptions and returning a default value.

| Requested operation | Consequence |
| --- | --- |
| Determine whether any triangle intersects a ray | Once a valid intersection is established, unrelated triangle tests need not run; faults confined to those tests may be skipped. |
| Compute every requested triangle area | Every requested area remains part of the result. A failed required computation cannot silently remove a triangle or substitute a value. |
| Validate the entire mesh | The requested validation coverage must be preserved. Exhaustive validation cannot become an early-success query. Whether it returns all diagnostics or one failure belongs to that validation API's contract. |

Materialization and memoization are separate choices. Eager materialization suits repeated traversal;
streaming suits a single traversal; deferred cached values suit expensive irregular reuse. The
compiler may choose among them only when the rule's supported semantic contract permits it. This
decision does not require a new lazy type, universal memoization, or a fully lazy language runtime.

## Boundaries and unresolved decisions

- Host side effects are outside this freedom. Arbitrary delegates and IReadOnlyList implementations
  are not proved pure merely by their types. A supported host boundary must state and enforce or
  explicitly trust its obligations before invoking these rules.
- Affine builder mutation/consumption still needs ownership and effect reasoning. Pure output does
  not license duplicating a builder or accessing it after Freeze.
- Skipping nonterminating work is not decided here. Rules requiring that freedom remain blocked
  until the termination policy is settled; accepting skipped faults does not implicitly settle it.
- Fault identity and the choice between competing required faults remain unresolved. This decision
  does not authorize arbitrary substitution of one required failure for another.
- Exact forcing/materialization boundaries and a mechanism for explicit exhaustive validation
  remain implementation-design work. No new source keyword or API is introduced by this decision.

## Rationale and alternatives

Preserving successful results and required validation matches the purpose of geometry computation
without treating incidental unused work as an essential observable. It permits useful fusion and
short-circuit queries while retaining meaningful correctness obligations.

- Requiring identical eager evaluation everywhere is rejected as the universal target contract:
  it unnecessarily restricts pure geometry queries. It remains the compatibility baseline until
  a specific transformation is migrated and validated against this decision.
- Making all calls and arrays lazy is not required: it introduces storage, debugging and ownership
  costs even where eager computation is preferable.
- Ignoring all faults is rejected: required geometry and explicit validation must remain reliable.
- Using real-number algebra for machine Number values is outside scope and not authorized.

## Adoption and validation

Adopt through the checked-transformation plan, one rule at a time. Each rule records the semantic
contract version, requested observations, dependency/ownership facts, permitted omission and numeric
profile. Existing strict checks remain in place until that rule's migration is implemented and reviewed.
Do not relabel every compiler-483 hypothesis as harmless because this direction was accepted.

Differential validation must distinguish unchanged successful results, permitted suppression of an
unnecessary fault, and forbidden changes to required results/failures. A candidate must independently
justify why suppressed work is unnecessary; a comparator cannot simply accept every baseline-fault /
optimized-success pair. Preserve an unoptimized reference and targeted adversarial cases. Any
nontermination-sensitive or unresolved competing-fault case remains outside admission.

This decision is documentation only. It changes neither optimizer defaults nor generated code.
