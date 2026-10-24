---
id: compiler-491
title: Differential law runs report success when both engines fail
type: bug
status: done
priority: p1
effort: S
risk: low
area: compiler
sprint:
created: 2026-10-24
closed: 2026-10-24
links: [plato-brep:TKT-1015]
---

## Problem

Without an explicit baseline, differential runs return zero when both engines fail or error, including budget errors. Backend agreement does not establish that a law passed.

## Done means

- [x] All backends return failure for fail, error, and vacuous verdicts; differential mismatches also fail.
- [x] Pure policy tests cover matched failure/error/budget outcomes and mismatch-only witness cases; CLI regressions cover matched pass/fail/error and preserve explicit baseline behavior.
- [x] Document the exit contract and record verification without overwriting concurrent status reports.

## Scope

Preserve skipped and host-obligation semantics and the explicit baseline comparison mode. No compiler, runtime, or stdlib algorithm changes. Related downstream ticket: plato-brep TKT-1015.

## Verification

- Isolated Release builds: test project passed with no warnings; CLI passed with eight existing nullable warnings. Build inputs: upstream 2dec1190 plus the five owned source/test/doc files; fingerprints in `.temp/law-run-exit/verified-inputs.json` stayed unchanged through the gates.
- `dotnet test .temp/law-run-exit/artifacts/bin/Plato.Interpreter.Tests/release/Plato.Interpreter.Tests.dll --filter "FullyQualifiedName~LawRunStatusTests|FullyQualifiedName~DifferentialBackendTests"`: 13 passed, zero skipped, 29 seconds.
- `python tools/check-law-run-exit.py --cli .temp/law-run-exit/artifacts/bin/Plato.CLI/release/Plato.CLI.dll`: 13 process cases passed, 114.8 seconds. Matching fail/error/vacuous verdicts exit 1; matching pass exits 0; explicit baseline comparison preserves its previous exit policy.
- `python tools/check-cli-behavior.py --cli .temp/law-run-exit/artifacts/bin/Plato.CLI/release/Plato.CLI.dll`: 27 cases passed, 26 output files, 13.9 seconds.
- Private build recorder: loaded `tools/record-csharp-build-errors.py` with importlib and assigned its `SNAPSHOT` to `.temp/law-run-exit/build-snapshot.json` before calling `main` on retained build logs. This preserves another session's dirty shared status snapshot and HTML. No shared report or PROGRESS.md was modified.
- Initial CLI build caught CS0104: `LawRunOutcome` was ambiguous between the new predicate and Navigation's record. Renamed the predicate to `LawRunStatus`; final builds have zero errors. Original log and categorized error remain in the private snapshot.
