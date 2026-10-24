# Abstract syntax trees

This project translates parser output into Plato abstract syntax trees, the source-shaped nodes consumed by symbol binding. `AstNodeFactory.cs` maps parser productions to nodes defined in `Ast.cs`; `AstBuilder.cs` supplies location-aware construction helpers.

`Operators.cs` owns operator classification and precedence. `Formatting/` contains source and XML writers plus formatting support. Parsing depends on the Parakeet projects referenced by `Plato.AST.csproj`; keep grammar changes in that repository.

See the [compiler and language guide](../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
