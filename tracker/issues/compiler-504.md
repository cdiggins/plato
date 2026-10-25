---
id: compiler-504
title: Experiment with statement inlining and lambda elimination
type: idea
status: in-progress
priority: p2
effort: L
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [compiler-483, compiler-485, compiler-486, compiler-487, compiler-499, compiler-500]
---

## Proposal

User requests a substantially broader C# experiment after compiler-499: inline statement
bodies, eliminate known lambdas, and continue expanding when simplification exposes
more opportunities. Larger expression-body budgets alone are not the requested test.
This item coordinates the experiment; reuse compiler-485/486 scope and specialization
work and compiler-487 callback machinery rather than introducing competing passes.
It is a proposal, not an instruction to execute the earlier design plan.

1. Introduce ordered, typed expression prerequisites. Bind receiver and actuals once
   in source evaluation order, clone local identities, preserve conversions and insert
   statements at the original branch/loop/lambda location. Begin with local declarations
   and a final return, then extend to conditionals, early returns and loops using an
   explicit inline-exit/result region. Do not use an invoked lambda as a block-expression
   substitute; that would reintroduce the overhead under investigation.
2. Track known callable values through immutable locals and parameters; substitute known
   lambda bodies at invocation sites, including statement bodies. Preserve immutable
   captures as explicit values. When a remaining callback consumer cannot inline, share
   specialized variants using the existing proposed clone registry. Unknown/escaping
   callables and unproved mutable-capture cases remain explicit residuals.
3. Alternate inlining, known-lambda elimination, conservative constant/copy propagation,
   branch simplification, fusion and supported component rewrites until stable or bounded.
   Re-enter callees exposed after simplification; use expansion ancestry rather than a
   blanket ban on any callee seen in an earlier pass. Lower loops in preserved scopes,
   then simplify newly exposed callback bodies. Count net growth after simplification.
4. Bound cumulative caller/module growth and compilation work. Prefer candidates that
   expose callable targets or constant arguments; compare generous bounded policies.
   General recursion remains a call; constant-decreasing specializations may expand only
   with proven progress and the same limits. Keep precise type/dispatch guards.
5. Benchmark the pinned BRep inputs with no inline, existing inline, statement inline,
   statement plus lambda elimination, and iterative simplification. Keep fusion/loops
   common; ablate each added stage. Validate generated C# and identical mesh diagnostics.
   Report unique eligible/refused sites, actual emitted lambdas/delegate calls and closure
   allocations in meshing kernels, whole-mesh latency/allocations, code size and compile
   cost. Attribute allocation changes using profiles; source counts alone do not explain
   them. Include a forward-component-adapted arm only if compiler-503 is implemented.

## Done means

- [x] Agree the experiment scope and shared prerequisites with compiler-483/485/486/487.
- [x] Statement-body calls, including selected branching/looping kernels, inline with preserved evaluation and conversions in executable C# tests.
- [ ] Known lambdas disappear through locals/parameters and inside nested callback scopes; remaining callables have explicit reasons.
- [x] Iterative simplification exposes deeper expansion with bounded growth and correct recursion handling.
- [ ] Ablation measurements on equivalent BRep meshes distinguish enabling effects, allocations and runtime performance; C# only.

- [x] Replace the inline-policy-specific loop restriction with shared ordered prerequisites, scoped placement and lexical cloning.
- [x] Simplify iteration control and support callback/condition scopes without sacrificing loop opportunities; verify semantic and optimization regressions.

## Implementation scope and recovery

The user's request to tackle this issue authorizes this experiment, not the older combined
plan. The shared prerequisite is ordered, explicitly typed bindings in the existing TirInliner
and C# emitter. compiler-485 owns broader preserved scopes; compiler-486's exact signature,
eager actual and growth constraints apply here. compiler-487's shared callback clone registry
does not yet exist: no competing registry or storage change is introduced. compiler-483's
hazards require executable witnesses before admitting the affected shapes. The experiment
remains opt-in, C# only, with the existing inline policy retained as an ablation baseline.

First milestone adds CSharpWriter.InlineStatements and direct expansion of straight-line
statement callees at whole initializers and returns. All actuals are bound in order, even
unused ones; local definitions are cloned and the return boundary has an explicit type.
An ancestry set and per-caller node/depth budgets bound expansion. Conditions and nested
expressions remain opaque, and lambdas, unresolved types and non-straight-line callees are
refused. There is no CLI flag or benchmark claim yet.

Verification: recorded Release test-project build passed; 17 focused generated-C# and rewrite
tests passed, including single evaluation, unused-actual faults, sibling fault order, fresh
local names and return conversion. Greenhouse evidence and optimizer guide review refreshed.
Root PROGRESS.md belongs to another mission; this section is the recovery record.
Second milestone adds ordered call-argument prerequisites, guarded conditional prerequisites,
and explicit inline result/exit regions. Branching and looping callees ending in a return
can expand; early returns jump to the local region exit. Tests assert disappearance of the
selected calls and preserve zero-trip loops, early exits, sibling faults and recursive calls.
The recorded build and 17 focused tests pass. Loop conditions and other compound expression
forms remain opaque; regions are not yet traversed by loop lowering.
Next: known callables, scoped loop lowering, bounded iterative simplification, and the pinned
BRep ablation/profile matrix. No mesh or allocation claim has been made.

Third milestone implements opt-in EliminateKnownLambdas: known lambdas pass through typed
parameters and local aliases, then expand through the same result regions. Original inferred
locals keep var declarations; cloned references use the binding type where known. Callable
creation ancestry permits nested uses of the same consumer without permitting ordinary
recursion. Unused known delegate bindings disappear after their uses expand. Unknown targets,
mutable captures, signature mismatches, expansion limits and already-rewritten callback bodies
have report categories. Tests execute three policies and require removal of invocations and
lambdas from statement-bodied and nested-capture fixtures; mutable captures remain explicit.
Recorded build and 18 focused tests pass. Shared callback variants and production eligibility
are not yet established; keep the callable acceptance box open until that coverage is assessed.

Fourth milestone adds SimplifyInlineStatements: repeated expansion, known-callable elimination,
conservative Integer/Boolean simplification, fusion and component rewriting. Regions preserve
callee ancestry between passes; callable expansion uses creation ancestry. Caller and module
budgets use net growth after simplification, with separate pass and visit limits. Over-budget
callers retain their original bodies. Four executable policy arms and a tight-budget check pass;
19 focused tests pass in the latest run. Next: expose policies for reproducible production
generation, assess remaining callback coverage, lower loops in preserved regions, then measure.

Scoped-loop milestone enters inline regions, branches and loop bodies only for direct intrinsic
calls with leaf arguments or safe lambdas. It does not broaden the legacy expression hoister.
Known callbacks are exposed at MapRange/Reduce/FlatMap sites; statement-bodied MapRange callbacks
emit an iteration-local result/exit region. Mutable captures and nested escaping callbacks are
refused. Generated loop suffixes avoid existing bindings. The recorded build and 39 focused
runtime/loop/filter/rewrite/emission tests pass. This stricter scoped route can leave calls that
the legacy top-level hoister accepted, so the production comparison must report that difference.
An isolated six-policy production build matrix is running under .temp/statement-brep against
the earlier 3d279705 compiler checkpoint; rerun treatment variants on this later milestone
before using them for performance claims. No production results have been accepted yet.

The final compiler snapshot is d0a6773f; ceda0337 only tightens malformed CLI argument
validation and does not affect its valid experimental recipes. The final build/measurement
workspace is .temp/statement-brep-final. The driver freezes all host dependencies and rejects
changed or failed variants. Allocation profiles use a separate host with phase events,
the same library DLLs, and a TraceEvent reader; only second-run meshing phases are attributed.
The first ODIN baseline/current profiles have complete stacks, no lost events, and equivalent
mesh diagnostics. Full profiling and timing remain in progress.

The repository gate recorder passed all 368 PlatoTests and both ratchets; a separate direct
assembly run also passed and saved .temp/compiler-504-tests/compiler-504-full.trx. The Studio
battery passed both lints, its SDK build and geometry tests. Its external frozen-artifact gate
failed in the already tracked repo-473 category. No checksum baseline was changed.


## Pipeline redesign requested 2026-10-25

The user stopped benchmark completion and asked for a simpler, flexible inlining pipeline with
proper scope handling. The earlier allocation profiles describe the superseded implementation;
they are not evidence for the revised pipeline. No timing matrix or closure is accepted yet.

Milestone 19df624e extracts TirEvaluation (statements followed by a value), TirStatementRewriter
(scope placement), and TirInlineCloner (fresh identities and return boundaries). Both inlining
and loop lowering use the ordered prerequisites. The loop traversal no longer switches to a
restricted direct-intrinsic recognizer when InlineStatements is enabled. Computed arguments,
guarded conditional expressions and statement-bodied consumers remain optimizable. Nested
lambda returns stay local to their lambda.

Recorded private Release build passed; 58 focused cases passed in 33 seconds, including
computed ranges, statement reductions, nested returns, sibling faults, loop emission and the
combined consumer-fusion/exact-output options. Greenhouse freshness and six maintenance tests
passed. Evidence: .temp/compiler-504-tests/pipeline-focused.trx and
.temp/compiler-504-pipeline/. The shared-output build failed MSB3021/MSB3027 because another
testhost held Plato.CSharpWriter.dll; verification now uses private outputs. An initial cloner
refactor failed two assertions by using unresolved declaration types instead of checked local
initializer types; corrected before commit. A temporary private harness reference to legacy
Ara3D.Geometry caused CS0121 (ambiguous Enumerable.Any/ArrayExtensions.Any); it was removed to
match the real test project's compile exclusion before verification.

Next: callback producer stages, condition prerequisites, and iteration/budget separation.
Then broader gates and any renewed measurements. Root PROGRESS.md belongs to another mission;
local recovery notes are .temp/compiler-504-pipeline/PROGRESS.md.

The second redesign milestone introduces TirGuardedLoop: condition prerequisites execute inside
each loop test, including the final false test. Producer stages and consumers share callback
result emission; statement callbacks retain fusion. Each callback use receives fresh local and
parameter identities before nested loops lower within its own scope. Known callback exposure
uses the argument's proven callable value instead of a list of intrinsic names. Statement
inlining can preserve lambdas without enabling elimination, and already rewritten callable
regions can be cloned. TirLoweredLoop and TirTempRef now have separate source files.

Recorded private build passed; all 58 focused cases passed in 28 seconds. New witnesses cover
condition call counts, statement producer fusion, repeated known callbacks, nested ranges, and
empty outer loops whose callback would fault. Greenhouse evidence/review/check passed. Iteration
and work-budget separation remains before the final broad verification.


The third redesign milestone separates StatementPipeline (pass scheduling and rollback) from
InlineExpansionBudget (growth and work limits) and StatementExpansion (call transformation).
Progress uses explicit transformation versions and structural identity, not printed tree comparison.
TirRewrite now uses each node's Children/WithChildren contract, preserving unchanged identity and
covering lowered markers without another visitor switch. Component expansion uses that traversal.
Ordered operands include constructors, arrays and unknown delegate invocations. Backward liveness
removes dead callable alias chains in one pass; cloned aliases retain their inferred checked types.
The large embedded test program/runtime are separate fixture files monitored by documentation evidence.

Component expansion no longer rejects statement regions or an entire function merely because a
lambda survives. The downstream capture rewriter already traverses marker children structurally;
an executable returned-closure witness verifies captured values after expansion. Calls without
optional enclosing-function metadata still permit independently typed literal-array rewrites.
Unchanged expression bodies retain expression emission. No eligibility guards were weakened to
satisfy a source-shape assertion: the initial nine component test failures exposed unnecessary block
wrapping, which was fixed before final validation.

Final verification of this worktree on 2026-10-25:
- Recorded Release CLI build: PASS, 18s; strict shipping-tier lint: PASS, 16s.
- Recorded complete non-diagnostic PlatoTests selection: 396 passed, 0 failed, 140s,
  including both ratchets. Logs/TRX: .temp/gate-logs/20261025-011637-890386/.
- Final focused statement/component/rewrite group: 30 passed, 7s; private recorded build: 5.1s.
- Studio battery: 61s. Legacy lint PASS 8.5s, forward lint PASS 24.7s, SDK build PASS 11.6s,
  GeometryTests 15/15 PASS 15.5s. External frozen-V1 checksum gate FAIL 0.5s, the existing
  cross-checkout repo-473 blocker; no manifest refresh. Log: .temp/compiler-504-pipeline/studio-gates.log.
- Greenhouse refresh, prose review, source freshness and six maintenance tests: PASS.

These are newly run gates, not inherited results. The gate recorder labels its row with the parent
HEAD cd6dac87; its tests included this milestone's uncommitted source, recorded with this work commit.
The shared status-report snapshot/HTML contain other sessions' edits and are not swept into this commit.

The requested pipeline redesign is verified. The original experiment stays open: fresh equivalent
BRep generation, production callable eligibility/residual assessment, allocation profiles and timing
ablations have not been run on this redesigned pipeline. Prior profiles cannot establish its performance.
