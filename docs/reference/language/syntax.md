# Reading and writing source

A Plato file is a sequence of declarations, with whitespace and `//` comments between tokens. Names are case-sensitive. Follow the library's naming convention: types and functions use descriptive PascalCase names, parameters and locals use camelCase. Convention is not a separate type rule.

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

A colon introduces a type annotation. Semicolons terminate field declarations, expression bodies, and statements. Braces group declaration bodies or statement blocks. Commas separate arguments, fields in tuple expressions, generic parameters, and bounds.

## Signature shape

```text
Name(parameter: Type, ...): ResultType where $T: Interface => expression;
Name(parameter: Type, ...): ResultType { statements }
Name(parameter: Type, ...): ResultType;
```

The `where` clause is optional. The last form has no body and creates a runtime obligation; ordinary library functions need bodies. Function parameters and return types are explicit. Local and lambda types are normally inferred.

A type expression is a named type, an application such as `Array<Point2D>`, or a signature variable such as `$T`. Declaration parameters use bare `T` in their own declaration; free generic variables in library signatures use `$T`. See [generics](generics.md).

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

A minus sign is an operator applied to a literal, not part of the unsigned token. Profile choice affects the representable value of numeric literals. Prefer ordinary decimal syntax shown here. The tokenizer also recognizes C#-like suffixes, hexadecimal, binary, and escape spellings; recognizing a token is not a guarantee of portable checked semantics for every spelling. The [grammar audit](../implementation/grammar.md) identifies that boundary.

## Parentheses and access

`(e)` groups one expression. `(e0, e1)` constructs a tuple. `a.F(b)` means `F(a, b)`; `a.F` means `F(a)` when it names a no-extra-argument member. `a[i]` means `At(a, i)`. These forms are elaborated into ordinary calls rather than separate object-oriented dispatch operations.

The grammar accepts more C#-shaped forms than the checked language supports. Never infer support from a successful parse alone. [Unsupported forms](diagnostics.md) and [the grammar](../implementation/grammar.md) give the supported subset and source evidence.

## Related work

The punctuation resembles the [C# expression grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions), but type annotations are postfix (`x: Number`), bodies may be expression-based, and a member call is receiver-first free-function syntax. C# casts, visibility modifiers, and exception syntax cannot be imported by analogy.
