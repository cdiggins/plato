---
id: repo-513
title: Separate slow corpus tests from routine regression runs
type: debt
status: in-progress
priority: p2
effort: M
risk: low
area: repo
sprint: 
created: 2026-10-25
closed:
links: []
---

## Done means
- [x] Identify expensive compiler tests from measured timings and keep focused regression checks in the default run.
- [x] Tag slow checks and diagnostic reports; provide default, slow, and full commands while preserving ratchets in recorded gates.
- [x] Verify default and full selections and record timing and test results.


## Outcome and verification (2026-10-25)

Compiler, interpreter, and navigation projects load a shared default filter only when no command-line filter is supplied. Slow corpus and compiled-law checks remain available through `TestCategory=Slow`; report-only compiler tests are explicit diagnostics. Full regression selection is `TestCategory!=Diagnostic`. Recorded compiler gates always use the full selection, save TRX timings, and reject empty test runs.

The TypeScript member-call regression now uses a minimal parsed program and checks both method calls and field access with no fallback bodies. Navigation retains full-corpus update equivalence as a slow parameter case while running the same edit transitions and cache assertions on a small program by default. No regression assertions or tests were deleted. An audit of the TRX files confirms every baseline compiler regression appears in the full run; only five assertion-free reports left that selection.

| Run | Result | Seconds |
|---|---|---:|
| Original compiler suite | 368 passed | 388 |
| Final default compiler suite | 352 passed | 49 |
| Full compiler recorded gate, including both ratchets | 363 passed | 199 |
| Interpreter default / slow | 94 / 19 passed | 77 / 148 |
| Navigation default / slow | 70 / 4 passed | 107 / 117 |
| Explicit diagnostic opt-in | 1 passed | <1 |
| CLI build / shipping lint | PASS / PASS | 15 / 34 |
| Studio battery | External SDK checksum failure only; existing repo-473 | 81 |

Timings are observations on a shared machine, not thresholds. The TypeScript fixture itself went from 23 seconds in the baseline to 0.07 seconds in the final run. The Studio battery passed both lints, SDK build, and geometry tests. repo-514 records an existing private-artifact fixture lookup defect; an initial private run failed sixteen cases, then passed after copying the source fixture into the temporary location it expected. Final compiler verification used the normal output layout.

Evidence: `.temp/test-speed/*.trx`, `.temp/test-speed-*.log`, and `.temp/gate-logs/20261025-002836-216277/`. The pre-existing dirty status report files are not part of this commit; the measured snapshot is preserved at `.temp/test-speed/status-verified.json`. Rerun commands and selection policy are in `tests/README.md`. The gate recorder's empty-run rejection and timing arguments were also verified with synthetic command results.
