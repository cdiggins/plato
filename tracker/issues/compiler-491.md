---
id: compiler-491
title: Differential law runs report success when both engines fail
type: bug
status: in-progress
priority: p1
effort: S
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [plato-brep:TKT-1015]
---

## Problem

Without an explicit baseline, differential runs return zero when both engines fail or error, including budget errors. Backend agreement does not establish that a law passed.

## Done means

- [ ] All backends return failure for fail, error, and vacuous verdicts; differential mismatches also fail.
- [ ] Pure policy tests cover matched failure/error/budget outcomes and mismatch-only witness cases; CLI regressions cover matched pass/fail/error and preserve explicit baseline behavior.
- [ ] Document the exit contract and record verification without overwriting concurrent status reports.

## Scope

Preserve skipped and host-obligation semantics and the explicit baseline comparison mode. No compiler, runtime, or stdlib algorithm changes. Related downstream ticket: plato-brep TKT-1015.
