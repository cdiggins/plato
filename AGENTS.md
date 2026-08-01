# Plato repo — agent guide

This is the canonical guide for agents and developers working in `submodules/Plato`.
`CLAUDE.md` points here; there is one copy of these rules.

**Start here for the language and multi-target codegen:** [`docs/plato-for-agents.md`](docs/plato-for-agents.md).
**Confused by the Plato.Generated / Intrinsics artifacts?** [`docs/plato-library-map.md`](docs/plato-library-map.md) maps every artifact and who consumes it.
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
**Durable docs** live under [`docs/`](docs/), and are written to
[`docs/documentation-conventions.md`](docs/documentation-conventions.md) — chiefly: **a durable doc
states design, not measurements.** No file counts, finding counts, ratchet ceilings or test tallies
in prose; name the constant, command or log that holds the number. The repo-local `write-docs` skill
loads those rules. Gate scripts still live in the studio repo
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
  **`future/` is not linted and not converted to C#** (stdlib-377): the default tier list
  everywhere is `foundation geometry graphics`, and `future` joins only under an explicit flag
  (`-IncludeFuture` on the PowerShell gates, `--include-future` on `tools/record-gates.py`).
  It must still PARSE and TYPE-CHECK — `ForwardStdLib*` in `tests/PlatoTests` reads all four
  tiers unconditionally. Nothing in a shipping tier may reference a `future` declaration.
  **Folder rules:** [`stdlib/AGENTS.md`](stdlib/AGENTS.md) — including the standing obligation to
  regenerate `stdlib/types-and-concepts.txt` (the generated index of every shipping type and
  concept) in the same commit as any declaration change.
  **Read before editing:** [`stdlib/README.md`](stdlib/README.md) (what the folder is, counts,
  partition rules), [`stdlib/CONVENTIONS.md`](stdlib/CONVENTIONS.md) (domain semantics — frames,
  winding, units, the no-generic-`Optional<T>` rule; when two files disagree, this one wins),
  [`stdlib/STYLE_GUIDE.md`](stdlib/STYLE_GUIDE.md) (authoring style for bodies, comments,
  literals, formulas), [`stdlib/LIBRARIES.md`](stdlib/LIBRARIES.md) (how library files relate to
  declaration files).
  Inner loop: `.\tools\check-stdlib-fast.ps1` from the studio root.
  **How it is tested and validated:** [`stdlib/VERIFICATION.md`](stdlib/VERIFICATION.md)
  — the seven rungs (parse → resolve → lint → style → type-check → codegen → law execution),
  which command runs each, the three ratchets and their scopes, and the wrong-greens to avoid.
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
- `src/Plato.Intrinsics/` — **the** handwritten C# runtime (System.Numerics-backed, method-form), shared-project form, imported by `generated/` and the smoke/experiment projects. The old V1 runtime (`Plato.Intrinsics.Legacy`) and its freeze were deleted 2026-07-31; the copies still living in `ara3d-sdk` belong to that repo and are not this repo's concern.
- ~~`tests/conformance/Ara3D.SDK.ConformanceTests/`~~ — **RETIRED 2026-07-30** together with the golden
  diff-gate (`tracker/decisions/2026-07-30-retire-legacy-conformance-and-goldens.md`). The forward
  suite below is the sole conformance target; making it run is `plato-308`. Until then, executable
  coverage = PlatoTests + GeometryTests.
- `tests/conformance/Plato.ForwardConformanceTests/` — forward-stdlib harness driven by
  `tools\regen-forward-conformance.ps1`. Stage 1 (type-check merged `stdlib` + `stdlib-tests`) is
  the gating stage and passes; Stage 2 (codegen + law runner) generates but does not compile —
  tracked as `plato-308`, detail in that folder's `README.md`. A red Stage 2 is not your fault
  unless your error count exceeds the number in the issue.
- `generated/` — buildable generated projects. `Plato.Generated.Foundation.Unoptimized` (forward `stdlib/foundation`, extension-style, **wrapper** scalars, optimizers off) is the only one; the two scalar-erased legacy projects were retired 2026-08-01. **Not a golden** (2026-07-30 retirement): the byte-identity diff-gate and its `regen-generated.ps1` script are gone; this is ordinary cached output anyone may regenerate, and staleness is acceptable. Docs in `generated/README.md`.
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

- `.\tools\check-stdlib-fast.ps1` — the forward-stdlib inner loop (seconds). Three gates:
  `lint --strict` over the three shipping tiers (`-IncludeFuture` adds `future`; 0 parse /
  0 resolution errors), the **checker ratchet**
  (`ForwardStdLibDiagnosticCountDoesNotRegress` in `tests/PlatoTests/ForwardStdLibCheckerTests.cs`) —
  your change may not raise the diagnostic count, and when you lower it you lower the ceiling in
  the same commit — and **index freshness**, which fails when `stdlib/types-and-concepts.txt` no
  longer matches the source ([`stdlib/AGENTS.md`](stdlib/AGENTS.md)). `-SkipIndex` opts out.
- `.\tools\regen-forward-conformance.ps1` — forward-stdlib milestone gate. Stage 1 gating (see
  `tests/conformance/Plato.ForwardConformanceTests/` above); `-Codegen` / `-Test` run the diagnostic stages.
- `.\tools\check-all.ps1` — full gate battery, PASS/FAIL table. **Run once at the end of a mission**; iterate on a single relevant gate during development.
- `.\tools\gate-timings.ps1` — how long the gates take. Every gate script records its duration
  (and failures) via `tools\gate-timing.ps1` into `%LOCALAPPDATA%\ara3d\gate-timings.csv`; this
  reports runs / median / P90 / max / total per gate, sorted by total time. `-Days`, `-Gate`,
  `-Tail`, `-Failures`. Include the seconds in your gate table when you report results.
- `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib-legacy` — exit 0 unless `--strict`; the finding count drifts with library content, so compare against the previous run, not a hardcoded baseline.

Every gate is PowerShell and Windows-pathed. An agent on Linux/CI is limited to `dotnet build`
and `dotnet test`.

**Recording what the gates said:** `python tools/record-gates.py [--full]` runs them and writes
the results down — current state into `docs/status-report-snapshot.json`, one appended history row
into [`docs/gate-log.md`](docs/gate-log.md). `--full` adds a clean regeneration of the forward
stdlib plus the conformance law runner (~7 min); without it you get lint + PlatoTests (~2 min).
Run it at the end of a mission instead of hand-copying numbers into a report. Note that it
REGENERATES before it tests: a suite that passes against a stale `Generated/` folder is the
easiest wrong green in this repo to produce.
**Ratchets are enforced in tests, not in the log:** `ForwardStdLibLintTests` (lint findings) and
`ForwardStdLibCheckerTests` (type-checker diagnostics), both in `tests/PlatoTests`. Each holds a
ceiling constant to LOWER, never raise, in the commit that earns it.
**Status report (optional HTML):** from the Plato repo root,
`python tools/gen-status-report.py` refreshes `docs/status-report.html` (live git/tracker;
gate/lint/C# build rows from `docs/status-report-snapshot.json`). Not required on every commit.
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
2. Nothing in this repo is frozen any more. The V1 runtime, its SHA-256 tripwire and the golden diff-gate are all retired (2026-07-31 / 2026-07-30); `generated/` is ordinary cached output that anyone may regenerate.
3. Generated code must compile with DEFAULT LangVersion on net8.0. No C# 14 features.
4. Known bugs are now BEING fixed (content-leads, from 2026-07-09). The `KnownFailures.json`
   manifest is the burn-down queue: when you fix a bug, REMOVE its manifest entry in the same change
   (a passing still-listed entry fails the runner with "remove from manifest").
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
  intrinsic you do add must have a `src/Plato.Intrinsics` counterpart or `IntrinsicObligationTests`
  fails. Full contract: `docs/plato-intrinsics-surface.md`.
- **An intrinsic must not be expressible in Plato from the other intrinsics** (plato-378,
  2026-07-31). The contract is an irreducible kernel — the scalar operators and transcendentals,
  five array functions (`Count`, `At`, `MapRange`, `Reduce`, `FlatMap`), three Boolean
  connectives, and the `List` / `Buffer` builders. If a portable reference body exists the
  function belongs in a `*.library.plato` file (`primitives-number`, `primitives-integer`,
  `primitives-arrays`), and a backend recovers native speed through its override table
  (plato-368) — never by re-adding a bodiless declaration. Before adding one, write the body;
  if the body compiles, that is your answer.

## Mission protocol

- Maintain `PROGRESS.md` in your workspace (10 lines max, updated as you go) so a crashed session resumes cheaply.
- On completion: close your tracker issue (`python tools/track.py close <id> --outcome "..."`)
  and record any lasting decision in the relevant `docs/` plan, and keep the final report
  under ~300 words using: files touched / gates table / surprises / rerun commands.

### Writing the final report

The report is read by a human who was not in your session. It is not a diff summary and not a
set of notes to yourself. Terse is fine; **unexplained is not** — the reader must not have to ask
"what does that mean?" about any line of it.

- **Every named thing gets a clause saying what it is, the first time it appears.** Constants,
  tables, allowlists, test names, gate names, issue ids. Not `re-pinned to the five scalars` but
  `re-pinned to the five scalars (the gate's scope list — Number/Integer/Boolean/Character/String;
  the other receivers left the runtime, so they are out of scope rather than fixed)`.
- **Say the consequence, not just the mechanism.** A reader wants to know what is now true that
  was not true before, and what would break if the change were wrong.
- **Name the decision behind every allowlist entry.** Adding a name to a known-missing /
  ignored / override list is a judgement call. Report which call you made and why it is not a
  regression — that is the part a reviewer is actually checking.
- **A tracker item filed is not a finding reported.** One sentence per filed issue, in plain
  terms: what is wrong, what it costs, how big the fix is. The issue file holds the detail; the
  report has to make the reader able to decide whether to care.
- **Distinguish verified / done-but-unverified / not started**, and say which gates you actually
  ran versus inherited from a previous run.
- No arrow chains, no invented abbreviations, no compound noun phrases standing in for a
  sentence. If a phrase needs a paragraph to unpack, you are at the wrong altitude: describe the
  effect instead of the mechanism.

### Build output and logs

Compile errors are evidence; do not let them exist only in your scrollback. Any C# build whose
result you intend to report — especially a RED one — goes through
`powershell tools/dotnet-build-record.ps1 -Project <csproj> -TargetName <name>` (add `-RecordOnly`
when you expect failure and want the run recorded rather than aborted). That writes the full build
log to `.temp/csharp-build-logs/<target>.log` and the error totals, by CS code and by category,
into `docs/status-report-snapshot.json`. `.temp/` is git-ignored and per-target files are
overwritten, so it is a working store, not a history: when a red build matters beyond the session,
**quote the distinct error codes and one representative message per code in the tracker issue or
the commit message**, which are the durable records. A bare `dotnet build` on a Plato project
leaves nothing behind and is the wrong tool when you care about the result.
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
