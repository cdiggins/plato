---
id: compiler-471
title: Parser dependency requires a machine-relative SDK checkout
type: debt
status: idea
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

The external `parakeet/Parakeet.Parsers/Ara3D.Parakeet.Parsers.csproj` references `../../../../ara3d-sdk/src/Ara3D.Logging` and `Ara3D.Utils`. The current checkout resolves these under the user's home directory, so a relocated clean source snapshot cannot build without that machine-specific layout. This prevents reproducible builds and complicates compiler performance comparison.

Observed during repo-468 baseline preparation: CS0234, `The type or namespace name 'Logging' does not exist in the namespace 'Ara3D'`; CS0246, `The type or namespace name 'ILogger' could not be found`. The normal checkout builds; this is an existing dependency-layout problem, not a refactoring regression. The temporary comparison fixture references the exact dependency DLLs from the current CLI output to keep its runtime inputs equivalent.

The source belongs to the parakeet repository. Fix and commit there, then update the Plato submodule pin through the established ownership workflow. Do not add sibling SDK project references to Plato projects.

## Done means

- [ ] Replace the parser project's machine-relative SDK references with its supported package dependency upstream.
- [ ] Validate a clean relocated build and update the Plato dependency pin.
