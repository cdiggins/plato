# Syntax formatting and export

This folder renders abstract syntax trees for source formatting, declaration export, and inspection. `AstWriterPlato.cs` writes source; `PlatoDeclarationWriter.cs` writes declaration-focused output; `AstWriterXml.cs` writes XML.

`PlatoFormatOptions.cs` defines writer options and `PlatoTextCompaction.cs` handles compact text output. These files preserve the `Ara3D.Geometry.AST` namespace so consumers use the same public API regardless of their source location.

See the [compiler and language guide](../../../docs/plato-for-agents.md) for the pipeline and repository commands. This guide describes responsibilities; build and test results belong in the repository gate logs.
