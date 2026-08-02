---
id: compiler-403
title: ExistentialConceptCheckerTests point at pre-rename fixture filenames
type: bug
status: ready
priority: p3
effort: S
risk: low
area: compiler
sprint: 
created: 2026-08-02
closed:
links: []
---

## What happens

Two `tests/PlatoTests` cases fail with `FileNotFoundException` rather than an assertion:

- `Positive_CompilesWithoutDiagnostics("stored-interface-ok.plato")`
- `Negative_RaisesExpectedDiagnostic("negatives/viewless-interface-field.plato")`

The fixtures were renamed `interface` -> `concept` (`tests/plato-test-existential/stored-concept-ok.plato`,
`tests/plato-test-existential/negatives/viewless-concept-field.plato`) and the `[TestCase]`
attributes at `tests/PlatoTests/ExistentialConceptCheckerTests.cs:51` and `:60` were not updated.

## Why it matters

Small, but it is two red tests in the default `dotnet test tests/PlatoTests` run, which makes the
suite's baseline "2 failures, ignore those" — the state in which a real regression hides. It also
means the existential-concept checker (stored interface-typed field diagnostics) currently has no
executing coverage at all.

## Simplest fix

Rename the two strings in the `[TestCase]` attributes to match the files on disk. No production
code involved.

## Found by

Noticed while running the PlatoTests suite for `compiler-399`; unrelated to that change.
