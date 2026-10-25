# Evaluation, order, and faults

Evaluation turns an expression into a value. A useful judgment is `ρ ⊢ e ⇓ v`: in value environment `ρ`, expression `e` evaluates to value `v`. An environment maps parameter/local identities to their values. This is distinct from the type environment `Γ` used when checking code.

The interpreter evaluates ordinary call arguments in source order, then applies the chosen function. It evaluates array and constructor elements in order. A function-value invocation evaluates its target before its arguments. A block executes statements sequentially, and `return` propagates outward without running later statements. These rules are visible directly in [Interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs).

## Conditional evaluation

Only the selected branch runs:

```text
ρ ⊢ c ⇓ true     ρ ⊢ a ⇓ v
--------------------------------
ρ ⊢ c ? a : b ⇓ v

ρ ⊢ c ⇓ false    ρ ⊢ b ⇓ v
--------------------------------
ρ ⊢ c ? a : b ⇓ v
```

`true` and `false` here are Boolean values, not arbitrary numeric truthiness. Thus `false ? FaultingComputation() : 1` does not evaluate the faulting branch, provided the whole expression type-checks.

For Boolean operators, `a && b` lowers to `a ? b : false`, and `a || b` lowers to `a ? true : b`. The right operand is evaluated only when needed. The named Boolean spellings `And(a, b)` and `a.And(b)` receive the same lazy lowering, as do Boolean `Or` calls. The elaborator checks the call name and that both arguments and the result are Boolean, not whether the source used operator punctuation. An `And` or `Or` overload over a custom non-Boolean type remains an ordinary eager call. A generic call that becomes Boolean only after specialization needs separate attention: the elaborator makes this decision using the types known when it runs.

## Eager arrays and folds

`MapRange` eagerly evaluates every indexed element once, in ascending index order, according to the specified intrinsic contract. Immutable arrays are materialized values, not an implicit lazy stream. A reduction visits its elements according to its runtime/library contract; it cannot arbitrarily use a parallel reduction tree for floating arithmetic.

Library predicates such as `All` or `Any` can stop invoking their predicate once the answer is known while still traversing remaining elements in a fold. “Predicate short circuiting” and “container traversal stops” are different promises. Read the actual combinator body when performance or termination depends on the distinction.

## Purity does not justify every rewrite

For a total mathematical function, repeated calls with identical inputs return equal outputs. Real programs also have faults, nontermination, numeric profiles, unique builders, and host obligations. An optimizer that moves a computation out of a conditional can turn a successful run into a fault. Reassociating floating addition changes rounding. Replacing a sequence of eager elements by a lazy iterator changes when work happens.

Consequently, behavioral equivalence must account for values, faults, and divergence, and preserve relevant evaluation order. “Both expressions are pure” alone is an insufficient proof of a rewrite.

## Known evaluation boundaries

Match lowering builds a conditional chain using a reusable subject subtree and payload projections; it does not insert a dedicated subject local. This can repeat evaluation of a nontrivial subject. Bind it first when its cost or fault behavior matters. Whether the source language should guarantee exactly one evaluation is an explicit specification issue, not a theorem assumed by this guide.

The interpreter can memoize expensive calls and has configurable depth/time limits. Those are execution controls, not a mathematical proof of termination. A timeout is an execution error; it does not prove the function diverges. Host C# exceptions may report bounds errors or unmet runtime obligations even though Plato has no source-level exception handlers.

## Evidence and related work

[Short-circuit construction](../../../src/Plato.Compiler/Checking/TirShortCircuit.cs), [short-circuit interpreter tests](../../../tests/Plato.Interpreter.Tests/ShortCircuitTests.cs), [Elaborator](../../../src/Plato.Compiler/Checking/Elaborator.cs), and [HostIntrinsics](../../../src/Plato.Interpreter/Evaluation/HostIntrinsics.cs) expose these mechanisms. [C# expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) specify operand evaluation separately from precedence; the same distinction matters in Plato. The [formal track](../formal/README.md) states the assumptions required for its narrower semantics and rewriting proofs.
