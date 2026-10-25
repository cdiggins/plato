# Split loops on an invariant Boolean

Exploration under [compiler-510](../../../tracker/issues/compiler-510.md), following
[contract O1](wave.md). This document describes a candidate transformation, not a shipped pass.
Measurements and the completed experiment are recorded in the
[dated review](../../reports/optimizer-loop-unswitch-2026-10-25.md).

## Opportunity and existing machinery

For a range callback such as `i => mode ? F(i) : G(i)`, where `mode` is an immutable
Boolean argument, select a loop once and execute only its chosen body. This removes a
repeated branch and may expose simpler arithmetic to the backend. It preserves the order
of evaluations inside the selected arm and does not reassociate reductions.

The existing [loop lowerer](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs)
produces `TirLoweredLoop`, a marker consumed by the
[loop emitter](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs). Its `MapRange`
case binds the normalized count and result allocation before entering a loop, then emits
the lambda body inside each iteration. An expression-bodied `TirConditional` can therefore
remain inside that loop. This is the preferred first integration point: preserve setup and
result binding while choosing between two versions of the fill loop.

[Inlining simplification](../../../writers/Plato.CSharpWriter/Optimization/TirInliner.Simplification.cs)
already selects constant Boolean arms. Runtime Boolean parameters are the new opportunity;
literal conditions should continue to use the existing simplifier. The
[scope walker](../../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.Scopes.cs)
uses resolved variable identities for assignment checks and deliberately keeps original
loop conditions opaque. General `TirLoop` unswitching would need a separate proof for
changing bounds, loop conditions, exits and repeated guard evaluation.

## Initial acceptance contract

Accept only an expression-bodied direct conditional in a lowered `MapRange`, without
producer stages. Its condition must be a resolved Boolean parameter or a definitely
initialized immutable outer Boolean local. Track identity by resolved definition, never
by spelling. Reject any condition whose binding is assigned anywhere in the function,
including through a captured binding. Reject lambda parameters and iteration-local values.

Do not treat purity as totality. Calls, field access, array reads, conversions, nested
conditions and statements before the branch remain out of scope. A user-defined conversion
must not be stripped to make a condition appear harmless. Keep the original count evaluation,
negative-count normalization, output allocation, conversion and result binding in their
original order. Read the condition only after setup and only for a positive normalized
count. Preserve every arm's computations, faults and termination behavior. Branch selection
must not precompute either arm.

Use a cumulative per-function emitted-node budget, counting actual cloned scaffolding and
callback bodies rather than nested-choice depth alone. Keep one traversal and one result
allocation. Report a refusal reason when binding proof, supported shape or budget is missing.
The prototype's small local budget is a test mechanism, not a proposed production ceiling.

## Critical assessment

A stable Boolean branch is often predictable. The .NET JIT may already move, fold or cheaply
predict it; exposing more C# control flow can cost code size without a runtime benefit.
Splitting is most plausible when it enables a later optimization, but that is a hypothesis,
not evidence. Nested unswitching can multiply emitted code and JIT compilation effort.

General condition hoisting is much harder than invariant-value recognition. A pure but
faulting expression still cannot be evaluated on an empty loop. A nonempty guard alone does
not justify moving it ahead of source indexing or a producer that can fault or diverge.
The initial Boolean-leaf rule avoids these changes because an already initialized wrapper
value is read without executable computation; that assumption must be verified against the
actual emitted Boolean conversion before implementation.

The executable model accepts externally supplied facts about initialized outer Boolean
bindings and assigned variables. It does not derive these facts from Plato, model wrappers,
or inspect hidden writes inside Python actions. Those are trusted model preconditions,
not compiler-analysis results. Its differential checks test the proposed scheduling rule.

## Ordered execution plan

1. Execute the isolated model and adversarial checks in
   [the experiment](../../../tools/experiments/optimizer-exploration/loop-unswitch/test_model.py).
   This bounded first step is implemented. Check empty ranges, both arms, binding identity,
   assignment refusal and fault order before requesting live compiler ownership.
2. Capture actual lowered TIR for a small Plato runtime-flag fixture under explicit flags.
   Establish how often direct invariant conditions survive existing simplification in useful
   geometry kernels. Reject the opportunity if source inspection finds no useful survivors.
3. Add an analyzer over resolved bindings with explicit refusal diagnostics and tests for
   shadowing, local initialization, assignments, captures and executable coercions. Keep it
   separate from emission. Refresh optimizer documentation evidence for production changes.
4. Under assigned emitter ownership, split only eligible MapRange fill loops after existing
   setup. Test compiled output against the interpreter and unsplit C# with explicit flags,
   empty/negative ranges, faulting arms and wrapper Boolean values. Assert generated structure
   and preserved single allocation without relying on text shape alone.
5. Compare warmed paired compiled fixtures with consumed outputs, randomized case order,
   allocation counts, emitted size and JIT disassembly. Cover tiny and large ranges and both
   mode values, with tiering/runtime/source fingerprints recorded. Keep the option experimental
   unless it improves a representative kernel beyond the JIT baseline without disproportionate
   size or compilation cost. Only then consider additional combinators or conditions.

No timings or speedup estimates are stored here because those belong to measured reports.
