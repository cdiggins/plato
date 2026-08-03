---
id: compiler-416
title: Library functions taking Buffer<T> as first parameter are silently dropped from emitted C#
type: problem
status: idea
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-08-03
closed:
links: [writers/Plato.CSharpWriter/CSharpWriter.cs, stdlib/foundation/primitives.plato, stdlib/geometry/triangulation.library.plato, experiments/earcut/earcut-fast.plato, plato-415]
---

## Issue

A `library` function whose **first** parameter is `Buffer<T>` (or `List<T>`) is never written to
the emitted C#, and nothing reports it. Call sites of that function survive, so the generated
code references a method that does not exist and the C# does not compile.

The mechanism is in `BuildExtensionPlans` (`writers/Plato.CSharpWriter/CSharpWriter.cs:68`): the
writer builds one extension-style plan per concrete type and skips `c.TypeDef.IsUnique`. A
function is assigned to the plan of its first parameter's type, so a function dispatching on a
builder has no plan to live in and falls out of the emission entirely. There is no
`DegradedBodies` entry and no diagnostic — the same silent-drop shape the CS0736 work removed
elsewhere.

The buffer in any other parameter position is fine: `NodePoint(points: Array<Point2D>, nodes:
Buffer<EarClipNode>, slot: Integer)` emits normally.

## Impact

The affine builders are the language's answer to "an algorithm that genuinely needs mutable
scratch storage" (`stdlib/foundation/primitives.plato`). Every such algorithm wants helpers that
take the builder and hand it back — `Unlink(nodes, slot)`, `LinkRing(nodes, ...)`. Today those
helpers cannot exist, so the whole algorithm has to be written as one enormous function. That is
exactly the shape of `experiments/earcut/earcut-fast.plato`: a single ~380-line body whose every
helper takes the buffer as a non-first parameter. It reads as a deliberate style choice and is
in fact a workaround for this defect.

`stdlib/geometry/triangulation.library.plato` works around it too, by leading each function with
the point pool or with the slot it operates on and taking the buffer second. A comment there
names this issue.

Discovered while porting the ear-clipping triangulator: the staged codegen produced CS1061 for
twelve call sites at once, all of them functions with a buffer receiver.

## Fix approaches

1. **Emit builder-receiver functions into `Extensions.g.cs`** as plain extension methods on
   `PlatoBuffer<T>` / `PlatoList<T>`. The runtime types are ordinary sealed classes, so
   `this PlatoBuffer<EarClipNode> nodes` is legal C#; the plan machinery is what excludes them,
   not the target language. Most direct fix.
2. **Reject at check time.** If emission cannot support it, a checker rule (CHK3xx) refusing a
   `unique` first parameter turns a broken build into an actionable error at the source. Strictly
   worse than (1) for authors, but far better than silence, and cheap.
3. **Record it as a degraded body** so the writer reports what it dropped, in the manner of
   `Writer.DegradedBodies`. Diagnostic only — the C# still would not compile.

(1) and (2) are not exclusive: ship (2) now, (1) when the plan machinery is touched anyway.

## Done means

- [ ] A library function whose first parameter is `Buffer<T>` either emits and compiles, or is
      refused with a diagnostic naming the function.
- [ ] A regression test covers the case — a small library with a builder-receiver function,
      generated and compiled.
- [ ] `stdlib/geometry/triangulation.library.plato` drops its parameter-order workaround comment
      if (1) lands.

## Simplest possible implementation

Approach (2): in the checker, refuse a `unique`-typed first parameter on a library function,
pointing at this issue and suggesting the buffer move to a later parameter.

- **What you get** — the failure becomes a source-level error at the moment of writing, instead
  of a pile of CS1061s in generated code that most authors will never generate.
- **What you give up** — the ergonomic form stays unavailable, so affine algorithms keep paying
  the awkward parameter order; and it enshrines the restriction, which someone must later
  remember to lift.
