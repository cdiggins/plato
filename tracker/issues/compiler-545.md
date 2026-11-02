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
- [ ] Frozen candidate passes aligned law, decode, stage and mesh gates.
- [ ] Matched measurements compare the additional array-read change with its predecessor.

Focused build cpp-array-borrow has zero errors; seven native tests pass (.temp-brep-array-borrow-tests.log). The runtime copy-counter test observes zero copies for a borrowed large read and one copy for an owned value. Full candidate and timing gates remain pending.
