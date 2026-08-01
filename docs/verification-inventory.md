# Verification inventory — every tool that checks this repo

**One entry per script, fixture, server tool and data file involved in verifying Plato: where it
lives, what it runs, what it reads and writes, and which question it answers.**

Companion to [`stdlib/VERIFICATION.md`](../stdlib/VERIFICATION.md), which describes the *policy* —
the ladder of checks, what each rung proves, the ratchets and their scopes. This file describes the
*implementations* of that policy. Read VERIFICATION.md to know what should happen; read this to
know what actually runs and where it lives.

Scope: verification of the forward stdlib and the compiler. Work tracking (`tools/track.py`) is a
different domain and appears only so the `tools/` listing is complete.

**This document records no measurements** — no file counts, finding counts, ratchet ceilings, test
tallies or timings, and no line numbers, all of which move week to week. Each entry names the
constant, command or log that holds the live value. See
[`docs/documentation-conventions.md`](documentation-conventions.md).

## How to read an entry

- **Where** — path, and which repository.
- **Runs** — what it actually executes.
- **Corpus** — which `.plato` files it reads. Different tools read different corpora *by design*;
  this is the field that explains why two tools' numbers legitimately differ.
- **Input / Output** — flags in, files and exit codes out.
- **Answers** — the question you run it to answer.
- **Depends on** — what has to be working for it to work at all.

Two repositories are involved. `plato` is this checkout. `studio` is the monorepo that carries
Plato as `submodules/Plato`; some gates still live there and resolve their paths **inside that
submodule working tree**, which is not this checkout. See "Structural observations" below.

---

## A. Cold gate scripts — this repo

### `tools/check-stdlib-fast.ps1`

- **Where** `plato`, PowerShell.
- **Runs** three gates: `Plato.CLI lint <tiers> --strict`; the PlatoTests type-checker ratchet, by
  test-name filter; and `export-types-context.ps1 -Check -IndexOnly` for declaration-index
  freshness.
- **Corpus** the shipping tiers, named explicitly because `Plato.CLI lint` enumerates each root
  top-directory-only — linting `stdlib` itself finds nothing. Tier list is a literal in the script.
  `future` joins only under `-IncludeFuture`.
- **Input** `-SkipLint`, `-SkipRatchet`, `-SkipIndex`, `-IncludeFuture`, `-Folders a,b`. `-Folders`
  lints an explicit set of roots compiled as one program — the cumulative-tier subset form.
- **Output** PASS/FAIL table with per-gate seconds; failing gates replay their captured output.
  Exit 0 or 1. Appends rows to the shared timing log.
- **Answers** "is my stdlib edit safe to commit?"
- **Depends on** `src/Plato.CLI`, `tests/PlatoTests`, `tools/export-types-context.ps1`,
  `tools/gate-timing.ps1`, the dotnet SDK, PowerShell.

### `tools/stage-stdlib.ps1`

- **Where** `plato`, PowerShell.
- **Runs** mirrors `stdlib/` into a staging area outside every git tree, then lints the copy with a
  **pinned** `Plato.CLI` publish snapshot rather than the live build.
- **Corpus** shipping tiers by default, its own literal list. `-Folders` bypasses staging and lints
  the named roots in place.
- **Input** `-NoSync`, `-Snapshot` (rebuild and re-pin the binary), `-IncludeFuture`, `-Folders`.
- **Output** full lint output, a summary, and a baseline pinned on the first green run, all in the
  staging area. Exit code is the CLI's.
- **Answers** "is my stdlib edit clean, independent of whatever state the compiler is in right now?"
- **Depends on** the pinned binary only — deliberately *not* the live compiler.
- **Note** this is the only gate that survives a broken compiler, which is its whole purpose. Any
  consolidation must leave that property intact. Re-pin with `-Snapshot` while trunk is green;
  multi-root support requires a snapshot taken after that support landed, or an older pinned binary
  silently lints only the first folder.

### `tools/export-types-context.ps1` (with `export-types-context.bat`)

- **Where** `plato`, PowerShell over `src/Plato.ContextExport`.
- **Runs** two exports: a sorted concept-then-type index of the shipping tiers, and a flat
  declaration dump of `legacy/stdlib-legacy`.
- **Corpus** shipping tiers for the index, its own literal list; `future` is deliberately absent
  because it is declared, not shipped.
- **Input** `-Check` (regenerate to a temp location, compare against the tracked file, exit 1 on
  difference, write nothing), `-IndexOnly`.
- **Output** writes the tracked `stdlib/types-and-concepts.txt` and
  `docs/types-and-concepts-context.txt`, plus gitignored stats.
- **Answers** "what is the whole vocabulary, compressed, for agent context?" and — in `-Check`
  mode — "does the tracked index still match the source?"
- **Note** dual-role: generator and gate. Only the write mode touches tracked files; the gate mode
  never does. The standing obligation to regenerate the index alongside declaration changes is in
  [`stdlib/AGENTS.md`](../stdlib/AGENTS.md).

---

## B. Cold gate scripts — studio repo

Both resolve their paths under `studio/submodules/Plato`, a separate working tree from this
checkout. Confirm which tree you are gating before trusting a result here — see
[plato-372](../tracker/issues/plato-372.md).

### `studio/tools/check-all.ps1`

- **Runs** the full battery: a checksum tripwire over the frozen V1 artifacts, `lint --strict` over
  `stdlib-legacy`, `lint --strict` over the forward shipping tiers, and the SDK build plus its
  geometry tests.
- **Corpus** forward shipping tiers, another literal list.
- **Output** PASS/FAIL table, exit code, timing rows.
- **Answers** "is the whole monorepo green?" Intended to run once at the end of a mission.
- **Note** its first gate runs the frozen-V1 tripwire, which [`AGENTS.md`](../AGENTS.md) records as
  retired. Its Plato project and library paths match the pre-restructure layout.

### `studio/tools/regen-forward-conformance.ps1`

- **Runs** merges `stdlib/` and `tests/stdlib-tests/` into a temporary folder, throwing on any
  filename collision, then two stages. Stage 1 (gating) type-checks the merged sources and asserts
  no resolution errors and at least one `Law_` function, proving the library and the laws resolve
  against each other. Stage 2 (`-Codegen` / `-Test`) generates C# with the shipping recipe, builds
  it, and runs the reflection law runner.
- **Corpus** stage 1 reads all four tiers; stage 2 excludes `future` — expressed not as a tier list
  but as a directory filter over the merged input. `-IncludeFuture` opts back in.
- **Input** `-Codegen`, `-Test`, `-IncludeFuture`, `-Configuration`.
- **Output** generated `.g.cs` into the conformance suite's `Generated/` folder; console; timing rows.
- **Answers** rungs 5 and 6 — "does the library become C#, and does that C# compute the right
  answers?"
- **Note** its `.DESCRIPTION` block carries a dated status narrative. VERIFICATION.md's standing
  rule applies: when a document and a gate disagree, the gate is right.

---

## C. Warm server

### `plato_check` — `studio/labs/PlatoNavigationMcp/CheckMcpTools.cs`

- **Where** `studio`, C#, an MCP tool inside a hand-launched long-running server.
- **Runs** parse, resolve to a full `Compilation`, `Linter`, `TypeChecker.CheckAll`,
  `SumTypeChecker` with `ExistentialConceptChecker`, and `StyleChecker`. Each gate is memoized per
  index generation, so asking for one never pays for another and a repeat with no edits is free.
- **Corpus** whatever the server was launched with as `--root`, in practice all four tiers **plus**
  `tests/stdlib-tests/`. The code has no concept of a tier.
- **Input** `gates` (a subset), `files` (scopes the *report* only — the whole corpus is always
  compiled), `maxFindings`.
- **Output** structured findings. `data.ok` inside the payload is the verdict on the code; the
  outer envelope's `ok` only says the call succeeded.
- **Answers** "what is wrong with the file I am editing, right now, in seconds?"
- **Depends on** `Plato.Compiler` and `Plato.Navigation`, and on being launched against the right
  roots — the server has no default corpus, by design, after one silently indexed the wrong checkout.
- **Note** two known defects, both [plato-389](../tracker/issues/plato-389.md): non-shipping tiers
  and the law packet count toward its verdict, and the type-checker ceiling arrives as a launch
  argument, making it a second copy of the test constant.
- **Usage** read the `plato-mcp` skill first.

---

## D. The enforcers

Everything above is a runner. These fixtures are what actually fails a build. Each names the
authority for its own ceiling; none is restated here.

### `tests/PlatoTests/ForwardStdLibCheckerTests.cs`

- `ForwardStdLibParsesAndCompiles` — parses every file under all four tiers and builds a
  `Compilation`, asserting empty parse failures, symbol resolution errors, semantic errors and
  internal errors, plus a completed compilation. Also asserts a **corpus floor**: every other
  assertion is an emptiness check, so an enumeration bug that found no files would pass while
  proving nothing. Must hold in Debug and Release.
- `IncompleteCompilationAlwaysReportsAnInternalError` — an incomplete compilation must always come
  with at least one internal error to act on. The compiler may not fail silently.
- `ForwardStdLibDiagnosticCountDoesNotRegress` — **the type-checker ratchet**. Ceiling is the
  constant in this file.
- `ForwardStdLibHasNoViewlessExistentialReferences` — a hard zero, not a ratchet: a concept stored
  in type position must have an object-safe view or the writer cannot lower it.
- `SummarizeForwardStdLibDiagnostics`, `SummarizeForwardStdLibSumTypeDiagnostics` — reports, not
  assertions. The sum-type one reports the sum population alongside the diagnostics, because zero
  diagnostics only means something if sum types were actually seen.

### `tests/PlatoTests/ForwardStdLibLintTests.cs`

- The **lint ratchet**, scoped to the shipping tiers, counting errors and warnings and excluding
  Info. Ceiling is the constant in this file; the comment above it is the historical record of what
  the number is made of.
- `SummarizeForwardStdLibLintIncludingFuture` — the opt-in view over all four tiers, reporting only.
- Exists because `lint --strict` fails on errors only: it prints the warning count and moves on, so
  nothing enforced that number until this fixture.

### The intrinsic contract

- `IntrinsicContractSizeTests` — a **size ratchet** on bodiless declarations in
  `stdlib/foundation/intrinsics.library.plato`, plus a scan asserting no other `*.library.plato`
  file declares one. A textual count, so it costs milliseconds and cannot be broken by an unrelated
  compiler regression.
- `IntrinsicObligationTests` — reflection over the compiled `src/Plato.Intrinsics`, failing when a
  declared intrinsic has no runtime counterpart. Matching is by name and receiver, not parameter
  count: overload drift is the C# build's job, this catches members missing entirely. Carries its
  own scope-sanity assertion, so a broken filter cannot produce a vacuous pass.
- `IntrinsicsSurfaceTests` — polices the writer's picture of the handwritten runtime surface
  (property versus method form), where a silent disagreement becomes errors across a thousand
  generated files.
- `tests/Plato.Intrinsics.Tests/` — the NUnit suite for the runtime itself, validating the other
  half of the contract.

### `tests/conformance/Plato.ForwardConformanceTests/`

- `LawTests` — discovers every `(type, Law_*)` pair by reflection over the generated assembly,
  constructs seeded values, and asserts through `KnownFailures.json`, the quarantine manifest. A
  passing but still-listed entry fails the runner, so a fix must remove its entry in the same change.
- `BlockerGuardTests` — asserts a nonzero discovered case count and a nonzero generated type count.
  A test run that discovers no tests is otherwise a passing run.

### Shared helper

`tests/PlatoTests/CheckerTestSupport.cs` — holds the `ShippingTiers` and `AllTiers` lists, the
walk-up folder resolution that lets fixtures find source folders from the test output directory,
recursive `.plato` enumeration, and multi-root compilation matching how `Plato.CLI lint` treats
several roots as one program.

---

## E. Recording and reporting

### `tools/record-gates.py`

- **Runs** the CLI build, `lint --strict`, and PlatoTests. With `--full`, also empties the
  conformance `Generated/` folder, regenerates with the shipping recipe, and runs the law runner.
- **Corpus** shipping tiers, its own `TIERS` constant; `--include-future` appends.
- **Input** `--full`, `--dry-run`, `--include-future`.
- **Output** current state into `docs/status-report-snapshot.json`, merging with prior rows and
  marking rows it did not run as stale rather than dropping them; one appended row to
  `docs/gate-log.md`.
- **Answers** "what did the gates say at this commit, written down rather than hand-copied?"
- **Note** it regenerates before it tests, because a suite passing against a stale `Generated/`
  folder is the easiest wrong green in this repo to produce. It also reads results by regular
  expression over console text, so an output-format change degrades a recorded number silently
  rather than failing — [plato-390](../tracker/issues/plato-390.md). It carries its own copy of the
  codegen recipe flags.

### `tools/gen-status-report.py`

- **Output** `docs/status-report.html`.
- **Live sections** git status, commits, dirty files, worktrees, submodules, studio pointer drift,
  tracker issues. **Cached sections** gate, lint and navigation rows, read from the snapshot JSON.
- **Answers** "one page showing where the repo stands."
- **Note** run by the pre-commit hook; needs the studio monorepo layout for tracker issue links.

### `tools/dotnet-build-record.ps1` with `tools/record-csharp-build-errors.py`

- **Runs** wraps `dotnet build`, writes a UTF-8 log under `.temp/csharp-build-logs/`, then parses it.
- **Input** `-Project`, `-TargetName`, `-Configuration`, `-RecordOnly` (a failing build still exits
  zero after recording, for diagnostic builds).
- **Output** C# error totals by code and by human category into the snapshot JSON.
- **Answers** "which kind of codegen defect dominates, and is it shrinking?"
- **Note** [`AGENTS.md`](../AGENTS.md) asks for this instead of a bare `dotnet build` on Plato
  projects whenever the status report matters. MSBuild prints every error twice — compare counts
  per error shape, not totals.

---

## F. Infrastructure

### `tools/gate-timing.ps1`

A dot-sourced library, not a gate. `Add-GateTiming` records one measurement; `Start-GateRun` /
`Complete-GateRun` / `Exit-GateRun` time a whole script. Appends timestamped rows carrying machine,
repo, script, gate, result, seconds and detail. The log is machine-global rather than per checkout,
because gates run from worktrees, the submodule copy and the staging area and the point is one
aggregate answer. Overridable by environment variable, and disablable. Retries briefly on a locked
file rather than failing a gate over a log write, since sessions run concurrently.

### `tools/gate-timings.ps1`

The reader. Reports runs, median, P90, max and total per gate. Flags: `-Days`, `-Gate`, `-Tail`,
`-Failures`, `-Path`. **This is the authority for how long any gate takes.**

### `tools/githooks/pre-commit` with `tools/install-githooks.ps1`

Regenerates `docs/status-report.html` and stages it with the commit. The only piece of this
inventory that runs automatically — and it runs the reporter, not a gate.

### `tools/track.py`

Work tracking, not verification. Listed for completeness of the `tools/` folder.

---

## G. Data files

| File | Written by | Read by | Tracked |
|---|---|---|---|
| `stdlib/types-and-concepts.txt` | `export-types-context.ps1` | agents; gated by its `-Check` mode | yes |
| `docs/types-and-concepts-context.txt` | `export-types-context.ps1` | agents | yes |
| `docs/status-report-snapshot.json` | `record-gates.py`, `record-csharp-build-errors.py` | `gen-status-report.py` | yes |
| `docs/gate-log.md` | `record-gates.py`, appended | humans | yes |
| `docs/status-report.html` | `gen-status-report.py` | humans | yes |
| shared gate timing CSV | `gate-timing.ps1` | `gate-timings.ps1` | no |
| staging lint output and baseline | `stage-stdlib.ps1` | humans | no |
| `KnownFailures.json` | humans | `LawTests` | yes |
| `.temp/csharp-build-logs/*.log` | `dotnet-build-record.ps1` | `record-csharp-build-errors.py` | no |

Every file above is an **output**. There is no input data file: every policy decision — which tiers
a check covers, which ceiling applies, which flags the codegen recipe uses — is expressed in code,
once per tool. That is [plato-388](../tracker/issues/plato-388.md).

---

## H. Where the same fact lives more than once

| Fact | Locations |
|---|---|
| shipping tier list | `check-stdlib-fast.ps1`, `stage-stdlib.ps1`, `export-types-context.ps1`, `record-gates.py`, `CheckerTestSupport.cs`, studio `check-all.ps1` |
| the `future` exclusion rule | all of the above, plus studio `regen-forward-conformance.ps1`, which expresses it as a directory filter rather than a tier list |
| type-checker ratchet ceiling | the constant in `ForwardStdLibCheckerTests`, and the MCP server's `--ratchet` launch argument |
| codegen recipe flags | `record-gates.py`, studio `regen-forward-conformance.ps1` |
| the static checks themselves | `Plato.CLI lint`, `CheckMcpTools.cs`, the PlatoTests fixtures |
| PASS/FAIL table with timing | `check-stdlib-fast.ps1`, studio `check-all.ps1`, `record-gates.py` |

Nothing asserts that any of these copies agree. Three languages, two repositories, three execution
models. Tracked as [plato-388](../tracker/issues/plato-388.md) (agree on *what* to check) and
[plato-390](../tracker/issues/plato-390.md) (agree on *how*).

---

## I. Structural observations

**Tier coverage and proof strength are two axes, not one.** The ladder in VERIFICATION.md measures
how strong a proof is. Which tiers a check covers measures how much of the library was proved.
Folding one into the other is what makes the current arrangement hard to reason about, and it is
why a "foundation only" check should be a scope, not a new rung. Both the `future` rule and any
cumulative scope are cells in a single tier-by-check matrix.

**`plato_check` is most of a consolidated tool already.** It runs every static check against a
`Compilation` and returns structured findings. What it lacks is tier awareness, a command-line
host, and the codegen and law-execution rungs. That makes extraction, not rewriting, the cheap path.

**Some gates resolve inside a different working tree.** The studio scripts name paths under
`studio/submodules/Plato`, which is a separate checkout from this one. Both trees contain content,
so neither script errors — they gate a different copy of the library than the one being edited.
Confirm which tree is authoritative before relying on a studio gate result, and before porting or
deleting those scripts: [plato-372](../tracker/issues/plato-372.md).

---

## See also

- [`stdlib/VERIFICATION.md`](../stdlib/VERIFICATION.md) — the policy: the ladder, what each rung
  proves, the ratchets, and the wrong greens this repo has actually produced
- [`AGENTS.md`](../AGENTS.md) — repo layout, hard rules, which command to run when
- [`docs/documentation-conventions.md`](documentation-conventions.md) — why this file states no
  measurements
- [`docs/plato-library-map.md`](plato-library-map.md) — which generated artifact is which, and the
  options used to build each
- the `plato-mcp` skill — `plato_check` and the symbol query chain
