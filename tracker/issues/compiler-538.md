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

Full frozen BREP candidate 85272cae compilation exposed two more runtime contract gaps: List<T>{capacity} was emitted but the native runtime only supplied aggregate pointer/count/capacity initialization; Bytes lacked JSON overloads. Capacity constructors now match managed semantics (nonpositive empty, positive preallocation, grow beyond hint) while preserving existing three-field initialization. The native runtime tests pass 2/2, including same-buffer appends within capacity and growth after it. Bytes JSON parity remains the next focused repair. Full candidate compilation did not pass; 14 objects were produced and no executable was linked.

Bytes JSON now uses the existing managed Latin-1 string representation on both runtimes. Native overloads encode/decode JSON UTF-8 without losing bytes; managed Bytes now has a converter so deserialization preserves its payload instead of falling through to a fieldless struct. All 256 bytes, nested arrays, empty views and managed invalid inputs are covered. Managed BytesTests: 38 passed; native runtime tests: 2 passed. Recorded managed build zero errors. New frozen BREP regeneration/compile remains required.

Fresh frozen f6823eb7 consumer validation completed: unpatched native build/link passed all 16 sources; shared laws passed 5/5 on each backend; isolated managed decode tests passed 90/90. SAM_AP203 matched all mesh fields (98 faces, 454 triangles) with one and two native workers; c211 matched 539 faces and 11610 triangles. Strict raw front-end gate remains red (17/21), tracked in compiler-542; no pin promotion or speedup claim. Integrated native regression selection passed 9/9, recorded in .temp-brep-focused-test.log.

Closeout 2026-11-01: tools/record-gates.py --full at 66fa5cc1 completed. CLI build PASS (0 errors), strict lint PASS (0 errors, 29 warnings), PlatoTests PASS (616/616). Fresh forward generation emitted 1042 files and one degraded Tween.Change body; conformance compilation FAILS on missing MapRangeEager, MapEager and IParametricSurface<OffsetSurface>.Base members. The full gate is red; no assertion that these broader failures are newly introduced or pre-existing without a matched baseline. Evidence: docs/status-report-snapshot.json, docs/gate-log.md and .temp/gate-logs/20261101-121206-885225/. No generated patches or ratchet changes.
