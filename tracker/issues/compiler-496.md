---
id: compiler-496
title: FunctionInstance never assigns Interface so selection ignores interface distance
type: bug
status: in-progress
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

FunctionInstance.cs declares a get-only Interface property but never assigns it. FunctionSelection.ScoreFunction (formerly copied into each writer's PlatoAnalyzer) treats Interface == null as concrete, returning -100 before examining OwnerType.DepthTo. A parsed Item implementing IChild inheriting IBase therefore gives both interface candidates -100, instead of using distance. Intrinsic precedence still works. This can make implementation selection depend on enumeration order rather than interface specificity.

Found while preserving behavior for repo-493; deliberately not fixed in that refactor. Constructor and selector source inspection plus the real-source FunctionSelectionTests fixture reproduce it. Correcting the constructor is small, but it changes selection across all writers and needs output review.

## Done means

- [x] Establish which FunctionInstance kinds should carry Interface and assign it from the correct declaration/implementation context.
- [x] Cover nearer and inherited interface candidates through normal construction, including ties and intrinsic/concrete precedence.
- [x] Review changed output and run backend tests before accepting the semantic change; update the repo-493 preservation test accordingly.

## Implementation and focused verification

Bound interface declarations and implementations receive Interface from their existing InterfaceImplementation context. Concrete/field functions and standalone interface declarations/extensions have no binding and keep null. The old reflection-based test and preservation assertion are replaced by normal parsed-source construction.

Output review exposed two necessary companion changes. ReifiedType previously retained the first same-signature library body even when the writer selected a more-specific one, producing a C# throwing stub; it now retains the lower-scored body using FunctionSelection, keeping first arrival on equal scores. Dormant InterfaceName-only array exclusions were removed from every writer so restoring metadata cannot silently remove public members. These are not new allowlists: existing field/name/backend representability filters remain in place.

The red run reproduced three failures through normal construction. Final focused selection/monomorphization tests pass (20 tests, 22 s), including compiling and executing emitted C# to observe the interface result 22 and concrete override 33. Native tests pass (8 tests, 29 s, MSVC and nvcc). The comparison across C#, TypeScript, Rust, GLSL, C++ and CUDA changes exactly the fixture's selected result from 11 to 22 in each dialect; all 509 demo/legacy/foundation files are unchanged after normalizing only creation timestamps. No additional degraded bodies were introduced. Logs, output snapshots and comparison.json are under .temp/compiler-496.

At the implementation milestone the full PlatoTests gate was still running; its completed result is recorded below. Studio integration passed both lints, the SDK build and geometry tests, but its frozen-artifact gate remains red for the same external PlatoList.cs, PlatoBuffer.cs and Number.cs files tracked in repo-473; those files and the manifest were not modified.

Root PROGRESS.md belongs to another mission. This issue and the explicit work commit serve as the checkpoint. Pre-existing dirty status JSON/HTML were backed up before running the recorders and restored byte-for-byte after final verification.

## Final verification

Fix commit: c508809f (pushed). All acceptance criteria are complete. The normal-construction regression fails against the old compiler and passes after the fix; the permanent execution assertion confirms the chosen body is emitted, compiles, and returns the expected result.

| Check run for compiler-496 | Result | Seconds |
|---|---|---:|
| Final recorded CLI build | PASS | 13 |
| Shipping strict lint and warning ceiling | PASS | 21 |
| Full PlatoTests and both regression ceilings | 361 passed, zero failed | 334 |
| Selection, executable C# and monomorphization tests | 20 passed | 22 |
| Native writer suite | 8 passed, none skipped; MSVC and nvcc | 29 |
| All-writer output comparison across fixture/demo/legacy/foundation | Only the six intended fixture body changes; corpus output unchanged | 124.3 |
| Studio legacy / forward strict lint | PASS / PASS | 43.6 / 42.7 |
| Studio SDK build / geometry tests | PASS / PASS | 4.6 / 13.4 |
| Studio frozen-artifact check | FAIL, unchanged external repo-473 drift | 1.5 |

The six changed fixture outputs replace the ancestor result 11 with the nearer-interface result 22; the concrete override remains 33. All 509 non-fixture output files match the baseline after removing only creation-time banners. Every code-generation invocation exited successfully and there were no additional degraded bodies. The external integration failure is not fixed or waived: Studio's three SDK files remain outside this change.

Evidence: .temp/compiler-496/selection-red.trx, selection-body.trx, native.trx, before.json, after.json, comparison.json, per-dialect diffs and check-all.log; full gate logs are .temp/gate-logs/20261024-214551-178174. docs/gate-log.md records the final run. The measured status snapshot is .temp/compiler-496/status-verified.json; the pre-existing dirty status JSON/HTML were preserved.

Rerun: python tools/record-gates.py; dotnet test tests/Plato.CppWriter.Tests/Plato.CppWriter.Tests.csproj -c Release; powershell -NoProfile -File C:/Users/cdigg/git/studio/tools/check-all.ps1 -PlatoRoot C:/Users/cdigg/git/plato. Session output comparison: python .temp/compiler-496/compare.py after (uses the preserved baseline).
