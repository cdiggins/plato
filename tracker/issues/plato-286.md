---
id: plato-286
title: Define a smaller Plato stdlib core for fast testing
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [submodules/Plato/stdlib/README.md, submodules/Plato/CLAUDE.md, plato-285]
---

## Idea
Full forward `stdlib` is large (~84 files, 150 interfaces, 1100+ types). Lint and compile of the whole tree are heavier than needed for tight compiler/type-checker/codegen loops. Define a smaller **core** subset — foundation vocabulary only — structured so agents and CI can run fast tests against it without pulling geometry, imaging, physics, etc.

## Assumptions
- Most compiler regressions can be caught with primitives, core/algebra/collections/functional interfaces, numbers, vectors/matrices, and a thin sample of libraries (e.g. `core-algebra`).
- A core that drifts from full stdlib (forked copies) is worse than a slow full lint — core must be a **slice**, not a fork.
- Shipping `plato-src` / conformance remain separate concerns; this is about forward `stdlib` (and possibly a parallel slice of law tests).

## Design decisions
- **What is "core"** — README Foundation layer only vs Foundation + one geometry smoke file vs explicit allowlist manifest.
- **How it is structured** — (A) `stdlib-core/` folder that is the only input to fast lint, with full `stdlib/` importing/extending it; (B) single `stdlib/` tree + `core.manifest` listing files for fast runs; (C) tag/comment markers in files (`// @core`) filtered by CLI.
- **Dependency rule** — core must parse/resolve alone (self-contained like today's stdlib claiming self-containment via `primitives.plato`).
- **Who consumes it** — local agent loops only vs CI job `lint-core` as a required fast gate before full lint.

## Related
- [stdlib/README.md](../../submodules/Plato/stdlib/README.md) — Foundation layer already listed (primitives … color).
- [plato-285](plato-285.md) — physical split; core structure should not fight the chosen layout.
- [CLAUDE.md](../../submodules/Plato/CLAUDE.md) — `lint` / `check-all` workflow; fast core would sit under mission-protocol "iterate on one gate".

## Approaches
Short term: add `tools`/`CLI` support for `lint <folder> --manifest core.txt` (or a second folder path) listing Foundation files + the `core-*.library.plato` / `algebra-*.library.plato` files; document `dotnet … lint … --manifest core` as the agent tight loop.
Long term: `stdlib/core` as a real package other layers depend on; conformance smoke laws only on core; full suite nightly.
Adjacent: plato-285 (folders/manifest); timed benchmark of full vs core lint to size the win.

## Bedrock
Strengthens the **stdlib dependency layering seam**: Foundation becomes an enforceable closed set, not just a README table. **Verdict: simplest-along-the-grain** — must NOT duplicate sources into a second tree; prefer a manifest (or recursive package root) over copy-paste `stdlib-core/`.

## Done means
- [ ] Written definition of core membership (file list or layer rule)
- [ ] One command runs lint (or compile) on core alone in meaningfully less time than full stdlib (record numbers)
- [ ] Core resolves with 0 parse / 0 symbol errors in isolation
- [ ] Docs tell agents when to use core vs full lint

## Simplest possible implementation
Create `stdlib/core.manifest` (or `tools/stdlib-core-files.txt`) listing Foundation paths; teach `Plato.CLI lint` an optional manifest flag; stop there.
- Pros: no file moves; enforces the existing layer table; easy to adjust membership.
- Cons: two ways to invoke lint; manifest can rot if files rename (mitigate: CI check that manifest paths exist).

## Case against
- **Full lint is already the truth.** A green core with a red full tree creates false confidence; agents may skip full lint.
- **Manifest rot.** Renames and new foundation files forget the list; worse than slow-but-honest full runs.
- **Win may be small.** If parse/resolve is dominated by fixed overhead, not file count, core buys little — measure first.
- Verdict: **pursue** only after a timing spike (full vs foundation-only file list). If speedup ≥ ~2×, ship manifest; if not, **drop**. Structure recommendation: **manifest over folder fork**.
