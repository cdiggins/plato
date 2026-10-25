# Interpreter and law checks

These tests exercise evaluation, generated law inputs, witness selection, memoization and agreement with compiled C#. `TestSupport.cs` supplies shared compilation setup and `fixtures/` holds Plato inputs. `SandboxProbe.cs` is explicitly selected because it requires an external checkout. Run this project with `dotnet test` after building its dependencies.

Ordinary runs exclude tests tagged `Slow` or `Diagnostic`. See [test selection](../README.md#test-selection) for the full regression command and how to run slow checks separately.
