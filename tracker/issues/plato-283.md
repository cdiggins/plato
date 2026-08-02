---
id: plato-283
title: Put color constants in their own folder
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [plato-272, submodules/Plato/stdlib/color.library.plato, submodules/Plato/stdlib/color.plato]
---

## Idea
The 141 CSS/X11 named colors currently live inside `library Colors` in `stdlib/color.library.plato`, co-located with `Color`'s `Numerical` bodies. Consider isolating the named-color table into its own folder (or at least its own file/package) so color arithmetic and the constant table can evolve independently, and so agents/readers looking for "named colors" don't wade through arithmetic.

## Assumptions
- The named-color surface is large enough (~141 entries) that discoverability and edit-churn matter.
- `Color8.AliceBlue`-style dispatch (`_: Color8` receivers) stays the idiom (see `constants.library.plato`).
- Any real subfolder requires tooling changes: `Plato.CLI` lint enumerates `*.plato` with `TopDirectoryOnly` (stdlib README); the old `interface-library/` subfolder was flattened for that reason.

## Design decisions
- **Folder vs file** — a `stdlib/colors/` (or `named-colors/`) subfolder vs a flat sibling `color.constants.library.plato`. Folder needs recursive lint; flat file does not.
- **What moves** — only the named-color table, or also `color-spaces` / imaging adjacency. Prefer constants only.
- **Library name** — keep `library Colors` split across files vs `library ColorConstants` separate from arithmetic `Colors`.

## Related
- [plato-272](plato-272.md) — Constants library feature; already notes color constants as a follow-up split from math constants.
- [plato-284](plato-284.md) — OpaqueColor8 / ColorWithAlpha8; would change the type the constants are declared on.
- [stdlib/color.library.plato](../../submodules/Plato/stdlib/color.library.plato) — current home of the table + Color Numerical bodies.
- [stdlib/README.md](../../submodules/Plato/stdlib/README.md) — documents TopDirectoryOnly / flatten history.

## Approaches
Short term: extract the named-color block into `color.constants.library.plato` (flat sibling); leave arithmetic in `color.library.plato`.
Long term: if CLI gains recursive folder lint, group color domain files under `stdlib/color/` (types, spaces, constants, imaging adjacency).
Adjacent: plato-284 (constant result type); plato-285 (broader types/libraries/interfaces layout).

## Bedrock
Strengthens the **stdlib file-ownership seam** for a large pure-data surface (named colors) separate from derived arithmetic. **Verdict: simplest-along-the-grain** — must NOT invent a subfolder until lint/codegen recurse; a flat `*.constants.library.plato` sibling keeps the stronger folder layout reachable.

## Done means
- [ ] Named-color table lives outside `color.library.plato`'s arithmetic section (own file or documented package)
- [ ] `lint stdlib` still 0 parse / 0 symbol errors
- [ ] README / file-map notes where named colors live
- [ ] Call-site idiom still reads as `Color8.<Name>` (or successor type from plato-284)

## Simplest possible implementation
Cut the named-color functions into `color.constants.library.plato` under `library ColorConstants` (or keep `Colors` if multi-file libraries are allowed); leave Arithmetic/`Numerical` in `color.library.plato`.
- Pros: zero tooling change; smaller review diffs; matches plato-272's "colors second" split.
- Cons: still a flat root; doesn't answer "folder" literally until CLI recurses.

## Case against
- **Already co-located on purpose.** `color.library.plato` documents two parts deliberately; splitting creates more files for agents to open for one domain.
- **Folder is blocked by tooling.** Pushing a subfolder without fixing `TopDirectoryOnly` silently drops the constants from lint — worse than today's flat file.
- **Churn without consumer pain.** Nobody has reported "can't find AliceBlue"; the table is one search away.
- Verdict: **pursue** the flat-file extract when touching colors next; **park** a real folder until CLI recursive lint lands (tie to plato-285).
