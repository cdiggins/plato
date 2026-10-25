# Grid sample reuse experiment — 2026-10-25

This is the isolated first step for compiler-511, the investigation of compiler-managed
neighboring sample reuse. It does not modify Plato or generated C#. Its legality contract
and subsequent steps are in the [design](../design/optimizer-exploration/grid-reuse.md).

## Experiment and actual results

Run from the repository root:

```powershell
python tools/experiments/optimizer-exploration/grid-reuse/prototype.py
```

The Python standard-library model compares a rectangular corner stencil evaluated by
repeated calls with a demand-filled cache retaining adjacent rows. Eight test methods
passed. The shape sweep covers widths and heights from zero through eight, comparing
exact outputs, order of first demands, call counts and peak cached entries. Separate
checks cover boundary reads, empty callbacks, refusal of every missing proof condition,
stateful callbacks, distinct captures, prefill fault ordering and floating-point coordinate
reassociation. The latter counterexamples must differ; they are not expected equivalences.

| Observation for a 64 by 64 cell grid | Measured model value |
| --- | ---: |
| Baseline sample calls | 16,384 |
| Reuse sample calls | 4,225 |
| Peak cached sample entries | 130 |
| Fully sampled grid entries for comparison | 4,225 |

These are instrumented counts, not a benchmark. Peak entries count retained sample values,
not dictionary bytes, temporary key lists, output tuples or the instrumentation trace.
Both paths produce exactly equal ordered corner tuples. No timing or allocation claim
is made for C#, the JIT, a mesher, or the Python implementation.

## Review and limitations

The callback proof object is supplied by the test, not computed by an analyzer. A falsely
asserted immutable callback changes results in the stateful counterexample. Unknown
totality refuses the rewrite; the test does not run a diverging program or establish a
termination checker. Exact lattice keys do not authorize floating-point reassociation:
the test exhibits distinct coordinates at a large origin. Eager filling visits required
nodes in a different order and changes the first thrown fault.

Source inspection found the direct practical overlap already tracked by plato-432,
the procedural marching-cubes resampling issue. Its algorithm-level solution can proceed
without waiting for this higher-risk compiler analysis. The current loop lowerer's
one-dimensional combinator representation does not establish a general grid schedule.

No production pass, generated C# validation, spatial slab model, runtime allocation
measurement or end-to-end speed test was attempted. Those remain ordered follow-up steps,
not evidence implied by these passing tests. Repository-wide gates are supervisor-owned.
The private run log is `.temp/optimizer-exploration/grid-reuse/check.log`.
