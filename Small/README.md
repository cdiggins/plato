# Small/ — self-contained optimizer-spike projects

A fast, decoupled loop for iterating on the Plato C# optimizer. The corpus
[`plato-src-small`](../plato-src-small) is a small, **self-contained** Plato library that
reproduces the optimizer-relevant shapes of the real geometry library — `Point3D`/`Bounds3D`/
`Line3D` component ops, `Deform`, `Eval`, `ZipComponents` — without depending on
`Plato.Intrinsics.V2` or the rest of `plato-src`. It builds against a tiny hand-written runtime
([`Plato.Small.Runtime`](Plato.Small.Runtime)) instead.

This is a **development aid, not a release gate**: the `.g.cs` here are overwritten in place by
`regen-small.ps1` (not diff-gated like `Generated/`), and the projects are intentionally left out
of `check-all.ps1`.

## Layout

| Path | What |
|---|---|
| `../plato-src-small/small.plato` | the self-contained corpus |
| `Plato.Small.Runtime/` | minimal runtime: `Intrinsics.MakeArray`/`CombineHashCodes`, eager `IReadOnlyList` combinators, and `Number`/`Integer`/`Boolean` scalar wrapper stubs |
| `Plato.Small.Generated.Unoptimized/` | recipe `--csharp-style=extensions --scalar=float --no-properties` |
| `Plato.Small.Optimized/` | + `--optimize --optimize-arrays --inline --methods --loops` |
| `regen-small.ps1` | regenerate both variants (and build them) |

## Use

```powershell
.\Small\regen-small.ps1            # regenerate + build both variants
.\Small\regen-small.ps1 -DumpTir   # also write per-phase TIR next to the optimized output
.\Small\regen-small.ps1 -NoBuild   # regenerate only
```

Then eyeball `Plato.Small.Optimized/Geometry.g.cs` (`Deform`/`Eval`) and `ArrayLibrary.g.cs`
(`ZipComponents`) and diff against `Plato.Small.Generated.Unoptimized/`.

## Constraints (keep the corpus self-contained)

- Only `Number`/`Integer`/`Boolean` are erasable primitives (native `float`/`int`/`bool`).
- **Never** use `Vector2`/`Vector3`/`Vector4`/`Angle`/`Matrix*`/`Quaternion` — `CSharpWriter.PrimitiveTypes`
  hardcodes those names to `System.Numerics.*`, which would require the V2 partials. Build geometry
  on `Point3D` (a plain generated struct) instead.
- Any new corpus function whose emitted C# needs a runtime helper must have that helper added to
  `Plato.Small.Runtime`.
