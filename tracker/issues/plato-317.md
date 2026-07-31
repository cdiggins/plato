---
id: plato-317
title: docs/stdlib-ai-summary.txt is stale - regenerate or retire
type: debt
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: []
---

## Problem

`submodules/Plato/docs/stdlib-ai-summary.txt` is a one-declaration-per-line dump
of the whole stdlib vocabulary, apparently produced for AI consumption. It is
checked in, referenced by no doc, no script, and no test, and no generator for it
exists in the repo — so nothing keeps it honest.

It is already behind the tree:

- no `JaggedArray` / `Jagged` (the plato-303 CSR work)
- still names `MeshTopology`, renamed `MeshElementCounts` by plato-314, and still
  shows the bare `EdgeIndex` / `EdgeCount` names that plato-314 qualified

An unmaintained machine dump that contradicts the source is worse than no dump:
an agent that reads it gets confidently wrong symbol names.

## Options

1. **Retire it.** Delete the file. Agents have `plato-navigation` MCP plus
   `stdlib/README.md` for the same job, both live.
2. **Generate it on demand.** Add a `Plato.CLI` verb (or a tools script) that
   emits it, do not check the output in.
3. **Regenerate and gate it.** Keep the file, add the generator, and add a check
   that fails when it drifts from the sources.

Option 1 unless something actually consumes it; find the consumer first.

## Done means

- [ ] The file is deleted, or regenerated with a committed generator and a drift
      check.
- [ ] No stale symbol names for renamed vocabulary remain in `submodules/Plato/docs`.
