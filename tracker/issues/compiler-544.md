---
id: compiler-544
title: Borrow large readonly native record parameters
type: feature
status: done
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-11-01
closed: 2026-11-01
links: []
---

## Done means

- [x] Borrow large closed readonly record parameters using native layout; preserve writable parameters, small values and owned return signatures.
- [x] Native execution tests cover exact signatures, temporary arguments, copied string payloads and parameter assignment.
- [x] Frozen BREP candidate passes aligned stage, law, decode and mesh gates.
- [x] Alternating native measurements record performance and correctness on every sample.

Focused verification: cpp-large-parameters recorded build has zero errors; LargeParameterTests, StringBorrowingTests, ScratchArenaTests and SharedCallsParityTests pass 9/9 (.temp-brep-large-parameters-tests.log). Full BREP and performance gates are pending; no speedup claimed.

Candidate1fbae01c full gate: native25-unit build, stage24/24, laws5/5 each backend, decode90/90, SAM1+2workers and c211539faces11610triangles pass. Layout probe finds 67 borrowed closed record types: BrepCurve680B, BrepSurface776B, BrepModel272B; Point3D/BrepFace24B stay values. Performance measurement waits for other jobs to finish. Evidence: consumer artifacts/cpp-validation/candidate-1fbae01c.

Matched A/B evidence (consumer artifacts/cpp-validation/ab-1fbae01c-*): unpinned c211 median 12780.415 ->12030.571ms (ratio0.9413), pinned CPU0 9213.139 ->8721.015ms (0.9466). SAM ratios1.0317 unpinned and1.0557 pinned. All56 sample meshes pass. Per-round c211 ratios include regressions (unpinned0.8917/0.9651/1.0456; pinned1.0412/0.9801/0.8674), so this is a mixed, noisy two-model result, not a demonstrated broad win. Shipping pins stay unchanged. Array reads still copy before borrowed calls; investigate separately rather than silently attributing this composite experiment solely to parameter passing.

Required compiler closeout: record-gates.py passes CLI build, strict lint (0errors/29warnings) and616PlatoTests. Earlier full forward conformance failures were not rerun by this default packet and remain separate. Logs .temp/gate-logs/20261101-164443-061153/.
