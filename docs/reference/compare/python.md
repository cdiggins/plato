# If you know Python

Plato looks less like Python than like a small, statically typed, functional language.
The biggest shifts: every type is declared and checked before anything runs, values do
not change after they are built, and there are no imports, exceptions, or `None`.
Plato's call syntax, `x.F(y)`, reads like a Python method call, which helps.

How to read the tables: the left column is Python, the middle column is Plato, and the
last column says what differs. The Plato cell links the rule. Names in `PascalCase`
follow the library convention described in [syntax](../language/syntax.md).

## Values and types

| Python | Plato | Difference |
|---|---|---|
| `x = 3` | `var x = 3;` inside a function body | A `var` local has the type of its initializer. See [locals](../language/state-and-builders.md). |
| `3`, `3.0` (`int`, `float`) | `3` is `Integer`, `3.0` is `Number` | Literals are typed by their spelling. `Number` is the one floating type; its width comes from the compilation's profile. See [numbers](../language/numbers.md). |
| `True`, `False` | `true`, `false` | `Boolean` only. See [evaluation](../language/evaluation.md). |
| `"text"`, `'c'` | `"text"` is `String`, `'c'` is `Character` | `String` is minimal; the host language owns text handling. Functions are in [text](../api/foundation/text.library.md). |
| `f"{x}"` | none | String interpolation parses but is not part of the language. See [diagnostics](../language/diagnostics.md). |
| `def f(x: float) -> float:` | `F(x: Number): Number => ...;` | Parameter and result types are required. The compiler infers locals and lambda parameters. See [functions](../language/functions.md). |
| duck typing | nominal types | Two types with the same fields are different types. See [model](../language/model.md). |
| `type(x)`, `isinstance` | none | No runtime type tests and no reflection. See [SEMANTICS](../../SEMANTICS.md). |
| `x if c else y` | `if c then x else y` | An expression with one result; only the selected branch runs. See [expressions](../language/expressions.md). |
| `a and b`, `a or b` | `a && b`, `a \|\| b` | Short circuit as in Python, and operands are `Boolean`. See [evaluation](../language/evaluation.md). |

## Records

| Python | Plato | Difference |
|---|---|---|
| `@dataclass(frozen=True) class P` | `type P { X: Number; Y: Number; }` | A product type. Equality is structural and derived. See [products](../language/products.md) and [equality](../language/equality.md). |
| `P(1.0, 2.0)` | `P(1.0, 2.0)`, or a tuple `(1.0, 2.0)` where a `P` is expected | Fields are positional, in declaration order. See [construction](../language/products.md). |
| `p.x` | `p.X` | Field read. |
| `p.x = 3.0` | none | Values are immutable. Write a function that returns a new value, such as `P(p.X + 1.0, p.Y)`. See [products](../language/products.md). |
| `dataclasses.replace(p, x=1.0)` | a function you write | The derived "with" helpers belong to the generated C# and are not automatically callable from Plato source. See [products](../language/products.md). |
| tuples, `NamedTuple` | `(a, b)` | A tuple expression works with field counts through ten, and becomes a product through an expected type. See [products](../language/products.md). |
| private constructor plus factory | `type T sealed by Make { ... }` | Only the named functions can construct the type. See [sealed construction](../language/products.md). |

## Functions

| Python | Plato | Difference |
|---|---|---|
| `def square(x): return x * x` | `Square(x: Number): Number => x * x;` | Functions live in a `library`. A library name does not qualify a call. See [functions](../language/functions.md). |
| `x.method(y)` | `x.F(y)`, the same as `F(x, y)` | Not a method on a class: the first parameter is the receiver, and any function can be called this way. See [receivers](../language/functions.md). |
| `x.name` as a property | `x.Name` | A stored field and a computed member with no extra argument share the spelling. See [functions](../language/functions.md). |
| `lambda x: x * 2` | `x => x * 2` | Passed to a higher-order function; parameter types come from the target. See [lambdas](../language/functions.md). |
| `map(f, xs)` | `xs.Map(f)` | Eager: every element is computed once, in index order. See [Map](../api/foundation/primitives.library.md). |
| default and keyword arguments | none | Arguments are positional. Overloads are selected by parameter types. See [functions](../language/functions.md). |
| `*args`, `**kwargs` | none | Fixed arities; function types are `Function0` and up. See [generics](../language/generics.md). |
| closures see later changes to a variable | a lambda captures a snapshot at creation | Later rebinding does not change the captured binding. See [functions](../language/functions.md). |
| generators, `yield` | none | `yield` parses and is not in the language. See [diagnostics](../language/diagnostics.md). |

## Interfaces and generics

| Python | Plato | Difference |
|---|---|---|
| `Protocol` | `interface IBlended { Blend(a: Self, b: Self, t: Number): Self; }` | A set of operations a type promises. `Self` is the implementing type. See [interfaces](../language/interfaces.md). |
| a class that matches a protocol by shape | `type Signal implements IBlended { ... }` | The relationship is declared with `implements`, and the operations are functions in a library, not methods inside the type. See [interfaces](../language/interfaces.md). |
| `TypeVar`, `Generic[T]` | `type Pair<T> { ... }`, and `$T` in function signatures | Each call infers `$T` afresh. See [generics](../language/generics.md). |
| `TypeVar("T", bound=P)` | `where $T: IInterpolatable` | The bound licenses operations on `$T` in the body. See [bounds](../language/generics.md). |
| inheritance, abstract base classes | `inherits` between interfaces only | Types do not inherit from types and there is no virtual dispatch. See [SEMANTICS](../../SEMANTICS.md). |
| property tests by convention | `Law_` functions | A function named with the `Law_` prefix is run by the law runner. See [verification](../implementation/verification.md). |

## Sum types and matching

| Python | Plato | Difference |
|---|---|---|
| `Optional[float]`, `None` | a sum such as `type Option<T> = None \| Some(T);` | There is no null value. Absence is a case you declare. See [sums](../language/sums.md). |
| `enum.Enum` | `type Orientation = Clockwise \| CounterClockwise;` | A sum whose cases carry no payload. See [sums](../language/sums.md). |
| union of dataclasses | `type R = Valid(Value: Number) \| Missing;` | Cases may carry fields; a value is exactly one case. |
| `match x: case Valid(v): ...` | `match (x) { Valid(v) => v; Missing => 0.0; }` | An expression. Every case appears exactly once. See [matching](../language/sums.md). |
| `case _:` | none | No wildcard, default, guard, literal, or nested pattern. Adding a case makes every old `match` incomplete. See [sums](../language/sums.md). |
| `isinstance` tests | `match` | The only way to take a sum apart. |

## Loops and mutation

| Python | Plato | Difference |
|---|---|---|
| `total += x` | `total = total + x;` on a `var` local | `+=` and `++` are not supported. See [locals and loops](../language/state-and-builders.md). |
| `while cond:` | `while (cond) { ... }` | Supported. |
| `for i in range(n):` | `for (var i = 0; i < n; i = i + 1) { ... }` | The C-style `for` is supported. `foreach` parses but is not usable as a library feature. See [loop forms](../language/state-and-builders.md). |
| `break`, `continue` | none | Put the exit test in the condition, `return`, or use an array function. See [state and builders](../language/state-and-builders.md). |
| `xs = []`, `xs.append(v)` | `var xs = new List<Integer>(); xs = xs.Add(v);`, then `xs.Freeze()` | A builder: each mutator returns the builder and you rebind it; `Freeze()` yields an immutable array. See [builders](../language/state-and-builders.md). |
| change a list the caller passed in | none | Ordinary values are immutable; a function cannot change what its caller holds. See [model](../language/model.md). |

## Numbers

| Python | Plato | Difference |
|---|---|---|
| `int` has unlimited size | `Integer` has a fixed signed width | Set by the profile. See [numbers](../language/numbers.md). |
| `float` is binary64 | `Number` is binary64 or binary32 by profile | One floating type for the whole compilation. |
| `a / b` | `a / b` resolves `Divide` for the operand types | Integer division truncates. See [expressions](../language/expressions.md) and [numbers](../language/numbers.md). |
| `x == y` | `x == y` resolves `Equals` | For `Number`, NaN equals NaN and the two zeros are equal, unlike Python. See [equality](../language/equality.md). |
| `math.sqrt(x)` | `x.Sqrt` | A library function. See [numbers](../api/foundation/numbers.library.md). |

## Collections

| Python | Plato | Difference |
|---|---|---|
| `list`, `tuple` | `Array<T>`, written `[1, 2, 3]` | Immutable and materialized, not a lazy stream. See [expressions](../language/expressions.md) and [primitives](../api/foundation/primitives.library.md). |
| `xs[i]` | `xs[i]` | Means `At(xs, i)`. See [syntax](../language/syntax.md). |
| `len(xs)` | `xs.Count` | See [collections](../api/foundation/collections.concepts.md). |
| `[f(x) for x in xs]` | `xs.Map(x => F(x))` | No comprehension syntax. See [primitives](../api/foundation/primitives.library.md). |
| `functools.reduce(f, xs, a)` | `xs.Reduce(a, f)` | Folds left to right from the initial value. See [collections](../api/foundation/collections.library.md). |
| `zip(xs, ys)` | `xs.Zip(ys, (x, y) => ...)` | Takes the combining function directly. See [primitives](../api/foundation/primitives.library.md). |
| `all`, `any` | `xs.All(f)`, `xs.Any(f)` | See [primitives](../api/foundation/primitives.library.md). |
| `sorted(xs, key=...)` | `SortedIndices(xs, lessOrEqual)` | A library function. See [sorting](../api/foundation/sorting.library.md). |
| `dict`, `set` | the `IMap` and `ISet` interfaces | Interfaces, not built-in literals. See [collections](../api/foundation/collections.concepts.md). |

## Modules and libraries

| Python | Plato | Difference |
|---|---|---|
| `import math` | none | There are no imports. A program is a folder of `.plato` files, and every declaration shares one global scope. See [model](../language/model.md). |
| module namespaces | `library Name { ... }` | A library organizes functions; it does not scope names. Two libraries that declare the same name join one overload group. See [functions](../language/functions.md). |
| `if __name__ == "__main__":` | none | A program is a set of declarations; the host decides what runs. |
| definition order matters | declaration order does not | See [model](../language/model.md). |

## Errors

| Python | Plato | Difference |
|---|---|---|
| `raise`, `try`, `except` | none | There are no source-level exceptions. A computation can still fault, for example on a bad index, and the host reports it. See [evaluation](../language/evaluation.md). |
| `None` for "no result" | a sum case, or a `Boolean` kept beside the value | Partiality is handled by convention or by a generic sum. See [sums](../language/sums.md). |
| `assert` | `Law_` functions, sealed types | A sealed type means a value exists only through named functions; the compiler does not prove those functions check anything. See [products](../language/products.md). |
| `print` | none | There is no I/O in the language. See [model](../language/model.md). |

## What Plato adds

- **Exhaustive matching.** Add a case and the compiler points at every `match` that
  needs a decision. See [sums](../language/sums.md).
- **Capabilities by interface.** Generic code states what it needs with `where` bounds.
  See [generics](../language/generics.md).
- **Laws.** Properties are written as functions in the same language. See
  [verification](../implementation/verification.md).
- **Number profiles.** One program means one arithmetic per profile. See
  [numbers](../language/numbers.md).

Next: the [language index](../language/README.md) or the [tour](../start/tour.md).
