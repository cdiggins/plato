---
date: 2026-08-01
title: Property-free C# emission is unconditional
status: accepted
superseded-by:
links: [../issues/compiler-387.md, ../issues/plato-331.md]
---

## Context

The C# writer could emit a no-arg Plato member two ways: as a C# property, or as a C# method. The
choice was a compiler flag (`--no-properties`, and its older weaker sibling `--methods`), carried
through `CSharpWriterExtensions.ToCSharp` and stored as `CSharpWriter.NoProperties`.

Only the method form is wanted. It is what the handwritten runtime (`src/Plato.Intrinsics`) is
shaped like, it is what every shipping recipe asked for, and it is the only form any test
deliberately exercised. The property form was a second output shape that nobody consumed and every
member-spelling site in the writer had to branch on.

The flag was also entangled with something unrelated. `ToCSharp` rejected `--no-properties` unless
`--scalar=float` was also given, on the grounds that property-free interfaces only made sense with
scalar-erased ones. Because the two could not come apart, roughly a dozen sites in the writer read
`NoProperties` when what they actually meant was "the five scalar wrapper types are erased to
native primitives" — the type of a Boolean-returning helper, whether an explicit interface
implementation should be wrapper-typed, whether the `IArrayLike` scaffolding returns `Integer` or
`int`, whether a scalar receiver has a generated struct at all.

That entanglement blocked the forward shipping direction. The 2026-08-01 decision behind
`generated/Plato.Generated.Foundation.Unoptimized` keeps WRAPPER scalars and performs no erasure.
Under the old guard that project therefore could not ask for the method form, and was the one
target still emitting properties.

## Decision

**The user's call: property-free emission is the correct and only behaviour.** It is not a flag,
not a recipe choice, and there is no property-ful variant to test.

- `--no-properties` and `--methods` are removed from the CLI; the `noProperties` / `methodsOnly`
  parameters and the `CSharpWriter.NoProperties` field are gone. Every generated C# target emits
  no properties and no indexers.
- Before that could happen, every `NoProperties` test had to be classified. The ones about
  member SPELLING collapsed to their method branch. The ones that were really about scalar
  erasure now test `ScalarErase` directly, so the two axes are independent: a wrapper-scalar
  recipe is exactly as property-free as an erased one.
- `--static-abstract` keeps its flag but loses its dependency guard on `--no-properties`, which
  had become vacuous.

Scalar erasure policy is untouched: wrapper scalars with no erasure remain the forward direction.

## Consequences

- `generated/Plato.Generated.Foundation.Unoptimized` now emits methods and still builds clean.
  Its project header no longer claims the method form is unavailable to it.
- Two obligations in the handwritten runtime had to follow the interface they implement:
  `NumericalLimits<Self>.MinValue` / `.MaxValue` on `Number` and `Integer` were explicit interface
  PROPERTIES and are now explicit interface METHODS. They sit inside the
  `PLATO_FORWARD_CONCEPTS` guard, are invisible on the public surface, and are forced by the
  generated interface changing shape — not a step of the runtime conversion below.
- `CSharpWriter.PrimitiveSurfaceOverrides` and `StructSurfacePropertyNames` remain. They record
  where the HANDWRITTEN runtime still spells a member as a struct field or property, which is a
  fact about that runtime rather than about any emitter flag. Converting the runtime itself to
  method form is **plato-331 and remains open**; that is the change that would empty the override
  table, and this decision does not do it or depend on it.
