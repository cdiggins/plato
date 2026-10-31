# Expressions, operators, and conversions

An **expression** computes a value. Literals, names, calls, field access, tuple and array construction, conditionals, lambdas, and `match` are expression forms. The checker types an expression before the interpreter evaluates it. Similar C# syntax is not a Plato rule.

## Operator names

An **operator** is syntax for a named function. The resolver uses the same overload machinery as an ordinary call.

| Syntax | Function name |
|---|---|
| `a + b`, `a - b` | `Add`, `Subtract` |
| `a * b`, `a / b`, `a % b` | `Multiply`, `Divide`, `Modulo` |
| `a == b`, `a != b` | `Equals`, `NotEquals` |
| `a < b`, `a <= b` | `LessThan`, `LessThanOrEquals` |
| `a > b`, `a >= b` | `GreaterThan`, `GreaterThanOrEquals` |
| `a & b`, `a \| b`, `a ^ b` | `BitwiseAnd`, `BitwiseOr`, `XOr` |
| `a && b`, `a \|\| b` | `And`, `Or`, with Boolean short circuiting |
| `-a`, `!a`, `~a` | `Negative`, `Not`, `Complement` |

The spelling does not create the overload. `Point2D + Point2D` is legal only when the program supplies a matching operation. Units and dimensional meaning come from the declared types and functions.

## Precedence and associativity

From highest to lowest, the binary groups are multiplication, division, and remainder; addition and subtraction; comparisons; equality; bitwise `&`; bitwise `^`; bitwise `|`; Boolean `&&`; Boolean `||`. Binary operators in that table associate to the left. `a - b + c` means `(a - b) + c`. `a + b * c` means `a + (b * c)`.

Member access, calls, and indexing bind more tightly than unary operators. Unary operators bind more tightly than binary operators. The conditional `condition ? yes : no` selects one branch. Parenthesize a complex condition or a nested conditional.

Shift precedences appear in the operator implementation, but shifts do not have a complete ordinary operator-name mapping. They are not thereby supported language operations.

[Operators](../../../src/Plato.AST/Operators.cs) is the mapping and precedence authority. [AstNodeFactory](../../../src/Plato.AST/AstNodeFactory.cs) rebalances the parser's recursive tree, and associativity follows that tree. The raw parse recursion is not the language's associativity.

[C# operator precedence](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/) is familiar background for this grouping. Plato's operator set is the table above. Compound assignment, `++`, and `--` are [unsupported forms](diagnostics.md).

## Conditions and arrays

A condition has type `Boolean`. Both branches of `c ? a : b` must unify to one result type. Unrelated branch types are not allowed merely because the host has an object base type. Alternatives with different payload types are a [sum](sums.md).

`[e0, e1, ...]` constructs `Array<T>` by unifying the element types. An empty array needs enough context to determine the element type. `xs[i]` is `At(xs, i)` and follows that function's partiality and bounds behavior. This does not promise Python-style negative indices, slicing, or multidimensional storage.

## Conversions

A one-argument function named after its result type is a **conversion**:

```plato
Number(x: Integer): Number => ...;
```

The signature above is an illustration. The primitive operation belongs to the intrinsic contract. A user conversion can have an ordinary body. The resolver may insert a conversion at an argument boundary or a return boundary. An exact match, a generic match, or an interface match is preferred over a conversion. Tuple-to-product construction is a separate compatibility path; see [products](products.md).

A conversion can round, overflow, or fault. It is not mathematical equality. The elaborator records an inserted conversion as `TirCoerce`, with the source type, the target type, and the chosen conversion function. A writer should consume that decision rather than print a different cast.

There is no general promise that arbitrary chains of user conversions are searched transitively. Write an intermediate conversion explicitly. [Solver.Coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs) and the compilation relation mechanism are the implementation of that compatibility.

In the generated C# surface, a conversion resembles a user-defined conversion operator. The resolver chooses it by Plato's cost model.

## Grammar

The expression productions, copied from [PlatoGrammar.cs](../evidence/grammar-source.md) and [PlatoTokenGrammar.cs](../evidence/grammar-source.md). The grammar is wider than the checked language: `BinaryOperator` lists compound assignment (`+=`, `-=`, ...) and `PrefixOperator` lists `++` and `--`, none of which the language supports. See [unsupported forms](diagnostics.md) and the [discrepancy register](../implementation/discrepancies.md).

```text
public Rule PrefixOperator => Node(Symbols("++", "--", "!", "-", "+", "~"));
public Rule BinaryOperator => Node(Symbols(
    "and", "or", "+", "-", "*", "/", "%", "<<", "&&", "||", "&", "|", "^",
    "+=", "-=", "*=", "/=", "%=", ">>>=", ">>=", "<<=", "&&=", "||=", "&=", "|=", "^=",
    "=", "<", ">", "<=", ">=", "==", "!="
));
public Rule BinaryOperation => Node(Not("=>") + BinaryOperator + AdvanceOnFail + Expression);
public Rule TernaryOperation => Node(Sym("?") + AdvanceOnFail + Expression + Sym(":") + Expression);
public Rule PostfixOperator => Node(
    Symbols("++", "--")
    | MemberAccess
    | ConditionalMemberAccess
    | FunctionArgs
    | Indexer
    | TernaryOperation
    | BinaryOperation
    | IsOperation
    | AsOperation
);
public Rule MemberAccess => Node(Sym(".") + Identifier);
public Rule Indexer => Node(Bracketed(Expression));
public Rule ParenthesizedExpression => Node(ParenthesizedList(Expression));
public Rule ArrayExpression => Node(BracketedList(Expression));
public Rule LambdaExpr => Node(LambdaParameters + Sym("=>") + AdvanceOnFail + LambdaBody);
public Rule InnerExpression => Node(
    PrefixOperator.ZeroOrMore()
    + LeafExpression
    + PostfixOperator.ZeroOrMore());
```

## Meaning

An expression computes one value. Operators are spellings of ordinary function calls (see the table above), and
`c ? a : b` selects one of two branches that must have one common type. The authority is
[SEMANTICS section 6](../../SEMANTICS.md#6-expressions-construction-conversions-operators).

## Common mistake

Plato has no compound assignment. A beginner from C# or Python writes an update in place:

```plato
library Bad {
    Bump(x: Integer): Integer { x += 1; return x; }
}
```

This program is rejected, but not by the parser: the grammar accepts `+=` as a binary operator, so the file parses. The AST
step then throws, and the reference tool prints (stack trace trimmed):

```text
Unhandled exception System.Exception: Operation `+=` is not a valid operator name
   at Ara3D.Geometry.AST.AstNodeFactory.CreateBinaryOp(...)
```

The run then carries on without the function: `lint --strict` and `run --laws` both exit successfully, and the function
is simply not in the compilation. The message names no file, line or function. This is an unhelpful diagnostic and a
silent drop; it is recorded as a candidate for the language-improvements list. Write the new value as an expression:

```plato
library Good {
    Bump(x: Integer): Integer => x + 1;
}
```

A second plausible mistake is a conditional whose branches differ in type, `c ? 1 : "no"`. The checker rejects it
(CHK101, "Cannot unify ..." in `Solver.Unification.cs`), but `lint --strict` and `run --laws` print nothing for it:
those commands do not surface the type checker's findings, and a law that takes the `true` branch passes. Use
`plato_check`, which runs the checker, to see CHK diagnostics.
