---
id: plato-454
title: C# writer drops the parentheses of a conditional used as a condition
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

`TirCSharpBodyWriter` writes a `TirConditional` as `cond ? a : b` without parentheses around `cond`. When `cond` is itself a conditional, the library's short-circuit "and" idiom `(a ? b : false) ? x : y`, C# parses the output as `a ? b : (false ? x : y)`: it compiles and means something else. Found by the bim-open-toolkit TKT-311 spike (STEP B-rep design): 109 sites in that library, about 97 of them compile with the wrong meaning, for example `Law_TriangleMinAngleMeetsThreshold` never tests the angle.

## Done means

- [ ] A conditional or assignment used as a condition is parenthesized.
- [ ] A PlatoTests regression test compiles a nested conditional with Roslyn and checks its whole truth table.
