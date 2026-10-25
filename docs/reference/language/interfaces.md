# Interfaces, capabilities, and Self

An interface groups operations a type promises to support. For example, ordering needs a comparison operation; interpolation needs a way to blend two values. `concept` and `interface` spell the same declaration kind.

```plato
interface IBlended {
    Blend(a: Self, b: Self, t: Number): Self;
}

type Signal implements IBlended { Value: Number; }

library Signals {
    Blend(a: Signal, b: Signal, t: Number): Signal
        => Signal(a.Value + (b.Value - a.Value) * t);
}
```

The expression constructs a new Signal from the blended scalar. No object inherits implementation state. The type declares a capability; library or intrinsic functions supply it.

## Self and inheritance

`Self` means the concrete implementing type. For `Signal`, the signature above becomes `Blend(Signal, Signal, Number): Signal`. It is not an interface-valued result. `inherits` includes the obligations of parent interfaces, substituting generic arguments along the way:

```plato
interface ISampled<T> inherits IIndexable<T> {
    SamplingStep(x: Self): Number;
}
```

A type implementing `ISampled<Number>` also promises the inherited `IIndexable<Number>` operations. The implementation follows a transitive interface closure; it must substitute type arguments rather than compare only interface names.

## Interface names in function signatures

A bare interface in a library function signature usually introduces an implicit generic constraint:

```plato
BlendMid(a: IBlended, b: IBlended): IBlended => a.Blend(b, 0.5);
```

Every occurrence of `IBlended` in this signature stands for the same implementing type. It does not mean the first argument can be one implementation and the second another. To permit independent types, use distinct explicit signature variables with suitable bounds.

Missing interface members are a soft obligation in the existing toolchain: the linter reports LINT001, and missing generated implementations may throw. `implements` is not by itself proof that all executable bodies exist. Treat the linter and intrinsic obligation checks as part of authoring, not optional formatting.

## Stored interface values: an important boundary

A concrete product field typed as an interface is different from the constraint position above. [ExistentialConceptChecker](../../../src/Plato.Compiler/Checking/ExistentialConceptChecker.cs) explicitly treats it as storage of “some implementing type,” and the C# writer has a non-generic **object-safe view** for eligible interfaces. “Object-safe” here means a member can be called without knowing an otherwise hidden concrete `Self` in argument/result positions. If every member uses `Self` beyond the receiver, no usable view exists and CHK308 rejects the field. The checker also inspects interface types nested in generic field arguments.

This conflicts with older blanket statements that Plato has no runtime interface values or heterogeneous storage. The supported constraint model is clear; the complete semantics and interpreter agreement of stored existential values need further specification. Do not conclude from this C# mechanism that arbitrary OO subtyping or runtime casts are language features. See [discrepancies](../implementation/discrepancies.md).

## Evidence

[ConceptClosure](../../../src/Plato.Compiler/Checking/ConceptClosure.cs) follows capability inheritance. [ConceptGrounding](../../../src/Plato.Compiler/Symbols/ConceptGrounding.cs) distinguishes grounding from existential storage. [Existential tests](../../../tests/PlatoTests/Checking/ExistentialConceptCheckerTests.cs) enforce the empty-view rejection. [Generic bounds](generics.md) explains what the compiler can infer from capability promises.

## Related work

[Haskell type classes](https://www.haskell.org/onlinereport/decls.html) describe constrained polymorphism; Plato's `Self` functions and declared `implements` relationships are closer to this than ordinary C# object inheritance. [Rust traits](https://doc.rust-lang.org/reference/items/traits.html) distinguish concrete generic use from trait objects; Plato's constraint-versus-storage distinction is comparable, but Plato does not adopt Rust's syntax, lifetime rules, or complete object-safety model.
