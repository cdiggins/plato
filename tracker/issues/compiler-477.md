---
id: compiler-477
title: Simplifier corpus rewrite introduces unresolved references
type: bug
status: idea
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

Applying all proposed simplifications to the forward corpus introduces seven resolution errors where the original corpus has none. `SimplifierCorpusTests.SimplifiedCorpusStillCompiles` fails with the same assertion using both the original `876fe36b` assemblies and the repo-468 organization changes. The test uses the same current source snapshot in both runs; the refactor did not introduce the failure.

This can make source returned by the simplifier fail compilation. The responsible rule and smallest reproducer have not been isolated. Investigate the edits and bind failures before changing a rewrite rule; this is a semantic correction, outside the organization wave. The already-closed plato-404 addressed constructor removal in branches and is not evidence that this new resolution failure is fixed.

Reproduce: `dotnet test tests/Plato.Navigation.Tests/Plato.Navigation.Tests.csproj -c Release --filter Name=SimplifiedCorpusStillCompiles`.

## Done means

- [ ] Isolate the failing rewrites and add focused regressions for their resolution semantics.
- [ ] The corpus simplification test passes without suppressing diagnostics or excluding files.
