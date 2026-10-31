# Diagnostics and unsupported forms

The parser recognizes text. The binder resolves names. The checker validates types and obligations. The elaborator produces executable typed nodes. A writer can still fail to lower a body. Success at an earlier stage does not establish success at the next.

The grammar recognizes some C#-like forms that sit outside the supported language. The implementation does not have to diagnose every unsupported form the same way. Some AST translations throw, and some produce a fallback node. An implementer should reject an unsupported form deliberately and keep its source location.

## Supported versus merely parsed

| Form | Reference status |
|---|---|
| Product/sum/primitive declarations, libraries, interfaces | Supported, subject to their specific well-formedness rules |
| Expression/block bodies, return, local declarations, assignment | Supported core; assignment is local rebinding |
| Conditionals, while, for/do desugaring | Supported mechanism; see loop caveats |
| `foreach` | Parsed; current desugaring is incomplete |
| `break`, `continue`, `yield`, `switch` | Parsed forms outside the supported elaboration subset |
| `throw`, `try`, `catch`, `finally` | No source-language exception system |
| `is`, `as`, `?.`, null, reflection-like forms | Do not use as checked language features |
| `++`, `--`, compound assignment | No supported ordinary update syntax |
| Local function declarations, arbitrary object initializers | Parser recognition is not a portability guarantee |
| `unique` on arbitrary user types | Hard-rejected; intrinsic List/Buffer only |

`typeof`, `nameof`, cast syntax, interpolated strings, numeric suffixes, and other parser affordances need a checked end-to-end contract before they count as reference-language forms. A grammar rule on its own does not promote them.

Familiar punctuation is easier to read. It does not import C# behavior. The [C# specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) states the rules for C#. The table above is Plato's supported subset.

## Diagnostic families

The checker, the elaborator, and the linter report the codes below. The table is a reader's map, not an exhaustive frozen list. A diagnostic's source and its tests define the exact trigger.

| Code | Meaning and consequence |
|---|---|
| CHK101 | Types cannot unify; correct the conflicting types or add an intended conversion at a supported boundary |
| CHK102 | Type argument arity mismatch |
| CHK103 | An inference variable would contain itself, giving an infinite inferred type |
| CHK201 | No viable overload matches the arguments |
| CHK202 | Informational common-return tie; the current solver accepts a candidate |
| CHK203 | Ground ambiguity with differing result types; selection is rejected |
| CHK204 | Informational unresolved polymorphic tie deferred to specialization |
| CHK205 | A known bound does not license an operation on a generic parameter |
| CHK206 | A function call's inferred type violates a declared function bound |
| CHK300-CHK307 | Sum and match well-formedness, including optional generic-sum restriction |
| CHK308 | Stored existential interface has no usable object-safe view |
| CHK309 | Constructed type argument violates a declaration bound |
| CHK310 | A bound names something other than an interface |
| CHK311 | Construction violates `sealed by` |
| ELB001 | Elaboration retained an unresolved expression; inspect the reason rather than treating it as a verified body |
| LINT001 | Missing interface member; can lead to runtime throwing implementations |
| LINT006/LINT007 | Builder storage violates field/container restrictions |

[Solver](../../../src/Plato.Compiler/Checking/Solver.cs), [SumTypeChecker](../../../src/Plato.Compiler/Checking/SumTypeChecker.cs), [TypeConstraintChecker](../../../src/Plato.Compiler/Checking/TypeConstraintChecker.cs), and [Linter](../../../src/Plato.Compiler/Analysis/Linter.cs) are the authorities for these codes.

## Avoiding false success

A lint run that is not in strict mode can still report findings. Code generation can exit successfully, print `DEGRADED bodies`, and emit throwing stubs. Inspect the diagnostics and the emitted bodies, then compile and exercise the relevant C# or interpreter path. [Verification](../implementation/verification.md) is the guide for that evidence. One exit code is not complete evidence.
