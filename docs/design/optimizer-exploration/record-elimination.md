# Temporary record elimination

Status: exploration under [compiler-507](../../../tracker/issues/compiler-507.md), governed by
[contract O1](wave.md). This proposes a bounded compiler transformation; the executable experiment
is handwritten C# plus an admission model, not a production pass. Measurements belong in the
[dated report](../../reports/optimizer-record-elimination-2026-10-25.md).

## Opportunity and existing behavior

After inlining, a function can construct a record and immediately read one field. Replacing
that record with scalar bindings could expose constant propagation, smaller copies and later
array simplification. It does not inherently remove heap allocation: ordinary generated record
values are structs, and the JIT may already eliminate their materialization. Arrays stored in
discarded fields must still be evaluated unless a separate proof establishes safe elimination.

[Component expansion](../../optimizer/component-expansion.md) removes fixed-array plumbing by
constructing records from scalar operations. This proposal consumes those results; it does not
repeat component expansion. The distinction matters when measuring the incremental benefit.

[TirNew](../../../src/Plato.Compiler/Checking/Tir.cs) carries typed constructor arguments;
[TirConstructorCall and TirComponentAccess](../../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs)
are writer markers for construction and field reads. The access marker stores a field name and
optional cast, not a resolved field identity. A new pass must obtain an authoritative field layout
and constructor mapping; string equality alone cannot establish eligibility. Ordinary field
reads can also arrive as resolved calls. Initial implementation should choose one proven shape.

## Released prototype contract and proposed production slice

The isolated experiment compares `new Pair(A(), B()).First` with ordered typed bindings for
`A()` and `B()` followed by the first binding. It retains every original evaluation and conversion.
Dropping `B()` because its result is unused is unsound when it faults or does not terminate.

The proposed first production slice accepts only a direct return-slot projection of a known
generated data constructor, with resolved field identity, exact argument/field types and known
declaration order. Emit every argument exactly once, in order, into a binding carrying its
original constructor-parameter type. Preserve the selected field's result conversion. Refuse
opaque constructors, unresolved or mismatched types, enclosing record coercions, captures,
reassignments, escaping records and nested expression slots requiring prerequisite motion.
Executable conversion expressions may remain inside their original typed arguments; unknown
implicit conversion requirements reject admission rather than becoming inferred `var` bindings.

Do not unwrap `TirCoerce` merely to expose a constructor. Do not rewrite a method called `First`
as if it were a generated field getter. Do not distribute construction over conditionals or
duplicate field evaluation when there are multiple reads. Those are separate slices.

## Critical review

The main objection is lack of incremental benefit: a simple temporary struct can disappear in
the JIT already. The prototype is useful precisely because it includes that baseline. Successful
compiler elimination should be pursued for a demonstrated surviving cost or an enabling effect
on later passes, not fewer C# characters. Large records crossing helper boundaries, repeated
projections and records containing array pipelines are candidates to investigate, not established
wins. A no-inline helper benchmark alone would overstate normal compiler benefit.

Eager bindings preserve faults but retain costly unused arrays. Claiming their allocation removal
requires totality and scheduling proof beyond scalar replacement. New binding names and scopes
must cooperate with statement inlining and capture rewriting. Shared compiler scheduling has
both dump and non-dump paths, so integration must cover both. There is no recommendation to add
a public flag before applicability and benefit are established.

## Execution order and gates

1. Execute the isolated admission and C# sequencing experiment. Compare warmed ordinary struct
   construction against ordered scalar bindings with setup excluded from measurements.
2. Census actual generated TIR after inlining and component expansion on representative geometry.
   Capture reachable constructor/projection shapes, authoritative field metadata and remaining
   generated machine-code cost. Stop or deprioritize if the JIT already removes the work.
3. With explicit shared-source ownership, implement the direct-return slice using existing typed
   bindings. Reuse a binder only after proving it preserves ordered prerequisites and conversion
   context. A raw expression substitution is insufficient.
4. Compile actual generated Plato with optimizers disabled and explicit combinations of inline,
   component expansion and the candidate pass. Exercise unused-field faults, conversion faults,
   selected/unselected branches, wrappers, tuple conversions and all refusal cases. Assert the
   emitted shape only in addition to behavioral checks.
5. Measure allocation and time against identical existing flags and inputs; inspect machine code
   where claimed benefits are only struct copies. Extend to local records only after proving
   use/escape/capture behavior. Keep this step separate from array dead-code elimination.
6. Refresh Greenhouse evidence and affected optimizer guides, run stewardship checks and stable
   integrated gates before production publication. The supervisor owns those shared resources.

Rerun `python tools/experiments/optimizer-exploration/record-elimination/eligibility.py` and
`python tools/experiments/optimizer-exploration/record-elimination/run.py` from the repository root.
The latter uses the repository build-error recorder with private evidence paths; it retains
categorized errors without overwriting another session's status report.
