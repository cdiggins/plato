# Array facts experiment — 2026-10-25

Work item: compiler-508, exploration of array length and index range analysis.
Contract: [O1](../design/optimizer-exploration/wave.md).
Design and execution plan: [array facts](../design/optimizer-exploration/array-facts.md).

## Result

The first bounded step is implemented and verified as an isolated Python arithmetic model.
It computes conservative intervals, refuses arithmetic overflow, preserves checked collection
conversion order, and certifies simple loop accesses only within logical array bounds.
It is not integrated with Plato TIR and does not demonstrate a compiler speedup.

Run from the repository root:

```powershell
python -m unittest discover -s tools/experiments/optimizer-exploration/array-facts -p 'test_*.py' -v
```

| Check | Actual result | Seconds |
| --- | --- | --- |
| Python unit suite | 7 tests passed | 0.150 test runner; 0.585 process and log capture |
| Exhaustive count intervals | 153 intervals over -8 through 8 matched concrete normalized lengths | Included above |
| Exhaustive loop/index domain | 14,175 length/start/stop/offset combinations matched concrete safety | Included above |
| Exhaustive signed addition domain | 18,496 pairs of four-bit intervals either matched concrete bounds or refused overflow | Included above |
| C# compilation, generated execution, integration gates | Not run; no production compiler/runtime files changed | — |

Logs and SHA-256 input fingerprints are retained in the ignored working directory
`.temp/optimizer-exploration/array-facts/`. The durable evidence is the reproducible suite and
this report; the ignored logs are not required to rerun it. There was no benchmark run.

## Counterexamples and critical assessment

- cpu64 `Integer.MapRange` performs a checked `int` conversion before the runtime handles
  nonpositive counts. A count below `int.MinValue` faults. Clamping that original count to zero
  first would silently suppress the fault; the test demonstrates the distinction.
- Adding one to a profile's maximum integer cannot use an ordinary mathematical interval.
  The model refuses both overflowing results and operands outside the selected domain.
- For logical length four, indices `i + 1` for `0 <= i < 4` are unsafe; stopping at three is safe.
  For logical length three, index three stays invalid even if storage capacity is larger.
- Empty iteration produces no proof for an access outside the loop. Unknown materialization
  or unstable ownership also refuses, even when supplied numerical bounds appear safe.
- A modeled eager callback faults at index one before a producer completes. Predicting its
  eventual successful length does not authorize replacing the producer with that length.

The eager callback and overflow checks execute a source-grounded model, not the C# runtime.
The prototype receives provenance booleans from its caller; it does not establish ownership,
dominance, effect safety or binding identity. A production adapter must prove those inputs.
The exhaustive checks cover bounded domains and do not constitute a formal proof for all
machine integers. Explicit boundary tests extend coverage to both profile widths.

The likely performance opportunity is narrower than a general bounds-check pass: current
loop emission already caches counts and emits simple induction variables. The JIT may already
remove those checks. Before transformation work, census actual emitted kernels and reuse the
producer/scope facts proposed by compiler-487, the existing concrete-storage investigation.

## Handoff

Ready for supervisor review: arithmetic prototype, tests, design, report and private checkpoint.
No shared compiler files, tracker files, status snapshots or Git index were changed by this track.
Root `git status` could not inspect parakeet metadata; `git status --ignore-submodules` succeeded
and showed existing unrelated edits. The track did not repair or overwrite them.

The next production step is a report-only TIR adapter under new shared-source ownership. Later
transformation, compiled differential tests, JIT evidence and full integration remain unstarted.
The supervisor owns tracker updates and the commit; production success must not be inferred
from completion of this exploration packet.
