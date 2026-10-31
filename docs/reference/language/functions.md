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

## Grammar

The function, call and lambda productions, copied from [PlatoGrammar.cs](../evidence/grammar-source.md). `FunctionArgKeyword` (`ref`, `out`, `in`, `params`) is also parsed in an argument but is not a Plato feature.

```text
public Rule MethodDeclaration =>
    Node(Identifier + FunctionParameterList + AdvanceOnFail + TypeAnnotation + ConstraintList + FunctionBody);
public Rule FunctionParameter => Node(Identifier + TypeAnnotation);
public Rule FunctionParameterList => Node(ParenthesizedList(FunctionParameter));
public Rule FunctionBody => Node(ExpressionBody | CompoundStatement | EOS);
public Rule ConstraintList => Node(Optional(Keyword("where") + AdvanceOnFail + List(Constraint)));
public Rule Constraint => Node((TypeVar | Identifier) + TypeAnnotation);
public Rule FunctionArgs => Node(ParenthesizedList(FunctionArg));
public Rule FunctionArg => Node(FunctionArgKeyword.ZeroOrMore() + Expression);
public Rule MemberAccess => Node(Sym(".") + Identifier);
public Rule LambdaParameters => Node(LambdaParameter | ParenthesizedList(LambdaParameter));
public Rule LambdaBody => Node(CompoundStatement | Expression);
public Rule LambdaExpr => Node(LambdaParameters + Sym("=>") + AdvanceOnFail + LambdaBody);
```

## Meaning

A function is a named, typed body. `Square(x)` and `x.Square` are the same call spelled two ways, and all overloads of a name
share one global group. The authority is
[SEMANTICS section 3](../../SEMANTICS.md#3-library--functions) for declarations and
[section 6](../../SEMANTICS.md#6-expressions-construction-conversions-operators) for the call forms.

## Common mistake

A misspelled name is the usual first error:

```plato
library Bad {
    Square(x: Integer): Integer => x * x;
    Use(y: Integer): Integer => Squar(y);
}
```

This program is rejected. Name resolution stops the build and the reference tool prints (the file name is trimmed to the
scratch file the program was saved in):

```text
Halted after 4 symbol resolution error(s); see SymbolFactory.Errors for the details
RESOLVE b.plato:1: Could not find symbol Squar
RESOLVE b.plato:1: Could not find function (AstIdentifier=Squar)
RESOLVE b.plato:1: Exception occurred System.Exception: Could not find function (AstIdentifier=Squar)
RESOLVE b.plato:1: Unexpected missing body
Compilation was not completed
```

Four lines carry one fact, and the last ("Unexpected missing body") is only a consequence. The message does not suggest
`Square`. Fix the spelling:

```plato
library Good {
    Square(x: Integer): Integer => x * x;
    Use(y: Integer): Integer => Square(y);
}
```

A call with the wrong argument count, such as `Square(1, 2)`, passes the parser and the resolver and fails only when a
`Law_` function reaches it, with `cannot resolve 'Square' for (Integer, Integer) (in Use < Law_U)`. A function no law
reaches is not elaborated, so `lint --strict` and `run --laws` stay silent about it; the type checker would report
CHK201, which these commands do not print.
