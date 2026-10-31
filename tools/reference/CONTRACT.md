# Reference integration contract — revision 3

The supervisor owns `tools/reference/`, tracker, shared indexes and all generated Markdown.
Language author owns `docs/reference/language/` and `docs/reference/implementation/` only.
Library author owns `docs/reference/library/` only (not the generated `api/` subtree).
Reader author owns `tools/reference-reader/` only, including package files and local dependencies.
Read and apply `C:/Users/cdigg/.codex/skills/platonic-coder/SKILL.md`.
Do not edit compiler, stdlib, other writers' files, tracker, or root scratch files.
Request a commit turn and wait for the supervisor's explicit grant before staging or committing.
No agent pushes; the supervisor integrates and pushes the completed wave.

## Content

Every page is Markdown with one H1 title. Keep topics small; use relative Markdown links.
Explain terms for junior scientific programmers. Include evidence links to repo source/tests.
Use `## Related work` where useful, comparing specific mappings and divergences with primary links.
Use fenced `mermaid` blocks for diagrams and fenced `text` blocks for inference rules.
Define all notation. Distinguish specified, implemented, observed, inferred and unresolved claims.
Existing ALL-CAPS docs remain normative. This descriptive reference does not silently change them.
Source-derived API pages retain complete source; teaching guides explain interpretation and limits.
Do not use non-C# writers as semantic evidence. Do not claim a clean-room spec is complete if gaps remain.

## Reader data

`tools/reference/build.py` writes `tools/reference-reader/public/reference.json` before reader build.
Top-level object: `{schemaVersion: 1, sourceRevision: string, pages: Page[]}`.
Page: `{id: string, path: string, title: string, group: string[], markdown: string}`.
`rawMarkdown` is an optional string preserving original relative links for downloads.
`id` is the repo-relative Markdown path without `.md`, e.g. `docs/reference/language/model`.
`path` retains `.md`. `group` is the hierarchical relative folder path below `docs/reference`.
Pages include all authored and generated `.md` files under `docs/reference`.
The generator rewrites relative Markdown links to pages as `#/PAGE_ID`, retaining fragment as
`@HEADING_SLUG`. Other repo-local source links become GitHub blob links pinned to `sourceRevision`.
Headings use lowercase slugs with non-letter/digit characters replaced by hyphens, trimmed.
The reader must render fenced `mermaid`, GFM tables and ordinary Markdown safely.
Navigation uses `#/PAGE_ID` and heading links `#/PAGE_ID@HEADING_SLUG`.
Raw Markdown remains downloadable from `rawMarkdown` (falling back to `markdown`) through a Blob URL.
No server API, execution sandbox, or network-dependent runtime assets are required.

## Verification and resources

Reader agent owns dependency installation, production build and preview server on IPv4 port 5391.
Supervisor owns the browser QA session and all repo-wide gates and generator outputs.
Agents save their checkpoints inside their assigned subtree. States: working, implemented,
verified, integrated. Report real checks and blockers; do not invent passing evidence.

## Reference site decisions

These settle Wave 0 of [`plans/reference-site-plan.md`](../../plans/reference-site-plan.md), toward
the [site guidance](../../docs/design/reference-site-guidance.md). Every later chunk reads them.

### Symbol URLs

- One page per declared name per source file: `docs/reference/api/<tier>/<stem>/<name>.md`,
  where `<stem>` is the source file stem and `<name>` is the declared name in lower case. The
  page id, and so the reader route, follows from that path.
- Every overload of a name in one file is listed on that name's one page. The same name in a
  different file is a different page; the symbol index lists both.
- The file page stays at `docs/reference/api/<tier>/<stem>.md` and links each name page.
- Nothing in the path depends on declaration order, so adding or reordering declarations never
  moves a page.
- The builder fails, rather than inventing a suffix, when two names in one file differ only by
  case, or when a lower-cased name is `readme`, `index`, or a Windows reserved device name.
- The numbered slice pages are retired. A frozen alias table,
  `docs/reference/api/aliases.json`, maps each retired page id to the name page of the first
  name in its slice. It is written once from the last numbered `catalog.json` and never
  regenerated. The reader resolves an unknown route through it before reporting a missing page.

### Status

Each API page and each library guide carries one status, set by its tier:

| Status | Tiers | Meaning |
| --- | --- | --- |
| Supported | `foundation`, `geometry`, `graphics` | In a shipping tier. The lint, type-check, and code-generation gates cover it. No release promises its signature yet. |
| Experimental | `future` | Aspirational vocabulary. It must parse and type-check; it is not linted or converted to C#. |
| Legacy | `legacy` | The older library, compiled separately. It is writable, but new vocabulary does not go there. |

The guidance's "stable" and "frozen" are not used: no language release exists to make a
signature stable, and the legacy library is not frozen. Status is separate from the evidence
labels in the reference README, which qualify individual claims, not pages.

### Example fixtures

- A fixture group is a folder under `docs/reference/implementation/examples/`. Groups are
  isolated: each compiles alone with its inputs.
- A group compiles with the `prelude` folder unless it holds an `inputs.txt`, which lists its
  compilation inputs, one repo-relative folder per line, such as `stdlib/foundation`. The
  `builders` group moves to that file.
- Every group declares at least one `Law_Reference*` function. A law states an example's
  expected result, so the result a page shows is the one the law runner checks.
- The tour is one group, `tour`. Each cookbook recipe is its own group, `cookbook-<recipe>`,
  so recipes cannot change each other's name resolution.
- Every `plato` fence on a tour or cookbook page is copied verbatim from a file in its group,
  and the page links that file. The builder rejects a fence it cannot find in the group.
- A runnable example that no group holds is labelled unchecked on its page.
