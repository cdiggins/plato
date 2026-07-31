# Gate log

Append-only history of gate runs, written by `tools/record-gates.py`. Current machine-readable
state lives in `docs/status-report-snapshot.json`; the ratchets themselves are enforced in
`tests/PlatoTests` (`ForwardStdLibLintTests`, `ForwardStdLibCheckerTests`), not here.

Read a row as: what the gates said at that commit. A number that moved between two rows is a
change somebody made — start at the commit in the second row.

| Date | Commit | lint ratchet (E+W) | lint info | PlatoTests | codegen | conformance |
|---|---|---|---|---|---|---|
| 2026-07-31 | `52b3f8c` | 229 (0+229) | 2564 | 196/196 | 1307 files, 48 degraded | — |
| 2026-07-31 | `d3f5047` | 159 (0+159) | 2549 | 196/196 | 1311 files, 48 degraded | — |
