---
id: repo-493
title: Consolidate writer selection and test fixture helpers
type: debt
status: in-progress
priority: p2
effort: M
risk: med
area: repo
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

The Greenhouse audit (repo-492) confirmed duplicated implementation-selection policy in all five writers and duplicated emission/reflection setup in the type-bound and function-bound test fixtures. Changes currently require several synchronized edits. This is a medium-sized cleanup in independently verifiable steps; it must preserve backend output and each distinct regression assertion.

## Done means

- [ ] Share ScoreFunction and ChooseBestFunction across the five writers, preserving intrinsic precedence, concrete precedence, interface distance, tie counts and first-candidate ordering; retain thin compatibility wrappers if public consumers require them.
- [ ] Share source-to-CSharpWriter setup, wrapper construction/invocation and the small runtime prelude in the constraint emission fixtures, reusing CSharpTestCompilation; preserve separate type-bound and function-bound assertions and negative controls.
- [ ] Parameterize the three array-receiver emission cases with named cases and preserve all expected signatures and stub assertions; measure before claiming a test-speed benefit.
- [ ] Verify focused tests and representative output for affected backends, then record the required integration gates.

## Follow-on candidates requiring design review

Shared eta-forwarding recognition in the C#, Rust and TypeScript body writers; shared parsing/repository-location test support across projects. Keep these separate from the initial cleanup if dependencies or semantics widen the change. Do not merge memo-key equality with language-value equality, or Rust/TypeScript emission pipelines merely because their bodies match.

Evidence and validation boundaries: `docs/reports/duplication-audit-2026-10-24.md`.
