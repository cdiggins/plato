# Compiler and navigation fixes — 2026-10-24

Work item: [repo-479](../../tracker/issues/repo-479.md). Baseline: `a1c37ede`, the completed organization wave. Three Sol agents owned conversion review, TypeScript emission, and navigation; the supervisor owned the intrinsic guard, integration and performance checks. Sources were held stable for each executable window and commits were serialized on `main`.

## Corrections and decisions

- [compiler-472](../../tracker/issues/compiler-472.md), assignment navigation: assignment targets now go through ordinary identifier resolution, retaining lexical binding while recording source occurrences. Nested-loop/shadowing tests and the corpus sweep pass. The added runtime work is a source-map insertion for each assignment, not a new traversal.
- [compiler-475](../../tracker/issues/compiler-475.md), conversion inventories: reviewed existing vocabulary additions and accepted the faithful field/SDF closure lifts and established single-field unwraps. `TpmsField3D` remains a scalar field, not a distance field. The issue explains every added category and the harmless Motor3D parameter-name change; no compiler conversion behavior was changed.
- [plato-438](../../tracker/issues/plato-438.md), TypeScript emission: the legacy writer now uses runtime constructors such as `Number` for static expressions. It also preserves expected result types through nested conditional branches, so capsule points remain `Point3D`, matching typed-body emission. Independent runtime-spelling/constructor assertions reuse the existing comparison's outputs instead of adding redundant full compilations.
- [stdlib-476](../../tracker/issues/stdlib-476.md), intrinsic-location guard: the library was valid. A greedy regular expression mistook a ternary expression in collision warm-starting code for a bodiless declaration. The guard now rejects body syntax and has positive/negative regressions; no library code, intrinsic permissions, exclusions or ceilings changed.
- [compiler-477](../../tracker/issues/compiler-477.md), simplifier: removing a thirteen-field constructor name introduced nonexistent `Tuple13` calls. The rule now requires the actual matching generic tuple declaration in the supplied vocabulary. Focused cases cover available Tuple2, missing Tuple2 and missing Tuple13, and rebind the edited source. The corpus gate no longer introduces the seven resolution errors.
- [compiler-478](../../tracker/issues/compiler-478.md), navigation latency: quiet phase measurements showed parsing dominated fresh rebuilds and the original budget was attainable. The original fresh-build test and ceiling remain unchanged. A separate cached-update test now exercises the production `IncrementalIndexer` path and checks parse reuse; an explicitly selected phase probe separates parse, bind and index costs. No production speedup is claimed.

The new assignment and missing-tuple regression tests were replayed with the saved original assemblies: three failed as expected, while the valid-tuple positive case passed. All four pass with the corrections. This demonstrates that the new tests detect the original bugs, rather than merely agreeing with the new implementation.

## Verification

Builds use `tools/dotnet-build-record.ps1`; logs are under `.temp/csharp-build-logs/`. Test logs and TRX files are under `.temp/failure-fixes/`. [The evidence JSON](compiler-navigation-fixes-evidence-2026-10-24.json) retains the measured results. Explicit diagnostics are selected separately and are not silently counted as regular passing tests.

| Check | Result | Wall seconds |
|---|---|---:|
| Initial navigation test build | Pass | 19.9 |
| Initial compiler test build | Pass | 14.9 |
| Assignment scope regression and corpus identifier sweep | 2 passed | 21.2 |
| Initial compiler focused tests | 16 passed; remaining TypeScript conditional mismatch then corrected | 50.5 |
| Simplifier rule-isolation diagnostic | Identified SIM001 as the sole source of seven errors | 35.7 |
| Final navigation test build | Pass | 16.2 |
| Final compiler test build | Pass | 15.5 |
| Complete navigation suite | 73 passed; 3 explicit diagnostics excluded | 155.2 |
| Final TypeScript suite | 3 passed; legacy and typed-body output identical | 22.6 |
| Original-assembly regression replay | 3 expected failures, 1 positive case passed | 20.7 |
| CLI build through gate recorder | Pass | 12 |
| Shipping strict lint | Pass; zero errors, unchanged ratchet | 42 |
| Complete compiler suite, including forward checking | 330 passed | 233 |
| Interpreter test build | Pass | 14.8 |
| Interpreter execution regressions | 35 passed, including assignment and loops | 13.5 |
| Studio frozen SDK artifact check | Existing external failure; tracked as repo-473 | 0.4 |
| Studio shipping legacy lint | Pass | 49.1 |
| Studio forward lint | Pass | 19.5 |
| External SDK geometry build | Pass | 6.9 |
| External SDK geometry tests | Pass | 16.9 |

No failure was addressed by weakening a ratchet, adding an exclusion or removing the original fresh-build performance check. Complete forward C# conformance regeneration is not part of this wave; no standard-library declarations or implementation bodies changed.

The required Studio battery ran once and remains red solely because its frozen SDK artifacts differ from its manifest. [repo-473](../../tracker/issues/repo-473.md) already tracks that external blocker. The Plato compiler/navigation corrections are verified; a completely green cross-repository battery remains outstanding and requires the Studio/SDK owners to reconcile their artifact changes. This mission changes neither the external artifacts nor their manifest.

## Performance evidence

The isolated navigation phase probe measured fresh rebuilds at 4.83–7.39 seconds and cached updates after priming at 0.77–1.30 seconds. During the complete suite, the original fresh-build gate took 4.242 seconds and the new cached-update gate 0.841 seconds against the same 7.760-second ceiling. Every cached input was reused. The earlier loaded-host failures therefore did not establish a new algorithmic regression; they remain a reminder that wall-clock gates need a quiet execution window.

The original navigation phase probe measured fresh rebuilds at 3.93–6.91 seconds and primed cached updates at 0.58–0.66 seconds. Those sequential probes were not matched alternating runs: they establish phase costs and budget feasibility, not a relative speed claim. The corrected index records 81,342 references versus 81,181 originally, with the same 22,358 definitions; the additional occurrences are the assignment references restored by this fix.

The CLI comparison uses the preserved pre-fix binaries and the same unchanged foundation source, verifies external runtime DLL hashes, alternates execution order, excludes warmups and compares generated C# after removing creation-date comments and normalizing line endings/BOM. No other owned timed workload ran during it. Both recipes emitted the same 283 C# files before and after, with no degraded bodies.

| Recipe and sample | Before wall median | After wall median | Before CPU median | After CPU median |
|---|---:|---:|---:|---:|
| Default, three measured pairs | 4.851 s | 4.776 s (-1.5%) | 4.938 s | 5.000 s (+1.3%) |
| Optimized, initial three pairs | 5.177 s | 6.078 s (+17.4%) | 5.234 s | 5.547 s (+6.0%) |
| Optimized, five additional pairs | 5.795 s | 5.029 s (-13.2%) | 5.250 s | 4.922 s (-6.3%) |

The elevated first optimized result triggered the additional comparison rather than being discarded. In its final slow pair, parsing took 2.473 seconds versus 1.625 seconds originally, while C# emission took 2.168 seconds versus 2.531 seconds. The parser is unchanged. The repeat reused each version's output directory and deferred output hashing until all timed runs finished; the final outputs also matched. Both datasets and their methods are retained in the evidence JSON. These measurements show substantial host variation and no reproducible material slowdown; they do not establish an optimization speedup or guarantee latency for workloads outside foundation.

## Reversible commits

| Work commit | Independent chunk |
|---|---|
| `c613197e` | Assignment targets recorded for navigation |
| `fba53605` | Reviewed conversion inventory updates |
| `cfe55871` | Intrinsic guard rejects conditional bodies |
| `3e4582a0` | Simplifier checks tuple vocabulary before removing constructors |
| `15a81f3f` | TypeScript runtime constructor spelling and conditional result types |
| `ea20cf96` | Navigation performance checks and documentation |

Tracker-only commits close each verified issue immediately after its acceptance commit. The intrinsic guard's full-suite acceptance is recorded separately in `316bbca2`, after its focused implementation commit. To undo a chunk, inspect the shared checkout and use `git revert <work-commit>`; revert dependent chunks in reverse order for a complete rollback. Pre-existing status-report edits were restored byte-for-byte after saving this mission's evidence; those edits and root scratch notes are excluded from this wave's commits.

## Rerun

From the Plato root:

```powershell
python tools/record-gates.py
powershell tools/dotnet-build-record.ps1 -Project tests/Plato.Navigation.Tests/Plato.Navigation.Tests.csproj -TargetName navigation-fixes-check
dotnet test tests/Plato.Navigation.Tests/Plato.Navigation.Tests.csproj -c Release --no-build --no-restore
```

To profile navigation explicitly, select `FullyQualifiedName=Plato.Navigation.Tests.CheckTimingTests.MeasureNavigationRebuildPhases` with `dotnet test --filter` after building the project, with competing workloads stopped. The locally preserved baseline and comparison harness are under `.temp/failure-fixes/`; the evidence JSON records all samples and the normalization method.
