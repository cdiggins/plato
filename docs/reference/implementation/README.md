# Implementing Plato

A compatible implementation needs more than the parser grammar. It must bind one global program, derive data operations, solve types and overloads, preserve conversion and evaluation behavior, and supply runtime primitives. These pages describe the current C# and interpreter paths as implementation evidence; proof-of-concept writers are intentionally excluded as semantic sources.

Read the [language guide](../language/README.md) for source meaning, then follow:

1. [Grammar and front end](grammar.md): syntax, precedence repair, binding, and implicit members.
2. [Type checking and overloads](type-checking.md): equations, constraints, ranking, and caveats.
3. [Typed IR and execution](typed-ir.md): explicit decisions, specialization, and interpreter behavior.
4. [C# representation](csharp.md): products, sums, wrappers, interfaces, and runtime contracts.
5. [Verification](verification.md): what each kind of test establishes.
6. [Discrepancies and open specification work](discrepancies.md): contradictions that must not be papered over.
7. [Formal work](../formal/README.md): separately stated models and proof scope.

## Reimplementation contract

A reimplementation can intentionally reject the grammar's unsupported forms. It must not silently select different arithmetic, insert extra conversions, treat every interface occurrence as a runtime object type, or substitute an arbitrary overload merely because the emitted C# compiles.

For a source fixture, record the intended outcome at every stage: parse, resolve, check, elaborate, execute, emit, compile, execute generated code. Keep source locations through all stages. Distinguish a source error from a missing host intrinsic or an unsupported lowering. A degraded throwing stub is useful diagnostic output, not an implementation of the source function.

This reference is substantially more detailed than a syntax tour, but it is not a completed independent language standard. The [discrepancy register](discrepancies.md) names the remaining boundaries, including existential storage, incomplete foreach desugaring, permissive generic fallbacks, and match-subject evaluation. A clean-room implementation must settle those explicitly or reproduce the chosen behavior with conformance fixtures.

## Source map

| Concern | Implementation authority |
|---|---|
| Concrete grammar | [PlatoGrammar](../../../parakeet/Parakeet.Grammars/PlatoGrammar.cs) |
| AST and precedence repair | [AstNodeFactory](../../../src/Plato.AST/AstNodeFactory.cs), [Operators](../../../src/Plato.AST/Operators.cs) |
| Bound symbol graph | [Compilation](../../../src/Plato.Compiler/Compilation.cs), [SymbolFactory](../../../src/Plato.Compiler/Symbols/SymbolFactory.cs) |
| Checking and typed elaboration | [Checking folder](../../../src/Plato.Compiler/Checking/README.md) |
| Interpreter | [Evaluation folder](../../../src/Plato.Interpreter/Evaluation/README.md) |
| C# writer | [CSharpWriter](../../../writers/Plato.CSharpWriter/CSharpWriter.cs), [TirCSharpBodyWriter](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) |
| Runtime | [Intrinsics contract](../../plato-intrinsics-surface.md) |

Legacy comments mentioning a shadow TIR or an optional old C# body writer are not the current architecture. Follow actual call sites and tests.
