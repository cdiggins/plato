# Prepared optimizer benchmark wave

Issue: compiler-522. Shared checkout: C:/Users/cdigg/git/plato.
Contract revision: R3 (run-set batching, verified provenance, declared host obligations). Supervisor owns integration, CLI, documentation, tracker and Git publication.
No writer changes compiler algorithms, runtime, stdlib, existing experiments, or sibling checkouts.
Existing dirty status snapshots, statement-inlining experiments and root PROGRESS.md are unrelated.

## Acceptance and boundaries

Prepare committed plato-brep sources into a fresh workspace below .temp, export pinned MeshKernel
and copy pinned SDK feeds. Read sibling repositories only. Snapshot the supplied compiler binary
closure and hash every input; do not imply the supplied binary was built from a particular commit.
Generate each recipe once, reject every degraded body and every new bodiless stub, then compile before running.
Only declared host obligations in the committed plato-brep generated-stubs.txt baseline are permitted;
the committed check_generated.py --log enforces this. No new names or compiler-error fallback is allowed.
Those declarations include STEP host entry points supplied before the measured prepared-model boundary.
Runtime
and host dependencies must be identical across recipes. Cache hits require content integrity.
No STEP parsing, disk I/O, serialization, digesting, contracts, progress logging or per-face clocks
inside the measured region. Precompute stage prerequisites for stage cases; model cases include
normal cache construction, discretization, refinement and assembly. Use real shipped methods.
No correctness failure, compile failure, timeout or absent workload may become a speedup.

## Wire format (R1)

Python modules use only the standard library. Package modules reside beside benchmark.py;
imports must also work under unittest discovery from this directory. JSON uses snake_case.
Worker is a net8.0 executable referencing the exported step-corpus-harness project.
Builder copies worker source into the exported snapshot, sets BrepRoot MSBuild property to
the snapshot root, builds it with tools/dotnet-build-record.ps1 and freezes host DLLs across variants.

Worker commands (all successful command output is one JSON object on stdout):

- `prepare --input <step path or witness:Name> --output <fixture path> --id <id> [--chordal <number>]`:
  writes portable prepared-model fixture, returns `{schema:1,id,fixture,tags:[...],workloads:[...],input_hash,...}`.
- `run --fixture <path> --workload <name> --warmup <count> --target-ms <number> --max-iterations <count>`:
  loads once, warms, measures calibrated iterations, returns `{schema:1,case_id,workload,elapsed_ns,iterations,
  allocated_bytes,digest,checks:{...},...}`. elapsed_ns and allocated_bytes are totals for iterations.
  Digest/checks are computed outside timing. Failure returns nonzero with JSON error if possible.
- `list`: returns available witness names and supported workload names for discovery.
- `run-set --fixture <path> --workloads <comma-separated names>` with the same measurement options
  loads a fixture once and returns `{schema:1,samples:[...]}` with one run object per workload.
  Parent schedules case/variant/process blocks and expands samples into the unchanged R1 raw rows.
  Per-workload failures remain explicit. Workload order is identical across variants within a block.

Prepared suite is `{schema:1,preparation_binary_identity:{...},cases:[{id,fixture,fixture_hash,
preparation_identity,tags:[...],workloads:[...],...}]}`. Every cached fixture is bound to a currently
verified preparation artifact closure; modified artifacts or fixtures require rebuilding/re-preparing.
The runner verifies expected build output_hashes, common host_hashes, and suite/fixture hashes before
and after execution. Worker sample schema and fixture identity must match the request.
Fixtures and suite are setup artifacts, hashed before run. File paths may be absolute in local
artifacts, never assume a particular developer checkout. Fixture caches invalidate on input and
preparation/runtime identity changes. Deserialization must reconstruct wrapper and sum types correctly.

Variant description is `{name,flags:[...],kind,description,...}`. Build result is
`{name,status,worker:<dll path>,flags:[...],...}`. `status` is ready/error/unsupported.
Full recipe: --inline --fuse --optimize --loops --fuse-consumers --exact-output.
Shipping recipe is read from the exported regeneration recipe, or explicitly supplied and recorded.
No-inline removes experimental inline policy/growth too. No-loops removes loop-dependent flags and
reports this dependency closure. Experimental policy/growth sweeps are opt-in and independent from
the ordinary full recipe. Exclude --optimize-arrays with its documented runtime mismatch reason.

Each raw run row is `{schema:1,block,variant,case_id,workload,status,elapsed_ns,iterations,
allocated_bytes,digest,checks,...}`; worker errors carry status=error/timeout with error detail.
Control comparisons are paired by block and case/workload, with identical iteration accounting.
Report module must reject malformed/nonfinite/nonpositive timing, missing/duplicate pairs,
failed checks and disagreeing digests. Unmeasured and inconclusive remain explicit states.

## Ownership and resources

| Track | Exclusive writable files | Checks/resources |
|---|---|---|
| prepare | prepare.py, recipes.py, test_prepare.py, test_recipes.py, PREPARE_PROGRESS.md | Python tests; .temp/optimizer-prepare-tests only |
| worker | worker/**, WORKER_PROGRESS.md | C# worker; .temp/optimizer-worker-tests only; request supervisor builds |
| report | analysis.py, selection.py, test_analysis.py, test_selection.py, REPORT_PROGRESS.md | Python tests; .temp/optimizer-report-tests only |
| supervisor | benchmark.py, README.md, CONTRACT.md, PROGRESS.md, integration tests, tracker, dated report | .temp/optimizer-benchmark; all builds/gates and publication |

All agents read parallel-wave and platonic-coder skills. No nested agents (slots are allocated).
Each acknowledges R1 before writes, checkpoints state/processes/results, and requests a commit turn.
Only the turn holder stages/commits; explicit paths only. Supervisor owns tracker progress updates.
Scoped tests may run concurrently only with stable inputs and independent output directories.
Stop writes and child processes and acknowledge before integrated validation.

## Verification and handoff

Python unit and integration tests exercise cache invalidation, dependency closure, correctness
gating, report pairing and coverage selection. Supervisor compiles and executes worker against
real prepared witness and STEP inputs and runs required repository gates once after stabilization.
Performance targets are budgets, not promises or measured speed claims. No publication until
integration checks and commit-history review. Record commands, results and limitations in dated evidence.
