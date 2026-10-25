# Additional optimizer exploration — 2026-10-25

The user authorized a dedicated subagent for each proposed optimization: elaborate, test,
critically review, write an execution plan and begin executing it. Each track completed
its isolated prototype step. None installed a production compiler pass. The
[coordination plan](../design/optimizer-exploration/wave.md) links every design and tracker item.
All subagents stopped writing before supervisor integration; no experiment processes remained.

## Outcomes

| Exploration | Executed evidence | Critical disposition |
| --- | --- | --- |
| [Multiple consumers](optimizer-multi-consumer-2026-10-25.md) | Real runtime wrappers and handwritten C# loops agreed bitwise on 209 cases; roughly 1.8x microkernel traversal improvement, zero allocation saving | Promising narrow target; actual TIR provenance and callback totality remain unproved |
| [Temporary records](optimizer-record-elimination-2026-10-25.md) | C# eager-field and conversion checks passed; both benchmark shapes allocated zero bytes | No incremental benefit established; inspect real surviving JIT cost before adding a pass |
| [Array facts](optimizer-array-facts-2026-10-25.md) | Exhaustive small-domain Python interval and indexing checks passed | Useful analysis groundwork; conversion order, ownership and actual TIR facts are prerequisites |
| [Exact output sizing](optimizer-output-sizing-2026-10-25.md) | Extracted runtime FlatMap compared with handwritten exact allocation; larger inputs allocated about 36% less | Promising allocation mechanism; timings noisy, static width and overflow fallback required |
| [Loop splitting](optimizer-loop-unswitch-2026-10-25.md) | Python tree model passed differential and unsafe-hoisting cases | No measured runtime benefit; begin with immutable resolved Boolean leaves only |
| [Grid reuse](optimizer-grid-reuse-2026-10-25.md) | Model reduced 64x64-grid sample calls from 16,384 to 4,225 with 130 peak cached value entries | Call-count evidence only; actual callback identity, argument equality and totality proof remain difficult |

These are different evidence levels, not comparable speedup rankings. Python models do not
validate generated C#, and handwritten loops do not validate a compiler transformation.
The reports retain methodology, limitations, commands and source fingerprints.

## Supervisor review and reruns

The supervisor inspected the actual prototypes and plans and reran every experiment family.
Review found an exact-output test that did not require its expected callback exception to
occur. An explicit assertion was added; the corrected recorded build and executable passed.
No production semantics changed. Runtime fault-order, mutable-host-source and floating
reassociation counterexamples remain explicit refusal evidence, not proofs that a compiler
recognizer enforces those refusals.

| Check actually run by supervisor | Result | Seconds |
| --- | --- | --- |
| Array-facts model | 7 tests passed | 0.209 test runner |
| Record admission model and recorded C# experiment | Passed, including 8 fault combinations | 7.31 combined command |
| Multi-consumer recorded C# experiment | Passed, including 209 bitwise cases and unsafe counterexamples | 9.07 command |
| Loop-splitting model | 10 tests passed | 0.016 test runner |
| Grid-reuse model | 8 tests passed | 0.006 test runner |
| Exact-output corrected C# experiment | Recorded build and executable passed | Timing retained in private build log |
| Local document links | Passed | — |
| Studio frozen-artifact checksum gate | Failed; existing repo-473 | 0.74 |
| Strict legacy stdlib lint | Passed | 21.55 |
| Strict forward stdlib lint | Passed | 24.34 |
| SDK Release build | Passed | 5.34 |
| SDK geometry tests | Passed | 35.54 |

Isolated build logs and categorized errors live under `.temp/optimizer-exploration/`.
The multi-consumer build emitted a NuGet vulnerability-feed warning but no compiler errors.
The other recorded experiment builds were clean. Private output paths preserved another
session's dirty status reports. During integration, that session was also modifying the
PlatoTests project, contract tests and the repository gate recorder; no such files belong
to this exploration.

## Tracking and handoff

Compiler-506 through compiler-511 remain in progress: their prototype, review and plan
milestones are complete, while actual-TIR candidate collection and the production integration
decision remain unchecked. The next bounded production step is described separately in each
plan. Continuing requires deliberate shared-source ownership and the optimizer documentation
stewardship workflow; prototype evidence does not release general fusion or unsafe indexing.

[repo-512](../../tracker/issues/repo-512.md) records an independently reproduced tooling
defect: direct project mode in the C# build recorder calls a missing function and cannot build
or save evidence. The working log-import path was used; the small tooling fix remains separate.

Rerun commands are in each dated track report. No new compiler flags or runtime API changes
were introduced. The experiment artifacts and tracker milestones were committed on the
existing branch; publication and final broad-gate results are recorded below when known.

The Studio battery ran once, taking 87.94 seconds overall. Its checksum failure matches
[repo-473](../../tracker/issues/repo-473.md), the existing external SDK artifact drift;
none of those SDK sources was edited here. The full log and timing rows are in
`.temp/optimizer-exploration/integration/`. Reproduce with
`powershell -File C:/Users/cdigg/git/studio/tools/check-all.ps1 -PlatoRoot C:/Users/cdigg/git/plato`.
No all-green repository claim is made.

The separate `record-gates.py`/PlatoTests run was not started: another session was actively
editing that recorder and test inputs. Repeated runs against those moving inputs would not
establish a stable integration result. Production compiler, runtime and stdlib sources were
unchanged by this exploration. Optimizer source-evidence refresh was consequently unnecessary.

Commits remain local. The branch also contains another session's unpublished tracker work;
this exploration did not publish that work or treat the known red integration gate as green.
All exploration agents completed their bounded assignments; no background subagent remains
running. The open tracker milestones describe future production work, not unattended execution.
