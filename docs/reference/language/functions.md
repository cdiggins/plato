# Functions, receivers, and lambdas

A function has ordered parameters, a result type, and a computation. Parameters and
the result type are explicit. The compiler normally infers locals and lambda
parameters. An expression body returns its expression:

```plato
Square(x: Number): Number => x * x;
```

Functions live in a `library` or an `interface`. A block body supports local steps
and an explicit `return`. See [locals](state-and-builders.md). A declaration that ends
immediately in `;` has no body. It asks the runtime to supply the operation. The
library's intrinsic contract says where such a declaration may appear and which
primitive types it may mention.

## One call, several spellings

The first parameter is the receiver. These two spellings are the same call:

```plato
Lerp(a, b, t)
a.Lerp(b, t)
```

`Square(x)` and `x.Square` are source-level uniform access. A stored field and a
computed member with no extra argument share that spelling. The C# writer emits a
method for an ordinary computed member, so the second spelling is `x.Square()` in
host code. A stored product field stays a field. A handwritten intrinsic member can
remain a property. Do not copy one C# spelling mechanically onto every source access.
[C# extension methods](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#extension-methods)
use a similar receiver spelling. In Plato, a receiver-first call is a uniform
source-language rule. It is not a C#-specific method-lookup category.

A first parameter named `_` marks a type-based operation:

```plato
Origin(_: Point2D): Point2D => (0.0, 0.0);
```

Call it as `Point2D.Origin`. The receiver's type matters. Its value is unused. A
type name in that position is a type token, not a default instance whose fields can
be read.

Every library contributes to one global function group per name. Overload resolution
uses parameter types, generic bindings, capabilities, and conversions. Library names
do not separate otherwise identical functions. See
[resolution](../implementation/type-checking.md).

## Higher-order functions

```plato
xs.Map(x => x * x)
```

`Map` takes a function. The target signature determines the type of `x` and the
element type of the result. For two parameters, write `(a, b) => expression`.
The function type `Function2<A, B, R>` has arguments `A` and `B` and result `R`.
`Function0<R>` has no arguments.

The grammar allows an expression lambda and a block lambda. The checked idiom passes
a lambda to a higher-order function and infers the parameter types from the target
signature. The implementation also has `TirLambda` and `TirInvoke` nodes. Having those nodes
does not establish arbitrary unrestricted first-class function storage, generic
local function declarations, or higher-rank polymorphism as specified features.

## Named function arguments and eta expansion

Where a function value is expected, a named function group can expand into a lambda:

```text
Square  becomes  x => Square(x)
```

This is **eta expansion**: the normalizer introduces parameters and forwards them.
It does so only when the function group has a consistent nonzero arity. A
mixed-arity group or a nullary group has special fallback handling. Write an
explicit lambda when the selection is unclear. The transformation is in
[Normalizer](../../../src/Plato.Compiler/Checking/Normalizer.cs).

A lambda can capture an outer ordinary value. The interpreter builds a closure over
a snapshot of the environment at creation, so later local rebinding does not change
the captured binding. The C# writer hoists captured locals into copies to reproduce
that value-at-creation behavior. The builder discipline forbids capturing a unique
builder, because that capture would be another path to mutable storage.
[TirLambdaCaptureRewriter](../../../src/Plato.Compiler/Checking/TirLambdaCaptureRewriter.cs)
handles the emitted capture. The
[Interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs) evaluates
function values.

## When the lambda runs

In `samples.Map(x => x * scale)`, `scale` comes from the enclosing function and `x`
is each array element. Writing the lambda does not run it. `Map` calls it while
producing results. How an array combinator evaluates its function is a separate
contract. See [Evaluation, order, and faults](evaluation.md).
