# Plato library map — V1 vs V2, runtimes, and goldens (2026-07-12)

> The one-page answer to "what are all these Plato.* things and which one is real?" Read this before
> touching any generated library or intrinsics runtime. Background: the repo is mid-migration from an
> old library shape (**V1**) to a new one (**V2**); this doc says exactly what each artifact is,
> which is frozen, and who consumes it.

## TL;DR

- **V1** = the old shape: wrapper-scalar structs (`Number` wraps a `float`), members as *properties*,
  default C# style. **This is what Ara3D.Studio ships today.** It is now **FROZEN** — never
  regenerated, never edited; a checksum tripwire guards it.
- **V2** = the target shape: native scalars (`float`), *extension methods*, no properties,
  `System.Numerics`-backed (SIMD-capable). **This is the live library** you develop against.
- A **golden** is a checked-in, byte-for-byte snapshot of generated code, kept so a regen can be
  *diffed* against it — any difference is either an intended change (refresh the golden) or a bug.

## The runtime (handwritten C# primitives — `Vector3`, `Number`, `Angle`, `Matrix4x4`, …)

| Path | Which | Status |
|---|---|---|
| `submodules/Plato/Plato.Intrinsics` | V1 (wrapper scalars, properties) | **FROZEN** |
| `ara3d-sdk/src/Plato.Intrinsics` | synced copy of V1 (so the SDK builds standalone) | **FROZEN** |
| `submodules/Plato/Plato.Intrinsics.V2` | V2 (native scalars, extensions, System.Numerics) | **LIVE** |

The source of truth for the runtime is `Plato.Intrinsics.V2`. Its C#-consumer surface (operators,
constructors, conversions, field-properties, static factories, BCL obligations) is contracted in
`docs/plato-struct-surface.md` and guarded by `IntrinsicsApiSnapshotTests`.

## The generated library (compiler output — `Triangle3D`, `Bounds3D`, curves, meshes, …)

| Path | Recipe | Status |
|---|---|---|
| `ara3d-sdk/src/Plato.Generated` | V1 default style, wrapper scalars, properties | **FROZEN — Studio ships this** |
| `submodules/Plato/Generated/Plato.Generated.Unoptimized` | V2 (`--csharp-style=extensions --scalar=float --no-properties`) | LIVE (readable reference) |
| `submodules/Plato/Generated/Plato.Generated.Optimized` | V2 + full optimizer (`--optimize --optimize-arrays --inline --methods --loops`) | LIVE (the adoption target) |

Both V2 goldens are diff-gated by `tools/regen-generated.ps1` (`-Apply` to refresh).

## Who consumes what

- **`ara3d-sdk/src/Ara3D.Geometry`** — the handwritten C# geometry library Studio actually uses. It
  references the **frozen V1** `Plato.Generated` + `Plato.Intrinsics`. Do not break this.
- **Nothing production yet consumes the V2 output.** Migrating `Ara3D.Geometry` onto V2 and then
  deleting V1 is the endgame (an adoption project, not yet started).
- **`submodules/Plato.Geometry`** — a separate, dormant legacy monorepo (Assimp/GLTF/Ifc/Revit/
  Speckle/Unity/PathTracer/…), NOT referenced by `Ara3D.Studio.sln`. Not part of the active build.

## Gates (run from the studio repo root)

| Gate | Command | Protects |
|---|---|---|
| Frozen V1 tripwire | `tools/check-frozen-v1.ps1` | V1 generated + both intrinsics copies never change |
| V2 golden diff | `tools/regen-generated.ps1` | the two V2 goldens match the emitter |
| Conformance | `tools/regen-conformance.ps1 -Test` | V2 recipe + runtime is semantically correct (204/204) + API snapshot |
| Compiler unit tests | `dotnet test submodules/Plato/PlatoTests` | checker/optimizer behavior |
| Everything | `tools/check-all.ps1` | all of the above + lint + SDK build + GeometryTests |

## Consolidation status (why it looks the way it does)

Per `docs/plato-consolidation-plan-2026-07-12.md`: V1 was frozen (C0); the five recipe-specific
conformance suites were collapsed to the one above (C2); the V2 runtime is being ported to
all-extension-methods (C3/M5, `Angle` done, others pending); the legacy emitter path deletion is C4
(in progress). The end state drops "V2" from every name and deletes V1 once Studio adopts V2.
