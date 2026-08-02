# Plato Navigation MCP improvements — faster, cheaper stdlib agents

**Date:** 2026-07-30 · **Status:** PROPOSED · **Scope:** `labs/PlatoNavigationMcp` (studio repo)
+ `submodules/Plato/Plato.Navigation` · **Goal:** cut agent wall-clock and token spend when
working on `stdlib/` (~398 flat `.plato` files, 2D/3D geometry + graphics priority).

Builds on the already-agreed baseline from the prior planning session: add
`plato_check(files)` returning structured diagnostics from the warm server, plus text-level
linters (forbid `New`, tuple cap, implicit operator, doc-length, one-kind-per-file, ≤12
decls). This document ranks that baseline against a wider candidate set, kills the weak
ideas, and specifies the survivors.

---

## 1. Where the time and tokens actually go

Grounded in the code as it stands today (all paths verified by reading source):

**The server side is already fast.** `PlatoNavigationContext` (labs/PlatoNavigationMcp)
wraps `IncrementalIndexer` + `ParseCache` (Plato.Navigation): parse is ~97% of an index
build and is fully cached per (path, content-hash); binding is whole-program and reruns in
~60 ms at 70 files (README benchmarks). At ~398 files expect a cold build of a few seconds
and a one-file reload well under a second. Navigation queries themselves are microseconds.
**Navigation is not the bottleneck.**

**The verification loop is the bottleneck.** An agent editing stdlib today runs
`tools/check-stdlib-fast.ps1` per iteration, which is two *cold processes*:

1. `dotnet run Plato.CLI lint stdlib --strict` — process start + build check + re-parse of
   all ~398 files + `Compilation` + `Linter`. No parse cache, no warm state.
2. `dotnet test --filter ForwardStdLibDiagnosticCountDoesNotRegress` — test-host launch,
   then `CheckerTestSupport.CompileForwardStdLib()` (another full re-parse + `Compilation`)
   plus `TypeChecker(...).CheckAll()` over ~2,100 functions.

Ballpark 1–2 minutes of wall-clock per iteration, repeated many times per session, of which
the *useful* computation (resolve + check on a warm parse cache) is seconds. This is the
single biggest speed lever available, and it is exactly the agreed `plato_check` direction.

**The token side.** Three concrete leaks:

- Console-formatted gate output (lint dump + NUnit test output) is verbose, unstructured,
  and re-read by the agent every iteration.
- "What vocabulary exists for type X?" today costs a chain of
  `plato_search_symbols` → `plato_outline` → `plato_references` → `plato_source` calls
  (the SKILL.md query chain), each returning per-item JSON with `id`, `kind`, `name`,
  `signature`, `file`, `line`, `column`, `owner` — a 213-definition outline of
  `vectors.plato` alone is thousands of tokens.
- Stale-index discipline: the skill mandates `plato_reload` after every edit; forgetting it
  produces silently wrong results (ids shift), which costs a debugging detour.

**The third loop — forward conformance** (~282 C# body errors when building Generated
output) is dominated by regen + `dotnet build`, minutes per cycle. The server cannot make
MSBuild faster; it *can* make the error list cheaper to consume (see P6).

---

## 2. Code inventory (what the sketches below anchor to)

- `labs/PlatoNavigationMcp/Program.cs` — arg parsing, port 8768, `McpServer` from
  `Ara3D.MCP`, registers `IndexMcpTools` + `LookupMcpTools`.
- `labs/PlatoNavigationMcp/PlatoNavigationContext.cs` — holds roots + immutable
  `NavigationIndex`, `Reload()` under a lock via `IncrementalIndexer`, `ResolveFile`,
  root-relative `Display`.
- `labs/PlatoNavigationMcp/IndexMcpTools.cs` — `plato_index_status`, `plato_reload`,
  `plato_search_symbols`, `plato_outline`.
- `labs/PlatoNavigationMcp/LookupMcpTools.cs` — `plato_definition`, `plato_references`,
  `plato_source`; `Resolve()` accepts id | position | name.
- `labs/PlatoNavigationMcp/Results.cs` — result shaping, `DefaultLimit = 50`, one trimmed
  source line per reference already included.
- `submodules/Plato/Plato.Navigation/` — `NavigationIndex`, `IncrementalIndexer`,
  `ParseCache` (keyed path+hash, holds `AstFile`), `PlatoBinder.BoundSnapshot`
  (parse + `SymbolFactory.CreateTypeDefs`, *not* a full `Compilation`), `Records.cs`
  (`DefRecord` with `NameSpan`/`DeclSpan`/`Signature`/`Owner`, `RefRecord` with
  `RefKind.Value|Type|Operator` and `Targets`).
- `submodules/Plato/Plato.Compiler/Compilation.cs` — full pipeline from ASTs: SymbolFactory,
  semantic checks, reified types, function analyses. Takes `IEnumerable<AstNode>`, never
  touches disk — so it can be fed cached ASTs.
- `submodules/Plato/Plato.Compiler/Analysis/Linter.cs` — LINT001–014 structural checks over
  a completed `Compilation`; `LintFinding {File, Line, Code, Severity, Message}`.
- `submodules/Plato/PlatoTests/ForwardStdLibCheckerTests.cs` — the ratchet:
  `new TypeChecker(CheckerTestSupport.CompileForwardStdLib()).CheckAll()`, per-function
  `Diagnostics` with CHK codes, plus `SumTypeChecker` and `ExistentialConceptChecker`.
- `submodules/Plato/tools/record-csharp-build-errors.py` — parses `dotnet build` logs into
  code/category counters in `docs/status-report-snapshot.json`. Counts only; **no**
  .plato source mapping today.
- `~/.claude/skills/plato-mcp/` — SKILL.md + ensure-server.ps1. (An earlier draft of this plan
  claimed the script still defaults its roots to `stdlib-legacy`; that was **wrong** — verified
  2026-07-30, `ensure-server.ps1:95-100` defaults to `stdlib` + `stdlib-tests`, with
  `stdlib-legacy` reachable only via explicit `-Root` / `PLATO_MCP_ROOTS`. There is no roots
  footgun; only the staleness one, which §P2 fixes.)

Key architectural fact: Navigation binds with `SymbolFactory` directly and deliberately
skips the type checker. `TypeChecker`, `Linter`, `SumTypeChecker` all want a `Compilation`.
So `plato_check` is not "expose what the index already has" — it is "run the real compiler
front-end inside the warm server, reusing the parse cache." The parse cache is the expensive
90%; the rest must be measured but is seconds, not minutes.

---

## 3. The kill list

Judged on (a) agent wall-clock, (b) token savings, (c) implementation cost.

| Idea | Verdict | Why |
|---|---|---|
| Standalone signature-search tool (find by type shape) | **Kill (fold in)** | The real question is almost always "what takes/returns type X" — that is the vocabulary card (P4) with a filter; a general shape-query language is cost without a user. |
| Dead/unimplemented-member report as its own tool | **Kill (fold in)** | LINT001/003/008–010/013 already compute this; `plato_check` (P1) surfaces it. A second tool is a second thing to document. |
| `plato_batch` (multiple queries per call) | **Kill** | Round-trip latency on localhost is milliseconds; the token envelope per call is small; complexity lands in every client. |
| Server-side grep over indexed text | **Kill** | Agents already have ripgrep locally at zero marginal cost; the index's value is binding, not text. |
| Watch-mode `FileSystemWatcher` reindex | **Kill (replaced)** | Right instinct, wrong mechanism: watchers on Windows + git worktrees are flaky (renames, atomic saves, debounce tuning). Staleness check at query time (P2) gets the same guarantee race-free with ~20 lines. |
| Test generator as a server feature | **Kill as server feature** | The server cannot run an LLM and property-law invention is judgment. The deterministic feed (existing tests that reference a symbol) already falls out of `plato_references` once `stdlib-tests` is a root; note in the skill, not a tool. |
| Simplification ("find shorter/idiomatic version") as a server feature | **Kill as server feature** | Same reason. The deterministic 90% is "show me the idiom pool": vocabulary card (P4) + real usages (P5). The judgment lives in a prompt skill (§6). |
| `plato_context_pack` (whole-corpus digest export) | **Kill for now** | `Plato.ContextExport` + `tools/export-types-context.ps1` already exist for bulk context; no evidence agents need a fresher variant via MCP. Revisit if P4 proves insufficient. |

---

## 4. Surviving proposals

### P1 — `plato_check`: warm compile + lint + type-check with structured diagnostics

**The headline item; everything else is secondary.**

What it does: runs the same three gates as `check-stdlib-fast.ps1` — parse, resolve
(`Compilation`), `Linter`, `TypeChecker.CheckAll()` (+ `SumTypeChecker`,
`ExistentialConceptChecker`) — inside the already-running server, on the already-cached
ASTs, and returns machine-shaped findings instead of console text.

Tool signature sketch:

```
plato_check
  files?: string[]     // scope the *report* to these files; check still compiles the corpus
  gates?: string[]     // subset of ["parse","resolve","lint","types","sums"]; default all
  maxFindings?: int    // default 50, per gate
→ {
  ok, generation, elapsedMs: { parse, resolve, lint, check },
  parse:   { failed: [{file, message}] },
  resolve: { errors: [{file, line, message}] },
  lint:    { errors, warnings, ratchet, findings: [{file, line, code, severity, message}] },
  types:   { functionsChecked, functionsFailing, ratchetCeiling, delta,
             byCode: {CHK101: n, ...},
             findings: [{file, line, function, owner, code, message}] },
  style:   { findings: [...] }   // the text-level rules, see below
}
```

Why speed improves: replaces two cold `dotnet` processes (~1–2 min) with one warm call.
Parse is cached (`ParseCache.Parse` returns the stored `AstFile` for unchanged files), so
the marginal cost is resolve + reified types + `CheckAll`. Honest uncertainty: `Compilation`'s
constructor eagerly runs `GetOrComputeFunctionAnalysis` over every function (Compilation.cs
line ~142), and `CheckAll` walks ~2,100 functions — this has never been timed warm. Estimate
2–10 s; even the pessimistic end is a ~10× wall-clock win per iteration, and if it lands at
the slow end, an incremental follow-up scopes `CheckAll` to functions whose defining file is
in `files` (per-function checking is already independent — the ratchet test iterates results
per function).

Why tokens improve: the agent stops re-reading a lint dump and NUnit console output.
A clean run is one ~150-token response. A failing run returns exactly the findings, scoped
to `files`, with the ratchet delta computed server-side (`delta: +2` replaces the agent
diffing two console summaries).

Implementation sketch:

- `Plato.Navigation`: add a way to get the cached `ParsedFile` set for the current snapshot
  — either expose `IncrementalIndexer.ParsedFiles` or a small
  `CheckSupport.Compile(SourceSnapshot, ParseCache) → Compilation` helper that does what
  `BoundSnapshot.Create(snapshot, cache)` does but hands the ASTs to
  `new Compilation(Logger.Null, asts)` instead of a bare `SymbolFactory`. (`Compilation`
  never reads disk — verified — so this is a straight substitution of the parse step.)
- Safety: the binder only writes to symbols, never AST nodes (Plato.Navigation README §v2),
  and `IncrementalIndexer` already rebinds the same cached ASTs on every update, so feeding
  them to a second `SymbolFactory` inside `Compilation` is the already-proven pattern.
  Verify once with the existing byte-identity gate (index an unchanged snapshot after a
  check; generations must match).
- `PlatoNavigationMcp`: new `CheckMcpTools.cs` registering `plato_check`; hold the last
  `Compilation` keyed by index generation so repeated checks with no edits are free.
- Ratchet ceiling: read `MaxFunctionsWithDiagnostics` semantics as data — simplest is a
  `--ratchet <n>` server arg or a constant fetched from the test source at review time;
  do not parse the C# test file at runtime. (Current ceiling is 0, which makes `delta`
  trivially "any failing function is a regression".)
- Fold in the **text-level style linters** from the agreed baseline (forbid `New`,
  tuple cap >10 fields, implicit-operator ban, doc-length, one-kind-per-file, ≤12 decls
  per file). These are line/regex/AST-count checks over the snapshot texts — put them in a
  small `StyleChecker` in Plato.Navigation (they need the AST for decl counts, the text for
  the rest) and report them as the `style` block. They also become enforceable pre-commit
  later without MCP.

Cost: ~1.5–2 days including timing measurement, the Plato.Navigation seam, tests
(`PlatoNavigationMcp.Tests` has a `ServerFixture` to extend), and the style rules.
Risks: warm-check time unmeasured (mitigation above); `Compilation` halts hard after
resolution errors (line ~78–84: it returns early) — the response must degrade the same way
the CLI does, reporting resolve errors and skipping later gates rather than pretending they
passed; concurrent check + reload (reuse the existing `_reloadLock` or accept
last-generation results, they are immutable either way).

### P2 — staleness auto-detect: kill `plato_reload` from the inner loop

What it does: before answering any query, the server compares a cheap disk fingerprint
(file list + last-write times of `*.plato` under the roots) against the fingerprint taken
at last index; on mismatch it reloads first. `plato_reload` stays for forcing.

Why: removes one mandatory tool call (+ round trip + ~100-token response) per edit
iteration, and — worth more — removes the *silent stale results* failure mode the skill
currently spends three rules warning about. Fingerprinting ~400 files' mtimes is a few
milliseconds; a triggered reload is sub-second on the parse cache (only changed files
reparse; bind reruns in full as always).

Implementation sketch: `PlatoNavigationContext.EnsureFresh()` — enumerate
`Roots.SelectMany(GetFiles("*.plato"))` with `(path, lastWriteUtc, length)`, compare to the
stored list, call the existing `Reload()` on drift. Call it at the top of every tool handler
(one line each in `IndexMcpTools`/`LookupMcpTools`/`CheckMcpTools`). Responses gain a
`reloaded: true` flag when it fired, so agents can see it happen.

Cost: ~0.5 day. Risks: mtime granularity on some filesystems (mitigate: also compare
length; content hash already guards correctness at the ParseCache level — a spurious reload
is cheap, a missed one falls back to `plato_reload`); half-written saves (the reload after
the next query self-heals — same exposure as today's manual reload).

### P3 — compact response formats + token budgets

What it does: a `format: "compact"` option (and better defaults) that renders list-shaped
results as aligned text lines instead of per-item JSON objects.

Today one outline item is ~25 output tokens
(`{"id":412,"kind":"Method","name":"Dot","signature":"Dot(a: Vector3, b: Vector3): Number","file":"stdlib/vectors.library.plato","line":88,"column":5,"owner":"Vectors"}`).
Compact renders `412 M Dot(a: Vector3, b: Vector3): Number @88` with the file stated once
in the header — ~14 tokens, and the JSON punctuation overhead (which tokenizes badly)
disappears. Measured against `vectors.plato`'s 213 definitions, that is roughly a 2–3×
reduction on the heaviest responses agents actually make (outline, references,
vocabulary card). Ids are kept, so chaining still works.

Implementation sketch: all shaping already funnels through `Results.Def` / `Results.Ref` /
`Results.Truncated` — add compact renderers beside them and a `format` string on the
list-returning tools (`plato_outline`, `plato_references`, `plato_search_symbols`, P4, P5).
Default: compact for outline (its consumers never parse the JSON fields individually),
JSON elsewhere for one release, then flip defaults once agents are observed coping.
Additionally drop `column` and per-item `owner` from compact (line + name is enough to act).

Cost: ~0.5–1 day. Risks: any hand-written parser of the old shape breaks — the only known
consumers are agents reading prose-wise and `PlatoNavigationMcp.Tests`, both cheap to
update; keeping `format` opt-in for existing tools removes even that.

### P4 — `plato_vocabulary`: one call answers "what can I say about type X"

The deterministic core of the "examples / stay-on-vocabulary" seed idea, and the main
anti-hallucination lever: agents invent APIs when discovering the real surface costs six
calls; make it cost one.

What it does: given a type/interface name, returns a single card:

```
plato_vocabulary
  name: string          // "Vector3", "IntervalLike", "Curves"
  include?: string[]    // subset of ["fields","interfaces","implementers","functions-taking",
                        //  "functions-returning","operators"]; default all
  limit?: int           // per section, default 30
→ (compact format by default)
  Type Vector3 @ stdlib/vectors.plato:12  [id 88]
  fields:      X: Number, Y: Number, Z: Number
  implements:  Vector<Self>, Numerical, Deformable3D ...
  functions taking Vector3 (94):   412 Dot(a,b): Number · 415 Cross(a,b): Self · ...
  functions returning Vector3 (61): ...
  operators:  + - * / == (via Numerical)
```

Why tokens improve: replaces the SKILL.md chain (search → definition → outline of the
defining file → references filtered by kind → source) — typically 4–7 calls and 3–10 K
tokens of partially relevant lists — with one call whose response is bounded by `limit`.
Rough claim: ~6 calls / ~8 K tokens → 1 call / ~600–900 tokens for a mid-sized type.
Speed: saves the round trips, but the real speedup is fewer wrong-API compile failures,
each of which costs a full P1 iteration.

Implementation sketch (all data already in `NavigationIndex`):

- Fields, interfaces: the type's `DefRecord` children via `Owner` + the `Signature` strings;
  implements-clause refs are `RefKind.Type` refs inside the type's `DeclSpan`.
- Functions taking/returning: for every `RefRecord` with `Kind == Type` and
  `Targets ∋ typeDefId`, find the enclosing `Method`/`Function` def by `DeclSpan`
  containment (`FileId` + span interval; precompute a per-file interval list once per
  index generation). Classify param-position vs return-position by comparing the ref span
  against the function's signature text — or simpler and nearly as good: substring-check
  the already-stored `Signature` (`": Vector3"` at end = returns; else takes). Ship the
  simple version, note the imprecision.
- Implementers of an interface: `RefKind.Type` refs to the interface whose enclosing def is a
  `Type` and whose span sits in the implements clause — the binder already resolves these.
- New `VocabularyMcpTools.cs`; ranking = library functions first, then by reference count
  (popularity), so truncation keeps the idiomatic core.

Cost: ~1–1.5 days (the span-containment owner map is the only new index-side machinery).
Risks: param/return classification by signature substring is heuristic (generic positions,
`Self`) — label the sections honestly and accept a few misfiles; response ranking needs one
tuning pass against real questions.

### P5 — usage examples: upgrade `plato_references` with body-snippet mode

The rest of the "examples" seed idea. A ranked list of *real call sites with context* is
the deterministic 90% of "show me how this is used"; the judgment layer stays in the agent.

What it does: `plato_references` gains `snippet: int` (0 = current one-line behavior,
n = ±n surrounding lines trimmed to the enclosing function) and `bodiesOnly: bool`
(filter to `RefKind.Value` refs inside `.library.plato` files — usage, not declaration).
Plus ranking: prefer refs from distinct files and distinct enclosing functions, so
`limit: 5` yields five *different* idioms rather than five lines of one algorithm. When
`stdlib-tests` is indexed, test-file refs are tagged `test: true` — free law/example
material for the test-writing workflow.

Why: today an agent wanting three real examples takes the references list (file + one
line), then makes 2–3 `plato_source` calls for context — ~4 calls, and the one-line
context regularly isn't enough to see the idiom. After: 1 call, ~400–700 tokens.

Implementation sketch: `LookupMcpTools.References` already has everything —
`Results.Ref` extends from `Line(...)` to a snippet slice of
`context.Index.Snapshot.Files[r.FileId].Text`; enclosing-function trim reuses P4's
span-containment map; diversity ranking is a `GroupBy(enclosing).Select(First)` before
truncation. Cost: ~0.5 day (after P4's owner map exists). Risks: none structural; snippet
size discipline matters (cap at ~8 lines each, else this becomes a token leak instead of a
fix).

### P6 — conformance mapping: C# build errors → .plato definitions (v2, not v1)

What it does: `plato_conformance(symbol?, category?, limit?)` reads the latest recorded
build log / `docs/status-report-snapshot.json` (written by
`tools/record-csharp-build-errors.py` and `dotnet-build-record.ps1`), extracts per-error
type/member names from the C# error text (`CS1061: 'Vector3' does not contain a definition
for 'Slerp'` — receiver + member are in the message), resolves them through the navigation
index to `.plato` def ids/locations, and returns the burn-down grouped by category with
jump-to targets.

Why: the ~282-error forward-conformance backlog is today consumed as raw counter JSON plus
manual grepping of Generated C# — each "which .plato function owns this error" question is
a multi-step detour. This makes the burn-down list one call and directly feeds the priority
order (CS1061 x115 first). It does **not** speed the build itself; wall-clock win is the
triage, not the compile.

Why v2: mapping precision is genuinely uncertain — the C# writer's naming must be reversed
(receiver type + member name usually suffices for CS1061/CS0535 since Plato names pass
through, but CS0019/CS1503 sites need the file/line of the Generated file plus a
name-nearest-def heuristic). Do CS1061 + CS0535 + CS0246 first (they carry names in the
message and cover the bulk), report the rest unmapped with the raw line. Cost: ~1–1.5 days.
Risk: heuristic mapping mislabels some sites; every result carries the raw error line so a
wrong mapping is visible, not silent.

---

## 5. LLM-flavored seeds: where they actually land

- **Simplification** — a prompt skill ("plato-simplify"): given a function id, the skill
  calls P4 (vocabulary of the types involved) + P5 (usages of the callees) and rewrites,
  then verifies with P1. Server contribution is P4+P5; no new server feature.
- **Examples** — delivered deterministically by P4 + P5; residual judgment ("which example
  is pedagogically best") stays with the agent for free.
- **Test/law generation** — a prompt skill layered on P4 (interface obligations are in the
  card: unimplemented obligations come from LINT001 via P1) + P5's `test: true` refs as
  style precedent, verified by P1. Revisit a deterministic `plato_laws` only if the skill
  demonstrably flounders.

## 6. Accompanying non-server work (cheap, do alongside v1)

- **Rewrite `~/.claude/skills/plato-mcp/SKILL.md`**: the loop becomes
  *edit → `plato_check` → fix → `plato_check`*, with navigation tools for discovery;
  delete the reload rules once P2 ships. (The roots are already correct — see §2; no fix
  needed there.) Half the token win of P1/P2 is only
  realized when the skill stops teaching the old workflow.
- **`check-stdlib-fast.ps1` stays** as the cold-start / CI / no-server fallback and the
  final-gate battery; add a header comment pointing agents at `plato_check` for the inner
  loop.

---

## 7. Ranking and build order

Scores: wall-clock / tokens / cost (H = high impact or high cost).

| # | Proposal | Wall-clock | Tokens | Cost | Order |
|---|---|---|---|---|---|
| P1 | `plato_check` (+ style rules) | **H** (~10× per iteration) | M | ~2 d | 1 |
| P2 | staleness auto-reload | M (removes a call + a failure mode) | M | ~0.5 d | 2 |
| P3 | compact formats | L | **H** (2–3× on heavy responses) | ~1 d | 3 |
| P4 | vocabulary card | M (fewer wrong-API iterations) | **H** (~6 calls → 1) | ~1.5 d | 4 |
| P5 | usage snippets | L–M | M | ~0.5 d | 5 |
| P6 | conformance mapping | M (triage only) | M | ~1.5 d | 6 |

**Minimal v1 cut (~3 days): P1 + P2 + the SKILL.md rewrite.** That alone converts the
per-iteration cost from "two cold dotnet processes + a mandatory reload call + console
scraping" to "one warm call returning scoped findings." P3–P5 are the token wave and ship
together as v1.1 (P4 and P5 share the span-containment owner map; build it once). P6 rides
whenever the conformance burn-down becomes the active mission.

First task inside P1, before any tool code: measure warm `new Compilation(asts)` +
`TypeChecker.CheckAll()` on the current stdlib from a scratch harness. That number decides
whether v1 ships whole-corpus checks or needs the scoped-`CheckAll` increment immediately,
and every latency claim above should be restated against it in the PR description.

## 8. Open questions

1. Warm check latency (above) — the one number that can reorder this plan.
2. Does the planned stdlib reorg (`.types.plato` rename + foundation/geometry/graphics/
   future folders, `docs/plato-stdlib-reorg-plan-2026-07-30.md`) change enumeration?
   `SourceSnapshot.FromDirectories` and this server must follow whatever
   recursive/per-folder policy the reorg lands (Plato.CLI is TopDirectoryOnly today);
   P1 should take its file enumeration from one shared helper so the two cannot drift.
3. Ratchet ceiling ownership: today it is a constant in `ForwardStdLibCheckerTests.cs`
   (currently 0). If `plato_check` reports deltas, the ceiling should live in one place
   both consumers read (a small JSON beside the test, or keep 0-as-floor and drop the
   knob entirely).
4. The known `Ara3D.MCP` `ToolRunner` envelope bug (`ok:false` with `isError:false`) —
   worth fixing while adding tools, since every new tool inherits it and every agent pays
   a documentation rule for it.
