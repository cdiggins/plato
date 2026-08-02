---
id: plato-374
title: Interface-generic law bodies mix the interface default with the type's override
type: bug
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-31
closed:
links: []
---

## Issue

When a generic function over an interface is monomorphized onto a concrete type that
*specializes* one of the interface's derived members, the emitted C# is **inconsistent about
which body it uses**, and which one wins depends on whether `--inline` fired.

Reproduced 2026-07-31 with the forward conformance recipe
(`--csharp-style=extensions --scalar=float --optimize --inline --methods ...`).
`stdlib/foundation/intervals-transforms-interval.library.plato` declares the generic
`Contains(self: IntervalLike<$T>, value: $T)` (linear, `Start <= v && v <= End`) plus a
wrap-aware specialization `Contains(self: AngleInterval, a: Angle)`. Monomorphizing
`Law_ContainsStart` / `Law_ContainsEnd` / `Law_ContainsCenter` (all of the form
`self.Contains(<expr>)`, `tests/stdlib-tests/foundation.laws.plato`) onto `AngleInterval`
produced, in `Generated/_AngleInterval.g.cs`:

```csharp
// Law_ContainsStart  -- the GENERIC body, inlined
public bool Law_ContainsStart() => ... this.Start.LessThanOrEquals(this.Start).And(this.Start.LessThanOrEquals(this.End));
// Law_ContainsCenter -- a surviving CALL, re-bound by C# to the AngleInterval extension
public bool Law_ContainsCenter() => ... this.Contains(this.Start.Lerp(this.End, 0.5f));
```

Three calls to the same name in the same file, two different callees. The mechanism: Plato
resolves `self.Contains` against the constraint (`IntervalLike`), where the specialization is
invisible, so the interface default is correct-by-construction; but when the inliner declines to
inline, the emitted `this.Contains(x)` is re-resolved by **C# extension-method overload
resolution**, which does see `Contains(this AngleInterval, Angle)` and prefers it. So the
generated program's meaning depends on an optimizer heuristic.

Consequence that surfaced this: `AngleInterval.Law_ContainsCenter` failed 5/25 trials with a
chimera -- the interface's linear `Center` (inlined) checked against the type's arc `Contains`
(re-bound). Neither reading fails on its own. Worked around in that commit by adding an
`AngleInterval`-receiver `Law_ContainsCenter` to the law packet, which resolves entirely on
the concrete surface; the generic law remains a latent trap for the next specialization.

## Fix approaches

1. Decide the rule and enforce it in one place. Either **the interface default always wins**
   inside a generic body (then the writer must emit a direct call to the interface's own static,
   not a C#-resolvable extension call), or **the concrete override always wins** (then
   monomorphization must re-resolve every member access against the substituted type before
   the inliner runs). The first is the smaller change and matches the current type-checker
   semantics; the second matches what most readers expect from `Contains`.
2. Either way, add a writer-level guard: an emitted call whose C# overload resolution would
   select a different function than the one the checker bound is an error, not a silent
   rebind. That catches the whole class rather than this instance.

## Done means

- [ ] A generic body monomorphized onto a specializing type emits the same callee whether or
      not `--inline` is on.
- [ ] A regression test in `tests/PlatoTests` pins the chosen rule for the
      `IntervalLike.Contains` / `AngleInterval.Contains` pair.
- [ ] The `AngleInterval`-receiver `Law_ContainsCenter` workaround in
      `tests/stdlib-tests/foundation.laws.plato` is revisited (kept on merit, not necessity).
