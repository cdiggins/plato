# Rewriting while preserving behavior

A rewrite replaces an expression by another expression. For [this core](core-language.md), the relevant observable behavior is its evaluation outcome: an integer value, a Boolean value, a fault, or a type error. Write `a ≈ b` to mean `eval(a) = eval(b)`. This is whole-expression outcome equality; it does not claim equality of execution cost or traces.

## Constant conditionals

The checked theorems `constant_cond_true` and `constant_cond_false` establish:

```text
cond(true,  a, b) ≈ a
cond(false, a, b) ≈ b
```

Neither needs an assumption that the discarded branch succeeds. The evaluator never selects that branch. This is why a constant conditional can safely discard a faulting branch.

The recursive function `fold` applies that same idea throughout an expression tree. It recursively folds children, replaces a conditional whose folded guard is a Boolean literal with its selected folded branch, and otherwise retains the conditional. It leaves additions in place. The theorem `fold_correct` proves:

```text
for every expression e: eval(fold(e)) = eval(e)
```

This equality covers even untyped trees and all modeled faults. The proof is by structural induction on expressions, with case analysis on the folded guard. In particular, a transformed guard that returns an integer or fault is retained as a conditional rather than accidentally treated as Boolean truthiness.

This is an outcome-preservation theorem. It does not say that folding preserves rejection of ill-typed input. For example, a conditional with a constant true guard, an integer first branch, and a Boolean second branch is rejected before folding; folding can leave just the integer literal. A compiler should type-check source programs according to its source rules before relying on such transformations. The actual Plato optimization passes have their own contracts and are not invoked by this proof.

## Short-circuit conjunction

Define a source-like abbreviation, not a new expression constructor:

```text
andThen(a, b) = cond(a, b, false)
```

`and_typed` proves that Boolean operands give a Boolean expression. `false_and` and `true_and` prove:

```text
andThen(false, b) ⇓ value(false)
eval(andThen(true, b)) = eval(b)
```

The first holds for every expression `b`, including a modeled fault. The second evaluates `b` only after the true guard is known. This matches the shape of [Plato's Boolean short-circuit lowering](../../../src/Plato.Compiler/Checking/TirShortCircuit.cs); proving that the actual elaborator always applies the same transformation in the appropriate cases remains a separate obligation.

## A checked counterexample to eager replacement

`strictAnd` is a different evaluator that always evaluates the second Boolean operand after obtaining a Boolean first operand. Lean proves `eager_and_counterexample`:

```text
strictAnd(false, fault(Boolean)) = fault
andThen(false, fault(Boolean))   ⇓ value(false)
```

Therefore eager replacement changes observable behavior. No mutable state is involved. Purity by itself is insufficient to justify evaluating an expression that short-circuit semantics would skip.

The counterexample's Lean proof is executable and short:

```lean
theorem eager_and_counterexample :
    strictAnd (.boolean false) (.fault .boolean) ≠
      eval (andThen (.boolean false) (.fault .boolean)) := by decide
```

`by decide` computes the decidable equality question and constructs a proof that the outcomes differ. It does not assume the inequality.

## Related work

[Software Foundations: Equivalence](https://softwarefoundations.cis.upenn.edu/plf-current/Equiv.html) develops program equivalence and verified transformations. Our theorem uses equality of outcomes for a finite expression language, rather than equivalence of state-transforming commands. The [C# expression specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) separately specifies conditional evaluation and conditional logical operators; preserving source behavior requires matching those rules during emission, not merely matching Boolean truth tables.
