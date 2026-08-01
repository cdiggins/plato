---
id: compiler-387
title: Property-free C# emission becomes unconditional
type: debt
status: done
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: []
---

## Problem

The C# writer can emit a no-arg member either as a C# property or as a method, selected by
`--no-properties` / `--methods` (one field, `CSharpWriter.NoProperties`). Only the method form
ships. The property form is a second output shape nobody wants, nobody tests deliberately, and
every writer site that spells a member has to branch on it.

Worse, the two shapes are not independent in the code. `CSharpWriterExtensions.ToCSharp` rejected
`--no-properties` without `--scalar=float`, so `NoProperties` and `ScalarErase` were equivalent in
practice and a dozen sites read `NoProperties` when what they actually meant was "the scalar
wrapper types are erased to native primitives". Those sites emit the wrong C# the moment the two
come apart — which is exactly what the forward shipping direction requires: the 2026-08-01 decision
(`decc091`) keeps wrapper scalars and no erasure, so
`generated/Plato.Generated.Foundation.Unoptimized` could not use `--no-properties` at all and was
still emitting properties.

## Done means

- [x] Every `NoProperties` test classified as property-vs-method spelling or as scalar erasure, and
      the erasure ones rewritten to test `ScalarErase`.
- [x] The `--no-properties requires --scalar=float` guard is gone and method-form emission over
      wrapper scalars generates and compiles.
- [x] `--no-properties` / `--methods` CLI flags, the `noProperties` / `methodsOnly` parameters on
      `ToCSharp`, and `CSharpWriter.NoProperties` deleted; every remaining site collapsed to its
      method-form branch.
- [x] `--static-abstract`'s dependency guard on `--no-properties` deleted (the flag stays).
- [x] Tests that exist only to cover the property-ful variant removed.
- [x] `generated/Plato.Generated.Foundation.Unoptimized` regenerated, building clean, with its
      no-arg members emitted as methods; its `.csproj` header no longer claims it cannot use the
      method form.
- [x] ADR recorded at `tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md`.

## Out of scope

`CSharpWriter.PrimitiveSurfaceOverrides` and `StructSurfacePropertyNames` stay. They record where
the HANDWRITTEN runtime (`src/Plato.Intrinsics`) still spells a member as a property, which is a
fact about the runtime, not about this flag. Converting the runtime to method form is plato-331 and
is what would empty that table.
