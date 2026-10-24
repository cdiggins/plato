# Standalone type inference experiment

This project contains a standalone type-term model and equality-constraint unification experiment. `BaseType.cs` defines type terms; `Constraints.cs` and `LegacyUnificationAlgorithm.cs` contain solving code.

The active compiler body checker lives in [`../Plato.Compiler/Checking/`](../Plato.Compiler/Checking/README.md). Keep this experiment distinct from that checker when tracing compilation behavior.

See the [compiler and language guide](../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
