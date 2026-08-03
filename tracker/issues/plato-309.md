---
id: plato-309
title: Plato agent guidance drift: add AGENTS.md, fix stale stdlib claims, move house knowledge into repo
type: debt
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [docs/working-on-plato.md, plato-293, plato-308]
---

Found while writing [`docs/working-on-plato.md`](../../docs/working-on-plato.md) (2026-07-29).
The Plato agent guidance has drifted from the code, and a chunk of the knowledge that makes
Plato work smoothly lives only in machine-local Claude Code state.

## Symptoms / impact

1. **`submodules/Plato/` has no `AGENTS.md` — only `CLAUDE.md`.** Codex, Cursor, Copilot,
   Gemini, Jules, and Amp read `AGENTS.md`. A non-Claude agent working in Plato gets *zero*
   project guidance: no freeze rules, no gate commands, no "never stage parakeet".
2. **`CLAUDE.md:22` and `docs/plato-for-agents.md` both describe `stdlib/` as "declarations
   only, no bodies yet".** Stale since the plato-293 flattening — `*.library.plato` files
   carrying implementation bodies are co-located in that folder now
   (`stdlib/README.md` is correct).
3. **Neither guide points at `stdlib/CONVENTIONS.md`, `STYLE_GUIDE.md`, or `LIBRARIES.md`** —
   the files that actually govern stdlib authoring, including the one-kind-per-file /
   ≤12-declaration / flat-folder partition rule.
4. **`CLAUDE.md` § Commands omits `tools\check-stdlib-fast.ps1` and
   `tools\regen-forward-conformance.ps1`** — the current inner loop and milestone gate for the
   most active workstream.
5. **`stdlib-tests/`** (forward law packet) appears in neither guide's layout table.
6. **Conflicting git policy.** `CLAUDE.md` hard rule 1 says "no git commits unless the mission
   says so"; the machine-global instruction says commit + push at every milestone. One must win,
   in writing, where non-Claude agents can see it.
7. **`submodules/Plato/.gitignore:14` ignores `.claude/`**, so no repo-level skill or setting
   can travel with the Plato repo.
8. **Machine-local-only knowledge.** Navigation-MCP launch recipe (manual exe, port 8768,
   `--root` set), the 10-field `TupleN` cap, the sum-type notes (`match` lowers to conditionals,
   `CHK306` forbids generic sums, parakeet grammar pushed manually), and the `--no-properties`
   V2 property-free story exist mainly in Claude memory, not in the repo.
9. `csharp-style` and `plato-mcp` skills live in machine-global `~/.claude/skills/` despite
   being entirely about this codebase; `write-readme` exists in both global and repo locations
   and will drift.

## Approach

- Add `submodules/Plato/AGENTS.md` as the real content; reduce `CLAUDE.md` to a pointer (or
  the reverse — pick one and make the other a one-liner).
- Fix items 2–5 in place.
- Settle the git policy (item 6) and state it in both `AGENTS.md` files.
- Drop `.claude/` from Plato's `.gitignore`; move `csharp-style` + `plato-mcp` in; delete the
  duplicate global `write-readme`.
- Land item 8's facts in their owning docs: `Plato.Navigation.CLI/README.md`,
  `stdlib/CONVENTIONS.md` or `docs/SEMANTICS.md`,
  `docs/design/plato-sum-types-design-2026-07-27.md`, `docs/plato-library-map.md`.

## Simplest thing that could work

Items 1–5 alone (one editing pass over two files, plus a new `AGENTS.md`) remove the
misleading claims and unblock non-Claude agents. The rest can follow.

## Case against

The guidance drift only costs anything when someone new — a different agent or a different
developer — works on Plato. If that stays hypothetical, this is documentation churn.
Counter: item 2 actively misleads *any* agent editing `stdlib/`, and item 6 is a live
contradiction that produces wrong behaviour today. Verdict: **pursue** items 1–6; items 7–9
are lower value and can wait.

## Outcome (2026-07-29, `4a818b3` in Plato)

Items 1–7 landed. The audit's premise for item 8 turned out to be wrong in a useful way: those
facts were **already documented** (`TupleN` cap in `../../docs/SEMANTICS.md`, `CHK306` in the
sum-types design doc, `--no-properties` in `plato-library-map.md`, the nav-MCP launch recipe in
`labs/PlatoNavigationMcp/README.md`). The problem was that no guide *linked* them, so agents
rediscovered them each session. Fixed by adding a "Language facts that are easy to get wrong"
section to `AGENTS.md` that points at each — no content duplicated.

Item 9 (skills) was deliberately **not** done. `.gitignore` no longer blocks it, but copying
`csharp-style` / `plato-mcp` out of `~/.claude/skills/` would create a second copy that drifts,
and two same-named skills in scope is worse than one in the wrong place. Deleting the duplicate
global `write-readme` is a change to personal machine config, not to this repo. Both left for a
deliberate move by their owner.

Residual, low stakes: `labs/PlatoNavigationMcp/README.md`'s example command indexes
`stdlib-legacy` roots while current use is `stdlib` + `stdlib-tests`.

## Done means

- [x] `submodules/Plato/AGENTS.md` exists and carries the real guidance (`CLAUDE.md` = pointer)
- [x] `stdlib/` described accurately in both guides, with pointers to its four doc files
- [x] `check-stdlib-fast.ps1` + `regen-forward-conformance.ps1` in the Commands section
- [x] `stdlib-tests/` in the layout table
- [x] git-commit policy stated once, consistently, in both repos' `AGENTS.md`
