# C# intrinsic runtime

This shared project supplies the handwritten runtime used by generated Plato C# libraries. Developers implementing a primitive host operation should start with the [intrinsic contract](../../docs/plato-intrinsics-surface.md), which defines what belongs here and what must remain a Plato reference body.

The scalar files implement wrappers and their primitive operations. `ArrayIntrinsics.cs`, `PlatoList.cs` and `PlatoBuffer.cs` implement array access and affine builders. `PlatoJson.cs` and `PlatoScalarJsonConverters.cs` control serialization; `GridExtensions.cs` adapts collection grid names. `NumberProfile.cs` defines profile-dependent machine aliases.

Consumers import `Plato.Intrinsics.projitems` rather than referencing a standalone runtime assembly. `PlatoNumberProfile` selects the numeric representation; see [language semantics](../../docs/SEMANTICS.md). The [compiled law backend](../Plato.Laws/README.md) also embeds the top-level C# files as sources for in-process compilation. Keep those paths stable unless both consumers are updated. The [bonepile](bonepile/README.md) contains excluded historical implementations.

Use the repository [foundation regeneration command](../../generated/README.md) to exercise generated consumers. Intrinsic obligation tests in [PlatoTests](../../tests/PlatoTests) check the declared host surface. Actual gate outcomes live in the [gate log](../../docs/gate-log.md), rather than this guide.
