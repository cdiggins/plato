---
id: plato-405
title: plato_simplify has no compact result mode: a full-corpus run exceeds the MCP result budget
type: debt
status: done
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed: 2026-08-02
links: []
---

## Symptom

`plato_simplify` over the whole corpus returns every edit in full — `before`
and `after` text for each. The 161 SIM001 findings in the shipping tiers came
back as ~77 KB / 1,500 lines, past the MCP result budget, so the caller gets a
"saved to a file, read it in chunks" fallback instead of a result. The same
happens on `apply`, where the payload is the applied list.

Both the preview and the apply were therefore unusable directly: the counts had
to be recovered by parsing the spilled file.

## Fix

A compact result shape, either as the default over some size or behind a
parameter:

- counts by code and by file, no edit text (what a caller needs to decide scope)
- edit text only when the caller asks for it, or only for the first N

`maxFindings` does not help: lowering it hides findings rather than compressing
them, and the caller cannot tell how many were suppressed until the run.

## Shipped (2026-08-02)

`detail`, a new `plato_simplify` parameter: `auto` (default), `summary`, `full`.

`total`, `byCode` and `byFile` counts now always cover every edit found and are
never suppressed — that is the part a caller needs in order to pick a scope, and
suppressing it is what made `maxFindings` the wrong lever. The `before`/`after`
text is the part that is budgeted: `auto` carries it while the result stays
under 12 000 characters and then stops, `summary` carries none, `full` carries
up to `maxFindings` whatever the size. `returned` and `omitted` always add up to
`total`, so the caller can see what it did not get. `apply` reports in summary
form (`filesChanged`, `editsApplied`, and the names of any files whose rewrite
changed nothing) unless `full` is asked for.

Measured against the corpus as of `40c85da`, which still held the 172 edits that
prompted this (154 SIM001 + 18 SIM002 across 33 files):

| call | result |
|---|---|
| `detail: full, maxFindings: 500` (the old shape) | 81 904 chars / 1 603 lines |
| `detail: auto` (the new default) | 18 984 chars / 460 lines, 45 shown, 127 omitted |
| `detail: summary` | 2 334 chars / 54 lines, all 172 counted |
| `auto`, scoped to one file | 1 118 chars, both edits with full text |

Landed in the studio repo as `d7651d1` — `labs/PlatoNavigationMcp/SimplifyMcpTools.cs`,
with `tests/PlatoNavigationMcp.Tests/SimplifyToolTests.cs` pinning the contract
(counts survive every detail mode, `returned` + `omitted` = `total`, a
corpus-wide preview fits, and `apply` still refuses to run without a scope).

## Done means

- [x] A full-corpus `plato_simplify` returns a usable result without spilling to a file
- [x] `apply` reports what it wrote in summary form
