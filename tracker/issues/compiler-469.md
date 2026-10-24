---
id: compiler-469
title: Retire inactive parser and symbol XML writer sketches
type: debt
status: idea
priority: p3
effort: S
risk: low
area: compiler
sprint: 
created: 2026-10-23
closed:
links: []
---

`src/Plato.Parser/Parser.cs` and `src/Plato.Compiler/Symbols/SymbolWriterXml.cs` contain only commented-out implementation sketches. Their filenames suggest active parser and symbol-export services, which makes source navigation misleading. The real parser integration is elsewhere.

Decide whether the sketches retain historical value, then remove them or relocate the evidence to an archive. Inspect project and solution references before retiring the parser project. This is a small cleanup with no intended runtime behavior change, deferred from repo-468 to keep structural moves independently reviewable.

## Done means

- [ ] Confirm there are no runtime or build consumers of the inactive sketches.
- [ ] Remove or archive the sketches and update references without changing parser behavior.
