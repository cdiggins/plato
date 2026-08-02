---

Create an isolated `submodules/Plato/plato-src-v2` source library that improves the
existing vocabulary and gives geometry, animation, numerical, scientific, graphical,
and engineering code a common set of reusable data types and capability interfaces.
This first slice deliberately defines structure only: no libraries, function bodies,
generated output, or production-pipeline changes.

## Design

- Organize declarations into many small, domain-focused `.plato` files.
- Separate semantic roles that the current library conflates: points versus vectors,
  instants versus durations, bounded versus unbounded geometry, and physical quantities
  versus unitless numbers.
- Use dimension-generic containers where Plato supports them, alongside ergonomic
  2D/3D/4D types for common graphics and geometry work.
- Model interfaces as small composable interfaces, avoiding a single deep inheritance tree.

## Case against

A very broad type catalog can become speculative, internally inconsistent, and expensive
to implement. Plato's current interfaces also encode operations as interface functions, so
types may lint while still lacking useful implementations. Calling this source tree "v2"
also overlaps the existing V2 code-generation recipe. The mitigating choice is to keep
the prototype isolated, declaration-only, domain-grouped, and parser/type-checker clean;
adoption and implementation remain separate decisions.

**Verdict: pursue as an isolated vocabulary prototype.** Its value can be judged from
coherence and coverage before any production migration or algorithm investment.

## Done means

- [x] `plato-src-v2` contains only type and interface declarations, grouped into focused source files.
- [x] The vocabulary covers reusable foundations plus 2D, 3D, 4D, and dimension-generic application domains.
- [x] Existing ambiguous geometry interfaces are replaced with explicit bounded/unbounded and semantic types.
- [x] The Plato CLI parses and resolves the complete folder without errors.
id: plato-228
title: Prototype a broad Plato v2 type and interface library
type: feature
status: done
priority: p1
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-26
closed: 2026-07-27
links: []
---
