# Typing, inference, and evaluation

The proofs in [Core.lean](Core.lean) connect an independent rule-based type system to an executable inference function, then connect those types to evaluation results. All statements apply only to [the bounded core](core-language.md).

## Reading typing rules

`⊢ e : T` means "expression `e` has type `T`." The empty space before `⊢` means there is no variable environment: all expressions are closed. A horizontal line separates premises above from a conclusion below. A rule with no premises is immediately available.

```text
                         ⊢ a : Integer     ⊢ b : Integer
-------------------      --------------------------------
⊢ integer(n) : Integer         ⊢ add(a, b) : Integer

                         ⊢ c : Boolean     ⊢ a : T     ⊢ b : T
-------------------      --------------------------------------
⊢ boolean(q) : Boolean              ⊢ cond(c, a, b) : T

-------------------
⊢ fault(T) : T
```

Both conditional branches must have the same type even though only one runs. Thus `cond(true, integer(1), boolean(false))` is rejected. A known guard does not excuse ill-typed dead code in the original expression.

A derivation is a tree built from these rules. For example:

```text
⊢ integer(1) : Integer     ⊢ integer(2) : Integer
------------------------------------------------
           ⊢ add(integer(1), integer(2)) : Integer

⊢ boolean(true) : Boolean       ⊢ add(integer(1), integer(2)) : Integer
                               ⊢ fault(Integer) : Integer
--------------------------------------------------------------------
⊢ cond(boolean(true), add(integer(1), integer(2)), fault(Integer)) : Integer
```

The last expression evaluates successfully to `3`; the faulting branch is not selected.

## Inference theorems

`infer(e)` returns `some T` when it finds a type and `none` when it rejects the expression. `some` and `none` are the constructors of Lean's `Option` type. The relation `HasType` is independently defined by inductive rules; it is not shorthand for inference success.

| Lean theorem | Statement and consequence |
|---|---|
| `infer_sound` | If `infer e = some T`, then `⊢ e : T`. The algorithm cannot accept an expression without a valid derivation. |
| `infer_complete` | If `⊢ e : T`, then `infer e = some T`. The algorithm cannot reject a derivable expression. |
| `infer_iff` | Combines both directions into an exact characterization. |
| `type_unique` | If `⊢ e : T` and `⊢ e : U`, then `T = U`. A core expression cannot have conflicting types. |

Completeness here means completeness for these syntax-directed rules. It is **not** a result about Plato's generic inference, nominal conversions, or overload search.

The soundness proof proceeds by induction on expression structure: assume the result for immediate subexpressions, inspect the inference branch, and assemble the corresponding typing constructor. [Lean's induction chapter](https://lean-lang.org/theorem_proving_in_lean4/induction_and_recursion.html) explains this kind of structural induction. Completeness instead proceeds by induction on a typing derivation.

## Evaluating safely

`e ⇓ r` means evaluation of `e` returns outcome `r`. In Lean, `Evaluates e r` is the graph `eval e = r` of the terminating recursive evaluator. This is an explicit design choice; evaluation is not an independently defined nondeterministic relation.

Addition evaluates its left operand first. A left fault is propagated immediately. If the left operand is an integer, the right operand is evaluated; integer values are added and a right fault is propagated. A wrong kind of value produces `typeError`. A conditional first evaluates its guard and then only its selected branch.

Define `Safe(T, r)` to mean that `r` is a value of type `T` or the `fault` outcome. It is false for `typeError`.

```text
⊢ e : T
------------------   type_safety
Safe(T, eval(e))
```

The theorem `typed_result` makes this concrete: a typed expression either returns a fault or returns a value whose type is exactly its static type. The proof rules out both runtime type mismatch and missing evaluation results. It does not rule out faults.

The theorem `deterministic` states that if `e ⇓ a` and `e ⇓ b`, then `a = b`. Because evaluation is defined as a total function, this is a direct consequence of equality. Its significance is the declared deterministic semantics, not evidence about an independently specified interpreter.

## What "type safety" does not prove here

There is no small-step relation in this model, so it does not claim separately checked one-step preservation or progress lemmas. `typed_result` is a terminal-result safety theorem for a total finite expression calculus. For loops and recursion, an eventual-result theorem is insufficient: the development needs a small-step relation or a suitable partial/coinductive evaluation semantics. [Software Foundations](https://softwarefoundations.cis.upenn.edu/plf-current/Types.html) proves progress and preservation separately for a small-step language, and both the statements and the proof structure differ from `typed_result`.
