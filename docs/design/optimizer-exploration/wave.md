# Additional optimizer exploration

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
| compiler-506 | multi-consumer | multi_consumer | Independent prototype |
| compiler-507 | record-elimination | record_elimination | Independent prototype |
| compiler-508 | array-facts | array_facts | Independent prototype |
| compiler-509 | output-sizing | output_sizing | Independent prototype |
| compiler-510 | loop-unswitch | loop_unswitch | Independent prototype |
| compiler-511 | grid-reuse | grid_reuse | Independent prototype |

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
