# Generated/ — buildable generated Plato projects

This folder holds **buildable C# projects** produced by compiling the Plato standard library
(`plato-src`) with `Plato.CLI`. Unlike `golden/` (which holds loose `.g.cs` diffed by a script),
each project here is a real SDK-style `.csproj`, references the intrinsics runtime, compiles
standalone on `net8.0`, and is a member of the root `Ara3D.Studio.sln`.

Each project is a **golden** in the golden-master sense: the `.g.cs` files are committed and are
the reviewed reference output. Any change to them must come from a deliberate regeneration
(`tools\regen-generated.ps1`), never a hand edit — hence the `// DO NOT EDIT` headers.

## Variants

Both variants use the extension-method C# style with **scalar erasure** (`scalar=float`): the
`Number`/`Integer`/`Boolean`/`Character`/`String` wrapper structs are erased to the native
`float`/`int`/`bool`/`char`/`string` primitives. `Angle`, `Time` and the other unit-carrying
structs remain real types.

| Project | Recipe (CLI flags) | Purpose |
|---|---|---|
| `Plato.Generated.Unoptimized` | `--csharp-style=extensions --scalar=float --no-properties` | Readable reference shape; optimizer passes OFF. |
| `Plato.Generated.Optimized`   | `--csharp-style=extensions --scalar=float --optimize --optimize-arrays --inline --methods --loops --no-properties` | Intended adoption/shipping shape; full optimizer pipeline ON. |

Both variants are **property-free** (`--no-properties`): every no-arg member is a method,
including the primitive-struct members (`angle.Cos()`, `v.Normalize()`, `n.Abs()`) that plain
`--methods` still keeps as properties. To make the fused `partial struct` halves agree, both
projects link **`Plato.Intrinsics.V2`** (a copy of the handwritten runtime whose primitive
members are methods, not properties) instead of `Plato.Intrinsics`. Genuine fields and
pseudo-fields (`X`/`Y`/`Z`, `M11`, `Row1`, `Plane.Normal`, `Count`) stay property/field syntax.

The optimized recipe is exactly the one the **Scalar conformance suite** semantically gates
(`tools\regen-conformance-scalar.ps1`), so the optimized golden is proven equivalent to the
unoptimized one. **Diff the two folders** to see precisely what the optimizer passes
(`--optimize` component unrolling, `--optimize-arrays` eager materialization, `--inline`
call inlining, `--methods` property/indexer erasure, `--loops` combinator-to-`for` lowering) do
to the emitted code.

## Intrinsics link

Both projects consume the handwritten runtime by importing the shared project:

```xml
<Import Project="..\..\Plato.Intrinsics.V2\Plato.Intrinsics.V2.projitems" Label="Shared" />
```

and reference `Ara3D.Collections` / `Ara3D.Memory` / `Ara3D.Utils` from `ara3d-sdk`.
`Plato.Intrinsics.V2` is the **method-form** copy of the wrapper structs (`Number`, `Vector2-8`,
`Matrix`, `Angle`, …), used only by these `--no-properties` goldens. `Plato.Intrinsics` (V1)
remains the source of truth for the property-form runtime and the default-mode SDK; it is byte-
synced to ara3d-sdk (never edit that copy). When you change a member in one, mirror it in the
other — V1 keeps the property, V2 exposes the method.

## Regenerating

From the studio repo root:

```
.\tools\regen-generated.ps1          # diff the checked-in golden vs a fresh generation, exit 1 on drift
.\tools\regen-generated.ps1 -Apply   # refresh the .g.cs files in place
```

The `.csproj` / `README.md` in each variant folder are hand-maintained; the regen script only
touches `.g.cs`. `docs.html` / `interfaces.txt` are generator side-products and are gitignored.

## Relationship to `golden/` and `ara3d-sdk/src/Plato.Generated`

- `golden/Plato.Generated.V2/` — the older loose-`.g.cs` golden, diff-gated by `regen-golden-v2.ps1`
  (a `.shproj`, not standalone-buildable). These `Generated/` projects supersede it as the
  buildable, solution-wired form.
- `ara3d-sdk/src/Plato.Generated` — the default C# style, wrapper-typed output, byte-identity gated
  by `regen-plato.ps1`. That is effectively the "original, unerased" golden.
