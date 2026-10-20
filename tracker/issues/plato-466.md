---
id: plato-466
title: plato_laws: a parse or compile failure answers ok=false with errors that a client reading typeErrors and results never sees (0 laws, 0 type errors)
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-10-20
closed:
links: []
---

## Problem

Reported 2026-10-20 by the STEP mesher validation track in bim-open-toolkit (F-58 in their
`sandbox/step-brep-mesh/reviews/FINDINGS.md`): a `plato_laws` run over a folder holding a call
chained on a payload-free qualified sum case, `Expectation.ShouldPass.IsMet(v)`, answered with
0 laws and 0 type errors, the library's known type error included, so the run looked clean and
had compiled nothing.

The compiler is not the cause. The same shape, as a two-law probe (`probe.plato`: a two-case
sum, `IsMet` by `match`, one law calling `Expectation.ShouldPass.IsMet(Detail(0))` and one
calling `IsMet(Expectation.ShouldPass, Detail(0))`) compiles and passes both laws with
`Plato.CLI run --laws` at e37c032f and at the server's own 951a70ab, on the interpreter and
the C# backend.

What the tool does: `WarmProgram.Run` returns `Errors` (parse or compile) and no results, and
`LawMcpTools` answers `{ ok = false, errors = [...], program, timings }` with no `results`,
no `typeErrors` and no `typeErrorCount` (studio `LawMcpTools.cs:137`). A client that reads
`typeErrors` and `results`, as the toolkit's `plato_laws.py` did, prints "0 laws, 0 type
errors". TKT-337 in the toolkit added a client-side guard (an empty run is never a pass); the
server should not need one.

## Done means

- A run that compiled nothing says so in the fields a client reads for a verdict: `isError` set
  on the MCP result, `typeErrorCount` present (the errors counted), `results` empty with a
  reason, and the first error in `content` text.
- A test in `tests/Plato.Navigation.Tests` runs a folder with one unparseable file through
  `WarmLawRunner` and asserts `Compiled == false` and `Errors` non-empty; the studio side asserts
  the shape.
