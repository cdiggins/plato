# Reference integration contract — revision 2

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
