---
id: repo-401
title: record-gates.py --full passes the retired --scalar=float flag
type: bug
status: ready
priority: p3
effort: S
risk: low
area: repo
sprint: 
created: 2026-08-02
closed:
links: [tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md]
---

## Symptom

`python tools/record-gates.py --full` cannot pass its codegen gate. The `RECIPE` list in
`tools/record-gates.py` still carries `--scalar=float`, and `src/Plato.CLI/Program.cs` now rejects
any `--scalar=` argument outright:

```
'--scalar=float' is no longer supported: scalar erasure was retired 2026-08-01 and wrapper
scalars are the only representation. Drop the flag.
```

The CLI exits non-zero before generating anything, so both the codegen row and the conformance row
of the recorded gate table are red for a reason that has nothing to do with the library.

## Cause

Scalar erasure was retired (`tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md`).
The flag was removed from the compiler in that change; the recipe constant in the recorder was not
updated with it.

## Fix

Delete `"--scalar=float"` from `RECIPE`. Check the same string in any other recipe that is still
executed (docs that quote historical recipes are fine as history). Then run
`python tools/record-gates.py --full` once and confirm the codegen row generates files again.

## Done means

- [ ] `RECIPE` in `tools/record-gates.py` contains no retired flag
- [ ] `record-gates.py --full` reaches the law runner instead of failing at argument parsing
