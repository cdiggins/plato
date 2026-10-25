# Aggressive inlining measured with plato-brep — 2026-10-24

Experiment tracked by [compiler-498](../../tracker/issues/compiler-498.md).
The compiler repair it exposed is [compiler-499](../../tracker/issues/compiler-499.md).

## Scope and controls

The experiment varies Plato's source-level inliner, before fusion and loop lowering,
while keeping `--loops --fuse --profile=cpu64`, the mesher, input models, and runtime
fixed. Generated C# already carries .NET `AggressiveInlining` annotations; the
baseline therefore includes the JIT's own inlining. These measurements concern
additional expansion in Plato, not turning all machine-code inlining on or off.

Inputs were exported from committed sources, without using concurrent working-tree
edits:

- Plato: `763034a67612fb1ada95891434cba8fc2ecf5707`.
- plato-brep: `99c8fdee6be8d3fc92f45401321d442e15375316`.
- MeshKernel, the C# mesh-host dependency: `047670a70232826f0f30c1112d327cff6c05a9ff`.
- Ara3D SDK: `98a90590b7f3b964c2a9bdbd01dba24ffe33fab0`; the compiler's Parakeet
  parser dependency: `f4f9a8af7fce930c03b75d990cc927070192133a`.

The compiler's dependency-closure builder restores locked packages and verifies its
snapshot. Experimental writers replace only the copied `TirInliner.cs` budget
constants. Type/binding checks, recursion prevention, and statement/lambda restrictions
remain enabled. No production optimizer source or default was changed.

## Policies and expansion

The cost threshold is exclusive: a compound argument may be duplicated only when
its estimated cost is below that value. Cheap leaves follow the existing rules.
The expansion counts include the entire generated program, including laws and
witnesses; they are not counts of calls executed while meshing.

| Policy | Callee body nodes | Pass cap | Duplication cost threshold | Calls expanded | Full generated C#, decimal MB |
|---|---:|---:|---:|---:|---:|
| Baseline: no Plato inlining | — | — | — | 0 | 17.499 |
| Current inliner | 120 | 8 | 4 | 62,785 | 25.284 |
| Larger bodies | 480 | 16 | 4 | 63,034 | 25.792 |
| Much larger bodies | 2,000 | 32 | 4 | 63,034 | 25.792 |
| More duplication | 2,000 | 32 | 32 | 63,471 | 25.834 |

The 480/16 and 2,000/32 outputs are byte-identical across every generated file.
Increasing the body/pass budget from 480/16 to 2,000/32 therefore changes no generated
code on this input. The most aggressive policy adds only about 1.1% more expansions than
the current policy. Size limits are not the principal barrier encountered here.

## Full-program compilation blocker

The no-inline baseline compiles. Every full inline policy passes the project's
throwing-stub check but fails C# compilation with the same 1,862 distinct diagnostics
in the same 51 generated files:

| C# diagnostic | Count | Representative failure |
|---|---:|---|
| CS1955 | 1,499 | `NumberInterval.End` is a field, but emitted code calls it as a method. |
| CS0030 | 357 | Emitted code casts a `Boolean` wrapper to an `Integer` wrapper. |
| CS1061 | 3 | Emitted code asks `VertexIndex` for `Orientation`. |
| CS1501 | 2 | Emitted code calls `Distance` with no arguments where no such overload exists. |
| CS1503 | 1 | A conditional expression cannot convert to `VertexIndex`. |

These errors exist with the current inliner, before relaxing any budget.
[Compiler-499](../../tracker/issues/compiler-499.md) records representative locations
and the required repair. Original generated outputs and full build logs are retained
under `.temp/inline-brep` and `.temp/csharp-build-logs/inline-brep-*-host.log`.

## What the partial builds mean

To obtain executable experiments without silently repairing unrelated compiler bugs,
each inline variant retains the baseline source for the **same** 51 failing generated
files. All other generated files come from that variant. This is an experiment-only
fallback, not an allowlist added to the compiler or a claim that full inlining works.

The excluded files include important code: `Curves.g.cs`, `EdgeDiscretizing.g.cs`,
`_BrepModel.g.cs`, `_RawDecodedModel.g.cs`, and scalar operations, as well as laws.
Consequently the measurements apply to this compilable subset; they cannot establish
the performance of a fully repaired inliner. The data file records the complete
exclusion set, rather than estimating coverage from a percentage of files.

Every partial variant must pass a full host build. Timing then uses frozen baseline
host/dependency binaries with only `Ara3D.StepBrep.dll` exchanged. A preflight hash
check rejects any other managed-DLL difference or a stale/failed candidate. This also
prevents enclosing-checkout Git metadata from making concurrent commits part of the
comparison.

## Measurement protocol

The existing plato-brep `step-corpus-harness time` command meshes ODIN-W160,
ELLA-W1, and JODY-W3 sequentially. Each fresh process performs one full warm-up,
then one measured run. Policies are shuffled within each model/repetition using a
recorded seed. The decisive comparison collects three repetitions of baseline,
current-policy partial inlining, and most-aggressive partial inlining on each model.

An initial sweep disabled tiered compilation and ReadyToRun. Windows sleep/Modern
Standby events interrupted that run, including a roughly six-minute process outlier.
Identical library DLLs also exposed substantial timing variation. That sweep was
stopped after 33 completed samples; its raw results are retained, but its wall times
are **not used for the performance conclusion**.

The controlled rerun sets `DOTNET_TieredCompilation=1` and `DOTNET_ReadyToRun=1`,
matching the normal runtime features, and temporarily requests Windows
`ES_SYSTEM_REQUIRED` to prevent idle sleep. The request is released when the runner
finishes; no persistent power setting is changed. Server GC remains as specified by
the unmodified harness. The machine is Windows 11 with 22 logical processors; the
SDK is 10.0.400 and the loaded runtime was verified as .NET 8.0.30. These are
one-warm-up measurements, not proof that every method has reached its final PGO tier.

“Meshing time” sums tables, edges, faces, refinement, and assembly. “Total time” also
includes parsing, decoding, and model preparation. Both exclude the independent
contract-check time. Allocations cover the meshing stages, matching the harness's
existing counter. Process CPU seconds sum user and kernel CPU time across the
process's threads for its **entire lifetime**, including startup/JIT, warm-up, both
contract checks, and the measured run. They are supplementary, not an isolated
meshing CPU counter. Build times are logged but are not a clean-build performance study.

Each run compares the assembled mesh's position/index digest, mesh checks, face
counts, exceptions, shell-boundary results, and contract verdict against baseline.
It also hashes edge, face, and contract CSV contents after removing timing columns,
so changes to per-face paths, diagnostics, or counts cannot masquerade as speedups.

## Results and interpretation

The [machine-readable evidence](brep-inlining-2026-10-24-data.json) retains all
controlled samples, correctness checks, policies, pins, package hashes, and the
interrupted sweep. All 27 controlled samples exited successfully and matched
baseline. No sleep/standby transition was recorded during the controlled run.

Median warm meshing time (observed minimum-maximum), in milliseconds:

| Model | Baseline | Current-policy partial | Most-aggressive partial | Aggressive vs baseline |
|---|---:|---:|---:|---:|
| ODIN-W160 | 730 (718-756) | 790 (775-834) | 779 (771-825) | +6.8% |
| ELLA-W1 | 5,964 (5,633-6,061) | 6,934 (6,321-7,021) | 6,763 (6,411-6,988) | +13.4% |
| JODY-W3 | 7,651 (7,317-9,475) | 8,002 (7,997-8,051) | 7,923 (7,598-7,960) | +3.6% |

The aggressive policy is about 1-2.5% faster than current-policy partial inlining
at these medians, but the ranges overlap. Three repetitions on a shared machine
do not establish a small improvement. Neither partial policy demonstrates a warm
meshing speedup over baseline; JODY's broad baseline range especially precludes
a strong latency claim.

| Model | Baseline allocations, MiB | Current partial, MiB | Aggressive partial, MiB | Change |
|---|---:|---:|---:|---:|
| ODIN-W160 | 435.1 | 481.4 | 481.4 | +10.65% |
| ELLA-W1 | 1,169.4 | 1,794.9 | 1,794.9 | +53.49% |
| JODY-W3 | 20,970.3 | 20,842.6 | 20,842.6 | -0.61% |

Allocation totals are highly repeatable and barely change with larger budgets.
[Compiler-500](../../tracker/issues/compiler-500.md) tracks profiling their cause;
the measurements do not identify allocation sites or prove a compiler bug.

Median whole-process CPU seconds (baseline/current/aggressive) were
10.13/10.59/9.61 for ODIN, 27.61/25.30/27.11 for ELLA, and
32.86/31.53/29.16 for JODY. These mixed results include JIT and two complete runs,
so they must not replace isolated warm meshing measurements.

The experiment supports leaving defaults unchanged while repairing compiler-499
and investigating allocations. It does **not** settle whether fully aggressive
inlining of excluded hot code would help. The useful next experiment is a fully
compiling inliner with allocation profiles and longer steady-state sampling,
not merely another increase to the body-size ceiling. Recursion, type safety,
and other eligibility guards were intentionally not disabled.

## Reproduction

The scripts require the local plato-brep checkout's pinned SDK package feed. They
write generated code and binaries only under the chosen experiment directory; the
standard build recorder also updates the repository's working status snapshot.
Use a new directory for preparation:

```powershell
powershell -File tools/experiments/prepare-brep-inlining.ps1 -Work C:/temp/brep-inline
python tools/experiments/brep-inlining.py generate --work C:/temp/brep-inline
python tools/experiments/brep-inlining.py partial --work C:/temp/brep-inline --variants default large huge duplicate
python tools/experiments/brep-inlining.py measure --work C:/temp/brep-inline --variants baseline default-partial duplicate-partial --corpus C:/Users/cdigg/git/plato-brep/data/ublox-benchmark --repeats 3 --tiered
```

Full inline compilation failures are intentional recorded outcomes of this experiment;
the measurement command refuses to time those invalid outputs. The partial action
requires compiler diagnostics in generated files, and does not turn restore failures
or arbitrary build failures into fallback eligibility.

## Verification

Experimental generation passed the throwing-stub gate for all five policies.
Baseline and all four matched partial hosts compiled; all four full inline hosts
failed with the diagnostics above. Python compilation and PowerShell syntax checks
passed for the runner scripts. These checks concern the pinned experiment.

Final repository checks concern the current shared checkout, which also includes
compiler-496's independently committed method-selection change:

| Check | Result | Seconds |
|---|---|---:|
| Recorded CLI build | PASS | 132 |
| Shipping strict lint and warning ceiling | PASS | 18 |
| Full PlatoTests and regression ceilings | 361 passed, zero failed | 537 |
| Studio legacy strict lint | PASS | 127.6 |
| Studio forward strict lint | PASS | 68.3 |
| External SDK build | PASS | 23.4 |
| External geometry tests | 15 passed | 98.5 |
| Studio frozen-artifact check | FAIL: existing external SDK drift, repo-473 | 0.9 |
| Optimizer documentation freshness | FAIL: concrete writer evidence/review stale, compiler-501 | not timed |

Studio's total battery took 319.2 seconds. The frozen-artifact failure is tracked
by [repo-473](../../tracker/issues/repo-473.md); no SDK file or checksum manifest was
changed here. [Compiler-501](../../tracker/issues/compiler-501.md) captures the stale
source evidence after the independent concrete-writer change; this experiment
changes no production optimizer source. Full logs are retained in
`.temp/inline-brep/check-all.log` and `.temp/inline-brep/repo-gates.log`.
