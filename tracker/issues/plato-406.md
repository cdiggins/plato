---
id: plato-406
title: plato_check style gate produced findings for retired rules, then stopped
type: bug
status: done
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed: 2026-08-02
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

## Cause (2026-08-02)

**A stale binary after all — in a process that no longer existed when the
process list was taken.** The second bullet above rests on "only one process
existed and it served both calls", and that inference is what is wrong.

The evidence:

- Every `PlatoNavigationMcp` / `Plato.Navigation` binary on disk was searched
  for the strings `STY004` and `STY006`. The ones the running server loads
  (`labs\PlatoNavigationMcp\bin\Release\net8.0`, built 17:32) do not contain
  them; ten older copies elsewhere in the two checkouts, none newer than
  2026-08-01 21:10, do.
- Windows holds an exclusive lock on a running executable and on every assembly
  it has loaded. Rebuilding the server while it is up fails —
  `error CS2012: Cannot open ... because it is being used by another process`,
  reproduced deliberately. So the 17:32 rebuild of that output folder proves no
  server was running at 17:32, and pid 65848 (start 17:35) was therefore born
  *after* the rebuild. Whatever answered a call made before 17:32 was a
  different process, running a pre-`40c85da` build, and it was gone by the time
  anyone looked.
- `%LOCALAPPDATA%\PlatoNavigationMcp\server.log` is truncated by the launcher's
  `Start-Process -RedirectStandardOutput` on every launch. Its single record is
  a cold index build stamped 17:35 — consistent with a new process, not with
  the one that answered earlier.
- Running one of those older builds reproduces report 1 exactly (see below).
  Both verdicts are correct for the code that produced them.

So this is not a wrong green or a wrong red in the checker. It is that
`plato_reload` re-reads SOURCE and never CODE — the process holds its assemblies
for its whole life — and nothing in the protocol said which build was answering,
so the only available diagnosis was a process list taken after the fact, which
cannot see a process that has been replaced.

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

The stale process would also have been carrying an older `Plato.Compiler.dll`,
which is enough on its own to move a `CHK201` count, so the same cause covers
this comfortably — but only the `style` half was reproduced, so treat that as
the likely explanation rather than a demonstrated one.

## Reproduced (2026-08-02)

Both verdicts, side by side, on the same unedited `stdlib`. Two servers were run
from the same `PlatoNavigationMcp` build, differing only in which
`Plato.Navigation.dll` sat beside it:

| checker assembly | `plato_check gates=style` |
|---|---|
| built 2026-08-01 21:10, source `9040737` (an ancestor of `40c85da`) | 162 warnings, first at `stdlib/foundation/algebra.concepts.plato:43 STY004` |
| built 2026-08-02 17:28, source `7f3de91` | 0 warnings, 0 errors |

162 and that file and line are the numbers from the original report, so this is
the observed failure and not a lookalike.

## Shipped (2026-08-02)

- `plato_index_status` now returns `serverBuild`: the process id, its start time,
  and for both `PlatoNavigationMcp` and `Plato.Navigation` (the assembly that
  holds the gates) the assembly version, the build time and the informational
  version. Source Link puts the source commit in that last field, so the two
  servers above are told apart by one call: `1.0.0+9040737...` against
  `1.0.0+7f3de91...`. Build time is read from the file rather than the PE header,
  which a deterministic build fills with a content hash.
- `ensure-server.ps1 -Force` restarts for real: it finds the
  `PlatoNavigationMcp` process on the port by image name and command line — the
  listener is an `HttpListener`, so http.sys owns the socket and netstat reports
  pid 4 — stops it, waits for the port, and starts a new one. When it cannot
  identify or stop the holder it says so and leaves the old server running
  instead of printing a start that did not happen. Every message names a pid.
- The launcher also reports a launched process that exited, which the old script
  could not distinguish from a successful start, because it only asked whether
  *somebody* was answering the port afterwards.

Note on the third box: the guarantee is "same source and same build give the
same findings", not "the same findings forever" — a rule change is supposed to
change the verdict. What is fixed is that a verdict from a build older than the
library can now be recognised as one.

Landed in the studio repo as `72472f8` — `labs/PlatoNavigationMcp/ServerBuild.cs`
and `IndexMcpTools.cs`, with `tests/PlatoNavigationMcp.Tests/ToolPackTests.cs`
carrying `StyleFindingsSurviveAReload`, `StyleReportsNoRetiredRule` (fails on any
STY004/STY006 whatever the mechanism) and `StatusReportsTheServerBuild`. The same
commit removes the server README's duplicate list of style rules, which still
named the two retired ones — a second copy of a fact going stale is the same
failure in documentation form.

`ensure-server.ps1` lives in `~/.claude/skills/plato-mcp/`, outside both repos,
so its `-Force` fix is not covered by any commit. Verified by hand: a second
`-Force` against a live server printed `-Force stopped pid 31400` then
`server started on port 8768 (pid 62592)`, and without `-Force` it stayed
silent.

## Done means

- [x] The two verdicts are reproduced and the cause named
- [x] `style` returns the same findings for the same source across reloads and restarts
- [x] `plato_index_status` reports the server build it is answering from
- [x] A regression test covers reload-then-check
