# Equality, ordering, and hashing

Equality asks whether a declared operation treats two values as the same value. Ordering asks which value comes first. Hashing assigns a compact integer for lookup. The three operations are different. Identical hashes do not prove equality, and an equality relation does not supply a total numeric order.

## Operator resolution comes first

`a == b` resolves `Equals(a, b)`. `a != b` resolves `NotEquals`. The types must satisfy the selected signature. Two nominal records with the same fields are not automatically comparable as the same type. A custom library operation still follows the ordinary overload rules.

The C# writer compares the fields of a generated product. An array field compares element by element, not by the identity of the host array object. A generated sum compares the active tag and the active payload. The interpreter's [ValueEquality](../../../src/Plato.Interpreter/Evaluation/Values.cs) uses the same comparison. This is **structural equality of values of a nominal type**. It is not structural typing. [CSharpConcreteTypeWriter](../../../writers/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs) distinguishes array-field equality and hashing.

For a product with fields `f₀,...,fₙ₋₁`, the intended derived relation is:

```text
x =R y  iff  x.f₀ =T₀ y.f₀ and ... and x.fₙ₋₁ =Tₙ₋₁ y.fₙ₋₁
```

`=R` means equality for product R, and `=Tᵢ` means the relevant equality for that field's value. For an array, the lengths must match and corresponding elements must compare equal. For a sum, the tags must match before the selected payloads are compared.

## Number equality

The interpreter's `Equals` for `Number` uses floating `.Equals`, and generated `Number` equality uses the wrapped floating value's `.Equals`. NaN compares equal to NaN under this equality, and positive zero equals negative zero. Native C# floating `==` does not treat NaN as equal to itself. Do not replace wrapper equality with native floating `==` during lowering merely because both return a Boolean.

[HostIntrinsics](../../../src/Plato.Interpreter/Evaluation/HostIntrinsics.cs) binds the interpreter's `Number` equality. The [.NET Double.Equals documentation](https://learn.microsoft.com/en-us/dotnet/api/system.double.equals) describes the NaN behavior this mapping inherits. That inheritance applies to `Number`. It does not import every .NET equality behavior into every Plato type.

An ordering comparison such as `LessThanOrEquals` keeps ordinary floating comparison. NaN does not become an ordered real because equality recognizes it. Trichotomy says that exactly one of less, equal, or greater holds. A theorem that assumes trichotomy has to exclude NaN, or it has to define a separate total order.

## Hashing

Equal values must receive compatible hashes under the implementation's hash contract. Unequal values may share a hash. A product combines its field hashes. An array hashes its elements. A sum accounts for the selected value. A `Number` hash follows the selected machine profile. The exact hash integers are host and implementation details. Do not persist an exact hash as a stable identifier or as a numerical fingerprint across runtime versions.

Structural equality can inspect every element of a collection. It is not necessarily constant time. Replacing collection equality with reference equality changes the meaning, including when many runs happen to share a backing array.

## Grammar

Equality has no production of its own. `==` and `!=` are two entries in the shared binary operator list, copied from [PlatoTokenGrammar.cs](../evidence/grammar-source.md) and [PlatoGrammar.cs](../evidence/grammar-source.md). `Equals` and `NotEquals` are then ordinary function names; see [expressions](expressions.md).

```text
public Rule BinaryOperator => Node(Symbols(
    "and", "or", "+", "-", "*", "/", "%", "<<", "&&", "||", "&", "|", "^",
    "+=", "-=", "*=", "/=", "%=", ">>>=", ">>=", "<<=", "&&=", "||=", "&=", "|=", "^=",
    "=", "<", ">", "<=", ">=", "==", "!="
));
public Rule BinaryOperation => Node(Not("=>") + BinaryOperator + AdvanceOnFail + Expression);
```

## Meaning

`a == b` calls `Equals(a, b)`. Two values of a nominal type are equal when their fields (or active case and payload) are
equal. Equality, ordering and hashing are separate operations. The authority is
[SEMANTICS section 6](../../SEMANTICS.md#6-expressions-construction-conversions-operators).

## Common mistake

The mistake is to compare values of two different types, here a record and a whole number:

```plato
type Reading {
    Estimate: Integer;
    Uncertainty: Integer;
}

library Bad {
    Same(r: Reading, n: Integer): Boolean => r == n;
}
```

This program is rejected by the type rules, which find no `Equals(Reading, Integer)`. The reference tool does not say so.
`lint --strict` accepts the file, and `run --laws` reports a failing law with an empty message:

```text
fail       Bad.Law_Same  case 1:
```

There is no CHK201, no mention of `Equals`, and no hint that the two types differ. Compare like with like:

```plato
library Good {
    Same(r: Reading, n: Integer): Boolean => r.Estimate == n;
}
```

A second trap is `Number`: NaN equals NaN under the wrapper's `Equals`, as the section above says, so `x == x` is not a NaN test.
