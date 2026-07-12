# optimizer-smoke — compilation smoke tests for the emitter recipes

Real `.csproj`-backed libraries (not the `.shproj` the golden ships as) so `dotnet build`
actually **compiles** the generated C#. Used while developing the optimizer TIR passes.

## The 2×2 matrix

Two axes; the adoption **shape** (`--csharp-style=extensions --scalar=float --methods` —
native primitives, extension functions, no properties) is constant across all four:

| project | subset | optimizer passes |
|---|---|---|
| `Smoke.Full.NonOpt` | all of plato-src | none |
| `Smoke.Full.Opt`    | all of plato-src | `--inline --optimize --optimize-arrays --loops` |
| `Smoke.Min.NonOpt`  | `manifest-min.txt` | none |
| `Smoke.Min.Opt`     | `manifest-min.txt` | `--inline --optimize --optimize-arrays --loops` |

Each variant's C# is generated into `Smoke.<V>/Generated/` (git-ignored, regenerated).

## Usage

```powershell
.\regen-smoke.ps1                    # regenerate all four
.\regen-smoke.ps1 -Which min -Build  # regenerate + compile just the minimal variants
.\regen-smoke.ps1 -Build             # regenerate + compile all four (the smoke test)
.\regen-smoke.ps1 -Build -DumpTir    # also write per-phase TIR to Smoke.<V>/tir-dump/
```

`-Build` prints a PASS/FAIL table and exits nonzero on any compile failure.

## The minimal subset (`manifest-min.txt`)

**It is only ~14% smaller than full** (21 files → 158 generated `.g.cs`, vs 184 for full),
and that is the *objective floor*: found by greedily removing files from the full set while it
still compiles. plato-src is **not cleanly layered** — `core.library.Sample` forward-refers to
`LinearSpace` (interval/curves), `interval` refers to `Circle`, `intrinsics` refers to `Plane`,
and `geometry.types`/`geometry.interfaces` pull in `curves` + `transforms`. Only the pure leaf
libraries drop out (solids, sdf3d, colors, fields, measures, meshes.library).

Consequences:
- The minimal subset exercises `--inline`, `--optimize` and `--loops` but **not**
  `--optimize-arrays` (that needs the Map-into-struct-field pattern from `meshes.library`;
  use a Full variant for it).
- A *genuinely* tiny fixture would need either a synthetic (non-subset) `.plato` that avoids
  the geometry concept lattice, or a re-layering of plato-src. Not done here.

## `--dump-tir=<dir>`

Writes the per-phase TIR of every emitted body — the elaborated/monomorphized **input**, then
each optimizer pass (inline → component-unroll → array-materialize → loop-lower) that **changed**
the tree — one `<TypeName>.tir.txt` file per owner type. A development aid; no effect on the
emitted C#. See `../docs/plato-emitter-phases.md` for the full layer map.
