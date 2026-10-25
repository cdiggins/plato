---
id: compiler-483
title: Characterize optimizer sequencing and identity hazards
type: problem
status: in-progress
priority: p1
effort: M
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization-plan.md]
---

Independent review identified H-01 through H-05 by source inspection, without executable reproductions: argument/Boolean hoisting; eager fusion completion/fault/termination changes; same-name combinator recognition and coercion stripping; assigned-local closure semantics; generated-name collisions. These can affect correctness if reachable. This investigation is separate from F1 and is not a claim that all five are confirmed bugs. Existing generic dispatch issue plato-374 and mutable-array issue plato-386 remain separate.

## Done means

- [ ] Build minimized executable reproductions or disprove reachability for H-01 through H-05.
- [ ] State expected behavior from semantics/runtime contracts and distinguish domain faults, termination and resource exhaustion.
- [ ] File confirmed independent defects or close disproved hypotheses with evidence; define conservative prerequisites for broader optimization.
