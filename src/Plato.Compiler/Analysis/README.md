# Declaration analysis

This folder analyzes compiled declarations, receiver types, generic instantiations, and interface implementations. `Linter.cs` reports source conventions separately from the type checker.

Use `LibraryFunctions.cs` to find receiver-based function indexing and `ConceptHierarchy.cs` for interface inheritance. Body constraint solving belongs in the neighboring `Checking/` folder.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
