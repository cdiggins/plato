---
id: repo-493
title: Consolidate writer selection and test fixture helpers
type: debt
status: done
priority: p2
effort: M
risk: med
area: repo
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: []
---

## Problem

The Greenhouse audit (repo-492) confirmed duplicated implementation-selection policy in all five writers and duplicated emission/reflection setup in the type-bound and function-bound test fixtures. Changes currently require several synchronized edits. This is a medium-sized cleanup in independently verifiable steps; it must preserve backend output and each distinct regression assertion.

## Done means

- [x] Share ScoreFunction and ChooseBestFunction across the five writers, preserving intrinsic precedence, concrete precedence, interface distance, tie counts and first-candidate ordering; retain thin compatibility wrappers if public consumers require them.
- [x] Share source-to-CSharpWriter setup, wrapper construction/invocation and the small runtime prelude in the constraint emission fixtures, reusing CSharpTestCompilation; preserve separate type-bound and function-bound assertions and negative controls.
- [x] Verify focused tests and representative output for affected backends, then record the required integration gates.

## Follow-on candidates requiring design review

Array-receiver case parameterization moved to repo-497: the user explicitly selected only the first two audit recommendations for this implementation pass.

Shared eta-forwarding recognition in the C#, Rust and TypeScript body writers; shared parsing/repository-location test support across projects. Keep these separate from the initial cleanup if dependencies or semantics widen the change. Do not merge memo-key equality with language-value equality, or Rust/TypeScript emission pipelines merely because their bodies match.

Evidence and validation boundaries: `docs/reports/duplication-audit-2026-10-24.md`.

## Milestone 1

Shared compiler FunctionSelection owns the unchanged selection algorithm. All five public backend PlatoAnalyzer APIs delegate to it. The existing Interface-construction defect is tracked separately as compiler-496 and is intentionally preserved; a focused test injects interface metadata only into isolated candidates to cover the otherwise inactive distance branch.

Verification: recorded CLI build passed (17.5 s), final test build passed (10.4 s), selection plus Rust/TypeScript regressions passed (9 tests, 28 s), final isolated selection tests passed (5 tests), native writer suite passed (8 tests, 34 s; MSVC and nvcc exercised). All 27 demo output files across six dialects match the baseline after normalizing only creation-time banners (4.78 s comparison). Evidence: .temp/repo-493 and .temp/csharp-build-logs/repo-493-*.log. Sandbox restore initially failed without compiler diagnostics; the recorded build succeeded with external dependency access.

Root PROGRESS.md belongs to another mission; checkpoints are recorded here. Pre-existing status-report changes are backed up under .temp/repo-493 and will be preserved.

## Milestone 2

GeneratedCSharpFixture shares source emission, resolution/file assertions and the minimal runtime prelude. GeneratedValueReflection shares constructor/method wrapper conversion and unwrapping; sum-type fixtures reuse only unwrapping because their prelude differs. CSharpTestCompilation remains the sole generated-C# compiler. No fixture caches or test cases were removed. All 17 existing test bodies are text-identical, and all 17 pass (5 s) after a recorded build (12.4 s). Evidence: .temp/repo-493/helpers.trx.

## Final verification

Implementation commits: 38e2f9e3 (shared function selection) and 65e4122a (shared generated-code test helpers), both pushed. Scope is complete; array-test parameterization is separately ready as repo-497.

| Check run for this mission | Result | Seconds |
|---|---|---:|
| Final repository CLI build | PASS | 7 |
| Shipping-tier strict lint | PASS, no errors; warning ceiling satisfied | 20 |
| Full PlatoTests, including both regression ceilings | 358 passed, none failed | 288 |
| Focused helper fixtures | 17 passed; original test bodies unchanged | 5 |
| Selection and Rust/TypeScript regressions | 9 passed | 28 |
| Native writer suite | 8 passed, none skipped; MSVC and nvcc exercised | 34 |
| Six-dialect baseline output comparison | 27 files identical except creation timestamps | 4.78 |
| Studio legacy / forward lints | PASS / PASS | 15 / 24.1 |
| Studio SDK build / geometry tests | PASS / PASS | 4.9 / 18.1 |
| Studio frozen-artifact check | FAIL, pre-existing repo-473 | 1.3 |

The external failure was rechecked: only studio/ara3d-sdk/src/Plato.Intrinsics/PlatoList.cs, PlatoBuffer.cs and Number.cs differ from its frozen manifest. None is part of this change; the manifest was not refreshed. This mission is verified locally and in the affected backends, but the whole Studio battery is not green.

The new compiler-496 issue records the constructor's unassigned Interface property, which bypasses specificity ranking today. The refactor preserves that behavior; assigning the property belongs to a separately reviewed semantic fix.

Logs: .temp/repo-493 (TRX, before/after generated-output snapshots and Studio logs), .temp/csharp-build-logs/repo-493-*.log, and .temp/gate-logs/20261024-210800-507962. docs/gate-log.md records the final suite. The pre-existing dirty status JSON/HTML were restored byte-for-byte; the measured snapshot is retained as .temp/repo-493/status-verified.json.

Rerun: python tools/record-gates.py; dotnet test tests/Plato.CppWriter.Tests/Plato.CppWriter.Tests.csproj -c Release; powershell -NoProfile -File C:/Users/cdigg/git/studio/tools/check-all.ps1 -PlatoRoot C:/Users/cdigg/git/plato. The repeatable baseline comparison script for this session is .temp/repo-493/compare.py.
