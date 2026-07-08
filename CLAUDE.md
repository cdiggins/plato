# Plato repo — agent guide

**Start here for the language and multi-target codegen:** [`docs/plato-for-agents.md`](docs/plato-for-agents.md).

Plato: pure language for geometry libraries, compiled to C# (TS/Rust writers exist, out of scope).
Part of the studio monorepo at `C:\Users\cdigg\git\studio` (this repo = `submodules/Plato`).
Roadmap + status: `../../docs/plato-roadmap.md`. Bug catalog: `../../docs/plato-library-review.md`.

## Layout (what matters)
- `plato-src/` — production stdlib source. **FROZEN until the Phase 4 bug wave** (additive new files OK).
- `plato-test-src/` — law/witness libraries (`Law_*`, `Witness_*` Boolean functions). Never merge into plato-src.
- `Plato.CLI/` — entry point. `Program.cs` args: `[input] [output] [--typescript|--rust] [--csharp-style=default|extensions] [--optimize] [--scalar=...]` and `lint <folder> [--strict]`. **CAVEAT: exits 0 even on compile errors** — always sanity-check output file count / build the result.
- `PlatoCompiler/` — compilation + `Analysis/Linter.cs` (LINT001–005).
- `Plato.AST/` — `AstNodeFactory.cs` has the KNOWN associativity bug (strict `<` at ~line 24; prefix-op ordering ~118–122). Diagnosed in `../../docs/plato-assoc-bug-diagnosis.md`. DO NOT fix until Phase 4.
- `Plato.CSharpWriter/` — `CSharpWriter.cs` (flags: `ExtensionStyle`, `Optimize`; global no-arg name partition in `BuildExtensionPlans()`), `ExtensionStyleWriter.cs` (classic extension methods, one static class per Plato library; moved no-arg fns are METHODS `v.Magnitude()`, kept struct members are properties), `ComponentUnroller.cs` (`--optimize` field-wise unrolling — must preserve known bugs bit-for-bit), `CSharpFunctionBodyWriter.cs` (call-site `()` injection via `MovedNoArgNames`).
- `Plato.Intrinsics/` — **SOURCE OF TRUTH** for the handwritten C# runtime (Number/Vector2-8/Matrix/Angle wrappers). ara3d-sdk holds a byte-identical synced copy, diff-gated — never let them diverge; never edit the ara3d-sdk copy.
- `conformance/Ara3D.SDK.ConformanceTests{,.V2,.Opt}/` — NUnit suites; `Generated/` folders are script-produced, gitignored. Expected result everywhere: **142 pass / 36 ignored-known / 0 fail** (manifest: `KnownFailures.json`; known+passing = must fail with "remove from manifest").
- `golden/Plato.Generated.V2/` — golden extension-style production output (the intended adoption shape).
- `parakeet/` — sub-submodule, PRE-EXISTING DIRTY STATE. Never touch, never stage.

## Commands (run from `C:\Users\cdigg\git\studio`)
- `.\tools\regen-plato.ps1` — default-mode byte-identity + intrinsics-sync diff vs ara3d-sdk. Exit 1 on drift. `-Apply` syncs. Excludes `_Sphere.g.cs`/`_Cylinder.g.cs` (name collisions, resolved at V2 adoption).
- `.\tools\regen-conformance.ps1 -Test` / `-v2` / `-opt` variants — regenerate merged (plato-src + plato-test-src) output into the respective conformance project and run it.
- `.\tools\check-all.ps1` — full gate battery, PASS/FAIL table. **Run once at the end of a mission**; iterate on a single relevant gate during development.
- `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src` — expect 246 findings (baseline), exit 0.

## Hard rules
1. No git commits unless the mission says so; never stage parakeet or pre-existing dirty files.
2. Off-flag emitter output must stay BYTE-IDENTICAL (regen-plato.ps1 is the gate).
3. Generated code must compile with DEFAULT LangVersion on net8.0. No C# 14 features.
4. Known bugs are preserved, not fixed (Phase 4 gate). If a KnownFailures test starts passing, your change altered semantics — that's a defect in your change.
5. The conformance law runner reflects instance members; `Law_*` functions stay in structs.

## Mission protocol
- Maintain `PROGRESS.md` in your workspace (10 lines max, updated as you go) so a crashed session resumes cheaply.
- On completion: update the relevant DONE note in `../../docs/plato-roadmap.md`, write `COMMIT_MSG.txt` (draft commit message) in the repo you changed, and keep the final report under ~300 words using: files touched / gates table / surprises / rerun commands.
