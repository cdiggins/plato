# BREP optimizer generation bottleneck — 2026-10-25

Tracking: [compiler-535](../../tracker/issues/compiler-535.md).

## Finding and correction

The complete C# recipe stalled while traversing an exponentially expanded reduction,
not while the inliner itself was running. `ModelMeshReport`, the BREP mesh-validation
report assembler, merges a literal array of reports. Inlining exposes a callback that
reads both fields of the accumulated report. Component expansion substituted that
accumulator twice at each fold step, including every previously expanded step.

A three-minute sampled-thread trace of the original frozen compiler attributed 72.47%
of samples exclusively to `TirNode.Descendants`; the loop lowerer's name collection
was a prominent caller. Temporary per-phase diagnostics on the current compiler
isolated `ModelMesh.ModelMeshReport`. It had 1,511 node occurrences after fusion and
approximately 8.541339190269978e17 after component expansion. Shared subtree references
kept the stored graph small (1,608 distinct nodes), but occurrence-based walks and
eventual expression emission could not exploit that sharing.

`TirComponentUnroller.Hof.TryFold` now applies the existing literal-fusion substitution
guard to both operands at every fold step. It refuses duplication of computed
accumulators/elements and repeated evaluation under lambdas. A refused fold remains
intact for ordinary loop lowering. Both component-field and literal-array reductions
use this helper. Single-use folds and delegate invocation remain supported. No pass
order, default, numeric grouping, or optimization flag changed.

The temporary diagnostic logging was removed. This correction does not claim to
resolve the other old-pass semantic questions tracked by compiler-483.

## Inputs and evidence

The original timeout and trace use `.temp/ob3/workspace.json`, including its hashed
compiler closure. The fixed compiler uses the same library/runtime source revision
`fd83e4a33b09fd2aa49e59e1b1bbb9f6cdb85a30` and BREP revision
`5057732c0c6858f71fac298535af203226f7f203`. The new benchmark workspace is
`.temp/ob535`; its manifest identifies the supplied fixed compiler separately from
those exported library sources.

The recipe is `--profile=cpu64 --inline --fuse --optimize --loops --fuse-consumers
--exact-output`. The recorded original attempt exceeded 900 seconds without output.
The fixed direct invocation generated successfully in 43.8 seconds. These are shared
machine observations, not a controlled speedup estimate. The benchmark's generation
field also includes generated-source checking and source publication; it is not
directly comparable to the direct CLI time.

Local evidence is retained under `.temp/compiler-535`: `baseline.nettrace`,
`baseline.speedscope.json`, `tree-sizes.log`, `fixed-generation.json`, generation and
checker logs, focused TRX, and gate logs. An initial fixed invocation finished emission
but failed publication because its output directory had not been created; the successful
timing above is the subsequent complete invocation. An initial sandboxed test run
could not connect to testhost; the rerun outside the sandbox executed the tests.

## Verification

| Check | Result | Seconds |
| --- | --- | ---: |
| Fixed CLI build, recorded | Pass | 17.5 |
| Focused generated-C# and optimizer tests | 28 passed | 12 |
| Direct full-recipe generation | Pass | 43.8 |
| Generated-source obligation checker | No degraded bodies or unapproved stubs | — |
| Benchmark full-recipe generation, checking and publication | Pass | 84.9 |
| Benchmark full-recipe C# build and host freezing | Pass; no compiler errors or warnings | 129.9 |
| Repository CLI build | Pass | 11 |
| Strict shipping-tier lint | Pass | 17 |
| Complete non-diagnostic PlatoTests, including both ratchets | 399 passed | 216 |
| Studio legacy and forward lint | Pass | 8.4 / 20.3 |
| Studio SDK build and geometry tests | Pass | 14 / 81.8 |
| Studio frozen-V1 checksum | Existing repo-473 failure | 0.9 |
| Greenhouse extraction, prose review, freshness, maintenance tests | Pass; six tests | — |

The new regressions reject exponential expansion before enumerating its result, then
compile and execute the generated C# with optimization off and on. They verify producer
evaluation before reduction callbacks, callback counts, producer faults, arithmetic
results, and retained expansion of single-use folds. Repository gate logs and TRX are in
`.temp/gate-logs/20261025-134339-797092`. Shared status-report files contained other
sessions' edits before this mission and are not included in the work commit.

The complete recipe is now a ready benchmark artifact, with the same frozen host
dependencies as the shipping reference. Its cache receipt is
`.temp/ob535/cache/b/5a7cfa5e0e27/complete.json`; the workspace identity is
`1c5d0b4de26a93eb6118558c8a1c540d1ce7bb99e386210d82405249a6c84d50`.
The underlying recorded C# build took 72 seconds; the table includes host freezing
and artifact hashing as defined by the benchmark. This verifies buildability, not
a whole-mesher runtime benefit from the complete optimizer recipe.

## Reproduction

Build the current CLI through `tools/dotnet-build-record.ps1`, then prepare and build
the benchmark with the pinned library inputs:

```powershell
python tools/benchmarks/optimizer/benchmark.py prepare --work .temp/ob535 --plato-ref fd83e4a33b09fd2aa49e59e1b1bbb9f6cdb85a30 --brep-ref 5057732c0c6858f71fac298535af203226f7f203 --cli src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll
python tools/benchmarks/optimizer/benchmark.py build --work .temp/ob535 --variants full --timeout 900
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter FullyQualifiedName~ReductionExpansionTests
```

The benchmark builds the shipping reference first to freeze host dependencies.
