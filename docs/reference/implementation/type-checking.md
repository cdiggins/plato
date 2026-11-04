# Type checking, inference, and overload selection

The checker works per bound function. It returns expression types, diagnostics, substitutions, and the overload each call committed to. [TypeChecker](../../../src/Plato.Compiler/Checking/TypeChecker.cs) normalizes the body, creates a shared fresh-variable factory, generates constraints, solves them, and checks sealed construction. Separate declaration-level passes validate sums, bounds, and existential field uses.

A losing candidate must not write into the substitution the rest of the function uses. The checker must not commit a call while a later constraint can still determine an argument type.

## Constraint vocabulary

`Γ` maps term bindings to types. A type is a nominal constructor applied to arguments, a rigid declaration or signature parameter, `Self`, or a fresh flexible inference variable. `σ` maps flexible variables to types. Applying `σ` recursively until the types are terminal is called zonking in the implementation.

- Equality `A ~ B` requires unification.
- Overload constraint `f(A₀,...,Aₙ₋₁) -> R` asks one function group to determine a call.
- Return coercion `A ~> B` admits equality or a supported implicit conversion.

A conditional checks its condition against Boolean and equates both branches with one fresh result type. An array equates element types with one element variable. A local initialized by `var` retains the initializer's inferred type. A return position produces a coercion constraint, so a returned tuple may become the declared record. The return is not an unconditional equality.

## Unification algorithm

Resolve existing substitution chains first. Identical variables succeed. A flexible variable binds to the other type after an occurs check. Two nominal applications must have the same declaration name and arity, and then their arguments unify. A rigid signature variable must not absorb a concrete type during trial matching. A flexible variable can bind toward a rigid one. `Self` has deliberately permissive compatibility before specialization.

```text
unify(C<A₀,...,Aₙ₋₁>, C<B₀,...,Bₙ₋₁>)
    = unify(A₀,B₀); ...; unify(Aₙ₋₁,Bₙ₋₁)

bind(α,T) is allowed only when α is not free in T
```

Here `C` is a nominal constructor, `α` a flexible variable, and "free in" means occurring through the type's arguments after chasing known bindings. Source evidence: [Solver.Unification](../../../src/Plato.Compiler/Checking/Solver.Unification.cs).

The solver uses recursion-depth guards and accepts some null and depth-limit cases. Those paths are permissive. The code is not an unbounded, formally verified decision procedure. A proof of first-order unification does not cover the escape paths unless it adds preconditions for them.

## Joint solving and deferred commitment

Solve unconditional equalities first. Then attempt overloads and return coercions again until a pass makes no progress. An inner call can determine an outer argument type. An expected product can ground a tuple's element types. Avoid committing too early. Remaining calls are forced in sequence, and the solver revisits the pending calls as new bindings become available. Unresolved coercions receive a final forced attempt.

Each candidate is instantiated with fresh variables on a scratch substitution, so a losing candidate cannot poison the global substitution. Interface parameters receive fresh constrained variables. Repeated occurrences of the same interface share the concrete `Self` binding. Declared function bounds are checked against the candidate's inferred arguments before it is viable.

## Ranking: the actual implementation rule

The solver sums costs over arguments:

| Match | Cost | Meaning |
|---|---|---|
| Exact | 0 | Nominal types unify without a fresh parameter binding at that position |
| Generic | 2 | Fresh generic parameter binds |
| Interface | 3 | Argument satisfies the parameter's capability |
| Conversion | 5 | Supported implicit conversion is needed |

These values are algorithm parameters in [Solver](../../../src/Plato.Compiler/Checking/Solver.cs). They are not benchmark measurements. The totals are what get compared: a conversion in one position can tie several generic or interface matches elsewhere. Do not replace the sum with a lexicographic "one exact match always wins" rule.

After retaining minimum-cost candidates, the solver applies a specificity tie-break. A candidate wins over another if each compared parameter is at least as specific by interface closure and some parameter is strictly more specific. Interface parameter variables contribute their associated interface when comparing specificity.

If one candidate remains, commit its bindings, parameter and result types, argument match kinds, and chosen conversions. If several remain with a common return type, the solver accepts the first and reports informational CHK202. It also accepts an all-variable-return tie. If differing results remain but arguments are still polymorphic, CHK204 defers selection to specialization. A ground differing-return tie is CHK203. No viable candidate is CHK201, with more specific diagnostics for failed bound obligations.

Common-return acceptance does not prove that the bodies agree. Two functions with the same signature can return different values of that type. A reimplementation needs the same deterministic candidate ordering to reproduce this fallback, or an explicit revised ambiguity rule. The normative summary omits the specificity and polymorphic details. Inspect the source and [discrepancies](discrepancies.md).

## Coercions and capabilities

Return coercion tries identity, a declared conversion relation, tuple-to-product compatibility with a coercion on each field, and implementation of an interface. A bare scalar does not automatically wrap into any one-field record. Wrapping uses the constructor. A one-field record may unwrap according to the declared or derived conversion policy. The two directions differ so a semantic quantity is not erased by accident.

Interface satisfaction walks transitive `implements` and `inherits` links and substitutes type arguments at each edge. A parameter already bound to a concrete `Self` cannot then accept a different type merely because both types implement the interface. Known bounds license operations. Function bounds also filter call-site candidates. See [TypeConstraints](../../../src/Plato.Compiler/Checking/TypeConstraints.cs) and [Solver.Coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs).

## Proof obligations

Useful obligations include substitution consistency, occurs-check soundness on finite types, trial isolation, unique or coherent overload selection, explicit conversion preservation, and specialization preservation. [Formal models](../formal/README.md) can prove bounded versions. A proof must state which overload, interface, and fallback behaviors it includes. [Haskell's type-and-class semantics](https://www.haskell.org/onlinereport/decls.html) is a comparison for constrained inference. Plato's weighted overloads and accepted ties block a casual claim to the principal-type and coherence results of a simpler calculus.
