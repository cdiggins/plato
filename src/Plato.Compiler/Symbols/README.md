# Bound symbols

This folder defines the source model after syntax nodes receive scopes, name bindings, and resolved declarations. `SymbolFactory.cs` builds that model; `Scope.cs` and `Binding.cs` provide name lookup.

`Definitions.cs`, `Expressions.cs`, and `Statements.cs` separate declaration, value, and control-flow forms. `SymbolRewriter.cs` rebuilds those forms. `SymbolWriterXml.cs` retains commented-out exploratory code and supplies no executable XML writer.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
