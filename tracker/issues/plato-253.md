---
id: plato-253
title: Diagnostics quality pass: file:line:col spans, source snippets, did-you-mean
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---

Proposed 2026-07-27 (agent idea, accepted by user for capture). Untriaged.

## Why

Language adoption lives or dies on error messages, and the type checker + TIR are new enough that
the messages have not had a deliberate pass. Two diagnostic systems exist today and only one of
them can point at source:

- **Linter** (`PlatoCompiler/Analysis/Linter.cs`) — codes LINT001-007, and findings carry a real
  location via `GetLocation(...)`.
- **Type checker** (`PlatoCompiler/Checking/CheckerModel.cs`) — `CheckDiagnostic` has Severity,
  Code (CHK*), Message, and an `Origin` Symbol. But its `ToString()` renders the origin as
  `(at FunctionCall #123)` — a symbol type and id, not a file, line, or column. The checker is
  total and *does* know which expression failed; the user just cannot see where it is.

## Sketch

1. **Locate.** Resolve `Origin` Symbol -> source span (the parser already tracks locations for the
   linter; find out why the checker path drops them and whether it is a plumbing gap or a
   desugaring gap where normalized nodes have no original span).
2. **Render.** One shared diagnostic formatter for both systems: `file:line:col: error CHK201: ...`,
   the offending source line, and a caret span underneath. Machine-readable form too, for
   `vscode-plato` and the navigation MCP.
3. **Explain.** For the common failures, say what was expected vs. found, and for overload
   no-match, list the candidates considered and why each was rejected (the solver already computes
   `ArgMatchKind` per argument — that reasoning is being discarded).
4. **Did you mean.** Edit-distance suggestions on unresolved names, using the existing name index
   in `Plato.Navigation/`.
5. **Document.** One page per diagnostic code, the way rustc has `--explain`.

## Open questions

- Do normalized/monomorphized TIR nodes retain a back-pointer to original source? If not, that is
  the real work, and it also blocks good runtime error attribution.
- Should the linter and checker unify on one diagnostic type, or share only the renderer?
- Snapshot-test the message text? Valuable, but it makes message edits noisy — decide deliberately.
