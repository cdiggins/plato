---
id: compiler-397
title: IArrayLike types with no generated fields emit an empty component type
type: bug
status: ready
priority: p2
effort: S
risk: low
area: compiler
sprint:
created: 2026-08-01
closed:
links: [decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md]
---

## Issue

`CSharpConcreteTypeWriter`'s `IArrayLike` block derives the component type from
`FieldTypes[0]`, falling back to `null` when the type declares no fields, and only the
`IsPrimitive` branch supplies a default (`"Number"`). A non-primitive `IArrayLike` type whose
fields the writer does not generate therefore emits an empty type name:

```csharp
public static Angle CreateFromComponents(IReadOnlyList<> numbers) => new Angle();
public static Angle CreateFromComponent( x) => new Angle();
```

`error CS1001: Identifier expected`.

## How it surfaced

Regenerating `tests/optimizer-smoke` after scalar erasure was retired. `Angle` stopped being a
`CSharpWriter.PrimitiveTypes` entry at plato-365, so it takes the non-primitive path while still
being `IArrayLike` with no writer-generated fields.

This is **not** caused by the erasure removal — the null-component-type path predates it and the
erasure branch never repaired it either. It was invisible because `tests/optimizer-smoke` could
not run at all: `regen-smoke.ps1` resolved `Plato.CLI` and `stdlib-legacy` as siblings of
`tests/`, a pre-reorg layout, so it threw before generating anything. Those paths are fixed now,
which is what exposed this.

## Impact

Blocks the `optimizer-smoke` suite (all four variants; one error each, same line). Does not
affect `stdlib/foundation` — the live forward tier regenerates byte-identically and builds clean.
So this is legacy-corpus-only today, but the defective branch is in the shared writer and any
forward type that becomes a fieldless `IArrayLike` would hit it.

## Fix approaches

1. **Default the component type the same way the primitive branch does** — when `FieldTypes` is
   empty, fall back to the type's declared `IArrayLike` element type from the interface
   instantiation rather than to `null`. Correct, needs the interface argument to be read.
2. **Fail loudly** — throw when an `IArrayLike` type resolves a null component type, the way the
   neighbouring code already throws for mismatched field types. Turns a CS1001 in generated C#
   into a generator error naming the type. Cheap, and strictly better than emitting `<>`.

Do 2 regardless; 1 is the actual fix.

## Simplest fix

Add the null guard (approach 2) next to the existing
`"IArrayLike types are assumed to have all of the fields of the same type"` throw, then resolve
the element type from the `IArrayLike` interface instantiation.
