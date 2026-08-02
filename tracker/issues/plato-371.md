---
id: plato-371
title: Interface hierarchy export and redundant inherits lint
type: feature
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-31
closed: 2026-07-31
links: [src/Plato.Compiler/Analysis/Linter.cs, src/Plato.ContextExport/Program.cs, tools/export-types-context.ps1, docs/types-and-concepts-context.txt]
---

## Issue
Agents and authors reviewing the interface lattice only have the flat
`docs/types-and-concepts-context.txt` dump (parse-only declaration text). There is no
readable inherits forest, and `LINT011` only flags redundant `implements` on concrete
types — redundant interface-to-interface `inherits` clauses are invisible.

## Impact
Lattice reviews (and "is this inherits line needed?") require manual reading of every
`*.concepts.plato` file. Redundant inherits hide the real shape and go stale when parents
are refactored. ~162 forward interfaces make a text forest practical.

## Affected code
- `src/Plato.Compiler/Analysis/Linter.cs:619` — `CheckRedundantImplements` / LINT011 (types only)
- `src/Plato.Compiler/Checking/ConceptClosure.cs` — transitive interface walk (checker)
- `src/Plato.Compiler/Symbols/Definitions.cs:206` — `GetAllImplementedConcepts`
- `src/Plato.ContextExport/Program.cs` — flat declaration export
- `tools/export-types-context.ps1` — regen script for the context dump

## Cause / analysis
ContextExport was scoped as an agent token pack (D6 in the navigation plan: leave it as
a text dump). Redundant-inherits detection is the same predicate as LINT011 applied to
`Inherits` on interfaces; it was simply never written.

## Priority
p2 — unblocks vocabulary review; low risk; compounds as the lattice grows.

## Dependencies
- Blocked by: none
- Blocks: none
- Touches: Linter, ContextExport, export script, `docs/interface-hierarchy.txt`

## Fix approaches
1. **LINT016 + ConceptHierarchy helper + `--hierarchy` on ContextExport** — lint for the
   gate; ASCII forest artifact for review. Preferred.
2. Navigation MCP query only — good for "ancestors of X", bad as a committed greppable
   whole-lattice doc.
3. Golden PlatoTests snapshot of the forest — fights intentional lattice edits.

## Bedrock
Strengthens the **interface-lattice as a first-class reviewed artifact** seam: one graph
builder shared by the linter (redundancy) and the export (forest). Verdict:
**simplest-along-the-grain**. Must NOT goldensnapshot the forest text; must NOT change
LINT011 severity or invent Mermaid as a required gate.

## Done means
- [x] LINT016 reports redundant interface `inherits` (Info), with a unit test
- [x] Shared `ConceptHierarchy` builds the graph and formats an ASCII forest
- [x] `Plato.ContextExport --hierarchy` writes the forest; script regenerates
      `docs/interface-hierarchy.txt` from forward `stdlib/`
- [x] Redundant-inherits section appears in the export trailer

## Simplest fix
LINT016 mirroring LINT011 + ASCII export mode on ContextExport.
- Pros: reuses binder closure; Info so `--strict` stays green; reviewable artifact.
- Cons: tracked hierarchy doc will churn with stdlib edits (acceptable, like the flat dump).

## Prevention
- LINT016 catches new redundant inherits at lint time.
- Hierarchy doc gives agents a single place to review lattice shape before suggesting
  inherits changes.
