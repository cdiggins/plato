---
id: compiler-516
title: CLI parser stack overflow on frozen mesher input
type: bug
status: ready
priority: p2
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [compiler-515]
---

## Problem

The frozen Debug CLI consumer run for compiler-515 terminated before any law
with Windows exit3221225725 (0xC00000FD). The log reports Stack overflow in
Parakeet CstNodeFactory.Create and repeats recursive child construction225 times.
This is an observed parser failure; its offending file and configuration cause
are not established. No stack-size workaround or parser change has been applied.

## Done means

- [ ] Isolate and minimize the failing input while retaining the recorded stack-overflow signature, with a smaller passing control.
- [ ] Correct the responsible parser or CST construction path and retain the confirmed regression in child-process tests.
- [ ] The original frozen consumer input parses in both supported Debug and Release CLI configurations with unchanged input hashes; report later type/law failures separately.

## Evidence and scope

Compiler source81d2437fbdb5cd56a4cb131389773f0e361c34db plus the compiler-515 runner
patch; Parakeetf4f9a8af7fce930c03b75d990cc927070192133a; mesher library
7cfb1630c6e3dbeae1eafec7c6665fb5f7fc8d02. Executed DLL hashes, complete argv,
unchanged input evidence and original logs are retained in plato-brep under
artifacts/continuous-backlog/law-budget-fix/candidate-01/consumer/results.
Source investigation and reproduction proposal are in its
staging/parser-stack-overflow/ISSUE.md and evidence.json.

The failed full run loads751 Plato files. Static nesting ranks are only suspects.
A subsequent isolated Debug CLI probe of face-meshing-lift.laws.plato,
pipeline-faceted.laws.plato, step.library.plato and a13-conditional reduction
parsed successfully and reached expected missing-symbol errors without stdlib.
Those are not successful compilations and do not reproduce the crash. Exact logs
and unchanged source/DLL hashes are under
artifacts/continuous-backlog/parser-isolation in plato-brep.

RunCommand constructs Document with Logger.Null, suppressing the per-file path
that would otherwise identify the crashing input. Its trace cannot establish the
file. The factory recursively constructs children through Select(Create).ToArray;
source delimiter depth alone does not establish CST depth. The normal Release
consumer control is pending separately. This issue does not change compiler-515's
law-budget contract or count a parser failure as a law verdict.
