---
id: docs-391
title: AGENTS.md still documents the retired --methods / --no-properties flags
type: debt
status: done
priority: p3
effort: S
risk: low
area: docs
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: []
---

## Problem

`compiler-387` deleted the `--methods` and `--no-properties` CLI flags (property-free C# emission
is unconditional now — see
`tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md`). Two places in
`AGENTS.md` still describe them as if they existed:

- the `src/Plato.CLI/` bullet lists `[--methods] [--no-properties]` among `Program.cs` args;
- the language-facts bullet "**The shipping recipe is property-free.**" explains the property-free
  output as something `--no-properties` selects.

Both are now wrong in the same way: they present as a choice something that is no longer
selectable. An agent reading either one will pass a flag the CLI does not recognise.

`AGENTS.md` had uncommitted edits from a concurrent session throughout the compiler-387 work, so
it was deliberately left untouched rather than swept into someone else's change.

## Done means

- [ ] The `src/Plato.CLI/` argument list in `AGENTS.md` no longer names `--methods` /
      `--no-properties`.
- [ ] The property-free language-fact bullet states that the emitted C# is always property-free
      and points at the ADR instead of a flag.
