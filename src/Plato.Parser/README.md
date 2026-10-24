# Inactive parser wrapper

This project retains a commented-out parser wrapper in `Parser.cs`. It does not provide an executable parser implementation.

The active compiler consumes syntax trees built through [`AstNodeFactory.cs`](../Plato.AST/AstNodeFactory.cs) from Parakeet parser output. The project file remains available for existing solution references; this folder guide does not establish a replacement parser API.

See the [compiler and language guide](../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
