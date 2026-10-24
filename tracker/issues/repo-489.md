---
id: repo-489
title: Consolidate codegen verification and isolate CLI responsibilities
type: debt
status: done
priority: p2
effort: M
risk: low
area: repo
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: []
---

## Done means

- [x] Generated-C# regression fixtures share compilation/reference policy and preserve executable assertions.
- [x] CLI source loading, linting and generated-file persistence have separate reusable responsibilities, with equivalent command results and generated output.
- [x] Focused and integrated verification is recorded, each milestone committed, and remaining repo-wide debt identified through existing tracker items.

## Scope

Apply the Platonic Coder priorities to the next incremental pass after repo-468. Preserve public CLI entry points, backend selection, input ordering, diagnostics and exit codes. Do not modify language semantics or generated runtime representations. Existing correctness and architecture backlog remains explicit; this issue does not certify the entire repository free of debt.

Root PROGRESS.md belongs to plato-448; this issue and milestone commits hold this mission's checkpoint instead.

Milestone 1: shared in-memory C# compilation preserves the fixture-specific parse/optimization options. Recorded build passed; focused emission and optimizer checks passed (43 tests, 77 seconds). The reference exclusion retains the existing decision to use embedded runtime wrappers, avoiding duplicate types from Ara3D.Geometry.dll.

Milestone 2: Program retains its public Main/Lint entry points and delegates to focused commands. SourceFiles returns ordered discovery results and GeneratedFiles owns writer persistence. The CLI build passes; tools/check-cli-behavior.py matches the original executable across every command case and emitted file after excluding only existing wall-clock banners.

## Verified outcome

Work commits: ed28e14f shares the generated-C# test compiler; 1abde52c separates CLI responsibilities. The independently tracked correctness fix 0067ad05 rejects ignored generation flags (compiler-490, closed).

| Check run for this mission | Result | Seconds |
|---|---|---:|
| Focused generated-code and optimizer tests | 43 passed | 77 |
| Original/refactored CLI comparison before the intentional flag change | 20 command cases and 19 generated files matched, excluding creation-time banners | 7 |
| Final CLI executable checks, including supported and rejected flags | 27 cases passed | 6 |
| Recorded CLI build | PASS | 9 |
| Shipping stdlib strict lint | PASS | 25 |
| Full PlatoTests, including both regression ceilings | 346 passed | 212 |
| Studio integration battery | SDK build, geometry tests and both lints passed; external frozen-artifact check failed | 170 |

The external failure is the existing repo-473 drift in studio/ara3d-sdk/src/Plato.Intrinsics/PlatoList.cs, PlatoBuffer.cs and Number.cs. A targeted rerun confirmed exactly those paths. No SDK source or frozen manifest was changed. Integration is therefore not wholly green, but no failure was attributed to this cleanup.

Repository gate results are appended to docs/gate-log.md. Full logs are under .temp/gate-logs/20261024-170655-885448 and .temp/platonic-pass; a copy of the measured snapshot is .temp/platonic-pass/status-report-snapshot.verified.json. Original shared edits to the status JSON/HTML and root PROGRESS.md were preserved.

Rerun: python tools/check-cli-behavior.py; python tools/record-gates.py; powershell -NoProfile -File C:/Users/cdigg/git/studio/tools/check-all.ps1 -PlatoRoot C:/Users/cdigg/git/plato.

## Remaining architecture work

This pass is not a claim that the repository is free of technical debt. Existing items identify the next substantive work: plato-390 centralizes verification policy now repeated across scripts, tests and the warm server; compiler-471 removes machine-relative parser dependencies upstream; compiler-483 requires executable evidence for suspected optimizer ordering and identity hazards before widening optimization. The latter hypotheses are not confirmed bugs. These larger changes were not started in this pass.

Publishing remains blocked: automatic approval review rejected pushing main because it includes pre-existing shared-session commits. All milestones are local; no alternate remote or branch was used.
