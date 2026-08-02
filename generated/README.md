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

It keeps scalars as **wrapper structs** — `Number`, `Integer`, `Boolean`, `Character`, `String`
stay distinct types. That is now the only scalar representation
([`../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md`](../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)).

The recipe is property-free: a no-arg member emits as a method, so the emitted `partial struct`
halves agree with the method-form runtime. That is not selectable — see
[`../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md`](../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md).
Genuine fields and pseudo-fields (`X`/`Y`/`Z`, `M11`, `Row1`, `Plane.Normal`, `Count`) keep
field/property syntax.

The `.csproj` carries its recipe in a header comment; that comment and this table are the two
places the flags are written down.

## The two legacy projects were retired (2026-08-01)

`Plato.Generated.Unoptimized` and `Plato.Generated.Optimized` are **gone** — sources, `.csproj`,
and `Plato.sln` entries. `tests/optimizer-smoke/Bench` went with them, since it existed only to
benchmark the optimized library against the unoptimized one through `extern alias`.

Both were emitted from `legacy/stdlib-legacy` with the **scalar-erasure** recipe
(`--scalar=float`), which rewrites the wrapper structs to native `float`/`int`/`bool`/`char`/
`string`. That recipe stopped matching the runtime — the intrinsic-kernel reduction (`plato-378`),
the wrapper-scalar decision, and the departure of `Angle`, the matrices and `Quaternion` from
`src/Plato.Intrinsics` into `bonepile/`. Their sources were emptied on 2026-08-01 and the shells
removed the same day once erasure itself was settled as a dead end.

**Deliberate retirement, not data loss** — every deleted file is in git history, and the emitted
text was always reproducible from its source library. Reviving the legacy tier means choosing a
recipe that works against the current runtime, not reverting these commits.

## Regenerating

```
.\tools\regen-foundation.ps1
```

That clears stale `*.g.cs` (so a type deleted from the library stops being emitted), runs the
recipe, and **builds** the result — the build is what makes the regeneration mean anything, since
generate mode exits 0 even when a body fails to lower into a throwing stub. `-WhatIf` previews the
diff into `.temp\` without touching the tracked output, `-Test` adds
`tests\Plato.Generated.Foundation.Tests`, `-Flags` tries an experimental recipe.

The underlying invocation, if you want to run it by hand from the repo root with the output folder
cleared first:

```
dotnet run --project src\Plato.CLI -c Release -- ^
    stdlib\foundation generated\Plato.Generated.Foundation.Unoptimized ^
    --csharp-style=extensions
```

The `.csproj` is hand-maintained — a regeneration only writes `.g.cs`.
`docs.html` is a generator side-product and is gitignored.

## Intrinsics link

The project consumes the handwritten runtime by importing the shared project:

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
