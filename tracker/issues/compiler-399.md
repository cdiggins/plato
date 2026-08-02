---
id: compiler-399
title: Single-field mirror emits unsound implicit operators in generated C#
type: problem
status: ready
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-08-02
closed:
links: []
---

## What happens

For every concrete type with exactly one field, the C# writer emits an implicit conversion in
**both** directions between the type and its field's type. Observed in freshly generated output
(`--csharp-style=extensions --optimize --optimize-arrays --inline --loops --static-abstract`):

```csharp
// _Length.g.cs
public static implicit operator Number(Length self) => self.Meters;
public static implicit operator Length(Number value) => new Length(value);
public static implicit operator Length(Integer value) => new Length(value);
public static implicit operator Length(int value) => new Integer(value);
public static implicit operator Length(float value) => new Number(value);
public static implicit operator float(Length value) => value.Meters;

// _VertexIndex.g.cs
public static implicit operator Integer(VertexIndex self) => self.Value;
public static implicit operator VertexIndex(Integer value) => new VertexIndex(value);

// _Direction2D.g.cs
public static implicit operator Vector2D(Direction2D self) => self.Vector;
public static implicit operator Direction2D(Vector2D value) => new Direction2D(value);
```

## Why it matters

Each of these erases an invariant the vocabulary spends effort establishing, and erases it
**silently**, at every call site, with no cast in the source:

- **Units.** A `Length` is freely a `float` and a `float` is freely a `Length`. Any API taking a
  `Length` accepts a bare number, and a `Length` flows into any `float` parameter. The quantity
  types stop carrying dimension the moment the code is C#.
- **Typed indices.** `VertexIndex` and `Integer` are interchangeable, so a face index passed where
  a vertex index is expected compiles. `CONVENTIONS.md` ("Typed indices") exists to prevent exactly
  this.
- **Unit-vector invariant.** `Direction2D d = someUnnormalizedVector;` compiles and produces a
  `Direction2D` that is not unit length. Every consumer that assumes normalization is now wrong.

The *outbound* direction (wrapper to payload) is defensible: the value really is that payload.
The *inbound* direction is the unsound one — it asserts an invariant the source value has not
been checked against.

The `Direction2D` case has direct evidence that the library considers the inbound direction a
real operation and not a coercion: `numeric-structures.library.plato` offers `FromVector` (which
normalizes) and `FromVectorUnchecked` (which asserts), deliberately named so a reader sees which
one they took. The emitted `implicit operator Direction2D(Vector2D)` is a third, nameless door
that does neither.

## Relationship to the conversion convention

`stdlib/CONVENTIONS.md` section "Conversions" governs conversions written in Plato source, and
`tests/PlatoTests/ImplicitCastInventoryTests.cs` pins them. **Neither covers these.** The field
mirror is minted by the writer from the type's shape, so it appears in no Plato declaration and in
no cast-relation set (`TypeRelations.ComputeCasts`). The cast inventory golden is therefore a
partial view of what the shipped C# actually coerces.

## Options

1. Emit the outbound conversion implicitly and the inbound one as `explicit`. Keeps the
   convenience of reading the payload out; makes constructing the wrapper a visible decision.
   Likely the smallest sound change; will require call-site fixes wherever generated or
   handwritten code relies on the inbound direction.
2. Keep both, but only for types that opt in (a marker on the Plato declaration). More control,
   more vocabulary surface.
3. Extend the cast-inventory pin to also record writer-emitted operators, so at minimum the set is
   visible and cannot grow unnoticed. This does not fix the unsoundness but stops it spreading,
   and is independent of 1 and 2.

## Done means

- [ ] A decision is recorded in `tracker/decisions/` on which directions the field mirror may emit
- [ ] Generated C# matches that decision
- [ ] The pin (or a sibling pin) covers writer-emitted implicit operators, not only Plato-declared casts
