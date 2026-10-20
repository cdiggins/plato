---
id: plato-467
title: plato_laws does not run the Linter, so a missing interface function (LINT001) passes the warm server and fails the CLI lint
type: bug
status: ready
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-10-20
closed:
links: []
---

## Problem

Reported 2026-10-20 by the STEP mesher foundation track in bim-open-toolkit (F-58 in their
`sandbox/step-brep-mesh/reviews/FINDINGS.md`): a type that `implements` an interface without
supplying one of its functions passes `plato_laws` (type check scope `all` included) and is only
caught by `Plato.CLI lint`, which reports LINT001. The tracks had to run the CLI lint over the
whole library once at the end to find what the server had let through.

`WarmProgram.CheckTypes` runs `TypeFindings.Check` (the `TypeChecker`) and nothing else; the
`Linter` runs in `plato_check` (studio `CheckMcpTools`, gate `lint`) but not in the law tool,
and the law tool is the one the authoring loop calls. LINT001 is a Warning to the CLI's
`--strict`, while `stdlib/SEMANTICS.md` treats it as an error when authoring.

## Done means

- `plato_laws` with a type check scope runs the Linter over the checked files and returns
  LINT001 (and the other Error-severity findings) beside the type errors, counted in
  `typeErrorCount` or in a `lintErrorCount` the client reads for its verdict.
- A test in `tests/Plato.Navigation.Tests` runs a folder with a type missing an interface
  function through `WarmLawRunner` and sees the finding.
