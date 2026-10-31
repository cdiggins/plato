---
date: 2026-10-30
title: Scalars are lowered to primitives inside a body, never across a boundary
status: accepted
superseded-by:
links: [2026-08-01-wrapper-scalars-are-the-only-representation.md, ../../docs/csharp-optimizer-pipeline.md, ../../writers/Plato.CSharpWriter/ScalarLowering.cs]
---

## Context

[2026-08-01](2026-08-01-wrapper-scalars-are-the-only-representation.md) made wrapper scalars the only
scalar representation: `Number`, `Integer` and `Boolean` are single-field readonly structs, and the C#
compiler checks the unit types and interface conformance built on them. That record argued that scalar
arithmetic is not where the wrapper costs anything, because the JIT flattens a single-field struct.

That holds only while the JIT inlines the wrappers' one-line methods. RyuJIT stops inlining into a method
once the locals it counts after inlining reach about 512, and `AggressiveInlining` does not lift the cap.
Loop lowering and fusion put nested producers into a single method, so a large lowered body can reach the
cap. Past it, each remaining `Number`, `Integer` or `Boolean` method call is a real call, refused as "too many
locals". The STEP mesher in plato-brep met this in its hot B-spline bodies (plato-brep ticket TKT-1129;
investigation `docs/plans/scalar-erasure-2026-10-30.md` in the plato-brep repository). The cause is the size of
one generated method, so the remedy has to act inside that method.

Whole-program erasure (`--scalar=float`) is not available as the remedy. It was retired on 2026-08-01 for
reasons that have not changed.

## Decision

The C# writer has a generation switch, `--lower-scalars`, off by default (`CSharpWriter.LowerScalars`). With it
on, the inside of every emitted body computes `Number`, `Integer` and `Boolean` values as the primitives they
wrap. Signatures, fields, interfaces, generic arguments and every call boundary keep the wrapper structs. A
value is wrapped once, as `Unsafe.BitCast<primitive, Wrapper>(...)`, where a wrapper is required: a call
argument, a return, a field, an array element, a delegate result. The wrapper is a single-field sequential
struct over the primitive, so the reinterpretation is the value the constructor would build; the
[2026-08-01](2026-08-01-wrapper-scalars-are-the-only-representation.md) record already relies on that layout for
`MemoryMarshal.Cast` at buffer boundaries. A lowered Integer literal carries an `L` suffix under a 64-bit
Integer profile, and a lowered Integer used as an indexer or array length is written `checked((int)(...))`
there, the conversion the wrapper's implicit operator to `int` performs. `Character` and `String` never
lower.

The switch also turns on a scalar-only mode of the inliner (`CSharpWriter.InlineScalarBodies`, derived from
`LowerScalars`): with `--inline` off, `TirInliner` still expands a callee whose parameters and return are all
`Number`, `Integer` or `Boolean`, and refuses any other as `NotScalarBody`. The stdlib's derived scalar helpers
(`GreaterThanOrEquals`, `LessThan`, `Abs`, `Min`, `Max`, `Square`) are Plato bodies, so their arithmetic lowers
only when they are expanded into the caller. The inliner's other rules (expression bodies, the cheap-argument
discipline, self-containment, budgets) are unchanged.

Lowering is part of emission (`TirCSharpBodyWriter`), after loop lowering, like `TirExactOutput`. It is not a
TIR pass and rewrites no tree. The table of intrinsics and their C# spellings is
`writers/Plato.CSharpWriter/ScalarLowering.cs`, and each row computes what the handwritten runtime does for the
call (`src/Plato.Intrinsics/Number.cs`, `Integer.cs`, `Boolean.cs`): `double.Equals` for `Number` equality,
unchecked integer arithmetic, a truncating `ToInteger`, non-short-circuit `&` and `|` for `And` and `Or`.

## Rationale

- **It acts where the cause is.** The cap is per method, so removing the wrapper calls from the method's text
  does not depend on the JIT's budget for that method. A call boundary resets the count, so the cost of
  keeping the wrapper there is small.
- **It keeps the checks the 2026-08-01 record protects.** The C# compiler still sees `Length`, `Area` and
  every interface-typed parameter as distinct nominal types, because only the inside of a body is changed.
  The type discipline is checked at each signature, which is where mistakes between units are made.
- **It refines the earlier record; it does not reverse it.** That record rejected an erased representation
  that is visible in signatures and in the output shape. This one adds no second output shape for signatures:
  the generated surface is identical with the switch on or off, and the difference is local to bodies.
- **Two engines can still be compared.** The compiled law backend accepts the switch, so the differential run
  can hold the lowered output against the interpreter law by law.

## Alternatives rejected

- **Reinstate `--scalar=float`.** Erased signatures lose the unit types and the interface conformance the C#
  compiler checks, which is the reason it was retired.
- **Rely on `AggressiveInlining` or a larger inlining budget.** The attribute does not lift the locals cap, and
  a runtime knob is not something a generated library can set for its consumers.
- **Wrap with the constructor or the implicit operator.** A spike on .NET 8.0.30 (TKT-1129) showed both
  become one real call per wrap once the enclosing method is past the cap, which reintroduces the calls at
  every wrap site. `Unsafe.BitCast` is expanded by the JIT's importer and compiles to no instruction.
- **Make the wrappers cheaper in the handwritten runtime.** The count that reaches the cap includes the
  temporaries of each inlined wrapper call, so a cheaper wrapper method still adds locals to the caller.
- **Lower only inside loop bodies.** Large bodies are not all loops, and the scalar let and the inlined helper
  are the same problem outside a loop. The switch covers every body, with extra handling where loops bind
  scalar parameters.
- **Make the switch the default.** It changes the generated text of every body, and consumers pin generated
  output by commit. It is opt-in until a consumer's regeneration and law runs show it holds.

## Consequences

- **Two renderings of every scalar node.** `WriteNode` writes the wrapper and `WritePrimitive` writes the
  primitive (`TirCSharpBodyWriter.Scalars.cs`). A new writer path that handles a scalar-typed node must
  choose between them. The loop, multi-consumer, control-flow and flat-map emitters call into it for loop
  parameters, accumulators and conditions.
- **The table mirrors the runtime.** A change to a runtime operator changes its row in `ScalarLowering.cs`.
  `tests/PlatoTests/Emission/ScalarLoweringTests.cs` runs both spellings over the awkward inputs to keep them
  agreeing, and
  `tests/PlatoTests/Optimization/ScalarBodyInliningTests.cs` pins the scalar-only inliner.
- **An intrinsic with no row is read through `.Value`.** Hash, Round and the others with no one-expression
  spelling stay calls on the wrapper. Adding a row is the way to lower one.
- **Scalar-only inlining is a side effect of the switch.** Someone who turns on `--lower-scalars` without
  `--inline` still gets all-scalar callees expanded. The `--inline-report` table shows the `NotScalarBody`
  refusals.
- **The tree dump does not show it.** `--dump-tir` lists the TIR after each pass, and lowering is not one, so
  its effect is visible only in the emitted C#.
- **The compiled arm of `plato run` can run lowered** (`--lower-scalars` with `--backend=csharp` or
  `--differential`). The interpreter arm is unaffected. The C++ writer reports `InlineScalarBodies` as false
  and the CLI rejects the switch for every target but C#.
- **`--lean-inlining` is the companion switch.** It keeps `AggressiveInlining` only on one-line forwarding
  members, so generated callees do not fill the same budget (TKT-1091; see the pipeline reference). The
  TKT-1129 witnesses showed the two are independent and complementary: with lowering alone, the budget it
  freed let the JIT inline more generated callees into some witness bodies, which kept wrapper calls; with
  both switches the witnesses reach zero.
- **The measurement belongs to a report.** Whether the switch removes the refused calls in a given consumer is
  a dated result with its inputs, kept in the consumer's plans and not here.
- See also the [optimizer pipeline reference](../../docs/csharp-optimizer-pipeline.md#7a-scalar-lowering-at-emission---lower-scalars).
