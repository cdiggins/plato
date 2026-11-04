# Language cheat sheet

Shapes only. Follow the links for the rules, the evidence, and the edge cases.

## Declarations

```plato
type Point2D { X: Number; Y: Number; }

type Option<T> = None | Some(Value: T);

interface Equatable {
    Equals(other: Self): Boolean;
}

library Geometry {
    Length(p: Point2D): Number => (p.X * p.X + p.Y * p.Y).Sqrt;
}
```

- Products: [Products and construction](products.md)
- Sums: [Sums and matching](sums.md)
- Interfaces: [Interfaces](interfaces.md)
- Functions: [Functions and lambdas](functions.md)

## Expressions

```plato
cond ? a : b    // both branches have one type
xs.Map(x => x * 2)
a.F(b)          // same as F(a, b)
(a, b)          // tuple
```

See [Expressions and operators](expressions.md) and [Syntax](syntax.md).

## Locals under a pure interface

```plato
var total = 0.0;
for (var i = 0; i < n; i = i + 1)
    total = total + xs[i];
return total;
```

See [Locals, loops, and builders](state-and-builders.md).

## Common traps

- Every function declares its result type: `F(x: Integer): Integer => x * x;`.
- There is no `+=`, `++` or `--`. Write `i = i + 1`. The command line does not reject the
  short forms; it drops the function silently. See [Diagnostics](diagnostics.md#what-the-command-line-shows).
- A library signature spells a type variable `$T`, not `T`. See [Generics](generics.md).
- `Library.F(x)` does not select a library. Write `F(x)` or `x.F`. See [Programs, values, and names](model.md).
- A `match` lists every case; there is no `_` arm. See [Sums and matching](sums.md).

## Reading order

1. [Programs, values, and names](model.md)
2. [Reading and writing source](syntax.md)
3. [Functions, receivers, and lambdas](functions.md)
4. Then products, sums, interfaces, and the rest from the
   [language index](README.md).
