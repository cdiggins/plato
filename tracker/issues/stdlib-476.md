---
id: stdlib-476
title: Future collision library violates the intrinsic declaration contract
type: bug
status: idea
priority: p2
effort: S
risk: med
area: stdlib
sprint: 
created: 2026-10-23
closed:
links: []
---

`OnlyIntrinsicsFileDeclaresHostIntrinsics` rejects `stdlib/future/collision.library.plato` because it contains bodiless signatures outside `intrinsics.library.plato`. The same test and diagnostic occur with original `876fe36b` and reorganized compiler assemblies. This is an existing contract violation, not a source-move regression.

Decide whether these aspirational operations need reference bodies or should remain unimplemented vocabulary elsewhere under the existing partition rules. The intrinsic contract should not be weakened or extended merely to accept placeholders. Until resolved, the full compiler suite remains red even though shipping-tier lint passes.

## Done means

- [ ] Express the collision placeholders in a form consistent with the library and intrinsic contracts.
- [ ] The intrinsic-location test and forward-library checking gates pass without added exclusions.
