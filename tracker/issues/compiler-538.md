---
id: compiler-538
title: Fix CPU native primitive signature collisions (BREP TKT-1133)
type: bug
status: in-progress
priority: p1
effort: S
risk: low
area: compiler
sprint: 
created: 2026-11-01
closed:
links: []
---

## Progress

- Integer.ToInteger now resolves to the CPU runtime definition instead of emitting a duplicate.
- PrimitiveSignatureTests generates forward-foundation conversions and compiles function and JSON units with MSVC.
- PLATO_VCVARS selects the same installation as downstream gates; MSVC gets a short child-only PATH.
- Verification: Release build zero errors; native regression passed with VS 2022 (2026-11-01).
- Initial native test failures were SDK discovery failures from the inherited 8980-character PATH; standalone native syntax passed and the isolated rerun passed.
- Optimizer documentation check is already stale at the starting pin across C# and CLI sources; this native-only change touches none of its monitored inputs.
- Remaining: borrow String parameters without changing semantic signature keys, then regenerate and validate the BREP host.

String parameter chunk: CPU declarations now borrow String as const String& in
ordinary functions, bodiless functions and sum factories. Semantic signature keys
remain String, and return values remain owned String values. Release build passed;
PrimitiveSignatureTests and StringBorrowingTests both passed, including generated
native execution with temporary strings and retained return values (2026-11-01).
Full matched BREP generation/compilation/corpus validation remains outstanding.
