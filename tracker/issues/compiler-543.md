---
id: compiler-543
title: Split native JSON compilation into bounded units
type: debt
status: done
priority: p2
effort: S
risk: low
area: compiler
sprint: 
created: 2026-11-01
closed: 2026-11-01
links: []
---

## Done means

- [x] Native JSON definitions split deterministically without splitting record converter pairs.
- [x] Native regression compiles and links separate units and round-trips records across units.

Verification: cpp-json-units recorded build has zero errors. JsonCompilationTests, StringBorrowingTests and PrimitiveSignatureTests pass (3/3); .temp-brep-json-units-tests.log. BREP integration and performance measurements are consumer follow-up gates, not implied by these focused tests.
