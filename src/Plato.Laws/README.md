# Compiled law execution

This library lets developers execute Plato laws against generated C# and compare those results with the interpreter. `CSharpProgram.cs` emits and compiles a program using Roslyn, the C# compiler library. `CSharpBackend.cs` invokes generated methods, and `HostValues.cs` converts their values. `DifferentialRun.cs` compares the two execution paths; `LawBackends.cs` names and constructs them for command-line and warm-server callers.

From the repository root, `dotnet run --project src/Plato.CLI -c Release -- run stdlib/foundation --laws --backend=csharp` selects compiled execution. The project targets .NET 8 and embeds the top-level [intrinsic runtime sources](../Plato.Intrinsics/README.md), so deployed consumers do not need a source checkout. Compilation and reflection add work compared with [interpreted execution](../Plato.Interpreter/README.md), but exercise emitted code and host conversions.

The shared law protocol and limitations are documented in the [interpreter design](../../docs/interpreter.md). [Plato.Interpreter.Tests](../../tests/Plato.Interpreter.Tests) exercises compiled and differential execution. A successful interpreter run alone does not verify generated C#. Record executable evidence in the [gate log](../../docs/gate-log.md), and report defects through the [tracker](../../tracker/readme.md).
