# Language cheat sheet

Shapes only. Follow the links for the rules, the evidence, and the edge cases.

## Declarations

```plato
type Point2D { X: Number; Y: Number; }

type Option<T> = None | Some(T);

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
if cond then a else b
xs.Map(x => x * 2)
a.F(b)          // same as F(a, b)
(a, b)          // tuple
```

See [Expressions and operators](expressions.md) and [Syntax](syntax.md).

## Locals under a pure interface

```plato
var total = 0.0;
for (var i = 0; i < n; i += 1)
    total += xs[i];
total
```

See [Locals, loops, and builders](state-and-builders.md).

## Reading order

1. [Programs, values, and names](model.md)
2. [Reading and writing source](syntax.md)
3. [Functions, receivers, and lambdas](functions.md)
4. Then products, sums, interfaces, and the rest from the
   [language index](README.md).
