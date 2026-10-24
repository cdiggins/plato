# Resolved type analysis

This folder resolves applied type names and analyzes function arguments, overload groups, conversions, and generic type relationships. `TypeResolver.cs` is the entry point for resolving type expressions.

These analyses operate on the bound symbol model. The body checker and its unification state live in `../Checking/`; primitive names shared with code writers are defined in `WriterPrimitiveNames.cs`.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
