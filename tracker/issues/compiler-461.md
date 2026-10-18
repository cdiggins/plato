---
id: compiler-461
title: A body whose lambdas capture only lambda-bound names is emitted as => /*?*/
type: bug
status: ready
priority: "?"
effort: "?"
risk: "?"
area: compiler
sprint: 
created: 2026-10-17
closed:
links: []
---

## Problem

`TirLambdaCaptureRewriter.Rewrite` statementizes an expression body (`return e;`) as soon as any lambda in it captures a name, before deciding which captures it will hoist. When every capture is skipped (a parameter of an outer lambda, or a local), nothing is hoisted but the body is still a `TirReturn`, and the C# writer, which emits the function expression-bodied, writes `=> /*?*/` (CS1525). Seen in the STEP B-rep design library: `FaceMeshDump` (`meshes.FlatMap(fm => ... 0.MapRange(j => fm.FaceMeshPart))`) and `LawRun.ToolingRuns`.

## Done means

- [ ] A body is statementized only when at least one capture is hoisted.
- [ ] Both functions emit compilable C#.
