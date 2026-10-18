---
id: plato-456
title: C# writer does not generate IIndexable functions for Array receivers
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

The `IIndexable<$T>` library functions (`IsValidIndex`, `At`, `LastIndex`, `First`, `Last`) are not generated for `Array<T>` receivers (`IReadOnlyList<T>` in C#), so a call such as `points.IsValidIndex(i)` fails with CS1929 (190 errors in the TKT-311 build). `First` and `Last` compile only because `System.Linq` supplies them, with LINQ's meaning.

## Done means

- [ ] Every `IIndexable<$T>` function with a body is emitted for `IReadOnlyList<T>` receivers, as `Extensions.g.cs` already does for `Take` and `Map`.
- [ ] The STEP design library builds without CS1929 on these names.
