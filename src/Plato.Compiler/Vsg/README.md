# Visual syntax graph

This folder projects compiler declarations and relationships into a visual syntax graph for inspection tools. `VisualSyntaxGraph.cs` builds the representation and `VisualSyntaxGraphQueries.cs` provides queries over it.

The graph uses the compiler symbol model; it does not replace binding or type checking.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
