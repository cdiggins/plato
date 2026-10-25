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
before the expression pipeline. It admits whole local initializers and return expressions
whose resolved callees contain local declarations followed by a return. It evaluates every
actual into an explicitly typed local in source order, clones local identities, and binds
the result with the declared return type. Branch and loop bodies retain their scopes;
conditions, nested expression positions, lambda bodies and non-straight-line callees remain
outside this initial slice. Exact ground signature checks exclude ambiguous generic dispatch.
Expansion ancestry and cumulative per-caller growth limits bound recursive expansion.
This property is an implementation experiment, not a production CLI flag or a performance
claim. [compiler-504](../../tracker/issues/compiler-504.md) tracks broader scope and measurements.

[StatementInliningTests](../../tests/PlatoTests/Optimization/StatementInliningTests.cs)
compiles emitted C# and checks cloned names and required evaluation of unused arguments.

Enable `--inline`; use `--inline-report` to see accepted totals and categorized refusals from
[InlineReport](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs). Use `--dump-tir`
to see which later transformations the exposed body enables.

[InlinerTests](../../tests/PlatoTests/Optimization/InlinerTests.cs) checks TIR shapes for the
Transform family, including removal of delegate calls and recovery of constructors. It is not
a comprehensive recursion/refusal test suite. Run its class filter using the
[evaluation commands](evaluation.md). A removed method call is evidence that inlining fired;
it does not prove a runtime gain because the JIT might already inline that call. The larger
opportunity is making an allocation-producing combinator visible to another pass. Code growth
and compiler work are the corresponding costs.
