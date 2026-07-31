# Plato Navigation Index — Plan & Decisions

**Status:** IMPLEMENTED 2026-07-27 — M0–M5 delivered as `submodules/Plato/Plato.Navigation`
(+ `.CLI`, `.Tests`); decisions captured in §14; tracker `plato-236`. See §15 for what the build
found that this plan had wrong.
**Date:** 2026-07-27 (v2 — claims code-verified, V2 continuous-update requirement folded in)
**Audience:** Christopher (decisions) → implementing agent (build)
**Related context:** `submodules/Plato/vscode-plato` (syntax only); historical `PlatoVSIX` deleted; binder lives in `PlatoCompiler`

---

## 0. What changed in v2 of this plan (evaluation summary)

Every load-bearing claim was checked against the code; the plan is now grounded with file:line references. Substantive changes:

1. **Measured scale changes the architecture.** `stdlib-legacy` is 28 files / 4,465 lines; `stdlib-legacy-tests` adds 6 files; the largest corpus in the repo (`stdlib`) is 70 files / 13,406 lines. Full CLI *compilation* of stdlib-legacy runs in seconds; parse + bind alone will be well under that. Consequence: the right "incremental" strategy for continuously changing files is **rebuild-the-world with a per-file parse cache**, not per-file incremental rebinding. This collapses the hardest v2 problem into a design constraint on the v1 API (§5, §6).
2. **New requirement folded in:** V2 must handle a continuously updated set of source files (IDE buffers, watch mode). v1 does not implement it, but the v1 API is shaped so v2 is purely additive (§6). Cost of the constraint now: near zero. Cost of retrofitting later: an API break for every consumer.
3. **Binder is *more* tolerant than v1 of this plan assumed** — `SymbolFactory` accumulates `Errors` and continues per-node (`InternalResolve` try/catch, `SymbolFactory.cs:416-420`); only `Compilation` halts hard on any binder error (`Compilation.cs:75-79`). But three hard-abort paths exist and must be wrapped (§4, D3).
4. **New gap found:** `ParameterDef` is never recorded in `SymbolsToNodes` (`SymbolFactory.cs:172-177` binds it without recording the AST node) — parameters have no spans today. One-line fix.
5. **D5 recommendation flipped** to a tiny recording hook inside `SymbolFactory.ResolveType` instead of an extraction-time AST pass. Reason: the extraction pass must *replicate* type-parameter scoping (the binder pushes/pops `TypeBindingsScope` around each type's members, `SymbolFactory.cs:478-546`) and would silently diverge; the hook reuses the binder's actual resolution for ~3 additive lines with no control-flow change. Byte-identity gates (`regen-generated.ps1`, conformance 204/204) prove no emitter drift. See D5 for the trade-off.
6. **SQLite cut from v1 entirely; correctness harness promoted** to its own milestone before export. "Proven correct across the whole stdlib" is a top-level goal; the validation section now specifies an exhaustive-sweep harness (§10), not spot checks.
7. **Estimate drops** from ~1–2 weeks to ~4–7 days (§11) due to the cuts and the verified reuse surface.

---

## 1. Motivation

We need a **reusable, navigatable index of a Plato codebase** that other systems can consume:

| Consumer | Needs from the index |
|----------|----------------------|
| **MCP tools** | Symbol search, go-to-def, find-refs, outline — for agents; concurrent reads |
| **VS Code / Cursor navigator** | DefinitionProvider, ReferenceProvider, DocumentSymbol; later: unsaved-buffer support |
| **Documentation tool** | Stable links from docs → source spans; "where is X defined / used" |

Today there is no such layer. `vscode-plato` is TextMate-only. `Plato.ContextExport` dumps declaration text (parse-only, not bound). The compiler binder already resolves names; it is not exposed as a queryable database.

**Non-functional goals (from the sponsor):** build fast; efficient; reusable from MCP and IDE; minimally invasive to other projects; proven correct across the entire `stdlib-legacy` standard library; V2 must handle a continuously updated set of source files.

---

## 2. Desired end state

A **new project** under `submodules/Plato/` (name TBD — see D1) that:

1. Takes an immutable **source snapshot** (set of file path + text + content hash) — with a convenience loader for folder roots of `*.plato` (recursive, like `ContextExport`, `Program.cs:41`; note `Plato.CLI` is non-recursive, `Program.cs:98`).
2. Parses and **binds** using existing libraries (`Plato.AST`, `Plato.Compiler` / `SymbolFactory`, Parakeet). No second resolver.
3. Materializes an immutable **navigation index**: files, definitions, references, spans, name indexes. Immutability ⇒ thread-safe reads for free (MCP concurrency).
4. Exposes a small **query API** (in-process) and a thin **CLI** to build/export the index (JSON).
5. Does **not** implement MCP, VS Code, or docs UI itself — those are later consumers.

**Done means (for the index project):**

- [x] Library builds against current Plato compiler projects.
- [x] Indexes `stdlib-legacy` + `stdlib-legacy-tests` end-to-end (34 files / 5020 lines → 4307 defs, 9708 refs, 0 diagnostics).
- [x] Queries work: definition at file+position, references of a def, search by name, per-file outline.
- [x] **Exhaustive correctness harness green** (§10): 7565 identifiers classified, zero unexplained; round-trip invariants hold. 25/25 tests.
- [x] Exported JSON index reloads to query-identical results (and re-exports byte-identically).
- [x] Build time measured: cold 917 ms (parse-dominated), warm full rebuild 590 ms — inside the < 2 s budget.
- [x] README at `submodules/Plato/Plato.Navigation/README.md`.

---

## 3. Recommendation (default path)

**Build a bind-only navigation library + thin CLI.** Reuse the existing binder; do not invent a second resolver; do not start with a full LSP.

```text
SourceSnapshot (paths + texts + hashes)          ← NEW, tiny; FS loader on top
      │
      ▼
  Parse (CommonParsers.PlatoParser)              ← exists (Plato.CLI/Document.cs, ContextExport ParseFile)
      │
      ▼
  SymbolFactory.CreateTypeDefs                   ← exists (name resolution, SymbolFactory.cs:425)
      │
      ▼
  NavigationBuilder (NEW)                        ← extract tables + fix gaps (§4)
      │
      ▼
  NavigationIndex (NEW, immutable)               ← query API + generation stamp
      │
      ├── MCP server (later)
      ├── VS Code extension host (later)
      ├── Doc generator (later)
      └── v2 IncrementalIndexer (later, additive) ← parse cache + full rebind + atomic swap
```

### Why this path (verified)

- `SymbolFactory.SymbolsToNodes` (`SymbolFactory.cs:32`) already maps symbols → AST nodes for: type/library defs (`:433,456`), fields (`:496`), methods (`:503`), and — via the `Resolve()` wrapper (`:190-196`) — every body expression including each `RefSymbol`.
- **Ref sites come for free:** `ToReference()` creates a *fresh* `RefSymbol` per resolution (`Definitions.cs:131,151,241,391`), so `SymbolsToNodes` holds one entry per reference occurrence. The ref table is a filter: `SymbolsToNodes.Where(kv => kv.Key is RefSymbol)` with `((RefSymbol)kv.Key).Def` as the target.
- Every `AstNode` is an `ILocation` (`Ast.cs:8-15`); `ParserRange` supplies begin/end char offsets, line/column, and `FilePath` (`ParserRange.cs:33-55`). Nothing new needed for spans.
- Full `Compilation` additionally runs reification, type relations, function analyses, and swallows exceptions into a log (`Compilation.cs:143-150`) — **not required** for navigation and it halts on the first binder error (`Compilation.cs:75-79`). Call `SymbolFactory` directly.
- Type checker / TIR **not** required for go-to-def / find-refs.
- Heuristic regex search is a dead end if this index is the shared foundation for MCP + IDE + docs.

### Closest existing code (reuse, don't fork)

| Piece | Path | Role |
|-------|------|------|
| Parse file | `Plato.CLI/Document.cs`; `Plato.ContextExport/Program.cs` `ParseFile` (:99) | Text → AST |
| Binder | `PlatoCompiler/Symbols/SymbolFactory.cs` | Names → defs/refs; `SymbolsToNodes` |
| Def/ref model | `Symbols/Definitions.cs`, `Expressions.cs` | `TypeDef`, `FunctionGroupDef`, `RefSymbol`, … |
| Location | `AstNode : ILocation` → `ParserRange` (Parakeet) | File + char offset + line/col |
| Symbol → span helper | `Compilation.GetAstNode` (:370), `Compilation.LogPosition` (:384) | Pattern to copy |
| Decl dump (non-nav) | `Plato.ContextExport` | Precedent for a small CLI tool project; recursive file walk |

### Project references (library)

- `Plato.AST`
- `Plato.Compiler` (`Plato.Compiler.csproj`)
- `parakeet/Parakeet`, `parakeet/Parakeet.Parsers`
- `Ara3D.Logging`, `Ara3D.Utils`

**Do not** reference C#/TS/Rust/GLSL writers. Keep the library free of MCP/VS Code/file-watcher dependencies.

### Efficiency notes (v1 targets)

- Corpus is tiny (4.5–13K lines): favor **flat arrays of records with integer ids** over object graphs. Per file: defs/refs sorted by begin offset → position hit-test is a binary search for the smallest containing span. Name lookup: dictionary name → def-id list (exact), plus a sorted name array for prefix search.
- Intern file paths to ids; records carry ids, not strings — this is also the export shape (§5).
- Budget: full snapshot build < 2 s (expect ≪ 1 s); queries O(log n) or O(1). Measure at M0 and record in README.

---

## 4. What the new project must add (gaps — all verified against code)

| Gap | Evidence | Why it matters | Fix |
|-----|----------|----------------|-----|
| **Bind-only entry point** | `Compilation` halts on any binder error (`Compilation.cs:75-79`) and swallows exceptions (`:143-150`) | Partial results needed for IDE/MCP | Call `new SymbolFactory(logger).CreateTypeDefs(decls)` directly; consume `TypeDefs`, `SymbolsToNodes`, `Errors` |
| **Binder hard-abort paths** | `CreateTypeDefs` returns **null** on an unrecognized member decl (`SymbolFactory.cs:508-510`); throws on null inherits/implements (`:562-572`); `AddToGroupDefinition` throws (`:74`); ctor helpers can throw | One bad decl can still kill the whole bind | Wrap the bind call; on abort, degrade to parse-level index (outline/search still work) and report which decl aborted |
| **Type-name reference sites** | `ResolveType` resolves but records nothing (`SymbolFactory.cs:128-170`); most call sites are direct (field/param/return types `:494,501`, inherits/implements `:565,572`, nested type args `:168`), bypassing the recording `Resolve()` wrapper | Find-refs for types misses all signature sites — the docs/MCP killer | **D5:** ~3-line recording hook inside `ResolveType` (recommended), or extraction-time AST pass (fallback) |
| **Parameter spans** (new in v2) | `Resolve(AstParameterDeclaration)` binds but never records the node (`SymbolFactory.cs:172-177`) | Param go-to-def/outline impossible | One line: record `ParameterDef → AstParameterDeclaration` in `SymbolsToNodes` (or record extraction-side from `AstMethodDeclaration.Parameters`) |
| **Position → symbol** | Only symbol → AST exists | Core IDE query | Invert spans per file; binary-search smallest containing range |
| **Function refs → groups** | Calls bind to `FunctionGroupDef`, not one overload | Go-to-def UX | Index the group; v1 returns all overloads (D4); ranking later via `FunctionGroupCallAnalysis` |
| **Stable IDs** | `Symbol.Id` is process-local | Persistence + cross-build diffing | Persist `kind + owner + name + signature` (+ span as secondary) |
| **Synthetic defs** | Generated ctors (`:464,633`), sum-case factories (`:540`), single-field casts (`:642`), tuple ctors (`:654`) have no source spans of their own | Go-to-def must not point at nothing | Mark `synthetic`; point at owning type's span |
| **Project roots** | CLI `GetFiles` non-recursive (`Plato.CLI/Program.cs:98`); ContextExport recursive (`Program.cs:41`) | Predictable input set | SourceSnapshot loader: explicit roots, recursive `*.plato`, deterministic ordering |

---

## 5. Proposed API surface (library)

Conceptual — exact names flexible. The shape is the **v2 contract** (§6): pure function from snapshot to immutable index.

```csharp
// Input: immutable, filesystem-decoupled. This is what makes v2 (and unsaved IDE buffers) additive.
SourceSnapshot SourceSnapshot.FromDirectories(IEnumerable<DirectoryPath> roots);        // recursive *.plato
SourceSnapshot SourceSnapshot.FromTexts(IReadOnlyDictionary<FilePath, string> texts);   // buffers, tests
// SourceSnapshot = ordered set of (FilePath, Text, ContentHash)

// Build: pure function of the snapshot. No hidden file reads.
NavigationIndex NavigationIndex.Build(SourceSnapshot snapshot, ILogger logger = null);

// Queries — all thread-safe (index is immutable):
SymbolHit? FindAt(FilePath file, int offset);                    // + (line, column) overload
IReadOnlyList<DefRecord> GetDefinitions(SymbolHit hit);          // 1..N (overload groups)
IReadOnlyList<RefRecord> FindReferences(DefId id);
IReadOnlyList<DefRecord> Search(string name, SearchKind kind = SearchKind.All);  // exact + prefix
IReadOnlyList<DefRecord> Outline(FilePath file);                 // parse-level; works even when bind failed

// Staleness / identity:
string Generation { get; }                                        // hash over ordered file hashes
IReadOnlyDictionary<FilePath, string> FileHashes { get; }
IReadOnlyList<FileStatus> Files { get; }                          // parsed | parse-failed | bound | bind-degraded
IReadOnlyList<Diagnostic> Diagnostics { get; }                    // parse + resolution errors with spans
```

**Records minimally include:** id, kind, name, signature (optional), file id, begin/end char offset **and** line/column, owner type/library, flags (`synthetic`, `partial`). Flat, integer-id-based — the in-memory shape *is* the JSON shape.

**Persistence v1: JSON only.** At 4.5K lines the index is tens of KB and loads in milliseconds; SQLite adds a dependency and a schema for zero benefit at this scale. Revisit only if a corpus grows ~100×. (D2)

---

## 6. V2 requirement: continuously updated source files

**Status: DONE 2026-07-27** — `IncrementalIndexer` + `ParseCache` in `Plato.Navigation`, tracker `plato-238`. Purely additive: `NavigationIndex.Build` and `BoundSnapshot.Create` are unchanged and every v1 test passes untouched. `PlatoNavigationMcp.Reload()` runs through it and `plato_index_status` / `plato_reload` report cache hits and misses. Reload of an unchanged corpus is 61 ms (34 files) / 59 ms (70 files) against a 706 / 779 ms full build; a one-file edit is 69 / 68 ms. The gate is that `Update(s)` is byte-identical to `Build(s)` across a first build, an unchanged snapshot, an edit, an add and a removal. Two things the sketch below did not say: the cache key must be **(path, content hash)**, because an AST carries its file path in every range; and there is no same-generation short circuit, because rebinding a cached corpus is cheap and skipping it would make the identity gate vacuous.

V2 must serve navigation over files that keep changing (editor buffers, file watcher). **v1 does not build this — v1 makes it additive.** Verified basis: whole-corpus parse+bind is sub-second at current scale, so rebuild-the-world *is* the incremental algorithm.

**Design constraints imposed on v1 (all already reflected in §5):**

1. **Snapshot in, index out.** `Build` is a pure function of `SourceSnapshot`; no file reads inside the pipeline. Unsaved buffers = `FromTexts`, no new pathway.
2. **Immutable index, atomic swap.** An update produces a *new* `NavigationIndex`; consumers (MCP server, IDE host) hold a reference and swap it. No locking, no invalidation protocol, no torn reads.
3. **Generation stamp + per-file hashes** let any consumer detect staleness cheaply and answer "index built from what?"
4. **Per-file parse caching is the only v2 optimization needed.** Binding is whole-program (scopes span all files — `SymbolFactory` binds every library/type into shared scopes before resolving bodies, `SymbolFactory.cs:425-547`), so per-file *rebind* would be major compiler surgery for zero benefit at this scale. Parse results (per-file ASTs) are cacheable by content hash today because parsing is per-file and independent.

**V2 itself (later, additive — nothing in v1 changes):**

```csharp
sealed class IncrementalIndexer            // owns a parse cache keyed by content hash
{
    NavigationIndex Current { get; }
    NavigationIndex Update(SourceSnapshot next);   // reparse changed files only; full rebind; swap
}
```

Watch mode (FileSystemWatcher + debounce) lives in the consumer or the CLI, **not** in the core library (D10).

---

## 7. Non-goals (v1)

- Full LSP server
- Live diagnostics / lint streaming
- Precise single-overload go-to on ambiguous calls
- **Executing** continuous updates (v2 — but v1 API is shaped for it, §6)
- Per-file incremental *rebinding* (rejected permanently at current scale, §6)
- SQLite persistence (rejected for v1, revisit on ~100× corpus growth)
- Restoring or depending on deleted `PlatoVSIX`
- Changing Plato language semantics

---

## 8. Suggested milestones

| # | Milestone | Outcome |
|---|-----------|---------|
| M0 | Skeleton + `SourceSnapshot` + parse + **bind-only wrapper** over stdlib-legacy; measure parse/bind timings; catalogue actual abort behavior | Foundation + perf numbers; degraded-mode policy grounded in observed behavior |
| M1 | **Def table** with spans (incl. `ParameterDef` fix) + name search + outline | Types/libraries/methods/fields/params navigable |
| M2 | **Ref table** (RefSymbol filter) + **type-site refs** (D5 hook) + position hit-test | Go-to-def + find-refs across bodies *and* signatures |
| M3 | **Correctness harness** (§10): exhaustive sweep + invariants over stdlib-legacy + stdlib-legacy-tests | "Proven across the board" — the sponsor's gate |
| M4 | JSON export/reload + CLI `build` / `def` / `refs` / `search` / `outline` | Consumable by other tools; reload-equivalence test |
| M5 | README + polish; record v2 contract | Hand-off ready |

Type-site refs moved into M2 (with the hook it is nearly free); the harness is promoted to its own milestone **before** export, because correctness is a stated top-level goal and the harness will shake out span bugs that export would otherwise fossilize.

MCP, VS Code providers, and doc linker remain **separate follow-on projects** consuming this library (or its exported index).

---

## 9. Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Binder hard-abort paths (null return / throws, §4 row 2) | High | Wrap bind; degrade to parse-level index; harness counts bound vs degraded files |
| Type-param scope divergence **if** D5 fallback (AST pass) chosen | High for correctness goal | Prefer the `ResolveType` hook (reuses real resolution); if the pass is chosen, replicate the 2-level lookup (owner's type params, then global) and harness-diff against binder results |
| Hook side-effects on `SymbolsToNodes` consumers | Low | `TypeExpression` entries already occur via `Resolve()` (`SymbolFactory.cs:190-196` on the `AstTypeNode` case) so no new key category; verify `Compilation.Symbols` / `SymbolWriterXml` consumers; conformance 204/204 + `regen-generated.ps1` byte-identity prove no emitter drift |
| Overload ambiguity | Medium | Return group (D4); document; refine later via `FunctionGroupCallAnalysis` |
| `Scope.Bind` overwrites shadowing history | Medium | Accept for v1; note in README |
| Index staleness vs source | Low (was Medium) | Resolved by design: generation stamp + per-file hashes + snapshot rebuild (§6) |
| Coupling to binder internals | Medium | All extraction lives in the Navigation project; compiler edits limited to the two 1–3-line recording additions (D5, param spans), both additive and gate-protected |

---

## 10. Validation — "proven correct across the board"

Spot checks are not enough for the stated goal. The harness is exhaustive over the corpus:

| Check | How |
|-------|-----|
| **Exhaustive identifier sweep** | Tokenize every `stdlib-legacy` (+ `stdlib-legacy-tests`) file; for every identifier token, `FindAt` must classify it: def-site, resolved ref, keyword/builtin, or *known*-unresolved (binder `Errors` entry). **Zero unexplained** outcomes. Committed golden summary (counts per file/kind); drift = review |
| **Round-trip invariants** | ∀ def d: `FindAt(d.span)` hits d. ∀ r ∈ `FindReferences(d)`: `GetDefinitions(FindAt(r.span))` contains d. ∀ def/ref: source text at span == recorded name |
| **Go to def** | Known positions → exact file/span (small curated set for readable failures) |
| **Find refs** | High-fan-in type (e.g. `Array`, `Number`) → includes signature sites (M2) and body sites; count golden-filed |
| **Search** | `Search("Array")` exact + prefix results golden-filed |
| **Reload equivalence** | Build → export JSON → load → structural equality of all tables + identical answers on the sweep |
| **Degraded mode** | Corpus copy with one injected parse error and one bind error → other files still fully navigable; statuses correct |
| **Perf budget** | Full build < 2 s (record actual); `stdlib` (70 files / 13,406 lines / 1,125 types) as **scale smoke only** — measure, don't gate (its bind-ability under current SymbolFactory is unverified; it is a vocabulary corpus) |
| **Compiler non-regression** | `regen-generated.ps1` diff-clean + conformance 204/204 after the D5/param hooks (proves the additive edits changed nothing) |

---

## 11. Effort (order of magnitude)

One engineer familiar with the Plato tree:

| Slice | Estimate |
|-------|----------|
| M0–M2 (snapshot, bind wrapper, defs, refs incl. type sites, hit-test) | ~2–3 days |
| M3 (exhaustive harness) | ~1–2 days |
| M4–M5 (JSON export/CLI, README) | ~1–2 days |
| **Total to hand-off-ready library** | **~4–7 days** |

Cut from v1 estimate: SQLite gone, milestones merged, reuse surface verified (less discovery risk). MCP / VS Code / docs consumers: each additional days–week, **after** this library exists.

---

## 12. Decisions you need to make

Answer these before (or as first task of) the implementing agent. Defaults are marked ★.

### D1 — Project name & location

- ★ `submodules/Plato/Plato.Navigation` (+ `Plato.Navigation.CLI`)
- `submodules/Plato/Plato.Index`
- Something else: _______________

### D2 — Persistence for v1

- ★ JSON export only; in-memory API is primary (corpus is tens of KB indexed; SQLite = dependency + schema for no benefit at this scale)
- SQLite anyway (choose if you expect a 100× corpus soon)
- In-memory only; persistence deferred

### D3 — Partial failure policy

- ★ Index all parsed files; bind via wrapped `SymbolFactory` (it accumulates `Errors` and continues — verified); on a hard abort (§4 row 2) degrade that build to parse-level (outline/search still work) and surface the offending decl; per-file `FileStatus` + `Diagnostics` always populated
- Require clean parse+bind of entire roots (fail closed)
- Harden `SymbolFactory` abort paths themselves (small compiler change; only if you want the compiler improved too)

### D4 — Function go-to-def UX

- ★ Return **all overloads** in the function group (and/or all `MethodDef` locations)
- Best-effort single overload via `FunctionGroupCallAnalysis` in v1 (more work, more risk)
- Types/fields/params only in v1; functions deferred

### D5 — Where to record type-name references  *(recommendation flipped in v2 — see §0.5)*

- ★ **~3-line recording hook inside `SymbolFactory.ResolveType`** (`SymbolFactory.cs:169`): record `TypeExpression → AstTypeNode` into the existing `SymbolsToNodes` before returning. Reuses the binder's real resolution incl. type-parameter scoping and nested type arguments; additive, no control flow touched; guarded by conformance 204/204 + `regen-generated.ps1` byte-identity. Same treatment (1 line) for `ParameterDef` spans
- Extraction-time AST pass over `AstTypeNode` (zero compiler edits, but must replicate the type-param/global 2-level scope lookup and can silently diverge — if chosen, harness must diff its resolutions against the binder's)
- Defer type find-refs entirely (kills the docs/MCP use case — not recommended)

### D6 — Relationship to `Plato.ContextExport`

- ★ Leave ContextExport as agent-context text dump; Navigation is separate
- Extend ContextExport to also emit navigation JSON (couples two purposes)
- Retire/replace ContextExport later (out of scope for v1)

### D7 — First consumer after the library

Pick priority so the API is dogfooded early:

- MCP tool pack (fits studio's existing `Ara3D.Studio.Mcp.Http` know-how)
- ★ VS Code / Cursor Definition + References providers (extends `vscode-plato`)
- Documentation cross-linker
- Library-only until API feels right

### D8 — Tracker

- ★ File a `ready` tracker issue before implementation (repo process)
- Proceed as spike without tracker until evaluated
- File as `idea` only for now

### D9 — Scope of "codebase"

- ★ `stdlib-legacy` + `stdlib-legacy-tests` as the correctness-gated golden targets; `stdlib` as ungated scale smoke; arbitrary extra roots via CLI args
- `stdlib-legacy` only
- Arbitrary folders only (no default stdlib injection)

### D10 — Watch-mode ownership (new; V2 prep)

- ★ Core library stays watcher-free; v2 adds `IncrementalIndexer` (parse cache + full rebind + swap); FileSystemWatcher + debounce live in the consumer or CLI
- CLI grows a `watch` command in v1 (small scope add; only if you want a demo loop immediately)
- Defer all of it (risk: v1 API drifts filesystem-coupled — the §5/§6 constraints then need enforcement by review alone)

---

## 13. Instructions for the implementing agent

1. Read this doc and the user's answers to **§12 Decisions**.
2. Do **not** execute until decisions are filled (or user says "use defaults").
3. Follow repo tracker rules: if D8 says file an issue, create/close via `tools/track.py`; name the issue id in commit scopes.
4. Follow the Plato repo mission protocol (`submodules/Plato/CLAUDE.md`): maintain `PROGRESS.md`, write `COMMIT_MSG.txt`, never touch `parakeet/`, run `check-all.ps1` once at the end. The frozen-V1 artifacts and `Generated/` byte-identity gates apply to the D5/param compiler edits.
5. Compiler edits are limited to the two additive recording lines (D5 + param spans) — nothing else in `PlatoCompiler` changes. All extraction/query logic lives in the Navigation project.
6. Reuse parse/bind code; do not copy the compiler. Keep the library free of MCP/VS Code/watcher dependencies.
7. Honor the §5/§6 shape: `Build(SourceSnapshot)` pure; no file reads inside the pipeline; immutable index.
8. Put scratch output under `.temp/`; durable notes in `docs/` or the project README.
9. When done: tick "Done means" in §2 (or the tracker issue), README with known limitations (overloads, synthetics, partial bind) and the v2 update contract.

---

## 14. Decision capture (fill in)

Answered 2026-07-27 by Christopher. Tracker issue: `plato-236`.

| ID | Choice |
|----|--------|
| D1 Name/location | ★ `submodules/Plato/Plato.Navigation` (+ `Plato.Navigation.CLI`) |
| D2 Persistence | ★ JSON export only; in-memory API primary |
| D3 Partial failure | ★ Index all parsed files; wrapped bind; degrade to parse-level on hard abort |
| D4 Function go-to | ★ Return all overloads in the function group |
| D5 Type-ref recording | ★ ~3-line recording hook inside `SymbolFactory.ResolveType` (+ 1 line for `ParameterDef` spans) |
| D6 vs ContextExport | **Non-default:** plan to retire/replace `Plato.ContextExport` later (direction set now; still out of scope for v1) |
| D7 First consumer | **Non-default:** MCP tool pack first (not VS Code) |
| D8 Tracker | ★ `ready` tracker issue filed — `plato-236` |
| D9 Codebase roots | ★ `stdlib-legacy` + `stdlib-legacy-tests` gated; `stdlib` ungated scale smoke; extra roots via CLI |
| D10 Watch ownership | ★ Core library watcher-free; v2 `IncrementalIndexer`; watcher in consumer/CLI |

**Approval:** ☑ custom (table above) — starred defaults except D6 and D7.

**Scope for the implementing session:** all milestones M0–M5.

### Consequences of the two non-default choices

- **D7 (MCP first)** shifts API pressure toward concurrent reads, name/prefix search, and a
  stable JSON/serializable result shape rather than LSP-style position providers. §5 already
  satisfies this (immutable index, flat integer-id records). No plan change; the CLI in M4 is
  the natural precursor to the MCP tool surface, so keep its verbs (`def`/`refs`/`search`/`outline`)
  shaped as tool calls: structured output, no interactive state.
- **D6 (retire ContextExport later)** means Navigation should be able to reproduce what
  ContextExport emits (declaration text per file). Cheap insurance in v1: make `Outline` carry
  each def's full declaration span (not just the name span) so a text dump is derivable.
  Retirement itself stays out of v1 scope; file a follow-on issue rather than touching it now.

---

## 15. What the build found that this plan had wrong (2026-07-27)

The architecture held. Five load-bearing details did not, and all of them were found by writing
the §10 sweep rather than by reading code — which is the argument for having promoted the harness
to its own milestone before export.

1. **§4/D5 said the recording hook could write into `SymbolsToNodes`. It cannot.**
   `TypeExpression` overrides `Equals`/`GetHashCode` **by value** (`TypeExpression.cs:38-45`), so a
   dictionary keyed by symbol collapses every occurrence of the same type into one entry — erasing
   exactly the per-site information find-refs needs. The risk table's "no new key category, so no
   drift" reasoning was right about drift and wrong about usefulness. Fixed with a side list
   (`SymbolFactory.TypeReferences`).

2. **Type variables returned before the hook.** `ResolveType` short-circuits on `$`-prefixed names
   (`SymbolFactory.cs:149`), so every `$T` site was invisible until the early return was recorded
   too. The parser's identifier range also excludes the `$`, so the index widens the span by one.

3. **Type parameters were never in `SymbolsToNodes` either.** The plan caught this for
   `ParameterDef` but not for `TypeParameterDef` — so `T` had a definition record from the AST and
   no way for a reference to reach it. One more additive line.

4. **Locals were unreachable.** `Resolve` records a wrapper node's symbol against the *wrapper*
   after the inner call already recorded it against the declaration, so the node stored for a local
   is the statement wrapper, not the `AstVarDef`. The builder unwraps.

5. **Operators arrive with the wrong span.** `AstBinaryOp.ToInvocation` (`Ast.cs:194`) synthesizes
   the function identifier with the *whole expression's* location, so `a + b` reached the index as
   a reference named "Add" spanning `+ b`. The span does start at the operator, so it is narrowed
   to the operator token and marked `RefKind.Operator`.

Confirmed as predicted: bind is cheap (44–53 ms for the whole corpus against ~900 ms of parsing —
so a parse cache really is the only v2 optimization worth having); `SymbolsToNodes` really does
hold one entry per `RefSymbol` occurrence; and the compiler hooks really are drift-free
(`regen-generated.ps1` 184/184 identical on both variants, conformance 205/205).

Not done, deliberately: match-expression binders are synthesized outside the recording path and are
not indexed. No file in `stdlib-legacy` uses `match`; `stdlib` does, so this is the first thing to
fix if the index is pointed at v3.
