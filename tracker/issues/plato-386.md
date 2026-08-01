---
id: plato-386
title: ArrayIntrinsics.FlatMap leaks a mutable List through IReadOnlyList
type: debt
status: idea
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-01
closed:
links: [src/Plato.Intrinsics/ArrayIntrinsics.cs, src/Plato.Intrinsics/PlatoList.cs, stdlib/foundation/intrinsics.library.plato]
---

## Issue

`ArrayIntrinsics.FlatMap` (`src/Plato.Intrinsics/ArrayIntrinsics.cs:35-45`)
accumulates into a `System.Collections.Generic.List<TResult>` and returns it
directly as `IReadOnlyList<TResult>`. The static type is immutable; the runtime
object is not. Any caller can write `((List<TResult>)result).Add(x)` — or
`.Clear()`, or `[i] = y` — and mutate a value the whole stdlib treats as an
immutable `Array<T>`.

The Plato-side contract is explicit that this is an array:
`FlatMap(xs: Array<$T1>, f: Function1<$T1, Array<$T2>>): Array<$T2>`
(`stdlib/foundation/intrinsics.library.plato:167`). `Array<T>` is immutable by
definition in this stdlib; `List<T>` is the `unique` affine builder, and the only
sanctioned way to turn one into the other is `Freeze`, which invalidates the
builder (`intrinsics.library.plato:199`). `FlatMap` performs that conversion by
upcast instead — the one place in the intrinsics that skips the discipline.

`FlatMap` is one of five host array intrinsics that survived the plato-378
reduction, so it is load-bearing, not incidental.

## Impact

- Latent aliasing bug: nothing in the generated stdlib downcasts today, so no
  current miscompilation is known. The exposure is to hand-written consumers of
  the runtime and to future generated code.
- The neighbouring intrinsic gets this right: `MapRange`
  (`ArrayIntrinsics.cs:23`) returns `new ReadOnlyList<T>(count, i => f(i))` — a
  genuinely immutable view. `FlatMap` is the odd one out among the five.
- It weakens the affine-builder story: `PlatoList<T>.Freeze` goes to considerable
  trouble to hand off *without copying* and then invalidate the builder
  (`PlatoList.cs:105`), so that a frozen array provably has no live mutator. A
  plain upcast asserts the same guarantee with nothing behind it.

## Affected code

- `src/Plato.Intrinsics/ArrayIntrinsics.cs:35-45` — `FlatMap`, the whole body.
- `src/Plato.Intrinsics/ArrayIntrinsics.cs:23` — `MapRange`, the correct pattern
  in the same file.
- `src/Plato.Intrinsics/PlatoList.cs:105` and the `FrozenArray<T>` class below it
  — the zero-copy immutable-handoff mechanism this should be using;
  `FrozenArray`'s constructor is `internal` and `ArrayIntrinsics` is in the same
  shared project, so it is reachable.
- `stdlib/foundation/intrinsics.library.plato:167` — the Plato signature this
  body implements, promising `Array<$T2>`.

## Cause / analysis

`FlatMap` is the only *length-varying* producer in the array kernel — the file's
own header comment says so (`ArrayIntrinsics.cs:10`) — so it is the only one that
needs to accumulate before it knows its size. `MapRange` knows the count up front
and can hand back a lazy view; `Reduce` produces a scalar. Accumulation reached
for the nearest growable container and the result was returned without the
freeze step, most likely because `IReadOnlyList<T>` made it type-check.

## Priority

Low, and non-compounding. No consumer downcasts today, and the fix is a handful
of lines that cannot break a caller (the static type does not change). Worth
doing because it is cheap and because the invariant it protects — "an
`Array<T>` is immutable" — is one the rest of the design leans on hard.

## Dependencies

- Blocked by: nothing.
- Touches: `src/Plato.Intrinsics/ArrayIntrinsics.cs` only. No collision with the
  other open runtime issues (plato-383/384/385 touch the wrappers).

## Fix approaches

1. **Freeze through the existing mechanism.** Accumulate into a `T[]` (or a
   `List<TResult>` used purely as a local) and return
   `new FrozenArray<TResult>(items, count)` — the same object `PlatoList.Freeze`
   hands out. Zero copy if the backing array is taken directly, no new type, and
   it puts `FlatMap` on the same discipline as every other builder consumer.
2. **Accumulate into a `PlatoList<TResult>` and call `Freeze()`.** Even more
   direct — the intrinsic then uses the affine builder exactly as a Plato body
   would — at the cost of the builder's per-call `CheckNotFrozen` branches.
3. **`return r.ToArray();`** — one word, but a full copy on every call, and a
   `T[]` is itself castable back to a mutable array, so it does not actually
   close the hole.

## Bedrock

The invariant: **the only route from a builder to an `Array<T>` is `Freeze`.**
That is what makes the affine-builder design sound — `Freeze` is where the
runtime stops anyone from holding a mutator on a value that is now immutable.
`FlatMap` is currently a second, undisciplined route, in the host contract
itself, where every generated body can reach it. Routing it through
`FrozenArray` (approach 1) removes the exception and means the rule can be
stated without a caveat — which is also what makes it checkable later.

Verdict: **right**, and it is nearly as small as the wrong version.

## Done means

- [ ] `FlatMap` returns a type that cannot be downcast to a mutable collection.
- [ ] A test in `tests/Plato.Intrinsics.Tests` asserts the returned object is not
      a `List<T>` (and that mutating it is impossible), pinning the invariant.
- [ ] `tests/Plato.Intrinsics.Tests` passes and forward conformance builds.

## Simplest fix

Approach 1: keep the accumulation loop, return `new FrozenArray<TResult>(...)`
instead of the `List`.

- Get: the immutability promise in `intrinsics.library.plato:167` becomes true,
  with no allocation added and no caller signature changed.
- Give up / risk: `FrozenArray` gains a second construction site, so its
  `internal` constructor is now part of a small contract inside the shared
  project rather than a `PlatoList`/`PlatoBuffer` private detail.

## Prevention

- The regression test above covers this instance.
- Worth its own idea: a runtime-wide rule that no public member of
  `Plato.Intrinsics` may return a `System.Collections.Generic.List<T>` instance
  through an `IReadOnlyList<T>` — a small analyzer or a reflection test over the
  shared project's public surface would enforce it, and it is the same class of
  guard plato-384 wants for the wrapper family.
