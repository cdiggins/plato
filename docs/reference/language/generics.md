# Generics, inference variables, and bounds

Generic code describes one algorithm over several types. A generic container can
store integers, points, or other containers without a second copy of the
declaration.

```plato
type Pair<T> { First: T; Second: T; }

library Pairs {
    Swap(p: Pair<$T>): Pair<$T> => (p.Second, p.First);
}
```

`T` is a parameter of `Pair<T>`. Its scope is that declaration. `$T` in `Swap` is
a signature variable. Each call infers it afresh. A bare `T` in an unrelated
library signature is not `Pair`'s parameter. Use `$T` there.

Inside a generic function being checked, that function's own signature variables
are **rigid**. Checking `Swap` must not silently decide that `$T` is always
`Number`. At a call site, the callee's variables are instantiated freshly and may
bind to the argument types. Sound inference depends on keeping those two roles
apart.

## Inference by equations

Let `α` denote a fresh inference variable, `σ` a substitution from variables to
types, and `σ(T)` the result of applying it. Calling `Swap` with a `Pair<Integer>`
produces the equation `Pair<α> = Pair<Integer>`, then `α = Integer`. The result
type is `Pair<Integer>`.

Nominal constructors have to agree. `Pair<Integer>` does not unify with
`Array<Integer>`. The number of type arguments has to agree. Binding
`α = Array<α>` fails an **occurs check**: a finite type cannot contain itself
through its own inference variable. That failure is not the same thing as a
separately declared recursive nominal type.

The solver combines these equations with overload choices, interface satisfaction,
and conversion constraints. That is more than textbook Hindley-Milner inference.
This reference does not claim principal types for the complete language. Overload
selection uses numeric matching costs and compatibility conversions.
[Haskell's type-and-class semantics](https://www.haskell.org/onlinereport/decls.html)
is a comparison for constrained variables. A proof about a pure equality-based
inference system does not transfer unchanged. The equations are solved in
[Solver unification](../../../src/Plato.Compiler/Checking/Solver.Unification.cs),
with the constraint representation in
[TypeConstraints](../../../src/Plato.Compiler/Checking/TypeConstraints.cs) and
[TypeConstraintChecker](../../../src/Plato.Compiler/Checking/TypeConstraintChecker.cs).

## Bounds on declarations

```plato
type Transition<T> where T: IInterpolatable {
    Start: T;
    End: T;
}
```

A bound requires every supplied argument to satisfy the interface. The compiler
rejects `Transition<String>` when `String` lacks `IInterpolatable`, including when
that construction sits inside a field or a nested type argument. A bound has to
name an interface, not a concrete product (CHK310). A violating argument is
CHK309.

A bound also licenses operations on the parameter in that generic context. A
library signature that uses `Transition<$T>` inherits the declaration's constraint
on `$T`. Satisfaction follows the transitive `implements` and `inherits` closure,
with type arguments substituted.

## Bounds on functions

```plato
Midpoint(pair: Pair<$T>): $T where $T: IInterpolatable
    => pair.First.Lerp(pair.Second, 0.5);
```

A function bound follows the return type and precedes the body. It licenses
operations in the body and filters overload candidates at the call. The inferred
argument type has to satisfy the bound. A failed call obligation is CHK206. An
operation absent from a known bound is CHK205. A bound that names a variable the
signature never mentions is a linter finding, because it constrains no input and
no output. The tests are in
[function-bound tests](../../../tests/PlatoTests/Checking/FunctionConstraintTests.cs).

The example uses a constructed receiver, `Pair<$T>`. A bare signature variable in
the first parameter, such as `F(x: $T)`, currently reaches an unhandled
receiver-classification exception before checking, even with a bound. Use a
concrete or constructed receiver, or an interface receiver, on the implemented
path. This is an implementation boundary. It needs a diagnostic and a specification
decision. It is not a theorem about bounded polymorphism.

An unbounded signature variable is not universally rejected for all capability
uses. The solver retains permissive behavior for unbounded parameters. Absence of
an error is not a proof of parametricity or of complete constraint inference. For
a reusable public algorithm, declare the capabilities actually required.

## Generic sums, functions, and specialization

Products, interfaces, and sums can be generic. A function value uses `Function0<TR>`
through `Function9<T0,...,TR>`. The last type argument is the result. A lambda
infers its parameter types from context. See [functions](functions.md).

The C# path specializes a typed function against a concrete reified signature and
redispatches an interface operation. Some emitted generic declarations, and some
generic structs, remain generic C#. Monomorphization does not promise that every
output file is free of generic syntax. The interpreter runs an unspecialized
generic body and uses the value's type at a dispatch point.
