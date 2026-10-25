# Exact-output geometry qualification — 2026-10-25

This follow-up to [compiler-509](../../tracker/issues/compiler-509.md) evaluates the
existing default-off exact-output pass without broadening its matcher. The experiment
sources are in `tools/experiments/optimizer-geometry/exact-output/`; generated code,
compiler/source fingerprints, build logs and raw samples belong under
`.temp/optimizer-geometry/exact-output/`.

## Source-backed candidate selection

| Candidate | Library evidence | Relationship to benchmark |
| --- | --- | --- |
| Triangle corner flattening | `meshes.library.plato`, `Points(TriangleArray3D)` calls `t.Points`; `spatial-primitives.library.plato`, `Points(Triangle3D)` returns `[t.A,t.B,t.C]` | The unchanged caller is inspected separately. `TriangleCorners` faithfully expands these expressions over records with the same fields, omitting unrelated interface implementations. |
| Quad corner flattening | The corresponding `Points(QuadArray3D)` and `Points(Quad3D)` return four ordered corners. | `QuadCorners` uses the same direct expansion and explicit record substitution. |
| A-C quad triangulation | `QuadTriangleFaces` and `FanTriangles` in `meshes.library.plato` define the A-B-C/A-C-D split. | `SplitQuads` is a newly authored representative fixed-face kernel using Integer fields instead of VertexIndex records. It is not the unchanged library implementation. |
| Indexed triangle unwelding | `meshes.library.plato` builds triangle positions by indexing stored face corners. | `GatherTriangles` is a newly authored representative kernel combining indexed lookup with direct fixed-width output; it tests bounds/fault preservation as well as allocation. |
| Opaque helper control | `TriangleArray3D.Points` calls another function instead of containing a raw literal. | `HelperCorners` retains that helper-call shape and is expected to refuse without inlining. Actual applicability must be checked from generated code. |

The source scan did not find direct raw array-literal FlatMap callbacks in the shipping
geometry tier. Many other sites perform filtering, marching-cell expansion, MapRange-based
fan triangulation or nested variable-output flattening. A fixed conceptual output count
does not establish that this narrowly structural pass sees an eligible literal.

## Qualification protocol

The supervisor supplies a frozen compiler snapshot including the compiled wrapper runtime.
The runner refuses to substitute working-tree assemblies or stdlib: all compiler inputs
come from the frozen snapshot and `snapshot/source/stdlib`. It validates manifest hashes,
copied private DLLs and actual loaded assembly paths before/after each phase, and binds
the built harness to its source and snapshot so `--no-build` cannot silently reuse stale code.
Generated off/on kernels use the
same flags except `ExactOutputAllocation`, keep wrapper scalars and callback arrays, and
compile with Roslyn Release optimization. Source and assembly SHA-256 hashes are recorded.
Generation, Roslyn compilation and correctness-check durations are recorded separately
before warmup and measured kernel invocation.

Correctness checks compare every output value and ordering with explicit reference results,
including empty, repeated and degenerate geometry, nonfinite coordinates, winding, bad vertex
indices and the first fault in an observed vertex list. The generated shape check requires
an exact-array guard in intended positive kernels and no guard in the opaque helper control.

Timing is reserved separately by the supervisor. It warms paired delegates, alternates
measurement order, constructs inputs outside measurements, consumes results, records
thread-local allocations and keeps raw samples. It measures generated geometry operations,
not a full mesher or application. Building faithful extracted kernels can establish benefit
at an eligible shape; it cannot establish unchanged-library adoption.

## Verified results

The frozen snapshot records revision `b612c392b8998ad719f70027eba1ac7c09d91779` and a
separate `GeometryRuntime.dll` built from the frozen Intrinsics sources under cpu64.
The writer SHA-256 is `d79b7f6dce387a1689f15abec259cc71a26f18ebeb2d1b5510927300782cfa4e`;
runtime SHA-256 is `7eb42bf7946a95026bce023b96c3a8ff080a8183900bd714338f9d2b9b275241`.
Full manifest and phase fingerprints hold the remaining identities. Pre/post manifest,
source, copied-DLL and loaded-assembly validation passed.

| Focused check | Result | Seconds |
| --- | --- | ---: |
| Latest private harness build | Passed, no errors/warnings | 3.80 |
| Generated geometry correctness, off/on | Passed for all five kernels | 26.54 |
| Selected unchanged-library caller census | Passed, no unresolved input TIR | 45.96 |

The correctness wall time includes generation and compilation; actual checks took about
0.034 seconds per variant. Generated off/on C# compiled without errors/warnings. The four
direct literal kernels contained exact-allocation guards only when enabled. The opaque
helper control retained its grow-list path in both variants.

The actual-source census found **unchanged-library applicability with existing inlining**:

| Unchanged caller | `--loops --exact-output` | Adding `--inline` |
| --- | --- | --- |
| `TriangleArray3D.Points` | Refused: callback is a function call | Accepted: raw array, width three |
| `QuadArray3D.Points` | Refused: callback is a function call | Accepted: raw array, width four |
| `QuadMesh3D.ToTriangleMesh` | Refused: callback is a function call | Still refused: callback remains a function call |

Every inspected input had zero unresolved TIR nodes. Canonical intrinsic provenance was
retained and emitted source confirms the exact-array guards in accepted cases. These are
structural qualifications of the unchanged callers, not execution of a complete unchanged
geometry library. Executable checks use the explicitly classified extracted/representative
kernels above. No matcher widening or source-library edits were needed.

An initial `--no-build` check correctly refused inconsistent bytes: the runner's text copy
normalized CRLF, although copied/source text was equivalent. The copy now preserves bytes;
only the private harness was rebuilt before the census. Kernel sources and the passing
generated correctness inputs were unchanged, so that check was not repeated.

## Reserved allocation and timing run

One bounded run held the supervisor's sole timing reservation. It used .NET 8.0.30, X64,
cpu64 wrapper scalars, and disabled tiered compilation. It measured two positive kernels
and the opaque-helper refusal control at 2,048 and 16,384 source faces. Each variant had
eight warmup calls followed by three rounds alternating which variant ran first. Each
round used `max(4, 65536 / sourceCount)` invocations. Inputs and delegates were built
outside measurements; returned length and final element were consumed into a checksum.
No runtime construction or handwritten replacement of the kernel occurred in the measured
region: calls invoked the compiled Plato-generated methods.

| Kernel; source faces | Median bytes/call off / on | Allocation reduction | Median ms/call off / on |
| --- | ---: | ---: | ---: |
| Triangle corners; 2,048 | 590,048 / 344,088 | 41.68% | 0.942 / 0.526 |
| Triangle corners; 16,384 | 4,718,896 / 2,752,536 | 41.67% | 3.421 / 0.924 |
| A-C quad split; 2,048 | 344,264 / 245,784 | 28.61% | 0.208 / 0.074 |
| A-C quad split; 16,384 | 2,752,792 / 1,966,104 | 28.58% | 3.364 / 0.396 |
| Opaque helper; 2,048 | 590,048 / 590,049 | None | 0.439 / 0.435 |
| Opaque helper; 16,384 | 4,718,902 / 4,718,904 | None | 3.811 / 3.902 |

Allocation savings were consistent across sizes and rounds while callback arrays remained.
The unchanged control allocated effectively identically; tiny counter variations are shown
rather than rounded into a false gain. These results support the destination-allocation
mechanism for eligible geometry records.

**Latency remains unqualified.** Raw times varied substantially despite the reservation.
For 16,384 triangles the optimized path ranged from 0.580 to 5.807 ms, against 1.325 to
3.819 ms for baseline; one round reversed the apparent benefit. Even the unchanged helper
control ranged from 1.255 to 4.320 ms in its nominally enabled variant. The small sample
and allocation/GC variability do not justify treating median ratios as stable speedups.
No additional timing run was made under the user's light-check constraint.

The phase completed in 11.429 seconds including setup and repeated correctness checks.
Setup was recorded separately: baseline/optimized emission took 2.131/1.115 seconds,
Roslyn compilation 3.726/0.219 seconds, and correctness 0.039/0.062 seconds. All setup
preceded warmup. Both generated compilations and correctness passed again as part of this
phase; manifest and loaded-assembly checks also passed. `timings.json` and `timing.log`
preserve every sample; `setup-timing.json` records setup independently.

Recommendation: keep the pass opt-in and prioritize these fixed-corner workloads when
selecting application trials. Existing `--inline` already exposes the actual triangle/quad
corner callers, so no new matcher or source-library rewrite is justified by this study.
Full-application impact, unchanged-library executable benchmarking, gpu32 behavior and a
reserved-List competitor remain unmeasured. The evidence establishes allocation benefit
for the classified generated kernels, not a whole-mesher speedup.

Rerun commands (timing requires the serialized reservation):

```powershell
python tools/experiments/optimizer-geometry/exact-output/run.py correctness
python tools/experiments/optimizer-geometry/exact-output/run.py census --no-build
python tools/experiments/optimizer-geometry/exact-output/run.py timing --no-build
```
