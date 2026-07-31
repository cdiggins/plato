---
id: plato-264
title: VS Code hover docs for Plato definitions
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-240, submodules/Plato/vscode-plato, submodules/Plato/Plato.Navigation]
---

## Idea

In VS Code/Cursor, hovering a Plato name should show documentation for that definition — ideally the declaration's doc comment plus a short signature, the way typeds languages surface JSDoc/XML docs. Builds on the navigation stack ([plato-240](plato-240.md)); hover is the next editor affordance after go-to-def / find-refs.

## Assumptions

- Plato source already carries (or will carry) useful doc comments on types/concepts/functions worth surfacing.
- `Plato.Navigation` can expose declaration text / doc-comment text at a symbol (or can be extended cheaply).
- vscode-plato is becoming a real language client (not TextMate-only) via plato-240.

## Design decisions

- **Doc source** — declaration doc comments only vs synthesized signature + comment vs pull from external HTML browser ([plato-265](plato-265.md)).
- **Transport** — extend Navigation `serve` with a `hover`/`documentation` query vs full LSP `textDocument/hover`.
- **Scope of hover target** — type/concept names only vs also values, members, and UFCS call sites.

## Related

- [plato-240](plato-240.md) — in-progress Definition/References; natural predecessor.
- [plato-265](plato-265.md) — HTML library browser; hover could deep-link there later.
- [plato-266](plato-266.md) — richer inline links in stdlib docs improve what hover shows.

## Approaches

Short term: Navigation query returns declaration range + leading doc comment; vscode `HoverProvider` renders markdown.
Long term: rich hover with concept parents, external links, lesson links, type diagrams.
Adjacent: signature help / parameter hints (separate issue if pursued).

## Case against

- **Nav first.** Shipping F12/refs ([plato-240](plato-240.md)) unfinished; hover on thin docs is empty UX.
- **Comment quality.** Many declarations lack comments; hover that only repeats the name teaches nothing.
- **LSP temptation.** Full language server is a large digression from the thin NDJSON client.

**Verdict: pursue** after plato-240 verification. Park until doc-comment coverage on the forward stdlib is worth hovering.

## Bedrock

Strengthens the **editor ↔ Navigation query seam** (`vscode-plato` + `Plato.Navigation.CLI serve`): one more query shape over the same index. **Verdict: simplest-along-the-grain** — HoverProvider + one serve method; must NOT invent a parallel doc index or full LSP in v1.

## Done means

- [ ] Hover on a documented type/concept in `.plato` shows doc comment + signature
- [ ] Undocumented symbols degrade gracefully (signature or nothing, no error toast)
- [ ] Works against configured `plato.navigation.roots`

## Simplest possible implementation

Extend Navigation serve with `documentation`/`hover` returning declaration text; register VS Code `HoverProvider`.

Pros: tiny; reuses index; immediate editor win  
Cons: only as good as comments; no rich formatting/links yet
