# Scalars and algebra

`Number` approximates a real number under the program's numeric profile. It is not an exact mathematical real. `Integer` is a signed whole-number primitive. `Boolean` has two truth values. `Character` and `String` represent a character and an immutable text value. Their host representation and edge cases belong to the [language reference](../../language/README.md). Do not assume that every C# operation is exposed in Plato.

## Values and capabilities

An interface names the operations a type provides. `IAdditive` has addition, subtraction, negation, and zero. `IScalable` has operations with a `Number` factor. `IMultiplicative` describes same-type multiplication. The selected overload defines the operation: distinguish `v * scalar` from a dot product or a matrix product.

`IInterpolatable.Lerp(a,b,t)` is an unclamped blend. In exact vector arithmetic its formula is `a + t*(b-a)`. At `t=0` the result is `a`, and at `t=1` it is `b`. Outside that interval it extrapolates. These identities require arithmetic assumptions. Floating-point overflow, NaN, and rounding can defeat literal equalities.

An algebraic name describes intended laws. Implementing the interface does not prove them. Floating-point addition is not associative, so changing a reduction order may change the result. `IArithmetic` is not evidence that every implementer is a mathematical field. Integer division truncates, and division by zero remains outside many contracts. Plato uses its own interfaces and floating profile. [CGAL's algebraic foundations](https://doc.cgal.org/latest/Manual/packages.html) do not transfer their exact-number facilities merely because an operation has a corresponding name.

## Comparison and constrained numbers

`ComparisonTolerance(Absolute, Relative)` expresses near-equality. The test is stated in the [global conventions](../../../../stdlib/CONVENTIONS.md):

```text
near(x,y) iff |x-y| <= Absolute + Relative * max(|x|,|y|).
```

Choose nonnegative allowances suited to the physical scale. Near-equality need not be transitive, so it is unsuitable as a sorting equivalence or as hash-key equality. Exact equality and hash agreement are a separate contract. A hash is an indexing aid, not a unique identity or a cryptographic digest.

Extended numbers include complex and rational values, constrained scalar wrappers, dual numbers, and special functions. A wrapper such as `Probability` communicates a range. Do not assume its raw constructor clamps. Dual numbers store a value and a derivative: `(a,a')*(b,b')=(ab,a'b+ab')`, implementing the product rule. Derivative formulas at discontinuities, or outside a function's domain, require separate analysis. A definition in the [NIST DLMF](https://dlmf.nist.gov/) still leaves a finite Plato approximation in need of its own accuracy and domain evidence.

## Where to look

- [Primitive declarations](../../api/foundation/primitives.md)
- [Core interfaces](../../api/foundation/core.concepts.md) and [algebra interfaces](../../api/foundation/algebra.concepts.md)
- [Core bodies](../../api/foundation/core.library.md), [algebra bodies](../../api/foundation/algebra.library.md), and [primitive bodies](../../api/foundation/primitives.library.md)
- [Constants](../../api/foundation/constants.library.md) and [hashing](../../api/foundation/hashing.library.md)
- [Extended number types](../../api/foundation/numbers.types.md) and [their bodies](../../api/foundation/numbers.library.md)
