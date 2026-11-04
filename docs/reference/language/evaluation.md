# Evaluation, order, and faults

Evaluation turns an expression into a value. The judgment `ρ ⊢ e ⇓ v` means that, in value environment `ρ`, expression `e` evaluates to value `v`. An environment maps parameter and local identities to their values. That environment is separate from the type environment `Γ` the checker uses.

The interpreter evaluates ordinary call arguments in source order, then applies the chosen function. It evaluates array elements and constructor elements in order. A call through a function value evaluates the target before the arguments. A block runs its statements in order, and `return` propagates outward without running later statements. These rules are in the [interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs).

[C# expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) specify operand evaluation separately from precedence. Plato keeps the same split. Grouping is under [precedence](expressions.md).

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

For Boolean operators, `a && b` lowers to `a ? b : false`, and `a || b` lowers to `a ? true : b`. The right operand is evaluated only when needed. The named Boolean spellings `And(a, b)` and `a.And(b)` receive the same lazy lowering, as do Boolean `Or` calls. The elaborator checks the call name and that both arguments and the result are Boolean. It does not check whether the source used operator punctuation. An `And` or `Or` overload over a custom non-Boolean type remains an ordinary eager call. A generic call that becomes Boolean only after specialization needs separate attention: the elaborator makes this decision with the types known when it runs.

See [short-circuit construction](../../../src/Plato.Compiler/Checking/TirShortCircuit.cs), the [short-circuit interpreter tests](../../../tests/Plato.Interpreter.Tests/ShortCircuitTests.cs), and the [elaborator](../../../src/Plato.Compiler/Checking/Elaborator.cs).

## Eager arrays and folds

`MapRange` eagerly evaluates every indexed element once, in ascending index order, as its intrinsic contract specifies. The interpreter binds it in [HostIntrinsics](../../../src/Plato.Interpreter/Evaluation/HostIntrinsics.cs). An immutable array is a materialized value. It is not an implicit lazy stream. A reduction visits its elements according to its runtime or library contract. An optimizer must not replace that order with a parallel reduction tree for floating arithmetic.

A library predicate such as `All` or `Any` can stop invoking its predicate once the answer is known while still traversing remaining elements in a fold. Predicate short circuiting and stopping the container traversal are different promises. Read the combinator body when performance or termination depends on the difference.

## Purity does not justify every rewrite

A total mathematical function returns equal outputs for identical inputs. A Plato program can also fault, fail to terminate, follow a numeric profile, use a unique builder, or hit a host obligation. An optimizer that moves a computation out of a conditional can turn a successful run into a fault. Reassociating floating addition changes rounding. Replacing a sequence of eager elements with a lazy iterator changes when the work runs.

A valid rewrite preserves values, faults, divergence, and the evaluation order that matters. Two expressions that are both pure can still differ on those points. The [formal track](../formal/README.md) states the assumptions behind its narrower rewriting proofs.

## Known evaluation boundaries

Match lowering builds a conditional chain from a reusable subject subtree and payload projections. It does not insert a dedicated subject local. A nontrivial subject can therefore be evaluated more than once. Bind it first when its cost or its faults matter. Whether the source language should guarantee exactly one evaluation is an explicit specification issue.

The interpreter can memoize an expensive call, and it has configurable depth and time limits. Those are execution controls, not a proof that a function terminates. A timeout is an execution error. It does not show that the function diverges. A host C# exception can report a bounds error or an unmet runtime obligation. Plato has no source-level exception handlers.

## Grammar

This chapter has no single grammar fragment: it describes how expressions run, not a
declaration form. The form it discusses most is the conditional. Its rule from [PlatoGrammar.cs](../evidence/grammar-source.md#platogrammarcs) is:

```text
public Rule TernaryOperation => Node(Sym("?") + AdvanceOnFail + Expression + Sym(":") + Expression);
```

[Expressions](expressions.md) and [Functions](functions.md) carry the rest.

## Meaning

Arguments evaluate in source order and only the selected branch of a conditional runs.
Plato is eager by default, but the language does not promise that unnecessary pure
work is performed. The accepted decision on
[mixed evaluation](../../../tracker/decisions/2026-10-25-mixed-evaluation-preserves-required-results.md)
lets the compiler skip pure computation, including faults confined to it, as long as every
defined successful result and all required work are preserved. It does not let an
optimizer drop work the program requested, and it does not decide whether skipping
nonterminating work is allowed. That decision records a direction, not a finished
implementation. [SEMANTICS section 6](../../SEMANTICS.md) holds the conditional and
short-circuit rules.

## Common mistake

A beginner uses a number as a condition, as in C. This program is rejected when it runs:

```plato
library Choose { Pick(n: Integer): Integer => n ? 1 : 2; }
library Laws { Law_Pick(): Boolean => Pick(1) == 1; }
```

`plato run <folders> --laws` reports:

```text
error      Laws.Law_Pick  case 1:   a conditional's condition is not a Boolean: 1 (in Pick < Law_Pick)
```

Compare to produce a Boolean: `n.Equals(0) ? 2 : 1`.

`plato lint --strict` accepts the same program with no finding, so a lint-only run does
not catch it.
