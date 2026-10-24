# Declaration context export

This .NET 8 executable exports Plato type and interface declarations for developers preparing compact language context. It parses source rather than compiling implementation bodies. `Program.cs` chooses source folders and formatting; `ExportDiagnostics.cs` measures the resulting document when diagnostics are requested.

From the repository root, `dotnet run --project src/Plato.ContextExport -c Release -- stdlib/foundation --output .temp/foundation-context.txt` exports declarations to a scratch file. Folder traversal is recursive. The checked-in vocabulary index has its own prescribed regeneration command in [stdlib/AGENTS.md](../../stdlib/AGENTS.md); an ad hoc export does not replace that workflow.

[Plato.ContextExport.Tests](../../tests/Plato.ContextExport.Tests) checks formatting and diagnostics. The exporter intentionally leaves implementation semantics to the compiler. See the [agent guide](../../docs/plato-for-agents.md) for related compiler tools and the [tracker](../../tracker/readme.md) for issue reporting.
