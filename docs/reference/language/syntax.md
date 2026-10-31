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

## Grammar

The declaration and function productions, copied from [PlatoGrammar.cs](../evidence/grammar-source.md). The parser accepts some forms that the checker rejects; see [unsupported forms](diagnostics.md) and the [discrepancy register](../implementation/discrepancies.md).

```text
public Rule File => Node(WS + TopLevelDeclaration.ZeroOrMore() + AdvanceOnFail + EndOfInput);
public Rule TopLevelDeclaration => Node(Library | Concept | Type);
public Rule Library =>
    Node(Keyword("library") + AdvanceOnFail + Identifier + Braced(MethodDeclaration.ZeroOrMore(), AdvanceOnFail));
public Rule MethodDeclaration =>
    Node(Identifier + FunctionParameterList + AdvanceOnFail + TypeAnnotation + ConstraintList + FunctionBody);
public Rule FunctionParameter => Node(Identifier + TypeAnnotation);
public Rule FunctionParameterList => Node(ParenthesizedList(FunctionParameter));
public Rule TypeAnnotation => Node(Sym(":") + AdvanceOnFail + TypeExpr);
public Rule FunctionBody => Node(ExpressionBody | CompoundStatement | EOS);
public Rule ExpressionBody => Node(Sym("=>") + AdvanceOnFail + Expression + EOS);
public Rule EOS => Named(Sym(";"));
public Rule Comment => Named(CppStyleComment);
```

## Meaning

A file is a list of `library`, `interface` (or `concept`) and `type` declarations. A function is a name, a parenthesised
parameter list, a mandatory `: Type` result annotation, an optional `where` clause, and a body. The body is
`=> expression;`, a braced block, or a bare `;`. The normative statement is
[SEMANTICS section 3](../../SEMANTICS.md#3-library--functions); this chapter does not restate it.

## Common mistake

The result annotation is not optional. A beginner from C#, Python or TypeScript often leaves it out and writes the
body straight after the parameters. The program is rejected at parse time, so nothing is checked or run.

```plato
library Bad {
    Square(x: Integer) => x * x;
}
```

The reference tool reports:

```text
Error at (0,14,0,33) while parsing MethodDeclaration, expected Rule failed TypeAnnotation ::= (Symbol(':')+_RECOVER_+TypeExpr+WS)
Parse error at line 1, column 46 (furthest grammar failure)
Parsing failed for one of the input files, halting
```

Name the result type:

```plato
library Good {
    Square(x: Integer): Integer => x * x;
}
```

Two more near-misses give weaker messages. A parameter with no type (`Square(x): Integer => x * x;`) and an expression
body with no closing `;` (`Square(x: Integer): Integer => x * x }`) are both rejected, but the text names the whole
`Library` or `File` rule (`expected Rule failed __ ::= '}'` and `expected Rule failed EndOfInput ::= _END_`) and, for the
second, a position of `(0,0,0,0)`, not the missing token. Read the reported line and column, not the rule name. Both are
candidates for the language-improvements list.
