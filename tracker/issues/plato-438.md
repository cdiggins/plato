---
id: plato-438
title: PlatoTests: TypeScript UseTir byte-identity test fails on number/Number casing
type: bug
status: ready
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

- [ ] The byte-identity test passes, with the surviving spelling being the one
      the generated library actually resolves at runtime
