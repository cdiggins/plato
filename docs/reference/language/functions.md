# Functions, receivers, and lambdas

A function declares ordered parameters, a result type, and a computation. Parameters and results are explicit; locals and lambda parameters are normally inferred. An expression body returns its expression:

```plato
Square(x: Number): Number => x * x;
```

Functions live inside `library` or `interface` declarations. A block body supports local steps and explicit `return`; see [locals](state-and-builders.md). A declaration ending immediately in `;` has no body and asks the runtime to supply the operation. The library's intrinsic contract restricts where such declarations belong and which primitive types they may mention.

## One call, several spellings

The first parameter is the receiver. These denote the same receiver-first call:

```plato
Lerp(a, b, t)
a.Lerp(b, t)
```

Likewise, `Square(x)` and `x.Square` are source-level uniform access. A stored field and a computed no-extra-argument member share this convenient spelling. Generated C# uses methods for ordinary computed members, so the second spelling becomes `x.Square()` in host code. Stored product fields remain fields, and handwritten intrinsic members can retain properties. Do not copy one C# spelling mechanically to every source access.

A first parameter named `_` indicates a type-based operation:

```plato
Origin(_: Point2D): Point2D => (0.0, 0.0);
```

Call it as `Point2D.Origin`. The receiver's type matters; its value is unused. A type name in this position is a type token, not an ordinary default instance whose fields may be read.

All libraries contribute to global function groups by name. Overload resolution uses parameter types, generic bindings, capabilities, and conversions. There are no library-qualified namespaces separating otherwise identical names. See [resolution](../implementation/type-checking.md).

## Higher-order functions

A higher-order function accepts another function. Mapping a numeric array can be written:

```plato
xs.Map(x => x * x)
```

The target `Map` signature determines `x`'s type and the result element type. For two parameters use `(a, b) => expression`. Function type `Function2<A, B, R>` means arguments `A`, `B`, result `R`; `Function0<R>` has no arguments.

The grammar allows expression and block lambda bodies. The checked reference idiom uses lambdas as arguments to higher-order functions, with parameter types inferred from the target signature. The implementation also has `TirLambda` and `TirInvoke` nodes, but this does not establish arbitrary unrestricted first-class function storage, generic local function declarations, or higher-rank polymorphism as specified features.

## Named function arguments and eta expansion

Where a function value is expected, a named function group can be expanded into a lambda:

```text
Square  becomes  x => Square(x)
```

This is **eta expansion**: introducing parameters and forwarding them. The normalizer only performs this transformation when the function group has a consistent nonzero arity. Mixed-arity or nullary groups have special fallback handling; write an explicit lambda when selection is unclear.

A lambda can capture an outer ordinary value. The interpreter creates a closure over a snapshot of the environment at creation, so later local rebinding does not change the captured binding. C# emission hoists captured locals into copies to reproduce this value-at-creation behavior. Capturing unique builders is forbidden by the builder discipline; it would create another path to mutable storage.

## Scientific reading example

For `samples.Map(x => x * scale)`, `scale` comes from the enclosing function and `x` is each array element. The function is not executed when its syntax is declared; `Map` invokes it while producing results. Array combinator evaluation contracts still matter: an eager map is not a lazy mathematical sequence.

## Evidence and related work

[Normalizer](../../../src/Plato.Compiler/Checking/Normalizer.cs) performs residual eta expansion. [TirLambdaCaptureRewriter](../../../src/Plato.Compiler/Checking/TirLambdaCaptureRewriter.cs) handles emitted capture concerns. [Interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs) evaluates function values. [C# extension methods](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#extension-methods) offer superficially similar receiver syntax, but Plato makes receiver-first calls a uniform source-language rule rather than a C#-specific method lookup category.
