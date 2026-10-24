---
id: plato-465
title: Warm server: a renamed or deleted declaration stays alive at a compiled path; plato_check reports a phantom CHK203
type: bug
status: ready
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-10-20
closed:
links: []
---

## Problem

Reported 2026-10-20 by the STEP mesher catalog builders in bim-open-toolkit (their
`sandbox/step-brep-mesh/reviews/FINDINGS.md`, F-58): after a declaration under a folder the warm
server (PlatoNavigationMcp, port 8768, studio `labs/PlatoNavigationMcp`) has already compiled is
renamed or deleted, `plato_check` keeps the old name alive and reports a phantom CHK203
(ambiguous call: the old and the new declaration both match). Copying the folder to a fresh path
and checking the copy gives the right answer, so the stale state is per path.

Where to look: `CheckMcpTools.Prepare` memoizes a `CheckState` per `_context.Index.Generation`
and `_context.EnsureFresh()` decides when that generation moves; `WarmProgram` (this repository,
`src/Plato.Navigation/Execution/WarmProgram.cs`) hashes every file's content into its generation and
recompiles from nothing, so the law runner is not the suspect. Either the index generation does
not change for a file under a `--law-folder` root, or the shared `ParseCache` returns a tree for
the old text (its key is path plus content hash, so only if the hash is reused).

The same session also measured the server growing to 2.8 GB and a check from 15 s to 4.5 min
after many fresh paths; `WarmLawRunner.Acquire` bounds the kept programs by `MaxPrograms`, the
navigation index does not bound anything.

## Done means

- A test in `tests/Plato.Navigation.Tests` (or the studio server tests) renames a declaration in
  a law folder between two checks and sees the old name gone.
- The index's generation changes for any file under any root the server reads.
