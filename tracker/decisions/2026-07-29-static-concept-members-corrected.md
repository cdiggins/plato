---
date: 2026-07-29
title: Static interface members — corrected; `_` is opt-in, `Zero` stays an instance member
status: accepted
superseded-by:
links: [tracker/decisions/2026-07-29-static-interface-members.md, tracker/issues/plato-312.md, submodules/Plato/stdlib/vectors.concepts.plato, submodules/Plato/stdlib/numeric-structures-components.library.plato]
---

Supersedes [2026-07-29-static-interface-members](2026-07-29-static-interface-members.md). Decisions 1–4
of that ADR stand; **decision 5 was wrong and is withdrawn**.

## Context

The original ADR assumed every `_`-receiver interface member is genuinely type-level, and therefore
that `Additive.Zero` should be redeclared `Zero(_: Self)` and emitted as C# `static abstract`,
with the twenty instance-ifying renames of Plato `b055944` reverted.

Implementing it disproved the assumption. Three measured facts:

1. `Zero(self: Vector) => self.Broadcast(0.0)`
   (`numeric-structures-components.library.plato:113`) **uses its receiver**: for `VectorN` the
   arity comes from the instance. My own `Zero(x: MatrixN)` does the same via `x.NumRows` /
   `x.NumColumns`. A static `Zero()` has no arity to build from.
2. `Broadcast(self: VectorN, x: Number)` (same file, line 191) also uses its receiver for arity,
   while `interface Vector` declared `Broadcast(_: Self, x: Number)` — drift in the *opposite*
   direction from the CS0736 case.
3. **`Self.` does not exist in the forward stdlib** (zero occurrences; `stdlib-legacy` has
   `Self.CreateFromComponent`). So a receiver-less body has no way to name its own type, which
   is what a generic `Zero` fill over `Quantity` would need.

## Decision

1. **`Zero` / `One` / `MinValue` / `MaxValue` stay INSTANCE obligations.** The `b055944` rename was
   correct and stands. The interface was right; the five concrete bodies were wrong.
2. **`--static-abstract` is an opt-in emitter flag**, default false, applied only to interface members
   that are genuinely type-level. Today that is exactly three: `Quantity.FromAmount`,
   `Vector.FromComponents`, `OriginBased.FromOffset`.
3. **`Vector.Broadcast` becomes an instance obligation** (`Broadcast(self: Self, x: Number)`), and
   the six fixed-arity implementations are renamed to match, resolving fact 2.
4. Opt-in rather than default because `stdlib-legacy`'s `IArrayLike<T>` declares three `_`-receiver
   members; enabling it there would add static abstract obligations to every implementor and move
   the diff-gated goldens.

## Rationale

"Receiver value unused" and "type-level operation" are not the same predicate, and the original ADR
conflated them — the same conflation it criticised the `_` convention for. `Color.Zero` ignores its
receiver; `VectorN.Zero` cannot. An interface obligation must be shaped for its *hardest* implementor,
so `Zero` is an instance member and `Color` simply ignores the argument.

This makes the checked-marker rule (decision 2 of the original ADR) more valuable, not less: it is
now the only thing that would catch drift in *either* direction, and both directions were present
in the tree simultaneously.

## Consequences

- Verified: `--static-abstract` on the forward recipe emits exactly the three intended members and
  is **net-neutral on compile errors** — 364 before, 364 after, identical distribution. CS0736 is 0
  either way, because `b055944` already fixed it.
- `regen-generated.ps1`: 368 files, 0 differing. Legacy emission byte-identical; hard rule 2 holds.
- `ForwardStdLibCheckerTests`: 0/2133 diagnostics (ceiling 0), 5/5 pass.
- The UFCS extension pairing (original decision 4) was **not needed** and was not implemented: the
  generated code monomorphizes before emission, so no call site needs it. It stays available — it
  was verified to work on net8.0 — if a C# consumer scenario ever demands it.
- The checker rule remains unimplemented and is the substantive piece of plato-312 still open.
