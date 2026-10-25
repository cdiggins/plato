# Type checking, inference, and overload selection

The implemented checker works per bound function. [TypeChecker](../../../src/Plato.Compiler/Checking/TypeChecker.cs) normalizes the body, creates a shared fresh-variable factory, generates constraints, solves them, and checks sealed construction. Separate declaration-level passes validate sums, bounds, and existential field uses. The result records expression types, diagnostics, substitutions, and committed overload decisions.

## Constraint vocabulary

`Γ` maps term bindings to types. A type is a nominal constructor applied to arguments, a rigid declaration/signature parameter, `Self`, or a fresh flexible inference variable. `σ` maps flexible variables to types. Applying `σ` recursively until terminal types are reached is called **zonking** in the implementation.

- Equality `A ~ B` requires unification.
- Overload constraint `f(A₀,...,Aₙ₋₁) -> R` asks one function group to determine a call.
- Return coercion `A ~> B` admits equality or a supported implicit conversion.

A conditional checks its condition against Boolean and equates both branches with one fresh result type. An array equates element types with one element variable. A local initialized by `var` retains the initializer's inferred type. A return position produces a coercion constraint rather than an unconditional equality: a returned tuple may become the declared record.

## Unification algorithm

Resolve existing substitution chains first. Identical variables succeed. A flexible variable binds to the other type after an occurs check. Two nominal applications must have the same declaration name and arity, then recursively unify arguments. A rigid signature variable must not absorb a concrete type during trial matching; a flexible variable can bind toward a rigid one. `Self` has deliberately permissive compatibility before specialization.

```text
unify(C<A₀,...,Aₙ₋₁>, C<B₀,...,Bₙ₋₁>)
    = unify(A₀,B₀); ...; unify(Aₙ₋₁,Bₙ₋₁)

bind(α,T) is allowed only when α is not free in T
```

Here `C` is a nominal constructor, `α` a flexible variable, and “free in” means occurring through the type's arguments after chasing known bindings. Source evidence: [Solver.Unification](../../../src/Plato.Compiler/Checking/Solver.Unification.cs).

Implementation caveat: the solver uses recursion-depth guards and accepts some null/depth-limit cases permissively. Its code is not an unbounded, formally verified decision procedure. A proof of first-order unification cannot be asserted to cover those escape paths without additional preconditions.

## Joint solving and deferred commitment

Solve unconditional equalities first. Then repeatedly attempt overloads and return coercions until no progress occurs. An inner call can determine an outer argument type; an expected product can ground a tuple's element types. Avoid committing too early. Remaining calls are forced in sequence, revisiting the pending calls as new bindings become available; unresolved coercions receive a final forced attempt.

Each candidate is instantiated with fresh variables on a scratch substitution, so a losing candidate cannot poison the global substitution. Interface parameters receive fresh constrained variables; repeated occurrences of the same interface share the concrete `Self` binding. Declared function bounds are checked against the candidate's inferred arguments before it is viable.

## Ranking: the actual implementation rule

The solver sums costs over arguments:

| Match | Cost | Meaning |
|---|---|---|
| Exact | 0 | Nominal types unify without a fresh parameter binding at that position |
| Generic | 2 | Fresh generic parameter binds |
| Interface | 3 | Argument satisfies the parameter's capability |
| Conversion | 5 | Supported implicit conversion is needed |

These values are algorithm parameters in [Solver](../../../src/Plato.Compiler/Checking/Solver.cs), not benchmark measurements. They matter because total costs are compared: a conversion in one position can tie several generic/interface matches elsewhere. Do not replace the sum with a lexicographic “one exact match always wins” rule.

After retaining minimum-cost candidates, the solver applies a specificity tie-break: a candidate wins over another if each compared parameter is at least as specific by interface closure and some is strictly more specific. Interface parameter variables contribute their associated interface when comparing specificity.

If one candidate remains, commit its bindings, parameter/result types, argument match kinds, and chosen conversions. If several remain with a common return type, the solver accepts the first and reports informational CHK202. It also accepts an all-variable-return tie. If differing results remain but arguments are still polymorphic, CHK204 defers selection to specialization. A ground differing-return tie is CHK203. No viable candidate is CHK201, with more specific diagnostics for failed bound obligations.

Common-return acceptance does not itself prove behavioral coherence: two equal-signature bodies can return different values of the same type. A reimplementation needs the same deterministic candidate ordering to reproduce this fallback, or an explicit revised ambiguity rule. The normative summary omits the specificity and polymorphic details; inspect the source and [discrepancies](discrepancies.md).

## Coercions and capabilities

Return coercion tries identity, a declared conversion relation, tuple-to-product compatibility with per-field coercions, and implementation of an interface. A bare scalar does **not** automatically wrap into any one-field record; wrapping uses its constructor. A one-field record may unwrap according to the declared/derived conversion policy. This asymmetry prevents accidentally erasing semantic quantity distinctions.

Interface satisfaction walks transitive `implements`/`inherits` links, substituting type arguments at each edge. A parameter already bound to a concrete `Self` cannot then accept a different type merely because both implement the interface. Known bounds license operations; function bounds also filter call-site candidates. See [TypeConstraints](../../../src/Plato.Compiler/Checking/TypeConstraints.cs) and [Solver.Coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs).

## Proof obligations and related work

Useful obligations include substitution consistency, occurs-check soundness on finite types, trial isolation, unique/coherent overload selection, explicit conversion preservation, and specialization preservation. [Formal models](../formal/README.md) can prove bounded versions; they must state which overload/interface/fallback behaviors are included. [Haskell's type-and-class semantics](https://www.haskell.org/onlinereport/decls.html) provides a comparison for constrained inference. Plato's weighted overloads and accepted ties prevent casually claiming the full principal-type/coherence results of a simpler calculus.
