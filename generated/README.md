# generated/ — buildable generated Plato projects

This folder holds **buildable C# projects** produced by running `Plato.CLI` over a Plato
standard library. Each is a real SDK-style `.csproj`, imports the handwritten intrinsics runtime
as a shared project, and compiles standalone on `net8.0`.

**Nothing here is a golden.** The byte-identity diff-gate, the `regen-generated.ps1` script that
drove it, and the legacy conformance suites were all retired 2026-07-30
(`tracker/decisions/2026-07-30-retire-legacy-conformance-and-goldens.md`). The `.g.cs` files are
ordinary cached output: regenerate them when useful, and accept staleness otherwise. They are
still not for hand editing (`// DO NOT EDIT`) — a change comes from rerunning the recipe.

## The projects

| Project | Source | Recipe (CLI flags) | State |
|---|---|---|---|
| `Plato.Generated.Foundation.Unoptimized` | `stdlib/foundation` (forward) | `--csharp-style=extensions` | Live; sources committed. |
| `Plato.Generated.Unoptimized` | `legacy/stdlib-legacy` | `--csharp-style=extensions --scalar=float` | **Empty shell** (see below). |
| `Plato.Generated.Optimized` | `legacy/stdlib-legacy` | the same plus `--optimize --optimize-arrays --inline --loops` | **Empty shell** (see below). |

The forward project keeps scalars as **wrapper structs** — `Number`, `Integer`, `Boolean`,
`Character`, `String` stay distinct types (decision `decc091`, 2026-08-01). The two legacy
projects use the older **scalar-erasure** recipe (`--scalar=float`), which rewrites those wrappers
to native `float`/`int`/`bool`/`char`/`string`.

Every recipe is property-free: a no-arg member emits as a method, so the emitted `partial struct`
halves agree with the method-form runtime. That is not selectable - see
[`../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md`](../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md).
Genuine fields and pseudo-fields (`X`/`Y`/`Z`, `M11`, `Row1`, `Plane.Normal`, `Count`) keep
field/property syntax in every recipe.

Each `.csproj` carries its own recipe in a header comment; that comment and this table are the
two places the flags are written down. (The two legacy `.csproj` headers still spell the retired
`no-properties` flag; they are empty shells and were left untouched.)

## The two legacy projects are empty shells (2026-08-01)

Their `.g.cs` sources were deleted on 2026-08-01. They no longer compiled: the scalar-erasure
recipe emits against a runtime shape that has moved on — the intrinsic-kernel reduction
(`plato-378`), the wrapper-scalar decision (`decc091`), and the departure of `Angle`, the matrices
and `Quaternion` from `src/Plato.Intrinsics` into `bonepile/`. Since the 2026-07-30 retirement
nothing regenerated them and no gate read them, so the checked-in text was stale output that only
produced compiler errors in a solution build.

The `.csproj` files are kept, still wired into `Plato.sln`, so the recipes stay recorded and the
projects build green while empty. **This was a deliberate deletion, not data loss** — the content
is reproducible from the source library by the command below, and the deleted text is in git
history before this change.

Whether the legacy recipe is worth reviving at all is a separate question; the empty shells are a
placeholder, not a commitment.

## Regenerating

From the repo root, output folder first cleared of `*.g.cs`:

```
dotnet run --project src\Plato.CLI -c Release -- ^
    legacy\stdlib-legacy generated\Plato.Generated.Unoptimized ^
    --csharp-style=extensions --scalar=float

dotnet run --project src\Plato.CLI -c Release -- ^
    legacy\stdlib-legacy generated\Plato.Generated.Optimized ^
    --csharp-style=extensions --scalar=float ^
    --optimize --optimize-arrays --inline --loops

dotnet run --project src\Plato.CLI -c Release -- ^
    stdlib\foundation generated\Plato.Generated.Foundation.Unoptimized ^
    --csharp-style=extensions
```

Regenerating the two legacy projects is expected to reproduce the compile errors described above
until the legacy library or the erasure recipe is brought back in line with the runtime; that is
why they were emptied rather than refreshed.

The `.csproj` in each folder is hand-maintained — a regeneration only writes `.g.cs`.
`docs.html` / `interfaces.txt` are generator side-products and are gitignored.

## Intrinsics link

All three projects consume the handwritten runtime by importing the shared project:

```xml
<Import Project="..\..\src\Plato.Intrinsics\Plato.Intrinsics.projitems" Label="Shared" />
```

and take `Ara3D.Collections` / `Ara3D.Memory` / `Ara3D.Utils` from the `Ara3D.SDK.Core` package.
`src/Plato.Intrinsics` is now **the** runtime: the V1/V2 split this file used to describe is gone,
the old `Plato.Intrinsics.Legacy` copy having been deleted 2026-07-31
(`tracker/decisions/2026-07-31-retire-v1-runtime-and-freeze.md`). The copies still living in
`ara3d-sdk` belong to that repo. Which artifact is which, and who consumes it, is mapped in
[`../docs/plato-library-map.md`](../docs/plato-library-map.md).

## No numbers here

This file records no file counts, error counts or build timings by design
(`docs/documentation-conventions.md`). For the current C# error totals per project run
`powershell tools/dotnet-build-record.ps1 -Project <csproj> -TargetName <name>` and read
`docs/status-report-snapshot.json`.
