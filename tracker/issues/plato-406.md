---
id: plato-406
title: plato_check style gate reports zero warnings when the index is served from the parse cache
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: []
---

## Symptom

The `style` gate's verdict depends on whether the index was parsed from disk or
served from the parse cache, so the same source yields different results.

Observed in one session, all against `stdlib/`:

1. First call after a cold index build (172 files parsed, 172 cache misses):
   `style` reported **162 warnings** — STY004 doc-comment-length and STY006
   file-declaration-count findings, e.g.
   `stdlib/foundation/algebra.concepts.plato:43 STY004 doc comment block is 13 lines (cap 12)`.
2. Every later call, including after `plato_reload` (172 files reused from
   cache, 0 parsed): `style` reported **0 warnings, 0 errors**, with
   `algebra.concepts.plato` untouched between the two runs.

A zero from a gate that found 162 findings minutes earlier is a wrong green —
exactly the failure mode `stdlib/VERIFICATION.md` warns about. Suspected cause:
the style rules read source text that only the fresh parse path retains.

`types` and `sums` may share the fault: the first call reported 3 `CHK201` in
`stdlib/tests/polyhedra.laws.plato` and `sums` 3 errors, later calls 0 and 2
respectively. The tests-tier part of that is plato-389; the disappearing style
findings are not.

## Done means

- [ ] `style` returns the same findings from a cache-served index as from a freshly parsed one
- [ ] A regression test covers reload-then-check
