---
id: plato-378
title: Shrink the host intrinsic contract to an irreducible kernel
type: debt
status: in-progress
priority: p1
effort: L
risk: med
area: plato
sprint:
created: 2026-07-31
closed:
links: [plato-368, plato-376, plato-365, stdlib/foundation/intrinsics.library.plato, stdlib/foundation/primitives-arrays.library.plato, stdlib/foundation/primitives-number.library.plato, stdlib/foundation/primitives-integer.library.plato]
---

## Task

`intrinsics.library.plato` carried **141 bodiless signatures**. Most were not
irreducible: they were formulas over other intrinsics, or exact duplicates of
generic bodies the stdlib already derives on `Orderable` / `Equatable`. Every
one is a line on the porting checklist for C++, CUDA, TypeScript, GLSL and Rust,
and an obligation `IntrinsicObligationTests` enforces against the C# runtime.

Reduce the contract to functions that genuinely cannot be written in Plato, and
state the admission rule so it cannot drift back up.

## Admission rule (new)

**An intrinsic must not be expressible in Plato from the other intrinsics.** If a
portable reference body exists, the function belongs in a `*.library.plato` file;
a backend recovers native speed through its override table (plato-368), not by
re-adding an intrinsic.

## What changed

Counted by section from the file itself:

| Section | Before | After |
|---|---|---|
| Number | 60 | 26 |
| Integer | 30 | 18 |
| Array | 26 | 5 |
| Array2D / Array3D | 5 | 0 |
| Boolean | 7 | 3 |
| Character / String | 2 | 2 |
| List | 7 | 7 |
| Buffer | 4 | 4 |
| **Total** | **141** | **65** |

A 54% cut, and the array surface — the question that started this — goes from
31 signatures to 5.

New reference-body files, all under `stdlib/foundation/`:

- `primitives-arrays.library.plato` (`library PrimitivesArrays`) — the whole
  derived array surface plus the Array2D/Array3D bodies.
- `primitives-number.library.plato` (`library PrimitivesNumber`)
- `primitives-integer.library.plato` (`library PrimitivesInteger`)

Edited: `primitives.library.plato` gained the Boolean derivations and lost its
circular `LessThanOrEquals(Boolean) => Compare(...)` body;
`core-comparison.library.plato` gained one generic
`NotEquals(a: Equatable, b: Equatable) => !a.Equals(b)`, which retires the
per-primitive NotEquals intrinsics tree-wide.

### The array kernel

`Count`, `At`, `MapRange`, `Reduce`, `FlatMap`. `MapRange` is the only
constructor and everything else is a reindexing of it. `Reduce` and `FlatMap`
stay because a Plato body is a pure expression with no loop and no recursion
contract — GLSL forbids recursion outright.

### Array2D / Array3D got an honest layout

They were opaque and field-less, which is why their construction and traversal
had to be intrinsics — the one documented exception to the primitive-only rule.
They now declare `Elements: Array<T>` plus extents **named for the obligations
they discharge** (`ColumnCount` / `RowCount` / `LayerCount`), so a FIELD fills
each one and the pairing defect never applies.

**This resolves the Array2D/Array3D half of plato-376 without a compiler
change.** The five grid LINT001 findings are gone (44 -> 39 warnings). The
generic-type pairing defect itself is untouched and still blocks the animation
tracks, so plato-376 stays open on that half.

## Gates

Measured with `plato_check` against `stdlib` + `stdlib-tests`:

| Gate | Before | After |
|---|---|---|
| parse | 0 failed | 0 failed |
| resolve | 0 errors | 0 errors |
| lint | 0 errors, 44 warnings | 0 errors, **39 warnings** |
| types | 3 failing | 3 failing (same three, pre-existing) |
| style | 0 errors, 27 warnings | 0 errors, 27 warnings |

## Open / follow-up

- [ ] **plato-368 is a hard prerequisite for hot paths.** Without the override
      table every derived view costs a closure per element on C#, where it used
      to bind to `Ara3D.Collections`. Nothing regressed in the gates because the
      gates do not execute; adoption in a hot path must wait for the table.
- [x] **`Drop` semantics — RESOLVED.** First landed as a synonym of `Skip`,
      which was wrong. `Drop(n)` is `Take(Count - n)`: remove the last n.
      That completes a 2x2 — Take/TakeLast KEEP n from the front/back, Skip/Drop
      REMOVE n from the front/back — and it is why the old contract declared all
      four. The synonym reading left the "remove from the back" cell empty and
      one name redundant.

      Evidence: `Ara3D.Collections.LinqArray` defines
      `DropLast(n) => Take(Count - n)` and has **no `Drop` at all**, so the old
      `Drop` intrinsic never had a runtime counterpart — a phantom declaration,
      which is why nothing ever called it. The plato-368 override table must map
      Plato `Drop` onto the runtime's `DropLast`.
- [ ] **Short-circuit loss.** `All` / `Any` are folds now, so they visit every
      element. Identical for pure callbacks; restore via the override table if a
      profile ever shows it.
- [ ] **Accuracy.** `Log10`, `Log2`, `Cbrt` and the inverse-trig family are
      identities over the kernel and are typically a few ulp worse than a native
      call. This is what plato-368 item 4 (ulp conformance harness) is for.
- [ ] **Codegen unproven.** `Array.Count` / `Array.At` are now DECLARED
      intrinsics where the writer previously synthesized them
      (`Linter.MembersImplementedByWriter`); the C# writer may emit both. The
      forward codegen path is red for unrelated reasons (plato-308), so this is
      not yet observable.
## Verification

`PlatoTests` **202/202 passed, 0 failed** (Release, 2026-07-31). Specifically:

- `IntrinsicObligationTests` + `IntrinsicsSurfaceTests` — 7/7. Removing
  declarations cannot break the obligation direction, and it did not.
- `ForwardStdLibLintTests` + `ForwardStdLibCheckerTests` — 9/9 with the ceiling
  lowered to 33. Type-checker diagnostics 0 / 3166 functions, ceiling 0.
- `IntrinsicContractSizeTests` (new) — 2/2, independently counting 65.

## Done means

- [x] Contract reduced to an irreducible kernel; admission rule stated in-file
      and in `AGENTS.md`
- [x] Reference bodies exist for everything removed
- [x] stdlib gates no worse than baseline on every axis, lint strictly better
- [x] C# test suite green (`PlatoTests` 202/202)
- [x] Count gate pinning the contract size (`IntrinsicContractSizeTests`), plus
      a companion test that no other `*.library.plato` file declares a bodiless
      signature
- [x] `Row(0).Count` workarounds replaced with the real extent members
