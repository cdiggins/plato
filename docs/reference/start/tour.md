# Quick tour

This tour is one small complete program. Each idea links to the chapter that owns it.
For tools and install steps, see [Getting started](getting-started.md).

## A measured value

```plato
type Measurement { Value: Number; Uncertainty: Number; }

library Measurements {
    Scaled(m: Measurement, factor: Number): Measurement
        => (m.Value * factor, m.Uncertainty * factor.Abs);
}
```

`type Measurement { ... }` declares a **product**. Every value has both fields.
See [Products and construction](../language/products.md).

`Number` is a primitive scalar from the foundation library.
See [Numbers](../language/numbers.md) and the
[Number API page](../api/foundation/primitives/001-number.md).

`library Measurements { ... }` groups functions. See
[Functions and lambdas](../language/functions.md).

`Scaled(...) : Measurement => ...` is an expression-bodied function. The return
tuple becomes a `Measurement` by field order.

`m.Value` and `factor.Abs` are member forms of ordinary functions. See
[Reading and writing source](../language/syntax.md).

## Rules in the example

Values are immutable. Local `var` bindings exist for algorithms, but published
library types do not expose mutation. See
[Locals, loops, and builders](../language/state-and-builders.md).

A type describes shape and capability. An interface names what a type promises.
A generic carries that promise into a reusable function. See
[Interfaces](../language/interfaces.md) and
[Generics and bounds](../language/generics.md).

A sum holds alternatives. When a value is one of several cases, match on it
exhaustively. See [Sums and matching](../language/sums.md).

## Where to look

- [Language reference](../language/README.md): the chapter list.
- [Language cheat sheet](../language/cheatsheet.md): shapes at a glance.
- [Glossary](../language/glossary.md): words used throughout this site.
- [Library guide](../library/README.md): geometry, units, and domains.
