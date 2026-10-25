# Grid reuse geometry probe — 2026-10-25

This advances compiler-511 under G3 with a focused actual-TIR probe. The private harness
built and executed successfully against the frozen snapshot; generated application bodies
were emitted but not compiled or executed. No production change or runtime benefit is claimed.

The previous [secondary census](optimizer-secondary-tir-2026-10-25.md) observed only the
outer loop lowered for a rectangular sampling fixture. That result cannot establish the
behavior of the redesigned traversal. The new probe records raw, optimized and captured
trees, nested loop counts and emitted bodies under explicit loops-on, inline-off/on
recipes against the supervisor's frozen compiler snapshot.

The probe includes newly authored rectangular and polynomial sampling fixtures and reads
unchanged `MarchingCubesLattice` and `MarchingCubesCornerValue` from frozen geometry sources.
The rectangular kernel is representative, not a verbatim marching-cubes implementation.
The unchanged lattice enumerates flattened cells using integer division and remainder;
even nested-loop support does not supply its overflow, bounds or affine schedule proof.

`TirIntrinsicIdentity`, the existing helper for exact intrinsic ownership and resolved
definition identity, is reused. The rectangular fixture checks that every raw callback
invocation references the same parameter definition. This establishes binding identity
only: arbitrary host callbacks still lack immutable-capture and totality evidence.
Emitted local spelling is never treated as identity.

The report-only totality checker accepts exact primitive Number literals, unassigned
parameter references and exactly typed resolved intrinsic Add/Multiply applications.
It uses the same narrow operation boundary as existing consumer fusion, without a new
source-name registry. A polynomial kernel provides a possible nontrivial arithmetic body;
integer-to-Number conversions, schedule bounds and cross-cell argument equality remain
separate unproven obligations. No expensive useful *safe reachable* sampling candidate
has yet been established. Unknown user functions and delegate environments refuse.

Reproduce during a supervisor-granted runtime window:

```powershell
python tools/experiments/optimizer-geometry/grid-reuse/probe.py --execute
```

The runner validates the frozen manifest and every copied source/binary hash, builds only
a private reference-only harness, records its full build log and private categorized
status, and verifies input stability afterward. It cannot silently use working-tree
compiler binaries or stdlib sources. Without `--execute` it does no work.

Next decisions depend on actual probe results. If a nested schedule survives, prove its
iteration domain and callback argument equivalence before proposing caching. If costly
samples remain opaque, prioritize the existing plato-432 library resampling issue rather
than adding a compiler pass with no justified useful candidate. Call-count reductions
from the earlier model are not evidence of runtime improvement here. Generated runtime
equivalence, allocation tests and timings remain unstarted.

## Focused run results

The single granted run used snapshot revision `b612c392b8998ad719f70027eba1ac7c09d91779`
with manifest SHA-256 `a40d562cff09b2750e8f3a367638b235a911f5a36bca01ee2f2ffff26ac88b75`.
The private harness build passed with no warnings or errors in 4.00 seconds as reported
by MSBuild. Parsing, selected ground-TIR checks, emission and before/after frozen-input
hash validation passed. No harness repair or second run was needed.

| Selected function | Observed in both inline recipes | Consequence |
| --- | --- | --- |
| Rectangular | Two nested MapRange loop markers; four callback invokes with the same raw parameter definition | The old nested-lowering barrier is gone. Opaque capture environment and totality still refuse caching. |
| Procedural | Two nested MapRange loop markers; four named polynomial calls remain in emitted C# | A candidate schedule is visible, but the call summaries, coordinate conversions, bounds and equivalence proof are not implemented. |
| Polynomial | Exact Number Add/Multiply body passes the narrow totality checker | A nontrivial arithmetic body is provably total within this limited check; it does not prove the complete sampling call sites safe or expensive enough to optimize. |
| MarchingCubesCornerValue | One callback invoke remains | Arbitrary caller-supplied sampling still has unknown environment and termination behavior. |
| MarchingCubesLattice | Sequential MapRange and FlatMap loop markers, not nested rectangular loops | Flattened indexing and the opaque MarchingCubesCell call still require schedule reconstruction and interprocedural analysis. |

The emitted procedural body retains Integer-to-Number conversions and separate capture
aliases for `gain`; aliases must be traced by resolved bindings if a future pass shares
calls. No equality inference from generated variable names is valid. The probe inspects
both inline settings, but does not assert they inline this particular helper: the observed
calls survive in both.

The recommendation is to retain compiler caching as an investigation. Nested lowering
now removes one concrete structural obstacle, and the polynomial helper supplies a small
proof boundary, but this run establishes no costly useful candidate with all obligations
discharged. Do not add a production switch or broad memoization based on these shapes.
Private evidence is in `.temp/optimizer-geometry/grid-reuse/`: `results.txt`, emitted
`*.cs.txt` bodies, `inputs.json`, `build.log` and `build-status.json`.
