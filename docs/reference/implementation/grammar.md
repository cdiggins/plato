# Grammar and front-end construction

The parser uses Parakeet's parsing-expression grammar in [PlatoGrammar](../../../parakeet/Parakeet.Grammars/PlatoGrammar.cs), with token rules in [PlatoTokenGrammar](../../../parakeet/Parakeet.Grammars/PlatoTokenGrammar.cs). The following compact EBNF is a **reading grammar for the checked core**, not a byte-for-byte replacement for those parser rules.

In this notation `[...]` means optional, `{...}` means repetition, `|` means alternatives, and quoted strings are literal tokens. `Identifier`, `Literal`, and `Expression` name grammar categories, not source names.

```text
Program      = { TypeDecl | InterfaceDecl | LibraryDecl } ;
TypeDecl     = ["unique"], ("type" | "primitive"), Identifier,
               [TypeParams], [Bounds], [Implements], [Sealed],
               ("{", {Field}, "}" | "=", Case, {"|", Case}, ";") ;
InterfaceDecl= ("interface" | "concept"), Identifier,
               [TypeParams], [Bounds], [Inherits], "{", {Function}, "}" ;
LibraryDecl  = "library", Identifier, "{", {Function}, "}" ;
TypeParams   = "<", Identifier, {",", Identifier}, ">" ;
Bounds       = "where", Bound, {",", Bound} ;
Bound        = (Identifier | "$", Identifier), ":", TypeExpr ;
Implements   = "implements", TypeExpr, {",", TypeExpr} ;
Inherits     = "inherits", TypeExpr, {",", TypeExpr} ;
Sealed       = "sealed", "by", Identifier, {",", Identifier} ;
Field        = Identifier, ":", TypeExpr, ";" ;
Case         = Identifier, ["(", [Parameters], ")"] ;
Function     = Identifier, "(", [Parameters], ")", ":", TypeExpr,
               [Bounds], ("=>", Expression, ";" | Block | ";") ;
Parameters   = Parameter, {",", Parameter} ;
Parameter    = Identifier, ":", TypeExpr ;
TypeExpr     = (Identifier | "$", Identifier),
               ["<", TypeExpr, {",", TypeExpr}, ">"] ;
Block        = "{", {Statement}, "}" ;
Statement    = Block | "return", Expression, ";"
             | "var", Identifier, "=", Expression, ";"
             | "if", "(", Expression, ")", Statement, ["else", Statement]
             | "while", "(", Expression, ")", Statement
             | Expression, ";" ;
```

The actual parser additionally recognizes compound type expressions, C#-style array rank suffixes, typed local declaration spellings, do/for/foreach forms, and deliberately unsupported C# constructs. Consult source rather than filling omissions by analogy. The safe core above omits these extra productions intentionally.

## Expression pipeline

The PEG initially parses a leaf with prefix and postfix components; a binary operation recursively includes its right expression. That raw recursion is not the final operator association. `AstNodeFactory.CreateBinaryOp` rebalances equal or lower right-side precedence to obtain left association. Prefixes apply after tight postfixes but before binary/ternary folding.

```text
a.F(b)   → F(a,b)
a.F      → F(a)
a[i]     → At(a,i)
a + b    → Add(a,b)
-a       → Negative(a)
(a,b)    → Tuple2(a,b)
```

Keep a distinction between “member syntax without an argument list” and “member syntax followed by parentheses”: later handling needs that call-shape information, including Boolean short circuiting and function-valued accessors. Boolean lazy lowering subsequently recognizes the call name and Boolean argument/result types, so the named and operator spellings share that behavior.

## Binding a global program

Collect declarations before resolving their references. Build nominal type identities, declared type parameter scopes, interface inheritance links, fields/cases, and global function groups. Libraries organize ownership but do not isolate names. Derive source constructors, accessors and conversion relations from products; derive case constructors and tests/projections from sums. The C# writer's additional immutable `With...` methods are host conveniences, not automatically resolved source setter declarations.

Reference nodes should point to definition identities, not merely equal-looking strings. Normalization deliberately reuses parameter/local definition objects so rewritten references still identify the same binding. A reimplementation that clones definitions without remapping references will break closures and local lookups.

A complete implementation must distinguish declaration parameters (`T`), signature variables (`$T`), `Self`, primitive names, and ordinary unresolved names. A typo in a library signature must not create an accidental free bare generic parameter.

## Recovery and unsupported syntax

The grammar's `AdvanceOnFail` skips toward semicolon or closing brace to recover after errors. Recovery is a tooling feature, not permission to execute a damaged syntax tree. Some parser-recognized forms throw during AST translation or never receive elaborator cases. Enumerate support deliberately instead of using “the parser accepted it” as the language membership test.

The lexical specification is not yet fully independent of Parakeet's shared base rules. Identifier character classes, escape interpretation, literal overflow, suffix handling, and error-recovery interactions need explicit conformance examples before claiming a standalone scanner specification. See [discrepancies](discrepancies.md).

## Related work

The [C# grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) supplies useful comparisons for surface syntax, but Plato's AST rewrites and supported subset are independent. Separating concrete parsing from abstract binding lets editor tooling recognize malformed or unsupported syntax while the executable language stays smaller.
