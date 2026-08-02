---
date: 2026-08-01
title: Wrapper scalars are the only scalar representation
status: accepted
superseded-by:
links: [2026-08-01-property-free-emission-is-unconditional.md, ../issues/plato-331.md]
---

## Context

The C# writer could represent Plato's five scalar types — `Number`, `Integer`, `Boolean`,
`Character`, `String` — two ways:

- **Wrapper** (default): each stays a distinct single-field readonly struct.
- **Erased** (`--scalar=float`): each is replaced by its native BCL primitive, and the wrapper
  struct survives only as a minimal shim (`WriteScalarErasedType`).

Erasure was built for the performance recipe, on the assumption that a wrapper costs something a
primitive does not. It is the mode `generated/Plato.Generated.Unoptimized` and
`generated/Plato.Generated.Optimized` were emitted in.

The forward foundation tier moved to wrapper scalars in `decc091`, but that choice was recorded
only in a commit message and a project header — no decision record stated why, so the reasoning
was not available to anyone deciding whether to keep the erased path alive. This record supplies
it and closes the question.

Erasure also acquired an unrelated passenger. Because `--no-properties` was gated behind
`--scalar=float`, sites across the writer read the erasure flag when they meant "spell this member
as a method". Retiring the property-ful variant
([2026-08-01-property-free-emission-is-unconditional.md](2026-08-01-property-free-emission-is-unconditional.md))
separated the two, which is what makes erasure removable on its own terms.

## Decision

**Wrapper scalars are the only scalar representation.** `--scalar=float`, the `scalarErase`
parameter, `CSharpWriter.ScalarErase`, `CSharpTypeWriter.EraseScalars`, and every branch reading
them are retired. There is no erased output shape and nothing tests one.

## Rationale

Erasure trades away the property that makes generated C# worth checking at all.

- **Constraints survive into C#.** `Length`, `Area`, and `Probability` are distinct types under
  wrappers. Erased, all three are `float`, so adding a length to an area compiles clean and the
  unit error reaches runtime.
- **A BCL primitive cannot be constrained retroactively.** No interface implementation, no
  invariant check, no restricted operator set can be attached to `float`. The wrapper struct is
  the only place those can hang. This is the load-bearing reason: erasure does not merely lose
  checking, it removes the surface that checking would attach to.
- **Interface conformance needs a nominal type.** `float` cannot implement `INumerical<Self>`. Under
  erasure, conformance stops being expressible in C# and the C# compiler takes Plato's word for it.
- **Two independent checkers beat one.** Wrappers let the C# compiler re-derive, from the emitted
  code alone, the type discipline Plato claims to enforce. That is a genuine check on the emitter,
  not a restatement of its own beliefs.

The cost erasure was built to avoid is narrower than it looks. A single-field readonly struct is
flattened by the JIT, so scalar arithmetic is not where the penalty lands. It lands at buffer
boundaries — SIMD loads, interop, GPU upload — where `Number[]` is not `float[]`. Emitted scalar
structs carry `[StructLayout(LayoutKind.Sequential, Pack=1)]` over a single field, so
`MemoryMarshal.Cast<Number, float>` is layout-sound and free. Reinterpreting at the boundary gets
the same bytes without an emitter mode.

## Alternatives rejected

- **Keep erasure behind the flag.** This is the debt the property-free decision just paid off,
  re-incurred under a different name: a second output shape nobody ships, with every affected site
  branching on it. An unshipped mode is not tested by the things that matter and rots against the
  one that is.
- **Erase only at buffer boundaries.** Already available as `MemoryMarshal.Cast`, and correct
  precisely because it is local. It needs no compiler flag and no whole-program mode.
- **Erase for the optimized recipe only.** This is the status quo, and it is what made the flag
  load-bearing for scalar lowering. See the consequence below — it is a reason to confront the
  coupling, not to preserve it.

## Consequences

- **Scalar lowering is gone; component unrolling survived.** This was the open risk when the
  decision was taken, and it resolved better than feared. `TirScalarLowerer` existed only to
  rewrite wrapper types into primitives, so it went with erasure and `RunOptimizerPasses` now
  runs four unconditional passes instead of five. `ComponentUnroller` / `TirComponentUnroller`
  turned out to branch on `ScalarErase` only to choose a CAST direction, not to decide whether to
  unroll — dropping that branch left field-wise unrolling fully intact, and its tests pass
  unchanged. The optimizer therefore keeps `--optimize`, `--optimize-arrays`, `--inline` and
  `--loops`; only the erasure-specific lowering, which had no consumer left once the erased
  generated projects were retired, is lost.
- **The change is behaviour-preserving for the live target.** `stdlib/foundation` regenerates
  byte-identically (301 files, ignoring the timestamp header) against the output committed before
  the deletion, and the full unit suite passes. Every erasure branch really was dead code on the
  shipping recipe.
- **The erased generated projects go with it.** `Plato.Generated.Unoptimized` and
  `Plato.Generated.Optimized` name `scalar=float` in their recipes. Retiring the flag retires
  those projects; they were already emptied against the retired V1 runtime.
- **`Plato.Generated.Foundation.Unoptimized` is unaffected** — it never asked for erasure.
- **Performance work moves to the boundary.** Any future SIMD or interop path reinterprets spans
  rather than asking the emitter for primitives.
- **Boxing becomes the cost to watch.** With wrappers, an interface-typed value used outside a
  constrained generic can box. That is the wrapper penalty worth measuring, not scalar arithmetic.
