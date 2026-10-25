# Scalars and algebra

Start with [primitive declarations](../../api/foundation/primitives.md), [core interfaces](../../api/foundation/core.concepts.md), [algebra interfaces](../../api/foundation/algebra.concepts.md), and their [core](../../api/foundation/core.library.md), [algebra](../../api/foundation/algebra.library.md) and [primitive bodies](../../api/foundation/primitives.library.md). [Constants](../../api/foundation/constants.library.md) supply named mathematical and representation values; [hashing](../../api/foundation/hashing.library.md) supplies hash composition.

## Values and capabilities

`Number` approximates a real number using the program's numeric profile; it is not an exact mathematical real. `Integer` is a signed whole-number primitive. `Boolean` has two truth values. `Character` and `String` represent character and immutable text values. Their host representation and edge cases belong to the [language reference](../../language/README.md), not an assumption that every C# operation is exposed in Plato.

An interface names available operations. `IAdditive` provides addition, subtraction, negation and zero. `IScalable` provides operations with a `Number` factor. `IMultiplicative` describes same-type multiplication. Distinguish `v * scalar` from a dot product or matrix product: the selected overload defines the operation.

`IInterpolatable.Lerp(a,b,t)` is an unclamped blend. In exact vector arithmetic its formula is `a + t*(b-a)`. At `t=0` the result is `a`, at `t=1` it is `b`; outside that interval it extrapolates. These identities require arithmetic assumptions: floating-point overflow, NaN and rounding can defeat literal equalities.

An algebraic name describes intended laws, not a proof supplied by interface implementation. Floating-point addition is not associative: changing a reduction order may change its result. `IArithmetic` is not evidence that every implementer is a mathematical field; integer division truncates, and division by zero remains outside many contracts.

## Comparison and constrained numbers

`ComparisonTolerance(Absolute, Relative)` expresses near-equality:

```text
near(x,y) iff |x-y| <= Absolute + Relative * max(|x|,|y|).
```

Choose nonnegative allowances suited to the physical scale. Near-equality need not be transitive, so it is unsuitable as a sorting equivalence or hash-key equality. Exact equality and hash agreement are a separate contract. A hash is an indexing aid, not a unique identity or cryptographic digest.

[Extended numbers](../../api/foundation/numbers.types.md) and [their bodies](../../api/foundation/numbers.library.md) include complex and rational values, constrained scalar wrappers, dual numbers, and special functions. A wrapper such as `Probability` communicates a range; do not assume its raw constructor clamps. Dual numbers store value and derivative: `(a,a')*(b,b')=(ab,a'b+ab')`, implementing the product rule. Derivative formulas at discontinuities or outside a function's domain require separate analysis.

## Related work

[CGAL's algebraic foundations](https://doc.cgal.org/latest/Manual/packages.html) also organize algorithms around capabilities. Plato uses its own interfaces and floating profile; CGAL's exact-number facilities do not transfer merely because operations have corresponding names. [NIST DLMF](https://dlmf.nist.gov/) supplies mathematical definitions for the special functions; a finite numerical approximation in Plato needs its own accuracy and domain evidence.
