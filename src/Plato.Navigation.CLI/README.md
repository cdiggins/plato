# Navigation command line

This .NET 8 executable exposes the [navigation library](../Plato.Navigation/README.md) to command-line users and editor clients. `Program.cs` handles one-shot queries, checks and diagnostics. `Serve.cs` maintains an incremental index for a persistent protocol in which each request and response is one line of JSON on standard input and output.

From the repository root, `dotnet run --project src/Plato.Navigation.CLI -c Release -- search Number --root stdlib/foundation` searches definitions in one source root. The usage text in `Program.cs` defines the supported verbs and options. `serve` is a process protocol host; editor integration and file watching belong to its caller.

[Plato.Navigation.Tests](../../tests/Plato.Navigation.Tests) checks the underlying indexing and analysis behavior. See the [navigation guide](../Plato.Navigation/README.md) for query precision and warm execution constraints. Record gate outcomes in the [gate log](../../docs/gate-log.md) and issues in the [tracker](../../tracker/readme.md).
