---
id: stdlib-476
title: Intrinsic-location guard mistakes conditional bodies for declarations
type: bug
status: done
priority: p2
effort: S
risk: med
area: stdlib
sprint: 
created: 2026-10-23
closed: 2026-10-24
links: []
---

`OnlyIntrinsicsFileDeclaresHostIntrinsics` rejects `stdlib/future/collision.library.plato` because it contains bodiless signatures outside `intrinsics.library.plato`. The same test and diagnostic occur with original `876fe36b` and reorganized compiler assemblies. This is an existing contract violation, not a source-move regression.

Decide whether these aspirational operations need reference bodies or should remain unimplemented vocabulary elsewhere under the existing partition rules. The intrinsic contract should not be weakened or extended merely to accept placeholders. Until resolved, the full compiler suite remains red even though shipping-tier lint passes.

## Done means

- [x] Express the collision placeholders in a form consistent with the library and intrinsic contracts.
- [x] The intrinsic-location test and forward-library checking gates pass without added exclusions.

## Corrected diagnosis and fix

The collision library contains valid implementation bodies, not placeholders. The textual guard mistook `SameContact(acc, old, tolerance) ? WithImpulsesOf(acc, old) : acc));` for a declaration because its greedy parameter match consumed the conditional and used its colon as a result-type separator. The earlier issue description recorded the failing assertion, not a proven source violation.

The signature matcher now excludes body syntax from its parameter/result portions. Positive declaration cases and negative expression/conditional/lambda cases pass, as do the unchanged intrinsic count and location gates. No library files, declaration permissions, ratchets or exclusions changed.

Integration verification: `python tools/record-gates.py` passed the complete compiler suite (330 tests, including forward checking and lint ratchets) and shipping strict lint with zero errors. Command output is retained under `.temp/gate-logs/20261024-002924-374993`; the durable result is in `docs/gate-log.md` and the repo-479 evidence report.
