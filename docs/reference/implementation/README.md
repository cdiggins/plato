# Implementing Plato

These pages are for someone implementing or debugging the compiler, the interpreter, or the C# writer. A page says what that stage does, what it must not invent, and which source file is the authority.

A compatible implementation binds one global program, derives data operations, solves types and overloads, preserves conversion and evaluation behavior, and supplies runtime primitives. The current C# path and the interpreter are the evidence. Proof-of-concept writers are excluded as semantic sources.

Read the [language guide](../language/README.md) for source meaning, then follow:

1. [Grammar and front end](grammar.md): syntax, precedence repair, binding, and implicit members.
2. [Type checking and overloads](type-checking.md): equations, constraints, ranking, and caveats.
3. [Typed IR and execution](typed-ir.md): explicit decisions, specialization, and interpreter behavior.
4. [C# representation](csharp.md): products, sums, wrappers, interfaces, and runtime contracts.
5. [Verification](verification.md): what each kind of test establishes.
6. [Discrepancies and open specification work](discrepancies.md): contradictions that must not be papered over.
7. [Formal work](../formal/README.md): separately stated models and proof scope.

## Reimplementation contract

A reimplementation can reject forms the grammar recognizes and the later stages do not support. It must not silently select different arithmetic, insert extra conversions, treat every interface occurrence as a runtime object type, or substitute an arbitrary overload because the emitted C# compiles.

For a source fixture, record the intended outcome at every stage: parse, resolve, check, elaborate, execute, emit, compile, execute generated code. Keep source locations through all of those stages. A source error, a missing host intrinsic, and an unsupported lowering are different results. A degraded throwing stub is diagnostic output. It is not an implementation of the source function.

The account goes past a syntax tour. It is not a completed independent language standard. The [discrepancy register](discrepancies.md) names the remaining boundaries, including existential storage, incomplete foreach desugaring, permissive generic fallbacks, and match-subject evaluation. A clean-room implementation must settle those explicitly or reproduce the chosen behavior with conformance fixtures.

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

Legacy comments that mention a shadow TIR, or an optional old C# body writer, are not the current architecture. Follow the call sites and the tests.
