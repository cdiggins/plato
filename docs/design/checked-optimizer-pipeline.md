# Checked optimizer transformations

Tracking: [compiler-521](../../tracker/issues/compiler-521.md). Contract revision: V2.
This is an implementation proposal, not an implemented safety guarantee. The current request
authorizes planning; the first implementation milestone remains pending in the tracker.

## Objective and constraints

Make each optimization small enough to understand, independently disable, review, and change.
Separate finding an opportunity from proving its legality, constructing a replacement, and
deciding whether its benefit justifies shipping. Prefer conservative refusal to speculative rewriting.

Preserve the existing TIR, the single C# pass scheduler, wrapper scalar representation,
method-only emission, resolved call identity, and explicit conversions. This proposal does not
supersede the [wrapper scalar decision](../../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)
or [emission decision](../../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md).
The [pipeline reference](../csharp-optimizer-pipeline.md) remains authoritative for current behavior.

## Architecture: typed rules, checked plans, existing TIR

Use declarative rule specifications expressed as typed C# data and small functions. Do not start
with a parser for a new rule language, a second IR, an equality-saturation engine, or a solver
dependency. Introduce a common interface only after the first two rules reveal a shared need.

The intended flow is:

```text
original typed region + phase context
    -> match a candidate
    -> independently check the candidate's obligations
    -> construct a checked plan
    -> build replacement without mutating the original
    -> validate replacement structure and rule-specific correspondence
    -> commit replacement, invalidate facts, emit C#
```

Each rule declares its stable identifier/version, phase, enable flag, required facts, supported
types, guards, replacement recipe, preserved observations, refusal reasons, and code-growth limit.
Candidates contain actual resolved nodes and bindings, not assertions such as `IsSafe = true`.
Only the checker constructs a checked plan. The builder accepts that plan and must not add
eligibility heuristics. The final check validates what the builder actually produced; checking
a plan alone would leave builder bugs outside the gate.

Prefer narrowly typed plans such as `ExactOutputPlan` and `ConsumerFusionPlan` to a dictionary
of arbitrary claims. The checker reads the original region and derives the facts it needs;
it must not merely replay the candidate matcher's success. Common low-level primitives may be
shared, but matching and validation must not share the same unverified eligibility conclusion.

Keep these existing seams:

- `CSharpWriter.RunOptimizerPasses` owns phase order and integration. Do not create a second scheduler.
- `TirRewrite` owns structural traversal through `Children` and `WithChildren`.
- `TirEvaluation` and `TirStatementRewriter` own ordered evaluation and statement scope.
- `TirIntrinsicIdentity` owns intrinsic provenance; names alone cannot authorize a rewrite.
- `TirInlineCloner` is the starting point for binding renewal. Centralize freshness only when
  reused; include unused lambda parameters and generated marker bindings.
- `LoopOptimizationOptions` retains independent production switches.

Exact-output allocation currently chooses its path during loop emission, after the relevant
capture/lowering work. Its first refactor must construct and consume the checked plan at that
same boundary. Do not hoist it earlier in `RunOptimizerPasses`. Separate selection from rendering
locally first; later scheduling changes require their own semantic review and focused tests.
Existing lowered-loop and multiple-reduction markers remain the backend representation.

Exact-output is an important first-slice exception to the TIR replacement flow: it changes emitted
C#, not the lowered TIR. A body verifier therefore cannot certify that renderer's output. Initially
keep the small renderer explicitly trusted and use test-only structured C# syntax checks plus
execution tests against its actual guard, callback order and fallback. Mutate each of those outputs
to ensure the comparison rejects it. Do not add a production Roslyn dependency merely for this
refactor, and do not claim per-compilation validation of emitted C#. If that stronger guarantee is
required, design a bounded output validator as a separate milestone before making the claim.
This limitation does not exempt actual TIR rewrites from validating their constructed replacement.

## Facts, revisions, and refusal

Each rule must identify which premises come from source-language semantics, intrinsic contracts,
and backend host compatibility. A pure source function need not terminate or be defined for every
input. Short-circuit conditionals preserve skipped faults/nontermination; ordinary type names and
operator spellings do not authorize algebraic rewrites. Numeric profiles determine machine
arithmetic, and a user-defined conversion is executable computation, not an annotation.

Track affine builder observation, mutation and consumption separately. Static affine checking
is incomplete, so well-typed TIR alone does not prove unique ownership, safe duplication or
safe capture. A sealed source type restricts Plato construction, but its public generated host
constructor is not a trustworthy certificate of a runtime predicate by itself.

Before expanding a rule or adopting a combined optimizer recipe, characterize the current older
passes under [compiler-483](../../tracker/issues/compiler-483.md). In particular, expression
substitution, eager producer fusion, literal expansion, component expansion and loop lowering
need their own evaluation/identity arguments. Checking a new rule relative to an already altered
input cannot establish equivalence of the entire pipeline. Use an unoptimized reference as well
as adjacent-stage checks; name the exact phase and flag combinations the evidence covers.

Start with facts needed by an actual consumer: resolved type/intrinsic identity, literal output
width, immutable bindings, completed owned array producers, assigned/captured symbols, and the
existing narrow total Number arithmetic contract. Keep separate facts for nonthrowing behavior,
termination, determinism, safe duplication, and safe reordering. Language purity proves none of
these automatically for arbitrary calls or host collections.

Facts belong to a compilation, function revision, phase, and lexical region. Initially recompute
after each accepted rewrite; do not build incremental or cross-function caches. A checked plan
is single-use against that same revision. Mutable `TirLoweredLoop` fields make object identity
insufficient: prohibit mutation while plans are live and validate every semantic field the plan
depends on before application. A later immutable-node migration is justified only if this narrow
boundary proves insufficient. Source locations and pretty-printed hashes are not identity proofs.

Do not promote the experimental producer name/origin pairing into a global analysis contract.
If a production rule needs array facts, give the completed producer and its result an explicit
typed binding relationship, then validate dominance, aliasing, scope, and invalidation.

Unknown facts, unsupported nodes, checker budgets, or unsatisfied guards mean refusal and the
original path. Candidate construction is side-effect free. Invalid transformed output is an
internal diagnostic: never emit it; strict development/CI mode fails, while a supported recovery
mode may retain the untouched original region and report the refusal. Disabling a rule skips it;
there is no production option to apply a rule while bypassing its safety checker.

## What safety means

### Accepted target and migration baseline

The [mixed-evaluation decision](../../tracker/decisions/2026-10-25-mixed-evaluation-preserves-required-results.md)
accepts omission of unnecessary pure work, including faults confined to it, while retaining defined
successful results and required validation. Use that decision as the authority for the intended
contract. A rule must prove which requested observations do not depend on the omitted work; purity
alone is insufficient. This is a permitted refinement of faulting behavior, not strict equivalence
of every formerly eager execution. Fault identity, competing required faults and omission of
nontermination remain open; do not infer permissions for them.

V2 records this policy direction; it does not expand any implemented rule. The initial architecture
migrations retain existing eligibility and the conservative baseline below. A later, separately
reviewed semantic migration can loosen an individual rule under compiler-534. Until then, retain
its existing guards and fault tests. Tag evidence and caches with the applicable semantic contract
as well as the numeric profile, so incompatible qualifications cannot be silently reused.

The conservative migration baseline is conditional equivalence: every accepted transformation preserves defined
results and the observable behavior specified by the language and supported runtime contracts.
This includes arithmetic grouping and conversions, branch selection, termination behavior,
first program fault, and ordered/count-preserving calls where the caller can observe them.
Reordering is allowed only when those observations are proved unaffected. Unknown callbacks forbid changes to invocation count, order, arguments or evaluation region unless
separately justified; retaining their original execution remains permitted. External collections
retain their original fallback. Uncertain aliases prohibit transformations that depend on isolation.

Specify the supported numeric/runtime profile in every rule context and evidence record. Existing
geometry measurements primarily cover cpu64 and extracted or newly authored kernels, not complete
applications or every backend/profile. Qualify additional profiles explicitly before claiming support.
Preserve fault kind and the observable prefix before it; exception messages are preserved only where
the runtime contract specifies them, not incidental stack traces or generated method names.

Retain NaN, infinity, signed-zero and overflow behavior under the selected numeric profile;
do not infer real-number algebra from floating-point syntax. Tests compare relevant bit patterns,
not just tolerances. Resource exhaustion, timing, and observing the concrete collection type behind
the supported array interface are outside the existing allocation-optimization contract. State
these exclusions explicitly; changing allocation cannot guarantee identical out-of-memory timing.

The trusted base includes the TIR semantics, primitive/runtime contracts, checker, builder and
emitter correspondence, generated C# compiler, and runtime. A small checker reduces the area to
review; it does not eliminate bugs. Tests alone cannot guarantee equivalence for every input.
Do not call this a formally verified compiler. A stronger mathematical guarantee would require
a formal semantics and a proved checker/emitter or independently validated generated output.
That is a separate scope decision, not a prerequisite for the first maintainable improvement.

For future rules licensed to suppress faults, comparisons classify a permitted unnecessary-work
omission separately from an ordinary match. Require independently checked dependency/observation
evidence for that classification; never accept every fault-to-success difference. Add counterexamples
where the failed computation supplies requested geometry or exhaustive validation and must not be
omitted. The current strict comparison tests remain valid for unmigrated rules.

Use complementary validation layers:

1. **Structural verification:** check the actual optimized body for types, resolved calls,
   coercions, lexical scope, use-before-definition, fresh bindings, and marker invariants.
   `TirTypeVerifier.Verify` currently re-elaborates source; it does not verify arbitrary optimized
   output or semantic equivalence. Reuse its checks through a per-body entry point, with writer
   marker checks in the writer layer rather than adding a compiler-to-writer dependency.
2. **Rule-specific correspondence:** check original and replacement against the licensed recipe,
   including operands retained, evaluation regions, guards, index schedules, and fallback body.
   This is bounded translation validation for supported templates, not a universal solver.
3. **Independent generated-code checks:** compile actual Plato flag-off/on output; compare values,
   faults and callback traces with independent oracles for representative cases. Interpreter
   comparisons are useful where contracts overlap; they do not cover all host-runtime behavior.
4. **Adversarial checker tests:** alter one premise or plan field at a time and require refusal:
   fake callee identity, stale revision, moved binding, wrong width, changed conversion, omitted
   operand, reordered callback, widened guard, and unexpected lowered-node field.

Every rule also gets a short preservation argument stating why its premises imply its contract.
An independent reviewer checks that argument and its negative tests before promotion. Solver-backed
proofs may later help bounded integer/index identities; they must model overflow and domain guards
exactly, treat unknown/timeouts as refusal, and never stand in for floating-point or callback proofs.

## Optimization-specific obligations and priority

| Optimization | Required proof and replacement contract | Next action and promotion gate |
| --- | --- | --- |
| Exact-output allocation | Canonical FlatMap; literal fixed output width and concrete types; evaluate source/count in the existing order; retain actual-array and representable-capacity guards; evaluate every callback fully in order; identical fallback. Zero width still evaluates callbacks. | First architecture slice. Preserve current eligibility/output and `--exact-output`; use the existing allocation evidence, rerunning only if behavior or code shape changes. |
| Multiple-consumer fusion | Adjacent independent reductions over a completed owned array; exact primitive identities; total immutable callback inputs; preserve each accumulator's grouping and seed semantics. Shared iteration changes interleaving, so totality/independence are essential. | Second migration, with `--fuse-consumers` unchanged. Dependable geometry benefit remains open; no widening on noisy timings. |
| Array facts | Exact producer/result binding, normalized checked length, dominance, scope and escape/assignment invalidation. A count fact alone licenses neither allocation removal nor bounds-check removal. | Extract only facts consumed by a migrated rule. Analysis-only has no output switch; each consumer remains independently gated. |
| Temporary-record elimination | Resolved generated constructor/getter identity and field types; eager ordered evaluation of every operand, including unused ones; preserve conversions, faults and binding freshness. | Keep the private typed projection experiment until a surviving cost or concrete downstream benefit is established. Then use a separate default-off rule. |
| Loop unswitching | Exact invariant Boolean binding and explicitly transparent coercion; retain setup/zero-trip guards; no new predicate evaluation on paths that formerly skipped it; preserve branch bodies and growth budget. | Park production implementation: the existing experiment found no consistent benefit. Resume only with a materially different candidate and bounded measurement. |
| Grid sample reuse | Resolved callback/environment identity, deterministic terminating nonthrowing sampling, safe conversion domain, equivalent sample arguments, alias isolation, bounds and valid dependence schedule. | Last and highest-risk. Complete a candidate-specific proof before runtime caching; refuse arbitrary callbacks. No generic memoization pass. |

The existing [geometry decisions](optimizer-exploration/wave.md#geometry-promotion-decisions)
link the measured evidence. Completed exploration items remain closed; compiler-521 owns the
new architecture. Existing open benefit/proof obligations remain with compiler-506, compiler-507
and compiler-511. New implementation slices get tracker items when scheduled, not duplicate
reopenings of completed experiments.

## Parallel execution, with explicit dependencies

Use the shared checkout on the current branch, with the supervisor plus at most three active
subagents. The supervisor owns shared contracts, scheduler/CLI/options integration, tracker,
documentation evidence, build reservations, commit turns and publication. Do not have multiple
agents edit `CSharpWriter.cs`, compiler node definitions or the runtime concurrently.

### Wave A: agree on the contract, then deliver one vertical slice

Freeze V2 signatures, observation contract and ownership before dispatch. First assign separate
agents to exact-output extraction, optimized-body verification, and independent adversarial
review. The supervisor integrates the smallest checked-plan boundary. Shared interface files
are supervisor-owned; agents must not invent incompatible local copies.

| Track | Exclusive proposed write fence | Dependency and completion evidence |
| --- | --- | --- |
| Exact-output | Existing `TirExactOutput.cs`, `TirCSharpBodyWriter.ExactOutput.cs`; new `Optimization/Checked/ExactOutput/`; `ExactOutputTests.cs`, `ExactOutputFixtures/` | V2 agreed. Current phase, acceptance/refusal and generated behavior preserved; forged/stale plans rejected; existing focused tests pass. |
| Body verifier | `src/Plato.Compiler/Checking/TirTypeVerifier.cs`; new `Optimization/Checked/Validation/`; `TirTypeVerifierTests.cs`; new `tests/PlatoTests/Optimization/CheckedValidation/` | V2 agreed. Check exact-output input/marker fields and relevant forms only; reject malformed evidence. This does not validate rendered C#. Extend coverage with fusion rather than requiring every writer marker first. |
| Independent reviewer | Read-only production sources; new `tests/PlatoTests/Optimization/CheckedAdversarial/`; private review checkpoint | Write fixture inputs while builders work; execute only against frozen integrated artifacts. Review proof, attack checker assumptions, and report correlated matcher/checker risks. |

Writer paths above are relative to `writers/Plato.CSharpWriter/`; named existing test files are
under `tests/PlatoTests/Optimization/`, except `TirTypeVerifierTests.cs` under `Checking/`.
Each track owns a distinct `tools/experiments/checked-optimizer/<track>/CHECKPOINT.md` and
`.temp/checked-optimizer/<track>/` output directory. Supervisor owns
`Optimization/Checked/` files outside those fenced child directories. Final file names and exact
interfaces are confirmed at the wave kickoff; no subtree ownership may overlap.

### Wave B: prove reuse on fusion, without expanding the optimizer

After Wave A is reviewed and committed, assign fusion migration to one agent, required-fact
extraction to another, and independent interaction review to the third. Fusion owns existing
`TirMultiConsumerFusion.cs`, `TirMultiReduction.cs`, `TirCSharpBodyWriter.MultiConsumer.cs`, its
existing tests/fixtures and new `Checked/ConsumerFusion/`. Facts own `Checked/Facts/` and
`tests/PlatoTests/Optimization/CheckedFacts/`. Reviewer retains `CheckedAdversarial/` test ownership.
Agents prepare fixtures in parallel; fusion consumes facts only after their contract and focused
checks are stable. The supervisor alone changes shared contracts and integration wiring.

Any newly refused previously accepted case is a compatibility change: document and review it, or preserve eligibility; do not call it a behavior-preserving migration.

Accept this wave when both rules use the same small execution/checking protocol without duplicated
analyses or changed acceptance boundaries. Delete superseded eligibility logic in the same migration;
do not keep permanent old/new implementations and a migration flag. If a common abstraction needs
many rule-specific exceptions, retain simple typed functions rather than expanding the framework.

### Wave C: benefit-driven optional extensions

Use one agent for record cost/downstream evidence, one for grid legality on a concrete candidate,
and one for independent review. Both remain inside their existing experiment fences until a
production gate is met. Unswitching receives no implementation slot without new benefit evidence.
Only one bounded benchmark runs at a time. A negative result closes that investigation slice;
it does not require a dormant production flag or broad helper infrastructure.

Once an extension qualifies, freeze its rule contract and grant a new isolated rule/test subtree.
Structural validation and rule checking are mandatory for that extension; runtime guards fall
back to the original computation without adding observable evaluations. Record the new switch
and required phase explicitly. Keep rule-specific changes reversible by their own commit and flag.

## Efficient tests and operational gates

Incremental changes run the smallest affected regression/filter. Examples from the repository root:

```powershell
dotnet test tests/PlatoTests/PlatoTests.csproj --filter FullyQualifiedName~ExactOutputTests
dotnet test tests/PlatoTests/PlatoTests.csproj --filter FullyQualifiedName~MultiConsumerFusionTests
```

Use a compatible already-built artifact with `--no-build` only when its inputs are proven current.
Record builds through the repository build recorder, or its established private-output adapter
for isolated harnesses. Shared project builds are serialized. Independent checks may overlap only
with stable source inputs and truly separate outputs. A private directory alone does not isolate
a build from another agent's edits. No timing runs overlap builds or other measurements.

For a rule change, run its positive case, changed refusal/fault case, and pertinent checker-negative
case. At a migration milestone, run the affected verifier and rule classes plus the relevant
interaction matrix: both new flags off, each on, both on; include inline on/off where it changes
the admitted geometry shape. Do not exhaustively enumerate unrelated compiler flags on each edit.
Run broader required integration gates once at the mission boundary, not after every commit.
For this planning-only change, only document/link checks are required.

No performance rerun is needed for a documentation-only change or behavior-preserving extraction
with unchanged emitted code. New code shape gets one short correctness-first paired measurement
covering allocation, latency, code growth and compile-time overhead. Define the expected benefit
before measuring. Noise or regression means defer, not a larger automatic benchmark campaign.

Every accepted rewrite can report rule/version, phase, source location, checked premises, runtime
guard, fallback and refusal reason through existing diagnostics/dump seams. Reports are evidence,
not authorization. Bound checking overhead; budget exhaustion refuses the rule. Keep detailed
tracing optional, while legality checking remains mandatory whenever the rule is enabled.

Before each integration, freeze writers, confirm child processes drained, record source/artifact
fingerprints, run only affected gates, and obtain independent review. Later input changes invalidate
only affected results. Optimizer source changes follow [documentation stewardship](../../tools/optimizer-docs/MAINTENANCE.md)
including source evidence refresh and guide review. Serialize explicit-path commits on the current
branch; preserve other sessions' files and scratch notes. Push only the reviewed integrated history.

## First implementation milestone

Start with exact-output's checked plan at the existing emission boundary, the minimal per-body
input/marker verifier support it needs, and adversarial validation. Completion means unchanged flag-off output,
unchanged admitted/refused cases and runtime guards, preserved geometry results/fault behavior,
rejection of tampered or stale plans, test-only validation of the actual emitted recipe, and an
independently reviewed preservation argument. The renderer remains trusted; this milestone does
not certify generated C# equivalence on every compilation.
It does not require migrating every old pass or introducing every proposed analysis first.

If the checker is as complicated as the optimizer, stop and shrink the admitted contract. If a
new analysis has no production consumer, leave it experimental. If meaningful benefit is absent,
do not promote the rule. Architecture improves by making obligations explicit and removing duplicate
logic, not by adding a framework around unproved transformations.
