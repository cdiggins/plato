# Plato.Navigation (plato-236) — navigation index library

Plan: `../../docs/plato-navigation-index-plan.md` (§14 decisions filled 2026-07-27).
Working in the MAIN repo (worktree `folder-approval-5f13ae` has an empty checkout; parakeet
must not be re-checked-out there). Repo has unrelated dirty work (plato-234 GLSL) — never stage it.

- [x] M0 skeleton + SourceSnapshot + BoundSnapshot. 34 files / 5020 lines, parse 917 ms (cold),
      bind 44 ms, 284 typedefs, 11628 SymbolsToNodes, 0 resolution errors, 0 aborts.
- [ ] M1 def table + spans + search + outline
- [ ] M2 ref table + type-site refs (D5) + hit-test
- [ ] M3 harness  [ ] M4 JSON/CLI  [ ] M5 README/gates

FINDING: `TypeExpression` overrides Equals/GetHashCode BY VALUE, so `SymbolsToNodes` collapses
type-expression occurrences. D5 must record type refs into a side list, not `SymbolsToNodes`.
