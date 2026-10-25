# Additional optimizer exploration

## Geometry qualification G3

The user authorizes representative geometry validation of both opt-in production passes and
continued execution of the remaining ideas, prioritized by measurable benefit. The existing
compiler-506 through compiler-511 issues remain the work records. No default changes are planned.

The fusion-geometry owner writes only `tools/experiments/optimizer-geometry/consumer-fusion/`,
`.temp/optimizer-geometry/consumer-fusion/`, and its dated geometry report. The exact-geometry
owner has the equivalent `exact-output` paths. Secondary owners receive distinct `array-facts`,
`loop-unswitch`, `record-elimination`, or `grid-reuse` paths as slots become available. Reports
are `docs/reports/optimizer-geometry-<slug>-2026-10-25.md`. Each owner keeps a short CHECKPOINT.md
inside its experiment subtree. Existing compiler source and shared documents are read-only to
agents until a specific production handoff is approved by the supervisor.

The supervisor owns compiler snapshots, shared harness contracts, tracker, this plan, integration,
documentation evidence, build reservations and Git turns. Initial agents inspect existing library
callers and prepare actual Plato workloads while the supervisor obtains a stable compiler build.
Frozen assemblies are supplied under `.temp/optimizer-geometry/compiler/`; agents must record
their hashes and never silently substitute later working-tree binaries. Private harness builds
may use that snapshot after it is declared ready. Timing runs are serialized, after correctness
and emitted-shape checks; no parallel timing or shared project builds are allowed.

Qualification requires actual generated code, flag-off/on comparison, preserved observable
results and fault behavior, representative sizes and degenerate inputs, consumed outputs,
warmed alternating-order samples, allocation data and explicit limitations. Distinguish unchanged
library callers, faithful extracted kernels and newly authored representative geometry kernels.
Report refusals and no benefit honestly. Do not widen a matcher merely to make a benchmark pass.
New production optimizations require measurable benefit or a concrete enabling benefit, bounded
legality, their own default-off switch, focused regressions and optimizer documentation stewardship.

## Geometry promotion decisions

Retain independent default-off production flags for exact-output allocation and consumer fusion.
The [allocation qualification](../../reports/optimizer-geometry-exact-output-2026-10-25.md)
supports the existing literal-width contract; it does not justify admitting helper calls.
The [fusion qualification](../../reports/optimizer-geometry-consumer-fusion-2026-10-25.md)
leaves dependable geometry performance as an open requirement.

Keep the [array-facts adapter](../../reports/optimizer-geometry-array-facts-2026-10-25.md)
private until a concrete consumer requires its producer facts. Do not add a production
[unswitch pass](../../reports/optimizer-geometry-loop-unswitch-2026-10-25.md) without new
benefit evidence. [Record projection](../../reports/optimizer-geometry-record-elimination-2026-10-25.md)
requires a surviving cost or useful downstream simplification; [grid caching](../../reports/optimizer-geometry-grid-reuse-2026-10-25.md)
requires complete capture, conversion and schedule proofs before runtime comparison.

Incremental verification targets the affected behavior only. Preserve frozen inputs and private
outputs; do not rerun broad gates or benchmark campaigns for documentation or bookkeeping changes.

## Production continuation P2

The user authorized production implementation after the initial experiments. The priority
is multiple-consumer fusion and exact-output allocation. Both are independently disabled
by default; generation flags `--fuse-consumers` and `--exact-output` require C# `--loops`.
The writer API exposes independent switches and retains the existing path on refusal.

The fusion owner writes `TirMultiConsumerFusion.cs`, `TirMultiReduction.cs`,
`TirCSharpBodyWriter.MultiConsumer.cs`, `MultiConsumerFusionTests.cs` and its fixture folder.
The exact-output owner writes `TirExactOutput.cs`, `TirCSharpBodyWriter.ExactOutput.cs`,
`ExactOutputTests.cs` and its fixture folder. All are under the existing writer optimization,
writer body or PlatoTests optimization folders. The supervisor owns shared files, CLI/API
wiring, identity checks, provenance retention, integration, documentation evidence and commits.
An independent reviewer reads both implementations. The secondary census owner writes only
its separate experiment/evidence subtree and dated report for the other proposals.

Fusion initially recognizes a contiguous source declaration followed by independent total
Number reductions. It runs before materialization and ordinary loop lowering, retaining
resolved calls and original binding identities in an explicit statement marker. Exact-output
allocation validates retained intrinsic provenance at loop emission and guards on a runtime
array with fixed length; it preserves ordered source reads and full callback evaluation,
so it need not prove source ownership. Both reject unsupported coercions and uncertain shapes.

Builds are supervisor-owned and run only after relevant writers freeze their files. Focused
tests must compile and execute actual Plato-generated C#, cover disabled/enabled/refused
cases and combined flags, and preserve arithmetic grouping, conversions and fault behavior.
The supervisor also checks CLI flag validation, both dump modes, optimizer regressions and
documentation freshness. Default-path output must remain unchanged by the optional features.

Status: active exploration under [compiler-506](../../../tracker/issues/compiler-506.md)
through [compiler-511](../../../tracker/issues/compiler-511.md). This is not a release of
unreviewed production optimizations.

## Contract O1

Each track elaborates its idea against current source, implements and runs a bounded
prototype, critically reviews correctness and benefit, and writes an ordered execution
plan. The first execution step is the isolated prototype. Production integration requires
an explicit ownership handoff and the optimizer documentation stewardship checks.

Preserve the contracts in the [BREP plan](../brep-compiler-optimization-plan.md).
In particular, purity does not justify discarding faulting or nonterminating evaluations;
retain wrapper scalars, conversions, evaluation guards and floating arithmetic grouping.
Compare against existing compiler passes and the JIT rather than assuming either does no work.

## Ownership

The shared checkout is `C:/Users/cdigg/git/plato`. The supervisor owns tracker changes,
this document, shared compiler files, integration, staging and publication. Agents write
only their assigned experiment subtree, design document, dated report and private evidence.
For each slug below those paths are:

- `tools/experiments/optimizer-exploration/<slug>/`
- `docs/design/optimizer-exploration/<slug>.md`
- `docs/reports/optimizer-<slug>-2026-10-25.md`
- `.temp/optimizer-exploration/<slug>/`

| Track | Slug | Owner | Readiness |
| --- | --- | --- | --- |
| [compiler-506](../../../tracker/issues/compiler-506.md) | [multi-consumer](multi-consumer.md) | multi_consumer | Independent prototype |
| [compiler-507](../../../tracker/issues/compiler-507.md) | [record-elimination](record-elimination.md) | record_elimination | Independent prototype |
| [compiler-508](../../../tracker/issues/compiler-508.md) | [array-facts](../../archive/optimizer-exploration/array-facts.md) | array_facts | Independent prototype |
| [compiler-509](../../../tracker/issues/compiler-509.md) | [output-sizing](../../archive/optimizer-exploration/output-sizing.md) | output_sizing | Independent prototype |
| [compiler-510](../../../tracker/issues/compiler-510.md) | [loop-unswitch](../../archive/optimizer-exploration/loop-unswitch.md) | loop_unswitch | Independent prototype |
| [compiler-511](../../../tracker/issues/compiler-511.md) | [grid-reuse](grid-reuse.md) | grid_reuse | Independent prototype |

Agent slots determine dispatch order. Tracks may read each other's completed evidence,
but no implementation depends on an unverified sibling prototype. Standalone projects must
keep build outputs private and must not rebuild shared project references. Each owner keeps
a checkpoint and records running processes, actual checks, limitations and requests.

## Verification and integration

Experiments require executable correctness checks including unsafe counterexamples and
explicit refusal conditions. Performance claims require warmed paired runs, consumed outputs,
allocation observations where applicable, and source/runtime fingerprints. Abstract models
and handwritten generated-shape comparisons must be labeled; neither proves a live compiler
transformation works. Reports record measurements; design documents state contracts.

The shared status reports and root scratch notes belong to another session. Isolated builds
may invoke the existing C# build recorder through an import with its snapshot and evidence
root redirected to the track's private directory. This preserves build logs and categorized
errors without the PowerShell wrapper overwriting those shared reports.

Before integration the supervisor reviews artifacts, reruns meaningful checks against stable
inputs, and serializes commits using explicit paths. Production source changes additionally
require the [optimizer stewardship workflow](../../../tools/optimizer-docs/MAINTENANCE.md).
Incomplete production steps remain explicitly tracked; a prototype is not a shipped pass.

Actual experiment outcomes and the supervisor's review are recorded in the
[dated integration report](../../reports/optimizer-exploration-2026-10-25.md).
The next milestone in each tracker item is the plan's actual-TIR candidate census and
integration decision. Those milestones remain open after the isolated experiments finish.
