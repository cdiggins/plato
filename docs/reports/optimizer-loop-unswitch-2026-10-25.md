# Loop-unswitch exploration — 2026-10-25

Issue: compiler-510, the investigation of selecting a loop once from an invariant runtime
Boolean. Contract O1. The completed step is an executable Python model, not Plato compiler
integration, C# generation or a performance benchmark.

## Evidence and result

`python -B -m unittest discover -s tools/experiments/optimizer-exploration/loop-unswitch -p test_model.py -v`
passed all 10 tests in the initial run (0.026 seconds reported by unittest). The differential
test exercises 414 size/flag combinations, including negative, empty and singleton ranges,
using deterministic seed 510. Both transformed and original executions preserve output and
observable body events. Instrumented reads change from one per iteration to one per nonempty
loop; this is structural model evidence, not measured machine-code branch savings.

The suite demonstrates why broader transformations are unsafe: an unguarded condition can
fault on an empty range; hoisting ahead of a preceding computation changes the first fault;
a mutable captured Boolean selects different later arms. It refuses calls even when their
return value looks invariant, and separately verifies original evaluation counts. It refuses
iteration-dependent/unknown bindings, assigned captures, nested bodies and exceeded budgets.
Same-spelled distinct symbols do not alias in the model. Chosen-arm faults and unchosen-arm
non-evaluation are preserved. Divergence is excluded by the condition syntax rule; no timeout
or infinite-loop experiment was run.

The source review inspected `TirLoopLowerer`, `TirLoopLowerer.Scopes`,
`TirCSharpBodyWriter.Loops`, and `TirInliner.Simplification`. These are respectively the lowered
loop marker/recognizer, the scope-aware lowering route, the C# loop emitter, and the existing
constant simplifier. The proposed first integration point is a runtime-Boolean conditional in
an expression-bodied MapRange callback. Existing constant simplification is not the proposed
new feature.

## Critical review and limitations

The model trusts its initialized-Boolean and assignment summaries. Python action callbacks
could violate their declared writes; the model is not a sound analyzer of arbitrary Python.
A production analyzer must compute those summaries from resolved TIR. It also needs wrapper
conversion and allocation/evaluation-order tests absent from this model. The current local
node budget does not establish a whole-function production code-growth policy.

No .NET build, compiled Plato fixture, JIT disassembly, allocation benchmark or timing
comparison ran. There is no demonstrated speedup. A predictable invariant branch may already
be cheap or optimized by the JIT, and duplicated code can be worse. The design therefore puts
real compiler survivors and paired compiled evidence before widening or enabling the pass.
The review is the implementing agent's critical self-review, not an independent external review.

## Artifacts and next step

`tools/experiments/optimizer-exploration/loop-unswitch/model.py` implements the bounded tree
rewrite. `test_model.py` exercises it and unsafe counterexamples. The design document at
`docs/archive/optimizer-exploration/loop-unswitch.md` records legality, source integration points
and the remaining ordered execution plan. Private `.temp/optimizer-exploration/loop-unswitch/`
contains the test log and checkpoint. Supervisor integration and production work remain pending.
