---
id: compiler-472
title: Navigation omits assignment-target references
type: bug
status: in-progress
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

Assignment left-hand identifiers are missing from navigation references. `IdentifierSweepTests.EveryIdentifierIsClassified` reports 161 unclassified identifiers among 50,857 in the forward corpus, including `src`, `dst`, `i` and `width` assignment targets in `stdlib/foundation/sorting.library.plato`. Original compiler assemblies from `876fe36b` reproduce the same failure and same identifiers; repo-468's folder moves did not introduce it.

`SymbolFactory` handles `AstAssign` by calling `GetValue(astAssign.Var, astAssign)` directly. The returned reference is not registered in `SymbolsToNodes`, so navigation's reference extractor cannot see the assignment occurrence. Ordinary reads and declarations remain recorded. This is distinct from plato-364, which covers match-arm names, payload binders and case-factory targets.

A focused fix should register the target reference against its identifier node without changing declaration binding. This changes a shared binder contract, so it is deferred from the source-organization wave rather than treated as a file-move correction.

## Done means

- [ ] Assignment targets navigate to their bound declaration, including nested loops and shadowed variables.
- [ ] Focused navigation regressions and the corpus identifier sweep pass without adding exclusions.
