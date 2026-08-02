---
id: plato-405
title: plato_simplify has no compact result mode: a full-corpus run exceeds the MCP result budget
type: debt
status: ready
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
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

## Done means

- [ ] A full-corpus `plato_simplify` returns a usable result without spilling to a file
- [ ] `apply` reports what it wrote in summary form
