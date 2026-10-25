# Expressions, operators, and conversions

An expression computes a value. Literals, names, calls, field access, tuple and array construction, conditionals, lambdas, and `match` are expression forms. Their types are checked before execution; C#'s ability to compile a similar-looking expression is not a separate language rule.

## Operator names

Operators are syntax for named functions. Overloads therefore use the same global resolution machinery as ordinary calls.

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

An operator spelling does not create the corresponding overload. `Point2D + Point2D` is legal only if the program supplies a matching operation. Units, geometry, and dimensional meaning come from declared types and functions.

## Precedence and associativity

Highest to lowest binary groups are multiplication/division/remainder; addition/subtraction; comparisons; equality; bitwise `&`; bitwise `^`; bitwise `|`; Boolean `&&`; Boolean `||`. Binary operators in the supported table are left-associative. Thus `a - b + c` means `(a - b) + c`, while `a + b * c` means `a + (b * c)`.

Member access, calls, and indexing bind more tightly than unary operators; unary operators bind more tightly than binary operators. The conditional `condition ? yes : no` selects one branch. Parenthesize complex conditions and nested conditionals for clarity. Shift precedences appear in the operator implementation but shifts do not have a complete ordinary operator-name mapping; they are not thereby supported language operations.

[Operators](../../../src/Plato.AST/Operators.cs) is the exact mapping and precedence authority; [AstNodeFactory](../../../src/Plato.AST/AstNodeFactory.cs) rebalances the parser's recursive tree. Do not take the raw parse tree's recursion direction as the language's associativity.

## Conditions and arrays

A condition has type `Boolean`. Both branches of `c ? a : b` must unify to a single result type; it is not an invitation to return unrelated values because the host has an object base type. Use a sum to represent alternatives with different payload types.

`[e0, e1, ...]` constructs `Array<T>` by unifying element types. Empty arrays need enough context to determine their element type. Indexing `xs[i]` is `At(xs, i)` and follows that function's partiality and bounds behavior. It does not promise Python-style negative indices, slicing, or multidimensional storage.

## Conversions

A one-argument function named after its result type defines a conversion:

```plato
Number(x: Integer): Number => ...;
```

This is a signature illustration; the actual primitive operation belongs to the intrinsic contract. A user conversion can have an ordinary body. The resolver may insert conversions at argument and return boundaries. They are less preferred than exact, generic, and interface matches. Tuple-to-product construction is another explicit compatibility path.

A conversion has behavior: it can round, overflow, or fault. It is not mathematical equality. The elaborator records an inserted conversion as `TirCoerce`, including source and target types and the chosen conversion function. Writers should consume that decision rather than invent a different cast during printing.

No general promise is made that arbitrary chains of user conversions are searched transitively. For predictable code, make an intermediate conversion explicit and inspect [Solver.Coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs) and the compilation relation mechanism when implementing compatibility.

## Related work

[C# operator precedence](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/) is familiar background for the grouping, but Plato's operator set and desugaring table are authoritative. In particular, compound assignment and increment/decrement are not imported from C#. Conversions resemble C# user-defined conversion operators in the generated surface; the resolver chooses them according to Plato's own cost model.
