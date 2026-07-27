# Plato.Navigation

A queryable navigation index over a snapshot of Plato source: go-to-definition, find-references,
per-file outline, and name search. It reuses the compiler's parser and binder — there is no second
resolver here, and there is no MCP server, IDE integration, or file watcher either. Those are
separate consumers that sit on top of this.

Plan and decisions: `docs/plato-navigation-index-plan.md` in the studio repo. Tracker: `plato-236`.

## Using it

```csharp
var index = NavigationIndex.Build(SourceSnapshot.FromDirectory("plato-src"));

var hit = index.FindAt(file, line, column);
var definitions = index.GetDefinitions(hit);          // 1..N — a call offers every overload
var references = index.FindReferences(definitions[0].Id);
var outline = index.Outline(file);                    // source order
var matches = index.Search("Vector", SearchKind.Prefix);
```

`Build` is a pure function of a `SourceSnapshot`, and nothing in the pipeline reads a file. Build a
snapshot from folders (`FromDirectories`), from explicit paths (`FromFiles`), or from text you
already hold (`FromTexts` — unsaved editor buffers use the same path as anything else).

The resulting index is immutable, so reads are thread-safe with no locking. `Generation` is a hash
over the ordered per-file content hashes: two snapshots with the same generation produce identical
indexes, and any consumer can check staleness with a string compare.

`NavigationJson.Write` / `.Read` persist an index as JSON, sources included, so a reloaded index
answers every query the original does.

### CLI

```bash
dotnet run --project Plato.Navigation.CLI -c Release -- stats --root plato-src --root plato-test-src
dotnet run --project Plato.Navigation.CLI -c Release -- search Number --kind exact --root plato-src
dotnet run --project Plato.Navigation.CLI -c Release -- refs plato-src/primitives.plato 4 5 --root plato-src
dotnet run --project Plato.Navigation.CLI -c Release -- export index.json --root plato-src
```

Positions are 0-based line and column; printed locations are 1-based.

## What the records mean

`DefRecord.NameSpan` is the identifier alone — what an editor highlights. `DeclSpan` is the whole
declaration, which is what a documentation dump would print. Both carry char offsets *and*
line/column, so a consumer never needs the source text to render a location. `Span.None`
(begin = -1) means a definition with no syntax of its own.

`RefRecord.Targets` lists every definition a name could mean. `RefKind` separates the three shapes
a reference takes: a `Value` name in an expression, a `Type` name in a signature or type argument,
and an `Operator` token.

## Known limitations

- **Overloads are not disambiguated.** A call binds to a function *group*, not to one overload, so
  go-to-definition offers all of them (decision D4). Narrowing this needs the type checker's
  `FunctionGroupCallAnalysis`, which navigation deliberately does not run.
- **Compiler-generated functions have no source.** Constructors, sum-case factories and implicit
  casts are synthesized by the binder, so references to them point at their owning type.
- **Operator spans cover the operator, not a name.** `a + b` is a genuine reference to `Add`, but
  the text under the span reads `+`. Marked `RefKind.Operator` so no consumer has to guess.
- **Some names have no definition to go to**: the `Self` type, type variables (`$T`) and the
  `default` keyword are compiler built-ins. They are indexed as references with an empty target
  list rather than dropped, so the correctness sweep can prove nothing went missing silently.
- **Match-expression binders are not indexed.** They are synthesized outside the binder's recording
  path. No file in `plato-src` uses `match`; `plato-src-v3` does.
- **Shadowing history is lost.** `Scope.Bind` overwrites, so an inner binding hides the outer one.
- **A hard bind abort degrades the whole build**, not one file — binding is whole-program. Parse
  failures are per-file. Either way `FileStatus` and `Diagnostics` say exactly what happened, and
  outline and search keep working from the AST alone.

## Correctness

`Plato.Navigation.Tests` gates the library against `plato-src` + `plato-test-src` (decision D9):

- an **exhaustive identifier sweep** — every one of the ~7.5k identifiers the parser produces must
  be a definition site, a reference site, or a name the binder reported it could not resolve;
- **round-trip invariants** — every definition is found at its own span, every reference resolves
  back to its targets, `FindReferences` is the exact inverse of the target lists, and the source
  text under every span reads as the record says;
- **degraded mode** — an injected parse error and an unresolvable name leave the rest navigable;
- **JSON round-trip** — reload is query-identical and re-export is byte-identical;
- **a build budget** — a warm full rebuild of the corpus (34 files, ~5k lines) runs in well under
  a second.

`plato-src-v3` (70 files) is a scale smoke test, not a gate.

The two compiler hooks this library depends on (recording type-name occurrences and parameter
spans, decision D5) are additive and guarded by `tools\regen-generated.ps1` byte-identity and the
conformance suite.

## Version 2: continuously updated sources

Built (tracker `plato-238`), and nothing in v1 changed to make room for it — `NavigationIndex.Build`
and `BoundSnapshot.Create` behave exactly as before.

```csharp
var indexer = new IncrementalIndexer();
var index = indexer.Update(SourceSnapshot.FromDirectories(roots));   // cold build
index = indexer.Update(SourceSnapshot.FromDirectories(roots));       // reload: reparses only edits
```

Parse dominates a build — on a warm process, ~700 ms of parsing against ~30-60 ms of binding — so a
per-file parse cache is the whole optimization. (A cold first parse in a fresh process reads much
higher, ~1800 ms for 70 files; that is JIT, not a steady-state number, and the table below is
best-of-two on a warm process.) Binding is whole-program — `SymbolFactory` builds scopes shared across all
files before resolving any body — so it is rerun in full every time, and per-file rebinding stays
off the table.

**The cache key is (path, content hash), not the content hash alone.** Every `AstNode` carries a
range whose file path comes from the `ParserInput` it was parsed with, so reusing an AST for a
different file with identical text would misattribute every span in it. Reuse is safe because the
binder only reads the AST: it writes to symbols (`TypeDef.IsUnique`, `MemberDef.Function`), never to
a node, so the mutable fields the AST does expose (`AstParenthesized.Inner`,
`AstTypeDeclaration.Cases`) are untouched by a bind.

`IncrementalIndexer.LastUpdate` reports files parsed, files reused, and parse/bind time;
`Cache.Hits`/`Misses`/`Count` accumulate over the session. `Cache.Retain` drops superseded versions
on every update, so an editing session does not grow the cache.

The gate is that `indexer.Update(s)` is byte-identical to `NavigationIndex.Build(s)` — compared
through the JSON export, which carries sources, defs, refs, spans, targets, file states,
diagnostics and generation — for a first build, an unchanged snapshot, an edit, an add and a
removal. There is deliberately no same-generation short circuit: a reload always rebinds, which
keeps that comparison meaningful and costs ~60 ms.

Measured with `Plato.Navigation.CLI bench` (best of two runs, snapshots built from in-memory text):

| corpus | v1 build | v2 cold | v2 reload, 0 changes | v2 reload, 1 file |
|---|---|---|---|---|
| `plato-src` + `plato-test-src` (34 files, 5020 lines) | 706 ms | 503 ms | 61 ms | 69 ms |
| `plato-src-v3` (70 files, 13234 lines) | 779 ms | 736 ms | 59 ms | 68 ms |

An update produces a new `NavigationIndex` and consumers swap their reference; `PlatoNavigationMcp`
is the worked example. A file watcher belongs in the consumer or the CLI, not in this library
(decision D10).
