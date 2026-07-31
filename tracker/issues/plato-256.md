---
id: plato-256
title: Plato.Navigation: find-references on a v3-defined type returns zero v3-internal sites (cross-root same-name pollution)
type: bug
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: []
---

## Symptom

With the navigation server indexing roots `plato-src`, `plato-test-src`, AND `plato-src-v3`
(2026-07-28, index generation `77e4d3f7…`), `plato_references` on a type DEFINED in
plato-src-v3 — e.g. `Vector2` (08-vectors.plato) — returned only sites in plato-src and
plato-test-src, and **zero** references inside plato-src-v3 itself, even though grep found
~337 whole-word `Vector[234]` occurrences across 45 v3 files.

## Diagnosis (probable)

Cross-root same-name pollution: when the same type name exists in more than one root
(`Vector2` in both plato-src/primitives.plato and plato-src-v3/08-vectors.plato), each
name reference binds to a merged *group* of 10+ target ids spanning roots. The
reference-lookup then appears to attribute all sites to one representative definition and
drops (or mis-buckets) the sites inside the other root. v3 is self-contained (declares its
own primitives), so its references should never bind across roots at all.

## Suggested fix direction

Scope binding per root (each root is a separate resolution universe), or at minimum make
`plato_references` return the union of sites for every definition in the target group,
grouped by defining root. Add a regression test: same-named type in two roots; references
inside each root must resolve to that root's definition only.

Found during the Vector2/3/4 → Number2/3/4/8 + Vector2D/3D rename (2026-07-28); the rename
had to fall back to grep for v3-internal sites.