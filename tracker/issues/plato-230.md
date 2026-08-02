---

Create `submodules/Plato/plato-src-v3`: a comprehensive, declaration-only vocabulary
library succeeding the plato-src-v2 prototype (plato-228). Types and interfaces only —
`interface` keyword with bare names (no I-prefix), domain-grouped numbered files, generous
coverage across geometry, 2D/3D/4D/N-D computation, animation, numerical/scientific
computing, graphics, physics, motion graphics, image processing, and engineering.

## Design

- 70 files in dependency-layer order: foundation (00-14) authored first as the shared
  spine, then 11 domain blocks (15-69) authored by parallel sub-agents.
- README.md carries the conventions (kind-pattern enums, 10-field cap, doc-comment
  requirements, quantity/unit usage) plus a cross-domain name registry so blocks
  reference — never re-declare — each other's types; agents validated in isolated
  scratch folders with stub declarations for cross-block names.
- Semantic splits preserved and extended from v2: points vs vectors, instants vs
  durations, quantities vs unitless numbers, typed mesh indices, natural-unit fields.
- Compiler constraint discovered and documented: every N-field type synthesizes a
  TupleN constructor and tuples cap at 10 — so no type exceeds 10 fields; matrices
  store row vectors.

## Outcome

- 154 interfaces + 1125 types (~13.4K lines) across 70 files.
- Full-folder `Plato.CLI lint`: 0 parse errors, 0 symbol-resolution errors, 0 duplicate
  declaration names (3 spec-overlap duplicates removed in the integration pass).
- Plato repo commits 7cec1e3 (foundation) + 72912d8 (domain blocks).

## Done means

- [x] plato-src-v3 contains only type and interface declarations, grouped into
      dependency-layered domain files with a conventions + registry README.
- [x] Coverage spans all requested application domains, substantially exceeding v2.
- [x] The Plato CLI parses and resolves the complete folder without errors and no
      duplicate declaration names exist.
id: plato-230
title: Comprehensive Plato v3 type and interface vocabulary (plato-src-v3)
type: feature
status: done
priority: p1
effort: L
risk: med
area: plato
sprint:
created: 2026-07-27
closed: 2026-07-27
links: [plato-228]
---
