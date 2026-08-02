---
date: 2026-08-02
title: The single-field mirror unwraps only; wrapping is a constructor call
status: accepted
superseded-by:
links: [../issues/compiler-399.md]
---

## Context

For every concrete type with exactly one field, the C# writer minted an implicit conversion in
**both** directions between the type and its field's type — `implicit operator Number(Length)` and
`implicit operator Length(Number)`, plus `Integer` / `int` / `float` inbound bridges whenever the
field was a `Number`.

The inbound half erases invariants the vocabulary spends real effort establishing, and erases them
silently at every call site with no cast in the source: a bare number becomes a `Length` with no
unit, an `Integer` becomes a `VertexIndex` (the typed-index discipline in `stdlib/CONVENTIONS.md`
exists to stop exactly a face index arriving where a vertex index is expected), and an
unnormalized `Vector2D` becomes a `Direction2D`. That last one has direct evidence the library
disagrees with the coercion: `numeric-structures.library.plato` offers `FromVector`, which
normalizes, and `FromVectorUnchecked`, which asserts, deliberately named so a reader sees which
one they took. The emitted operator was a third, nameless door that did neither.

**The language never had this conversion.** `SymbolFactory` synthesizes exactly one cast for a
one-field type — the UNWRAP, `Number(arg: Length): Number` — and a constructor for the wrap.
`TypeRelations.ComputeCasts` picks up only declared and reified functions, so no wrap relation
exists on the Plato side and the checker never coerces that way. The cast-inventory pin
(`tests/PlatoTests/implicit-cast-inventory.txt`), which reads that same relation set, could not
see the writer's mirror at all. The C# was coercing where Plato does not, and the pin that was
supposed to make every coercion visible was a partial view.

## Decision

**Unwrap implicitly; do not emit the wrap at all.**

- `implicit operator F(T)` — reading the payload out of a one-field wrapper — stays implicit. The
  value really is that payload; nothing is asserted. For a `Number` payload the direct
  `implicit operator float(T)` stays too.
- `implicit operator T(F)` is **not emitted**, and neither are the `Integer` / `int` / `float`
  inbound bridges. Constructing a wrapper is `new T(f)`, or a named library function
  (`FromVector`, `FromVectorUnchecked`, `FromAmount`), or a conversion **declared in Plato**.

`explicit operator T(F)` was considered and rejected. It would still be a nameless door — for
`Direction2D` it reads as routine while asserting normalization — and it would collide with
declared conversions: an explicit operator in the target type plus a declared implicit one in the
source type are two equally applicable user-defined conversions for the same pair, which is
ambiguous (CS0457) at any explicit cast site.

**Declared conversions are unaffected and now actually emit.** A one-parameter Plato function
named after its return type (`Angle(x: Number): Angle` in `angles.library.plato`) is still an
implicit conversion, still governed by the faithfulness bar in `stdlib/CONVENTIONS.md`, and still
pinned. It used to be *suppressed* by `CSharpTypeWriter.WriteMemberFunction`, which skipped it
because the target type's shape mirror already covered the pair; with the mirror gone that guard
is wrong and was removed. `implicit operator Angle(Number)` now comes from the declaration, in
`_Number.g.cs`, where the pin can account for it.

Value-tuple converters on multi-field types are out of scope and unchanged.

## Consequences

- Generated C# no longer coerces a raw payload into a discipline type. Any consumer that relied on
  it gets a compile error naming the exact site, which is the point.
- Every implicit conversion between two named types in the generated library now traces to either
  a Plato declaration or the unwrap rule. Nothing is minted that the language does not have.
- The blind spot is closed by a sibling pin,
  `tests/PlatoTests/EmittedConversionInventoryTests.cs` (golden
  `emitted-conversion-inventory.txt`), which inventories the conversion operators the writer
  actually **emits** rather than the relations the checker computes. A re-added wrap fails it.
- Measured fallout across the shipping tiers was **none**: the forward-conformance build and
  `generated/Plato.Generated.Foundation.Unoptimized` both hold their previous error counts, so no
  stdlib body was leaning on the nameless wrap.
