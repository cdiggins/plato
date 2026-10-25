# Compiler and writer checks

Tests are grouped by responsibility: [Checking](Checking/README.md), [Parsing](Parsing/README.md), [Emission](Emission/README.md), [Optimization](Optimization/README.md), and [Contracts](Contracts/README.md). `CheckerTestSupport.cs` locates source fixtures and constructs compilations. Snapshot and inventory tests remain at this level beside their text baselines because they use source-relative paths. `input/` and `output/` preserve historical parser fixtures. Run `dotnet test tests/PlatoTests/PlatoTests.csproj -c Release` from the repository root; use `--filter` for a focused test class.

`CSharpTestCompilation.cs` compiles generated C# in memory against the runtime embedded in the test assembly. Emission and optimizer fixtures share its reference selection and diagnostics; callers specify language and optimization options when those are part of the behavior under test. It excludes the legacy geometry package because that package duplicates the runtime scalar types.

Test selection and the commands for routine, slow, and full runs are documented in
[the verification guide](../README.md#test-selection). The full run retains both stdlib ratchets.
