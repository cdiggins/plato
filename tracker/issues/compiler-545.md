---
id: compiler-545
title: Avoid large array element copies at borrowed calls
type: feature
status: in-progress
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-11-01
closed:
links: []
---

## Done means

- [x] Internal readonly array access avoids large argument temporaries while public At remains value-returning.
- [x] Native tests prove zero-copy borrowing, owned copies/returns, scratch-reset survival, mutation isolation and both writer settings.
- [x] Frozen experiment candidate passes aligned law, decode, stage and mesh gates.
- [ ] Final opt-in defaults pass the same frozen aligned gates.
- [x] Matched measurements compare the additional array-read change with its predecessor.

Focused build cpp-array-borrow has zero errors; seven native tests pass (.temp-brep-array-borrow-tests.log). The runtime copy-counter test observes zero copies for a borrowed large read and one copy for an owned value. Full candidate and timing gates remain pending.

Candidate 1e1e41e8 passes stage 24/24, laws 5/5 per backend, decode 90/90, SAM with one and two workers, and c211 (539 faces, 11610 triangles). C# 1771 source files are unchanged. Default compiler gates pass build, lint and 616 tests.

Five balanced CPU0 rounds against 1fbae01c preserve all 44 measured meshes, but c211 median is 0.5% slower and SAM improvement is below control variation. Commit 2361ccb9 makes parameter and array-read borrowing independent opt-in flags; ten focused native tests cover all combinations. Final frozen default gates are running. Evidence: plato-brep artifacts/cpp-validation/candidate-1e1e41e8 and ab-array-1e1e41e8-*; docs/plans/cpp-borrowing-measurement-2026-11-01.md.
