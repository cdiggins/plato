# Plato semantics and optimization audit — 2026-10-25

Work: [compiler-483](../../tracker/issues/compiler-483.md), the existing sequencing/identity
investigation, and [compiler-521](../../tracker/issues/compiler-521.md), the checked-pipeline plan.
Three independent read-only tracks reviewed language contracts, expression transformations,
and array/loop transformations. This report records source inspection, not new execution evidence.
No compiler builds, benchmark runs or executable reproductions were performed for this audit.
Earlier geometry measurements remain evidence for their stated recipes, not the whole pipeline.

## Main conclusion

Plato's purity helps optimization, but it does not make every expression total, every callback
unobservable, every coercion transparent, or every array access harmless. The newer exact-output
and multiple-consumer passes deliberately have narrow contracts. Several older passes rely on
broader substitution and fusion assumptions that must be characterized before making a whole-pipeline
safety claim. A checked new pass cannot repair an invalid input transformation performed earlier.

## Semantic authority and boundaries

| Property | Evidence and consequence |
| --- | --- |
| Pure values versus partial computations | [SEMANTICS §6](../SEMANTICS.md#6-expressions-construction-conversions-operators) says Boolean short-circuit preserves skipped faults and nontermination. Purity does not authorize deleting unused or conditionally used actual arguments. The introductory claim that order/exceptions were wholly unobservable was too broad and is clarified in this change. |
| Evaluation order | The semantic reference does not state a universal left-to-right rule for all calls. Do not invent one. The runtime expressly specifies eager ordered MapRange, while the existing ordered-evaluation helpers preserve backend sequencing. Rule premises must identify their authority rather than conflate these contracts. |
| Eager arrays | [ArrayIntrinsics](../../src/Plato.Intrinsics/ArrayIntrinsics.cs) computes MapRange elements once, in order, before returning. Streaming a producer into a consumer changes that schedule; legality needs an argument about faults, termination and observations. The interpreter guide's obsolete claim that C# construction is lazy is corrected. |
| Floating-point profiles | [SEMANTICS §1](../SEMANTICS.md#1-the-model) binds Number/Integer to cpu64 or gpu32. Number's abstract-real description is not permission for reassociation, `x * 0 = 0`, or changing rounding boundaries. NaN, infinity, signed zero and checked index narrowing matter. |
| Nominal calls and conversions | Operators are overloaded calls; type-named conversion functions may execute arbitrary bodies. Match resolved definitions and instantiated types. A name such as Map, Add or Multiply, or the presence of a TirCoerce wrapper, does not establish the intended operation. |
| Affine builders | [The primitive builder contract](../../stdlib/foundation/primitives.plato) distinguishes observe/mutate/consume. Freeze invalidates the builder. Static affine enforcement is incomplete: ordinary type correctness cannot prove unique ownership, safe capture or safe duplication. |
| Host collections | Immutable language arrays are represented through C# IReadOnlyList. That interface alone proves neither stable Count nor effect-free indexers. Distinguish owned completed arrays from arbitrary supplied collections; document the supported host contract. |
| Sealed source types | [SEMANTICS §2](../SEMANTICS.md#2-type--products-and-sums) restricts source construction, but generated host constructors remain public. Do not accept a proof-like value from arbitrary host code as evidence that its predicate was checked. |
| Allocation | Existing new-pass contracts exclude resource exhaustion and host inspection of concrete result collection type. Allocation optimization cannot promise identical out-of-memory behavior. These exclusions do not license skipping ordinary program faults or divergent computations. |

The current expression/loop audit did not find a reduction reassociation: literal reduction builds
a left fold, and statement simplification limits its constant arithmetic folding to bounded
Integer operations/comparisons. Preserve that restraint rather than introducing floating-point
algebra under the abstract-real description.

Short-circuit normalization is language behavior, not an optional optimization. Its stage matters:
the reference distinguishes a direct Boolean And/Or from an ordinary generic call that only
becomes Boolean during monomorphization. A later optimizer must not casually reinterpret calls
from their final names/types and silently change their argument-evaluation contract.

## Existing algorithms

The table identifies source-confirmed mechanics. Unless explicitly described as an existing
test, examples below are candidate reproductions whose full source-to-runtime reachability is
not established by this audit. They extend the existing compiler-483 hypotheses, not a claim that
every example is a newly confirmed production defect.

| Algorithm | Current behavior and semantic assessment | Required next check |
| --- | --- | --- |
| Statement inlining | `TirEvaluation`, `TirStatementRewriter` and the statement inliner use ordered bindings and preserve evaluation regions. Existing statement tests cover unused actuals/fault order. | Retain these mechanisms and check every later pass too: `CSharpWriter.RunOptimizerPasses` still runs the older expression inliner afterward. |
| Expression inlining and beta reduction | `TirInliner.cs` admits compound arguments used at most once, including zero uses; `TirRewrite.TryBetaReduce` similarly checks duplication/capture rather than unconditional once-only evaluation. A single occurrence under a branch is not eager execution. | Characterize an unused faulting actual and a conditionally used actual; bind before substitution or prove safe omission/motion. Do not treat cheapness as a semantic property. |
| Producer fusion | `TirFusion` recognizes several operations by name and composes eager stages without a general totality argument. Its broad comment claiming no evaluation-count changes is not established by the guards. | Check source completion before downstream work, shortened Take, ignored callback values, first-fault order, callee identity and executable conversions. |
| Literal expansion | `TirLiteralUnroller` turns literal-array maps/folds into substituted expressions; All/Any becomes a short-circuit chain. That can omit construction of later array elements or interleave construction with consumers. | Retain eager ordered literal construction before traversal unless those element computations are safe to omit/reorder. Check unused fold inputs and partial elements. |
| Component expansion | `TirComponentUnroller.Hof.Apply` substitutes arguments; literal-array Reverse reverses element expressions; component All/Any forms conditional chains. Field expansion is not by itself a proof that receiver/operand evaluation survives. | Check multiplicity, ignored operands, receiver evaluation and conversion preservation separately from fixed-size shape recognition. |
| Array materialization | The live MapRange is already eager. The existing guide documents that renamed eager helper calls can remain unsupported by the runtime. | Do not use this pass as the authority for ownership or eagerness facts. Treat retirement/redesign separately; textual rename tests are not executable validation. |
| Loop lowering | `TirLoopLowerer` can stream Map/MapRange stages without a totality check. `TirCSharpBodyWriter.Loops` caches Count and emits early breaks for All/Any. | Establish when skipping source traversal is harmless; characterize external Count/indexer behavior and earlier producer faults. Name recognition/coercion stripping also needs resolved-identity checks. |
| Multiple-consumer fusion | Canonical owned Number arrays, completed producer, literal seeds, and independent total Number Add/Multiply callbacks permit interleaving while retaining each accumulator's expression/order. | Keep the narrow contract. This does not justify streaming the producer, admitting arbitrary callbacks, or reassociating the reduction. |
| Exact-output allocation | Canonical FlatMap, no fused stages, fixed literal width and concrete types; actual-array/capacity guards retain the ordinary fallback. Callback evaluation remains ordered and complete even at zero output width. | Preserve this checked contract and test its renderer. Its current equivalence evidence is relative to existing loop lowering, not a proof that loop lowering matches every host-list behavior. |

### Minimized cases to characterize next

These are semantic sketches, not asserted compilable Plato fixtures. Use a valid partial operation
such as an out-of-range access for `Fault`, then establish actual resolved TIR before execution.

1. **Unused/conditional actual:** `Ignore(Fault())` or a callee that uses its actual only in an
   unselected branch. Direct substitution can remove evaluation even without duplication.
2. **Eager producer and early consumer:** `MapRange(2, i => i == 0 ? 1 : Fault()).Any(x => true)`.
   An eager producer faults before Any begins; a streamed early-exit loop may avoid it.
3. **Literal array traversal:** `[1, Fault()].Any(x => true)` constructs its source before the
   reference traversal. A short-circuit expression over element expressions can skip construction.
4. **Two partial stages:** a producer faults at element 1 while its consumer faults at element 0.
   Streaming can change which fault is first even if it eventually evaluates the same values.
5. **External collection:** Count succeeds once and faults on its next read, or the second indexer
   access faults after Any already decided its answer. The runtime Reduce/FlatMap reads Count per
   iteration; emitted loops cache it. This is a host-contract question, not evidence that immutable
   Plato arrays can mutate spontaneously.
6. **False identity:** a user-defined same-name operation or an executable conversion around a
   recognized shape. Establish reachability before calling a name-based match an executable defect.

`ExactOutputTests.SourceMutationAndNonArrayFallbackPreserveReads` explicitly expects a single
Count read. That is valuable exact-output-versus-lowered-loop evidence, but cannot validate
lowered-loop-versus-original-runtime Count semantics. Keep both comparison levels distinct.

## Consequences for the six proposals

| Proposal | Required semantic facts |
| --- | --- |
| Exact output | Stable source length under its guard; unchanged source/count conversions; complete ordered callbacks; unchanged fallback; explicit resource-exhaustion exclusion. Width zero does not imply no callback work. |
| Multiple consumers | Completed owned input plus independent total callbacks; retain each reduction's floating-point operation tree. Reusing the source is different from fusing its construction. |
| Array facts | Producer/result binding, completion, normalized checked count, scope and alias validity. Length is not proof of stable elements, bounds safety or removable materialization. Invalidate builder facts on mutation/consume. |
| Record elimination | Resolved generated constructor/getter and exact field types; preserve every constructor operand in order, including unused values and conversions. Fresh names must include unused lambda parameters. |
| Unswitching | Exact immutable Boolean binding, proved identity coercion, unchanged setup and zero-trip behavior. Invariance alone does not imply safe speculation. Existing negative benefit evidence still stands. |
| Grid reuse | Same coordinate values under the selected profile and same capture environment; deterministic, terminating, nonthrowing sampling; safe schedule, conversions, bounds and lifetime. A functional type or a claimed pure callback is insufficient. |

## Changes to the execution plan

1. Make every rule cite language, intrinsic and backend premises separately. Preserve distinctions
   between immutable, owned, completed, deterministic, terminating, nonthrowing and safely reorderable.
2. Use compiler-483 to reproduce or disprove the older-pass hypotheses before widening admission
   or making whole-pipeline safety claims. Start with unused actuals, eager producer completion,
   literal construction and identity; run only the relevant minimized regression for each.
3. Keep the exact-output checked-plan architecture pilot, but label its adjacent-stage guarantee.
   Include an unoptimized runtime reference when qualifying composed recipes. The new flags staying
   off does not disable older flags or prove those algorithms safe.
4. Add affine effects, profile and coercion obligations to the checked-rule context. Avoid a universal
   purity bit, blanket coercion stripping, and callback equivalence based only on function identity.
5. Distinguish structural correctness, rule equivalence, whole-recipe behavior and measured benefit
   in evidence. None substitutes for the others. No broad test campaign is required to begin this work.

No optimization implementation or flag default changes in this audit. The introductory semantics
and eager-array documentation corrections reconcile explicit existing contracts; they do not define
a new universal evaluation order or silently settle every host compatibility question.
