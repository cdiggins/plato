# Interfaces, capabilities, and Self

An **interface** groups operations a type promises to support. Ordering needs a
comparison. Interpolation needs a way to blend two values. `concept` and
`interface` spell the same declaration kind.

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

The expression returns a new `Signal` from the blended scalar. No object inherits
implementation state. The type declares the capability. A library function or an
intrinsic supplies it.

[Haskell type classes](https://www.haskell.org/onlinereport/decls.html) describe
constrained polymorphism. Plato's `Self` functions and declared `implements`
relationships are closer to type classes than to ordinary C# object inheritance.

## Self and inheritance

`Self` is the concrete type that implements the interface. For `Signal`, the
signature above is `Blend(Signal, Signal, Number): Signal`. The result is that
concrete type, not an interface value. `inherits` adds the parent interface's
obligations and substitutes generic arguments:

```plato
interface ISampled<T> inherits IIndexable<T> {
    SamplingStep(x: Self): Number;
}
```

A type that implements `ISampled<Number>` also promises the inherited
`IIndexable<Number>` operations. The compiler follows the transitive interface
closure. It substitutes type arguments. Comparing interface names alone is not
enough. [ConceptClosure](../../../src/Plato.Compiler/Checking/ConceptClosure.cs)
walks that inheritance.

## Interface names in function signatures

A bare interface in a library function signature usually introduces an implicit
generic constraint:

```plato
BlendMid(a: IBlended, b: IBlended): IBlended => a.Blend(b, 0.5);
```

Every occurrence of `IBlended` in this signature stands for the same implementing
type. The first argument and the second argument are that one type. Independent
implementations need distinct signature variables, each with its own bound.

A missing interface member is a soft obligation in the current toolchain. The
linter reports LINT001, and a missing generated implementation may throw.
`implements` does not by itself prove that every executable body exists. Run the
linter and the intrinsic obligation checks as part of authoring.

## Stored interface values

A concrete product field whose type is an interface is different from the
constraint position above.
[ExistentialConceptChecker](../../../src/Plato.Compiler/Checking/ExistentialConceptChecker.cs)
treats the field as storage of "some implementing type." The C# writer has a
non-generic **object-safe view** for an eligible interface. Object-safe means a
caller can invoke the member without knowing a concrete `Self` that would
otherwise be hidden in an argument or a result. If every member uses `Self` beyond
the receiver, no usable view exists, and the compiler rejects the field with
CHK308. The checker also inspects an interface type nested in a generic field
argument.
[Existential tests](../../../tests/PlatoTests/Checking/ExistentialConceptCheckerTests.cs)
enforce that empty-view rejection.
[ConceptGrounding](../../../src/Plato.Compiler/Symbols/ConceptGrounding.cs)
distinguishes grounding a concept from this existential storage.

This storage conflicts with older blanket statements that Plato has no runtime
interface values and no heterogeneous storage. The constraint model is clear. The complete semantics of a stored existential
value, and interpreter agreement for that value, need further specification. This
C# mechanism does not make arbitrary object-oriented subtyping or runtime casts
into language features. See
[discrepancies](../implementation/discrepancies.md).
[Rust traits](https://doc.rust-lang.org/reference/items/traits.html) separate
concrete generic use from trait objects. Plato's distinction between a constraint
and storage is comparable. Plato does not adopt Rust's syntax, lifetime rules, or
complete object-safety model.

[Generic bounds](generics.md) says what the compiler can infer from a capability
promise.
