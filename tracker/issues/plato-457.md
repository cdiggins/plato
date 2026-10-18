---
id: plato-457
title: C# writer does not escape C# keywords used as Plato names
type: bug
status: ready
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-10-17
closed:
links: []
---

## Problem

A Plato parameter or local named like a C# keyword (`base` in the stdlib's `Sampling`, `short` and `explicit` in the STEP design library) is emitted verbatim, which is a syntax error (CS1001). The writer's keyword list (`CSharpTypeWriter.CSharpKeywords`) escapes field names only.

## Done means

- [ ] Every identifier the writer emits from a Plato name (parameters, locals, lambda parameters, fields) is escaped with `@` when it is a C# keyword.
- [ ] The stdlib and the STEP design library build without CS1001 from keywords.
