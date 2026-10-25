# Prepared Plato optimizer benchmark

This tool compares complete C# optimization recipes on geometry and meshing work
used by plato-brep. It prepares inputs and builds artifacts separately, then measures
repeated operations without STEP parsing, file access or result validation inside
the timer. It is intended for compiler developers deciding whether a recipe or
parameter change helps representative workloads.

Start from the Plato repository root in PowerShell:

```powershell
python tools/benchmarks/optimizer/benchmark.py matrix
python tools/benchmarks/optimizer/benchmark.py prepare --work .temp/ob
python tools/benchmarks/optimizer/benchmark.py build --work .temp/ob
python tools/benchmarks/optimizer/benchmark.py fixtures --work .temp/ob --input-list tools/benchmarks/optimizer/corpus.json --output .temp/ob/suite.json
python tools/benchmarks/optimizer/benchmark.py discover --work .temp/ob --suite .temp/ob/suite.json --output .temp/ob/discovery.json
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob --suite .temp/ob/suite.json --discovery .temp/ob/discovery.json
```

The last command prints a Markdown report path. The adjacent JSON report contains
comparison details, and `samples.jsonl` preserves every raw sample and failure.
Use `--help` on any subcommand for its options. Prefer a short workspace path on
Windows because the exported projects contain long generated filenames.

## Inputs and reproducibility

Prerequisites are Python, Git, PowerShell, the .NET SDK required by the exported
projects, a built Plato CLI and a plato-brep checkout with its pinned MeshKernel
repository and SDK package feed available. The default compiler DLL is the Release
output under `src/Plato.CLI`; `prepare --cli` selects another compiler build.
Generation uses the `cpu64` profile and the worker targets .NET 8. Results apply to
that C# backend, numeric profile and recorded runtime.

Preparation exports committed library and runtime sources at `--plato-ref` and
`--brep-ref`, defaulting to HEAD. Dirty source edits are excluded. The supplied
compiler binary directory is copied and hashed: that proves binary identity, but
does not establish which source revision produced it. The preparation manifest
records that distinction. To test a compiler edit, first build the edited compiler
and supply its DLL. To test a library edit, commit it before preparing.

All generated projects and builds reside under `--work`. The sibling plato-brep
checkout is read only. Source, package and compiler identities determine cache
entries; cache receipts detect missing, added and changed files. A valid artifact
is reused. A corrupted completed cache is rejected rather than trusted. Retain
failed build logs when investigating a configuration that does not compile.

The worker and dependency binaries are frozen from the shipping build. Only the
generated `Ara3D.StepBrep` library varies between recipes. Each build is recorded
through Plato's C# build recorder, including compiler errors. Generation rejects
degraded bodies and new bodiless stubs through plato-brep's committed checker.
Existing explicitly declared host obligations, including STEP entry points supplied
by the host before timing, retain that project's existing baseline; the benchmark
adds no exemptions. A failed recipe remains a failed
recipe in the run; it is never replaced with a less optimized fallback.

## Fixture preparation and workload boundaries

The checked-in `corpus.json` supplies portable witness models spanning analytic and
spline geometry and meshing paths. It is a seed corpus; add actual application
models before drawing production conclusions. Use repeated `fixtures --input <file.step>` arguments to prepare specific files,
or `--corpus <directory>` to enumerate STEP files recursively. Duplicate source
contents are prepared once. `--max-inputs` bounds setup during experimentation;
it is an input-order limit and does not select representative models. Use
`--input witness:Name` for a built-in model; `catalog --work .temp/ob` lists
witness names. `--chordal` fixes the meshing tolerance in model
units. Use different suite output names for different input or tolerance choices.

Preparation saves the decoded/prepared model, cached stage inputs, discretizations,
face outcomes, final refinement state and evaluation samples. Wrapper and sum
values are reconstructed through their constructors; preparation requires an exact
serialization round-trip. Fixtures carry their input, preparation-library and
content identities. The same fixture is used by every recipe. A run verifies the
entire artifact tree against its build receipt and requires the suite's preparation
closure to match a verified build. Re-prepare fixtures after changing that closure.

| Workload | Work inside the timer |
|---|---|
| `model` | Topology and projection caches, tables, edge and face work, blocking refinement, numbering and assembly. |
| `projection-grids` | Construction of projection caches from the prepared model. |
| `edges` | Edge discretization from saved model inputs and precomputed tables. |
| `faces` | First-pass face outcomes using saved edge discretizations. |
| `refinement` | Blocking refinement using saved first-pass outcomes and edges. |
| `assembly` | Vertex numbering and assembly using saved final outcomes. |
| `curve-eval` | Evaluation at deterministic parameters on supported model curves. |
| `surface-eval` | Evaluation at deterministic parameters on supported model surfaces. |
| `projection` | Inversion of deterministic offset points onto supported surfaces. |

Stages with no applicable work are omitted. Tags distinguish geometry kinds,
meshing paths, input-size bins and spline characteristics. Coverage reflects those
tags, not a claim to exercise every function or every possible input.

Whole-model work uses sequential initial edge and face processing to expose the
generated compiler code's cost. The shipped blocking-refinement implementation
retains its own scheduling. This is a prepared-model benchmark; separate application
measurements are needed for STEP ingestion, cold startup and parallel throughput.

Stage workloads use saved prerequisites from the reference build. They explain
local changes under identical inputs; they do not replace the whole-model workload,
which recomputes the interacting stages. Never add their times to model times.

## Recipes and parameters

`matrix` is the authority for the full recipe, its feature removals, the shipping
reference and the unoptimized reference. The shipping flags are read from the
committed plato-brep regeneration script. Removing loop lowering also removes its
dependent consumer-fusion and exact-allocation options. Disabling the fusion pass
still permits fusion implemented within loop lowering. The array-materialization
flag is explicitly unsupported because its helper names do not match the live
runtime; see the [materialization guide](../../../docs/optimizer/materialization.md).

Use `build --variants shipping,full,no-inline` to build a subset while iterating.
Use the same `--variants` filter on `run` to measure a subset of the saved builds.
Subset builds retain earlier recipes when their source workspace and frozen host
match. A changed host or workspace starts a new saved set; shipping is always built
or verified because it supplies the common host. The build exit status describes
the requested build, while the manifest also retains earlier compatible failures.
Quick mode defaults to full, shipping and unoptimized references; use
`run --variants all` to screen every ablation, or name the variants of interest.
Confirm and corpus modes use all saved variants unless filtered explicitly.
Exact artifact aliases reuse measurements and retain their original recipe labels.

Experimental inlining policies and growth sweeps require `--experimental`:

```powershell
python tools/benchmarks/optimizer/benchmark.py matrix --experimental --inline-policy statements --inline-growth 2048 --inline-growth 8192
```

Pass those options to `build` to create the displayed recipes. Growth comparisons
hold the statement-inlining policy fixed; compare growth variants against that
policy's default configuration. The default full recipe keeps experimental policy
changes out of ordinary feature-removal comparisons.

An ablation measures the contribution of a feature in the complete recipe. It does
not establish an independent effect or resolve interactions between disabled
features. Add a targeted paired experiment when evidence suggests such an interaction.

## Selection, budgets and interpretation

`discover` records setup-inclusive invocation costs for the supplied fixtures.
Quick selection greedily covers geometry, path, size and workload dimensions per
cost, reserving a deterministic holdout. With discovery it estimates requested
warmup and batch costs and accounts for variant and block counts when fitting the
selection to the run budget. The estimate is not a runtime guarantee. Without discovery it uses uniform costs
and labels the estimate as unknown. Redundant tag coverage does not earn another
fixture a place merely because it came from another file.

Preparation failures remain explicit in the suite and run report, outside measured
coverage. A partial preparation cannot quietly become a complete corpus result.

The mode defaults live in `benchmark.py:MODES`. Quick mode screens for promising
changes and obvious regressions; its default repetition count cannot establish
an effect classification. Confirm mode requires affected case IDs or workload
names and spends more repetitions on them. Corpus mode includes the reserved
fixtures. The report lists follow-up case IDs and workloads for these commands:

```powershell
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob --suite .temp/ob/suite.json --mode confirm --affected projection
python tools/benchmarks/optimizer/benchmark.py run --work .temp/ob --suite .temp/ob/suite.json --mode corpus
```

`--seconds` bounds the run's measurement phase, including worker startup, fixture
loading, warmup and checking. An individual process is additionally limited by
`--timeout`. A timeout applies to the fixture's workload set; completed earlier
stages in a timed-out process are unavailable because it emits its response when
the set finishes. Expired budgets produce explicit unmeasured rows, not silently reduced
coverage. Preparation and builds have separate costs. A measurement lock prevents
this tool from rebuilding its artifacts during a run; it cannot exclude unrelated
programs or other repository sessions. Run performance measurements on an idle
machine. `--affinity` accepts a Windows CPU mask for selecting a consistent CPU class.

Each process loads one fixture and executes its selected workloads. Case, variant
and workload order is shuffled reproducibly; workload order is shared across
variants within a process block. Warmup honors both invocation and duration
limits, then a pilot calibrates the reported batch. Actual warmup, iteration counts,
runtime settings and binary hashes are recorded. The worker uses the monotonic
clock and process-wide allocation counter. No forced garbage collection is inserted
between operations; collection costs inside the measured batch are included.

Time and allocation totals are normalized by actual invocation counts. Comparisons
pair process blocks and use an interval on log time ratios. The practical effect
threshold and minimum block count live in `analysis.analyze`. Small or noisy results
remain inconclusive. Intervals describe an individual comparison under the stated
sampling assumptions; exploratory comparisons across a broad suite need confirmation.

Reports balance stage families, then report whole-model results separately. A
partial family summary is marked incomplete. Digest disagreement, failed checks,
duplicate/missing pairs, nonfinite measurements and timeouts cannot yield a speedup.
First-pass face checks compare the prepared first-pass contract verdict because
pending refinement is an expected intermediate outcome; final mesh checks enforce
the completed contract. Exact result matching is conservative: investigate any
numerical difference before introducing a tolerance.

Reanalyze raw samples without running geometry again:

```powershell
python tools/benchmarks/optimizer/benchmark.py report --samples <run-directory>/samples.jsonl --output <run-directory>/reanalysis.md
```

The `validate` command compares selected and reserved cases descriptively when
both have sufficient evidence, after checking suite, binary and actual runtime
identities. Use matching target, warmup and iteration-limit options in both runs:

```powershell
python tools/benchmarks/optimizer/benchmark.py validate --quick <selected-run-directory> --corpus <corpus-run-directory> --output .temp/ob/prediction.json
```

Neither selection nor a small
successful run proves that the quick suite predicts unseen corpus behavior. Add
reserved cases when they reveal a missed family or regression and retain a new
holdout for subsequent tuning.

Run the tool's tests with:

```powershell
python -m unittest discover -s tools/benchmarks/optimizer -p 'test_*.py'
```

Set `PLATO_BENCHMARK_WORKER` to a built worker DLL to include executable fixture,
geometry, meshing and invalid-input checks. Without it those integration checks
are explicitly skipped.

The implementation and verification record is tracked by
[compiler-522](../../../tracker/issues/compiler-522.md).
