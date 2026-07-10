# Plato repo — agent guide

**Start here for the language and multi-target codegen:** [`docs/plato-for-agents.md`](docs/plato-for-agents.md).

Plato: pure language for geometry libraries, compiled to C# (TS/Rust writers exist, out of scope).
Part of the studio monorepo at `C:\Users\cdigg\git\studio` (this repo = `submodules/Plato`).
Roadmap + status: `../../docs/plato-roadmap.md`. Bug catalog: `../../docs/plato-library-review.md`.

**C# style:** for handwritten compiler C# (`PlatoCompiler/`, `Plato.CSharpWriter/`, etc.) follow the
`csharp-style` skill — full reference `../../docs/csharp-style-guide-for-agents.md`. Does NOT govern
Plato-language `.plato` source, nor the C# the writers emit (that shape is set by the writer code).

## Layout (what matters)
- `plato-src/` — production stdlib source. **WRITABLE as of 2026-07-09** (content-leads
  refactor; the old Phase-4 freeze is retired). Edit freely; each change = `regen-plato.ps1 -Apply`
  + `check-all.ps1` green + commit. Plan: `../../docs/plato-execution-plan-2026-07-09.md`.
- `plato-src-legacy/` — **FROZEN 2026-07-09 snapshot** of the pre-refactor library. Reference only;
  never edit, never compile. Diff `plato-src` against it to see how far the library has moved.
- `plato-test-src/` — law/witness libraries (`Law_*`, `Witness_*` Boolean functions). Never merge into plato-src.
- `Plato.CLI/` — entry point. `Program.cs` args: `[input] [output] [--typescript|--rust] [--csharp-style=default|extensions] [--optimize] [--scalar=...] [--no-tir]` and `lint <folder> [--strict]`. Exits 1 on parse/compile failure (fixed 2026-07-10).
- `PlatoCompiler/` — compilation + `Analysis/Linter.cs` (LINT001–005) + `Checking/` (the type checker + Typed IR: Normalize → Constrain → Solve → Elaborate → Monomorphize; handoff doc `docs/type-checker-handoff.md`).
- `Plato.AST/` — the old associativity bug was FIXED in `392dfa8` (2026-07-09); `../../docs/plato-assoc-bug-diagnosis.md` is historical.
- `Plato.CSharpWriter/` — `CSharpWriter.cs` (flags: `ExtensionStyle`, `Optimize`, `ScalarErase`, `UseTir` — **on by default**: default-style member bodies emit from the Typed IR via `TirCSharpBodyWriter`; `--no-tir` = legacy path), `ExtensionStyleWriter.cs` (classic extension methods, one static class per Plato library; moved no-arg fns are METHODS `v.Magnitude()`, kept struct members are properties), `ComponentUnroller.cs` (`--optimize` field-wise unrolling), `CSharpFunctionBodyWriter.cs` (the LEGACY body writer — still serves default-style static bodies and the extension/scalar/optimize styles).
- `Plato.Intrinsics/` — **SOURCE OF TRUTH** for the handwritten C# runtime (Number/Vector2-8/Matrix/Angle wrappers). ara3d-sdk holds a byte-identical synced copy, diff-gated — never let them diverge; never edit the ara3d-sdk copy.
- `conformance/Ara3D.SDK.ConformanceTests{,.V2,.Opt}/` — NUnit suites; `Generated/` folders are script-produced, gitignored. Expected result everywhere: **142 pass / 36 ignored-known / 0 fail** (manifest: `KnownFailures.json`; known+passing = must fail with "remove from manifest").
- `golden/Plato.Generated.V2/` — golden extension-style production output (the intended adoption shape).
- `parakeet/` — sub-submodule, PRE-EXISTING DIRTY STATE. Never touch, never stage.

## Commands (run from `C:\Users\cdigg\git\studio`)
- `.\tools\regen-plato.ps1` — default-mode byte-identity + intrinsics-sync diff vs ara3d-sdk. Exit 1 on drift. `-Apply` syncs. Excludes `_Sphere.g.cs`/`_Cylinder.g.cs` (name collisions, resolved at V2 adoption).
- `.\tools\regen-conformance.ps1 -Test` / `-v2` / `-opt` variants — regenerate merged (plato-src + plato-test-src) output into the respective conformance project and run it.
- `.\tools\check-all.ps1` — full gate battery, PASS/FAIL table. **Run once at the end of a mission**; iterate on a single relevant gate during development.
- `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src` — exit 0 unless `--strict`; the finding count drifts with library content, so compare against the previous run, not a hardcoded baseline.

## Hard rules
1. No git commits unless the mission says so; never stage parakeet or pre-existing dirty files.
2. Off-flag emitter output must stay BYTE-IDENTICAL (regen-plato.ps1 is the gate).
3. Generated code must compile with DEFAULT LangVersion on net8.0. No C# 14 features.
4. Known bugs are now BEING fixed (content-leads, from 2026-07-09). The `KnownFailures.json`
   manifest is the burn-down queue: when you fix a bug, REMOVE its manifest entry in the same change
   (a passing still-listed entry fails the runner with "remove from manifest"). Off-flag byte-identity
   (rule 2) still holds for source you did NOT change — it protects against unintended emitter drift.
5. The conformance law runner reflects instance members; `Law_*` functions stay in structs.

## Mission protocol
- Maintain `PROGRESS.md` in your workspace (10 lines max, updated as you go) so a crashed session resumes cheaply.
- On completion: update the relevant DONE note in `../../docs/plato-roadmap.md`, write `COMMIT_MSG.txt` (draft commit message) in the repo you changed, and keep the final report under ~300 words using: files touched / gates table / surprises / rerun commands.
