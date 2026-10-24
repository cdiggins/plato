---
id: compiler-485
title: Lower combinators within preserved statement scopes
type: feature
status: idea
priority: p2
effort: M
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/scoped-lowering.md]
---

Reviewed enabling sequence: restricted existing statement scopes, tail-return branches, then callback-local prerequisites. CR-03 prohibits reusing the general hoister on newly admitted complex expressions without ordered sequencing. Arrays remain allocated until the separate storage work.

## Done means

- [ ] Define ordered expression prerequisites or explicit direct-call refusal rules for newly admitted scopes.
- [ ] Verify branch guards, sibling evaluation, zero-trip loops, varying inner counts, names and captures in compiled tests.
- [ ] Demonstrate the selected BREP kernel becomes eligible without claiming storage elimination.
