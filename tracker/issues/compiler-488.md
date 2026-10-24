---
id: compiler-488
title: Prove local-binding fusion and empty filter lowering
type: problem
status: idea
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization/fusion-filter.md]
---

Beyond the first singleton branch optimization: local-binding fusion needs totality/termination and eager scheduling proof, while Take(0) recognition needs standard-operation provenance and receiver-evaluation preservation. CR-04/05 are prerequisites. Named Filter and sibling BREP adoption belong to TKT-507 and are not completed by compiler F1.

## Done means

- [ ] Define admissible operations/input domains and provenance before broader fusion or empty-call recognition.
- [ ] Prove producer/seed ordering, short-circuit/fault behavior and Take receiver evaluation with bounded executable fixtures.
- [ ] Demonstrate measured allocation benefit at real eligible sites while retaining shared materialized values.
