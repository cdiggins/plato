# Plato.ForwardConformanceTests

Conformance suite for the **forward** stdlib (`submodules/Plato/stdlib`, the v3 vocabulary),
the sibling of `Ara3D.SDK.ConformanceTests` (which covers `stdlib-legacy`).

It runs the same reflection law runner (`LawTests`) and manifest machinery
(`ConformanceSupport`) over C# generated from the forward stdlib merged with the forward law
packet (`submodules/Plato/stdlib-tests/foundation.laws.plato`), against `Plato.Intrinsics.V2`.

## Status (2026-07-28): BLOCKED on codegen — does not execute yet

Full C# codegen of the forward stdlib does **not** yet succeed. Under both the plain and the
full V2 recipe, `Plato.CLI` aborts in `Plato.CSharpWriter` with:

```
System.InvalidOperationException: No ground TIR for bodied AnimationTrack.ValueAt;
the legacy body writer was removed (consolidation plan C4).
```

The forward stdlib contains bodied functions on concrete types whose monomorphized TIR is not
fully ground; `CSharpTypeWriter.WriteBody` throws on the first one and aborts **all** output
(`Plato.CSharpWriter/CSharpTypeWriter.cs:282`). `AnimationTrack.ValueAt` is merely the first —
fixing it will surface the next. Because the forward stdlib is a tightly coupled whole (a
foundation-only file subset fails symbol resolution), there is no runnable file subset either.

Consequently `Generated/` is empty. Note this project does **not even compile** with an empty
`Generated/`: `Plato.Intrinsics.V2` references generated extension methods (`Number.Pow2`,
`Number.Pow3`, `Vector3.AlmostZero`, ...) that only the generated stdlib supplies — exactly as
the legacy `Ara3D.SDK.ConformanceTests` only builds after `regen-conformance.ps1` produces its
`Generated/`. So today the suite neither builds nor runs; the Stage 1 type-check
(`regen-forward-conformance.ps1`, no args) is the only executable forward gate. Once codegen
lands and the project builds, `BlockerGuardTests` makes an empty/law-less generation an explicit
**RED** (never a vacuous green).

## What DOES pass today

`tools/regen-forward-conformance.ps1` (no args) — Stage 1 **type-check gate**: it merges
`stdlib` + `stdlib-tests`, lints the union, and asserts **0 symbol resolution errors**. This
proves the whole forward vocabulary plus the `Law_*` packet resolve against each other. That is
the honest, currently-green forward-stdlib gate.

## Activating execution (once the writer blocker is fixed)

1. Fix the "No ground TIR for bodied ..." writer gap so the full recipe generates the forward
   stdlib without aborting.
2. `tools/regen-forward-conformance.ps1 -Test` — regenerates `Generated/` and runs the suite.
3. Quarantine any real law failures in `KnownFailures.json`; remove each entry as its fix lands.
4. Once codegen is reliably green, relax/remove `BlockerGuardTests` and wire the suite into CI.

Deliberately **not** added to any `.sln`. `Generated/` is script-produced and gitignored.

## The law packet

`submodules/Plato/stdlib-tests/foundation.laws.plato` (`library FoundationLaws`) currently
covers the interval remap kit (`At`/`ParameterOf`/`Remap`), the generic `IntervalLike`
containment surface, and the concrete `Bounds2D`/`Bounds3D` AABB operations. Every member it
references was verified against the forward library source. Grow it the same way the legacy
`stdlib-legacy-tests/laws.plato` grew.
