---
id: plato-406
title: plato_check style gate produced findings for retired rules, then stopped
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: []
---

## Symptom

Within one session, against unedited files, the `style` gate of `plato_check`
returned two different verdicts, and the first one cited rules that no longer
exist in this repo.

1. First call: `style` reported **162 warnings**, all STY004
   (doc-comment length) and STY006 (declarations per file), e.g.
   `stdlib/foundation/algebra.concepts.plato:43 STY004 doc comment block is 13 lines (cap 12)`.
2. Every later call, including after `plato_reload`: **0 warnings, 0 errors**,
   with `algebra.concepts.plato` untouched between the two.

A gate that reports 162 findings and then reports none, on the same source, is a
wrong green in one of the two directions — `stdlib/VERIFICATION.md` names this
class of failure directly. Which direction is the question below.

## What the evidence rules out

- **The rules are gone from source.** STY004 and STY006 were retired in
  `40c85da` (2026-08-01 23:24) and no identifier `STY004` or `STY006` survives
  anywhere under `src/`. `StyleChecker`'s own docstring records the retirement:
  "Deliberately absent: caps on doc-comment length and on declarations per file."
  So report 2 is the correct one and report 1 came from code that is not in the
  tree.
- **A stale binary does not close it either.** Only one `PlatoNavigationMcp`
  process existed (pid 65848, started 2026-08-02 17:35) and it served both
  calls; its executable was built at 17:32 the same day, after the retirement
  commit, and `PlatoNavigationMcp.csproj` references the standalone
  `plato\src\Plato.Navigation`, not studio's pinned submodule. A binary built
  then should not contain STY004 at all.
- **A persisted result cache does not exist.** `CheckMcpTools` memoizes per
  index generation in memory only; `%LOCALAPPDATA%\PlatoNavigationMcp` holds
  the two log files and nothing else.

So the observation and the three obvious explanations disagree, and the task is
first to reproduce and explain it, not to patch a guessed cause.

## Secondary defect, worth fixing regardless

`ensure-server.ps1 -Force` printed "server started on port 8768" twice during
that session while pid 65848 — started hours earlier — kept the port and kept
answering. A caller cannot tell a freshly started server from an old one, and
`plato_index_status` returns no build stamp, so nothing in the protocol
identifies which build produced an answer. Reporting the server assembly's
build time and informational version in `plato_index_status`, and making
`-Force` either genuinely restart or say plainly that it did not, would have
made the above diagnosable in one call.

## Related

`types` and `sums` also differed between the first and later calls (3 `CHK201`
in `stdlib/tests/polyhedra.laws.plato` and 3 sum errors, later 0 and 2). The
tests-tier scoping part of that is plato-389; check whether the rest shares a
cause with this issue.

## Done means

- [ ] The two verdicts are reproduced and the cause named
- [ ] `style` returns the same findings for the same source across reloads and restarts
- [ ] `plato_index_status` reports the server build it is answering from
- [ ] A regression test covers reload-then-check
