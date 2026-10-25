# Prepared optimizer benchmark verification — 2026-10-25

The executable benchmark is in `tools/benchmarks/optimizer`; its README documents
setup, workload boundaries, recipes, selection, correctness checks and interpretation.
This report records the integration experiment rather than establishing a preferred
compiler recipe. Measurements on this shared development machine require a separate
idle-machine confirmation before guiding production defaults.

## Frozen inputs

The experiment workspace is `.temp/ob3`. Its source preparation key is
`2f445bbef8e4ce82d6d1e53763c59dd33f86a6bdb7a2cd5754155ec1f8261236`.
The exported Plato library/runtime revision is
`fd83e4a33b09fd2aa49e59e1b1bbb9f6cdb85a30`; plato-brep is
`5057732c0c6858f71fac298535af203226f7f203`. The workspace manifest records
MeshKernel and SDK pins and every supplied compiler/package hash. Compiler binaries
are separate identified inputs; their source provenance is not inferred from the
library revision. Generation uses `cpu64`; execution targets .NET 8.

The shipping recipe is `--loops --fuse`. The complete recipe adds `--inline`,
`--optimize`, `--fuse-consumers` and `--exact-output`. The unoptimized reference
disables optional passes and anchors cross-recipe output comparisons.

## Verified checks

| Check | Result | Seconds |
|---|---|---:|
| Benchmark tests, including executable generated-code integration | 77 passed; one symlink-privilege skip | 18.7 |
| Recorded CLI build | Passed | 16 |
| Recorded forward lint | Passed: no errors | 15 |
| Recorded PlatoTests, including ratchets | 396 passed | 207 |
| Studio legacy lint | Passed | 22.7 |
| Studio forward lint | Passed | 22.2 |
| Studio SDK build | Passed | 10.9 |
| Studio geometry tests | Passed | 24.4 |
| Studio frozen-V1 checksum | Failed, existing `repo-473` | 2.5 |
| Optimizer documentation freshness | Passed | — |
| Independent fixture-reader probe build | Passed: no errors or warnings | 11.2 |

`tools/record-gates.py` wrote the repository snapshot and gate-log entry. The
studio battery's retired checksum failure is documented by the existing issue
and retirement decision; benchmark work changes none of its SDK runtime inputs.
Logs are `.temp/optimizer-record-gates.log` and `.temp/optimizer-check-all.log`.

The executable tests verify deterministic box and spline fixture round trips,
finite geometry, strict final meshing contracts, exact repeat results, and unchanged
borrowed inputs. Unit tests exercise recipe dependencies, cache corruption and source
mutation, fixed host artifacts, missing and duplicate pairs, budget exhaustion,
malformed worker output, preparation failures, and holdout reporting.

## Experiment

The complete recipe exceeded its 900-second generation limit before emitting C#.
No worker artifact or runtime timing exists for that configuration. The failure is
captured in `tracker/issues/compiler-535.md`; profiling and correcting compiler
generation is separate from this benchmark implementation. Removing inlining is
a diagnostic configuration, not evidence of an independent inlining effect.

| Recipe | Generation, checking and source publication (seconds) | Build and host freezing (seconds) | State |
|---|---:|---:|---|
| Shipping | 181.7 | 53.0 | Ready |
| Unoptimized | 327.8 | 131.6 | Ready |
| Complete with inlining disabled | 247.3 | 155.1 | Ready |
| Complete | Generation limit: 900 | Not run | Failed |

These setup fields exclude source copying and final cache sealing. They are not
geometry-operation times. Every successful generation passed plato-brep's committed
checker with no degraded bodies or unapproved bodiless declarations. The checker
retains only the project's existing declared host obligations, such as STEP entry
points provided by the host before timing; no baseline exceptions were added.

The portable seed corpus was supplemented with the STEP Tools `block.stp` and
the UBlox JODY-W4 application model. All nine models prepared successfully; JODY-W4
has 2,621 faces. Discovery exercised every applicable workload successfully before
selection. STEP ingestion, fixture construction and serialization happened during
setup, rather than inside any geometry timer.

| Run | Scope | Process blocks | Timed batches | Result | Worker process time (seconds) |
|---|---|---:|---:|---|---:|
| Quick | Four selected models; three built recipes | 5 | 465 | All correctness checks and output comparisons pass | 151.4 |
| Confirm | Spline Crescent projection only; three recipes | 5 | 15 | All checks and comparisons pass | 12.9 |
| Corpus | All nine models; three recipes | 1 | 198 | All checks and comparisons pass | 147.5 |

The batch counts include each recipe/workload/process-block combination, not just
the model count. Worker process time includes loading, warmup, calibration and
checking; the raw rows separately contain geometry-operation nanoseconds and bytes
allocated per batch. Stage times are never added to whole-model times.

These runs explicitly use `no-inline` as the timing control, with the unoptimized
recipe as correctness anchor. They do not substitute a timing for the unavailable
complete recipe. All compared output digests agree across recipes, all meshes pass
the applicable contracts, and no borrowed input mutation was detected.

The quick selection retained the two spline cases, the through-hole case and the
STEP block. It reserved the sphere and reported uncovered circle/cylinder/torus,
large-model and spline-degree dimensions. Its individual exploratory comparisons
are mostly inconclusive. The corpus run deliberately uses a single block to check
broad correctness, so all its timing classifications remain inconclusive. The
holdout-validation command correctly returns `insufficient_evidence` and
`prediction_validated: false`. This experiment does not validate corpus prediction
or select a production optimization recipe.

## Portable setup and retained evidence

Fixture compatibility is based on exported dependency contents and package bytes,
not the candidate compiler hash or whole Plato commit. The original preparation
artifact remains independently verified. A real check with that artifact excluded
from the candidate set passed. The standalone reflection probe also round-tripped
older box and spline fixtures through different generated libraries byte for byte.
Unit tests reject changed source/runtime contracts, changed fixtures, and a tampered
original reference.

Candidate-reader validation is cached by fixture, complete candidate closure, probe
and runtime settings. Confirmation reused all its reader checks in 0.07 seconds.
The corpus reused twelve checks and performed fifteen new ones during setup; none
of that validation is inside geometry-operation timers.

[The evidence manifest](data/compiler-522-2026-10-25/summary.json) records binary,
source, tool and raw-sample hashes; inputs, coverage, runtime settings, classifications
and setup costs. Compressed raw samples are retained for
[quick](data/compiler-522-2026-10-25/quick-integration.samples.jsonl.gz),
[confirmation](data/compiler-522-2026-10-25/confirm-integration.samples.jsonl.gz) and
[corpus](data/compiler-522-2026-10-25/corpus-integration.samples.jsonl.gz).
Saved-data reanalysis reproduced the corpus comparisons without executing geometry.

## Rerun

The existing `.temp/ob3` workspace retains the identified compiler, source snapshots,
original reference build, candidates and prepared suite. Use new run names:

```powershell
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob3 --suite .temp/ob3/suite.json --discovery .temp/ob3/discovery.json --mode quick --name quick-repeat --variants shipping,unoptimized,no-inline --baseline no-inline --seconds 180 --blocks 5 --max-cases 4 --target-ms 20 --warmup 1 --warmup-ms 50 --timeout 120
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob3 --suite .temp/ob3/suite.json --discovery .temp/ob3/discovery.json --mode confirm --name confirm-repeat --variants shipping,unoptimized,no-inline --baseline no-inline --seconds 180 --blocks 5 --max-cases 1 --affected projection --target-ms 20 --warmup 1 --warmup-ms 50 --timeout 120
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob3 --suite .temp/ob3/suite.json --discovery .temp/ob3/discovery.json --mode corpus --name corpus-repeat --variants shipping,unoptimized,no-inline --baseline no-inline --seconds 600 --blocks 1 --target-ms 20 --warmup 1 --warmup-ms 50 --timeout 180
python tools/benchmarks/optimizer/benchmark.py validate --quick .temp/ob3/runs/quick-repeat --corpus .temp/ob3/runs/corpus-repeat --output .temp/ob3/prediction-repeat.json
```

For a new workspace, follow the benchmark README, supplying an identified CLI and
the source revisions above. Add the application STEP inputs explicitly. Existing
compatible suites can be used after compiler-only changes without parsing STEP
again; retain their original reference artifacts. Increase repetitions and warmup
for an actual optimization decision and resolve the complete-recipe generation
failure before making claims about its feature removals or parameter choices.
