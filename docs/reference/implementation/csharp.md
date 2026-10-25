# Mapping Plato to C#

C# is the production-oriented target used in this reference. It is evidence for host representation and executable behavior, not permission to expand Plato into all of C#. [CSharpWriter](../../../writers/Plato.CSharpWriter/CSharpWriter.cs), [ExtensionStyleWriter](../../../writers/Plato.CSharpWriter/ExtensionStyleWriter.cs), and [TirCSharpBodyWriter](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) are the rendering authorities.

## Values and members

Ordinary products emit as readonly partial structs with stored fields and derived operations. Ordinary computed no-extra-argument functions emit as methods. Stored product fields remain public readonly fields, so a Plato field read `p.X` maps to C# `p.X`, while a computed `p.Magnitude` normally maps to `p.Magnitude()`. Some handwritten primitive operations retain properties, identified by the writer's primitive surface table. There is no switchable source property/indexer style; the blanket phrase “property-free” in older prose should not be read as erasing these field/runtime exceptions.

Schematic mapping (illustrative, not a golden output contract):

```plato
type Position { X: Number; Y: Number; }
library Positions { ShiftX(p: Position, d: Number): Position => (p.X + d, p.Y); }
```

```csharp
// Schematic consumer shape:
var shifted = position.ShiftX(delta);
var x = shifted.X;
```

Functions generally become classic extension methods grouped in a static class for each library, with derived/runtime members on their associated structs where required. A function over a closed instance of a generic sum belongs in a library extension class; it cannot be grafted as a member of only one closed C# generic instantiation.

The generated constructor accepts fields in declaration order. Immutable `With...` functions construct a new product. C# tuple bridges and conversion operators implement compatible source conversions. A one-field wrapper has an unwrap direction under the single-field policy; a bare payload is not automatically an implicit wrap. Sealed source construction does not make the C# constructor private.

## Sums

A sum emits as a readonly partial struct with an integer case tag, flattened payload fields, per-case predicates/accessors, and static factories. Tags follow declaration order. Generic sums retain generic parameters; a payload case use names the instantiated struct in generated code. Only active payload fields have source meaning. Host default construction and forged host values must not be treated as evidence that an arbitrary source invariant holds.

The flattened representation is not a promise of optimal memory layout, reference-free storage for every payload, or host-level prevention of invalid states. Equality and other derived operations must account for the active case rather than reading unrelated payloads as meaningful source fields.

## Scalars and profiles

`Number`, `Integer`, `Boolean`, `Character`, and `String` retain wrapper identities. Scalar erasure flags are retired. Machine arithmetic lives behind runtime aliases and wrapper operations. Number uses binary64 or binary32 according to the [profile](../language/numbers.md), and Integer uses the paired signed width. Generated profile guards prevent compiling under a conflicting runtime setting.

Array and function primitives map to runtime collection/delegate support rather than generated field-only products. List/Buffer builders map to handwritten builder types. C# host indexing/count APIs can impose a narrower integer range than the language Integer representation.

## Interfaces and generic code

Constraint-style interfaces can lower to generic self-constrained interfaces and concrete stamped methods. The optional static-abstract rendering mode affects the C# capability representation, not source syntax. Stored existential interface fields can lower to non-generic object-safe views; that boundary is described in [interfaces](../language/interfaces.md) and still needs a fully reconciled normative account.

Generic structs and some generic methods remain in generated C#. Specialization resolves the concrete member instances the reifier requests; it does not mean every generic symbol is textually erased from all emitted code.

## Body generation and optimization

All C# bodies render from TIR. The historical AST-based body writer and `--no-tir` switch are retired. Optimizer flags are separate transformations over that typed representation. [The optimizer pipeline](../../csharp-optimizer-pipeline.md) defines pass order and refusal conditions; do not infer correctness from a performance improvement or from C# compilation alone.

Generated output must compile with the default language version on net8.0. Do not emit newer syntax merely because a locally installed compiler accepts it. An unavailable typed body is emitted as a degraded throwing stub with diagnostics. CLI success alone does not mean the library contains usable implementations.

## Runtime trust boundary

Bodiless primitive signatures are discharged by [Plato.Intrinsics](../../../src/Plato.Intrinsics/). The runtime must supply matching signatures and behavior, including profile rounding, eager element order, and builder invalidation. The irreducible intrinsic kernel should not absorb functions expressible as ordinary Plato bodies; backend overrides can recover host performance without widening the semantic primitive surface.

## Related work

[C# structs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct) explain host value representation. [C# expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) define host conversions and evaluation; the writer must preserve Plato's selected operations rather than delegate unresolved semantic choices to host overload lookup. No claim in this page relies on the other experimental writers.
