# BREP compiler optimization plan

This is a reviewed design, not a claim that the roadmap is implemented. [compiler-481](../../tracker/issues/compiler-481.md) tracks the parallel proposals and independent review. Only **F1**, the conditional-singleton append slice under [compiler-482](../../tracker/issues/compiler-482.md), is released for initial implementation. Other work remains explicitly deferred in its own tracker item. Measurements belong in validation logs and dated reports, not this design.

The [optimizer reference](../csharp-optimizer-pipeline.md) describes implemented behavior. The [independent critical review](../reports/brep-compiler-optimization-review-2026-10-24.md) supplies findings CR-01 through CR-12 and the source-inspected hypotheses H-01 through H-05. The proposals below incorporate that feedback; their earlier broad examples must be read within their explicit reviewed boundaries.

## Proposals and implementation order

| Opportunity | Mechanism and expected benefit | Principal challenge and disposition |
| --- | --- | --- |
| [Private scratch storage](brep-compiler-optimization/scratch-storage.md) | Convert exact self tail calls to loops, expose the recurrence, then reuse proven private alternating buffers. This targets repeated spline evaluation allocations. | Tail conversion alone removes no arrays. Ownership, captures, logical bounds and overflow need proof. [compiler-484](../../tracker/issues/compiler-484.md) is deferred until scoped lowering and its own acceptance contract are ready. |
| [Scoped combinator lowering](brep-compiler-optimization/scoped-lowering.md) | Lower within existing statement scopes, then direct tail-return branches, then callback-local prerequisites. This exposes nested spline kernels and can remove callback overhead. | Scope alone does not preserve sibling evaluation order. Restrict newly entered expressions or introduce ordered prerequisites. [compiler-485](../../tracker/issues/compiler-485.md) is the next enabling candidate after F1. |
| [Constant specialization](brep-compiler-optimization/specialization.md) | Straight-line statement inlining and narrow Integer/Boolean propagation expose fixed derivative orders; shared clones, CSE and invariant motion are later stages. | Expensive derivative branches already skip work, and the JIT already attempts inlining. Preserve eager actuals, call identity, conversions and growth limits. [compiler-486](../../tracker/issues/compiler-486.md) requires comparison with existing --inline. |
| [Fusion and filter lowering](brep-compiler-optimization/fusion-filter.md) | F1 emits Add directly for a recognized singleton branch in an already lowered FlatMap. It removes that branch's temporary array without changing producer scheduling. | Exact typing and an actual BREP-shaped preflight are mandatory. [compiler-482](../../tracker/issues/compiler-482.md) covers F1; broader local fusion and empty-call reasoning remain [compiler-488](../../tracker/issues/compiler-488.md). |
| [Concrete storage and callbacks](brep-compiler-optimization/concrete-storage.md) | Preserve logical interface values but read known backing arrays directly in generated loops. Later specialize surviving known callbacks through shared clone machinery. | Interface upcasts allocate nothing; the JIT may already produce the same code. First establish reachable shapes. Callback specialization needs the later clone registry and capture contract. [compiler-487](../../tracker/issues/compiler-487.md) remains an experiment. |

F1 is first because it has a direct allocation mechanism and no prerequisite for moving computations across scopes. Scoped lowering comes next, with strict sequencing eligibility. Tail conversion and storage reuse then address the largest long-term allocation opportunity. Specialization can expose more useful shapes but shares scope/identity infrastructure. Concrete storage and callbacks proceed only after a census/profile establishes remaining opportunity. The shared loop emitter, lowerer, capture handling and optimizer pipeline are not independent implementation tracks; assign one owner at a time.

## Common correctness contract

- Preserve source immutability, wrapper scalars, method-only emission, public array interfaces and the net8 default C# language target. No new intrinsic or spline-name special case is needed.
- Preserve guards, eager actual evaluation, defined domain faults, logical array bounds and floating arithmetic grouping. Do not infer totality or nontermination freedom from source purity. Resource-exhaustion timing is separately excluded only where an allocation/stack optimization explicitly requires it; this is not a general revision of language semantics.
- Use resolved symbol identity and complete types for new semantic operation recognition. Null or ambiguous provenance refuses a future rewrite. F1 adds no call recognition and does not repair existing matcher weaknesses.
- Treat unknown coercions as executable until proved otherwise. A C# target type can supply a conversion even when the TIR node prints no explicit cast.
- Keep expensive computed values shared when the original program shares them. Physical backing-array knowledge does not imply ownership or permission to mutate.
- Compile and execute generated output with explicit optimizer flags. String assertions establish shape, not semantic equivalence. No mesher speedup is claimed without a representative end-to-end measurement.

## F1 implementation contract, revision 1

**Observable behavior:** in an already selected lowered FlatMap, a raw conditional callback with a provably exact-typed raw singleton array arm can append its element directly. The predicate executes once, only the selected arm executes, order and duplicates remain, and unmatched branches retain evaluation followed by copying. Support either conditional orientation. Keep the existing result List, IReadOnlyList boundary and capacity policy.

**Eligibility:** require a raw expression-bodied TirLambda and raw TirConditional; exact one-dimensional array and element type agreement among callback return, conditional result, singleton arm and authoritative lowered result; and an opposite branch with the same original result context. Preserve element conversions. Refuse wrapped callback/result shapes, generic-unknown or mismatched types, and any case whose fallback conversion cannot be preserved. Bind fallback results explicitly to the original IReadOnlyList element type. Do not strip a coercion to make a pattern match.

**Excluded:** Take/MapRange empty-call recognition, recursive conditional expansion, multi-element literal expansion, local-binding fusion, new call matching, loop-scope expansion, tail conversion, buffer reuse, capacity changes, named Filter vocabulary and sibling source adoption. The sibling TKT-507 is not completed by this compiler slice.

**Preflight:** generate a minimized version of the current projection candidate filter using actual primitive/runtime declarations and --loops --fuse, without --inline. Confirm raw typed eligibility and compiled baseline behavior. If the current representation blocks matching, report the barrier before changing the contract. The reviewer permits returning to planning, not silently broadening coercion handling.

**Acceptance:**

1. Compile and execute generated C# with actual wrappers and C# 12 for net8. Cover loops off, loops on/fuse off and loops on/fuse on; preserve outputs for all-kept, all-rejected, mixed, empty, duplicate and reversed-arm cases.
2. Verify predicate/element/fallback evaluation counts and selected faults, including a selected [Fault()].Take(0) fallback. Use materialized sources for scheduling assertions to isolate this change from existing producer fusion.
3. Cover immutable captures, nested Boolean predicates, wrapper/record elements, conversion-bearing elements and conservative refusal of wrapped/mismatched/unknown result types. Unsupported callbacks and multi-element arrays keep their path.
4. Assert emitted singleton allocation removal only at recognized arms and continued presence of the fallback. Run existing loop/fusion and relevant conditional/lambda emission regressions.
5. Measure warmed allocation against an equivalent ordinary path with the same source materialization and lowering flags. Warm invocation delegates and keep reflection/setup/assertions outside the measured region. Consume results and compare values. Do not attribute unrelated loop/fusion savings to F1.
6. Verify stable inputs and run applicable integrated gates. Preserve and report unrelated baseline failures. A full regenerated BREP corpus/law/digest run requires separate consumer ownership; until run, whole-BREP behavior and speed remain unverified.

**Ownership:** implementation owns `writers/Plato.CSharpWriter/TirCSharpBodyWriter.Loops.cs`, an optional narrowly focused partial helper, `tests/PlatoTests/Optimization/FlatMapFilterTests.cs` and agreed private evidence paths. Supervisor owns tracker, this plan, optimizer documentation, shared validation and publication. No lowerer/fusion/capture edits are delegated by F1. Obtain a commit turn before staging; commit explicit owned paths only.

## Critical review dispositions

| Finding | Incorporated decision |
| --- | --- |
| CR-01, conversion and target typing | Exact structural type eligibility, original conversion context and explicitly typed fallback are mandatory in F1; unsupported shapes refuse. |
| CR-02, workload applicability | Current BREP-shaped generation is a preflight gate; a matching failure returns to review rather than widening the matcher. |
| CR-03, scoped sibling ordering | Restricted S0 direct slots replace unrestricted recursive use of the hoister. Broader scope work requires ordered prerequisites and one binder contract. |
| CR-04, local fusion eagerness/termination | Deferred to compiler-488 until a defined input domain and totality/scheduling proof exist. Single-use analysis is insufficient. |
| CR-05, operation identity | Future semantic rewrites require precise provenance. F1 is structural only; inherited matcher hazards are separately investigated. |
| CR-06, tail control/capture | Exact self-return trees, no executable result conversion, iteration-local snapshots, parallel state assignment, and both pipeline paths are required before tail work. |
| CR-07, storage safety | Explicit ownership refusal, logical bounds, overflow handling and alternating buffers precede in-place/stack/pool designs. |
| CR-08, specialization context | Eager actuals, cloned bindings, resolved result conversion, generic exclusions and cumulative budgets are mandatory; compare against existing --inline. |
| CR-09, storage fixture mismatch | Replace the out-of-scope multi-consumed-local example with a census-confirmed surviving direct source, such as Zip inputs; alias propagation remains a later reviewed slice. |
| CR-10, callable dependency | Known noncapturing callable specialization waits for the shared clone registry. Assignable captures and struct functors remain deferred. |
| CR-11, verification quality | Explicit-flags compiled execution and isolated warmed allocation evidence are acceptance gates. Existing-build baseline tests are not new-feature validation. |
| CR-12, shared implementation conflicts | Parallel planning is complete; one fenced implementation agent starts F1. Future writers need revisioned contracts and stable-input validation. |

[compiler-483](../../tracker/issues/compiler-483.md) records H-01 through H-05 as unexecuted investigations: sibling/Boolean hoisting, eager fusion behavior, name/coercion recognition, assigned-local captures, and generated-name collisions. These may affect correctness and constrain broader optimization. They are not represented as reproduced bugs or silently folded into F1. Existing [plato-374](../../tracker/issues/plato-374.md), generic dispatch inconsistency, and [plato-386](../../tracker/issues/plato-386.md), mutable array exposure, remain separate.

## Verification and handoff

Use focused compiled optimizer tests during implementation. Retain command logs and allocation protocol under the implementation issue's private `.temp` directory, and summarize reproducible evidence in its tracker record. Report builds with the repository's recording workflow when shared report ownership permits; otherwise retain independent test/build logs and explicitly report the conflict. Do not overwrite another session's status files or root PROGRESS.md. Repository-wide gates remain supervisor-owned and run on stable inputs; their failures must be attributed rather than waived silently.

Compiler-only microbenchmarks establish removed overhead at the selected kernel. Full BREP validation requires pinned generation, law comparison, mesh digests and corpus timings under identical inputs. Cross-query projection-grid caching is a mesher lifetime/design concern and is outside this compiler roadmap's initial implementation.
