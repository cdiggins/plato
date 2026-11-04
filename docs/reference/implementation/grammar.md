# Grammar and front-end construction

The front end turns source text into a bound program. It parses with the concrete grammar, repairs operator association, collects declarations before it resolves names, and derives members from products and sums. It must not treat "the parser accepted it" as membership in the language, and it must not fill a missing production by analogy with C#.

The parser uses Parakeet's parsing-expression grammar in [PlatoGrammar](../../../parakeet/Parakeet.Grammars/PlatoGrammar.cs), with token rules in [PlatoTokenGrammar](../../../parakeet/Parakeet.Grammars/PlatoTokenGrammar.cs). The compact EBNF below is a reading grammar for the checked core. It is not a byte-for-byte replacement for those parser rules.

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

The actual parser also recognizes compound type expressions, C#-style array rank suffixes, typed local declaration spellings, do/for/foreach forms, and deliberately unsupported C# constructs. Consult the source for those productions. The safe core above omits them on purpose.

## Expression pipeline

The PEG parses a leaf with its prefix and postfix components. A binary operation then recursively includes the expression on its right. That raw recursion is not the final operator association. `AstNodeFactory.CreateBinaryOp` rebalances a right-hand side of equal or lower precedence so the operator associates to the left. Prefixes apply after tight postfixes, and before binary and ternary folding.

```text
a.F(b)   → F(a,b)
a.F      → F(a)
a[i]     → At(a,i)
a + b    → Add(a,b)
-a       → Negative(a)
(a,b)    → Tuple2(a,b)
```

"Member syntax without an argument list" and "member syntax followed by parentheses" are different shapes. Later handling needs that call-shape information, including Boolean short circuiting and function-valued accessors. Boolean lazy lowering then recognizes the call name and the Boolean argument and result types, so the named spelling and the operator spelling share that behavior.

## Binding a global program

Collect declarations before resolving their references. Build nominal type identities, declared type parameter scopes, interface inheritance links, fields and cases, and global function groups. A library organizes ownership. It does not isolate names.

Derive source constructors, accessors, and conversion relations from products. Derive case constructors, tests, and projections from sums. The C# writer's additional immutable `With...` methods are host conveniences. They are not source setter declarations that the binder resolves on its own.

A reference node points at a definition identity. An equal-looking string is not enough. Normalization deliberately reuses parameter and local definition objects, so a rewritten reference still identifies the same binding. A reimplementation that clones definitions and does not remap references breaks closures and local lookups.

A complete implementation must distinguish declaration parameters (`T`), signature variables (`$T`), `Self`, primitive names, and ordinary unresolved names. A typo in a library signature must not create an accidental free bare generic parameter.

## Recovery and unsupported syntax

The grammar's `AdvanceOnFail` skips toward a semicolon or a closing brace to recover after errors. Recovery is a tooling feature. It is not permission to execute a damaged syntax tree. Some parser-recognized forms throw during AST translation, and some never receive elaborator cases. Enumerate support deliberately.

The lexical specification is not yet fully independent of Parakeet's shared base rules. Identifier character classes, escape interpretation, literal overflow, suffix handling, and error-recovery interactions need explicit conformance examples before anyone claims a standalone scanner specification. See [discrepancies](discrepancies.md).

The [C# grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) is a useful comparison for surface syntax. Plato's AST rewrites and the supported subset are independent of it. Concrete parsing stays separate from abstract binding, so editor tooling can recognize malformed or unsupported syntax while the executable language stays smaller.
