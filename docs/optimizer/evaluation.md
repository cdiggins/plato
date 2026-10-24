# Evaluating the optimizer

[Overview](README.md) · [Pipeline worked example](../csharp-optimizer-pipeline.md)

## Evidence has different strengths

| Evidence | What it establishes | What it does not establish |
|---|---|---|
| TIR dump and generated-text assertion | A rule fired and produced a particular shape | Valid C# or equal behavior |
| Compiling the emitted C# | Types and runtime calls link | Equal results or faster execution |
| Executing paired variants | Agreement on the tested inputs | Equivalence on every input |
| Allocation/time experiment | Benefit on a specified workload and environment | A universal speedup |

The [optimization test folder](../../tests/PlatoTests/Optimization) contains both structural tests
and execution tests. In particular, `FlatMapFilterTests.SingletonAllocationIsRemovedAgainstTheSameLoweredPath`
compares allocation against the same lowered baseline, isolating the singleton-arm specialization.
Its assertions and experiment are the authority; this durable guide deliberately does not copy
timings, allocation totals or passing-test counts.

The [2026-10-24 validation report](../reports/optimizer-handbook-validation-2026-10-24.md)
records a controlled allocation comparison and the focused test run used for this handbook.

## Repeatable commands

From the repository root, run the focused tests (replace the filter with one class for an inner loop):

```powershell
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter 'FullyQualifiedName~InlinerTests|FullyQualifiedName~CombinatorFusionTests|FullyQualifiedName~LiteralUnrollingTests|FullyQualifiedName~ComponentUnrollerTests|FullyQualifiedName~ArrayMaterializerTests|FullyQualifiedName~LoopLowererArrayTypeTests|FullyQualifiedName~FlatMapFilterTests|FullyQualifiedName~TirRewriteTests'
```

For a small self-contained Plato input folder, compare generated variants:

```powershell
dotnet run --project src/Plato.CLI -c Release -- .temp/optimizer-input --out=.temp/optimizer-off
dotnet run --project src/Plato.CLI -c Release -- .temp/optimizer-input --out=.temp/optimizer-on --inline --fuse --optimize --loops --dump-tir=.temp/optimizer-tir --inline-report
```

The input must include the declarations and intrinsics its functions need. CLI input roots are
enumerated at the top directory only. A successful CLI exit is insufficient: check for
`DEGRADED bodies` and compile the generated artifact against the live runtime. For a buildable
experiment project, retain its build result through the repository's recorder:

```powershell
powershell tools/dotnet-build-record.ps1 -Project .temp/optimizer-experiment/Experiment.csproj -TargetName optimizer-experiment
```

The [smoke harness](../../tests/optimizer-smoke/README.md) offers existing generated-project
recipes, but its recipes and runtime compatibility must be checked before reusing it. In
particular, do not infer that `--optimize-arrays` is usable because a generation step succeeds.

## Performance experiments

Use identical source, inputs, runtime, build configuration and scalar representation for both
variants. Change one flag or one rule at a time, then test the intended combination. Warm up
generated code, consume outputs, repeat samples, and retain both elapsed time and allocated bytes.
Measure compiler cost and output growth separately from execution cost. Compare numeric results
under the workload's declared tolerance; do not hide a correctness failure inside a speedup.

Include empty, singleton, small fixed and large dynamic arrays; early and late Boolean exits;
capturing and noncapturing callbacks; reused mapped values; and accepted/refused source shapes.
Add domain-fault and evaluation-order cases where a transformation changes producer completion.
Record commit, source fixture, commands, SDK/runtime, machine and raw outputs in a dated report
under `docs/reports/`. Link that report from the affected component if it supports a lasting claim.

There is no single trustworthy effectiveness percentage for this pipeline. Each component page
identifies the overhead it can remove and the cases it leaves intact. The
[sequencing investigation](../../tracker/issues/compiler-483.md) limits broad correctness claims
until its hypotheses have been tested.
