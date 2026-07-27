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

Not built yet, and nothing here changes when it is. Whole-corpus parse and bind take well under a
second, so rebuild-the-world *is* the incremental algorithm; the only optimization v2 needs is a
per-file parse cache keyed by content hash. Per-file *rebinding* is not on the table — the binder
builds shared scopes across all files before resolving any body.

An update produces a new `NavigationIndex` and consumers swap their reference. A file watcher
belongs in the consumer or the CLI, not in this library (decision D10).
