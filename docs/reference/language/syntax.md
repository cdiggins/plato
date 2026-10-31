# Reading and writing source

A Plato file is a sequence of declarations, with whitespace and `//` comments
between tokens. Names are case-sensitive. The library convention uses descriptive
PascalCase for types and functions, and camelCase for parameters and locals. That
convention is not a type rule.

```plato
// A position and its timestamp.
type Observation {
    Position: Point2D;
    Time: Number;
}

library Observations {
    Earlier(a: Observation, b: Observation): Boolean => a.Time < b.Time;
}
```

A colon introduces a type annotation. A semicolon terminates a field declaration,
an expression body, or a statement. Braces group a declaration body or a statement
block. Commas separate arguments, fields in a tuple expression, generic parameters,
and bounds.

## Signature shape

```text
Name(parameter: Type, ...): ResultType where $T: Interface => expression;
Name(parameter: Type, ...): ResultType { statements }
Name(parameter: Type, ...): ResultType;
```

The `where` clause is optional. The last form has no body. It creates a runtime
obligation. An ordinary library function needs a body. Function parameters and
return types are explicit. The compiler normally infers the types of locals and of
lambda parameters.

A type expression is a named type, an application such as `Array<Point2D>`, or a
signature variable such as `$T`. A declaration parameter uses bare `T` inside its
own declaration. A free generic variable in a library signature uses `$T`. See
[generics](generics.md).

## Literals

| Source | Meaning |
|---|---|
| `12` | `Integer` literal |
| `12.0`, `1.0e-3` | `Number` literal |
| `true`, `false` | `Boolean` literals |
| `'a'` | `Character` literal |
| `"sample"` | `String` literal |
| `[1, 2, 3]` | Immutable array expression |
| `(1.0, 2.0)` | Tuple expression |

A minus sign is an operator applied to a literal, not part of the unsigned token.
The numeric profile changes which values a numeric literal can represent. Prefer
the ordinary decimal syntax in the table. The tokenizer also recognizes C#-like
suffixes, hexadecimal, binary, and escape spellings. Recognizing a token does not
guarantee portable checked semantics for every spelling. The
[grammar audit](../implementation/grammar.md) identifies that boundary.

## Parentheses and access

`(e)` groups one expression. `(e0, e1)` constructs a tuple. `a.F(b)` means `F(a, b)`.
`a.F` means `F(a)` when the member takes no extra argument. `a[i]` means `At(a, i)`.
The compiler elaborates these forms into ordinary calls. They are not a separate
object-oriented dispatch.

Type annotations are postfix (`x: Number`). A body may be an expression. A member
call is receiver-first syntax for a free function. The punctuation resembles the
[C# expression grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions).
C# casts, visibility modifiers, and exception syntax are not Plato syntax.

The grammar accepts more C#-shaped forms than the checked language supports. A
successful parse does not mean the form is supported.
[Unsupported forms](diagnostics.md) and [the grammar](../implementation/grammar.md)
give the supported subset and the source evidence.
