# Repository organization results — 2026-10-23

Work item: [repo-468](../../tracker/issues/repo-468.md). This is a dated verification record; [source organization and navigation](../repository-organization.md) holds the lasting policy.

## What is easier to change

Three parallel Sol agents owned compiler, writer, and runtime/navigation paths. The supervisor owned tests, documentation, tracking and executable checks, with serialized commit turns. The compiler's coercion and unification implementation now live in named partial files. AST formatting has its own folder. The C# writer's optimization passes are grouped, and loop emission is separated from expression emission. Interpreter evaluation and verification, and navigation indexing, analysis and execution, each have a visible home. Compiler tests are grouped by checking, parsing, emission, optimization and contracts.

The solver's original main file went from 1,056 to 779 physical lines, and the C# body writer from 1,164 to 902. Their extracted method bodies were compared against the original source. This is a reduction in the amount a reader must scan for a responsibility, not a reduction in executed work. No new runtime layer or public API was introduced. The implementation audit recorded 93 file moves and 123 new folder guides; subsequent tracking and validation documents are separate additions.

Maintained handwritten source has opening documentation, and maintained folders have local guides. The final audit found no uncovered maintained source headers or folders. Generated output, exact parser fixtures, vendored modules, packaged artifacts, machine-readable formats and external submodules retain their appropriate ownership/format contracts, documented in the organization guide. Standard-library tiers remain flat because normal CLI input enumeration is not recursive. Intrinsic runtime files remain flat because project imports and embedded resources name their paths.

## Independent improvements

- `c2c0d4d9`: verification-policy tests now discover nested source files and prune build outputs. This prevents test moves from silently reducing policy coverage; its focused regression passes.
- `743d44c1`, [repo-470](../../tracker/issues/repo-470.md): removed tracked Python bytecode, leaving the Python source as the maintained input.
- `29cbf2a8`, [repo-474](../../tracker/issues/repo-474.md): recorded gates retain command, exit status, stdout and stderr under a unique `.temp/gate-logs/` run directory. A red gate can now be investigated without repeating the expensive command. Child-process failure, both output streams, dry-run behavior and per-gate routing were checked; an independent reviewer found no actionable issue.

## Executable validation

The source implementation was stable before executable integration. Later changes only added gate logging and tracking/evidence. Builds used `tools/dotnet-build-record.ps1`; full logs remain under `.temp/csharp-build-logs/`. Gate output, test TRX files and original-assembly replays remain under `.temp/organization/`. This report records actual runs, not inherited green results.

| Check | Result | Seconds |
|---|---|---:|
| CLI and all referenced writer/runtime projects, Release build | Pass | 94.0 |
| Compiler test project, Release build | Pass | 31.9 |
| Recursive verification-policy regression | 4 passed | 3.1 |
| `record-gates.py`: CLI build | Pass | 32 |
| `record-gates.py`: shipping-tier strict lint | Pass | 53 |
| `record-gates.py`: complete compiler suite | 318 passed, 4 failed; same failures with original assemblies | 772 |
| Navigation test project, Release build | Pass | 34.8 |
| Navigation suite | 65 passed, 3 failed, 2 explicitly excluded; all failures reproduced with original assemblies | 693.0 |
| Interpreter test project, Release build | Pass | 77.3 |
| Interpreter suite | 103 passed, 2 explicitly excluded | 382.6 |
| Context exporter test project, Release build | Pass | 32.9 |
| Context exporter suite | 5 passed | 14.5 |
| Intrinsic runtime test project, Release build | Pass | 14.7 |
| Intrinsic runtime suite | 182 passed | 7.5 |
| Studio full battery, run once against this Plato checkout | Failed only on external SDK frozen-file drift | 301.7 |

The studio battery passed legacy lint (23.1 s), forward lint (57.8 s), SDK build (13.0 s) and GeometryTests (193.8 s). Its frozen-file check failed in 13.6 s on external SDK `PlatoList.cs`, `PlatoBuffer.cs` and `Number.cs`; this wave did not edit that checkout. No frozen manifest was refreshed.

The compiler's four failing tests were replayed both with current assemblies (125.8 s) and with original `876fe36b` Plato assemblies (145.4 s), using the same tests, source corpus and external dependencies. All four error messages matched exactly. The failures concern TypeScript output comparison, implicit conversion inventory, emitted conversion inventory, and bodiless signatures in the future collision vocabulary. Navigation replay reproduced the assignment-reference omissions and the seven resolution errors after simplification. Its rebuild timing also failed on the original implementation: 21,255 ms before versus 17,664 ms after, against a 7,760 ms limit. These observations attribute the failures; they do not make the failing gates pass.

Full forward C# conformance regeneration and the separate experimental backend suites were not run. The CLI build covers all writer projects, and the compiler suite plus generation comparison exercises C# emission. The organization wave does not claim that the repository's entire gate battery is green.

## Compilation comparison

The baseline source is `876fe36b`; the measured reorganized revision is recorded in [the evidence JSON](repository-organization-evidence-2026-10-23.json). Both versions compiled the same current `stdlib/foundation` inputs. Each recipe had one warmup pair and three measured pairs, alternating execution order. Output directories were created before timing. Timings include the `dotnet` CLI process and generation, but exclude fingerprinting and C# compilation of the generated library.

| Recipe | Original median | Reorganized median | Change | Original measured range | Reorganized measured range |
|---|---:|---:|---:|---:|---:|
| Extension-style C#, default optimizers | 12.10 s | 12.24 s | +1.16% | 8.12–14.60 s | 11.61–20.84 s |
| Extension-style C#, inlining, component unrolling, array optimization, loops and fusion | 11.60 s | 9.62 s | −17.11% | 8.88–12.55 s | 8.20–9.96 s |

Every pair produced equivalent contents and names for all 283 generated C# files, after normalizing line endings/BOM and removing the generated creation-date comment. No run reported degraded bodies. The observed medians show no material regression on these inputs. Shared-machine load and overlapping read-only test execution explain why this is a smoke comparison rather than a statistically controlled benchmark; the faster optimized median is not evidence of an algorithmic speedup. This does not certify every compilation workload or IDE latency.

The original source was built in `.temp/organization/baseline`, without switching the shared checkout. Its archived parser project initially failed because its SDK project references depend on checkout depth. Only that temporary fixture's SDK references were replaced with binary references; no production submodule files changed. All non-Plato runtime DLLs were then copied from the current CLI output to the baseline and SHA-256 equality checked. The evidence JSON retains those dependency hashes and all measured samples. The local harness is `.temp/organization/compare_compilers.py`.

## Findings left for separate work

These findings were captured immediately and remain open; they need behavioral or external-repository decisions beyond safe organization.

- [compiler-469](../../tracker/issues/compiler-469.md): inactive commented parser and XML writer sketches obscure which implementation is live; confirm consumers before removing them.
- [compiler-471](../../tracker/issues/compiler-471.md): parser SDK references depend on checkout location and broke the isolated build with `CS0234`/`CS0246`; fix the external parser repository and update its pin.
- [compiler-472](../../tracker/issues/compiler-472.md): assignment targets are absent from navigation references, so go-to-definition misses those occurrences; register binder references and test shadowing/loops.
- [repo-473](../../tracker/issues/repo-473.md): the studio frozen-file gate disagrees with its external SDK checkout; inspect those changes before repairing or retiring that gate.
- [compiler-475](../../tracker/issues/compiler-475.md): conversion inventories disagree with current compiler behavior; review conversion semantics before updating code or accepted inventories.
- [stdlib-476](../../tracker/issues/stdlib-476.md): the future collision vocabulary declares forbidden bodiless functions; supply valid bodies or revise the declarations under the intrinsic contract.
- [compiler-477](../../tracker/issues/compiler-477.md): simplifier edits introduce unresolved names in the corpus; isolate the offending rules before changing rewrite behavior.
- [compiler-478](../../tracker/issues/compiler-478.md): navigation rebuild exceeds its latency budget on this host even before the refactor; controlled profiling must distinguish implementation cost from measurement instability.
- Existing [plato-438](../../tracker/issues/plato-438.md) covers the TypeScript generation comparison failure; this wave did not change its emitted bodies.

## Commit markers and rollback

Each implementation chunk has an explicit commit boundary. Revert a selected commit with `git revert <sha>` after checking the current shared checkout. For a complete rollback, revert dependent chunks in reverse chronological order; later documentation references may need to follow reverted paths. The policy scanner fix is independently useful but its nested-file regression assumes the grouped tests exist.

| Commit | Reviewable chunk |
|---|---|
| `aa59ac51` | Writer grouping, loop-emitter split, writer guides and headers |
| `402dec10` | Compiler solver split, AST formatting group, compiler guides and headers |
| `2210ac29` | Runtime/navigation grouping, extracted edit record, guides and headers |
| `c47c0e00` | Compiler tests grouped by responsibility and documented |
| `c2c0d4d9` | Recursive policy scanning and its regression |
| `743d44c1` | Tracked Python bytecode removed |
| `7abd8a2a` | Examples and editor integration documentation |
| `efd5692a` | Repository-wide navigation guide, folder documentation and source headers |
| `29cbf2a8` | Durable per-command gate logs |

Tracking-only commits record the plan, discovered issues, progress and closures. No unrelated root scratch notes were included. The pre-existing `docs/status-report-snapshot.json` and `docs/status-report.html` bytes were restored after the gate writers stopped; this mission's results are preserved in the dated evidence JSON and appended gate-log row. The generated intermediate reports are also retained under `.temp/organization/final-status-report*`.

## Rerun commands

From the Plato root:

```powershell
powershell tools/dotnet-build-record.ps1 -Project src/Plato.CLI/Plato.CLI.csproj -TargetName organization-cli
python tools/record-gates.py
$projects = 'Plato.Navigation.Tests', 'Plato.Interpreter.Tests', 'Plato.ContextExport.Tests', 'Plato.Intrinsics.Tests'
foreach ($project in $projects) {
    powershell tools/dotnet-build-record.ps1 -Project "tests/$project/$project.csproj" -TargetName "organization-$project"
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
    dotnet test "tests/$project/$project.csproj" -c Release --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { Write-Warning "Tests failed: $project; inspect the output before proceeding" }
}
```

The local comparison harness requires the prepared original fixture and fresh output folders; its already-recorded samples are in the evidence JSON. To reproduce a single generation invocation, create an empty output folder first, then run `dotnet <CLI.dll> stdlib/foundation --out=<folder> --csharp-style=extensions`; add `--inline --optimize --optimize-arrays --loops --fuse` for the optimized recipe. Use the same corpus and external runtime DLLs for both revisions.
