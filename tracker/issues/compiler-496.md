---
id: compiler-496
title: FunctionInstance never assigns Interface so selection ignores interface distance
type: bug
status: in-progress
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

FunctionInstance.cs declares a get-only Interface property but never assigns it. FunctionSelection.ScoreFunction (formerly copied into each writer's PlatoAnalyzer) treats Interface == null as concrete, returning -100 before examining OwnerType.DepthTo. A parsed Item implementing IChild inheriting IBase therefore gives both interface candidates -100, instead of using distance. Intrinsic precedence still works. This can make implementation selection depend on enumeration order rather than interface specificity.

Found while preserving behavior for repo-493; deliberately not fixed in that refactor. Constructor and selector source inspection plus the real-source FunctionSelectionTests fixture reproduce it. Correcting the constructor is small, but it changes selection across all writers and needs output review.

## Done means

- [ ] Establish which FunctionInstance kinds should carry Interface and assign it from the correct declaration/implementation context.
- [ ] Cover nearer and inherited interface candidates through normal construction, including ties and intrinsic/concrete precedence.
- [ ] Review changed output and run backend tests before accepting the semantic change; update the repo-493 preservation test accordingly.
