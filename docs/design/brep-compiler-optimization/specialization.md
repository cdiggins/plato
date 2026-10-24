# Constant specialization and statement-aware inlining for spline evaluation

Reviewed proposal for compiler-481. The [combined plan](../brep-compiler-optimization-plan.md) governs implementation order and the released first slice. This document states design and proof obligations; measurements belong in validation logs.

## Incorporated critical review

CR-05/08 makes exact resolved identity, full type arguments and original result context mandatory. Evaluate nontrivial actuals, including unused actuals and conversions, once in original order. Clone binding identities and names; use an explicitly typed result binding or retain the original conversion boundary rather than assuming a new var local preserves it. Existing substitution is not proof of eager equivalence. Exclude ambiguous plato-374 dispatch, unknown operations and unproved arithmetic. Enforce a cumulative growth budget and test the selected number profile.

Measure baseline recipe, existing compiler with --inline, and the new feature with --inline separately. RationalPartial's skipped derivative branches are not new savings. CR-10 places callback specialization after the later shared clone registry; straight-line inlining alone does not supply it. CSE and invariant motion require their own totality, dominance and guarded-execution proof. Compiler-486 records this future work; only the combined plan's F1 contract is released now.

## Verified starting point

- `writers/Plato.CSharpWriter/Optimization/TirInliner.cs:138-160` retrieves a ground callee for the receiver type, refuses recursion and rejects statement bodies. Its current budgets are `MaxBodyNodes` and `MaxPasses`. Its receiver/signature type checks, tuple return handling, capture checks and alpha-renaming are substantive correctness constraints, not optional heuristics to discard.
- The current inliner substitutes arguments without binding them first. Its documentation at lines 23-29 explicitly permits evaluating an argument fewer times when its sole use moves under a branch. New work should preserve eager argument evaluation conservatively instead of extending that relaxation.
- `CSharpWriter.RunOptimizerPasses` at `writers/Plato.CSharpWriter/CSharpWriter.cs:187` runs inline, fusion/literal unrolling, component unrolling, array materialization, then loop lowering. It has no general constant-propagation, common-subexpression or invariant-motion pass.
- `src/Plato.Compiler/Checking/Tir.cs:86,110,329,340` supplies variable definitions, lets, blocks and returns, but ordinary expressions cannot contain a statement block. The C# body writer emits statement blocks and block-bodied lambdas (`TirCSharpBodyWriter.cs:190-250,493-510`). General statement-aware inlining therefore requires placement logic, not just deleting the refusal.
- In sibling `plato-brep/library/01-geometry/bspline-evaluation.library.plato:268-296`, `RationalPartial` contains sequential local definitions followed by a conditional result; `BSplinePointBody` calls it with `(0,0)`. Surface tangents use `(1,0)` and `(0,1)` in `geometry-surfaces.library.plato:111,134`; second derivatives use `(2,0)`, `(1,1)` and `(0,2)` in `geometry-jets.library.plato:69`.
- `HomogeneousPartial` at lines 250-261 passes those orders into both dimensions' `LocalDerivative`; `LocalDerivative` at lines 150-158 has an order-zero base case and a recursive order-decrement path. `RationalPartial` is the statement barrier above that potentially useful specialization chain.
- Cached `plato-brep/generated/Ara3D.StepBrep/Generated/BsplineEvaluation.g.cs:246-258` retains the general method and constant-argument call. Its header says it was generated with `--loops --fuse`, **without `--inline`**. It already uses `AggressiveInlining` on these C# methods. An inline-based compiler change will not affect this recipe unless the consumer opts into it.

The crucial qualification: `RationalPartial(0,0)` already skips the expensive `hu`, `hv` and `hab` homogeneous calls through runtime conditionals. Specialization does not newly remove those allocations. It can remove branches and unused scalar work, and expose order constants to deeper passes. Claimed allocation wins must identify actual formerly executed calls eliminated, rather than count removed unreachable C# text.

## Recommended mechanism

Start with local inlining of straight-line statement callees, plus narrow constant propagation. Retain ordinary function signatures and the existing `--inline` control. This avoids a second method registry before demonstrating benefit. Do not add CSE, invariant motion, recursion expansion and global cloning to the same first implementation.

Represent an expression rewrite as a value plus prerequisites which must execute at that expression's original evaluation point. Initially admit only a whole return expression or whole local initializer whose callee contains lets followed by one final return; reject early returns, loops, assignments, statement conditionals, unresolved bodies and nested lambdas. For a caller expression-body, convert the outer function to a block. A direct call result can become the final return; a result used by an enclosing conversion/member call requires an explicit result local. Do not hoist through `?:`, Boolean short-circuit evaluation, a loop condition or a lambda boundary. These placements remain refused until the scoped-lowering plan supplies a shared region mechanism.

At a supported call site:

1. Resolve the precise ground callee and prove every argument/result type and conversion agrees with the emitted target. Preserve full generic identity, not a bare type-name match.
2. Create fresh locals and cloned definition identities for the callee's locals. Also remap nested bound definitions if a later phase admits lambdas. Distinct symbol objects are required even if the rendered names are unique.
3. Evaluate the receiver and every nontrivial actual argument exactly once in original argument order, at the call's position, before callee statements. Preserve evaluation of unused actuals. Include coercions in those evaluated expressions; a `TirCoerce` can represent a real user conversion and cannot be stripped merely because the inner node is literal. Cheap existing locals and proved transparent literal conversions can be substituted directly.
4. Substitute parameters with those locals or literal values and emit cloned local definitions in order. Rewrite only the callee's final return into the caller's result; do not emit a callee return that exits the caller early.
5. Propagate known `Integer`/`Boolean` literals through local definitions. Fold only positively identified intrinsic comparisons and safe integer operations with profile-correct semantics. Match intrinsic identity and signature, not a function name such as `Add` or `Equals`. Initially decline overflow, division, shifts and custom coercions rather than invent arithmetic semantics.
6. Select a branch only after its condition is proved constant. Remove an unused let only when its initializer is proved total and has no required evaluation. An unused arbitrary call, index, integer divide or checked conversion stays. For the first implementation, a deliberately small total-operation whitelist is appropriate.

Schematic original:

```text
return GetSurface().RationalPartial(GetUv(), 0, 0).ToPoint;
```

Schematic result after supported placement, integer constant propagation and conservative dead-local removal:

```text
let surface = GetSurface();
let uv = GetUv();
let h = surface.HomogeneousPartial(uv, 0, 0);
let s = h.Spatial * (1.0 / h.W);
return s.ToPoint;
```

The short form above requires proof that removing the other initializers is allowed. If the compiler cannot prove the wrapper vector/scalar operations total, retain them: do not present this exact output as acceptance for the initial slice. IEEE division by zero is not an integer divide exception, but an unknown call or conversion must still remain until classified. Evaluation order, not just evaluation count, matters for runtime failures at array and integer conversion boundaries.

For `(1,0)`, the retained computation is `h`, `s`, `hu` and `su`; deeper specialization can make `LocalDerivative(order=0)` dispatch directly to de Boor. Higher derivative orders do not justify blindly expanding recursion: specialize the nonrecursive entry first and retain general recursive calls until a separately bounded policy is reviewed.

## Cloning alternative

If local expansion materially increases generated code or repeatedly copies the same sizable body, add writer-private specialized methods later. A constant mask and values select a copy of the ground function; dynamic actual arguments remain ordinary method arguments, preserving eager call evaluation without expression lifting. The emitted helper must target the precise original declaration and cannot rely on extension overload resolution to recover it.

Cache key: original function identity within this compilation; complete resolved receiver and parameter/result types including generic arguments; constant argument positions and typed values; number profile; and the optimizer configuration when caching optimized bodies. Keep this cache per writer/generation. No process-global cache, cross-compilation symbol identity or mutable cached TIR. Restrict initial constants to small integer/Boolean literals; floating constants require bit-pattern keys (signed zero, NaN payloads and profile width), so defer them.

Cap variants per original function, total generated nodes, expansion depth and cumulative nodes per caller. An active-key set prevents exact recursive cycles; distinct keys in a decrementing recursion still consume the global budget. When any limit is reached, leave the original call intact. Deterministic discovery and naming are acceptance conditions. The existing per-callee inliner budget does not replace a cumulative budget after statement expansion.

Cloning has a cleaner arbitrary-expression call boundary but requires method inventory, declaration placement, generic naming, emission and reachability support. Local inlining has less infrastructure and exposes downstream fusion immediately, but needs scope-safe argument bindings and can duplicate large blocks. Choose by measurement after the first prototype, not by assuming one representation is universally best.

## CSE and invariant motion are later, separate work

For local common-subexpression elimination, use symbol- and type-aware keys over immutable scalar expressions inside one straight-line region. Replace later occurrences with an earlier dominating value, never hoist the first evaluation. Do not equate algebraically rearranged floats; `x+x` and `2*x`, or `x/x` and `1`, are not interchangeable over IEEE inputs. Initially exclude array allocation identity, foreign `IReadOnlyList` reads, opaque calls and mutable loop markers. Proving Plato source purity does not establish purity of every host collection supplied to generated C#.

For loop-invariant motion, start with total field projections and scalar operations whose operands are invariant. A zero-trip loop must not start evaluating a previously unexecuted index, conversion or call. For partial operations, either establish execution and dominance or leave them inside. If an expression is guarded in the loop, its guard also constrains movement. Do not attempt cross-query memoization or synthesize a persistent spline cache.

Sharing repeated `SplineSpan` and control-net setup among multiple calls in a surface jet could be valuable, but it requires more interprocedural exposure and may compete with already specialized mesher jet implementations. It is not a first-slice acceptance requirement.

## Correctness and architectural constraints

- `docs/SEMANTICS.md:30-33` describes a pure language with no observable evaluation order, while lines 55-58 document checked host index conversion failures. Existing inlining also tolerates exception-timing changes. Record an explicit conservative optimizer contract for new transformations: preserve eager actual evaluation and do not speculate partial operations. This does not silently redefine the entire language or fix historical optimizations.
- Preserve wrapper scalars and property-free emission under the accepted decisions `tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md` and `2026-08-01-property-free-emission-is-unconditional.md`. Constant propagation may understand wrapper values; it must not resurrect scalar erasure.
- `tracker/issues/plato-374.md` is ready and documents generic calls selecting different implementations depending on inlining. Treat unresolved generic/default-vs-override binding as an eligibility barrier; direct concrete spline functions can be a first scope. Do not expand this known risk to all interface-generic bodies without its regression coverage and an agreed binding rule.
- `ITirInlineHost` is shared with other writers. Keep statement placement C#-specific initially or require explicit host capability; adding nodes to its shared inliner without updating consumers is unsafe.
- Preserve floating arithmetic order and coefficient grouping. Initially fold only integer and Boolean control, not floating calculations. Test NaN, infinities, signed zero and both number profiles on any later numeric simplification.

## Ordered, independently verifiable chunks

1. **First slice:** statement inlining for a direct call occupying a whole return/local-initializer slot; straight-line lets and one return only; no lambdas, loops or control-flow lifting. Preserve actual evaluations and coercions, clone local identities, enforce cumulative growth limits. Test executable results and failures, not only emitted strings. This makes a small primitive useful without pretending it already optimizes the full spline path.
2. Add narrow typed integer/Boolean propagation, branch selection and conservative dead-let removal, with explicit refusal reporting. A synthetic rational-partial-shaped fixture should expose a constant order without changing partial-operation behavior.
3. Coordinate with the scoped-lowering track on safe placement of a statement result inside a conversion/member expression and inside branch/lambda regions. Apply the real `BSplinePointBody` shape. Demonstrate generated code and runtime equivalence; assess benefit with and without JIT inlining.
4. Only if measured benefit warrants: interprocedural variant sharing and order propagation into `HomogeneousPartial`/`LocalDerivative`. Then consider local CSE; invariant motion is a separate proof and benchmark.

Anticipated implementation paths: new `Optimization/TirStatementInliner.cs` and `Optimization/TirConstantPropagation.cs`; narrowly shared helpers in `TirRewrite.cs`; integration in `CSharpWriter.cs`; scope support shared with the scoped-lowering owner; additions to `InlineReport.cs`; new focused tests in `tests/PlatoTests/Optimization/` and compiled-behavior coverage using the existing Roslyn/law backend test infrastructure. Broad changes to `Tir.cs` or C# body emission need supervisor-owned contract coordination. Update the optimizer pipeline documentation only after behavior is implemented.

## Evidence, benchmarks and falsification

Use synthetic fixtures to cover receiver and argument evaluation once and in order; ignored invalid array accesses; repeated compound actuals; colliding local names; custom coercions; result conversions; branch-local calls; nested lambdas; recursion; generic identity; code-size limits; flag-off behavior; deterministic regeneration; and both profiles. Differential execution must run an optimized C# configuration explicitly: do not assume the default compiled law backend enables these flags.

For real evidence, the supervisor or consumer owner generates a private BREP output with its recorded baseline flags and then with the candidate plus `--inline`. Also compare existing compiler `--inline` without the new change, so savings are not wrongly attributed to this patch. Verify no degraded bodies, compile with default net8.0 language version, run spline laws and mesh digests. Measure point/first-/second-derivative samples separately, allocated bytes per call, warmed latency, generated and JIT code size, compile/startup time and whole-mesher time. Use the same dataset and worker count; do not infer whole-mesher gain from summed face-worker percentages.

Falsification: if the current JIT already specializes the constant-order path and measured machine code, allocations and latency do not improve, this is enabling infrastructure rather than a standalone performance win. If new code growth worsens warm performance or startup, stop propagation earlier or prefer shared clones. If allocation reductions only appear with storage reuse or scoped lowering, report the combined improvement and ablate each feature. No speedup estimate is established by this investigation.

## Findings for the supervisor

- Existing tracked binding hazard: plato-374, generic default/override selection changes with inlining. Reuse that issue; do not file a duplicate or silently repair it in this work.
- Stale source comments still describe the retired `TirScalarLowerer` (`TirInliner.cs:263-269`) and scalar erasure (`ITirInlineHost.cs:25`). The documentation agent can assess whether its assigned scope includes these comments; no edits were made here.
- Existing source evaluation-count relaxation is documented, not newly reproduced as a bug. The review should decide the proposed stricter new-pass contract before implementation.
- Standard `git status` failed reading the parakeet submodule metadata under the sandbox; `git status --short --ignore-submodules` worked. The observed working tree contained another mission's progress/tracker and status-report changes, all left untouched.
