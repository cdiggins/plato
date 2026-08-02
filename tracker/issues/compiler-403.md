---
id: compiler-403
title: ExistentialConceptCheckerTests point at pre-rename fixture filenames
type: bug
status: done
priority: p3
effort: S
risk: low
area: compiler
sprint: 
created: 2026-08-02
closed: 2026-08-02
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

## Resolution (2026-08-02)

Fixed the opposite way from "Simplest fix": the terminology sweep (e85f761) is the intended
direction, so the two fixture files were renamed to the interface names the `[TestCase]`
attributes already use (`stored-interface-ok.plato`, `negatives/viewless-interface-field.plato`).
Landed in 051f538 (the renames were staged in a shared working tree and picked up by a
concurrent session's commit). Full `dotnet test tests/PlatoTests`: 247/247 green.

Note: the causality in "What happens" was backwards — the sweep renamed the strings in the
test file, not the fixtures; the files on disk still had the pre-sweep concept names.

## Found by

Noticed while running the PlatoTests suite for `compiler-399`; unrelated to that change.
