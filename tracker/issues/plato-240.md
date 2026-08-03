---
id: plato-240
title: vscode-plato: compiler-backed Go to Definition + Find All References
type: feature
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-27
closed:
links: [submodules/Plato/vscode-plato, submodules/Plato/Plato.Navigation, docs/archive/plato-navigation-index-plan.md, plato-236, plato-237, plato-238]
---

## Issue

`vscode-plato` is TextMate-only. Plato.Navigation (plato-236/238) already provides compiler-backed go-to-def and find-refs; MCP (plato-237) is the first consumer. Wire the VS Code/Cursor extension to the same index so editors get solid navigation.

## Impact

Anyone editing `.plato` in VS Code/Cursor cannot jump to definitions or list references without leaving the editor (MCP/CLI only).

## Affected code
- `submodules/Plato/vscode-plato/` — declarative highlighter today
- `submodules/Plato/Plato.Navigation/` + `.CLI` — index + queries
- `labs/PlatoNavigationMcp/` — reference consumer pattern (`IncrementalIndexer` swap)

## Approach
1. Add a long-lived NDJSON `serve` mode on `Plato.Navigation.CLI` (IncrementalIndexer, definition/references/reload/update).
2. Activate `vscode-plato` as a TypeScript extension with DefinitionProvider + ReferenceProvider talking to that process.
3. Roots via `plato.navigation.roots` (default: discover `plato-src` / `plato-test-src` under the workspace).

## Done means
- [x] `Plato.Navigation.CLI serve` answers definition + references over stdio NDJSON
- [x] vscode-plato registers Go to Definition and Find All References for `plato`
- [ ] Against `plato-src`, F12 on a type/name use jumps to the declaration; Find All References lists body + type sites
- [x] README documents install, roots setting, and known limits (overload groups)
- [ ] Tracker closed after verification

## Notes
Serve smoke-tested 2026-07-27: `definition` at `primitives.plato` `type Number` → one Type location; `references` returns corpus-wide type/value sites. Editor F12 verification still outstanding (install extension from `vscode-plato` after `npm run compile`).

## Simplest fix
CLI serve + thin VS Code client — same index as MCP; no full LSP shell in v1.
