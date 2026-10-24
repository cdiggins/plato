# Compiler front end

This project turns parsed Plato source into bound symbols and checked, typed function bodies for interpreters and code writers. Start with `Compilation.cs` for symbol binding and shared compilation state; `Checking/TypeChecker.cs` runs the body-checking passes.

`Symbols/` owns binding and the resolved source model. `Analysis/` computes declaration and implementation relationships. `Types/` resolves and compares type expressions. `Checking/` owns body checking and typed intermediate representation. `Vsg/` exposes a visual syntax graph, while `Utilities/` holds diagnostic and hashing support.

See the [compiler and language guide](../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
