# Diagnostics and unsupported forms

A parser recognizes text. A binder resolves names. A checker validates types and obligations. An elaborator produces executable typed nodes. A writer may still fail to lower a body. Success at an earlier stage does not establish success at the next.

The grammar deliberately recognizes C#-like forms that are outside the supported language. The implementation is not required to give every unsupported form the same diagnostic: some AST translations throw or produce fallback nodes. An agent implementing the language should reject unsupported forms deliberately and retain source locations.

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

`typeof`, `nameof`, cast syntax, interpolated strings, numeric suffixes, and other parser affordances need a specific checked end-to-end contract before being treated as reference-language forms. This guide does not silently promote them because their grammar rules exist.

## Diagnostic families

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
| CHK300–CHK307 | Sum and match well-formedness, including optional generic-sum restriction |
| CHK308 | Stored existential interface has no usable object-safe view |
| CHK309 | Constructed type argument violates a declaration bound |
| CHK310 | A bound names something other than an interface |
| CHK311 | Construction violates `sealed by` |
| ELB001 | Elaboration retained an unresolved expression; inspect the reason rather than treating it as a verified body |
| LINT001 | Missing interface member; can lead to runtime throwing implementations |
| LINT006/LINT007 | Builder storage violates field/container restrictions |

This is a reader's map, not an exhaustive frozen list. The diagnostic's source and tests define its exact trigger. [Solver](../../../src/Plato.Compiler/Checking/Solver.cs), [SumTypeChecker](../../../src/Plato.Compiler/Checking/SumTypeChecker.cs), [TypeConstraintChecker](../../../src/Plato.Compiler/Checking/TypeConstraintChecker.cs), and [Linter](../../../src/Plato.Compiler/Analysis/Linter.cs) are the authorities.

## Avoiding false success

A successful lint invocation without strict mode may still report findings. A code-generation process can exit successfully while printing `DEGRADED bodies` and emitting throwing stubs. Inspect diagnostics and emitted-body health, then compile and exercise the relevant C# or interpreter path. Use [verification guidance](../implementation/verification.md) rather than taking one exit code as complete evidence.

## Related work

Language standards commonly separate lexical syntax, static semantics, and runtime behavior. The [C# specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) describes these distinctions for C#; Plato's much smaller supported subset must be specified on its own. Familiar syntax should make code easier to read, not act as a promise of inherited C# behavior.
