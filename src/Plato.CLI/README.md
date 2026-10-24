# Plato command line

This executable is the command-line front door for developers compiling Plato libraries or running their laws. `Program.cs` validates generation options and selects a writer; `RunCommand.cs` handles execution and reports law verdicts. `Document.cs` holds parser state for compiler inputs, and `Config.cs` supplies standalone defaults.

From the repository root, `dotnet run --project src/Plato.CLI -c Release -- lint stdlib/foundation --strict` parses and resolves a tier without generating output. The project targets .NET 8. See the [agent command guide](../../docs/plato-for-agents.md) for generation commands and exit-code limits, and the [interpreter guide](../../docs/interpreter.md) for law execution. Backend support differs; a successful generation exit does not establish that every body lowered successfully.

The compiler and law suites under [tests](../../tests) exercise this entry point's dependencies. Gate results belong in the [gate log](../../docs/gate-log.md). Report issues through the repository [tracker](../../tracker/readme.md).
