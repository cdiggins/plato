# Plato library map — the runtime and the generated libraries (2026-08-02)

> The one-page answer to "what are all these Plato.* things and which one is real?" Read this before
> touching the generated libraries or the intrinsics runtime.

## TL;DR

There is now exactly one shape: **wrapper scalars** (`Number` / `Integer` / `Boolean` /
`Character` / `String` stay distinct structs), extension methods, no properties or indexers,
`System.Numerics`-backed. The old property-bearing V1 shape was deleted from this repo on
2026-07-31 along with its freeze; scalar erasure to native `float` — the shape this paragraph used
to describe — was retired the day after
([decision](../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)),
and `--scalar=` is now a hard CLI error.

Nothing in this repo is frozen. `generated/` is ordinary cached output — regenerate it whenever you
like; staleness is acceptable and there is no byte-identity gate.

## The runtime (handwritten C# primitives — `Vector3`, `Number`, `Angle`, `Matrix4x4`, …)

| Path | What |
|---|---|
| `src/Plato.Intrinsics` | **The** runtime. Shared project (`.shproj` / `.projitems`), root namespace `Plato.Intrinsics`, code in namespace `Ara3D.Geometry`. |

Its C#-consumer surface (operators, constructors, conversions, field-properties, static factories,
BCL obligations) is contracted in `docs/plato-struct-surface.md` and guarded by
`IntrinsicsApiSnapshotTests`. Every bodiless signature in
`stdlib/foundation/intrinsics.library.plato` must have a counterpart here, or
`IntrinsicObligationTests` fails.

Importers, all via `<Import ... Label="Shared" />` of the `.projitems`:

- `generated/Plato.Generated.Foundation.Unoptimized`
- `tests/optimizer-smoke/smoke.props` (all four smoke variants)
- `experiments/csg/Ara3D.Csg.Tests`, `experiments/earcut/Ara3D.Earcut.Tests`

## The generated library (compiler output — `Triangle3D`, `Bounds3D`, curves, meshes, …)

| Path | Recipe |
|---|---|
| `generated/Plato.Generated.Foundation.Unoptimized` | `--csharp-style=extensions` over `stdlib/foundation` — wrapper scalars, optimizers off |

It is not a golden. The byte-identity diff-gate and `regen-generated.ps1` were retired 2026-07-30.

`Plato.Generated.Unoptimized` and `Plato.Generated.Optimized` were **retired 2026-08-01**. Both
were emitted from `legacy/stdlib-legacy` with the scalar-erasure recipe, which no longer matches
the runtime; erasure itself is on the way out
([`../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md`](../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md)).
Their sources, project files and solution entries are gone, along with
`tests/optimizer-smoke/Bench`, which existed only to benchmark one against the other. Recovering
them means a fresh recipe, not a revert. Detail in
[`../generated/README.md`](../generated/README.md).

## What still lives in ara3d-sdk

`ara3d-sdk/src/Plato.Generated` and `ara3d-sdk/src/Plato.Intrinsics` are copies of the **old V1
shape**, and `ara3d-sdk/src/Ara3D.Geometry` — the handwritten library Ara3D.Studio actually ships —
references them. They were not touched when V1 was deleted here: they belong to that repo, and
retiring them is an adoption project on that side.

So "Studio ships V1" is still true today. What changed is that this repo no longer carries a second
copy of it, no longer guards it with a checksum tripwire, and no longer names anything "V2".

## Gates

The "run from" column says which checkout the command resolves its paths against. The studio copy
of `check-stdlib-fast.ps1` is stale (it points into `studio/submodules/Plato`); use the Plato-local
one.

| Gate | Command | Run from | Protects |
|---|---|---|---|
| Forward stdlib (inner loop) | `tools/check-stdlib-fast.ps1` | plato | lint clean + checker diagnostic ratchet + `types-and-concepts.txt` freshness |
| Forward stdlib (warm) | MCP `plato_check` | server | same gates, cached in the running navigation server; see the `plato-mcp` skill |
| Foundation codegen | `tools/regen-foundation.ps1` | plato | the foundation tier still emits C# that compiles (`-WhatIf` previews, `-Test` runs the generated tests) |
| Forward conformance | `tools/regen-forward-conformance.ps1` | studio | stdlib tiers + law packet type-check |
| Compiler unit tests | `dotnet test tests/PlatoTests` | plato | checker/optimizer behavior, intrinsic obligations |
| Studio battery | `tools/check-all.ps1` | studio | frozen-V1 tripwire + both `lint --strict` passes + SDK build + GeometryTests. **Not a superset** — it runs none of the rows above. |

`tools/check-frozen-v1.ps1` (studio) is **not** retired. Its third root — this repo's V1 runtime
copy — went away when V1 was deleted, but the two it still guards are the `ara3d-sdk` copies
Ara3D.Studio builds against, so the gate passes and remains meaningful. Nothing in *this* repo is
frozen; the `check-all.ps1` row protects the other checkout.

`check-all.ps1` does not run the Plato-repo gates. It runs the frozen-V1 tripwire, both `lint
--strict` passes, and the SDK build + GeometryTests. `check-stdlib-fast.ps1`, `dotnet test
tests/PlatoTests` and `regen-forward-conformance.ps1` are separate commands you run yourself.
