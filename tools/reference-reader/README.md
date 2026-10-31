# Plato reference reader

This folder is a standalone React/Vite reader for the Markdown pages assembled by `tools/reference/build.py`. It reads the generated v2 data in `public/` at runtime, in pieces (see Data below), and does not require a server API. Markdown HTML is not enabled; Mermaid diagrams use Mermaid's strict SVG rendering mode.

## Run it from the repository root

Use Node.js 20.19 or a release at least 22.12, npm, and Python 3.10 or later. The checked
dependency versions are pinned in `package-lock.json`; `npm ci` installs that exact dependency tree.

```powershell
python tools/reference/build.py
npm --prefix tools/reference-reader install
npm --prefix tools/reference-reader run dev
```

Open `http://127.0.0.1:5391`. Use the exact printed loopback address and dedicated port;
a generic localhost URL can reach a different listener on a shared development machine.
Build and serve the production bundle with:

```powershell
npm --prefix tools/reference-reader run build
npm --prefix tools/reference-reader run preview
```

The small pure navigation/search helpers can be checked with `npm --prefix tools/reference-reader test`.

The [Pages workflow](../../.github/workflows/pages.yml) publishes the same production build under
the site's `reference/` folder, passing Vite's `--base` flag so that the data file, the brand folder
and the bundle resolve there. The reader reads that base at runtime, so nothing in the source names
the folder.

The `predev` and `prebuild` scripts regenerate the reference data automatically, so Python 3.10
or later is also required. A failed source/link check stops the build instead of silently using
stale reader data. The initial explicit generator command is useful for seeing documentation
errors before dependency installation.

## Data

The reader fetches only what the current view needs; the shapes are in the contract's "Reader data v2".

- `reference-index.json` on start: page list, symbols and aliases, no Markdown.
- `pages/<id>.json` when a page opens (each path segment encoded); fetched pages are cached for the session. Markdown download uses that file's `rawMarkdown`.
- `search.json` on the first search.
- `examples.json` on the first visit to the playground.

Each fetch shows a loading state and, on failure, an error with a retry. The reader no longer reads `reference.json`.

## Structure and extension points

- `src/navigation.js` holds the pure logic: hash routes (pages and playground), the five doors, status meanings, heading slugs, the per-door folder trees, previous/next, and title, body and symbol search with ranking. `src/data.js` holds the caching JSON loader and the schema checks.
- `src/playground/` is the playground module; the reader renders its default export for `#/playground` routes.
- `src/main.jsx` owns route state, the expandable navigation, Markdown rendering, Mermaid rendering, and raw Markdown download.
- `src/style.css` lays out the navigation and the reading surface. Every colour, font, spacing step and radius in it is a custom property from the [brand tokens](../../docs/brand/tokens.json); the builder writes them to `public/brand/tokens.css` and copies the mark beside it, so the stylesheet never restates a brand value. Dark is the default theme; the light theme applies when the viewer prefers it and in print. Add future playgrounds as separate reader panels that consume the same page record shape.

Routes use `#/PAGE_ID`, `#/PAGE_ID@HEADING_SLUG`, `#/playground`, `#/playground/<group>` and `#/playground/<group>~<payload>` (the payload is passed through undecoded). The source revision and page path come from the generator's data. The reader currently offers search over titles and page bodies, page-level active navigation, a static heading outline, and browser-native history through the hash.

## Reader features

- Five doors (Tour, Reference, Cookbook, Design, Playground) in a header; the sidebar groups the page tree under them, and Previous/Next stay within a door.
- Each page shows its door, its status (Supported, Experimental, Legacy) with the contract's meaning, and its short source revision linked to the commit.
- Search lists titles at once, page text after `search.json` arrives, and a Symbols group matching names (exact, prefix, substring) and signature shapes.
- An "Open in playground" link follows each `plato` fence on a tour or cookbook page.
- Syntax coloring for `plato` and `csharp` fences, with Copy and a language label.
- Symbol links from the builder's API index for known type and function names in code and inline `` `code` ``.
- Clickable breadcrumbs, source path → GitHub, and Previous/Next on guide chapters.
- Search results include a short body snippet.

## Limits

Search is an in-memory term match: a title match outranks a body match and an exact title ranks first, but phrases are not ranked. Body search needs the whole of `search.json` in memory once used. Signature search is a case-insensitive, whitespace-normalised substring of a declaration's first line, so it cannot see a parameter that a declaration wraps onto another line. Symbol links prefer pages titled for the name and foundation over legacy; ambiguous prose identifiers stay plain text. The heading outline is derived from Markdown heading lines, so unusual heading syntax may not be reflected there even though Markdown renders it. Reader assets are bundled locally, including the three brand typefaces from their `@fontsource` packages; no remote fonts, scripts, or runtime data services are used. There is no theme switch in the page; the theme follows the viewer's system preference.
