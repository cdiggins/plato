# The stdlib verification ladder

**How `stdlib/` is tested and validated — what each rung proves, what it cannot prove, and which
command runs it.**

Scope: the **forward** stdlib (`stdlib/`) and its law packet (`tests/stdlib-tests/`). The legacy
pair (`legacy/stdlib-legacy/` + `legacy/stdlib-legacy-tests/`) has its own gates and is out of scope
here; see [`AGENTS.md`](../AGENTS.md).

The organizing idea: **a `.plato` file is not a program you can run.** Nothing in `stdlib/` executes
until it has been converted to C# and linked against `src/Plato.Intrinsics`. So validation is a
ladder of seven rungs, each cheaper and weaker than the one above it, and the honest question about
any stdlib claim is *how far up the ladder is it backed?*

| Rung | Proves | Enforced by |
|---|---|---|
| 0 · parse | the text is Plato | `ForwardStdLibParsesAndCompiles` |
| 1 · resolve | every name binds to something | same test + `lint --strict` |
| 2 · lint | structural defects the writer would turn into runtime holes | `ForwardStdLibLintTests` (ratchet) |
| 3 · style | authoring invariants no compiler pass owns | `StyleChecker` via `plato_check` |
| 4 · type-check | every function body is well-typed | `ForwardStdLibCheckerTests` (ratchet) |
| 5 · codegen | the vocabulary survives the C# writer and compiles | `regen-forward-conformance.ps1 -Codegen` |
| 6 · execute | the bodies compute the right answers | `Plato.ForwardConformanceTests` law runner |

Rungs 0–4 run in seconds, rung 5 in minutes, rung 6 in minutes plus a build. For measured
per-gate timings use `.\tools\gate-timings.ps1` (runs / median / P90 / max, from the shared
timing log) rather than a number written down here.

Rungs 0–4 are static and green. Rung 5 generates but does not yet compile clean. Rung 6 is the only
rung that runs a body, and it is the one still being brought up
([plato-308](../tracker/issues/plato-308.md) / [plato-323](../tracker/issues/plato-323.md)).
**Until rung 6 is routinely green, every behavioural claim about `stdlib/` — that a formula is
correct, that a sign is right — is backed by inspection, not by a test.** That is the single most
important thing this document has to say.

---

## The inner loop: which command, when

**While editing** — `plato_check` from the `plato-navigation` MCP server. It runs rungs 0–4 inside a
warm process against cached ASTs, so it costs seconds instead of the minutes two cold `dotnet`
processes need, and returns structured findings instead of console text:

```
gates: "parse,resolve,lint,types,sums,style"   # or a subset; `types` alone never pays for lint
```

Read the `plato-mcp` skill before using it. Two gotchas that matter for validation:

- **Its corpus is `stdlib/` *plus* `tests/stdlib-tests/`** (both are index roots), and it lints all
  four tiers. The gate tests scope differently — see "Scope differences" below — so its numbers
  will not match the ratchet ceilings, by design.
- `data.ok` (inside the payload) is the corpus verdict; the envelope's `ok` only says the call
  worked.

**After an edit batch / before committing** — the cold gate:

```bash
powershell -File tools/check-stdlib-fast.ps1
```

Two checks, a PASS/FAIL table, failing output replayed: `lint --strict` over the three shipping
tiers, and the checker ratchet. Flags: `-SkipLint`, `-SkipRatchet`, `-IncludeFuture`,
`-Folders a,b` (lint an explicit tier subset).

**When the compiler itself is in flux** — `tools/stage-stdlib.ps1` lints a scratch copy of `stdlib/`
against a *pinned* `Plato.CLI` snapshot in `C:\Users\cdigg\git\plato-staging`, so library work never
blocks on (or falsely fails from) another session's half-built compiler. Re-pin with `-Snapshot`
while trunk is green.

**At the end of a mission** — record what the gates said rather than hand-copying numbers:

```bash
python tools/record-gates.py
```

`--full` adds a clean regeneration plus the conformance law runner; `--dry-run` measures and writes
nothing. It writes current state to `docs/status-report-snapshot.json` and appends one
history row to [`docs/gate-log.md`](gate-log.md). Note that it **regenerates before it tests** — a
suite that passes against a stale `Generated/` folder is the easiest wrong green in this repo to
produce.

> **The studio copies are stale.** `AGENTS.md` still says the gates run from
> `C:\Users\cdigg\git\studio\tools\`. Those copies predate the repo reorg: they reference
> `submodules\Plato\Plato.CLI\` and lint `stdlib` top-only, which now finds **zero files** and
> passes vacuously ([plato-372](../tracker/issues/plato-372.md)). Prefer the repo-local
> `tools/check-stdlib-fast.ps1` and `tools/stage-stdlib.ps1`, which derive every path from
> `$PSScriptRoot`. `regen-forward-conformance.ps1` and `check-all.ps1` exist only in studio.

---

## The rungs

### Rung 0–1 · Parse and resolve

`ForwardStdLibParsesAndCompiles` (`tests/PlatoTests/ForwardStdLibCheckerTests.cs`) parses every
`.plato` file under all four tiers and builds a `Compilation`, asserting four empty collections:
parse failures, `SymbolFactory.Errors` (name resolution), `SemanticErrors`, `InternalErrors`, plus
`CompletedCompilation`. It also carries a **corpus floor** — a minimum file count, set far below the
real one so ordinary growth or pruning never trips it — because every
other assertion is an `IsEmpty`, so an enumeration bug that found no files would pass this gate
while proving nothing. That is not hypothetical: it happened when the tier subfolders landed and a
top-only enumeration silently emptied the corpus.

`Plato.CLI lint --strict` covers the same ground from the command line and is what
`check-stdlib-fast.ps1` runs. Note `lint` enumerates each root **top-directory-only**, which is why
every caller names the tier folders explicitly rather than passing `stdlib`.

This rung proves the vocabulary is internally consistent — every member a body calls exists
somewhere. It proves nothing about types and nothing about values.

### Rung 2 · Lint

`Linter` (`src/Plato.Compiler/Analysis/Linter.cs`) runs a numbered rule set, `LINT001` upward; the
class comment at the top of that file is the authoritative list. The ones that carry validation
weight rather than tidiness:

- **LINT001** — a type implements a concept but an obligation has no implementation. The generated
  member throws `NotImplementedException`. This is a *runtime hole with a green compile*, which is
  exactly why it is ratcheted.
- **LINT012** — an obligation and its implementation disagree on the `_` receiver marker
  (static vs instance). Written as a lint rule after that shape produced 40 CS0736 errors a thousand
  generated files downstream.
- **LINT013** — a concept with no concrete implementer that library bodies nonetheless dispatch on:
  unreachable derived surface. The bulk of the lint ratchet is this one rule.
- **LINT002 / 004 / 005** — errors: bad `where` clauses, duplicate signatures, uninferable type
  variables.
- **LINT006 / 007** — the affine-type structural bans (no `unique` builder in a field, none as a
  generic argument).
- **LINT003** — declared-but-unused fields. **Info, deliberately excluded from the ratchet**: it
  fires in the thousands over vocabulary declared ahead of its bodies, which would make the ceiling
  a number nobody could move. It also **cannot see field reads inside
  statement blocks or `var` initializers**, so converting an expression body to a statement body
  makes the count *rise* for fields that are genuinely read. Do not read LINT003 deltas as coverage.

Why a test and not just the script: **`lint --strict` fails on errors only.** It prints the warning
count and moves on, so twenty new LINT001 warnings would pass every gate silently. Nothing enforced
the printed number until `ForwardStdLibLintTests`.

### Rung 3 · Style

`StyleChecker` (`src/Plato.Navigation/StyleChecker.cs`) — the authoring invariants no compiler pass
owns. It runs on a `ParsedFile`, needs no `Compilation`, and is effectively free.

- **STY001** (Error) — the identifier `New`, the C# writer's reserved constructor name.
- **STY002** (Error) — a concrete type with more than 10 fields; the `TupleN` surface stops at 10,
  so an 11-field type has no functioning generated form.
- **STY003** (Error) — the token `implicit`; implicit operators are a C#-side decision.
- **STY005** (Error) — more than one declaration kind in a file, or a kind contradicting the
  `.concepts.` / `.types.` / `.library.` suffix.
- **STY004 / STY006** (Warning) — doc-comment blocks over 12 lines (file header exempt); more than
  12 top-level declarations per file.

STY001/002/003/005 are the ones that prevent a file from having a working generated form; they are
worth treating as hard.

### Rung 4 · Type-check

`TypeChecker` runs over every function in the forward compilation. Three fixtures:

- `ForwardStdLibDiagnosticCountDoesNotRegress` — **the ratchet**, ceiling 0. The forward vocabulary
  type-checks clean. At 0 a ceiling is also a floor: any new diagnostic is a regression.
- `SummarizeForwardStdLibDiagnostics` — the worklist, not an assertion. Run it to see what failed,
  grouped by code and by unresolved name.
- `SummarizeForwardStdLibSumTypeDiagnostics` — `SumTypeChecker`, which matters because the forward
  stdlib is where sum types actually live (`PathSegment2D`, `Paint`, `MaskSource2D`,
  `ScalarFieldNode2D/3D`, `WindowFunction`). It reports the sum *population* alongside the
  diagnostics, because "0 diagnostics" only means something if sums were seen.
- `ForwardStdLibHasNoViewlessExistentialReferences` — **hard zero, not a ratchet**. Every concept
  stored in type position (an existential `any C`) must have an object-safe surface or it has no
  defined C# lowering. A new CHK308 means the writer cannot emit it.

Known permissiveness worth knowing when reading a green: **`Self` in return position is not
arity-checked against the receiver's fields.** `Self` unifies with anything by design and is grounded
at monomorphization, so a wrong-shape tuple body is accepted here and only caught downstream.

### The intrinsic contract (a side rail on rungs 2–4)

A bodiless signature in `stdlib/foundation/intrinsics.library.plato` is a promise that each backend
runtime supplies the member. Three fixtures police it:

- `IntrinsicObligationTests` — reflects over the compiled `src/Plato.Intrinsics` and fails when a
  declared intrinsic has no runtime counterpart. Matching is by name and receiver, deliberately not
  by arity: overload drift is the C# build's job; this catches members **missing entirely**. Before
  it existed, those failed a thousand generated files downstream.
- `IntrinsicContractSizeTests` — a **size ratchet**, ceiling 65 (was 141 before plato-378). Textual
  count of bodiless declarations, so it costs milliseconds and cannot be broken by an unrelated
  compiler regression. The rule it enforces: *an intrinsic must not be expressible in Plato from the
  other intrinsics.* If you are here because it failed: write the Plato body first; if it compiles,
  that is your answer and the function belongs in a `*.library.plato` file.
- `IntrinsicsSurfaceTests` — polices the writer's *picture* of the handwritten runtime surface
  (property-vs-method spelling under `--no-properties`), where a silent disagreement means
  CS0030/CS1955 across a thousand files.

`tests/Plato.Intrinsics.Tests/` is the NUnit suite for the runtime itself (numbers, integers,
booleans, characters, strings, arrays, builders, the public surface). It validates the *other half*
of the contract: the C# the stdlib bottoms out in.

### Rung 5 · Codegen

```bash
powershell -File C:\Users\cdigg\git\studio\tools\regen-forward-conformance.ps1 -Codegen
```

The script merges `stdlib/` + `tests/stdlib-tests/` into a temp folder (throwing on any filename
collision), then runs two stages:

- **Stage 1** (gating, green): type-check the merged sources, assert 0 symbol resolution errors,
  and assert the law packet declares at least one `Law_` function. This proves the whole forward
  vocabulary and the `Law_*` packet resolve *against each other* — the laws are not compiled in
  isolation. This is the honest currently-green forward gate.
- **Stage 2** (`-Codegen` / `-Test`): full-recipe C# codegen of the merged sources minus
  `stdlib/future`, into `tests/conformance/Plato.ForwardConformanceTests/Generated/`, then build,
  then run.

The recipe is the shipping one: `--csharp-style=extensions --scalar=float --optimize
--optimize-arrays --inline --methods --loops --no-properties --static-abstract`.

Codegen **succeeds**: it emits over a thousand `.g.cs` files, plus some *degraded bodies* — a member
with no ground TIR emits a throwing stub and is recorded in `Writer.DegradedBodies` rather than
aborting the whole output. Both counts are reported by the run itself and logged in
[`docs/gate-log.md`](gate-log.md); the degraded count is a burn-down number, so read it there rather
than from prose. Whether the generated C# **compiles** is the live question — see "Reading the
current state" below. Build-level quarantine of failing files was measured and does **not** work —
the forward stdlib is too densely linked and exclusion cascades into CS0246.

### Rung 6 · Execute the laws

`tests/stdlib-tests/` holds `Law_*` / `Witness_*` Boolean functions in `library` blocks, one
`*.laws.plato` file per domain. They are **never merged into `stdlib/`**; the harness merges them at
gate time.

`tests/conformance/Plato.ForwardConformanceTests` discovers `Law_*` members by **reflection** over
the generated assembly and runs each as a test case. Consequences to respect:

1. **`Law_*` functions stay in structs** — the runner reflects instance members.
2. **`KnownFailures.json` is the burn-down queue.** Each entry quarantines one (law, type) pair. When
   you fix the bug, remove the entry **in the same change** — a passing but still-listed entry fails
   the runner with "remove from manifest". The file is empty today.
3. **`BlockerGuardTests` prevents a vacuous green.** Zero discovered law cases would otherwise be a
   passing NUnit run; the guard turns "nothing was generated" into an explicit RED.

Writing a law: every member a law references must be verified against the forward library source
first (LIBRARIES.md rule 3) — use `plato_search_symbols` → `plato_definition`, never guess a name.
Note the open defect [plato-374](../tracker/issues/plato-374.md): concept-generic law bodies can mix
a concept default with a type's override, so prefer laws written against concrete types until that
lands.

---

## The ratchets

Three ceilings, all enforced **in tests**, none in a log:

| Ratchet | Constant | Scope |
|---|---|---|
| lint findings (E+W, Info excluded) | `ForwardStdLibLintTests.MaxLintRatchet` | foundation + geometry + graphics |
| type-checker diagnostics | `ForwardStdLibCheckerTests.MaxFunctionsWithDiagnostics` | all four tiers |
| intrinsic contract size | `IntrinsicContractSizeTests.MaxIntrinsics` | `intrinsics.library.plato` |

**The constant in the test is the only copy of each ceiling.** Do not restate its value here or in
any other doc — a second copy is a copy that goes stale silently while the enforced one moves.

The rule is the same for all three: **a ceiling to LOWER, never to raise.** When you earn a lower
number, lower it in the same commit. If a ceiling genuinely has to move up, say why in the comment
above the constant — that comment is the historical record of what the number is made of, and it is
worth reading before touching the number.

A recorded number nobody asserts on is a number that drifts. `docs/gate-log.md` and
`status-report-snapshot.json` are history and reporting; they are not gates.

### Scope differences (why the numbers disagree)

`stdlib/future` is aspirational vocabulary — declarations for domains the library intends to cover
but does not implement yet. It is held to a lower bar **on purpose**:

| | foundation / geometry / graphics | future |
|---|---|---|
| parses | yes | yes |
| type-checks (0 diagnostics) | yes | yes |
| linted | yes | only with a flag |
| converted to C# | yes | only with a flag |

Opt `future` back in with `-IncludeFuture` (PowerShell gates), `--include-future`
(`record-gates.py`), or the `SummarizeForwardStdLibLintIncludingFuture` reporting test. Parsing and
type-checking are **not** behind a flag.

So four different tools report four legitimately different lint counts, and none of them is wrong.
Before comparing two numbers, check they cover the same corpus:

| Tool | Corpus |
|---|---|
| lint ratchet test | shipping tiers only |
| `record-gates.py` / `lint --strict` | shipping tiers (four with the flag) |
| `plato_check` | all four tiers **plus** `tests/stdlib-tests/` |
| `SummarizeForwardStdLibLintIncludingFuture` | four tiers, reporting only |

The same applies to type-check counts: the ratchet compiles `stdlib/` alone; `plato_check` includes
the law packet, so a defect in a law shows up there and *not* in the ratchet.

---

## Wrong greens: the failure modes this repo has actually produced

Every item below happened. They are the reason several assertions look paranoid.

1. **Empty corpus passes everything.** A top-only enumeration after the tier reorg found zero files;
   every `IsEmpty` assertion passed. Mitigated by the corpus floor and by
   `regen-forward-conformance.ps1` enumerating `-Recurse`.
2. **Zero law cases passes NUnit.** Mitigated by `BlockerGuardTests`.
3. **Stale `Generated/`.** A suite that passes against last week's output proves nothing.
   `record-gates.py` regenerates before it tests for exactly this reason.
4. **`lint --strict` ignores warnings.** Mitigated by the lint ratchet test.
5. **A Debug-only assert swallowed in Release.** `ForwardStdLibParsesAndCompiles` must hold in both
   configurations; the bug it replaced was a `Debug.Assert` in `FunctionInstance` that fired only
   under Debug and was then caught.
6. **MSBuild prints every error twice.** Count *distinct* errors, and compare per-shape counts, not
   totals — two sessions working concurrently make the total an unstable quantity.
7. **Stale gate scripts.** See the studio note above ([plato-372](../tracker/issues/plato-372.md)).

---

## Reading the current state

**This document deliberately records no measurements.** Counts of files, findings, diagnostics,
generated files and passing tests all move every week; a copy of one in prose is a fact that goes
stale silently, and a reader cannot tell a stale copy from a fresh one. Every such number has exactly
one live home:

| Question | Where the answer lives |
|---|---|
| what do the gates say right now? | `plato_check`, or `python tools/record-gates.py --dry-run` |
| what did they say at commit X? | [`docs/gate-log.md`](gate-log.md) — one appended row per run |
| current machine-readable state | `docs/status-report-snapshot.json` (+ `tools/gen-status-report.py` for HTML) |
| what is the ceiling for a ratchet? | the constant in the test (see the ratchet table above) |
| how long does a gate take? | `.\tools\gate-timings.ps1` |
| what is left on a burn-down? | the tracker issue, via `python tools/track.py show <id>` |

**Prefer measuring to reading.** Several status blocks in this repo describe blockers that were
fixed after they were written — the conformance suite's `README.md` and
[plato-308](../tracker/issues/plato-308.md) both still describe a declaration-layer error inventory
that no longer exists, while the gate log shows the law runner passing. That pattern is the norm, not
the exception: **when a document and a gate disagree, the gate is right.** Re-measure before quoting
any status prose, including this file.

---

## Adding coverage

**A new law.** Verify every member it calls with `plato_search_symbols` / `plato_definition`. Put it
in a `library` block in `tests/stdlib-tests/` — never in `stdlib/`. Keep it in a struct (the runner
reflects instance members). Filenames must not collide with any `stdlib/` file; the merge step
throws on collision. Then `regen-forward-conformance.ps1 -Test`.

**A new intrinsic.** Write the Plato body first. If it compiles, the function belongs in a
`*.library.plato` file and you are done — a backend recovers native speed through its override table
([plato-368](../tracker/issues/plato-368.md)), never by re-adding a bodiless declaration. If it
genuinely cannot be written (it needs a loop, bit-level access, or a representation constant): add
the counterpart in `src/Plato.Intrinsics`, raise the `IntrinsicContractSizeTests` ceiling in the same
commit stating which of those three it is, and add a `Plato.Intrinsics.Tests` case for it.

**A new rule.** Rules that need a `Compilation` go in `src/Plato.Compiler/Analysis/Linter.cs` as the
next `LINT0nn`; text-and-shape rules that need only a `ParsedFile` go in
`src/Plato.Navigation/StyleChecker.cs` as the next `STY0nn` — the latter is essentially free and
therefore the right home for authoring conventions. Either way, a new rule that fires on existing
content lands with its ceiling raised and a comment saying what the findings are, or it lands with
the content fixed. It does not land silent.

---

## See also

- [`AGENTS.md`](../AGENTS.md) — repo layout, hard rules, mission protocol
- [`stdlib/README.md`](../stdlib/README.md) — tiers, the `future` bar, partition rules
- [`stdlib/CONVENTIONS.md`](../stdlib/CONVENTIONS.md) — domain semantics (wins on conflict)
- [`stdlib/STYLE_GUIDE.md`](../stdlib/STYLE_GUIDE.md) / [`stdlib/LIBRARIES.md`](../stdlib/LIBRARIES.md) — authoring rules the style checker encodes
- [`docs/plato-library-map.md`](plato-library-map.md) — which artifact is which, and the recipe per artifact
- [`docs/gate-log.md`](gate-log.md) — appended history of gate runs
- `tests/conformance/Plato.ForwardConformanceTests/README.md` — law runner detail (status section is stale)
- the `plato-mcp` skill — `plato_check` and the symbol query chain
