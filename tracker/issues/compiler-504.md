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
- [ ] Iterative simplification exposes deeper expansion with bounded growth and correct recursion handling.
- [ ] Ablation measurements on equivalent BRep meshes distinguish enabling effects, allocations and runtime performance; C# only.

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
