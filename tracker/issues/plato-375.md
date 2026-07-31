---
id: plato-375
title: plato-321 matrix obligation bodies break forward codegen: CS0736 static-vs-instance + CS0111 against handwritten intrinsics
type: bug
status: ready
priority: p1
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-31
closed:
links: []
---

## Issue

`c25e0bc` (plato-321, "matrix + primitive concept obligations, 229 -> 171 lint") added
`stdlib/foundation/matrices-dense.library.plato` with 240 lines of obligation fills. The lint
burn-down is real — the ratchet went 229 -> 159 — but a clean regeneration of the forward stdlib
no longer **compiles**, in two shapes:

```
_Matrix2x2.g.cs(13,38): CS0736: 'Matrix2x2' does not implement instance interface member
  'MatrixLike.RowCount()'. 'Matrix2x2.RowCount()' cannot implement the interface member
  because it is static.                      (also Matrix3x3, Matrix4x3; RowCount + ColumnCount)
Matrix3x2.cs(170,53): CS0111: Type 'Matrix3x2' already defines a member called 'RowCount'
  with the same parameter types              (also ColumnCount, ElementAt; also Matrix4x4)
```

12 distinct errors. Measured at `32891dd` after `python tools/record-gates.py --full`.

## Cause

**CS0736 — the `_` receiver.** The fills are spelled `RowCount(_: Matrix2x2): Integer => 2;`.
`CSharpFunctionInfo.IsStatic` is purely syntactic (`ParameterNames[0] == "_"`), so the writer
emits `public static int RowCount()`, which cannot implement the INSTANCE obligation
`MatrixLike.RowCount(x: Self)`. This is the identical trap plato-308 hit on
`Zero(_: Color)` in July and fixed by naming the receiver — the fix there was
`Zero(x: Color)`. LINT012 exists for exactly this disagreement and did not stop the commit,
because nothing gates on lint warnings at commit time.

**CS0111 — collision with the handwritten runtime.** `Matrix3x2` / `Matrix4x4` already carry
`RowCount` / `ColumnCount` / `ElementAt` in `src/Plato.Intrinsics` (kept in `d006258` precisely
because the stdlib had no bodies for them). Now both sides declare them.

## Fix

1. Name the receiver in every fill that discharges an instance obligation:
   `RowCount(self: Matrix2x2): Integer => 2;`. Check the whole new file, not just the matrices.
2. Delete `RowCount` / `ColumnCount` / `ElementAt` from `Plato.Intrinsics/Matrix3x2.cs` and
   `Matrix4x4.cs` — the declaration owns them now, same rule the rest of `d006258` followed.
3. Regenerate and build before claiming green (`python tools/record-gates.py --full`).

## Done means

- [ ] `python tools/record-gates.py --full` shows the conformance gate PASS.
- [ ] Lint ratchet no higher than 159, and the ceiling in `ForwardStdLibLintTests` matches it.
