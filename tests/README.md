# Verification projects

Each project tests the production component named in its directory. [PlatoTests](PlatoTests/README.md) covers compiler and writer contracts; [interpreter tests](Plato.Interpreter.Tests/README.md) cover execution and laws; [navigation tests](Plato.Navigation.Tests/README.md) cover source queries. Fixture directories beginning `plato-test-` hold input programs, while `conformance/` and `optimizer-smoke/` hold generated-program harnesses. Run existing project tests after the relevant source builds; gate policy lives in [stdlib/VERIFICATION.md](../stdlib/VERIFICATION.md).

## Test selection

The compiler, interpreter, and navigation projects load `tests/default.runsettings` for ordinary `dotnet test` runs. It excludes
`Slow` (whole-library checks, emission inventories, and expensive compiled law runs) and `Diagnostic` (reports without
regression assertions). Small parser, checker, emitter, optimizer, interpreter, and navigation regression cases stay
in the default run. A command-line filter replaces this default selection:

```powershell
# Routine regression checks.
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release
# All regression checks, including slow checks and both stdlib ratchets.
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter 'TestCategory!=Diagnostic'
# Slow checks alone.
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter 'TestCategory=Slow'
# Diagnostic reports (also marked Explicit so unfiltered runners skip them).
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter 'TestCategory=Diagnostic'
```

Use the full regression command before landing compiler or stdlib changes.
`python tools/record-gates.py` always selects that full regression set, and
`tools/check-stdlib-fast.ps1` selects the checker ratchet by name. Neither gate relies on
the default selection. Recorded compiler runs also save per-test timings in
`.temp/gate-logs/<run>/compiler-tests.trx`. In an IDE, select `tests/default.runsettings` if the runner does not
honor the project's `RunSettingsFilePath` property. Running the assembly directly without
settings includes slow tests.

Classify tests by work performed, not a fixed machine-dependent duration. Keep small
regressions enabled even when they compile generated C#; use `Slow` for corpus-wide
checks that have focused counterparts. Prefer a minimal reproducer when a whole library
is only being used to obtain one declaration. To reassess cost, add
`--logger 'trx;LogFileName=timings.trx' --results-directory .temp/test-speed` and inspect
the recorded per-test durations. No tests are deleted merely for being slow.

Replace the project path in these commands with `tests/Plato.Interpreter.Tests/Plato.Interpreter.Tests.csproj` or `tests/Plato.Navigation.Tests/Plato.Navigation.Tests.csproj` for those components. Run their full regression sets before landing changes to those components; `record-gates.py` covers the compiler project only. Backend-parameterized law tests keep their interpreter case enabled and tag expensive compiled cases separately.
