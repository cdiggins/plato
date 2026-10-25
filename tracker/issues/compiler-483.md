---
id: compiler-483
title: Characterize optimizer sequencing and identity hazards
type: problem
status: in-progress
priority: p1
effort: M
risk: high
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [docs/design/brep-compiler-optimization-plan.md]
---

Independent review identified H-01 through H-05 by source inspection, without executable reproductions: argument/Boolean hoisting; eager fusion completion/fault/termination changes; same-name combinator recognition and coercion stripping; assigned-local closure semantics; generated-name collisions. These can affect correctness if reachable. This investigation is separate from F1 and is not a claim that all five are confirmed bugs. Existing generic dispatch issue plato-374 and mutable-array issue plato-386 remain separate.

## Done means

- [ ] Build minimized executable reproductions or disprove reachability for H-01 through H-05.
- [x] State expected behavior from semantics/runtime contracts and distinguish domain faults, termination and resource exhaustion.
- [ ] File confirmed independent defects or close disproved hypotheses with evidence; define conservative prerequisites for broader optimization.

## Semantic audit — 2026-10-25

[Audit and per-pass consequences](../../docs/reports/optimizer-semantics-audit-2026-10-25.md) distinguish source purity, partiality, intrinsic sequencing, numeric profiles, affine effects and host compatibility. The broad introductory no-order/no-exceptions wording and stale interpreter claim of lazy C# arrays are corrected against existing detailed contracts. Read-only review confirms mechanisms behind H-02/H-03 and substitution/expansion concerns; these are not new executable reproductions. The remaining criteria stay open: minimize actual Plato/C# cases or disprove reachability, then file confirmed independent defects. Inherited Count caching must be distinguished from exact-output-versus-lowered-loop equivalence. No compiler code or flag defaults changed; no builds or runtime tests ran.
