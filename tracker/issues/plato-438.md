---
id: plato-438
title: PlatoTests: TypeScript UseTir byte-identity test fails on number/Number casing
type: bug
status: in-progress
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`PlatoTests.TypeScriptEmitFlagOnTests.UseTirOnReproducesTheFullTypeScriptLibraryByteForByte`
fails at HEAD (found 2026-08-03 while landing plato-436, and verified with that
change stashed, so it is unrelated to the memoized `Arr`). The flag-off (legacy
symbol-graph) and flag-on (TIR) TypeScript body writers disagree on the casing
of the native-number receiver in static constant bodies:

```
off: static MaxValue(): Vector2 { return Vector2.CreateFromComponent(number.MaxValue()); }
on : static MaxValue(): Vector2 { return Vector2.CreateFromComponent(Number.MaxValue()); }
```

(`MinValue` likewise; run the test for the full DIFFERS list.) The TIR
spelling `Number.MaxValue()` is the one the prelude installs statics on, so the
flag-off writer looks like the wrong side. The test is the flip criterion for
the TS writer's TIR migration, so it should be green before that flag flips.

Rerun: `dotnet test tests\PlatoTests\PlatoTests.csproj -c Release --filter
"FullyQualifiedName~UseTirOnReproducesTheFullTypeScriptLibraryByteForByte"`.

## Done means

- [x] The byte-identity test passes, with the surviving spelling being the one
      the generated library actually resolves at runtime

## Resolution

Legacy TypeScript type expressions now use the existing primitive-keyword-to-runtime-constructor mapping, matching the typed-body writer and the prelude's installed statics. `Number.MinValue()` and `Number.MaxValue()` therefore resolve to the constructor on which the runtime installs them, rather than the erased type keyword `number`.

Fixing that spelling exposed one remaining mismatch: conditional branches in the legacy writer discarded the expected result type, causing capsule points to be emitted as `Tuple3` instead of `Point3D`. Both branches now receive the same expected type recursively. This preserves the declared result type through nested conditionals.

The existing full-library comparison independently checks the runtime installation/static spelling and all named point-constructor branches, reusing its two generated outputs rather than compiling the corpus again. All three TypeScript tests passed (22.6 seconds wall time), including byte-identical legacy and typed-body output. No normalization or exclusion was added.
