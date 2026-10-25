# Language reference

Plato describes immutable data and numerical algorithms. A scientist can give lengths, angles, positions, transformations, and algorithms distinct types, then run the same checked program in the interpreter or generate C# libraries. A **type** describes values; a **function** computes a result from arguments; an **interface** describes capabilities a type promises.

This reference teaches the language and records implementation evidence. [SEMANTICS](../../SEMANTICS.md) remains the normative source. Pages distinguish a specified rule from an implemented mechanism and an unresolved conflict. These documents deliberately omit gate tallies and directory inventories; use the repository's gate logs for measurements.

Start with the [program model](model.md), then [syntax](syntax.md) and [functions](functions.md). Read topics independently when needed:

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

For implementers, continue to [the implementation guide](../implementation/README.md), [the source grammar](../implementation/grammar.md), and [known discrepancies](../implementation/discrepancies.md). The [formal model](../formal/README.md) gives deliberately bounded theorems rather than a claim that the whole compiler has been proved correct.

## A small complete example

```plato
type Measurement { Value: Number; Uncertainty: Number; }

library Measurements {
    Scaled(m: Measurement, factor: Number): Measurement
        => (m.Value * factor, m.Uncertainty * factor.Abs);
}
```

`Measurement` is a product: both fields exist in every value. The function multiplies the measured value and scales uncertainty by a nonnegative magnitude. The return tuple is converted to the declared product by field order. This example assumes the foundation declarations and arithmetic library are included in the compilation. A source file alone does not implicitly import a prelude.

## How to read examples

`plato` fences use source syntax; `csharp` fences show host-language mappings and are labelled when schematic. Small snippets are fragments unless described as a complete input. Inference rules use `Γ ⊢ e : T`, read “in environment Γ, expression e has type T.” Each mathematical page defines additional notation before using it.
