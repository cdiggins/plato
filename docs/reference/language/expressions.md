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
