# Geometry qualification of consumer fusion — 2026-10-25

The optional consumer-fusion pass correctly combines the two scalar reductions in a newly
authored point-cloud plane-residual kernel. The selected unchanged geometry callers are all
refused. A short timing run favored fusion at both sampled sizes, but its large outliers prevent
a reliable general speedup claim. This establishes correctness and a useful accepted geometry
shape; it does not justify enabling the option by default.

## Workload and unchanged callers

[PlaneMoments](../../tools/experiments/optimizer-geometry/consumer-fusion/geometry.plato) is a
**new representative kernel**, not an existing stdlib call made faster. It materializes the
signed residual `x * nx + y * ny + z * nz + offset` from three coordinate arrays, then computes
its sum and sum of squares, returning both in a generated record. A unit normal makes the
residual a signed distance. The first sum measures directional bias; the second is the
least-squares objective. The motivation is the stdlib's
[fit-quality and residual definitions](../../stdlib/geometry/geometry.library.plato), especially
`SumOfSquaredError` and `Residual(PlaneFit3D, Point3D)`. Its signed first moment is intentionally
different from that existing unsigned residual API. The compiler matcher was not widened.

The actual resolved TIR census considered these unchanged callers, with no unresolved nodes
and no accepted pair in each:

| Existing caller | Why the current pass refuses it |
| --- | --- |
| [Clothoid2D Eval](../../stdlib/geometry/curves-shapes.library.plato), the Euler-spiral quadrature | The source is Integer indices constructed through Range, and callbacks use trigonometry, conversions and local captures. |
| [BoundsOfPoints for Point2D and Point3D](../../stdlib/geometry/geometry.library.plato), axis-aligned bounds of point lists | The source is an external point array, accumulators are points, seeds call First, and callbacks use field reads and Min/Max. |
| [EdgeLengthRatio](../../stdlib/geometry/polyhedra.library.plato), longest-to-shortest mesh-edge ratio | The producer is UndirectedEdgeLengths, callbacks use Min/Max, and the shortest fold is seeded by the longest fold's result. |

This is a census of selected promising sibling consumers, not an exhaustive claim about every
possible specialization in the library.

## Executed checks

The frozen compiler snapshot records revision `b612c392b8998ad719f70027eba1ac7c09d91779`.
Its manifest binds compiler binaries and source copies. A separate `GeometryRuntime.dll`
contains the exact frozen intrinsic sources under the cpu64 profile, rather than borrowing the
whole test assembly. The harness verifies manifest hashes before and after every command,
checks copied and actually loaded assembly hashes, and rejects stale harness builds.

| Check | Result |
| --- | --- |
| Private Release harness build | Passed, no errors/warnings; build reported 4.19 seconds. |
| Actual generated off/on kernel structure | Passed: source materialization remains; separate reduction loops become one shared traversal. |
| Both result fields, bitwise | Passed across 30 input/plane combinations, including empty, on-plane, mixed signed, tiny/large, infinite and NaN coordinates. |
| Independent simple geometric oracles | Passed: on-plane moments are zero; signed distances 1, -1, 2, -2 yield sum zero and squared sum ten. |
| Producer failure and source-read order | Passed: both modes read the same prefix and fail at the same y-coordinate access before reductions. |
| Selected unchanged-caller TIR census | Passed; all four selected functions remain refused. |

Correctness/census command elapsed times were not recorded. These were focused checks only;
no broad test suite or shared project build was run. The first harness build reported CS0104,
`DiagnosticSeverity` ambiguous between the compiler and Roslyn enums; qualifying the Roslyn
name fixed that harness-only error before the passing build.

The generated kernel and result record compile at Release optimization and default C# language
version. The harness preserves actual generated Number Add/Multiply bodies but changes their
instance receiver into an extension receiver: the already compiled standalone runtime cannot
accept additional partial struct methods. A small host bridge returns both generated record
fields as a tuple, avoiding reflection inside timed calls. This is an explicit harness boundary,
not a claim that the entire foundation output compiled as a new assembly. Only cpu64 was checked.

Raw evidence is under `.temp/optimizer-geometry/consumer-fusion/`: `build.log`, `verify.log`,
`census.log`, `census.json`, `correctness.json`, generated C#, selected-caller TIR,
`build-inputs.json`, `input-hashes.json` and `loaded-assemblies-before/after.json`.
The private [runner](../../tools/experiments/optimizer-geometry/consumer-fusion/run.py) records
build diagnostics through the existing recorder with its destination redirected to this subtree.

## Performance disposition

The supervisor granted one exclusive, bounded timing run. It used 4,096 and 65,536 points,
three alternating-order rounds after warmup, consumed both fields and recorded allocations.
Tiered compilation was disabled for the child process so the measurement did not depend on
background tier promotion. The timed calls include residual-array allocation and coordinate
projection; traversal fusion is only part of that cost. Input coordinate arrays were prepared
outside timing. No repeated benchmark or broad suite was run.

| Points | Calls per round | Separate median, range (ms) | Fused median, range (ms) | Bytes allocated per call, either mode |
| --- | --- | --- | --- | --- |
| 4,096 | 64 | 4.0245; 3.6691–4.6931 | 3.5321; 2.5514–4.3523 | 32,792 |
| 65,536 | 4 | 5.3500; 4.4393–36.9097 | 3.5816; 2.9092–11.9538 | 524,312 |

The ratios of these batch medians are 1.139 and 1.494 respectively. They are observations from
this short run, not stable performance estimates. In particular, both large-input recipes had
substantial final-round outliers. The residual arrays remain allocated, and allocations were
identical across recipes and rounds. Large arrays enter the runtime's large-object allocation
regime; collection and unrelated operating-system activity were not isolated or attributed.
The small sample count, millisecond batches, receiver adaptation and newly authored workload
all limit extrapolation. The measurements support retaining the pass as an opt-in candidate
for this shape, while the unchanged library callers still gain nothing from it.

The same run repeated the focused correctness checks successfully. Raw samples, checksums and
allocation totals are in `timings.json`; `benchmark.log` records the generated shapes and result.
Frozen-input and loaded-assembly hash checks passed before and after execution. No runtime
process remained active after the run.
