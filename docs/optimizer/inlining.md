# Inlining: expose the operation behind a helper

[Overview](README.md) · [Pipeline contract](../csharp-optimizer-pipeline.md) · [Source map](source-map.md)

## Idea and motivation

A library helper can hide the combinator a later pass understands. Replacing `Scale(xs, k)`
with its resolved body can expose `xs.Map(x => x * k)`. This is source-level inlining and
lambda beta reduction, performed while the compiler still knows Plato types. It lets later
[fusion](fusion.md) and [loop lowering](loops.md) eliminate collection machinery that a C# JIT
may not see through across generated methods and delegates.

The [GHC inliner paper](references.md#inlining-and-substitution) describes the related tradeoff
between exposing simplifications and excessive code growth. Plato uses bounded syntactic rules,
not GHC's full simplifier or a profile-guided cost model.

## Implementation and boundaries

[TirInliner](../../writers/Plato.CSharpWriter/Optimization/TirInliner.cs) obtains bodies through
[ITirInlineHost](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs). This separates
body lookup, concrete receiver context and reporting from the rewrite. It substitutes parameters,
rebinds `Self` and renames conflicting bound lambda parameters for C# emission.

The pass requires a suitable expression body and resolved types. Statement bodies, unresolved
names, eta-forwarding shapes and unsupported capture situations are refused. Expensive arguments
cannot simply be duplicated; the cheap-expression and parameter-use checks limit substitution.
Inside lambdas the rules are stricter. Body size and iteration budgets bound expansion, and
callee/receiver pairs prevent recurring specialization across passes. The constants in the
implementation are the authority for budgets.

Skipping evaluation of an unused argument or moving it into a branch is distinct from duplicating
work. The [sequencing investigation](../../tracker/issues/compiler-483.md) covers these questions;
the current guard set should not be read as proof of unrestricted eager equivalence.

Constructor recovery retains the callee's resolved return type on the new constructor
node. Later passes must preserve it: the emitter uses the receiver type to distinguish
fields from methods. Inlined conditional expressions also require parentheses when
used as array receivers or beneath a typed-index cast; otherwise C# changes their meaning.

[InlinedExpressionEmissionTests](../../tests/PlatoTests/Emission/InlinedExpressionEmissionTests.cs)
compiles and executes conditional array receivers, typed indices and constructor-field
reads with inlining disabled, enabled, and followed by loop lowering.

## Usage, tests and effectiveness

The experimental `CSharpWriter.InlineStatements` property runs the statement extension
in [TirInliner.Statements.cs](../../writers/Plato.CSharpWriter/Optimization/TirInliner.Statements.cs)
before the expression pipeline. It admits resolved statement callees ending in a return,
including branches, early returns and loops. It evaluates every
actual into an explicitly typed local in source order, clones local identities, and binds
the result with the declared return type. An inline region assigns that result and jumps to
its own exit label, so a callee return cannot exit the caller. Call arguments are sequenced
before any later argument's prerequisites; conditional branches keep their prerequisites
behind the guard. Branch and loop bodies retain their scopes. `TirGuardedLoop` holds prerequisites
for each condition evaluation without moving them outside the loop. Statement inlining can preserve
lambda bodies independently of whether callable elimination is enabled. Exact ground signature checks
exclude ambiguous generic dispatch.
The pass represents an expression's prerequisites as `TirEvaluation`: an ordered statement list
and a value. Sequencing operands completes each earlier operand before starting a later one's
prerequisites. `TirInlineCloner` owns fresh lexical identities and return redirection independently
of call eligibility. It preserves checked initializer types for inferred locals and keeps nested
lambda returns separate from the enclosing function's return.

Expansion ancestry and cumulative per-caller growth limits bound recursive expansion.
This property is an implementation experiment, not a default policy or a performance
claim. [compiler-504](../../tracker/issues/compiler-504.md) tracks broader scope and measurements.

`CSharpWriter.EliminateKnownLambdas` adds callable tracking to that experiment. Immutable
parameter bindings and local aliases retain known lambda bodies. Applications bind their
arguments eagerly and use the same result regions, including for statement bodies. Captures
must refer to unassigned values; unknown callables and unproved mutable captures stay explicit.
Nested consumers use ancestry from the callable's creation context, so repeated use of the
same consumer is not mistaken for recursion. Unused known-callable bindings are removed only
after their applications disappear. Remaining callback bodies are processed in their own
scope. The inline report distinguishes unknown targets, mutable captures, signature mismatches,
and budget refusals. Already rewritten callback regions can be cloned with fresh identities.
There is no shared specialization registry: callbacks whose consumers remain calls still
allocate delegates. No storage or ABI changes are implied by callable elimination.

`SimplifyInlineStatements` enables bounded repeated expansion, callable elimination, constant
propagation, fusion and component rewriting. It also admits expression callees through the same
eager bindings. Only unassigned bindings with identical types propagate; Integer comparisons and
in-range addition, subtraction and multiplication fold only for the designated intrinsic
library and exact scalar declarations. Floating arithmetic, overflow and arbitrary conversions
do not fold. Constant guards select only their executed branch. Inline regions retain expansion
ancestry across iterations, so recursion cannot restart on a later pass. Caller and module growth
budgets count net nodes after simplification; a caller exceeding its allowance retains its input.
A separate visit limit and pass cap bound compilation work. Budget properties on CSharpWriter
and constants in StatementExpansion are the authority for limits. These are conservative bounds,
not a profitability model, and no runtime or allocation advantage is established.

Use the C#-only `--inline-policy=statements`, `--inline-policy=lambdas`, or
`--inline-policy=iterative` to reproduce these ablations. The latter policies include the
earlier capabilities. `--inline-growth=<positive integer>` selects the per-caller allowance.
The growth option requires an explicit policy; malformed values are errors.
These options do not imply the existing `--inline` expression pass; pass that flag explicitly
when comparing the added stage against the current inliner. With no policy option, existing
recipes are unchanged.

[StatementInliningTests](../../tests/PlatoTests/Optimization/StatementInliningTests.cs)
compiles emitted C# and checks cloned names, required evaluation of unused arguments, sibling
fault order, return conversions, conditional guards, zero-trip loops, early exits and recursion.

Enable `--inline`; use `--inline-report` to see accepted totals and categorized refusals from
[InlineReport](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs). Use `--dump-tir`
to see which later transformations the exposed body enables.
The report also counts distinct source sites and their outcomes, separately from expansion
events. One source site may have several outcomes across cloned callers or iterations;
outcome totals must not be added to infer a site count. Generated nodes without source origins
use node identity. The dump records the original input before experimental expansion.

[InlinerTests](../../tests/PlatoTests/Optimization/InlinerTests.cs) checks TIR shapes for the
Transform family, including removal of delegate calls and recovery of constructors. It is not
a comprehensive recursion/refusal test suite. Run its class filter using the
[evaluation commands](evaluation.md). A removed method call is evidence that inlining fired;
it does not prove a runtime gain because the JIT might already inline that call. The larger
opportunity is making an allocation-producing combinator visible to another pass. Code growth
and compiler work are the corresponding costs.
