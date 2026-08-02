# Verification — how this library is tested

**What each level of checking proves, what it cannot prove, and which command runs it.** The fourth
companion doc for this folder, beside [`CONVENTIONS.md`](CONVENTIONS.md) (what the vocabulary
means), [`STYLE_GUIDE.md`](STYLE_GUIDE.md) (how to write it) and [`LIBRARIES.md`](LIBRARIES.md)
(where function bodies go): this one is **how you know your edit is good.**

This file describes the **policy** — what is checked and what that proves. Its companion
[`docs/verification-inventory.md`](../docs/verification-inventory.md) describes the
**implementations**: one entry per script, fixture, server tool and data file, with what each one
runs, which corpus it reads, what it writes, and which question it answers. Read this file to know
what should happen; read that one to find the code that does it, or to work out why two tools
disagree.

Scope: this folder (`stdlib/`, the current "forward" standard library) and its test suite
(`tests/stdlib-tests/`). The older "legacy" pair (`legacy/stdlib-legacy/` +
`legacy/stdlib-legacy-tests/`) has its own checks and is out of scope here; see
[`AGENTS.md`](../AGENTS.md).

## Terms this document uses

- **Gate** — any automated check that must pass before a change is considered good.
- **Green / red** — a gate passing / failing.
- **Tier** — one of the four subfolders of `stdlib/`: `foundation`, `geometry`, `graphics`
  (the three *shipping* tiers), and `future` (planned vocabulary, held to a lower bar — see
  "Scope differences" below).
- **Corpus** — the set of `.plato` files a given tool actually reads. Different tools read
  different corpora, which is why their numbers legitimately differ.
- **Ratchet** — a test that asserts a defect count never exceeds a fixed ceiling (a constant in
  the test). When you reduce the count, you lower the ceiling in the same commit; it is never
  raised without a written justification. This stops old defects from quietly multiplying while
  they are being burned down.
- **Law** — a Boolean function named `Law_*` in `tests/stdlib-tests/` stating a property that
  should always hold (e.g. that an operation is commutative). Running the laws is the only check
  that actually *executes* library code. `Witness_*` functions are helpers that construct example
  values for laws.
- **Concept / obligation** — a *concept* is Plato's interface-like construct; an *obligation* is a
  member a concept requires its implementers to provide.
- **Intrinsic** — a function declared in Plato without a body, with the implementation supplied by
  handwritten C# in `src/Plato.Intrinsics`. The stdlib ultimately "bottoms out" in these.
- **Wrong green** — a gate that passes while proving nothing, e.g. because it found zero files to
  check. Several assertions below exist only to prevent specific wrong greens that have actually
  happened.

## The ladder

The organizing idea: **a `.plato` file is not a program you can run.** Nothing in `stdlib/`
executes until it has been converted to C# and linked against `src/Plato.Intrinsics`. So
validation is a ladder of seven rungs. Each rung up gives a stronger guarantee and costs more to
run, and the honest question about any stdlib claim is *how far up the ladder is it backed?*

| Rung | Proves | Enforced by |
|---|---|---|
| 0 · parse | the text is valid Plato syntax | `ForwardStdLibParsesAndCompiles` |
| 1 · resolve | every name refers to something that exists | same test + `lint --strict` |
| 2 · lint | no structural defects that would become runtime failures | `ForwardStdLibLintTests` (ratchet) |
| 3 · style | authoring rules no compiler pass enforces | `StyleChecker` via `plato_check` |
| 4 · type-check | every function body is well-typed | `ForwardStdLibCheckerTests` (ratchet) |
| 5 · codegen | the library survives conversion to C# and that C# compiles | `regen-forward-conformance.ps1 -Codegen` |
| 6 · execute | the code computes the right answers (the laws hold) | `Plato.ForwardConformanceTests` law runner |

Rungs 0–4 run in seconds, rung 5 in minutes, rung 6 in minutes plus a build. For measured
per-gate timings use `.\tools\gate-timings.ps1` (runs / median / P90 / max, from the shared timing
log) rather than a number written down here.

Rungs 0–4 are static analysis and currently green. Rung 5 generates C# but that C# does not yet
compile clean. Rung 6 is the only rung that runs a function body, and it is still being brought up
([plato-308](../tracker/issues/plato-308.md) / [plato-323](../tracker/issues/plato-323.md)).
**Until rung 6 is routinely green, every behavioural claim about `stdlib/` — that a formula is
correct, that a sign is right — is backed by human inspection, not by a test.** That is the single
most important thing this document has to say.

---

## The inner loop: which command, when

**While editing** — `plato_check`, a tool provided by the `plato-navigation` MCP server (a
language-server-like process agents query). It runs rungs 0–4 inside an already-warm process
against cached syntax trees, so it costs seconds instead of the minutes two cold `dotnet`
processes need, and returns structured findings instead of console text:

```
gates: "parse,resolve,lint,types,sums,style"   # or a subset; `types` alone never pays for lint
```

Read the `plato-mcp` skill before using it. Two gotchas that matter for validation:

- **Its corpus is `stdlib/` *plus* `tests/stdlib-tests/`** (both are index roots), and it lints
  all four tiers. The gate tests scope differently — see "Scope differences" below — so its
  numbers will not match the ratchet ceilings, by design.
- In its response, `data.ok` (inside the payload) is the verdict on the code; the outer envelope's
  `ok` only says the call itself worked.

**After a batch of edits / before committing** — the cold (fresh-process) gate:

```bash
powershell -File tools/check-stdlib-fast.ps1
```

Three checks, a PASS/FAIL table, failing output replayed: `lint --strict` over the three shipping
tiers, the type-checker ratchet, and index freshness — whether `types-and-concepts.txt` still
matches the source (the rule it enforces is in [`AGENTS.md`](AGENTS.md)). Flags: `-SkipLint`,
`-SkipRatchet`, `-SkipIndex`, `-IncludeFuture`, `-Folders a,b` (lint an explicit subset of tiers).

**When the compiler itself is being changed** — `tools/stage-stdlib.ps1` lints a scratch copy of
`stdlib/` against a *pinned* (frozen, known-good) `Plato.CLI` snapshot in
`C:\Users\cdigg\git\plato-staging`, so library work never blocks on — or falsely fails because of —
another session's half-built compiler. Re-pin with `-Snapshot` while the main build is green.

**At the end of a work session** — record what the gates said rather than hand-copying numbers:

```bash
python tools/record-gates.py
```

`--full` adds a clean regeneration plus the conformance law runner; `--dry-run` measures and
writes nothing. It writes current state to `docs/status-report-snapshot.json` and appends one
history row to [`docs/gate-log.md`](../docs/gate-log.md). Note that it **regenerates the C# before
it tests** — a test suite that passes against a stale `Generated/` folder is the easiest wrong
green in this repo to produce.

> **The studio copies of the gate scripts are stale.** `AGENTS.md` still says the gates run from
> `C:\Users\cdigg\git\studio\tools\`. Those copies predate the repo reorganization: they reference
> `submodules\Plato\Plato.CLI\` and lint only the top level of `stdlib`, which now finds **zero
> files** and passes as a wrong green ([plato-372](../tracker/issues/plato-372.md)). Prefer the
> repo-local `tools/check-stdlib-fast.ps1` and `tools/stage-stdlib.ps1`, which derive every path
> from their own location. `regen-forward-conformance.ps1` and `check-all.ps1` exist only in
> studio.

---

## The rungs

### Rung 0–1 · Parse and resolve

`ForwardStdLibParsesAndCompiles` (`tests/PlatoTests/ForwardStdLibCheckerTests.cs`) parses every
`.plato` file under all four tiers and builds a `Compilation`, asserting four empty collections:
parse failures, `SymbolFactory.Errors` (name resolution), `SemanticErrors`, `InternalErrors`, plus
`CompletedCompilation`. It also asserts a **corpus floor** — a minimum file count, set far below
the real count so ordinary growth or pruning never trips it. The floor exists because every other
assertion is an `IsEmpty`: a file-enumeration bug that found no files would pass this gate while
proving nothing. That is not hypothetical — it happened when the tier subfolders were introduced
and an enumeration that only read the top directory silently emptied the corpus.

`Plato.CLI lint --strict` covers the same ground from the command line and is what
`check-stdlib-fast.ps1` runs. Note `lint` reads each root folder **without recursing into
subfolders**, which is why every caller names the tier folders explicitly rather than passing
`stdlib`.

This rung proves the library is internally consistent — every member a body calls exists
somewhere. It proves nothing about types and nothing about values.

### Rung 2 · Lint

`Linter` (`src/Plato.Compiler/Analysis/Linter.cs`) runs a numbered rule set, `LINT001` upward; the
class comment at the top of that file is the authoritative list. The rules that catch real
defects, rather than untidiness:

- **LINT001** — a type implements a concept but one obligation has no implementation. The
  generated C# member throws `NotImplementedException`: a *runtime hole behind a green compile*,
  which is exactly why this rule is ratcheted.
- **LINT012** — an obligation and its implementation disagree on the `_` receiver marker (which
  distinguishes static from instance members). Written as a lint rule after that mismatch produced
  40 CS0736 errors a thousand generated files downstream.
- **LINT013** — a concept with no concrete implementing type that library bodies nonetheless call
  through: derived API surface nothing can ever reach. The bulk of the lint ratchet count is this
  one rule.
- **LINT002 / 004 / 005** — errors: bad `where` clauses, duplicate signatures, type variables the
  checker cannot infer.
- **LINT006 / 007** — structural bans for affine types (Plato's use-at-most-once types): no
  `unique` builder stored in a field, none used as a generic argument.
- **LINT003** — declared-but-unused fields. **Informational, deliberately excluded from the
  ratchet**: it fires in the thousands over vocabulary declared ahead of its implementations,
  which would make the ceiling a number nobody could lower. It also **cannot see field reads
  inside statement blocks or `var` initializers**, so converting an expression body to a statement
  body makes the count *rise* for fields that are genuinely read. Do not read LINT003 changes as a
  coverage signal.

Why a ratchet test and not just the script: **`lint --strict` fails on errors only.** It prints
the warning count and moves on, so twenty new LINT001 warnings would pass every gate silently.
Nothing enforced the printed number until `ForwardStdLibLintTests`.

### Rung 3 · Style

`StyleChecker` (`src/Plato.Navigation/StyleChecker.cs`) enforces the authoring rules no compiler
pass owns. It runs on a single parsed file — no whole-program `Compilation` needed — and is
effectively free.

- **STY001** (Error) — the identifier `New`, which the C# writer reserves as its constructor name.
- **STY002** (Error) — a concrete type with more than 10 fields; the generated tuple surface
  (`TupleN`) stops at 10, so an 11-field type has no working generated form.
- **STY003** (Error) — the token `implicit`; implicit conversion operators are a C#-side decision.
- **STY005** (Error) — more than one kind of declaration in a file, or a kind contradicting the
  file's `.concepts.` / `.types.` / `.library.` name suffix.
- **STY004 / STY006** (Warning) — doc-comment blocks over 12 lines (file header exempt); more than
  12 top-level declarations per file.

STY001/002/003/005 are the ones that prevent a file from having a working generated form; treat
them as hard errors.

### Rung 4 · Type-check

`TypeChecker` runs over every function in the forward compilation. Four relevant test fixtures:

- `ForwardStdLibDiagnosticCountDoesNotRegress` — **the ratchet**, with a ceiling of 0: the forward
  library currently type-checks clean. At 0 the ceiling is also a floor — any new diagnostic is a
  regression.
- `SummarizeForwardStdLibDiagnostics` — a report, not an assertion. Run it to see what failed,
  grouped by diagnostic code and by unresolved name.
- `SummarizeForwardStdLibSumTypeDiagnostics` — runs `SumTypeChecker`, the checker for sum types
  (tagged unions), which matters because the forward stdlib is where sum types actually live
  (`PathSegment2D`, `Paint`, `MaskSource2D`, `ScalarFieldNode2D/3D`, `WindowFunction`). It reports
  how many sum types were seen alongside the diagnostics, because "0 diagnostics" only means
  something if sum types were actually checked.
- `ForwardStdLibHasNoViewlessExistentialReferences` — **a hard zero, not a ratchet**. An
  *existential* is a concept used in type position (`any C`, "some value implementing C"); each
  one must have an object-safe view (a surface expressible as a C# interface) or the writer has no
  defined way to lower it to C#. A new CHK308 diagnostic means the writer cannot emit that code.

Known permissiveness worth knowing when reading a green: **`Self` in return position is not
arity-checked against the receiver's fields.** `Self` (the implementing type, as in other
languages' `Self`/`this`-type) unifies with anything by design and is only pinned down when
generics are specialized to concrete types (monomorphization), so a tuple body of the wrong shape
is accepted at this rung and only caught downstream.

### The intrinsic contract (a side rail on rungs 2–4)

A bodiless signature in `stdlib/foundation/intrinsics.library.plato` is a promise that each
backend runtime supplies the member. Three fixtures police that promise:

- `IntrinsicObligationTests` — uses reflection over the compiled `src/Plato.Intrinsics` and fails
  when a declared intrinsic has no runtime counterpart. Matching is by name and receiver,
  deliberately not by parameter count: overload drift is the C# build's job to catch; this catches
  members **missing entirely**. Before it existed, a missing member failed a thousand generated
  files downstream.
- `IntrinsicContractSizeTests` — a **size ratchet** on the number of bodiless declarations
  (ceiling lowered from 141 in plato-378). It is a textual count, so it costs milliseconds and
  cannot be broken by an unrelated compiler regression. The rule it enforces: *an intrinsic must
  not be expressible in Plato using the other intrinsics.* If you are here because it failed:
  write the Plato body first; if it compiles, that is your answer, and the function belongs in a
  `*.library.plato` file instead.
- `IntrinsicsSurfaceTests` — polices the code writer's *picture* of the handwritten runtime
  surface (whether each member is spelled as a struct field/property or a method),
  where a silent disagreement means CS0030/CS1955 errors across a thousand files.

`tests/Plato.Intrinsics.Tests/` is the NUnit suite for the runtime itself (numbers, integers,
booleans, characters, strings, arrays, builders, the public surface). It validates the *other
half* of the contract: the handwritten C# the stdlib bottoms out in.

### Rung 5 · Codegen

```bash
powershell -File C:\Users\cdigg\git\studio\tools\regen-forward-conformance.ps1 -Codegen
```

The script merges `stdlib/` + `tests/stdlib-tests/` into a temporary folder (throwing on any
filename collision), then runs two stages:

- **Stage 1** (gating, currently green): type-check the merged sources, assert 0 symbol
  resolution errors, and assert the test suite declares at least one `Law_` function. This proves
  the whole forward library and the laws resolve *against each other* — the laws are not compiled
  in isolation. This is the honest currently-green forward gate.
- **Stage 2** (`-Codegen` / `-Test`): full C# code generation of the merged sources minus
  `stdlib/future`, into `tests/conformance/Plato.ForwardConformanceTests/Generated/`, then build,
  then run.

The code-generation options are the shipping ones: `--csharp-style=extensions
--optimize --optimize-arrays --inline --loops --static-abstract`.

Code generation **succeeds**: it emits over a thousand `.g.cs` files, plus some **degraded
bodies** — when the generator cannot produce real code for a member (its intermediate
representation never becomes fully concrete), it emits a stub that throws at runtime and records
the member in `Writer.DegradedBodies` rather than aborting the whole output. Both counts are
reported by the run itself and logged in [`docs/gate-log.md`](../docs/gate-log.md); the degraded
count is a number being driven to zero, so read it there rather than from prose. Whether the
generated C# **compiles** is the live question — see "Reading the current state" below. Excluding
the failing files from the build was tried and does **not** work — the forward stdlib is too
densely interlinked, and each exclusion cascades into CS0246 (missing type) errors elsewhere.

### Rung 6 · Execute the laws

`tests/stdlib-tests/` holds the `Law_*` / `Witness_*` Boolean functions in `library` blocks, one
`*.laws.plato` file per domain. They are **never merged into `stdlib/`**; the harness merges the
two folders at gate time.

`tests/conformance/Plato.ForwardConformanceTests` discovers `Law_*` members by **reflection** over
the generated assembly and runs each as a test case. Consequences to respect:

1. **`Law_*` functions stay in structs** — the runner reflects over instance members only.
2. **`KnownFailures.json` is the quarantine list.** Each entry excuses one known-failing
   (law, type) pair so the rest of the suite can gate. When you fix the bug, remove the entry
   **in the same change** — a passing but still-listed entry fails the runner with "remove from
   manifest". The file is empty today.
3. **`BlockerGuardTests` prevents a vacuous green.** Zero discovered law cases would otherwise be
   a passing NUnit run; the guard turns "nothing was generated" into an explicit failure.

Writing a law: every member a law references must be verified against the forward library source
first (LIBRARIES.md rule 3) — use `plato_search_symbols` then `plato_definition`; never guess a
name. Note the open defect [plato-374](../tracker/issues/plato-374.md): a law written generically
over a concept can mix a concept's default implementation with a type's override, so prefer laws
written against concrete types until that is fixed.

---

## The ratchets

Three ceilings, all enforced **in tests**, none in a log:

| Ratchet | Constant | Scope |
|---|---|---|
| lint findings (errors + warnings, Info excluded) | `ForwardStdLibLintTests.MaxLintRatchet` | foundation + geometry + graphics |
| type-checker diagnostics | `ForwardStdLibCheckerTests.MaxFunctionsWithDiagnostics` | all four tiers |
| intrinsic contract size | `IntrinsicContractSizeTests.MaxIntrinsics` | `intrinsics.library.plato` |

**The constant in the test is the only copy of each ceiling.** Do not restate its value here or in
any other document — a second copy goes stale silently while the enforced one moves.

The rule is the same for all three: **a ceiling is lowered, never raised.** When you earn a lower
number, lower the constant in the same commit. If a ceiling genuinely has to move up, say why in
the comment above the constant — that comment is the historical record of what the number is made
of, and it is worth reading before touching the number.

A recorded number nobody asserts on is a number that drifts. `docs/gate-log.md` and
`status-report-snapshot.json` are history and reporting; they are not gates.

### Scope differences (why the numbers disagree)

`stdlib/future` is aspirational vocabulary — declarations for domains the library intends to
cover but does not implement yet. It is held to a lower bar **on purpose**:

| | foundation / geometry / graphics | future |
|---|---|---|
| parses | yes | yes |
| type-checks (0 diagnostics) | yes | yes |
| linted | yes | only with a flag |
| converted to C# | yes | only with a flag |

Opt `future` back in with `-IncludeFuture` (PowerShell gates), `--include-future`
(`record-gates.py`), or the `SummarizeForwardStdLibLintIncludingFuture` reporting test. Parsing
and type-checking are **not** behind a flag.

So four different tools report four legitimately different lint counts, and none of them is
wrong. Before comparing two numbers, check they cover the same corpus:

| Tool | Corpus |
|---|---|
| lint ratchet test | shipping tiers only |
| `record-gates.py` / `lint --strict` | shipping tiers (all four with the flag) |
| `plato_check` | all four tiers **plus** `tests/stdlib-tests/` |
| `SummarizeForwardStdLibLintIncludingFuture` | all four tiers, reporting only |

The same applies to type-check counts: the ratchet compiles `stdlib/` alone; `plato_check` also
reads the law files, so a defect in a law shows up there and *not* in the ratchet.

---

## Wrong greens: the failure modes this repo has actually produced

Every item below happened. They are the reason several assertions look paranoid.

1. **An empty corpus passes everything.** After the tier reorganization, an enumeration that only
   read the top directory found zero files; every `IsEmpty` assertion passed. Mitigated by the
   corpus floor and by `regen-forward-conformance.ps1` enumerating with `-Recurse`.
2. **Zero law cases passes NUnit.** A test run that discovers no tests is a passing run.
   Mitigated by `BlockerGuardTests`.
3. **A stale `Generated/` folder.** A suite that passes against last week's generated output
   proves nothing about today's sources. `record-gates.py` regenerates before it tests for
   exactly this reason.
4. **`lint --strict` ignores warnings.** Mitigated by the lint ratchet test.
5. **A Debug-only assert, swallowed in Release.** `ForwardStdLibParsesAndCompiles` must hold in
   both build configurations; the bug it replaced was a `Debug.Assert` in `FunctionInstance` that
   fired only under Debug and was then caught by the exception filter.
6. **MSBuild prints every error twice.** Count *distinct* errors, and compare counts per error
   shape, not totals — two sessions working concurrently make the total an unstable quantity.
7. **Stale gate scripts.** See the studio note above
   ([plato-372](../tracker/issues/plato-372.md)).

---

## Reading the current state

**This document deliberately records no measurements.** Counts of files, findings, diagnostics,
generated files and passing tests all move every week; a copy of one in prose goes stale
silently, and a reader cannot tell a stale copy from a fresh one. Every such number has exactly
one live home:

| Question | Where the answer lives |
|---|---|
| what do the gates say right now? | `plato_check`, or `python tools/record-gates.py --dry-run` |
| what did they say at commit X? | [`docs/gate-log.md`](../docs/gate-log.md) — one appended row per run |
| current machine-readable state | `docs/status-report-snapshot.json` (+ `tools/gen-status-report.py` for HTML) |
| what is the ceiling for a ratchet? | the constant in the test (see the ratchet table above) |
| how long does a gate take? | `.\tools\gate-timings.ps1` |
| what is left on a known-failure list? | the tracker issue, via `python tools/track.py show <id>` |

**Prefer measuring to reading.** Several status write-ups in this repo describe blockers that
were fixed after they were written — the conformance suite's `README.md` and
[plato-308](../tracker/issues/plato-308.md) both still describe an error inventory that no longer
exists, while the gate log shows the law runner passing. That pattern is the norm, not the
exception: **when a document and a gate disagree, the gate is right.** Re-measure before quoting
any status prose, including this file.

---

## Adding coverage

**A new law.** Verify every member it calls with `plato_search_symbols` / `plato_definition`. Put
it in a `library` block in `tests/stdlib-tests/` — never in `stdlib/`. Keep it in a struct (the
runner reflects over instance members). Filenames must not collide with any `stdlib/` file; the
merge step throws on collision. Then run `regen-forward-conformance.ps1 -Test`.

**A new intrinsic.** Write the Plato body first. If it compiles, the function belongs in a
`*.library.plato` file and you are done — a backend recovers native speed through its override
table ([plato-368](../tracker/issues/plato-368.md)), never by re-adding a bodiless declaration.
If it genuinely cannot be written in Plato (it needs a loop, bit-level access, or a
representation constant): add the counterpart in `src/Plato.Intrinsics`, raise the
`IntrinsicContractSizeTests` ceiling in the same commit stating which of those three reasons
applies, and add a `Plato.Intrinsics.Tests` case for it.

**A new rule.** Rules that need a whole-program `Compilation` go in
`src/Plato.Compiler/Analysis/Linter.cs` as the next `LINT0nn`; text-and-shape rules that need
only a single parsed file go in `src/Plato.Navigation/StyleChecker.cs` as the next `STY0nn` — the
latter is essentially free to run and therefore the right home for authoring conventions. Either
way, a new rule that fires on existing content lands with its ceiling raised and a comment saying
what the findings are, or it lands with the content fixed. It does not land silent.

---

## See also

- [`docs/verification-inventory.md`](../docs/verification-inventory.md) — **every tool that implements
  the policy on this page**: where each script, fixture and server tool lives, what it runs, which
  corpus it reads, and what it writes
- [`AGENTS.md`](../AGENTS.md) — repo layout, hard rules, mission protocol
- [`stdlib/README.md`](README.md) — tiers, the `future` bar, partition rules
- [`stdlib/CONVENTIONS.md`](CONVENTIONS.md) — domain semantics (wins on conflict)
- [`stdlib/STYLE_GUIDE.md`](STYLE_GUIDE.md) / [`stdlib/LIBRARIES.md`](LIBRARIES.md) — authoring rules the style checker encodes
- [`docs/plato-library-map.md`](../docs/plato-library-map.md) — which artifact is which, and the options used to build each
- [`docs/gate-log.md`](../docs/gate-log.md) — appended history of gate runs
- `tests/conformance/Plato.ForwardConformanceTests/README.md` — law runner detail (status section is stale)
- the `plato-mcp` skill — `plato_check` and the symbol query chain
