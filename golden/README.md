# golden/ — compiler golden artifacts

`Plato.Generated.V2` is the golden extension-style production output of `plato-src`. Since
2026-07-10 the optimizers are ON by default — the golden shows the optimized adoption shape
(component-op unrolling + eager materialization of struct-stored Map/MapRange results):

```
Plato.CLI plato-src golden\Plato.Generated.V2 --csharp-style=extensions --scalar=float --optimize --optimize-arrays
```

Regenerate/diff with `tools\regen-golden-v2.ps1` (`-Apply` to refresh); `check-all.ps1` gates it.
The same recipe is semantically gated by the Scalar conformance suite (which runs both optimizer
flags over merged plato-src + plato-test-src).

This is the **intended adoption shape** (roadmap Phase 2 revision): classic extension methods
(one static class per Plato library), default LangVersion on net8.0, and **scalar erasure** —
the `Number`/`Integer`/`Boolean`/`Character`/`String` wrapper structs are retired from all
generated signatures in favor of the native primitives. `Angle`, `Time` and every other struct
remain real types (unit safety is their purpose).

## Signature-shape changes vs the previous (wrapper-typed) golden

| Where | Before (`--scalar=wrapper`) | After (`--scalar=float`) |
|---|---|---|
| Library extension methods | `public static Number ClampZeroOne(this Number x)` | `public static float ClampZeroOne(this float x)` |
| Scalar member fanout | `public Number Sqr { get => this.Pow2; }` (struct property) | `public static float Sqr(this float x) => x.Pow2();` |
| Array-typed signatures | `public static IReadOnlyList<Number> Fractions(this Integer x)` | `public static IReadOnlyList<float> Fractions(this int x)` |
| Struct fields | `[DataMember] public readonly Number X;` | `[DataMember] public readonly float X;` |
| Literals | `((Number)0.5)` | `0.5f` |

Details of the erased shape:

- **`_Number.g.cs` / `_Integer.g.cs` / `_Boolean.g.cs` / `_Character.g.cs` / `_String.g.cs`**
  no longer declare partial structs. Each holds `public static class {Name}Extensions` with
  every scalar member as a classic extension method on the primitive: functions with Plato
  bodies are emitted in full; handwritten intrinsics get forwarders
  (`public static float Sqrt(this float self) => ((Number)self).Sqrt;`); operator-named
  intrinsics forward through the wrapper operators (`Add(this float a, float b)`).
  Operators, indexers, and C# interface implementations are **dropped** (see the
  `// scalar-erasure drop` comments at the bottom of each file) — primitives already have
  operators, and Plato interface obligations are meaningless without a generated struct.
- **Wrapper-receiver bridges**: each scalar extension also has a `this Number`/`this Integer`/…
  twin, because unerased kept members and handwritten intrinsics still return wrapper-typed
  values at some call sites (extension receivers never apply user-defined conversions, so the
  twins cannot conflict or bind ambiguously).
- **`public partial struct Number` shim** (bottom of `_Number.g.cs`): only
  `AlmostZero`/`Pow2`/`Pow3` as wrapper-typed properties — handwritten `Plato.Intrinsics` code
  accesses these with property syntax and cannot change.
- **Re-homed implicit conversions**: the broadcast conversions that lived on the `Number`
  struct (`implicit operator Vector2(Number)` …) now live on the target types' partial structs,
  still wrapper-sourced to keep overload resolution unambiguous.
- **Deliberately NOT erased** (the handwritten-intrinsics boundary, until full intrinsic
  erasure at V2 adoption): the concept interfaces in `Interfaces.g.cs`, and struct-KEPT members
  of non-scalar types that implement those interfaces (handwritten members like
  `Vector3.Magnitude : Number` satisfy the same obligations and cannot change signature).
  Their bodies still interoperate with erased code through the wrappers' implicit conversions
  and emitter-inserted casts.

`Plato.Generated.V2.projitems` / `.shproj` are hand-maintained (glob-include all `.cs`).
`docs.html` / `interfaces.txt` are generator side-products, not tracked.

Gates: byte-identity of the default emitter (`tools\regen-plato.ps1`), conformance suites V1 /
V2 / Opt / Scalar all at 142 pass / 36 ignored-known / 0 fail (`tools\regen-conformance*.ps1`),
and this folder compiling standalone against `Plato.Intrinsics` on net8.0 with the default
LangVersion.
