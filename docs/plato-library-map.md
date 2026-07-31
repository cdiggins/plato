# Plato library map — the runtime and the generated libraries (2026-07-31)

> The one-page answer to "what are all these Plato.* things and which one is real?" Read this before
> touching the generated libraries or the intrinsics runtime.

## TL;DR

There is now exactly one shape. The old wrapper-scalar, property-bearing shape (**V1**) was deleted
from this repo on 2026-07-31, along with its freeze. What remains is native scalars (`float`),
extension methods, no properties, `System.Numerics`-backed and SIMD-capable.

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

- `generated/Plato.Generated.Unoptimized`, `generated/Plato.Generated.Optimized`
- `tests/optimizer-smoke/smoke.props` (all four smoke variants)
- `experiments/csg/Ara3D.Csg.Tests`, `experiments/earcut/Ara3D.Earcut.Tests`

## The generated library (compiler output — `Triangle3D`, `Bounds3D`, curves, meshes, …)

| Path | Recipe |
|---|---|
| `generated/Plato.Generated.Unoptimized` | `--csharp-style=extensions --scalar=float --no-properties` (readable reference) |
| `generated/Plato.Generated.Optimized` | the same plus `--optimize --optimize-arrays --inline --methods --loops` (the adoption target) |

Neither is a golden. The byte-identity diff-gate and `regen-generated.ps1` were retired 2026-07-30.

## What still lives in ara3d-sdk

`ara3d-sdk/src/Plato.Generated` and `ara3d-sdk/src/Plato.Intrinsics` are copies of the **old V1
shape**, and `ara3d-sdk/src/Ara3D.Geometry` — the handwritten library Ara3D.Studio actually ships —
references them. They were not touched when V1 was deleted here: they belong to that repo, and
retiring them is an adoption project on that side.

So "Studio ships V1" is still true today. What changed is that this repo no longer carries a second
copy of it, no longer guards it with a checksum tripwire, and no longer names anything "V2".

## Gates (run from the studio repo root)

| Gate | Command | Protects |
|---|---|---|
| Forward stdlib | `tools/check-stdlib-fast.ps1` | lint clean + checker diagnostic ratchet |
| Forward conformance | `tools/regen-forward-conformance.ps1` | stdlib + law packet type-check |
| Compiler unit tests | `dotnet test tests/PlatoTests` | checker/optimizer behavior, intrinsic obligations |
| Everything | `tools/check-all.ps1` | all of the above + lint + SDK build + GeometryTests |

`tools/check-frozen-v1.ps1` and its `frozen-v1.sha256` manifest are **retired**. They still exist in
the studio checkout and will fail there against the deleted files until someone removes them and
their `check-all.ps1` row.
