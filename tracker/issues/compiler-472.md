---
id: compiler-472
title: Navigation omits assignment-target references
type: bug
status: done
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-23
closed: 2026-10-24
links: []
---

Assignment left-hand identifiers are missing from navigation references. `IdentifierSweepTests.EveryIdentifierIsClassified` reports 161 unclassified identifiers among 50,857 in the forward corpus, including `src`, `dst`, `i` and `width` assignment targets in `stdlib/foundation/sorting.library.plato`. Original compiler assemblies from `876fe36b` reproduce the same failure and same identifiers; repo-468's folder moves did not introduce it.

`SymbolFactory` handles `AstAssign` by calling `GetValue(astAssign.Var, astAssign)` directly. The returned reference is not registered in `SymbolsToNodes`, so navigation's reference extractor cannot see the assignment occurrence. Ordinary reads and declarations remain recorded. This is distinct from plato-364, which covers match-arm names, payload binders and case-factory targets.

A focused fix should register the target reference against its identifier node without changing declaration binding. This changes a shared binder contract, so it is deferred from the source-organization wave rather than treated as a file-move correction.

## Done means

- [x] Assignment targets navigate to their bound declaration, including nested loops and shadowed variables.
- [x] Focused navigation regressions and the corpus identifier sweep pass without adding exclusions.

## Resolution

Assignment targets now pass through the ordinary identifier resolver, preserving lexical binding while recording the identifier occurrence in `SymbolsToNodes`. A focused regression covers nested loops, a shadowing declaration, restored outer scope, exact spans, go-to-definition and find-references. That regression and the unchanged full-corpus identifier sweep passed together (two tests, 21.2 seconds wall time); all 50,857 identifiers were classified. No exclusion or binding fallback was added.
