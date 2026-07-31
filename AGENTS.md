# Plato repo — agent guide

This is the canonical guide for agents and developers working in `submodules/Plato`.
`CLAUDE.md` points here; there is one copy of these rules.

**Start here for the language and multi-target codegen:** [`docs/plato-for-agents.md`](docs/plato-for-agents.md).
**Confused by V1/V2/Plato.Generated/Intrinsics?** [`docs/plato-library-map.md`](docs/plato-library-map.md) maps every artifact, which is frozen, and who consumes it.
**Process / monorepo coupling:** studio's [`docs/working-on-plato.md`](https://github.com/ara3d/studio/blob/main/docs/working-on-plato.md)
(gate scripts still run from the studio checkout). **Docs and work tracking live in this repo.**

Plato: pure language for geometry libraries, compiled to C# (TS/Rust/GLSL/C++/CUDA writers exist as POCs).
Also checked out as `submodules/Plato` inside the studio monorepo.

**Ara3D SDK is consumed as a NuGet package.** Every project that needs
`Ara3D.Collections` / `Ara3D.Logging` / `Ara3D.Memory` / `Ara3D.Utils` carries a single
`<PackageReference Include="Ara3D.SDK.Core" Version="$(Ara3DVersion)" />`; the version lives in
`Directory.Build.props` at the repo root. Do not add project references into a sibling
`ara3d-sdk` checkout, and do not switch to the `Ara3D.SDK` meta-package — that one is
`net8.0-windows` and pulls WPF, IFC and `Studio.API` into the compiler.

**Work tracking** lives here under [`tracker/`](tracker/) (`python tools/track.py list --open`).
**Durable docs** live under [`docs/`](docs/). Gate scripts still live in the studio repo
(`C:\Users\cdigg\git\studio\tools\`) until those move too.

Plan + status: [`docs/plato-execution-plan-2026-07-09.md`](docs/plato-execution-plan-2026-07-09.md).
Bug catalog: [`docs/plato-library-review.md`](docs/plato-library-review.md).
(`docs/plato-roadmap.md` was superseded and archived 2026-07-16 — it is now
[`docs/archive/plato-roadmap.md`](docs/archive/plato-roadmap.md), historical only.)

**C# style:** for handwritten compiler C# (`src/Plato.Compiler/`, `writers/Plato.CSharpWriter/`, etc.) follow the
`csharp-style` skill — full reference in the studio repo `docs/csharp-style-guide-for-agents.md`.
Does NOT govern Plato-language `.plato` source, nor the C# the writers emit (that shape is set by
the writer code).

## Layout (what matters)

**Stdlib mapping:** `stdlib` = forward vocabulary (the next-generation library);
`stdlib-legacy` = shipping generation (drives `Plato.Generated` / Studio). Do not confuse the two.

- `stdlib/` — **Forward stdlib vocabulary** (ex-`plato-src-v3`). New *vocabulary* goes here.
  Since the plato-293 re-partition it carries **both** declarations and implementation bodies:
  `<stem>.plato` = types, `<stem>.concepts.plato` = concepts, `<stem>.library.plato` = exactly one
  `library` block. One kind of declaration per file, at most twelve top-level declarations per
  file. The folder is partitioned into four subfolders — `foundation/`, `geometry/`, `graphics/`
  and `future/` (aspirational vocabulary) — each of which is itself flat.
  **Read before editing:** [`stdlib/README.md`](stdlib/README.md) (what the folder is, counts,
  partition rules), [`stdlib/CONVENTIONS.md`](stdlib/CONVENTIONS.md) (domain semantics — frames,
  winding, units, the no-generic-`Optional<T>` rule; when two files disagree, this one wins),
  [`stdlib/STYLE_GUIDE.md`](stdlib/STYLE_GUIDE.md) (authoring style for bodies, comments,
  literals, formulas), [`stdlib/LIBRARIES.md`](stdlib/LIBRARIES.md) (how library files relate to
  declaration files).
  Inner loop: `.\tools\check-stdlib-fast.ps1` from the studio root.
- `tests/stdlib-tests/` — forward law packet (`Law_*` functions) for `stdlib/`. Merged with `stdlib` by
  `tools\regen-forward-conformance.ps1`. Keep separate from `stdlib`, same as the legacy pair.
- `legacy/stdlib-legacy/` — **Shipping stdlib** (ex-`plato-src`). **WRITABLE as of 2026-07-09** (content-leads
  refactor; the old Phase-4 freeze is retired). Edit freely for runtime/body fixes; gate =
  `lint --strict` + `check-all.ps1` green (the golden-refresh step retired 2026-07-30). Plan: [`docs/plato-execution-plan-2026-07-09.md`](docs/plato-execution-plan-2026-07-09.md).
- `legacy/stdlib-snapshot-2026-07-09/` — **FROZEN 2026-07-09 snapshot** of the pre-refactor library (ex-`plato-src-legacy`).
  Reference only; never edit, never compile. Diff `stdlib-legacy` against it to see how far the library has moved.
- `legacy/stdlib-legacy-tests/` — law/witness libraries (`Law_*`, `Witness_*` Boolean functions). Never merge into stdlib-legacy.
- `src/Plato.CLI/` — entry point. `Program.cs` args: `[input] [output] [--typescript|--rust|--glsl|--cpp|--cuda] [--csharp-style=extensions] [--optimize] [--optimize-arrays] [--inline] [--scalar=...] [--methods] [--no-properties] [--loops]` and `lint <folder> [--strict]`. Exits 1 on parse/compile failure (fixed 2026-07-10). The legacy default C# style and `--no-tir` were retired at C4 (the TIR is the sole body writer). `--inline` is wired for the C# writer today; GLSL/C++/CUDA skip lambdas until that lowering is shared.
- `src/Plato.Compiler/` — compilation + `Analysis/Linter.cs` (LINT001–005) + `Checking/` (the type checker + Typed IR: Normalize → Constrain → Solve → Elaborate → Monomorphize; handoff doc `docs/type-checker-handoff.md`).
- `src/Plato.AST/` — the old associativity bug was FIXED in `392dfa8` (2026-07-09); [`docs/archive/plato-assoc-bug-diagnosis.md`](docs/archive/plato-assoc-bug-diagnosis.md) is historical.
- `writers/Plato.CSharpWriter/` — `CSharpWriter.cs` (flags: `ExtensionStyle`, `Optimize`, `ScalarErase`, `NoProperties`), `TirCSharpBodyWriter.cs` (the SOLE C# body writer — every function body renders from the monomorphized Typed IR; the legacy `CSharpFunctionBodyWriter` was deleted at C4), `ExtensionStyleWriter.cs` (classic extension methods, one static class per Plato library; moved no-arg fns are METHODS `v.Magnitude()`), `TirScalarLowerer.cs` (`--scalar=float` erasure as a TIR lowering pass — it replaced the emit-time `ScalarEraseAnalysis`, deleted at S3), `ComponentUnroller.cs` (`--optimize` field-wise unrolling table).
- `writers/Plato.GlslWriter/` / `writers/Plato.CppWriter/` — TIR-only POC backends (GLSL ES 3.00; C++17 / CUDA with shared bodies + dialect preamble). Compile-gated by their `*.Tests` projects; not in `Ara3D.Studio.sln`. See each project's `README.md`.
- `legacy/Plato.Intrinsics.Legacy/` — **FROZEN V1 runtime** (consolidation plan C0). The live runtime is `src/Plato.Intrinsics.V2/` (System.Numerics-backed, method-form). Both `Plato.Intrinsics.Legacy` and the ara3d-sdk `Plato.Generated`/`Plato.Intrinsics` copies are frozen — protected by `tools\check-frozen-v1.ps1` (manifest `tools\frozen-v1.sha256`), never edit/regenerate.
- ~~`tests/conformance/Ara3D.SDK.ConformanceTests/`~~ — **RETIRED 2026-07-30** together with the golden
  diff-gate (`tracker/decisions/2026-07-30-retire-legacy-conformance-and-goldens.md`). The forward
  suite below is the sole conformance target; making it run is `plato-308`. Until then, executable
  coverage = PlatoTests + GeometryTests + the frozen-V1 tripwire.
- `tests/conformance/Plato.ForwardConformanceTests/` — forward-stdlib harness driven by
  `tools\regen-forward-conformance.ps1`. Stage 1 (type-check merged `stdlib` + `stdlib-tests`) is
  the gating stage and passes; Stage 2 (codegen + law runner) generates but does not compile —
  tracked as `plato-308`, detail in that folder's `README.md`. A red Stage 2 is not your fault
  unless your error count exceeds the number in the issue.
- `generated/` — buildable generated projects (extension-style, scalar-erased): `Plato.Generated.Unoptimized` (optimizers off, readable reference) and `Plato.Generated.Optimized` (full optimizer pipeline, adoption shape). **No longer goldens** (2026-07-30 retirement): the byte-identity diff-gate and its `regen-generated.ps1` script are gone; these are ordinary cached output anyone may regenerate, and staleness is acceptable. Docs in `generated/README.md`.
- `src/Plato.Navigation/` (+ `src/Plato.Navigation.CLI/`, `tests/Plato.Navigation.Tests/`) — navigation index over a source snapshot: go-to-def,
  find-refs, outline, name search, JSON export, and an `IncrementalIndexer` with a per-file parse
  cache. Reuses the parser and binder; adds no second resolver. Its README lists the known
  imprecisions. Consumer: `labs/PlatoNavigationMcp` in the studio repo — **that server is launched
  by hand**; its README has the command, the repeatable `--root` flag, and the default port 8768.
- `parakeet/` — submodule (`github.com/ara3d/parakeet`), the parser generator this compiler is
  built on. A plain clone leaves it empty; `git submodule update --init --recursive` fills it.
  Its contents belong to that repo: grammar changes are committed and pushed there, and a
  commit here only records which parakeet commit to use. Never stage files inside it from
  this repo.

## Commands (run from `C:\Users\cdigg\git\studio`)

Iterate on the one gate relevant to your workstream; run `check-all.ps1` **once**, at the end.

- `.\tools\check-stdlib-fast.ps1` — the forward-stdlib inner loop (seconds). Two gates:
  `lint --strict` over `stdlib` (0 parse / 0 resolution errors) and the **checker ratchet**
  (`ForwardStdLibDiagnosticCountDoesNotRegress` in `tests/PlatoTests/ForwardStdLibCheckerTests.cs`) —
  your change may not raise the diagnostic count, and when you lower it you lower the ceiling in
  the same commit.
- `.\tools\regen-forward-conformance.ps1` — forward-stdlib milestone gate. Stage 1 gating (see
  `tests/conformance/Plato.ForwardConformanceTests/` above); `-Codegen` / `-Test` run the diagnostic stages.
- `.\tools\check-frozen-v1.ps1` — freeze tripwire: SHA-256 of the frozen V1 artifacts (ara3d-sdk `Plato.Generated`/`Plato.Intrinsics` + Plato-repo `Plato.Intrinsics.Legacy`). Exit 1 on any drift. `-Update` re-baselines (deliberate only). Replaced regen-plato in check-all (C0); `regen-plato.ps1` + the legacy default-style emitter were deleted at C4.
- `.\tools\check-all.ps1` — full gate battery, PASS/FAIL table. **Run once at the end of a mission**; iterate on a single relevant gate during development.
- `.\tools\gate-timings.ps1` — how long the gates take. Every gate script records its duration
  (and failures) via `tools\gate-timing.ps1` into `%LOCALAPPDATA%\ara3d\gate-timings.csv`; this
  reports runs / median / P90 / max / total per gate, sorted by total time. `-Days`, `-Gate`,
  `-Tail`, `-Failures`. Include the seconds in your gate table when you report results.
- `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib-legacy` — exit 0 unless `--strict`; the finding count drifts with library content, so compare against the previous run, not a hardcoded baseline.

Every gate is PowerShell and Windows-pathed. An agent on Linux/CI is limited to `dotnet build`
and `dotnet test`.

**Status report (committed HTML):** from the Plato repo root,
`python tools/gen-status-report.py` refreshes `docs/status-report.html` (live git/tracker;
gate/lint/C# build rows from `docs/status-report-snapshot.json`). Install the commit hook with
`powershell tools/install-githooks.ps1` so every commit regenerates and stages the HTML.
**C# builds:** use `powershell tools/dotnet-build-record.ps1 -Project <csproj> -TargetName <name>`
(or the studio gates that already call it) so error totals by category land in the snapshot
after every build — do not bare-`dotnet build` Plato projects when you care about the report.

## Hard rules

1. **Commits.** Commit your own work at clear milestones, on the current branch — never create a
   branch unless asked. This working tree is SHARED with concurrent sessions, so always
   `git status` first and commit by explicit pathspec: `git commit -- <file> <file>`. Never
   `git add -A`, `git add .`, or a bare `git commit` — that sweeps someone else's staged work into
   your commit. Never stage `parakeet/` or pre-existing dirty files you did not touch. Committing
   here does not update the parent: bump the submodule pointer in studio as its own commit
   (`git commit -- submodules/Plato`), and push both remotes.
   **Status report:** every Plato commit must refresh `docs/status-report.html`. Install the
   hook once with `powershell tools/install-githooks.ps1` (sets `core.hooksPath` to
   `tools/githooks`); the `pre-commit` hook runs `python tools/gen-status-report.py` and stages
   the HTML. If hooks are not installed, run the generator yourself and include
   `docs/status-report.html` (and `docs/status-report-snapshot.json` when you change gate/lint
   snapshot data) in the commit pathspec.
2. The FROZEN V1 artifacts (ara3d-sdk Plato.Generated/Plato.Intrinsics + Plato-repo Plato.Intrinsics.Legacy) must not change — `tools\check-frozen-v1.ps1` is the gate. The live V2 goldens (`generated/`) are diff-gated by `regen-generated.ps1`; refresh them in the same change as any intended emitter-behavior change.
3. Generated code must compile with DEFAULT LangVersion on net8.0. No C# 14 features.
4. Known bugs are now BEING fixed (content-leads, from 2026-07-09). The `KnownFailures.json`
   manifest is the burn-down queue: when you fix a bug, REMOVE its manifest entry in the same change
   (a passing still-listed entry fails the runner with "remove from manifest"). Off-flag byte-identity
   (rule 2) still holds for source you did NOT change — it protects against unintended emitter drift.
5. The conformance law runner reflects instance members; `Law_*` functions stay in structs.

## Language facts that are easy to get wrong

The normative reference is [`docs/plato-language-semantics.md`](docs/plato-language-semantics.md).
The ones agents most often rediscover the hard way:

- **`TupleN` exists up to 10 fields.** Tuple expressions resolve as a `TupleN` call, so a
  compilation without `primitives-tuples.plato` rejects them outright
  (`docs/plato-language-semantics.md`, "Tuples construct types structurally").
- **Sum types are non-generic.** A generic sum is rejected with `CHK306`; `match` lowers to
  conditionals with no new TIR node. Design doc: `docs/plato-sum-types-design-2026-07-27.md`.
  Consequence for the stdlib: no generic `Optional<T>` / `Maybe<T>` — see `stdlib/CONVENTIONS.md`
  for the three sanctioned partial-operation styles.
- **The shipping recipe is property-free.** `--no-properties` plus the V2 runtime makes the
  `Generated` libraries method-form; see `docs/plato-library-map.md` for the recipe per artifact.
- **Intrinsics may mention only `primitive` types** (2026-07-30). A bodiless signature is legal
  only inside `stdlib/foundation/intrinsics.library.plato` and only over the set declared with
  the `primitive` keyword in `stdlib/foundation/primitives.plato`. Operations on `Angle`,
  `Number2/3/4/8`, `Vector2D/3D`, the matrices and `Quaternion` are **reference bodies** in
  `*-ops.library.plato` — write ordinary Plato there, do not add a bodiless declaration. Every
  intrinsic you do add must have a `src/Plato.Intrinsics.V2` counterpart or `IntrinsicObligationTests`
  fails. Full contract: `docs/plato-intrinsics-surface.md`.

## Mission protocol

- Maintain `PROGRESS.md` in your workspace (10 lines max, updated as you go) so a crashed session resumes cheaply.
- On completion: close your tracker issue (`python tools/track.py close <id> --outcome "..."`)
  and record any lasting decision in the relevant `docs/` plan, and keep the final report
  under ~300 words using: files touched / gates table / surprises / rerun commands.
- `PROGRESS.md` and `COMMIT_MSG.txt` at the repo root are single-slot scratch files. Sessions run
  concurrently here — if one already holds another mission's notes, leave it alone and put yours in
  your own commit message rather than overwriting someone's work in progress.

<!-- workquarry:begin -->
## Work tracking — `tracker/` (WorkQuarry)

All trackable work (features, bugs, debt, ideas, open design problems, retire candidates) lives in [`tracker/`](tracker/) — one file per issue, indexed in [`tracker/BACKLOG.md`](tracker/BACKLOG.md), completed work logged in [`tracker/DONE.md`](tracker/DONE.md), decisions in [`tracker/decisions/`](tracker/decisions/). Full process: [`tracker/readme.md`](tracker/readme.md).

Non-negotiable agent rules:

1. **Never execute a plan doc without checking its status.** A plan without a linked `ready`/`in-progress` issue in the tracker, or carrying an `EXECUTED` banner, is historical — ask before acting on it.
2. **File what you find.** Out-of-scope debt, bugs, or retire candidates discovered mid-task: file via `/track-issue` (capture-only short form) or `python tools/track.py new`. Do not fix inline, silently drop, or hand-edit BACKLOG.md.
3. **Name the item in the commit.** Any commit doing work on a tracked item names its id in the Conventional-Commits scope — `fix(studio-149): atomic menu clear+rebuild`. This is the join key between git history and tracker state; without it, landed work is invisible to the tracker. Tracker bookkeeping commits (`docs(tracker):`, `chore(tracker):`) are not work commits and do not count as progress on an item.
4. **Mark progress at the commit that makes it.** In the same commit that lands work, tick every `## Done means` checkbox that commit satisfies. Progress is read from those boxes — never store a percentage. Then:
   - **All boxes ticked** → close it now: `python tools/track.py close <id> --outcome "done (<work-commit-sha>)"` and commit that tracker change (see rule 5). The sha is unknown until the work commit exists, so closing is the immediately-following commit, not a deferred chore.
   - **Any box unticked** → the item stays `in-progress`. Say which box is outstanding. A landed fix awaiting verification is not done.

   Never close an item from commit history alone — an unticked box is the item telling you it is not finished. Status/priority/sprint changes go through `track.py set`; BACKLOG.md is generated, never hand-edited, and the executed plan doc gets bannered + archived at close.
5. **Commit tracker changes immediately.** Any time you create or update an issue, `git add` the issue file(s) **and** `BACKLOG.md` together and commit. Never leave them uncommitted or commit one without the other.
6. **Capture user ideas** ("we should someday…") as `type: idea` issues immediately.
7. **Check `tracker/decisions/` before proposing architecture changes**; disagree via a superseding ADR, not silent divergence.
<!-- workquarry:end -->
