# Body checking

This folder turns bound function bodies into typed intermediate representation, the resolved node model consumed by execution and code generation. `TypeChecker.cs` coordinates normalization, constraint generation, solving, and elaboration.

`Solver.cs` coordinates constraint solving and overload selection. `Solver.Coercion.cs` contains conversion matching, and `Solver.Unification.cs` contains type-variable binding and substitution resolution. These files share one partial class so method bodies retain access to the same solver state. `Tir.cs` defines the node model; the other `Tir*` files provide transformations, validation, and emission selection. `Monomorphizer.cs` specializes generic bodies.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
