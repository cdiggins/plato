# If you know C#

Plato's punctuation is close to C#: braces, semicolons, `=>` bodies, generics in angle
brackets, and `a.F(b)` calls. The language underneath is different. Values are
immutable, functions are pure, there are no classes, no null, and no exceptions. The
parser accepts more C#-shaped forms than the language supports, so a form that parses
is not necessarily Plato. See [diagnostics](../language/diagnostics.md).

How to read the tables: the left column is C#, the middle column is Plato, and the
last column says what differs. The Plato cell links the rule.

## Values and types

| C# | Plato | Difference |
|---|---|---|
| `int`, `long` | `Integer` | One signed integer type; its width comes from the compilation's profile. See [numbers](../language/numbers.md). |
| `double`, `float` | `Number` | One floating type. The profile picks binary64 or binary32 for the whole compilation. See [numbers](../language/numbers.md). |
| `bool`, `char`, `string` | `Boolean`, `Character`, `String` | `String` is minimal. See [text](../api/foundation/text.library.md). |
| `var x = 3;` | `var x = 3;` | Same spelling, inside a function body. See [locals](../language/state-and-builders.md). |
| `int x`, prefix types | `x: Integer`, postfix | Parameter and result types are required; locals and lambda parameters are inferred. See [syntax](../language/syntax.md). |
| `c ? a : b` | `if c then a else b` | The conditional is an expression; only the selected branch runs. See [expressions](../language/expressions.md) and [evaluation](../language/evaluation.md). |
| `(int)x` casts, `is`, `as` | none | No casts and no runtime type tests. Conversions follow the typing rules. See [expressions](../language/expressions.md). |
| `null`, `?.` | none | There is no null value. See [model](../language/model.md). |
| structural typing absent; types are nominal | types are nominal | Same idea. One exception: a tuple converts to an expected product. See [products](../language/products.md). |
| `default(T)` | `default` | Takes its type from context. It is a representation value, not evidence of a valid domain value. See [products](../language/products.md). |

## Records

| C# | Plato | Difference |
|---|---|---|
| `readonly record struct P(double X, double Y)` | `type P { X: Number; Y: Number; }` | A product type. The compiler derives the constructor, field access, and structural equality. See [products](../language/products.md) and [equality](../language/equality.md). |
| `new P(1, 2)` | `P(1.0, 2.0)`, or a tuple `(1.0, 2.0)` where a `P` is expected | Positional, in declaration order. Swapping two fields of one type still type-checks. See [construction](../language/products.md). |
| `p with { X = 3 }` | a function you write | The generated C# has `With...` helpers; they are not automatically callable from Plato source. See [products](../language/products.md). |
| settable property, mutable field | none | A `type` is immutable. "Setters" return new values. See [SEMANTICS](../../SEMANTICS.md). |
| private constructor plus factory | `type T sealed by Make { ... }` | The compiler allows construction only in the named functions. The generated C# constructor stays public, so this is not a security boundary. See [sealed construction](../language/products.md). |
| `Equals`, `GetHashCode` overrides | derived | Equality and hashing are structural for a nominal type. `Number` equality treats NaN as equal to NaN. See [equality](../language/equality.md). |

## Functions

| C# | Plato | Difference |
|---|---|---|
| `static double Square(double x) => x * x;` | `Square(x: Number): Number => x * x;` | Declared in a `library`. See [functions](../language/functions.md). |
| extension method `x.F(y)` | `x.F(y)`, the same as `F(x, y)` | Receiver-first calls are a core language rule, not a lookup category. See [receivers](../language/functions.md). |
| property `x.Length` | `x.Length` | A stored field and a computed member with no extra argument share one spelling; in generated C# a computed member is a method. See [functions](../language/functions.md). |
| `static` classes group functions | `library Name { ... }` | A library name does not qualify a call. Every declaration shares one global scope and one overload group per name. See [model](../language/model.md). |
| overloads, no named or default arguments | overloads by parameter types | Resolution uses parameter types, generic bindings, capabilities, and conversions. See [functions](../language/functions.md). |
| `Func<A, R>` | `Function1<A, R>` | `Function0` and up; the last type argument is the result. See [generics](../language/generics.md). |
| `x => x * 2` | `x => x * 2` | Parameter types come from the target signature. A lambda captures a snapshot of outer values. See [lambdas](../language/functions.md). |
| `ref`, `out`, `in` | none | Parameters are values. See [state and builders](../language/state-and-builders.md). |
| `async`, `yield`, iterators | none | `yield` parses and is not in the language. See [diagnostics](../language/diagnostics.md). |
| local functions, delegates stored in fields | not specified | Lambdas are passed to higher-order functions. Unrestricted function storage is not a specified feature. See [functions](../language/functions.md). |

## Interfaces and generics

| C# | Plato | Difference |
|---|---|---|
| `interface IBlended<T> { T Blend(T a, T b); }` | `interface IBlended { Blend(a: Self, b: Self, t: Number): Self; }` | Operations are functions over `Self`, the implementing type; they are not instance methods. See [interfaces](../language/interfaces.md). |
| `class C : IBlended` | `type C implements IBlended { ... }` | A declared capability. The operations are supplied by library functions or intrinsics. See [interfaces](../language/interfaces.md). |
| `static abstract` interface members | `Self` functions | Closest in spirit. Plato's model is nearer to type classes than to object inheritance. See [interfaces](../language/interfaces.md). |
| interface inheritance | `interface ISampled<T> inherits IIndexable<T>` | The parent's obligations are added, with type arguments substituted. See [interfaces](../language/interfaces.md). |
| a field of interface type holding any implementer | allowed for eligible interfaces | A stored interface value is checked for an object-safe view. Its full semantics are not yet specified. See [stored interface values](../language/interfaces.md). |
| `class D : Base`, `virtual`, `override` | none | No inheritance between types and no virtual dispatch. See [SEMANTICS](../../SEMANTICS.md). |
| `public`, `private`, `internal` | none | No visibility modifiers. `sealed by` limits construction. See [products](../language/products.md). |
| `List<T>` generic code | `type Pair<T> { ... }`, `$T` in function signatures | Each call infers `$T` afresh; a bare `T` is the declaration's own parameter. See [generics](../language/generics.md). |
| `where T : IComparable<T>` | `where $T: IInterpolatable` | Bounds name interfaces; the bound licenses operations in the body. See [bounds](../language/generics.md). |
| runtime reflection, `typeof` | none | See [SEMANTICS](../../SEMANTICS.md). |
| `Debug.Assert`, unit tests | `Law_` functions | A `Boolean` function named with the `Law_` prefix is run by the law runner. See [verification](../implementation/verification.md). |

Plato code is specialized per concrete type when it compiles, so generic and interface
code costs no run-time dispatch. See [model](../language/model.md).

## Sum types and matching

| C# | Plato | Difference |
|---|---|---|
| `enum` | `type Orientation = Clockwise \| CounterClockwise;` | A sum whose cases have no payload. See [sums](../language/sums.md). |
| class hierarchy or `OneOf<>` | `type R = Valid(Value: Number) \| Rejected(Reason: String) \| Missing;` | Cases carry ordered fields; a value has one active case. A sum is closed. See [sums](../language/sums.md). |
| `Nullable<T>`, `T?` | a sum such as `type Option<T> = None \| Some(T);` | Absence is a declared case. Generic sums are supported. See [sums](../language/sums.md). |
| `x switch { A a => ..., _ => ... }` | `match (x) { Valid(v) => v; Missing => 0.0; }` | Every case exactly once. No wildcard, guard, literal, or nested pattern. See [matching](../language/sums.md). |
| `switch` statement | none | `switch` parses and is not elaborated. Use `match`. See [diagnostics](../language/diagnostics.md). |
| `is` pattern tests | `match` | The only way to take a sum apart. |

## Loops and mutation

| C# | Plato | Difference |
|---|---|---|
| `total += x;`, `i++` | `total = total + x;`, `i = i + 1;` | Compound assignment and `++` are not supported. See [locals and loops](../language/state-and-builders.md). |
| `while`, `do ... while`, C-style `for` | the same | Supported; `do` and `for` are rewritten to the `while` form. See [loop forms](../language/state-and-builders.md). |
| `foreach` | none usable | It parses, but is not a library-authoring feature. Use `Map`, `Reduce`, and similar. See [loop forms](../language/state-and-builders.md). |
| `break`, `continue` | none | Put the exit test in the condition, `return`, or use an array function. See [state and builders](../language/state-and-builders.md). |
| `List<T>`, `Add`, then use | `var xs = new List<Integer>(); xs = xs.Add(v);` then `xs.Freeze()` | A unique builder: a mutator returns the builder and you rebind it, and `Freeze()` produces an immutable array. The discipline is lint- and runtime-checked, not statically proven. See [builders](../language/state-and-builders.md). |
| `T[] a = new T[n]; a[i] = v;` | `Buffer<T>` with `Set` | Fixed slots for indexed writes during construction. See [builders](../language/state-and-builders.md). |
| shared mutable objects | none | A function cannot change a value its caller holds. See [model](../language/model.md). |

## Numbers

| C# | Plato | Difference |
|---|---|---|
| `float` and `double` both | `Number` only | The width is chosen once for the compilation, not per declaration. See [numbers](../language/numbers.md). |
| `int` overflow wraps or throws by context | `Integer` has a profile width | Under a 32-bit profile it wraps at 32 bits. See [numbers](../language/numbers.md). |
| `double.Epsilon` | read the declared operation | The C# constant is the smallest subnormal; a Plato name such as `Epsilon` needs the declaration read. See [numbers](../language/numbers.md). |
| `a == b` on `double` | `a == b` resolves `Equals` | NaN equals NaN under Plato's `Number` equality, unlike native C# `==`. Ordering comparisons stay ordinary. See [equality](../language/equality.md). |
| `Math.Sqrt(x)` | `x.Sqrt` | Uses the host's math functions. See [numbers](../api/foundation/numbers.library.md). |
| operator overloads | `+` resolves `Add`, `==` resolves `Equals`, and so on | An operator is syntax for a named function. See [expressions](../language/expressions.md). |

## Collections

| C# | Plato | Difference |
|---|---|---|
| `T[]`, `IReadOnlyList<T>` | `Array<T>`, written `[1, 2, 3]` | Immutable and materialized, not a lazy sequence. See [expressions](../language/expressions.md) and [primitives](../api/foundation/primitives.library.md). |
| `xs[i]`, `xs.Length` | `xs[i]`, `xs.Count` | `xs[i]` means `At(xs, i)`. See [syntax](../language/syntax.md) and [collections](../api/foundation/collections.concepts.md). |
| LINQ `Select` | `xs.Map(f)` | Eager, in index order. See [primitives](../api/foundation/primitives.library.md). |
| LINQ `Aggregate` | `xs.Reduce(acc, f)` | Left to right from the initial value. See [collections](../api/foundation/collections.library.md). |
| LINQ `Zip`, `All`, `Any` | `xs.Zip(ys, f)`, `xs.All(f)`, `xs.Any(f)` | See [primitives](../api/foundation/primitives.library.md). |
| `OrderBy` | `SortedIndices(xs, lessOrEqual)` | Stable, and returns indices. See [sorting](../api/foundation/sorting.library.md). |
| `Dictionary`, `HashSet` | the `IMap` and `ISet` interfaces | Interfaces, not built-in literals. See [collections](../api/foundation/collections.concepts.md). |
| deferred execution | none | An array is a value, not a lazy stream. See [evaluation](../language/evaluation.md). |

## Modules and libraries

| C# | Plato | Difference |
|---|---|---|
| `namespace`, `using` | none | No modules, imports, or namespaces. Every declaration in the compilation shares one global scope. See [model](../language/model.md). |
| assembly and project references | the input folders | Include every dependency in the input roots; the compiler does not import a prelude implicitly. See [model](../language/model.md). |
| declaration order | does not matter | See [model](../language/model.md). |
| `extern` methods | a function with no body | A bodiless signature asks the runtime to supply the operation. See [functions](../language/functions.md). |

## Errors

| C# | Plato | Difference |
|---|---|---|
| `throw`, `try`, `catch` | none | They parse and are not in the language. A computation can still fault or fail to terminate; the host reports it. See [evaluation](../language/evaluation.md). |
| `TryParse` and `out` | a result type such as `NumberParse` | Partiality is a value you match on. See [text](../api/foundation/text.types.md). |
| `ArgumentException` for bad input | `sealed by` construction, or a `Boolean` kept beside the value | Convention or sealed construction, not an exception. See [products](../language/products.md). |
| `Console`, files | none | No I/O in the language; the host owns it. See [model](../language/model.md). |

## How Plato code appears from C#

Plato compiles to C#, and you call the result from ordinary C# code. The mapping is
summarized here and owned by [Mapping Plato to C#](../implementation/csharp.md).

- A product becomes a readonly partial struct; a field read stays a field read, and a
  computed member such as `x.Square` becomes a method call in C#.
- A sum becomes a tagged struct with static factories per case.
- Functions in a library become extension methods in a static class for that library.
- `Number` and `Integer` are runtime wrapper types selected by the profile.

Do not read the generated C# back into Plato semantics: Plato's rules come from the
[language reference](../language/README.md), and the C# is the host representation.
For example, a public generated constructor does not make a `sealed by` type
constructible in Plato source.

## What Plato adds

- **Exhaustive matching** over closed sums. See [sums](../language/sums.md).
- **Interfaces over `Self`**, with the capability declared on the type and checked at
  each generic call. See [interfaces](../language/interfaces.md).
- **Laws** written as functions in the language. See
  [verification](../implementation/verification.md).
- **One arithmetic per program**, by number profile. See [numbers](../language/numbers.md).

Next: the [language index](../language/README.md) or the [tour](../start/tour.md).
