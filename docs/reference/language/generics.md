# Generics, inference variables, and bounds

Generic code describes the same algorithm over several types. A generic container can store integers, points, or other containers without copying its declaration.

```plato
type Pair<T> { First: T; Second: T; }

library Pairs {
    Swap(p: Pair<$T>): Pair<$T> => (p.Second, p.First);
}
```

`T` is a parameter declared by `Pair<T>`. Its scope is that declaration. `$T` in `Swap` is a signature variable inferred freshly at each call. A bare `T` in an unrelated library signature is not the declaration's parameter. Use `$T` there.

Inside a generic function being checked, its own signature variables are **rigid**: checking `Swap` must not silently decide that `$T` is always `Number`. At a call site the callee's variables are instantiated freshly and may bind to actual argument types. Distinguishing these two roles is essential for sound inference.

## Inference by equations

Let `α` denote a fresh inference variable, `σ` a substitution mapping variables to types, and `σ(T)` the result of applying it. Calling `Swap` with a `Pair<Integer>` value produces the equation `Pair<α> = Pair<Integer>`, then `α = Integer`. The result is `Pair<Integer>`.

Nominal constructors must agree: `Pair<Integer>` cannot unify with `Array<Integer>`. Type argument counts must agree. Binding `α = Array<α>` fails an **occurs check**, because a finite type cannot contain itself through its own inference variable. This is not the same as a separately declared recursive nominal type.

The actual solver combines these equations with overload choices, interface satisfaction, and conversion constraints. It is therefore more than textbook Hindley–Milner inference; this reference does not claim principal types for the complete language.

## Bounds on declarations

```plato
type Transition<T> where T: IInterpolatable {
    Start: T;
    End: T;
}
```

A bound means every supplied argument must satisfy the interface. `Transition<String>` is rejected if `String` lacks it, including when the invalid construction occurs inside a field or nested type argument. A bound must name an interface, not a concrete product (CHK310); a violating argument is CHK309.

Bounds also license operations on a parameter within the relevant generic context. A library signature using `Transition<$T>` inherits the declaration's constraint on `$T`. Satisfaction follows the transitive `implements`/`inherits` closure with argument substitution.

## Bounds on functions

```plato
Midpoint(pair: Pair<$T>): $T where $T: IInterpolatable
    => pair.First.Lerp(pair.Second, 0.5);
```

Function bounds follow the return type and precede the body. They license body operations and filter overload candidates at each call: the inferred argument type must satisfy the bound. A failed call obligation is CHK206. An operation absent from a known bound is CHK205. A bound naming a variable not mentioned in the signature is a linter finding because it constrains no useful input or output.

The example uses a constructed receiver `Pair<$T>`. A bare signature variable as the first
parameter, such as `F(x: $T)`, currently reaches an unhandled receiver-classification exception
before checking, even with a bound. Use a concrete/constructed receiver or an interface receiver
for the implemented path. This restriction is an implementation boundary requiring a diagnostic
and specification decision; it is not a theorem about bounded polymorphism.

An important implementation caveat: an unbounded signature variable is not universally rejected for all capability uses. The solver retains permissive behavior for unbounded parameters. This means absence of an error is not a proof of parametricity or complete constraint inference. For reusable public algorithms, declare the capabilities actually required.

## Generic sums, functions, and specialization

Products, interfaces, and sums can be generic. Function values use `Function0<TR>` through `Function9<T0,...,TR>`, where the last type argument is the result. Lambdas infer their parameter types from context; see [functions](functions.md).

The C# path specializes typed functions against concrete reified signatures and redispatches interface operations. Some emitted generic declaration surfaces and generic structs remain generic C#. “Monomorphization” is not a promise that every output file contains no generic syntax. The interpreter executes unspecialized generic bodies using value types at dispatch points.

## Evidence and related work

Read [Solver unification](../../../src/Plato.Compiler/Checking/Solver.Unification.cs), [TypeConstraints](../../../src/Plato.Compiler/Checking/TypeConstraints.cs), [TypeConstraintChecker](../../../src/Plato.Compiler/Checking/TypeConstraintChecker.cs), and [function-bound tests](../../../tests/PlatoTests/Checking/FunctionConstraintTests.cs). [Haskell's type-and-class semantics](https://www.haskell.org/onlinereport/decls.html) is a useful comparison for constrained variables. Unlike a pure equality-based inference system, Plato overload selection uses numeric matching costs and compatibility conversions; proofs for that smaller system cannot be transferred unchanged.
