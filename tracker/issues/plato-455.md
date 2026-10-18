---
id: plato-455
title: C# writer emits a raw indexer for a typed-index subscript (m.Faces[f])
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

A subscript with a typed index (`m.Faces[f]`, `f: BrepFaceIndex`) is emitted as the raw C# indexer `m.Faces[f]`. C# will not chain the index struct to `Integer` to `int`, so it fails with CS1503 (290 errors in the TKT-311 build of the STEP B-rep library).

## Done means

- [ ] A single-`Integer` index struct converts to the C# index type, so `xs[i]` compiles for every `IIndex` type.
- [ ] The STEP design library builds without CS1503 on a subscript.
