# Plato reference reader

This folder is a standalone React/Vite reader for the Markdown pages assembled by `tools/reference/build.py`. It reads the generated `public/reference.json` at runtime and does not require a server API. Markdown HTML is not enabled; Mermaid diagrams use Mermaid's strict SVG rendering mode.

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

The `predev` and `prebuild` scripts regenerate the reference data automatically, so Python 3.10
or later is also required. A failed source/link check stops the build instead of silently using
stale reader data. The initial explicit generator command is useful for seeing documentation
errors before dependency installation.

## Structure and extension points

- `src/navigation.js` parses hash routes, derives heading slugs, builds the hierarchical folder tree, and searches page titles and Markdown bodies.
- `src/main.jsx` owns route state, the expandable navigation, Markdown rendering, Mermaid rendering, and raw Markdown download.
- `src/style.css` lays out the navigation and the reading surface. Every colour, font, spacing step and radius in it is a custom property from the [brand tokens](../../docs/brand/tokens.json); the builder writes them to `public/brand/tokens.css` and copies the mark beside it, so the stylesheet never restates a brand value. Dark is the default theme; the light theme applies when the viewer prefers it and in print. Add future playgrounds as separate reader panels that consume the same page record shape.

Routes use `#/PAGE_ID` and `#/PAGE_ID@HEADING_SLUG`. The source revision and page path come from the generator's data. The reader currently offers search over titles and page bodies, page-level active navigation, a static heading outline, and browser-native history through the hash.

## Reader features

- Teaching-order navigation (Start → Language → Library → API → …) with human section labels.
- Syntax coloring for `plato` and `csharp` fences, with Copy and a language label.
- Symbol links from the builder's API index for known type and function names in code and inline `` `code` ``.
- Clickable breadcrumbs, source path → GitHub, and Previous/Next on guide chapters.
- Search results include a short body snippet.

## Limits

Search is a simple in-memory term match and does not rank exact phrases. Symbol links prefer pages titled for the name and foundation over legacy; ambiguous prose identifiers stay plain text. The heading outline is derived from Markdown heading lines, so unusual heading syntax may not be reflected there even though Markdown renders it. Reader assets are bundled locally, including the three brand typefaces from their `@fontsource` packages; no remote fonts, scripts, or runtime data services are used. There is no theme switch in the page; the theme follows the viewer's system preference.
