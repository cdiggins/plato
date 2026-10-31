# Find a root with Newton's method

Evaluate the cubic `x^3 - 2x - 5` and its derivative, then use both to find the real root near 2.

Needs: `stdlib/foundation`. Status: Supported.

## The program

Source: [newton-root.plato](../implementation/examples/cookbook-newton-root/newton-root.plato).

```plato
library CookbookNewtonRoot {
    // Newton's method for a cubic: repeat x <- x - p(x) / p'(x), a fixed number
    // of times. Precondition: p'(x) is non-zero along the way.
    NewtonCubic(p: CubicPolynomial, start: Number, steps: Integer): Number
        => steps.MapRange(i => i).Reduce(start, (x, i) => x - p.Eval(x) / p.Derivative(x));

    // x^3 - 2x - 5, the cubic Newton used as his own example.
    Example(): CubicPolynomial => new CubicPolynomial(1.0, 0.0, -2.0, -5.0);

    Law_ReferenceNewtonValue(): Boolean
        => Example().Eval(2.0).AlmostEqual(-1.0, 1e-12);
    Law_ReferenceNewtonDerivative(): Boolean
        => Example().Derivative(2.0).AlmostEqual(10.0, 1e-12);
    Law_ReferenceNewtonRoot(): Boolean
        => NewtonCubic(Example(), 2.0, 6).AlmostEqual(2.0945514815423265, 1e-9);
    Law_ReferenceNewtonResidual(): Boolean
        => Example().Eval(NewtonCubic(Example(), 2.0, 6)).Abs < 1e-9;
}
```

## Result

| Quantity | Value |
|---|---|
| `p(2)` | -1 |
| `p'(2)` | 10 |
| Root after 6 steps from 2 | 2.0945514815423265, to `1e-9` |
| `Abs(p(root))` | below `1e-9` |

## Walk through

1. `CubicPolynomial(A, B, C, D)` is `A*x^3 + B*x^2 + C*x + D`, school order. The example is `(1, 0, -2, -5)`.
2. `p.Eval(x)` and `p.Derivative(x)` are the library's closed forms for the cubic.
3. One Newton step replaces `x` by `x - p(x) / p'(x)`. `Reduce` threads `x` through a fixed number of steps; the index is unused and only counts the steps.
4. At `x = 2` the value is -1 and the slope is 10, so the first step moves to 2.1, already close to the root.

## Library pages used

- Guide: [Polynomials, series and basis functions](../library/foundation/polynomials.md), [Collections](../library/foundation/collections.md).
- API: [polynomials.types](../api/foundation/polynomials.types.md) (`CubicPolynomial`), [polynomials.library](../api/foundation/polynomials.library.md) (`Eval`, `Derivative`).

## Limits

- The loop runs a fixed number of steps and has no convergence test. Six steps are enough from this start; another function or start may need more or may never converge.
- A step divides by `p'(x)`. A start at a stationary point of `p` (here `x = sqrt(2/3)`) divides by zero.
- Newton's method finds one root near the start. It does not enumerate roots. A repeated root converges slowly.
- The polynomial guide warns that coefficient order differs between school-form records such as `CubicPolynomial` and the dense, ascending-power `Polynomial`; do not mix them.
