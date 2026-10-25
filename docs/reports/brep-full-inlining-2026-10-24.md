# Full inlining and downstream optimization on plato-brep — 2026-10-24

Work: [compiler-499](../../tracker/issues/compiler-499.md). This follows the
[partial-inlining experiment](brep-inlining-2026-10-24.md), which could not test
important hot code because the generated C# failed to compile.

## Compiler repair

The compiler retains resolved constructor result types through inlining and loop
lowering. Field access therefore remains field access after a tuple-returning helper
becomes an explicit constructor. The emitter also groups conditional expressions
when used as array receivers and typed-index cast operands. Without those parentheses,
C# casts the condition or indexes only the last branch instead of the whole value.
These two causes explain the earlier field-as-method, Boolean-cast, missing-member,
and conditional-result diagnostics. Inlining budgets and eligibility guards are unchanged.

The focused executable regression passed without inlining and failed in both inline
configurations before the repair. All three configurations now compile and execute
both conditional branches and the constructor-field read. Existing inliner and
conditional-codegen checks also pass.

## Experimental question and controls

The question is whether source inlining exposes operations that subsequent passes
can simplify, not merely whether removing a call is faster. The matrix compares no
source inlining, current budgets (120 nodes, 8 passes, duplication cost below 4), and
aggressive budgets (2,000 nodes, 32 passes, duplication cost below 32), with:

- Fusion and loop lowering (`--fuse --loops`).
- Fusion, component expansion, and loop lowering (`--fuse --optimize --loops`).

Here, full inlining means applying the policy throughout the program without the
previous source-file fallback. It does not mean eliminating every call: expression
eligibility, recursion/capture restrictions and duplication guards still apply.
This experiment increases budgets within an expression-substitution inliner; it
does not implement unrestricted statement inlining or a general simplifier.

The aggressive run still records 26,410 statement-body refusals, 16,377
inside-lambda refusals, and 33,735 compound-argument refusals. These count attempts
across passes and generated functions, not unique call sites or runtime calls.
For example, `ModelMeshReport` calling `EdgeTableOf` is refused for a statement
body, `Eval` calling `KnotRatio` is refused inside a lambda, and `ChordLength`
calling `Offset` is refused by the compound-argument rule. The self-contained-body
check also excludes local bindings, assignments, unsupported generic/forwarding
lambdas, nested tuples and calls that cannot be emitted on their receiver.
Missing concrete bodies and intrinsic/runtime calls remain outside source inlining;
the JIT can still optimize emitted calls. The data records all refusal categories.

The compiler is pinned to repair commit `8515ca59`. B-rep, MeshKernel, SDK packages,
model inputs and runtime controls match the earlier experiment. All configurations
use the repaired compiler, including the no-inline baseline. No source-file fallback
is permitted. Generated methods retain their .NET `AggressiveInlining` annotations;
no source inlining does not mean that JIT inlining is disabled. The host and dependency binaries are frozen; only the generated
`Ara3D.StepBrep.dll` changes.

The experiment-only compiler records node and call counts before and after each
pass. These describe the emitted program, including library functions and laws;
they are not execution frequencies. Stage counts separate inlining/beta reduction,
fusion/literal expansion, component expansion, and loop lowering. A higher literal
count alone is not proof of constant folding. Plato has no general constant-propagation
pass in this pipeline; the .NET JIT may simplify exposed constants, but elapsed time
alone cannot attribute a benefit to that mechanism.

`--optimize-arrays` is excluded: its eager helper renaming targets methods absent
from the live runtime, while the existing `MapRange` is already eager. See the
[materialization contract](../optimizer/materialization.md). This is not a missing
caching optimization in the timed matrix.

Timing uses sequential fresh processes, one full warm-up then one measured mesh,
three shuffled repetitions per model/configuration, normal tiered compilation and
ReadyToRun, and a temporary Windows idle-sleep prevention request. Mesh digests,
face/edge/contract tables without timing columns, shell boundaries and errors must
match. Process CPU time includes startup, warm-up and contract checking. The primary measures
are complete warm meshing elapsed time and meshing allocations. No compiler builds or repository
tests are intentionally run concurrently with timing.


## Results

All six configurations compile without fallback. All 54 primary samples match the
baseline mesh digest, face/edge diagnostics, boundary counts and contract tables.
The complete raw samples, hashes and compiler-stage aggregates are in the
[machine-readable evidence](brep-full-inlining-2026-10-24-data.json).

Complete warm meshing time, milliseconds, median of three fresh-process samples:

| Model | No inline | Current | Aggressive | No inline + optimize | Current + optimize | Aggressive + optimize |
|---|---:|---:|---:|---:|---:|---:|
| ODIN-W160 | 767 | 836 | 880 | 787 | 856 | 878 |
| ELLA-W1 | 5,963 | 6,706 | 6,671 | 5,773 | 6,773 | 6,719 |
| JODY-W3 | 7,803 | 7,617 | 7,959 | 7,776 | 8,325 | 7,673 |

JODY has particularly wide ranges: current inlining spans 7,493–14,971 ms,
and current plus optimization spans 7,827–17,504 ms. These are descriptive
medians, not confidence intervals or evidence that small differences are real.
The evidence retains every sample; none was discarded as an outlier.

The separate performance-core comparison gave these medians and observed ranges:

| Model | No inline, ms | Aggressive, ms | Median change |
|---|---:|---:|---:|
| ODIN-W160 | 809 (806–925) | 857 (838–1,011) | +5.9% |
| ELLA-W1 | 5,874 (5,669–5,945) | 7,060 (6,710–7,219) | +20.2% |
| JODY-W3 | 7,630 (7,367–7,646) | 7,690 (7,518–7,909) | +0.8% |

All 18 additional samples match the baseline checks. Allocation totals reproduce
the primary matrix. No sleep/resume events were recorded over the measurement
interval. This follow-up also provides no evidence of a speedup; its small sample
size does not establish precise effect sizes or explain the earlier variability.

Meshing allocations are much more stable. Current and aggressive inlining have
essentially identical totals, with or without `--optimize`:

| Model | No inline, MiB | Inline, MiB | Change |
|---|---:|---:|---:|
| ODIN-W160 | 435.07 | 494.05 | +13.6% |
| ELLA-W1 | 1,169.43 | 1,801.04 | +54.0% |
| JODY-W3 | 20,970.32 | 20,855.21 | −0.55% |

The extra allocations remain unexplained; [compiler-500](../../tracker/issues/compiler-500.md)
tracks attribution with allocation profiles. They should not be attributed to
delegates or duplicated arrays without those profiles.

### What inlining enables

The pipeline does exploit newly exposed structure. In `GridTriangleFacesNormalSampled`
(in `SharedChordalCorners.g.cs`), generated loops fall from three to one.
In `AssembleModelMeshIndexed` (in `ModelMeshAssemblyIndexed.g.cs`), lambdas fall
from ten to six. Both changes occur with the current policy; raising budgets
does not improve these examples further.

Across the whole generated program, fusion removes 2,104 lambda nodes after
current-policy inlining. This does not mean the final program contains fewer
lambdas: expansion also copies bodies into callers, and unused helper definitions
remain. Actual C# syntax totals make that distinction visible:

| Policy, fusion + loops | Calls inlined | C# lambdas | Explicit `.Invoke` | C# loops | Source MiB | Assembly MiB |
|---|---:|---:|---:|---:|---:|---:|
| No inline | 0 | 3,399 | 194 | 3,338 | 16.69 | 7.11 |
| Current | 63,915 | 6,141 | 278 | 6,324 | 24.08 | 9.15 |
| Aggressive | 64,604 | 6,311 | 278 | 6,332 | 24.61 | 9.22 |

Aggressive budgets inline only 1.08% more call sites than current budgets. Both
perform 15 inliner beta reductions; aggressive mode exposes only eight additional
loop-lowering markers. More loops are not inherently better: expansion can copy
loops as well as expose fusion opportunities. These counts include laws and
unexecuted helpers, and assembly size is managed IL rather than native JIT code.

Adding `--optimize` removes 14 generated lambdas and six loops from either inline
policy, chiefly in laws and reporting helpers. It introduces no component-access
nodes for this input. The vocabulary mismatch below limits the experiment's ability
to evaluate field expansion; this is not a test of a fully adapted vector optimizer.

## Reproduction

Use a fresh experiment directory. Preparation requires the local pinned B-rep dependency
checkouts/package feed; the compiler snapshot is built from the specified commit.

```powershell
powershell -NoProfile -File tools/experiments/prepare-brep-inlining.ps1 -Work C:/temp/brep-full -PlatoCommit 8515ca59
python tools/experiments/brep-inlining.py generate --work C:/temp/brep-full --variants baseline default duplicate baseline-opt default-opt duplicate-opt --stage-evidence
powershell -NoProfile -File tools/dotnet-build-record.ps1 -Project tools/experiments/CSharpShape/CSharpShape.csproj -TargetName inline-csharp-shape
dotnet tools/experiments/CSharpShape/bin/Release/net8.0/CSharpShape.dll C:/temp/brep-full/default/generated
python tools/experiments/brep-inlining.py measure --work C:/temp/brep-full --variants baseline default duplicate baseline-opt default-opt duplicate-opt --corpus C:/Users/cdigg/git/plato-brep/data/ublox-benchmark --repeats 3 --tiered
```

The syntax utility accepts any generated variant directory and prints per-file/per-method
counts as JSON. It counts actual lambda syntax and explicit `.Invoke` calls, not all
possible JIT delegate optimizations. The stage counter is injected only into the isolated
compiler. Neither counter runs inside the measured host. The common host adds only the whole-mesh stopwatch described below.

The timing variation prompted a separate comparison of baseline and aggressive
inlining on the same performance cores. Windows reports the Intel Core Ultra 7
155H's logical processors 0, 1 and 10–19 as the higher efficiency class (the
[CPU-set API defines higher values as faster cores](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-system_cpu_set_information)).
The runner applies mask `0xffc03` to its own process, which its
[child processes inherit](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setprocessaffinitymask),
and restores the previous mask afterward. This selects twelve logical processors
on six performance cores. The affinity inheritance was checked with a child-process
smoke test before the run. No system power policy was changed.

```powershell
python tools/experiments/brep-inlining.py measure --work C:/temp/brep-full --variants baseline duplicate --corpus C:/Users/cdigg/git/plato-brep/data/ublox-benchmark --repeats 3 --tiered --run-name runs-performance-cores --affinity 0xffc03
```

That mask is specific to this machine: discover the topology before reproducing
on another computer. Affinity can also affect runtime processor counts and garbage
collector behavior, so compare policies within this group rather than pooling its
samples with unrestricted runs. Frequency and unrelated background load remain
uncontrolled. Both groups use .NET 8.0.30 and the same Server GC configuration.


## Measurement boundaries and optimizer coverage

The original harness's stage sum excludes inter-stage work, including adjacency
preparation and changed-edge/remeshed-face bookkeeping. `TotalMs` is also a stage
sum, not a complete wall clock. The isolated host therefore records `MeshingElapsedMs`
from immediately after the allocation counter starts through assembly, before contract
checking. It uses allocation-free timestamp reads, and every variant receives the same
host binary. The original `MeshingMs` stage sum remains in the evidence for comparison.
The upstream harness gap is [compiler-502](../../tracker/issues/compiler-502.md).

The fixed-component field table requires `IArrayLike`, which is absent from this
pinned program's inputs. Forward vectors use other contracts, including `IVector`.
The current `--optimize` flag still performs fixed-array rewrites, but cannot establish
the benefits of a field-based optimizer adapted to the forward vocabulary. That
coverage question is [compiler-503](../../tracker/issues/compiler-503.md).

## Interpretation

The enabling mechanism is real: source inlining exposes fusion and callback
removal in meshing code. This experiment does not show a net meshing speed benefit
from raising the current budgets. Most eligible expansion already happens under
the current policy, while ODIN and ELLA allocate more with either inline policy.
Production defaults therefore remain unchanged.

The next useful investigations are allocation attribution and adapting component
recognition to the forward vocabulary. Constant propagation and native JIT
inspection would require a separate experiment; neither was implemented or inferred
from timing here. The broader hypothesis remains plausible, but these measurements
support improving the transformations enabled by inlining before increasing budgets
further.

## Validation

The repaired compiler passes all 364 Plato tests (203s), the recorded CLI build (14s),
and shipping strict lint (21s). The focused red/green regression and existing inliner
checks passed nine cases (9s). Optimizer documentation freshness and six maintenance
tests pass. The syntax-count utility builds on net8.0 against the same Roslyn package
version as the compiler's execution tests (9.7s).

C++/CUDA checks were not run, at the user's request; this mission validates C#.

Studio integration passed legacy lint (9.7s), forward lint (23.4s), SDK build (4.1s),
and all 15 geometry tests (13.2s). Its existing external frozen-artifact failure remains
[repo-473](../../tracker/issues/repo-473.md), taking 0.9s; total battery 51.6s. The SDK
files and manifest were not changed. Repository gate logs:
`.temp/gate-logs/20261024-224420-676696`; integration log:
`.temp/compiler-499/check-all.log`.
