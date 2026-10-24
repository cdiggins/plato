---
id: compiler-482
title: Eliminate singleton arrays in lowered FlatMap branches
type: feature
status: done
priority: p1
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed: 2026-10-24
links: [docs/design/brep-compiler-optimization/fusion-filter.md]
---

Implement only reviewed F1: raw typed conditional singleton arms in an existing lowered FlatMap become conditional append; retain original fallback evaluation and copying. No new call matching, Take rewriting, local fusion, capacity policy, or public ABI change. Review: docs/reports/brep-compiler-optimization-review-2026-10-24.md (CR-01, CR-02, CR-11, CR-12).

## Done means

- [x] Prove a minimized current BREP projection-candidate shape matches under --loops --fuse without --inline or stripped coercions.
- [x] Preserve exact element/result typing, selected branch evaluation, conversions, source order and fallback behavior in compiled net8/C#12 tests.
- [x] Verify retained fallback for unsupported shapes and existing optimizer/emission regressions with explicit flag combinations.
- [x] Measure warmed allocation reduction against the same lowered ordinary path with equal outputs and retained source/evidence.
- [x] Update the optimizer guide and record integrated validation, including any pre-existing failures and unrun whole-BREP checks.

## Implementation and review

The loop emitter recognizes only a raw typed conditional callback in an already lowered FlatMap. A singleton arm appends its element directly; another arm retains evaluation and copying through an explicitly typed IReadOnlyList. Exact types and conversion context are preserved. No Take simplification, producer scheduling, capacity policy, lowerer traversal or public runtime API changed. Both singleton orientations and two singleton arms are supported.

Independent review found no production correctness issue. Its test-harness finding was fixed: the evidence directory derives from tracked stdlib, so a fresh checkout need not contain .temp. The reviewer confirmed the final source fingerprints after this fix and the additional both-singleton test.

## Validation — 2026-10-24

Inputs: parent revision 45019b46 plus this issue's implementation and tests; final SHA-256 fingerprints retained in `.temp/compiler-482/verified-inputs.json` and checked unchanged after integration. Full logs and generated sources are retained under `.temp/compiler-482/` and `.temp/brep-compiler-plans/`; they are working evidence, not tracked artifacts.

| Check | Result | Seconds |
| --- | --- | --- |
| Fresh pre-change BREP-shaped typed-IR and generated C#12 preflight | PASS | 12.4 |
| Feature tests, including compiled values/faults/conversions and conservative refusals | 16 passed | 17.1 |
| Existing optimizer and emission regressions | 36 passed | 11.8 |
| Fresh Plato.CLI Release build | PASS | 20 |
| Strict shipping-stdlib lint | PASS | 38 |
| Full PlatoTests, including both ratchets | 346 passed, no failures | 255 |
| Studio strict legacy / forward lint | PASS / PASS | 15.2 / 20.2 |
| Studio SDK Release build / geometry tests | PASS / PASS | 12.4 / 27.5 |
| Studio frozen-V1 checksum gate | FAIL, existing repo-473 | 1.8 |

The Studio battery ran once. Its frozen-artifact failure matches [repo-473](repo-473.md): `studio/ara3d-sdk/src/Plato.Intrinsics/PlatoList.cs`, `PlatoBuffer.cs` and `Number.cs` differ from the checksum manifest. Those source files were not modified by this work. The read-only diagnostic check reproduced exactly those paths; no baseline was updated. Cross-repository green status is not claimed.

The repository gate recorder ran through `.temp/brep-compiler-plans/run-integration-gates.py`, which redirects only snapshot/history destinations into private evidence to preserve another session's dirty status reports. It used the ordinary build, lint and full-test commands, retaining their complete logs in `.temp/gate-logs/20261024-015621-815469/`.

Allocation ablation used the same materialized Number input, --loops --fuse and no --inline. A same-type TIR callback wrapper forced the ordinary emitter without adding a runtime conversion. Both versions were compiled in Release with real wrapper runtime types and C#12, warmed for 10,000 calls, then measured over 1,000 calls using GC.GetAllocatedBytesForCurrentThread. Setup, reflection and assertions were outside measurement; results were consumed and compared.

| Accepted elements | Optimized bytes/call | Ordinary bytes/call | Saved bytes/call |
| --- | --- | --- | --- |
| 8 | 176 | 432 | 256 |
| 64 | 1,144 | 3,192 | 2,048 |
| 512 | 8,384 | 24,768 | 16,384 |

This measured 32 bytes removed per accepted Number singleton. The result List and unrecognized rejected-arm arrays remain. Whole-BREP regeneration, corpus laws/digests and end-to-end timing were not run; no whole-mesher speedup is claimed.

Rerun focused verification with `dotnet test tests/PlatoTests/PlatoTests.csproj -c Release --filter FullyQualifiedName~FlatMapFilterTests`. The allocation test writes its protocol/results under `.temp/compiler-482/`. Rerun the repository gates without altering shared reports using `python tools/record-gates.py --dry-run`; run the Studio battery with `powershell -File C:/Users/cdigg/git/studio/tools/check-all.ps1 -PlatoRoot C:/Users/cdigg/git/plato`.
