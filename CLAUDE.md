# Plato repo — agent guide

**Start here for the language and multi-target codegen:** [`docs/plato-for-agents.md`](docs/plato-for-agents.md).
**Confused by V1/V2/Plato.Generated/Intrinsics?** [`docs/plato-library-map.md`](docs/plato-library-map.md) maps every artifact, which is frozen, and who consumes it.

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
- `Plato.CLI/` — entry point. `Program.cs` args: `[input] [output] [--typescript|--rust] [--csharp-style=extensions] [--optimize] [--optimize-arrays] [--inline] [--scalar=...] [--methods] [--no-properties] [--loops]` and `lint <folder> [--strict]`. Exits 1 on parse/compile failure (fixed 2026-07-10). The legacy default C# style and `--no-tir` were retired at C4 (the TIR is the sole body writer).
- `PlatoCompiler/` — compilation + `Analysis/Linter.cs` (LINT001–005) + `Checking/` (the type checker + Typed IR: Normalize → Constrain → Solve → Elaborate → Monomorphize; handoff doc `docs/type-checker-handoff.md`).
- `Plato.AST/` — the old associativity bug was FIXED in `392dfa8` (2026-07-09); `../../docs/plato-assoc-bug-diagnosis.md` is historical.
- `Plato.CSharpWriter/` — `CSharpWriter.cs` (flags: `ExtensionStyle`, `Optimize`, `ScalarErase`, `NoProperties`), `TirCSharpBodyWriter.cs` (the SOLE C# body writer — every function body renders from the monomorphized Typed IR; the legacy `CSharpFunctionBodyWriter` was deleted at C4), `ExtensionStyleWriter.cs` (classic extension methods, one static class per Plato library; moved no-arg fns are METHODS `v.Magnitude()`), `TirScalarLowerer.cs` (`--scalar=float` erasure as a TIR lowering pass — it replaced the emit-time `ScalarEraseAnalysis`, deleted at S3), `ComponentUnroller.cs` (`--optimize` field-wise unrolling table).
- `Plato.Intrinsics/` — **FROZEN V1 runtime** (consolidation plan C0). The live runtime is `Plato.Intrinsics.V2/` (System.Numerics-backed, method-form). Both `Plato.Intrinsics` and the ara3d-sdk `Plato.Generated`/`Plato.Intrinsics` copies are frozen — protected by `tools\check-frozen-v1.ps1` (manifest `tools\frozen-v1.sha256`), never edit/regenerate.
- `conformance/Ara3D.SDK.ConformanceTests/` — **THE** Plato conformance suite (consolidation plan C2 retired the V1/V2/Opt/Scalar recipe suites; their shared sources moved here so it is self-contained). Runs the shipping V2 recipe against `Plato.Intrinsics.V2`. `Generated/` is script-produced, gitignored. Expected: **204 pass / 0 fail** (manifest: `KnownFailures.json`; known+passing = must fail with "remove from manifest"). Regen: `tools\regen-conformance.ps1 -Test`.
- `Generated/` — buildable generated projects (extension-style, scalar-erased), each a real csproj in `Ara3D.Studio.sln`: `Plato.Generated.Unoptimized` (optimizers off, readable reference) and `Plato.Generated.Optimized` (full optimizer pipeline, adoption shape). Diff-gated by `tools\regen-generated.ps1`; docs in `Generated/README.md`. Supersedes the retired `golden/Plato.Generated.V2`.
- `parakeet/` — sub-submodule, PRE-EXISTING DIRTY STATE. Never touch, never stage.

## Commands (run from `C:\Users\cdigg\git\studio`)
- `.\tools\check-frozen-v1.ps1` — freeze tripwire: SHA-256 of the frozen V1 artifacts (ara3d-sdk `Plato.Generated`/`Plato.Intrinsics` + Plato-repo `Plato.Intrinsics`). Exit 1 on any drift. `-Update` re-baselines (deliberate only). Replaced regen-plato in check-all (C0); `regen-plato.ps1` + the legacy default-style emitter were deleted at C4.
- `.\tools\regen-conformance.ps1 -Test` — regenerate merged (plato-src + plato-test-src) output into the one conformance suite and run it (204/204).
- `.\tools\check-all.ps1` — full gate battery, PASS/FAIL table. **Run once at the end of a mission**; iterate on a single relevant gate during development.
- `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\plato-src` — exit 0 unless `--strict`; the finding count drifts with library content, so compare against the previous run, not a hardcoded baseline.

## Hard rules
1. No git commits unless the mission says so; never stage parakeet or pre-existing dirty files.
2. The FROZEN V1 artifacts (ara3d-sdk Plato.Generated/Plato.Intrinsics + Plato-repo Plato.Intrinsics) must not change — `tools\check-frozen-v1.ps1` is the gate. The live V2 goldens (`Generated/`) are diff-gated by `regen-generated.ps1`; refresh them in the same change as any intended emitter-behavior change.
3. Generated code must compile with DEFAULT LangVersion on net8.0. No C# 14 features.
4. Known bugs are now BEING fixed (content-leads, from 2026-07-09). The `KnownFailures.json`
   manifest is the burn-down queue: when you fix a bug, REMOVE its manifest entry in the same change
   (a passing still-listed entry fails the runner with "remove from manifest"). Off-flag byte-identity
   (rule 2) still holds for source you did NOT change — it protects against unintended emitter drift.
5. The conformance law runner reflects instance members; `Law_*` functions stay in structs.

## Mission protocol
- Maintain `PROGRESS.md` in your workspace (10 lines max, updated as you go) so a crashed session resumes cheaply.
- On completion: update the relevant DONE note in `../../docs/plato-roadmap.md`, write `COMMIT_MSG.txt` (draft commit message) in the repo you changed, and keep the final report under ~300 words using: files touched / gates table / surprises / rerun commands.
