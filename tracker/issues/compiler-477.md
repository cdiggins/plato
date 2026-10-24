---
id: compiler-477
title: Simplifier corpus rewrite introduces unresolved references
type: bug
status: in-progress
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

- [x] Isolate the failing rewrites and add focused regressions for their resolution semantics.
- [x] The corpus simplification test passes without suppressing diagnostics or excluding files.

## Resolution

The constructor-shortening rule SIM001 removed names from thirteen-field constructor results, causing the binder to look for nonexistent `Tuple13`. Isolating the rules showed all seven errors came from SIM001; SIM002 and SIM003 introduced none.

SIM001 now requires a matching concrete generic `TupleN` declaration with the same parameter and field count in the supplied vocabulary. The check uses actual declarations rather than assuming a fixed maximum. Focused cases bind both before and after the proposed edit: available Tuple2 permits shortening, while missing Tuple2 and missing Tuple13 preserve the constructor. Existing positive fixtures explicitly supply their tuple declaration.

The complete navigation suite passed all 73 executed tests, including the corpus simplification gate and the new focused cases. No files or diagnostics were excluded. Corpus failures now print their source range and message to make future regressions actionable.
