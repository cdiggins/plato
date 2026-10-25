# Opt-in loop optimizations — 2026-10-25

The production continuation implements the bounded slices of compiler-506 (multiple-consumer
fusion) and compiler-509 (exact FlatMap result allocation). Both default to off. This report
distinguishes the actual compiler work from the earlier handwritten microkernels.

## Behavior and architecture

`--loops --fuse-consumers` recognizes a completed local Number array followed immediately by
independent reductions whose callbacks use only total Number literals, parameters, Add and
Multiply. The producer remains eager and each fold retains its arithmetic order. Generated
loops change from three to two for the MapRange fixture, or two to one for a literal source.
Foreign arrays, dependent results, assignments, unknown calls and uncertain conversions refuse.

`--loops --exact-output` sizes a FlatMap result when its raw callback returns a fixed-width
array literal. The runtime source must be an array and the widened size must fit the runtime
capacity bound; otherwise it retains the growing-list path. Callback arrays and their complete
evaluation remain, including source reads and faults. Mutable array contents are supported;
stable length does not imply immutable values.

Shared code now retains resolved intrinsic identity in lowered loops and checks membership in
the actual intrinsic library. It does not add another name resolver. Dumping and ordinary
emission use one scheduling sequence. Small immutable API options expose both switches through
generation and compiled-law APIs; the run-command CLI does not expose these two new options.

Independent source review found and corrected name collisions involving unused lambda
parameters. It also requested an executable combined-flags test, which passed. A generated-code
size limit bounds duplication of exact and fallback callback branches. The new analysis does
not change existing optimizer recognition rules or the runtime intrinsic surface.

## Verification

| Check | Result | Seconds |
|---|---|---:|
| Existing loop/filter/statement baseline | 24 passed | 14 test duration |
| New exact-output cases | 17 passed | Part of mixed 27.16-second run |
| New consumer-fusion cases | 13 passed | 64.79 |
| Writer build | Passed | 4.46 |
| Laws API build | Passed | 3.68 |
| CLI build with existing dependency binaries | Passed | 5.65 |
| Normal CLI build, with dependency access | Passed | 52.67 MSBuild duration |
| CLI generation and invalid-option checks | Passed | Not separately timed |
| Default-off generated output comparison | 284 files identical | Not separately timed |
| Greenhouse source extraction and documentation check | Passed on isolated source snapshot | Not separately timed |
| Documentation tooling tests | 6 passed | 0.395 |

Focused execution compiles actual Plato fixtures and generated C# using default C# language
version and Release optimization. Fusion tests need generated Number Add/Multiply methods:
the harness adapts their emitted instance receivers to extensions because a partial struct
cannot extend the embedded test runtime from another assembly. The operator bodies are the
writer's real generated bodies. This is not a full generated-foundation application build.

The default-off comparison used a private copy of the pre-change CLI binaries and the new CLI,
with the foundation plus both fixture directories and `--loops`. Neither new option was enabled.
The comparison script allows removal of timestamp comment lines; no semantic normalization is used.

Earlier non-escalated builds could not access the machine-relative SDK dependencies and reported
MSB3030 (missing Ara3D.Utils.dll) and MSB3202 (missing sibling project). The normal build subsequently
passed with dependency access. The underlying checkout-layout coupling is already compiler-471.
Testhost IPC and Python temporary-file access also required environment adjustments; those failed
attempts are not behavioral test failures.

The shared checkout contains a concurrent inliner refactor. Documentation evidence was generated
from committed sources plus this work's explicit file list, excluding that unfinished refactor.
Private evidence lives under `.temp/optimizer-exploration/production/`; binary fingerprints and
the exact tested outputs identify the validation scope. The broader integration results are
recorded below when completed; they must not be inferred from the focused checks.

## Measured benefit and limits

The actual exact-output wrapper test uses an input of 4,096 elements and literal width two,
32 warmup calls, and eight measured calls. Allocated bytes per call fell from 295,200 to
229,400: a 22.29% reduction. Callback arrays remain included. This is allocation evidence for
one generated workload, not an elapsed-time claim or a geometry-wide speedup.

Fusion correctness includes empty input, signed zero, NaN, infinities, producer call counts and
faults. Neither pass reassociates floating-point arithmetic. Resource-exhaustion equivalence,
GPU32 performance and representative geometry-wide speedups are not established by this suite.
Practical tests do not allocate arrays near Array.MaxLength; the widened capacity guard is
reviewed and the custom-list fallback is executed separately.

The [secondary census](optimizer-secondary-tir-2026-10-25.md) advances the other four ideas to
actual TIR shapes and emission evidence. Record elimination still lacks measured benefit;
array facts need producer/alias provenance; unswitching needs capture-aware invariant analysis
and a useful workload; grid reuse lacks a proved nested schedule and callback totality. No
speculative production switches or general solver were added for those tracks.

## Reproduction

After building the compiler and PlatoTests, run:

```powershell
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --no-build --filter 'FullyQualifiedName~MultiConsumerFusionTests|FullyQualifiedName~ExactOutputTests'
dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --no-build --filter 'FullyQualifiedName=PlatoTests.Optimization.MultiConsumerFusionTests.GeneratedReleaseTraversalTiming' --logger 'console;verbosity=normal'
python tools/experiments/optimizer-exploration/secondary-census/probe.py
python tools/optimizer-docs/update.py check
```

To generate C#, add either or both `--fuse-consumers --exact-output` alongside `--loops` to an
existing C# recipe. Remove the relevant flag to disable the optimization. Omitting both preserves
the existing loop path. The CLI rejects their use without loops or with another target language.
