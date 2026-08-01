---
id: plato-390
title: Extract Plato.Verify: one policy engine behind the gates, the tests and plato_check
type: feature
status: idea
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-01
closed:
links: [docs/verification-inventory.md, tracker/issues/plato-388.md, tracker/issues/plato-372.md, studio labs/PlatoNavigationMcp/CheckMcpTools.cs]
---

## Issue

Verification is implemented once per host rather than once. The same checks exist as PowerShell
scripts, as Python, as NUnit fixtures and as an MCP tool, across two repositories, and the
consumers that only need results parse them back out of console text with regular expressions
(`tools/record-gates.py`, `parse_lint` and `parse_test_counts`). A change to any output format
silently degrades a recorded number to nothing.

Full per-tool inventory: [`docs/verification-inventory.md`](../../docs/verification-inventory.md).

plato-388 makes the tools agree on *what* to check. It does not make them agree on *how*, and it
leaves the text-scraping in place.

## Approach

Extract the policy engine as a library, `src/Plato.Verify`, with a thin CLI over it. Each check
runs against a `Compilation` and returns a structured result; the CLI serialises that result to
JSON.

Then every existing entry point becomes a caller rather than an implementation: the shell gates
become wrappers, the measuring half of `tools/record-gates.py` disappears and its reporting half
reads JSON, the PlatoTests fixtures assert on results with ceilings from the manifest, and
`plato_check` becomes an adapter that runs the same library against its warm cache — making
warm-versus-cold a caching detail rather than a second implementation.

Most of this already exists. `CheckMcpTools.cs` runs parse, resolve, lint, type-check, sum-type
and style against a `Compilation` and returns structured findings today; what it lacks is tier
awareness (plato-389), a command-line host, and the codegen and law-execution rungs. Extract it,
do not rewrite it.

Worth having beyond tidiness: the gates stop being Windows-and-PowerShell-only, which is what
currently limits an agent on Linux or in CI to `dotnet build` and `dotnet test` (AGENTS.md); and
the policy becomes unit-testable without running a gate.

## Constraints

- `tools/stage-stdlib.ps1` must stay a script and must not be absorbed. Its entire purpose is to
  gate stdlib edits when the compiler does not build, which a tool built on the compiler cannot do.
- Keep measurement separate from reporting. The engine produces result data; `docs/gate-log.md`
  and `docs/status-report.html` stay renderers over it.
- Depends on plato-372, which decides whether the studio gate scripts are re-pointed at the
  post-restructure layout or replaced by calls into this repo. That answer decides whether the
  studio scripts are ported or deleted.

## Simplest implementation

Library first, callers migrated one at a time, old scripts kept running alongside and compared
until they agree. Rungs 5 and 6 (codegen, law execution) move last — they shell out to
`dotnet build` and `dotnet test` and are the least shared.

## Done means

- [ ] `src/Plato.Verify` runs the static checks against a `Compilation` and returns structured results.
- [ ] A CLI over it emits those results as JSON.
- [ ] PlatoTests fixtures and `plato_check` both call it rather than reimplementing it.
- [ ] `tools/record-gates.py` reads JSON; no regular expression parses tool output.
- [ ] `tools/check-stdlib-fast.ps1` is a wrapper or is gone.
- [ ] The gates run on a platform without PowerShell.
