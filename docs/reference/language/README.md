# Language reference

The language reference says what a Plato program means. A **type** describes values.
A **function** returns a result computed from its arguments. An **interface** is a
set of capabilities a type promises. [What is Plato](../start/what-is-plato.md)
introduces the language for a new reader. [SEMANTICS](../../SEMANTICS.md) is the
normative source.

A page separates a specified rule, an implemented mechanism, and an unresolved
conflict. These chapters omit gate tallies and directory inventories. The
[reference home](../README.md#authority-and-evidence) says where measurements live.

## A small complete example

```plato
type Measurement { Value: Number; Uncertainty: Number; }

library Measurements {
    Scaled(m: Measurement, factor: Number): Measurement
        => (m.Value * factor, m.Uncertainty * factor.Abs);
}
```

`Measurement` is a product: every value has both fields. `Scaled` multiplies the
measured value and scales the uncertainty by a nonnegative magnitude. The compiler
converts the returned tuple to the declared product by field order. The example
assumes the foundation declarations and the arithmetic library are included in the
compilation. A source file alone does not implicitly import a prelude.

## How to read examples

A `plato` fence shows source syntax. A `csharp` fence shows a host-language mapping
and is labelled when the mapping is schematic. A short snippet is a fragment unless
the page calls it a complete input. An inference rule uses `Γ ⊢ e : T`, read "in
environment Γ, expression e has type T." A mathematical page defines any further
notation before it uses that notation.

## Where to look

Start with the [program model](model.md), then [syntax](syntax.md) and
[functions](functions.md). Read any other chapter on its own when you need it:

- [Products and construction](products.md): records, fields, tuples, defaults, sealed construction.
- [Sums and matching](sums.md): alternatives with payloads and exhaustive case analysis.
- [Interfaces](interfaces.md): capabilities, `Self`, inherited obligations, and stored interface values.
- [Generics and bounds](generics.md): type parameters, signature variables, and constraints.
- [Expressions and operators](expressions.md): precedence, conditionals, collections, conversions.
- [Functions and lambdas](functions.md): receiver syntax and higher-order functions.
- [Locals, loops, and builders](state-and-builders.md): local state under a pure interface.
- [Evaluation](evaluation.md): order, short circuiting, faults, and termination.
- [Numbers](numbers.md): arithmetic profiles, rounding, and scientific assumptions.
- [Equality and ordering](equality.md): structural comparison, NaN, and hashing.
- [Diagnostics and unsupported forms](diagnostics.md): the difference between parsing and support.

The [glossary](glossary.md) and [cheat sheet](cheatsheet.md) are short reminders.
New readers may want the [Quick tour](../start/tour.md) before these chapters.

Implementers continue to the [implementation guide](../implementation/README.md),
the [source grammar](../implementation/grammar.md), and
[known discrepancies](../implementation/discrepancies.md). The
[formal model](../formal/README.md) states a small set of theorems and the limits
of those theorems.
