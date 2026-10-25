# Equality, ordering, and hashing

Equality asks whether two values are considered the same value under a declared operation. Ordering asks which comes before which. Hashing assigns a compact integer suitable for lookup. These are different operations; identical hashes do not prove equality, and an equality relation does not automatically supply a total numeric order.

## Operator resolution comes first

`a == b` resolves the named `Equals(a, b)` operation; `!=` resolves `NotEquals`. Types must satisfy the selected signature. Two different nominal records with identical fields are not automatically comparable as the same type. Custom library operations still participate in the normal overload rules.

Generated products compare their fields. Array fields compare element by element rather than by the identity of the host array object. Generated sums compare the active tag and active payload. The interpreter's [ValueEquality](../../../src/Plato.Interpreter/Evaluation/Values.cs) supplies matching structural value comparisons. This is **structural equality of values of a nominal type**, not structural typing.

For a product with fields `f₀,...,fₙ₋₁`, the intended derived relation is:

```text
x =R y  iff  x.f₀ =T₀ y.f₀ and ... and x.fₙ₋₁ =Tₙ₋₁ y.fₙ₋₁
```

`=R` means equality for product R and `=Tᵢ` the relevant field-value equality. For arrays, lengths must match and corresponding elements must compare equal. For sums, tags must match before comparing the selected case payloads.

## Number equality has a surprising host choice

The interpreter's Number `Equals` uses floating `.Equals`, and generated Number equality uses its wrapped floating value's `.Equals`. Therefore NaN compares equal to NaN under this equality, and positive zero equals negative zero. This differs from native C# floating `==`, for which NaN does not equal itself. Do not replace wrapper equality by native floating `==` during lowering merely because both return a Boolean.

Ordering comparisons such as `LessThanOrEquals` retain floating comparison behavior, so NaN does not become an ordinary ordered real simply because the equality relation recognizes it. A theorem assuming trichotomy (exactly one of less/equal/greater) needs to exclude NaN or define a separate total order.

## Hashing

Equal values must receive compatible hashes within the chosen implementation's hash contract. Unequal values may share a hash. Products combine field hashes; arrays hash their elements; sums account for their selected value. Number hashes follow the selected machine profile. Exact hash integers are host/implementation details and should not be persisted as stable identifiers or used as a numerical fingerprint across runtime versions.

Structural equality can inspect every element of a collection. It is not necessarily constant time. An optimizer replacing collection equality with reference equality would change semantics even when many executions happen to share a backing array.

## Evidence and related work

[CSharpConcreteTypeWriter](../../../writers/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs) distinguishes array-field equality and hashing. [HostIntrinsics](../../../src/Plato.Interpreter/Evaluation/HostIntrinsics.cs) binds Number equality; [ValueEquality](../../../src/Plato.Interpreter/Evaluation/Values.cs) compares interpreter values. The [.NET Double.Equals documentation](https://learn.microsoft.com/en-us/dotnet/api/system.double.equals) describes the NaN behavior that this mapping inherits. This is a deliberate implementation fact to expose in numerical reasoning, not a license to import every .NET equality behavior into every Plato type.
