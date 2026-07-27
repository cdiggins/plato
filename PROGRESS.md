# v3 sum-type mission (2026-07-27, this session)
- Done: checkpoint 4ab855e; sweep 1432b10 (~100 types); review fixes dc11c3e; emitter Array fix 5e3efb4.
- Steps 1-4 PASS (lint 0/0, type-check OK, docs.html emitted, 1124 .g.cs).
- Step 5 FAIL: standalone build 570 errors (CS0246 scalar leak, CS0216 op pairs, CS0552, CS0315/535 runtime gap).
- Now: background agent a1e05f8ccef9f8842 burning down emitter-side classes.
- Guards green: conformance 205/205, PlatoTests 142/142, regen-generated identical, frozen V1 clean.

# Plato.Navigation (plato-236) — navigation index library

Plan: `../../docs/plato-navigation-index-plan.md` (§14 decisions filled 2026-07-27).
Working in the MAIN repo (worktree `folder-approval-5f13ae` has an empty checkout; parakeet
must not be re-checked-out there). Repo has unrelated dirty work (plato-234 GLSL) — never stage it.

DONE — M0..M5 all delivered (3ce8655, 9794fa6, 428fc4a + this commit). 4307 defs / 9708 refs
over 34 files; 25/25 navigation tests; sweep classifies all 7565 identifiers; warm rebuild 590 ms.
Gates: regen-generated 184/184 identical (both variants), conformance 205/205.

Compiler edits (all additive, in SymbolFactory): TypeReferences side list + 3 recording lines.
FINDING that changed the plan: `TypeExpression` overrides Equals/GetHashCode BY VALUE, so
`SymbolsToNodes` collapses type-expression occurrences — D5 needed a side list. Written up as
§15 of the plan doc along with four other plan-vs-code corrections.
